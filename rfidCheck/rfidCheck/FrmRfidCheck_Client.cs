using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using AsynchronousSocket;
using rfidCheck;
using RfidReader;
using System.Net.Sockets;
using nsConfigDB;


namespace LogisTechBase.rfidCheck
{
    public partial class FrmRfidCheck_Client : Form, IRFIDHelperSubscriber
    {
        Rmu900RFIDHelper rmu900Helper = null;
        IDataTransfer dataTransfer = null;
        SerialPort comport = null;
        #region Members
        //记录考勤成功的的epc，防止同一标签不断发送考勤信息
        List<string> epcList = new List<string>();
        //在窗口初始化完成后，会首先发送一个停止获取标签的stop命令，
        //在窗口关闭时，也会发送一个stop命令，此时bStartOrCloseStop为false
        bool bStartOrCloseStop = true;
        // 记录已经发送向服务端，但是未得到回复的EPC，防止在处理过程中重复发送
        List<string> ProcessingepcList = new List<string>();
        //SerialPortConfigItem spci =        SerialPortConfigItem.GetConfigItem(SerialPortConfigItemName.超高频RFID串口设置);
        public static ManualResetEvent EventEPCList = new ManualResetEvent(true);
        List<byte> maxbuf = new List<byte>();

        //public Dictionary<string, bool> dicFormUpdatedList = new Dictionary<string, bool>();
        InvokeDic _UpdateList = new InvokeDic();
        RFIDHelper _RFIDHelper = new RFIDHelper();

        bool bRfidCheckClosed = true;//标识本地考勤服务是否已经关闭，如果为true，则表示已经关闭

        bool bInitiallizing = false;
        //rfidOperateUnitBase operaterGetTag = null;
        //rfidOperateUnitWirteEPC operateWriteTag = null;
        //IDataTransfer dataTransfer = null;
        //SerialPort comport = null;

        int _Port;
        string _IP;
        System.Timers.Timer _timer = new System.Timers.Timer(1000);
        #endregion

        public FrmRfidCheck_Client()
        {
            InitializeComponent();

            dataTransfer = new SerialPortDataTransfer();
            //comport = StaticSerialPort.getStaticSerialPort();
            comport = new SerialPort(sysConfig.comportName, int.Parse(sysConfig.baudRate), Parity.None, 8, StopBits.One);
            ((SerialPortDataTransfer)dataTransfer).Comport = comport;
            rmu900Helper = new Rmu900RFIDHelper(dataTransfer);
            rmu900Helper.Subscribe(this);
            dataTransfer.AddParser(rmu900Helper);
            this.labelStatus.Text = "";
            //this.Shown += new EventHandler(FrmRfidCheck_Client_Shown);
            //comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            //使得Helper类可以向串口中写入数据
            //_RFIDHelper.evtWriteToSerialPort += new deleVoid_Byte_Func(RFIDHelper_evtWriteToSerialPort);
            // 处理当前操作的状态
            //_RFIDHelper.evtCardState += new deleVoid_RFIDEventType_Object_Func(_RFIDHelper_evtCardState);

            //_timer.Elapsed += new System.Timers.ElapsedEventHandler(_time_Elapsed);

            this.Shown += new EventHandler(FrmRfidCheck_Client_Shown);

            this.FormClosing+=new FormClosingEventHandler(FrmRfidCheck_Client_FormClosing);
        }

        void _time_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //string readCommand =
            //RFIDHelper.RmuReadDataCommandComposer(
            //        RMU_CommandType.RMU_SingleReadData
            //           , "12345678",
            //           0,
            //           2,
            //           2,
            //           null);
            //_RFIDHelper.SendCommand(readCommand, RFIDEventType.RMU_SingleReadData);

            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_GetStatus, RFIDEventType.RMU_CardIsReady);
        }
        string GetLocalIP4()
        {
            IPAddress ipAddress = null;
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
            {
                ipAddress = ipHostInfo.AddressList[i];
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    break;
                }
                else
                {
                    ipAddress = null;
                }
            }
            if (null == ipAddress)
            {
                return null;
            }
            return ipAddress.ToString();
        }

        void FrmRfidCheck_Client_Shown(object sender, EventArgs e)
        {
            this.txtIP.Text = this.GetLocalIP4();
            this.txtPort.Text = "13000";
            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_StopGet, RFIDEventType.RMU_StopGet);
        }
        void _RFIDHelper_evtCardState(RFIDEventType eventType, object o)
        {
            string value = "";
            string secret = null;
            string readCommand = null;
            switch ((int)eventType)
            {
                case (int)RFIDEventType.RMU_Exception:
                    if (null != o)
                    {

                    }
                    //MessageBox.Show("设备尚未准备就绪！");
                    value = "设备出现异常！";
                    if (this.labelStatus.InvokeRequired)
                    {
                        this.labelStatus.Invoke(new deleUpdateContorl(UpdateStatusLable), value);
                    }
                    else
                    {
                        UpdateStatusLable(value);
                    }
                    break;
                case (int)RFIDEventType.RMU_CardIsReady:
                    _RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_InventoryAnti3, RFIDEventType.RMU_InventoryAnti);
                    //value = "设备状态正常！";
                    //if (this.labelStatus.InvokeRequired)
                    //{
                    //    this.labelStatus.Invoke(new deleUpdateContorl(UpdateStatusLable), value);
                    //}
                    //else
                    //{
                    //    //this.statusLabel.Text = value;
                    //    UpdateStatusLable(value);
                    //}
                    //secret = ConfigManager.GetLockMemSecret();
                    //if (secret == null)
                    //{
                    //    secret = "12345678";
                    //}

                    //readCommand =
                    //         RFIDHelper.RmuReadDataCommandComposer(
                    //             RMU_CommandType.RMU_SingleReadData
                    //                , secret,
                    //                0,
                    //                2,
                    //                2,
                    //                null);
                    //_RFIDHelper.SendCommand(readCommand, RFIDEventType.RMU_SingleReadData);
                    break;
                case (int)RFIDEventType.RMU_InventoryAnti:
                    this.bRfidCheckClosed = false;
                    if (this.button1.InvokeRequired)
                    {
                        this.button1.Invoke(new deleUpdateContorl(this.UpdateButton1), "关闭");
                    }
                    else
                    {
                        this.button1.Text = "关闭";
                    }
                    if (null == o)
                    {
                        value = "正在检测标签...";
                    }
                    else
                        if ((string)o != "ok")
                        {
                            string id = RFIDHelper.GetEPCFormUII((string)o);
                            //string id = RFIDHelper.GetIDFromEPC((string)o); 
                            value = "读取到学号：" + id;
                            // value = "读取到标签：" + (string)o;
                            // secret = ConfigManager.GetLockMemSecret();
                            // if (secret == null)
                            // {
                            //     secret = "12345678";
                            // }
                            // //读取密码段
                            //readCommand = 
                            //     RFIDHelper.RmuReadDataCommandComposer(
                            //                         RMU_CommandType.RMU_SingleReadData
                            //                            , secret,
                            //                            0,
                            //                            2,
                            //                            2,
                            //                            null);
                            // _RFIDHelper.SendCommand(readCommand, RFIDEventType.RMU_SingleReadData);

                            CheckToRemoteServer(id);

                        }
                    if (this.labelStatus.InvokeRequired)
                    {
                        this.labelStatus.Invoke(new deleUpdateContorl(UpdateStatusLable), value);
                    }
                    else
                    {
                        //this.statusLabel.Text = value;
                        UpdateStatusLable(value);
                    }
                    break;
                case (int)RFIDEventType.RMU_SingleReadData:
                    if (null != o)
                    {
                        string data = (string)o;
                        int n = data.IndexOf("&");//data + & + uii
                        string uii = data.Substring(n + 1);
                        string epc = RFIDHelper.GetEPCFormUII(uii);
                        value = "读取到标签：" + uii;
                        if (this.labelStatus.InvokeRequired)
                        {
                            this.labelStatus.Invoke(new deleUpdateContorl(UpdateStatusLable), value);
                        }
                        else
                        {
                            UpdateStatusLable(value);
                        }
                        CheckToRemoteServer(epc);
                    }
                    break;
                case (int)RFIDEventType.RMU_StopGet:
                    string buttonText = "";
                    if (bStartOrCloseStop == true)//如果只是开始时的初始化命令
                    {
                        this.bRfidCheckClosed = false;
                        value = "本地考勤服务开始";
                        buttonText = "关闭";
                        this.StartReadRFIDTag();
                    }
                    else
                    {
                        _RFIDHelper.StopCallback();
                        this.bRfidCheckClosed = true;
                        value = "本地考勤服务停止";
                        buttonText = "打开";
                    }

                    if (this.button1.InvokeRequired)
                    {
                        this.button1.Invoke(new deleUpdateContorl(this.UpdateButton1), buttonText);
                    }
                    else
                    {
                        this.button1.Text = buttonText;
                    }

                    if (this.labelStatus.InvokeRequired)
                    {
                        this.labelStatus.Invoke(new deleUpdateContorl(UpdateStatusLable), value);
                    }
                    else
                    {
                        //this.statusLabel.Text = value;
                        UpdateStatusLable(value);
                    }
                    break;
            }
        }

        private void StartReadRFIDTag()
        {
            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_GetStatus, RFIDEventType.RMU_CardIsReady);
            //            _timer.Start();
        }
        private void StopReadRFIDTag()
        {
            //          _timer.Stop();
        }
        void UpdateButton1(string value)
        {
            if (!_UpdateList.CheckItem("UpdateButton1"))
            {
                return;
            }
            _UpdateList.SetItem("UpdateButton1", false);

            this.button1.Text = value;

            _UpdateList.SetItem("UpdateButton1", true);
        }
        void UpdateStatusLable(string value)
        {
            if (!_UpdateList.CheckItem("UpdateStatusLable"))
            {
                return;
            }
            _UpdateList.SetItem("UpdateStatusLable", false);

            this.labelStatus.Text = value;

            _UpdateList.SetItem("UpdateStatusLable", true);
        }
        void RFIDHelper_evtWriteToSerialPort(byte[] value)
        {
            if (comport == null)
            {
                return;
            }
            try
            {
                if (!comport.IsOpen)
                {
                    //ConfigManager.SetSerialPort(ref comport,this.spci);
                    comport.PortName = sysConfig.comportName;
                    comport.BaudRate = int.Parse(sysConfig.baudRate);
                    comport.DataBits = 8;
                    comport.StopBits = StopBits.One;
                    comport.Parity = Parity.None;
                    comport.Open();

                }
                comport.Write(value, 0, value.Length);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int n = comport.BytesToRead;//n为返回的字节数
                byte[] buf = new byte[n];//初始化buf 长度为n
                comport.Read(buf, 0, n);//读取返回数据并赋值到数组
                _RFIDHelper.Parse(buf);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        // 将读到的EPC添加到列表中
        bool AddEPC2List(string strEpc)
        {

            EventEPCList.WaitOne(1000, false);
            EventEPCList.Reset();// 防止多线程干扰
            bool bR = false;

            if (epcList.Count > 0)
            {
                if (epcList[0] != strEpc)
                {
                    epcList.Clear();
                }
                epcList.Add(strEpc);
            }
            else
            {
                epcList.Add(strEpc);
            }
            if (epcList.Count > 5)
            {
                epcList.Clear();
                bR = true;
            }
            EventEPCList.Set();
            return bR;
        }

        private void FrmRfidCheck_Client_Load(object sender, EventArgs e)
        {
            this.FormClosing += new FormClosingEventHandler(FrmRfidCheck_Client_FormClosing);
            //this.txtIP.Text = "";
            //this.txtPort.Text = "13000";
        }

        void FrmRfidCheck_Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.bRfidCheckClosed == false)
            {
                e.Cancel = true;
                MessageBox.Show("请先关闭本地考勤服务！");
                return;
            }
            this.comportClear();
        }
        void comportClear()
        {
            bool bOK = false;
            bOK = _UpdateList.ChekcAllItem();
            while (!bOK)
            {
                Application.DoEvents();
                bOK = _UpdateList.ChekcAllItem();
            }
            if (null != comport)
            {
                if (comport.IsOpen)
                {
                    comport.Close();
                }
            }
        }
        bool bEpcDisposed = false;//标识是否已经处理过标签，防止多次重复处理

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int port = 13000;
                if (this.txtPort.Text != null && this.txtPort.Text != "")
                {
                    port = int.Parse(this.txtPort.Text);
                }
                string ip = "127.0.0.1";
                Regex r = new Regex(@"((2[0-4]\d|25[0-5]|[01]?[0-9]?\d)\.){3}(2[0-4]\d|25[0-5]|[01]?[0-9]?\d)");
                if (r.IsMatch(this.txtIP.Text))
                {
                    ip = r.Match(this.txtIP.Text).ToString();
                }
                else
                {
                    MessageBox.Show("请输入正确的ip地址！");
                    return;
                }
                _Port = port;
                _IP = ip;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            bEpcDisposed = false;
            //if (this.button1.Text == "关闭")
            //{
            //    //_RFIDHelper.StopCallback();
            //    this.StopReadRFIDTag();
            //    bStartOrCloseStop = false;
            //    _RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_StopGet, RFIDEventType.RMU_StopGet);
            //    this.btnSerialPortConfig.Enabled = true;
            //    comportClear();
            //    return;
            //}
            
            if (this.button1.Text.IndexOf("打开") >= 0)
            {
                _RFIDHelper.StartCallback();
                bStartOrCloseStop = true;
                //// 首先发送停止获得标签的指令，防止正在不断返回标签导致读取失败
                _RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_StopGet, RFIDEventType.RMU_StopGet);
                ////_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_GetStatus, RFIDEventType.RMU_CardIsReady);
                this.btnSerialPortConfig.Enabled = false;
                rmu900Helper.StartInventoryOnce();
                //operaterGetTag = new rfidOperateUnitGetTagEPC(dataTransfer);
                //operaterGetTag.registeCallback(new deleRfidOperateCallback(CheckToRemoteServer));
                //operaterGetTag.OperateStart(true);
            }
        }
        public void NewMessageArrived()
        {
           
            string r1 = rmu900Helper.ChekcInventoryOnce();
            if (r1 != string.Empty)
            {
               // Debug.WriteLine("读取到标签 " + r1);
                AudioAlert.PlayAlert();
                this.CheckToRemoteServer(r1);
               // this.UpdateEpcList(r1);
            }
            //string r = rmu900Helper.CheckRmuStatus();
            //if (r == "ok")
            //{
            //    MessageBox.Show("设备状态良好！");
            //}
        }
        public delegate void deleControlInvoke(object o);
        void CheckToRemoteServer(object o)
        {
            if (o == null)
            {
                return;
            }
            //string om = (string)o;
           // operateMessage om = (operateMessage)o;
            //if (om != "")
            //{
                deleControlInvoke dele = delegate(object oepc)
                {
                    string id = (string)oepc;
                    //if (AddEPC2List(id))
                    if (!epcList.Contains(id) && !ProcessingepcList.Contains(id))
                    {
                        AsynchronousSocketClient asc = new AsynchronousSocketClient();
                        asc.eventProcessMsg += new deleAsynSocketProcessMsg(asc_eventProcessMsg);
                        try
                        {
                            ProcessingepcList.Add(id);
                            asc.StartClient(id, _IP, _Port);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }
                    }
                };
                this.Invoke(dele, o);
            //}
        }

        void asc_eventProcessMsg(AsynSocketProcessMsg msg)
        {
            switch (msg.nCode)
            {
                case (int)enumAsynSocketPocessCode.SocketNormalEndReceive:
                    if (((int)CheckProtocol.Success).ToString() == msg.strMsg.Substring(0, 1))
                    {
                        string[] tempA = msg.strMsg.Split('&');
                        epcList.Add(tempA[1]);//考勤成功id加入到列表中，
                        ProcessingepcList.Remove(tempA[1]);
                        if (tempA.Length > 2)
                        {
                            MessageBox.Show(tempA[2] + "同学，您的已经考勤完成!");
                        }
                    }
                    else
                        if (((int)CheckProtocol.Failed).ToString() == msg.strMsg)
                        {
                            ProcessingepcList.Clear();
                            MessageBox.Show("考勤失败，你可能使用了错误的学生卡!");
                        }
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSerialPortConfig_Click(object sender, EventArgs e)
        {
            //SerialPortConfig spc = new SerialPortConfig(spci, null);
            ////SerialPortConfig spc = new SerialPortConfig();
            //spc.ShowDialog();
        }
    }
}
