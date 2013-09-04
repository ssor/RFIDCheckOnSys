using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using LogisTechBase.rfidCheck;
using System.IO.Ports;
using System.Text.RegularExpressions;
using AsynchronousSocket;
using rfidCheck;
using RfidReader;
using System.Diagnostics;

namespace LogisTechBase
{

    public partial class FrmRfidCheck_Server : Form
    {
        #region Members

        IDataTransfer dataTransfer = null;
        SerialPort comport = null;


        RFIDHelper _RFIDHelper = new RFIDHelper();

        //SerialPort comport = new SerialPort();
        //ISerialPortConfigItem ispc =  SerialPortConfigItem.GetConfigItem(SerialPortConfigItemName.超高频RFID串口设置);
        List<byte> maxbuf = new List<byte>();
        List<string> epcList = new List<string>();//记录读到的epc
        public static ManualResetEvent EventEPCList = new ManualResetEvent(true);
        public static ManualResetEvent EventControlInvoke = new ManualResetEvent(true);
        public static ManualResetEvent EventIdList_Temp = new ManualResetEvent(true);
        public static ManualResetEvent EventSerialPortCallback = new ManualResetEvent(true);

        List<Person> personList;
        List<CheckRecord> checkRecordList_temp = new List<CheckRecord>();//作为数据库的缓存，页面load时读入数据，页面退出时写入数据库中
        string receiveData;
        AsynchronousSocketListener listener = null;
        //public Dictionary<string, bool> dicFormUpdatedList = new Dictionary<string, bool>();
        InvokeDic _UpdateList = new InvokeDic();
        bool bRfidCheckClosed = true;//标识本地考勤服务是否已经关闭，如果为true，则表示已经关闭
        bool bRemoteCheckClosed = true;//标识远程考勤服务是否关闭，如果为true，则表示已经关闭
        #endregion

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

        public FrmRfidCheck_Server()
        {
            InitializeComponent();
            this.labelStatus.Text = "";
            this.btnReadRfid.EnabledChanged += new EventHandler(btnReadRfid_EnabledChanged);

            dataTransfer = new SerialPortDataTransfer();
            comport = new SerialPort(sysConfig.comportName, int.Parse(sysConfig.baudRate), Parity.None, 8, StopBits.One);
            ((SerialPortDataTransfer)dataTransfer).Comport = comport;

            //comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            //使得Helper类可以向串口中写入数据
            //_RFIDHelper.evtWriteToSerialPort += new deleVoid_Byte_Func(RFIDHelper_evtWriteToSerialPort);
            // 处理当前操作的状态
            //_RFIDHelper.evtCardState += new deleVoid_RFIDEventType_Object_Func(_RFIDHelper_evtCardState);
        }

        void btnReadRfid_EnabledChanged(object sender, EventArgs e)
        {
            //this.btnSerialPortConfig.Enabled = btnReadRfid.Enabled;
        }
        void UpdateStopRfidCheckButton(string value)
        {
            if (!_UpdateList.CheckItem("UpdateStopRfidCheckButton"))
            {
                return;
            }
            _UpdateList.SetItem("UpdateStopRfidCheckButton", false);

            bool bValue = false;
            if (value == "True")
            {
                bValue = true;
            }
            this.btnStopRfidCheck.Enabled = bValue;

            _UpdateList.SetItem("UpdateStopRfidCheckButton", true);
        }
        void UpdateStartRfidCheckButton(string value)
        {
            if (!_UpdateList.CheckItem("UpdateStartRfidCheckButton"))
            {
                return;
            }
            _UpdateList.SetItem("UpdateStartRfidCheckButton", false);

            bool bValue = false;
            if (value == "True")
            {
                bValue = true;
            }
            this.btnReadRfid.Enabled = bValue;

            _UpdateList.SetItem("UpdateStartRfidCheckButton", true);
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
        void _RFIDHelper_evtCardState(RFIDEventType eventType, object o)
        {
            string value = "";
            switch ((int)eventType)
            {
                case (int)RFIDEventType.RMU_Exception:
                    if (null != o)
                    {

                    }
                    MessageBox.Show("设备尚未准备就绪！");
                    break;
                case (int)RFIDEventType.RMU_CardIsReady:
                    _RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_InventoryAnti3, RFIDEventType.RMU_InventoryAnti);
                    break;
                case (int)RFIDEventType.RMU_InventoryAnti:
                    this.bRfidCheckClosed = false;
                    if (this.btnReadRfid.InvokeRequired)
                    {
                        this.btnReadRfid.Invoke(new deleUpdateContorl(this.UpdateStartRfidCheckButton), false.ToString());
                    }
                    else
                    {
                        this.btnReadRfid.Enabled = false;
                    }

                    if (this.btnStopRfidCheck.InvokeRequired)
                    {
                        this.btnStopRfidCheck.Invoke(new deleUpdateContorl(this.UpdateStopRfidCheckButton), true.ToString());
                    }
                    else
                    {
                        this.btnStopRfidCheck.Enabled = true;
                    }
                    if (null == o)
                    {
                        value = "正在检测标签...";
                        //this.btnReadRfid.Enabled = false;
                        //this.btnStopRfidCheck.Enabled = true;
                    }
                    else
                        if ((string)o != "ok")
                        {
                            value = "读取到标签：" + (string)o;
                            //string id = ((string)o).Substring(4, 6);
                            string epc = RFIDHelper.GetEPCFormUII((string)o);
                            AddNewIDToCheckRecordList_Temp(epc);
                            UpdateCheckListControl(null);
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
                case (int)RFIDEventType.RMU_StopGet:
                    _RFIDHelper.StopCallback();
                    this.bRfidCheckClosed = true;
                    value = "本地考勤服务停止";
                    //this.btnReadRfid.Enabled = true;
                    //this.btnStopRfidCheck.Enabled = false;
                    if (this.btnReadRfid.InvokeRequired)
                    {
                        this.btnReadRfid.Invoke(new deleUpdateContorl(this.UpdateStartRfidCheckButton), true.ToString());
                    }
                    else
                    {
                        this.btnReadRfid.Enabled = true;
                    }
                    if (this.btnStopRfidCheck.InvokeRequired)
                    {
                        this.btnStopRfidCheck.Invoke(new deleUpdateContorl(this.UpdateStopRfidCheckButton), false.ToString());
                    }
                    else
                    {
                        this.btnStopRfidCheck.Enabled = false;
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
                    //ConfigManager.SetSerialPort(ref comport, this.ispc);
                    comport.PortName = sysConfig.comportName;
                    comport.BaudRate = int.Parse(sysConfig.baudRate);
                    comport.DataBits = 8;
                    comport.StopBits = StopBits.One;
                    comport.Parity = Parity.None;
                    //ConfigManager.SetSerialPort(ref comport);
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
                //_RFIDHelper.Parse(buf,true);
                _RFIDHelper.Parse(buf);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void FrmRfidCheck_Server_Shown(object sender, System.EventArgs e)
        {
            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_StopGet, RFIDEventType.RMU_StopGet);
        }


        private void FrmRfidCheck_Server_Load(object sender, EventArgs e)
        {

            this.labelStatus.Text = "服务未运行";

            //this.checkRecordList_temp = rfidCheck_CheckOn.GetCheckRecords(DateTime.Today.ToShortDateString());
            this.personList = rfidCheck_CheckOn.GetPersonList();
            UpdateCheckListControl(null);

            this.FormClosing += new FormClosingEventHandler(FrmRfidCheck_Server_FormClosing);
        }

        void FrmRfidCheck_Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            //_RFIDHelper.StopCallback();
            //this.bClosing = true;
            if (this.bRemoteCheckClosed == false)
            {
                e.Cancel = true;
                MessageBox.Show("请先关闭远程考勤服务！");
                return;
            }
            if (this.bRfidCheckClosed == false)
            {
                e.Cancel = true;
                MessageBox.Show("请先关闭本地考勤服务！");
                return;
            }

            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_StopGet, RFIDEventType.RMU_StopGet);

            ExitProg();
            if (checkRecordList_temp.Count > 0)
            {
                for (int i = 0; i < checkRecordList_temp.Count; i++)
                {
                    rfidCheck_CheckOn.AddCheckRecord(checkRecordList_temp[i]);
                }
            }

        }
        void InvokeCheckListControl(object o)
        {
            if (!_UpdateList.CheckItem("InvokeCheckListControl"))
            {
                return;
            }
            _UpdateList.SetItem("InvokeCheckListControl", false);

            DataTable table = (DataTable)o;
            this.dataGridView1.DataSource = table;

            //this.dataGridView1.Columns[0].Width = 80;
            //this.dataGridView1.Columns[1].Width = 80;
            //this.dataGridView1.Columns[2].Width = 120;
            this.dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int headerW = this.dataGridView1.RowHeadersWidth;
            int columnsW = 0;
            DataGridViewColumnCollection columns = this.dataGridView1.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                columnsW += columns[i].Width;
            }
            if (columnsW + headerW < this.dataGridView1.Width)
            {
                int leftTotalWidht = this.dataGridView1.Width - columnsW - headerW;
                int eachColumnAddedWidth = leftTotalWidht / columns.Count;
                for (int i = 0; i < columns.Count; i++)
                {
                    columns[i].Width += eachColumnAddedWidth;
                }
            }
            //this.dicFormUpdatedList["InvokeCheckListControl"] = true;
            _UpdateList.SetItem("InvokeCheckListControl", true);

        }
        void UpdateCheckListControl(object o)
        {
            //EventControlInvoke.WaitOne();


            DataTable table = new DataTable("Records");
            DataColumn column;
            DataRow row;

            // Create new DataColumn, set DataType, 
            // ColumnName and add to DataTable.    
            column = new DataColumn("学号");
            table.Columns.Add(column);
            column = new DataColumn("姓名");
            table.Columns.Add(column);
            column = new DataColumn("考勤时间");
            table.Columns.Add(column);

            for (int i = 0; i < this.checkRecordList_temp.Count; i++)
            {
                for (int j = 0; j < this.personList.Count; j++)
                {
                    string recordID, personID;
                    recordID = this.checkRecordList_temp[i].id;
                    personID = this.personList[j].epc;
                    if (recordID == personID)
                    {
                        row = table.NewRow();
                        row["学号"] = this.personList[j].id_num;
                        row["姓名"] = this.personList[j].name;
                        row["考勤时间"] = this.checkRecordList_temp[i].checkDate;
                        table.Rows.Add(row);
                        break;
                    }
                }
            }
            if (this.dataGridView1.InvokeRequired)
            {
                this.dataGridView1.Invoke(new deleControlInvoke(InvokeCheckListControl), table);

            }
            else
            {
                InvokeCheckListControl(table);
                //this.dataGridView1.DataSource = table;
                //this.dataGridView1.Columns[0].Width = 80;
                //this.dataGridView1.Columns[1].Width = 80;
                //this.dataGridView1.Columns[2].Width = 120;
            }

        }
        string GetPersonNamebyID(string id)
        {
            if (null == id)
            {
                return null;
            }
            string strR = null;
            for (int i = 0; i < personList.Count; i++)
            {
                if (id == personList[i].epc)
                {
                    strR = personList[i].name;
                    break;
                }
            }
            return strR;
        }
        bool CheckIdList_TempExist(string newID)
        {
            EventIdList_Temp.WaitOne(1000, false);
            EventIdList_Temp.Reset();

            bool bExist = false;
            if (null == checkRecordList_temp)
            {
                return bExist;
            }
            for (int i = 0; i < checkRecordList_temp.Count; i++)
            {
                if (newID == checkRecordList_temp[i].id)
                {
                    bExist = true;
                    break;
                }
            }
            EventIdList_Temp.Set();
            if (bExist == true)
            {
                Debug.WriteLine(
                    string.Format("FrmRfidCheck_Server.CheckIdList_TempExist  ->  = {0}"
                    , "already exist"));
            }
            else
            {
                Debug.WriteLine(
                    string.Format("FrmRfidCheck_Server.CheckIdList_TempExist  ->  = {0}"
                    , "no yet exist"));
            }
            return bExist;
        }
        void AddNewIDToCheckRecordList_Temp(string newID)
        {
            EventIdList_Temp.WaitOne(1000, false);
            EventIdList_Temp.Reset();

            bool bAdd = true;
            for (int i = 0; i < checkRecordList_temp.Count; i++)
            {
                if (newID == checkRecordList_temp[i].id)
                {
                    bAdd = false;
                    break;
                }
            }
            if (bAdd == true)
            {
                Debug.WriteLine(
                    string.Format("FrmRfidCheck_Server.AddNewIDToCheckRecordList_Temp  ->  = {0}"
                    , newID + " should be added"));
            }
            if (bAdd)
            {
                string strDateTime = rfidCheck_CheckOn.GetFormatDateTimeString(DateTime.Now);
                checkRecordList_temp.Add(new CheckRecord(newID, strDateTime));
            }

            EventIdList_Temp.Set();
        }
        void frmRfidCheck_Server_Checkin(object o)
        {
            string id = (string)o;

            EventSerialPortCallback.Reset();
            {
                this.AddNewIDToCheckRecordList_Temp(id);
                EventControlInvoke.WaitOne();

                this.Invoke(new deleControlInvoke(this.UpdateCheckListControl), id);
            }
            EventSerialPortCallback.Set();
        }
        void frmRfidCheck_Local_Checkin(string id)
        {
            EventSerialPortCallback.Reset();
            if (AddEPC2List(id))
            {
                //rfidCheck_CheckOn.CheckOn(id);
                this.AddNewIDToCheckRecordList_Temp(id);
                EventControlInvoke.WaitOne();

                this.Invoke(new deleControlInvoke(this.UpdateCheckListControl));
            }
            //UpdateCheckListControl();
            EventSerialPortCallback.Set();
        }
        private void btn_startserver_Click(object sender, EventArgs e)
        {
            int nPort = sysConfig.tcp_port;
            //Regex r = new Regex(@"\d[1-9]\d{1,4}");

            //if (r.IsMatch(this.txtPort.Text))
            //{
            //    try
            //    {
            //        nPort = int.Parse(r.Match(this.txtPort.Text).ToString());
            //    }
            //    catch (System.Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //        return;
            //    }
            //}
            listener = new AsynchronousSocketListener();
            listener.eventProcessMsg += new deleAsynSocketProcessMsg(listener_eventProcessMsg);
            listener.eventGetSendContent += new deleAsynSocketListenerGetContent(listener_eventGetSendContent);
            listener.PortNum = nPort;
            listener.StartListening();
            this.bRemoteCheckClosed = false;
            this.btn_startserver.Enabled = false;
            this.labelStatus.Text = "远程考勤服务(" + this.GetLocalIP4() + ":" + nPort.ToString() + ")运行中...";
            //rfidCheck_CheckOn.CheckOn("20112104");
        }
        string listener_eventGetSendContent()
        {
            // 根据协议返回特定值，客户端由此确定成功与否
            //if (rfidCheck_CheckOn.CheckIDExist(receiveData))
            if (this.CheckIdList_TempExist(receiveData))
            {
                string name = GetPersonNamebyID(receiveData);
                if (name != null)
                {
                    string str = ((int)CheckProtocol.Success).ToString() + "&" + receiveData + "&" + name;
                    Debug.WriteLine(
                        string.Format("FrmRfidCheck_Server.listener_eventGetSendContent  ->  = {0}"
                        , str));
                    return str;
                }
                else
                {
                    string str = ((int)CheckProtocol.Failed).ToString();
                    Debug.WriteLine(
        string.Format("FrmRfidCheck_Server.listener_eventGetSendContent  ->  = {0}"
        , str));
                    return str;
                }
            }
            else
            {
                string str = ((int)CheckProtocol.Failed).ToString();
                Debug.WriteLine(
    string.Format("FrmRfidCheck_Server.listener_eventGetSendContent  ->  = {0}"
    , str));
                return str;
            }
        }

        void listener_eventProcessMsg(AsynSocketProcessMsg msg)
        {
            switch ((int)msg.nCode)
            {
                //case (int)enumAsynSocketPocessCode.SocketNormalOutPut:
                //    Console.WriteLine(msg.strMsg);
                //    break;
                case (int)enumAsynSocketPocessCode.SocketNormalEndReceive:
                    // 接收到客户端传来的学号信息，记录到数据库中
                    receiveData = msg.strMsg;
                    Debug.WriteLine(
                        string.Format("FrmRfidCheck_Server.listener_eventProcessMsg  ->  = {0}"
                        , receiveData));
                    //rfidCheck_CheckOn.CheckOn(receiveData);
                    this.frmRfidCheck_Server_Checkin(receiveData);
                    break;
            }
        }

        private void btn_stopserver_Click_1(object sender, EventArgs e)
        {
            if (null != this.listener)
            {
                listener.StopListener();
                listener = null;
            }
            this.bRemoteCheckClosed = true;
            this.btn_startserver.Enabled = true;

            this.labelStatus.Text = "服务于 " + DateTime.Now.ToString() + " 停止运行.";
            //this.lbCheckList.Items.Add("服务于 " + DateTime.Now.ToString() + " 停止运行.");
        }

        private void btnReadRfid_Click(object sender, EventArgs e)
        {
            //rfidOperateUnitInventoryTag u = new rfidOperateUnitInventoryTag(dataTransfer);
            //u.registeCallback(new deleRfidOperateCallback(frmRfidCheck_Server_Checkin));
            //u.OperateStart(false);
            //this.bRfidCheckClosed = false;
            //_RFIDHelper.StartCallback();
            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_GetStatus, RFIDEventType.RMU_CardIsReady);
            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_InventoryAnti3, RFIDEventType.RMU_InventoryAnti);
            //this.btnReadRfid.Enabled = false;
            //this.lblStatus.Text = "本地考勤服务启动";
        }
        void btnStopRfidCheck_Click(object sender, System.EventArgs e)
        {
            //rfidOperateUnitStopInventoryTag u = new rfidOperateUnitStopInventoryTag(dataTransfer);
            //u.OperateStart(true);
            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_StopGet, RFIDEventType.RMU_StopGet);
            //_RFIDHelper.StopCallback();
            //ExitProg();
            //this.btnReadRfid.Enabled = true;
            //this.btnStopRfidCheck.Enabled = false;
            //this.labelStatus.Text = "本地考勤服务停止";
        }
        void ExitProg()
        {
            bool bOk = false;
            if (null != comport)
            {
                if (comport.IsOpen)
                {
                    bOk = _UpdateList.ChekcAllItem();
                    // 如果没有全部完成，则要将消息处理让出，使Invoke有机会完成
                    while (!bOk)
                    {
                        Application.DoEvents();
                        bOk = _UpdateList.ChekcAllItem();
                    }
                    comport.Close();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSerialPortConfig_Click(object sender, EventArgs e)
        {
            //            SerialPortConfigItem spci =
            //SerialPortConfigItem.GetConfigItem(SerialPortConfigItemName.超高频RFID串口设置);
            //            SerialPortConfig spc = new SerialPortConfig(spci, null);
            //            //spc.ShowDialog();
            //            //SerialPortConfig spc = new SerialPortConfig();
            //            spc.ShowDialog();
        }
    }
}
