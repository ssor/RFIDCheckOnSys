using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using rfidCheck;
using RfidReader;

namespace LogisTechBase.rfidCheck
{

    public partial class FrmRfidCheck_Write : Form, IRFIDHelperSubscriber
    {
        Rmu900RFIDHelper rmu900Helper = null;
        IDataTransfer dataTransfer = null;
        SerialPort comport = null;
        /*
         标签初始化过程：
         * 1 获取标签
         * 2 读取密码区域数据
         * 3 检查是否已经使用
         * 4 与数据库字段相关联
         */
        //bool bInitiallizing = false;
        //rfidOperateUnitBase operaterGetTag = null;
        //rfidOperateUnitWirteEPC operateWriteTag = null;
        //IDataTransfer dataTransfer = null;
        //SerialPort comport = null;
        //SerialPort comport = new SerialPort();
        //RFIDHelper _RFIDHelper = new RFIDHelper();
        //ISerialPortConfigItem ispc =          SerialPortConfigItem.GetConfigItem(SerialPortConfigItemName.超高频RFID串口设置);
        //string tagEpc = string.Empty;
        string tagUII = string.Empty;
        public FrmRfidCheck_Write()
        {
            InitializeComponent();

            dataTransfer = new SerialPortDataTransfer();
            comport = new SerialPort(sysConfig.comportName, int.Parse(sysConfig.baudRate), Parity.None, 8, StopBits.One);
            ((SerialPortDataTransfer)dataTransfer).Comport = comport;
            rmu900Helper = new Rmu900RFIDHelper(dataTransfer);
            rmu900Helper.Subscribe(this);
            dataTransfer.AddParser(rmu900Helper);

            //this.lblSecretConfirm.Text = "";
            this.button1.Enabled = false;
            this.txtId.TextChanged += new EventHandler(txtId_TextChanged);
            label7.Text = "未选中学生";


            toolTip1.SetToolTip(this.button1, "学生信息与rfid标签关联");
            toolTip1.SetToolTip(this.btnPersonManage, "管理学生信息");
            toolTip1.SetToolTip(this.EditTag, "编辑rfid标签");

        }
#region 新加的方法
        public delegate void deleControlInvoke(object o);
        void UpdateEpcList(object o)
        {
            //把读取到的标签epc与产品的进行关联
            deleControlInvoke dele = delegate(object oTag)
            {
                string value = oTag as string;
                
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
                this.CheckIfEPCUsed(r1);
                //this.UpdateEpcList(r1);
            }
            string r = rmu900Helper.CheckRmuStatus();
            if (r == "ok")
            {
                MessageBox.Show("设备状态良好！");
            }
        }

#endregion
        void txtId_TextChanged(object sender, EventArgs e)
        {
            if (null == txtId.Text || txtId.Text.Trim().Length <= 0)
            {
                this.button1.Enabled = false;
            }
            else
            {
                this.button1.Enabled = true;
            }
        }
        void FrmRfidCheck_Write_Shown(object sender, System.EventArgs e)
        {
        }
        private void FrmRfidCheck_Write_Load(object sender, EventArgs e)
        {
            this.FormClosing += new FormClosingEventHandler(FrmRfidCheck_Write_FormClosing);
            this.ShowPerson();


            // 将串口数据输入到Helper类
            //comport.DataReceived += new SerialDataReceivedEventHandler(comport_DataReceived);
            //使得Helper类可以向串口中写入数据
            //_RFIDHelper.evtWriteToSerialPort += new deleVoid_Byte_Func(RFIDHelper_evtWriteToSerialPort);
            // 处理当前操作的状态
            //_RFIDHelper.evtCardState += new deleVoid_RFIDEventType_Object_Func(_RFIDHelper_evtCardState);
            //启动时检查设备状态
            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_GetStatus, RFIDEventType.RMU_CardIsReady);

        }

        void FrmRfidCheck_Write_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (comport.IsOpen)
            {
                comport.Close();
            }
        }


        private void LintEPC2Person(string xh, string epc)
        {
            Debug.WriteLine(
                string.Format("FrmRfidCheck_Write.LintEPC2Person  ->  = {0}   {1}"
                , xh, epc));
            //将EPC与学生相关联
            rfidCheck_CheckOn.UpdatePersonEPC(xh, epc);
            this.ShowPerson();
        }
        /// <summary>
        /// 查看该标签是否已经使用，如果尚未使用，则开始锁定
        /// </summary>
        private void CheckIfEPCUsed(object otagEpc)
        {
            string oEpc=(string)otagEpc;
            //if (bEpcDisposed == true)
            //{
            //    Debug.WriteLine(
            //        string.Format("FrmRfidCheck_Write.CheckIfEPCUsed  ->  = {0}"
            //        , "已经处理过读取到的标签"));
            //    return;
            //}
            //bEpcDisposed = true;//标签已经处理过
    //        if (bInitiallizing)
    //        {
    //            Debug.WriteLine(
    //                string.Format("FrmRfidCheck_Write.CheckIfEPCUsed  ->  = {0}"
    //                , "正在处理读取到的标签"));
    //            return;
    //        }
           deleControlInvoke dele = delegate(object o)
            {
    //            bInitiallizing = true;
               
    //            operateMessage om = (operateMessage)otagEpc;
    //            Debug.WriteLine(
    //string.Format("FrmRfidCheck_Write.CheckIfEPCUsed  ->  = {0}   {1}"
    //, om.status, om.message));
                bool result = false;
                //if (om.status == "success")
                //{
                  //  string tagEpc = (string)om.message;
                    result = rfidCheck_CheckOn.CheckEPCUsed(oEpc);
                    if (result == false)
                    {
                        // not used
                        //_RFIDHelper.RmuLockTagReserverdEpcTid(txtSecret.Text, tagUII);
                        //_RFIDHelper.RmuLockTagReserverdEpcTid("00000000", tagUII);
                        this.LintEPC2Person(txtId.Text, oEpc);
                        //bInitiallizing = false;
                    }
                    else
                    {
                        MessageBox.Show("该标签已经被使用！");
                       // bInitiallizing = false;
                        Debug.WriteLine(
                            string.Format("FrmRfidCheck_Write.CheckIfEPCUsed  ->  = {0}"
                            , "该标签已经被使用！"));
                    }
               // }
            };
            this.Invoke(dele, otagEpc);
        }
        void UpdateButton2(string value)
        {
            //this.button2.Text = value;
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
                    comport.Open();

                }
                comport.Write(value, 0, value.Length);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public void ShowPerson()
        {
            DataSet DataSet = rfidCheck_CheckOn.GetPersonDataSet();
            deleControlInvoke dele = delegate(object omyDataSet)
                {
                    DataSet myDataSet = (DataSet)omyDataSet;
                    if (myDataSet != null)
                    {
                        dataGridView1.DataSource = myDataSet.Tables[0];
                        int iNumberofStudents = myDataSet.Tables[0].Rows.Count;
                        this.groupBox2.Text = "学生列表 共有学生" + iNumberofStudents.ToString() + "名";

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


                    }
                };

            this.Invoke(dele, DataSet);
            //myDataSet.ReadXml("Person.xml");
        }
        void ResetDataGridDataSource(object ds)
        {
            DataSet myDataSet = (DataSet)ds;
            if (myDataSet != null)
            {
                dataGridView1.DataSource = myDataSet.Tables[0];
                int iNumberofStudents = myDataSet.Tables[0].Rows.Count;
                this.groupBox2.Text = "学生列表 共有学生" + iNumberofStudents.ToString() + "名";

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


            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].Selected = true;
                txtId.Text = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                txtName.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                txtTel.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                txtMail.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();

                string epc = dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString().Trim();
                if (epc == null || epc.Length <= 0)
                {
                    label7.Text = string.Format(" 选中学生{0}({1}),无考勤号",
                            dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(),
                           dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                }
                else
                {
                    label7.Text = string.Format(" 选中学生{0}({1}),考勤号为 {2}",
                                                dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString(),
                                               dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(),
                                              epc);
                }

            }
        }

        bool bEpcDisposed = false;//标识是否已经处理过标签，防止多次重复处理

        RFID_EPCWriter _EPCWriter = null;
        // 写卡之前首先检测环境是否设置完毕，比如设备连接状态，读卡器周围是否有卡等等
        private void button1_Click(object sender, EventArgs e)
        {
            rmu900Helper.StartInventoryOnce();
            string strID = txtId.Text;
            if (strID == null)
            {
                MessageBox.Show("请先选择要与标签关联的学生！");
            }
            bEpcDisposed = false;
            //if (strID.Length < 0 || strID.Length > 6 || !Regex.IsMatch(strID, "[0-9]{6,12}"))
            //{
            //    MessageBox.Show("学号应为六位数字！");
            //    return;
            //}
            //strID = RFIDHelper.GetFormatEPC(strID);

            //_EPCWriter = new RFID_EPCWriter(_RFIDHelper);
            //_EPCWriter.InitialTag(strID, null);
            //this.InitialTag(strID);
            //operaterGetTag = new rfidOperateUnitGetTagEPC(dataTransfer);
            //operaterGetTag.registeCallback(new deleRfidOperateCallback(CheckIfEPCUsed));
            //operaterGetTag.OperateStart(true);
        }
        void InitialTag(string epc)
        {
            /* 
            
            _RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_InventorySingle,
                                    RFIDEventType.RMU_InventorySingle);
            */
            //_RFIDHelper.SendCommand(RFIDHelper.RFIDCommand_RMU_Inventory,
            //RFIDEventType.RMU_Inventory);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPersonManage_Click(object sender, EventArgs e)
        {
            FrmRfidCheck_StudentManage frm = new FrmRfidCheck_StudentManage();
            frm.ShowDialog(this);
        }
        private void EditTag_Click(object sender, EventArgs e)
        {
            if (this.comport.IsOpen)
            {
                this.comport.Close();
            }
            frmEditEPC frm = new frmEditEPC();
            frm.ShowDialog();
        }
    }


}
