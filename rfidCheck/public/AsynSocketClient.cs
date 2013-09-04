using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace AsynchronousSocket
{
    //异步客户端
    //下面的示例程序创建一个连接到服务器的客户端。该客户端是用异步套接字生成的，因此在等待服务器返回响应时不挂起客户端应用程序的执行。该应用程序将字符串发送到服务器，然后在控制台显示该服务器返回的字符串。

    public class AsynchronousSocketClient
    {

        public event deleAsynSocketProcessMsg eventProcessMsg;
        // The port number for the remote device.
        private int port_num = 13000;
        bool bStop = false;
        Socket client = null;
        string dataToSend;
        public int Port_num
        {
            get { return port_num; }
            set { port_num = value; }
        }
        //public static string hostname = "ssor-PC";

        // The response from the remote device.
        private static String response = String.Empty;


        void ReturnProcessMsg(int nCode, string msg)
        {
            if (null != eventProcessMsg && !this.bStop)
            {
                eventProcessMsg(new AsynSocketProcessMsg(nCode, msg));
            }
        }
        public void StopClient()
        {
            this.bStop = true;
        }
        private void ClearClient()
        {
            if (this.client != null)
            {
                this.client.Shutdown(SocketShutdown.Both);
                this.client.Close();
                this.client = null;
            }
        }
        public void StartClient(string strDataSent, string strIP, int port)
        {
            IPAddress ipAddress = null;
            ipAddress = GetSeverIP(strIP);
            this.port_num = port;
            this.dataToSend = strDataSent;
            StartClient(ipAddress);
        }
        IPAddress GetSeverIP(string strIP)
        {
            IPAddress ipAddress = null;
            bool bParseIP = false;
            if (strIP != null)
            {
                bParseIP = IPAddress.TryParse(strIP, out ipAddress);
            }
            if (!bParseIP)
            {
                try
                {
                    // Establish the remote endpoint for the socket.
                    // The name of the 
                    // remote device is "host.contoso.com".
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
                }
                catch (Exception e)
                {
                    this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
                }
            }
            return ipAddress;
        }
        public void StartClient(IPAddress ipAddress)
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                //IPAddress ipAddress = null;
                //for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                //{
                //    ipAddress = ipHostInfo.AddressList[i];
                //    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                //    {
                //        break;
                //    }
                //    else
                //    {
                //        ipAddress = null;
                //    }
                //}
                //if (null == ipAddress)
                //{
                //    return;
                //}
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port_num);

                // Create a TCP/IP socket.
                client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                //connectDone.WaitOne();

                // Send test data to the remote device.
                //Send(client, "This is a test<EOF>qqqqqqqqqq");
                //Send(client, strDataSent);
                //Send(client, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                //sendDone.WaitOne();

                // Receive the response from the remote device.
                //Receive(client);
                //receiveDone.WaitOne();

                // Write the response to the console.
                //Console.WriteLine("Response received : {0}", response);
                //             this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut,
                //string.Format("Response received : {0}", response));
                // Release the socket.
                //client.Shutdown(SocketShutdown.Both);
                //client.Close();
                //client = null;
                //Console.ReadLine();

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            if (this.bStop)
            {
                this.ClearClient();
                return;
            }
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                //Console.WriteLine("Socket connected to {0}",
                //    client.RemoteEndPoint.ToString());
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut,
string.Format("Socket connected to {0}", client.RemoteEndPoint.ToString()));
                // Signal that the connection has been made.
                //connectDone.Set();
                Send(client, dataToSend);
            }
            catch (Exception e)
            {
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
                //Console.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client)
        {
            if (this.bStop)
            {
                this.ClearClient();
                return;
            }
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalBeginReceive, null);
            }
            catch (Exception e)
            {
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
                //Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (this.bStop)
            {
                this.ClearClient();
                return;
            }
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 0)
                    {
                        response = state.sb.ToString();
                    }
                    this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalEndReceive, response);
                    // Signal that all bytes have been received.
                    //receiveDone.Set();
                    ClearClient();
                }
            }
            catch (Exception e)
            {
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
                //Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, String data)
        {
            if (this.bStop)
            {
                this.ClearClient();
                return;
            }
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            //byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
            this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalBeginSend, data);
        }

        private void SendCallback(IAsyncResult ar)
        {
            if (this.bStop)
            {
                this.ClearClient();
                return;
            }
            this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalEndSend, null);
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut,
                   string.Format("Sent {0} bytes to server.", bytesSent));
                // Signal that all bytes have been sent.
                //sendDone.Set();
                Receive(client);
            }
            catch (Exception e)
            {
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
                //Console.WriteLine(e.ToString());
            }
        }
    }
}
