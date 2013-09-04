using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace RfidReader
{
    public class UDPDataTransfer : IDataTransfer
    {
        //List<deleVoid_Byte_Func> delegateList = new List<deleVoid_Byte_Func>();
        public  Socket serverSocket;
         byte[] byteData = new byte[1024];
         int __port = 9001;
         public static StringBuilder sbuilder = new StringBuilder();
         public static ManualResetEvent Manualstate = new ManualResetEvent(true);
         string __serverIP = string.Empty;
        public UDPDataTransfer(int _port)
        {
            this.__port = _port;
        }
        public bool getReadyForTransfer()
        {
            bool bReady = false;
            try
            {
                //We are using UDP sockets
                serverSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);
                //IPAddress ip = IPAddress.Parse(this.__serverIP);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, this.__port);
                //                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);

                //Bind this address to the server
                serverSocket.Bind(ipEndPoint);
                //防止客户端强行中断造成的异常
                long IOC_IN = 0x80000000;
                long IOC_VENDOR = 0x18000000;
                long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                byte[] optionInValue = { Convert.ToByte(false) };
                byte[] optionOutValue = new byte[4];
                serverSocket.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);

                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                //The epSender identifies the incoming clients
                EndPoint epSender = (EndPoint)ipeSender;

                //Start receiving data
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length,
                    SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);

                //**********************************************************************

            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    string.Format("UDPServer.startUDPListening  -> error = {0}"
                    , ex.Message));
            }
            return bReady;
        }
        public  void OnReceive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;

                this.serverSocket.EndReceiveFrom(ar, ref epSender);

                string strReceived = Encoding.UTF8.GetString(byteData);

                Debug.WriteLine(
                    string.Format("UDPServer.OnReceive  -> received = {0}"
                    , strReceived));

                Array.Clear(byteData, 0, byteData.Length);
                int i = strReceived.IndexOf("\0");
                Manualstate.WaitOne();
                Manualstate.Reset();
                //todo here should deal with the received string
                sbuilder.Append(strReceived.Substring(0, i));
                Manualstate.Set();

                //Start listening to the message send by the user
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epSender,
                    new AsyncCallback(OnReceive), epSender);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    string.Format("UDPServer.OnReceive  -> error = {0}"
                    , ex.Message));
            }
        }
        #region IDataTransfer 成员
        public void removeParser()
        {
            //this.delegateList.Remove(parser);
        }

        public void AddParser(IParser parser)
        {
            //if (this.delegateList.Contains(parser) == false)
            //{
            //    this.delegateList.Add(parser);
            //}
        }

        //public void writeData(string data)
        public void writeData(byte[] data)
        {

        }

        public byte[] readData()
        {
            return null;
        }

        #endregion
    }
}
