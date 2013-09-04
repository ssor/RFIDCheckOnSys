using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using nsConfigDB;

namespace rfidCheck
{
    public partial class frmSysSettings : Form
    {
        public frmSysSettings()
        {
            InitializeComponent();

            this.cmbBaut.Items.AddRange(new object[] {
            "300",
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200",
            "28800",
            "36000",
            "38400",
            "57600",
            "115200"});

            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            cmbPortName.Items.AddRange(ports);

            this.Shown += new EventHandler(frmSysSettings_Shown);
        }

        void frmSysSettings_Shown(object sender, EventArgs e)
        {
            string portName = string.Empty;
            string portBaut = string.Empty;
            string tcpPort = string.Empty;

            object o = ConfigDB.getConfig("comportName");
            if (o != null)
            {
                portName = (string)o;
            }
            o = ConfigDB.getConfig("baudRate");
            if (o != null)
            {
                portBaut = (string)o;
            }
            o = ConfigDB.getConfig("tcp_port");
            if (o != null)
            {
                tcpPort = (string)o;
            }
            this.cmbPortName.Text = portName;
            this.cmbBaut.Text = portBaut;
            this.txtPort.Text = tcpPort;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.checkValidation())
            {
                string portName = this.cmbPortName.Text;
                string portBaut = this.cmbBaut.Text;
                string tcpPort = this.txtPort.Text;
                ConfigDB.saveConfig("comportName", portName);
                ConfigDB.saveConfig("baudRate", portBaut);
                ConfigDB.saveConfig("tcp_port", tcpPort);

                sysConfig.baudRate = portBaut;
                sysConfig.comportName = portName;
                sysConfig.tcp_port = int.Parse(tcpPort);

                this.Close();
            }
        }
        bool checkValidation()
        {
            bool bR = true;
            int tcpPort = 5000;
            try
            {
                tcpPort = int.Parse(this.txtPort.Text);

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("端口设置不正确，请重新设置！");
                return false;
            }
            return bR;
        }

        private void cmbPortName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
