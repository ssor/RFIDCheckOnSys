using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LogisTechBase.rfidCheck;
using LogisTechBase;
using System.Diagnostics;

namespace rfidCheck
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Debug.WriteLine("form1");
        }

        private void 标签编辑EToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmEditEPC frm = new frmEditEPC();
            frm.ShowDialog();
        }

        private void 退出XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 学生卡分发DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmRfidCheck_Write frm = new FrmRfidCheck_Write();
            frm.ShowDialog();
        }

        private void 启动考勤服务RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmRfidCheck_Server frm = new FrmRfidCheck_Server();
            //frm.ShowDialog();
            frm.Show();
        }

        private void 启动考勤学生端UToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmRfidCheck_Client frm = new FrmRfidCheck_Client();
            //frm.ShowDialog();
            frm.Show();
        }

        private void 系统设置TToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSysSettings frm = new frmSysSettings();
            frm.ShowDialog();
        }

        private void 关于AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox2 afrm = new AboutBox2();
            afrm.ShowDialog();
        }

        private void 考勤信息统计SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmCheckStatistics sta = new FrmCheckStatistics();
            sta.ShowDialog();

        }
    }
}
