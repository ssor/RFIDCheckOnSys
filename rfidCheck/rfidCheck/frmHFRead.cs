using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace LogisTechBase
{
    public partial class frmHFRead : Form
    {
        private System.IO.Ports.SerialPort comport = new System.IO.Ports.SerialPort();//定义串口
        private bool bClosing = false;
        bool isPortOpen = false;
        bool isReading = false;
        InvokeDic _InvokeList = new InvokeDic();
        Timer _timer = new Timer();
        DataTable dataTable = null;

        public frmHFRead()
        {
            InitializeComponent();


            this.comport.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(comport_DataReceived);
            this._timer.Interval = 1000;
            this._timer.Tick += new EventHandler(_timer_Tick);


            dataTable = new DataTable();
            dataTable.Columns.Add("标签UID", typeof(string));
            dataTable.Columns.Add("协议类型", typeof(string));
            dataTable.Columns.Add("读取次数", typeof(int));
            dataTable.Columns.Add("读取时间", typeof(string));

            this.Shown += new EventHandler(frmHFRead_Shown);

        }

        void frmHFRead_Shown(object sender, EventArgs e)
        {
            this.refreshTable(string.Empty, string.Empty);
        }
        int currentProto = 1;
        void _timer_Tick(object sender, EventArgs e)
        {

            if (this.isReading == true)
            {
                string str2Write = string.Empty;
                switch (currentProto)
                {
                    case 1://tagit
                        if (this.ckbTagit.Checked)
                        {
                            str2Write = HFCommandItem.读取TAGIT协议标签;
                            Debug.WriteLine(
                                string.Format("frmHFRead._timer_Tick  ->  = {0}"
                                , "读取TAGIT协议标签"));
                        }
                        break;
                    case 2://
                        if (this.ckb15693.Checked)
                        {
                            str2Write = HFCommandItem.读取15693协议标签;
                            Debug.WriteLine(
    string.Format("frmHFRead._timer_Tick  ->  = {0}"
    , "读取15693协议标签"));
                        }
                        break;
                    case 3://
                        if (this.ckb14443a.Checked)
                        {
                            str2Write = HFCommandItem.读取14443A协议标签;
                            Debug.WriteLine(
string.Format("frmHFRead._timer_Tick  ->  = {0}"
, "读取14443A协议标签"));
                        }
                        break;
                    case 4://
                        if (this.ckb14443b.Checked)
                        {
                            str2Write = HFCommandItem.读取14443B协议标签;
                            Debug.WriteLine(
string.Format("frmHFRead._timer_Tick  ->  = {0}"
, "读取14443B协议标签"));
                        }
                        break;
                }
                if (currentProto == 4)
                {
                    currentProto = 1;
                }
                else
                {
                    currentProto++;
                }
                try
                {
                    //转换列表为数组后发送
                    //comport.Write(bytesCommandToWrite, 0, bytesCommandToWrite.Length);
                    this.comport.Write(str2Write);
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine(
                        string.Format("frmHFRead._timer_Tick  ->  = {0}"
                        , ex.Message));
                }
            }
        }
        StringBuilder buffer = new StringBuilder();
        void comport_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (this.bClosing == true)
            {
                return;
            }
            try
            {
                string temp = comport.ReadExisting();
                buffer.Append(temp);

                //解析返回的数据
                // 首先确定已经接收到的数据中含有指示命令和标签UID
                string currentData = buffer.ToString();

                Debug.WriteLine(
                    string.Format("frmHFRead.comport_DataReceived  -> current buffer = {0}"
                    , currentData));

                int iTagit = -1;
                int i1443a = -1;
                int i1443b = -1;
                int i15693 = -1;
                int iPro = -1;
                string strPro = string.Empty;
                iTagit = currentData.IndexOf(HFCommandItem.读取TAGIT协议标签);
                if (iTagit >= 0)
                {
                    iPro = iTagit;
                    strPro = "TAGIT协议";
                    goto Found;
                }
                i1443a = currentData.IndexOf(HFCommandItem.读取14443A协议标签);
                if (i1443a >= 0)
                {
                    iPro = i1443a;
                    strPro = "14443A协议";

                    goto Found;
                }
                i1443b = currentData.IndexOf(HFCommandItem.读取14443B协议标签);
                if (i1443b >= 0)
                {
                    iPro = i1443b;
                    strPro = "14443B协议";
                    goto Found;
                }
                i15693 = currentData.IndexOf(HFCommandItem.读取15693协议标签);
                if (i15693 >= 0)
                {
                    iPro = i15693;
                    strPro = "15693协议";
                    goto Found;
                }

            Found: if (iPro > -1)
                {
                    int iright = -1;
                    iright = currentData.IndexOf("]", iPro);//先检测右边括号，没有右边的话说明数据不完整
                    if ((iright > -1) && iright > iPro)// ] 必须在协议的后面，否则就说明这不是同一个数据段
                    {
                        int ileft = -1;
                        ileft = currentData.IndexOf("[", iPro);
                        if (ileft != -1 && ileft < iright)
                        {
                            string tagID = string.Empty;
                            tagID = currentData.Substring(ileft + 1, iright - ileft - 1);
                            if (tagID.Length > 0)
                            {
                                int dindex = tagID.IndexOf(",");
                                if (dindex >= 0)
                                {
                                    tagID = tagID.Substring(0, dindex);
                                    if (tagID != null && tagID.Length > 0)
                                    {
                                        Debug.WriteLine(
                                                        string.Format("frmHFRead.comport_DataReceived  -> tagID = {0}"
                                                        , tagID));
                                        this.Invoke(new deleControlInvoke(receiveNewTagInfo), new HFTagInfo(strPro, tagID));
                                    }
                                }

                            }

                            buffer.Remove(0, iright + 1);
                        }
                    }
                }
                //if (buffer.IndexOf("\r\n") != -1)
                //{
                //this.Invoke(new deleUpdateContorl(updateText), buffer);
                //buffer = string.Empty;
                //}

            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(
                    string.Format("frmHFRead.comport_DataReceived  -> exception = {0}"
                    , ex.Message));
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (this.isPortOpen == true)
            {
                MessageBox.Show("请先关闭串口！", "提示");
                return;
            }
            this.Close();
        }

        void receiveNewTagInfo(object o)
        {
            HFTagInfo info = (HFTagInfo)o;
            this.refreshTable(info.标签ID, info.协议类型);
        }
        void refreshTable(string id, string proto)
        {
            if (!_InvokeList.CheckItem("refreshTable"))
            {
                return;
            }
            _InvokeList.SetItem("refreshTable", false);

            if (id != string.Empty)
            {
                DataRow[] rows = dataTable.Select("标签UID = '" + id + "'");
                if (rows.Length > 0)
                {
                    DataRow dr = rows[0];
                    int count = (int)dr["读取次数"];
                    count++;
                    //dr["读取次数"] = count++;
                    //dr["读取时间"] = DateTime.Now.ToString("");
                    DataRow drNew = this.dataTable.NewRow();
                    drNew["标签UID"] = dr["标签UID"];
                    drNew["协议类型"] = dr["协议类型"];
                    drNew["读取次数"] = count;
                    drNew["读取时间"] = DateTime.Now.ToString("HH:mm:ss");
                    dataTable.Rows.Remove(dr);//将新更改的行置顶
                    dataTable.Rows.InsertAt(drNew, 0);
                }
                else
                {

                    DataRow dr = this.dataTable.NewRow();
                    dr["标签UID"] = id;
                    dr["协议类型"] = proto;
                    dr["读取次数"] = 1;
                    dr["读取时间"] = DateTime.Now.ToString("HH:mm:ss");
                    this.dataTable.Rows.InsertAt(dr, 0);
                    //this.dataTable.Rows.Add(new object[] { str, DateTime.Now.ToString("") });
                }

            }
            this.dataGridView1.DataSource = dataTable;
            DataGridViewColumnCollection columns = this.dataGridView1.Columns;
            columns[0].Width = 240;
            columns[1].Width = 150;
            columns[2].Width = 100;
            columns[3].Width = 120;
            _InvokeList.SetItem("refreshTable", true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.isReading == false)//开始读取条码
            {
                if (this.isPortOpen == false)
                {
                    if (this.open_serialport() == true)
                    {
                        this.beginToRead();
                        this.ProgressControl1.Start();
                    }
                }
                else
                {
                    this.beginToRead();
                    this.ProgressControl1.Start();
                }
            }
            else//停止读取条码
            {
                this.isReading = false;
                this.button1.Text = "开始";
                this._timer.Enabled = false;
                this.ProgressControl1.Stop();
            }
        }
        void beginToRead()
        {
            this.isReading = true;
            this.button1.Text = "停止";
            // 循环发送命令
            this._timer.Enabled = true;

        }
        bool open_serialport()
        {
            bool bR = true;
            try
            {
                if (this.isPortOpen == false)
                {
                    bClosing = false;
                    // 设置串口参数
                    if (!comport.IsOpen)
                    {

                        if (ConfigManager.SetSerialPort(ref comport, ispci))
                        {

                            comport.Open();//尝试打开串口
                            this.btn_opencom.Text = "关闭串口";
                            this.btnSerialPortConfig.Enabled = false;
                            this.isPortOpen = true;
                            //bClosing = false;
                        }
                    }
                }
            }
            catch (Exception ex)//进行异常捕获
            {
                MessageBox.Show(ex.Message);
                bR = false;
            }
            return bR;
        }
        bool close_serialport()
        {
            bool bR = true;
            try
            {
                if (this.isPortOpen == true)
                {

                    bClosing = true;
                    bool bOk = false;
                    if (comport.IsOpen)
                    {
                        bOk = _InvokeList.ChekcAllItem();
                        // 如果没有全部完成，则要将消息处理让出，使Invoke有机会完成
                        while (!bOk)
                        {
                            Application.DoEvents();
                            bOk = _InvokeList.ChekcAllItem();
                        }
                        //打开时点击，则关闭串口
                        comport.Close();
                        this.btnSerialPortConfig.Enabled = true;
                        btn_opencom.Text = "打开串口";
                        this.isPortOpen = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                bR = false;
            }
            return bR;
        }

        private void btn_opencom_Click(object sender, EventArgs e)
        {
            if (this.isPortOpen == true)
            {
                this.close_serialport();
                this._timer.Enabled = false;
            }
            else
            {
                this.open_serialport();
            }
        }

        private void btnSerialPortConfig_Click(object sender, EventArgs e)
        {
            SerialPortConfig spc = new SerialPortConfig(this.ispci, "高频RFID模块串口设置");
            spc.ShowDialog();
        }
    }

    public class HFTagInfo
    {
        public string 协议类型 = string.Empty;
        public string 标签ID = string.Empty;
        public HFTagInfo(string pro, string tag)
        {
            this.标签ID = tag;
            this.协议类型 = pro;
        }
    }
}
