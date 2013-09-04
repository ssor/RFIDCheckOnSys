using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Diagnostics;
using rfidCheck;
using nsConfigDB;
using RfidReader;


namespace LogisTechBase.rfidCheck
{
    public partial class frmEditEPC : Form, IRFIDHelperSubscriber
    {
        Rmu900RFIDHelper rmu900Helper = null;
        IDataTransfer dataTransfer = null;
        SerialPort comport = null;
        public frmEditEPC()
        {
            InitializeComponent();

            dataTransfer = new SerialPortDataTransfer();
            //comport = StaticSerialPort.getStaticSerialPort();
            comport = new SerialPort(sysConfig.comportName, int.Parse(sysConfig.baudRate), Parity.None, 8, StopBits.One);
            ((SerialPortDataTransfer)dataTransfer).Comport = comport;
            rmu900Helper = new Rmu900RFIDHelper(dataTransfer);
            rmu900Helper.Subscribe(this);
            dataTransfer.AddParser(rmu900Helper);

        }
        public delegate void deleControlInvoke(object o);
        void UpdateEpcList(object o)
        {
            //把读取到的标签epc与产品的进行关联
            deleControlInvoke dele = delegate(object oTag)
            {
                string value = oTag as string;
                this.textBox1.Text = value;
                Debug.WriteLine(
                    string.Format("Form1.UpdateEpcList  ->  = {0}"
                    , value));
            };
            this.Invoke(dele, o);
        }
        public void NewMessageArrived()
        {
            //string r3 = rmu900Helper.CheckWriteEpc();
            //if (r3 != string.Empty)
            //{

            //    Debug.WriteLine("写入标签成功 " + r3);
            //}
            //string r2 = rmu900Helper.CheckInventory();
            //if (r2 != string.Empty)
            //{
            //    this.UpdateEpcList(r2);
            //    //AudioAlert.PlayAlert();
            //    Debug.WriteLine("读取到标签 " + r2);

            //}
            string r1 = rmu900Helper.ChekcInventoryOnce();
            if (r1 != string.Empty)
            {
                Debug.WriteLine("读取到标签 " + r1);
                AudioAlert.PlayAlert();
                this.UpdateEpcList(r1);
            }
            string r = rmu900Helper.CheckRmuStatus();
            if (r == "ok")
            {
                MessageBox.Show("设备状态良好！");
            }
        }

       
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string Value = string.Empty;
            string txtContent = textBox1.Text;
            if (txtContent == null)
            {
                Value = "未输入EPC";
                lblTip.ForeColor = Color.Red;
            }
            else
                if (txtContent.Length >= 0 && txtContent.Length < 24)
                {
                    Value = "当前EPC长度为 " + txtContent.Length.ToString();
                    lblTip.ForeColor = Color.Red;

                }
                else
                    if (txtContent.Length == 24)
                    {
                        Value = "当前EPC长度为 " + txtContent.Length.ToString();
                        lblTip.ForeColor = Color.Red;
                        if (Regex.IsMatch(txtContent, "[0-9a-fA-F]{24}"))
                        {
                            Value = "当前EPC符合要求";
                            lblTip.ForeColor = Color.Black;
                        }
                    }
                    else
                        if (txtContent.Length > 24)
                        {
                            Value = "当前EPC长度为 " + txtContent.Length.ToString();
                            lblTip.ForeColor = Color.Red;
                        }
            this.lblTip.Text = Value;
        }

      
        private void button2_Click(object sender, EventArgs e)
        {
            rmu900Helper.StartInventoryOnce();
        }
      
        private void button1_Click(object sender, EventArgs e)
        {
            this.rmu900Helper.StartWriteEpc(this.textBox1.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
        }

        private void txtSecretAgain_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void btnLockTag_Click(object sender, EventArgs e)
        {
            
        }
        void UpdateStatusLable(string value)
        {
            
        }

        private void btnSerialPortConfig_Click(object sender, EventArgs e)
        {
            //SerialPortConfig spc = new SerialPortConfig(this.ispc, null);
            //spc.ShowDialog();
        }

        private void btnSaveSecret_Click(object sender, EventArgs e)
        {
           
        }

        private void txtSecret_TextChanged(object sender, EventArgs e)
        {
            
        }
    }
}
