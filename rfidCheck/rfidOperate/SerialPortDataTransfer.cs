using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows.Forms;

namespace RfidReader
{
    public class SerialPortDataTransfer : IDataTransfer
    {
        IParser parser = null;
        SerialPort comport = null;
        public System.IO.Ports.SerialPort Comport
        {
            get { return comport; }
            set
            {
                comport = value;
                comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            }
        }
        public bool getReadyForTransfer()
        {
            bool bReady = false;
            try
            {
                if (this.comport.IsOpen == false)
                {
                    this.comport.Open();
                }
                bReady = true;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(
                    string.Format("SerialPortDataTransfer.getReadyForTransfer  ->  = {0}"
                    , ex.Message));
                MessageBox.Show(ex.Message, "信息提示");
            }
            return bReady;
        }
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int n = comport.BytesToRead;//n为返回的字节数
                byte[] buf = new byte[n];//初始化buf 长度为n
                comport.Read(buf, 0, n);//读取返回数据并赋值到数组
                this.parser.Parse(buf);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(
                    string.Format("SerialPortDataTransfer.port_DataReceived  -> exception = {0}"
                    , ex.Message));
            }
        }
        #region IDataTransfer 成员
        public void removeParser()
        {
            this.parser = null;
            //this.delegateList.Remove(parser);
        }

        public void AddParser(IParser parser)
        {
            this.parser = parser;
        }

        //public void writeData(string data)
        public void writeData(byte[] data)
        {
            if (this.comport != null)
            {
                try
                {
                    if (this.comport.IsOpen == false)
                    {
                        this.comport.Open();
                    }
                    this.comport.Write(data, 0, data.Length);

                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine(
                        string.Format("SerialPortDataTransfer.writeData  ->  = {0}"
                        , ex.Message));
                }
            }
        }

        public byte[] readData()
        {
            int n = 0;//n为返回的字节数
            n = comport.BytesToRead;//n为返回的字节数
            byte[] buf = new byte[n];//初始化buf 长度为n
            comport.Read(buf, 0, n);//读取返回数据并赋值到数组
            return buf;
        }

        #endregion
    }
}
