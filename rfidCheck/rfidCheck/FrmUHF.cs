using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;

namespace LogisTechBase
{
    public partial class FrmUHF : Form
    {
        private SerialPort comport = new SerialPort();//定义串口
        List<byte> maxbuf = new List<byte>();
        //   List<byte> maxbuf1 = new List<byte>();
        string strinfo;
        bool distflag;//连接标志
        string Cmd;//发送命令
        string Lock;

        bool BtnFlag;//按钮标志



        StringBuilder builder = new StringBuilder();
        int FREMODE;//频率工作模式
        int FREBASE;//频率基数
        int BF;//起始频率
        int CN;// 频道数
        int SPC;//频道带宽
        int FREHOP;//调频顺序方式



        //避免在事件处理方法中反复的创建，定义到外面。
        private long received_count = 0;
        //接收计数
        private long send_count = 0;
        //发送计数
        private bool Listening = false;
        //是否没有执行完invoke相关操作
        private bool PortClosing = false;
        //是否正在关闭串口，执行Application.DoEvents， 并阻止再次invoke


        private void BtnStatus()
        {

        }






        private void DecodeUIIData(string id)//识别标签数据
        {
            int count = ctrList.Items.Count;
            bool findflag = false;
            foreach (ListViewItem lVI in ctrList.Items)
            {
                string s1 = lVI.SubItems[0].Text;
                string s2 = lVI.SubItems[1].Text;

                if (s1 == "识别" && s2 == id)
                {
                    int tempNum = int.Parse(lVI.SubItems[2].Text);
                    tempNum++;
                    lVI.SubItems[2].Text = tempNum.ToString();
                    findflag = true;
                    break;
                }
                //lVI.SubItems[2].Text = c.ToString();
            }
            if (!findflag)
            {
                ListViewItem li = new ListViewItem();
                li.SubItems.Clear();
                li.SubItems[0].Text = "识别";
                li.SubItems.Add(id);
                li.SubItems.Add("1");
                ctrList.Items.Add(li);
            }

        }
        private void DecodeReadUIIData(string id)//不指定UII读反馈的数据
        {
            int count = ctrList.Items.Count;
            bool findflag = false;
            string UII;

            int Len = 2 * Byte.Parse(id.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);// int.Parse();//包长度
            UII = id.Substring(Len - 26, 28);
            foreach (ListViewItem lVI in ctrList.Items)
            {
                string s1 = lVI.SubItems[0].Text;//状态
                if (s1 != "读取")
                    continue;
                string s2 = lVI.SubItems[1].Text;//ID
                string s3 = lVI.SubItems[3].Text;//数据块
                string s4 = lVI.SubItems[4].Text;//地址
                string s5 = lVI.SubItems[5].Text;//长度
                string s6 = lVI.SubItems[6].Text;//数据


                if (s1 == "读取" && s2 == UII && s3 == cbx_bank.SelectedIndex.ToString("X2") && s4 == txt_address.Text && txt_datalen.Text == s5 && s6 == id.Substring(8, id.Length - 38))
                {
                    int tempNum = int.Parse(lVI.SubItems[2].Text);
                    tempNum++;
                    lVI.SubItems[2].Text = tempNum.ToString();
                    findflag = true;
                    break;
                }
                //lVI.SubItems[2].Text = c.ToString();
            }
            if (!findflag)
            {
                ListViewItem li = new ListViewItem();
                li.SubItems.Clear();
                li.SubItems[0].Text = "读取";

                li.SubItems.Add(UII);
                li.SubItems.Add("1");
                li.SubItems.Add(cbx_bank.SelectedIndex.ToString("X2"));
                li.SubItems.Add(txt_address.Text);
                li.SubItems.Add(txt_datalen.Text);
                li.SubItems.Add(id.Substring(8, id.Length - 38));
                ctrList.Items.Add(li);
            }

        }
        private void DecodeFixUIIData(string id, string str)//指定UII读取反馈的数据
        {
            int count = ctrList.Items.Count;
            bool findflag = false;
            string UII;
            UII = txt_UII.Text;
            foreach (ListViewItem lVI in ctrList.Items)
            {
                string s1 = lVI.SubItems[0].Text;//状态
                if (s1 != str)
                    continue;
                string s2 = lVI.SubItems[1].Text;//ID
                string s3 = lVI.SubItems[3].Text;//数据块
                string s4 = lVI.SubItems[4].Text;//地址
                string s5 = lVI.SubItems[5].Text;//长度
                string s6 = lVI.SubItems[6].Text;//数据


                if (s1 == str && s2 == UII && s3 == cbx_bank.SelectedIndex.ToString("X2") && s4 == txt_address.Text && txt_datalen.Text == s5 && s6 == id.Substring(8, id.Length - 10))
                {
                    int tempNum = int.Parse(lVI.SubItems[2].Text);
                    tempNum++;
                    lVI.SubItems[2].Text = tempNum.ToString();
                    findflag = true;
                    break;
                }
                //lVI.SubItems[2].Text = c.ToString();
            }
            if (!findflag)
            {
                ListViewItem li = new ListViewItem();
                li.SubItems.Clear();
                li.SubItems[0].Text = str;

                li.SubItems.Add(UII);
                li.SubItems.Add("1");
                li.SubItems.Add(cbx_bank.SelectedIndex.ToString("X2"));
                li.SubItems.Add(txt_address.Text);
                li.SubItems.Add(txt_datalen.Text);


                li.SubItems.Add(id.Substring(8, id.Length - 10));

                ctrList.Items.Add(li);
            }

        }

        private void DecodeWriteData(string id)//不指定UII写反馈数据
        {
            int count = ctrList.Items.Count;
            bool findflag = false;
            string UII;

            int Len = 2 * Byte.Parse(id.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);// int.Parse();//包长度
            UII = id.Substring(Len - 26, 28);
            foreach (ListViewItem lVI in ctrList.Items)
            {
                string s1 = lVI.SubItems[0].Text;//状态
                if (s1 != "写入")
                    continue;
                string s2 = lVI.SubItems[1].Text;//ID
                string s3 = lVI.SubItems[3].Text;//数据块
                string s4 = lVI.SubItems[4].Text;//地址
                string s5 = lVI.SubItems[5].Text;//长度



                if (s1 == "写入" && s2 == UII && s3 == cbx_bank.SelectedIndex.ToString("X2") && s4 == txt_address.Text && txt_datalen.Text == s5)
                {
                    int tempNum = int.Parse(lVI.SubItems[2].Text);
                    tempNum++;
                    lVI.SubItems[2].Text = tempNum.ToString();
                    findflag = true;
                    break;
                }
                //lVI.SubItems[2].Text = c.ToString();
            }
            if (!findflag)
            {
                ListViewItem li = new ListViewItem();
                li.SubItems.Clear();
                li.SubItems[0].Text = "写入";

                li.SubItems.Add(UII);
                li.SubItems.Add("1");
                li.SubItems.Add(cbx_bank.SelectedIndex.ToString("X2"));
                li.SubItems.Add(txt_address.Text);
                li.SubItems.Add(txt_datalen.Text);
                li.SubItems.Add(txt_datahex.Text);
                ctrList.Items.Add(li);
            }


        }
        private void DDecodeWriteData(string id)//指定UII写数据
        {
            int count = ctrList.Items.Count;
            bool findflag = false;
            string UII;
            UII = txt_UII.Text;
            foreach (ListViewItem lVI in ctrList.Items)
            {
                string s1 = lVI.SubItems[0].Text;//状态
                if (s1 != "写入")
                    continue;
                string s2 = lVI.SubItems[1].Text;//ID
                string s3 = lVI.SubItems[3].Text;//数据块
                string s4 = lVI.SubItems[4].Text;//地址
                string s5 = lVI.SubItems[5].Text;//长度
                string s6 = lVI.SubItems[6].Text;//数据


                if (s1 == "写入" && s2 == UII && s3 == cbx_bank.SelectedIndex.ToString("X2") && s4 == txt_address.Text && txt_datalen.Text == s5 && s6 == txt_datahex.Text)
                {
                    int tempNum = int.Parse(lVI.SubItems[2].Text);
                    tempNum++;
                    lVI.SubItems[2].Text = tempNum.ToString();
                    findflag = true;
                    break;
                }
                //lVI.SubItems[2].Text = c.ToString();
            }
            if (!findflag)
            {
                ListViewItem li = new ListViewItem();
                li.SubItems.Clear();
                li.SubItems[0].Text = "写入";
                li.SubItems.Add(UII);
                li.SubItems.Add("1");
                li.SubItems.Add(cbx_bank.SelectedIndex.ToString("X2"));
                li.SubItems.Add(txt_address.Text);
                li.SubItems.Add(txt_datalen.Text);
                li.SubItems.Add(txt_datahex.Text);
                ctrList.Items.Add(li);
            }


        }

        private void SendCommandToRmu900(string str0)
        {

            string str1, str2;
            str1 = "";
            for (int i = 2; i < str0.Length - 2; i += 2)
            {
                str2 = str0.Substring(i, 2);
                if (str2 == "FF" || str2 == "AA" || str2 == "55")
                    str2 = "FF" + str2;
                str1 += str2;

            }
            str0 = "AA" + str1 + "55";

            MatchCollection mc = Regex.Matches(str0, @"(?i)[\da-f]{2}");
            List<byte> buf = new List<byte>();//填充到这个临时列表中


            //依次添加到列表中
            foreach (Match m in mc)
            {
                //   Byte.Parse(m.ToString(), System.Globalization.NumberStyles.HexNumber);
                buf.Add(Byte.Parse(m.ToString(), System.Globalization.NumberStyles.HexNumber));
            }
            //  ;
            //转换列表为数组后发送
            if (comport.IsOpen)
                comport.Write(buf.ToArray(), 0, buf.Count);

        }


        public FrmUHF()
        {
            InitializeComponent();
            InitialiseControlValues();
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//zxy
        }

        private void InitialiseControlValues()
        {
            cmbParity.Items.Clear(); cmbParity.Items.AddRange(Enum.GetNames(typeof(Parity)));
            cmbStopBits.Items.Clear(); cmbStopBits.Items.AddRange(Enum.GetNames(typeof(StopBits)));

            cmbPortName.Items.Clear();
            cmb_Qvalue.SelectedIndex = 3;
        }
        #region serialport
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {//zxy
            // This method will be called when there is data waiting in the port's buffer
            // Read all the data waiting in the buffer and pasrse it

            /* http://forums.microsoft.com/MSDN/ShowPost.aspx?PageIndex=2&SiteID=1&PostID=293187
             * You would need to use Control.Invoke() to update the GUI controls
             * because unlike Windows Forms events like Button.Click which are processed 
             * in the GUI thread, SerialPort events are processed in a non-GUI thread 
             * (more precisely a ThreadPool thread). 
             */


            this.Invoke(new EventHandler(HandleSerialData));
        }
        private void HandleSerialData(object s, EventArgs e)
        {//zxy

            if (PortClosing) return;
            strinfo = "";
            try
            {
                Listening = true;//听到返回数据
                int n = comport.BytesToRead;//n为返回的字节数
                byte[] buf = new byte[n];//初始化buf 长度为n
                comport.Read(buf, 0, n);//读取返回数据并赋值到数组
                bool date_a_catched = false;//赋值成功
                maxbuf.AddRange(buf);//将buf数组添加到maxbuf

                while (maxbuf.Count > 1 && maxbuf[0] != 170)//数组必须以aa开头，否则移
                {
                    maxbuf.RemoveAt(1);
                    //                 //   maxbuf.Remove(0);
                    //  maxbuf.RemoveAt(0);
                }
                int i = 1;
                while (maxbuf.Count > 0 && i < maxbuf.Count - 1 && i % 2 == 0)
                {
                    i++;
                    if (maxbuf[i] == 0xFF && maxbuf[i + 1] == 0xFF) //连续两个FF,需要删除
                        maxbuf.RemoveAt(i - 1);

                    if (maxbuf[i] == 0xFF && maxbuf[i + 1] == 0x55) //连续两个55,需要删除
                        maxbuf.RemoveAt(i - 1);

                    if (maxbuf[i] == 0xFF && maxbuf[i + 1] == 0xAA) //连续两个55,需要删除
                        maxbuf.RemoveAt(i - 1);

                }

                if (maxbuf.Count == 5 && maxbuf[2] == 23)
                {
                    if (maxbuf[3] == 0)
                    {
                        strinfo = "销毁标签成功";
                        distflag = true;
                    }
                    else
                    {
                        distflag = false;
                        strinfo = "销毁标签失败";
                    }
                }


                if (maxbuf.Count < 4)
                {
                    maxbuf.Clear();
                    return;
                }
                if (maxbuf.Count == 5 && maxbuf[2] == 22)
                {
                    if (maxbuf[3] == 0)
                    {
                        strinfo = "锁定成功!";
                        distflag = true;
                    }

                    else
                    {
                        distflag = false;
                        strinfo = "锁定失败！";
                    }

                }
                if (maxbuf[2] == 20 && maxbuf.Count == 5)//指定UII写数据
                {
                    if (maxbuf[3] == 0)
                    {
                        byte[] binary_data_1 = new byte[maxbuf[1] + 2];
                        maxbuf.CopyTo(0, binary_data_1, 0, maxbuf.Count);
                        date_a_catched = true;
                        StringBuilder str0 = new StringBuilder();

                        foreach (byte b in binary_data_1)
                        {
                            str0.Append(b.ToString("X2"));

                        }
                        DDecodeWriteData(str0.ToString());
                    }
                    else
                    {
                        distflag = false;
                        strinfo = "写入数据失败！";

                    }
                }
                if (maxbuf.Count == 7 && maxbuf[2] == 19)//指定UII读取数据
                {
                    if (maxbuf[3] == 0)
                    {
                        byte[] binary_data_1 = new byte[maxbuf[1] + 2];
                        maxbuf.CopyTo(0, binary_data_1, 0, maxbuf.Count);
                        date_a_catched = true;
                        StringBuilder str0 = new StringBuilder();
                        string str1 = "读取";
                        foreach (byte b in binary_data_1)
                        {
                            str0.Append(b.ToString("X2"));

                        }
                        DecodeFixUIIData(str0.ToString(), str1);
                    }
                }
                if (maxbuf.Count == 5 && maxbuf[2] == 19)
                {
                    distflag = false;
                    strinfo = "读取数据失败！";

                }

                if (maxbuf.Count == 5 && maxbuf[2] == 21)//指定UII读取数据
                {
                    if (maxbuf[3] == 128)
                    {
                        byte[] binary_data_1 = new byte[maxbuf[1] + 2];
                        maxbuf.CopyTo(0, binary_data_1, 0, maxbuf.Count);
                        date_a_catched = true;
                        StringBuilder str0 = new StringBuilder();
                        string str1 = "擦出数据";
                        foreach (byte b in binary_data_1)
                        {
                            str0.Append(b.ToString("X2"));

                        }
                        DecodeFixUIIData(str0.ToString(), str1);
                    }
                    else
                    {
                        distflag = false;
                        strinfo = "擦出数据失败！";

                    }

                }

                if (maxbuf.Count <= 12 && maxbuf[maxbuf.Count - 1] == 85)
                {
                    byte[] crcTemp = new byte[maxbuf.Count];
                    maxbuf.CopyTo(0, crcTemp, 0, maxbuf.Count);

                    if (crcTemp[2] == 0)
                    {

                        strinfo = "成功连接UHF 模块!";
                        //  btn_opencom.Enabled = false;//使打开按钮无效
                        distflag = true;
                    }

                    if (crcTemp[2] == 1)
                    {
                        strinfo = "读取功率成功!";
                        //    byte[] bs = BitConverter.GetBytes(crcTemp);
                        //  bs[7] = 0;
                        tb_dbm.Text = Convert.ToString(crcTemp[4] & 127);


                    }
                    if (crcTemp[2] == 2)
                    {
                        strinfo = "设置功率成功!";
                        btn_repower.Text = cb_dbm.Text;
                    }
                    if (crcTemp[2] == 5)
                    {
                        strinfo = "读取频率设置成功!";
                        cb_FREMODE.SelectedIndex = crcTemp[4];//频率模式
                        if (crcTemp[5] == 0)
                            chb_50FREBASE.Checked = true;
                        else
                            chb_125FREBASE.Checked = true;
                        //cb_FREMODE.Items.IndexOf("中国标准920-925MHz");
                    }
                    if (crcTemp[2] == 6)
                    {

                        strinfo = "频率设置成功!";
                    }
                    if (crcTemp[2] == 7)
                    {
                        if (crcTemp[3] == 0 && crcTemp.Length > 11)
                        {
                            strinfo = "读取版本信息成功，版本号为" + crcTemp[10].ToString();
                        }
                        if (crcTemp[3] == 1)
                        {
                        strinfo = "未定义版本信息";
                        }
                    }
                    if (crcTemp[2] == 16)
                        strinfo = "开始单标签识别!";
                    //    if (crcTemp[2] == 17)
                    //      strinfo = "开始多标签识别，请将射频标签放置天线读取范围内!";
                    if (crcTemp[2] == 18)
                    {
                        distflag = false;
                        strinfo = "成功断开UHF模块的连接!";
                    }
                    if (crcTemp[2] == 33)
                    {
                        distflag = false;
                        strinfo = "写入数据失败！";
                    }


                    if (crcTemp[1] != 17)//获得串不是返回的标签数据
                    {
                        lb_status.Items.Add(strinfo);
                        maxbuf.RemoveRange(0, crcTemp[1] + 2);
                    }
                    //maxbuf.Clear();   

                }

                while (maxbuf.Count >= 19 && maxbuf[maxbuf.Count - 1] == 85)
                {
                    if (maxbuf[2] == 17 && maxbuf.Count > 3)
                    {
                        byte[] binary_data_1 = new byte[maxbuf[1] + 2];
                        maxbuf.CopyTo(0, binary_data_1, 0, maxbuf[1] + 2);
                        date_a_catched = true;
                        StringBuilder str0 = new StringBuilder();
                        foreach (byte b in binary_data_1)
                        {
                            str0.Append(b.ToString("X2"));
                        }
                        string id = str0.ToString().Substring(8, (maxbuf[1] - 3) * 2);
                        DecodeUIIData(id);//UII数据解析
                        // maxbuf.RemoveRange(0, maxbuf[1] + 3);
                    }
                    if (maxbuf.Count >= 10 && maxbuf[2] == 32 && maxbuf[maxbuf.Count - 1] == 85)//不指定UII读取数据
                    {
                        byte[] binary_data_1 = new byte[maxbuf[1] + 2];
                        maxbuf.CopyTo(0, binary_data_1, 0, maxbuf[1] + 2);

                        date_a_catched = true;
                        StringBuilder str0 = new StringBuilder();
                        foreach (byte b in binary_data_1)
                        {
                            str0.Append(b.ToString("X2"));
                        }


                        DecodeReadUIIData(str0.ToString());//UII数据解析
                        //  maxbuf.RemoveRange(0, maxbuf[1]+2);
                    }
                    if (maxbuf.Count >= 10 && maxbuf[2] == 33)//不指定UII写入数据
                    {
                        byte[] binary_data_1 = new byte[maxbuf[1] + 2];
                        maxbuf.CopyTo(0, binary_data_1, 0, maxbuf[1] + 2);

                        date_a_catched = true;
                        StringBuilder str0 = new StringBuilder();
                        foreach (byte b in binary_data_1)
                        {
                            str0.Append(b.ToString("X2"));
                        }
                        DecodeWriteData(str0.ToString());//UII数据解析

                    }
                    maxbuf.RemoveRange(0, maxbuf[1] + 2);
                }
            }
            finally
            {
                Listening = false;
            }
        }
        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            cmbPortName.Items.AddRange(ports);
            cmbPortName.SelectedIndex = cmbPortName.Items.Count > 0 ? 0 : -1;
            cmbBaudRate.SelectedIndex = cmbBaudRate.Items.IndexOf("57600");
            cmbParity.SelectedIndex = cmbParity.Items.IndexOf("None");
            cmbDataBits.SelectedIndex = cmbDataBits.Items.IndexOf("8");
            cmbStopBits.SelectedIndex = cmbStopBits.Items.IndexOf("One");
            btn_closecom.Enabled = false;

            ctrList.Clear();
            ctrList.Columns.Clear();
            ctrList.Columns.Clear();
            ctrList.Columns.Add("状态", 40);
            ctrList.Columns.Add("标签ID号", 120);
            ctrList.Columns.Add("次数", 50);
            ctrList.Columns.Add("数据块", 60);
            ctrList.Columns.Add("地址", 40);
            ctrList.Columns.Add("长度", 40);
            ctrList.Columns.Add("数据", 60);
            ctrList.Columns.Add("错误代码", 40);
            ctrList.GridLines = true;

            this.FormClosing += new FormClosingEventHandler(FrmUHF_FormClosing);
        }

        void FrmUHF_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.comport != null)
            {
                this.comport.Close();
            }
        }

        private void btn_opencom_Click(object sender, EventArgs e)
        {
            lb_status.Items.Clear();
            // 设置串口参数
            if (btn_opencom.Text == "连接")
            {
                if (comport.IsOpen) comport.Close();

                comport.PortName = cmbPortName.Text;//设置串口名称
                comport.BaudRate = int.Parse(cmbBaudRate.Text);//设置波特率
                comport.DataBits = int.Parse(cmbDataBits.Text);//设置数据位
                comport.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text);//设置停止位
                comport.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);//设置校验位

                try
                {
                    comport.Open();//尝试打开串口
                    SendCommandToRmu900("aa020055");//查询状态
                    while (Listening) Application.DoEvents();//等待处理
                    //    SendCommandToRmu900("aa020155");//查询功率
                    btn_opencom.Text = "断开连接";
                }
                catch (Exception ex)//进行异常捕获
                {
                    //捕获到异常信息，创建一个新的comm对象，之前的不能用了。
                    comport = new SerialPort();
                    //现实异常信息给客户。
                    MessageBox.Show(ex.Message);
                }
                //finally
                //{
                //    SendCommandToRmu900("aa020055");//查询状态
                //    while (Listening) Application.DoEvents();//等待处理
                ////    SendCommandToRmu900("aa020155");//查询功率
                //    btn_opencom.Text = "断开连接";
                //}
                //  btn_closecom.Enabled = true;    //使关闭按钮有效 
            }
            else
            {
                SendCommandToRmu900("aa021255");//断开连接命令
                btn_opencom.Text = "连接";

            }

        }

        private void btn_closecom_Click(object sender, EventArgs e)
        {
            SendCommandToRmu900("aa021255");//断开连接命令

            //  System.Threading.Thread.Sleep(200);
            //  while (Listening) Application.DoEvents();
            //   if (comport.IsOpen) comport.Close();
            //   btn_opencom.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!comport.IsOpen)
            {
                MessageBox.Show("串口没有打开，请打开串口！");
                return;
            }
            int n = 0;
            //16进制发送
            if (checkBoxHexSend.Checked)
            {
                //正则得到有效的十六进制数
                MatchCollection mc = Regex.Matches(txt_Send.Text, @"(?i)[\da-f]{2}");
                List<byte> buf = new List<byte>();//填充到这个临时列表中
                //依次添加到列表中
                foreach (Match m in mc)
                {
                    //   Byte.Parse(m.ToString(), System.Globalization.NumberStyles.HexNumber);
                    buf.Add(Byte.Parse(m.ToString(), System.Globalization.NumberStyles.HexNumber));
                }
                //  ;
                //转换列表为数组后发送
                comport.Write(buf.ToArray(), 0, buf.Count);
                //记录发送的字节数
                n = buf.Count;
            }
            else//ascii编码直接发送
            {
                string str0;
                str0 = txt_Send.Text;
                comport.WriteLine(str0);
                n = txt_Send.Text.Length;

            }
            send_count += n;
            //累加发送字节数
            labelSendCount.Text = "Send:" + send_count.ToString();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (Listening) Application.DoEvents();
            if (comport.IsOpen) comport.Close();
            this.Close();
            // Environment.Exit(0);
        }

        private void cmbPortName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txt_showinfo_TextChanged(object sender, EventArgs e)
        {

        }

        private void btn_RtxtClear_Click(object sender, EventArgs e)
        {
            txt_showinfo.Clear();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection IC = this.ctrList.SelectedItems;

            if (IC.Count > 0)
            {
                txt_UII.Text = IC[0].SubItems[1].Text;
            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            lb_status.Items.Clear();
            if (btn_start.Text == "识别标签")
            {
                int QValue = int.Parse(cmb_Qvalue.Text);
                Cmd = "aa0311" + QValue.ToString("X2") + "55";
                SendCommandToRmu900(Cmd);//开始识别标签命令
                btn_start.Text = "停止";
            }
            else
            {
                btn_start.Text = "识别标签";
                SendCommandToRmu900("aa021255");
            }

        }

        private void btn_setfreq_Click(object sender, EventArgs e)
        {

        }

        private void btn_readfrq_Click(object sender, EventArgs e)
        {
            SendCommandToRmu900("aa020555");//读取频率命令

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void btn_repower_Click(object sender, EventArgs e)
        {
            //发送读取功率命令

            SendCommandToRmu900("aa020155");

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (cb_dbm.Text == String.Empty)
            {
                MessageBox.Show("功率值尚未输入！");
                return;
            }
            try
            {
                byte pw = byte.Parse(cb_dbm.Text);
                Cmd = "aa040201" + pw.ToString("X2") + "55";
                SendCommandToRmu900(Cmd);//发送设置功率命令
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ctrList_ColumnClick(object sender, ColumnClickEventArgs e)
        {


        }

        private void ctrList_DoubleClick(object sender, EventArgs e)
        {
            if ((this.ctrList.FocusedItem != null) && (tabControl1.SelectedIndex == 1))//这个if必须的，不然会得到值但会报错   
            {
                //MessageBox.Show(this.listView1.FocusedItem.SubItems[0].Text);   
                txt_UII.Text = this.ctrList.FocusedItem.SubItems[1].Text;//获得的listView的值显示在文本框里   
            }
            //       if ((lVI.SubItems[0].Text== "识别") && (tabControl1.SelectedIndex == 1))
            //SubItems[1].Text

        }

        private void cB_uii_CheckedChanged(object sender, EventArgs e)
        {
            if (cB_uii.Checked)
                txt_UII.Enabled = false;
            else
                txt_UII.Enabled = true;
        }

        private void ta_Click(object sender, EventArgs e)
        {
            lb_status.Items.Clear();
            if (!cB_uii.Checked)
            {

                if (txt_UII.TextLength != 28)
                {
                    MessageBox.Show("UII 数据不正确!");
                    return;
                }
            }
            if ((txt_accesspsw.TextLength != 8) && (cb_safe.Checked))
            {
                MessageBox.Show("安全密码长度不够,安全密码长度为8!");
                return;
            }
            if (txt_address.TextLength == 0)
            {
                MessageBox.Show("请输入地址!");
                return;

            }
            Cmd = "";
            if (cb_safe.Checked)//选择了安全模式
            {
                Cmd = txt_accesspsw.Text;//

            }
            else
            {
                Cmd = "00000000";
            }
            Cmd = Cmd + cbx_bank.SelectedIndex.ToString("X2");//所选区块
            int add = int.Parse(txt_address.Text);
            Cmd = Cmd + add.ToString("X2");//输入地址
            int datalen = int.Parse(txt_datalen.Text);
            Cmd = Cmd + datalen.ToString("X2");


            if (cB_uii.Checked)//不指定UII
            {
                Cmd = "AA" + (Cmd.Length / 2 + 2).ToString("X2") + "20" + Cmd + "55";
            }
            else
            {
                Cmd = Cmd + txt_UII.Text;
                // Cmd = Cmd + txt_UII.Text.Substring(4, 4);
                Cmd = "AA" + (Cmd.Length / 2 + 2).ToString("X2") + "13" + Cmd + "55";

            }
            SendCommandToRmu900(Cmd);//读取标签数据
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
                cbx_bank.SelectedIndex = 0;
        }

        private void cmb_Qvalue_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cbx_bank_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            ctrList.Items.Clear();
            lb_status.Items.Clear();
        }

        private void ritedata_Click(object sender, EventArgs e)
        {
            lb_status.Items.Clear();
            if (!cB_uii.Checked)
            {

                if (txt_UII.TextLength != 28)
                {
                    MessageBox.Show("UII 数据不正确！");
                    return;
                }
            }
            if ((txt_accesspsw.TextLength != 8) && (cb_safe.Checked))
            {
                MessageBox.Show("安全密码长度不够,安全密码长度为8!");
                return;
            }
            if (txt_address.TextLength == 0)
            {
                MessageBox.Show("请输入地址!");
                return;

            }
            if (txt_datalen.TextLength == 0)
            {
                MessageBox.Show("请输入数据!");
                return;
            }
            //Cmd = "";
            //if (cb_safe.Checked)//选择了安全模式
            //{
            //    Cmd = txt_accesspsw.Text;//;//14为写入指令

            //}
            //else
            //{
            //    Cmd = "00000000";
            //}
            //Cmd = Cmd + cbx_bank.SelectedIndex.ToString("X2");//所选区块
            //int add = int.Parse(txt_address.Text);
            //Cmd = Cmd + add.ToString("X2");//输入地址
            //int datalen = int.Parse(txt_datalen.Text);
            //Cmd = Cmd + datalen.ToString("X2");
            //Cmd = Cmd + txt_datahex.Text;//十六进制数据

            //if (cB_uii.Checked)//不指定UII
            //{
            //    Cmd = "AA" + (Cmd.Length / 2 + 2).ToString("X2") +"21"+ Cmd +"55";
            //}
            //else
            //{
            //    Cmd = Cmd + txt_UII.Text;
            //    Cmd = "AA" + (Cmd.Length / 2 + 2).ToString("X2") +"14"+ Cmd + "55";
            //}
            //SendCommandToRmu900(Cmd);//写入标签数据

            string strRmuCmd = null;
            string strPwd = null;
            //string strAdd = null;
            int nAdd = 0;
            nAdd = int.Parse(txt_address.Text);
            if (cB_uii.Checked)//不指定UII
            {
                strRmuCmd = "21";
            }
            else
            {
                strRmuCmd = "14";
            }
            if (cb_safe.Checked)//选择了安全模式
            {
                strPwd = txt_accesspsw.Text;//;//14为写入指令
            }
            WriteDataToRmu900(strRmuCmd, strPwd, cbx_bank.SelectedIndex.ToString("X2"), nAdd, txt_datahex.Text, txt_UII.Text);
        }
        //
        /// <summary>
        /// 拼凑字符串，所有字符串已经转成最终形式，并将其发送到串口
        /// </summary>
        /// <param name="strRmuCmd_in"> 1 byte ,已转成16进制的字符串</param>
        /// <param name="strPwd_in"> 4 byte</param>
        /// <param name="strBank_in">1 byte</param>
        /// <param name="strAddress_in">1 byte</param>
        /// <param name="strData_in">要写入的数据</param>
        /// <param name="strUii"></param>
        /// <returns></returns>
        bool WriteDataToRmu900(string strRmuCmd_in, string strPwd_in, string strBank_in, int nAddress, string strData_in, string strUii)
        {
            if (null == strRmuCmd_in || strData_in == null)
            {
                return false;
            }
            int TotalDataLength = strData_in.Length;
            for (int i = 0; i < strData_in.Length; i += 4, nAddress++)
            {
                string strAddress_in = nAddress.ToString("X2");
                string strData2Send = strData_in.Substring(i, 4);
                string strCmd = "aa";

                int nLength = 0;// 命令长度，以byte计算
                nLength = 1 + 1 + 4 + 1 + 1 + 1 + 2;// length +cmd +pwd + bank + address + cnt + data len
                if (strRmuCmd_in == "21")//不指定UII
                {
                }
                else if (strRmuCmd_in == "14")//指定UII
                {
                    if (null == strUii)
                    {
                        return false;
                    }
                    nLength += strUii.Length / 2;// 加上uii的长度
                }
                else
                {
                    return false;
                }
                strCmd += nLength.ToString("X2");
                strCmd += strRmuCmd_in;
                if (strPwd_in != null)
                {
                    strCmd += strPwd_in;
                }
                else
                {
                    strCmd += "00000000";
                }
                strCmd += strBank_in;
                strCmd += strAddress_in;
                strCmd += "01";//目前硬件只支持 1
                strCmd += strData2Send;
                if (strRmuCmd_in == "14")//指定UII
                {
                    strCmd += strUii;
                }
                strCmd += "55";
                SendCommandToRmu900(strCmd);//写入标签数据
                Thread.Sleep(500);
            }

            return true;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            lb_status.Items.Clear();
            if (txt_UII.TextLength != 28)
            {
                MessageBox.Show("UII 数据不正确！");
                return;
            }

            if ((txt_accesspsw.TextLength != 8) && cb_safe.Checked)
            {
                MessageBox.Show("安全密码长度不够,安全密码长度为8!");
                return;
            }

            Cmd = "";
            if (cb_safe.Checked)//选择了安全模式
            {
                Cmd = txt_accesspsw.Text;//

            }
            else
            {
                Cmd = "00000000";
            }
            Cmd = Cmd + cbx_bank.SelectedIndex.ToString("X2");//所选区块
            int add = int.Parse(txt_address.Text);
            Cmd = Cmd + add.ToString("X2");//输入地址
            int datalen = int.Parse(txt_datalen.Text);
            Cmd = Cmd + datalen.ToString("X2");
            Cmd = Cmd + txt_datahex.Text;//十六进制数据
            Cmd = Cmd + txt_UII.Text;
            Cmd = "AA" + (Cmd.Length / 2 + 2).ToString("X2") + "15" + Cmd + "55";
            SendCommandToRmu900(Cmd);//擦出数据
        }
        private void btn_lockpara_Click(object sender, EventArgs e)
        {
            Frm_LockPara frmLockPara = new Frm_LockPara();
            frmLockPara.ShowDialog();
            txt_lockpara.Text = frmLockPara.lockpara;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //读取版本信息
            SendCommandToRmu900("aa020755");
        }

        private void btn_lock_Click(object sender, EventArgs e)
        {
            lb_status.Items.Clear();
            if (txt_UII.TextLength != 28)
            {
                MessageBox.Show("UII 数据不正确！");
                return;
            }

            if ((txt_accesspsw.TextLength != 8) && cb_safe.Checked)
            {
                MessageBox.Show("安全密码长度不够,安全密码长度为8!");
                return;
            }

            Cmd = "";
            if (cb_safe.Checked)//选择了安全模式
            {
                Cmd = txt_accesspsw.Text;//

            }
            else
            {
                Cmd = "00000000";
            }
            Cmd = Cmd + txt_lockpara.Text;
            Cmd = Cmd + txt_UII.Text;
            Cmd = "AA" + (Cmd.Length / 2 + 2).ToString("X2") + "16" + Cmd + "55";
            SendCommandToRmu900(Cmd);//锁定标签
        }

        private void btn_KillTag_Click(object sender, EventArgs e)
        {
            lb_status.Items.Clear();
            if (txt_UII.TextLength != 28)
            {
                MessageBox.Show("UII 数据不正确！");
                return;
            }
            if (txt_KillPsw.TextLength != 8)
            {
                MessageBox.Show("杀死密码长度为8！");
                return;
            }
            Cmd = txt_KillPsw.Text + txt_UII.Text;
            Cmd = "AA" + (Cmd.Length / 2 + 2).ToString("X2") + "17" + Cmd + "55";
            SendCommandToRmu900(Cmd);//销毁标签
        }

        private void cmbBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void txt_datahex_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }


    }
}
