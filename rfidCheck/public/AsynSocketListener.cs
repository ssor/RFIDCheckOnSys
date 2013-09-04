using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AsynchronousSocket
{

    public class AsynchronousSocketListener
    {
        int portNum = 13000;
        Socket listener = null;
        bool bStop = false;
        IPAddress ipAddress = null;

        public int PortNum
        {
            get { return portNum; }
            set { portNum = value; }
        }
        public event deleAsynSocketProcessMsg eventProcessMsg;
        public event deleAsynSocketListenerGetContent eventGetSendContent;
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public AsynchronousSocketListener()
        {
        }
        void ReturnProcessMsg(int nCode, string msg)
        {
            if (null != eventProcessMsg && !this.bStop)
            {
                eventProcessMsg(new AsynSocketProcessMsg(nCode, msg));
            }
            allDone.Set();
        }
        public void StopListener()
        {
            this.bStop = true;
            this.ClearListener();
        }
        void ClearListener()
        {
            if (null != this.listener)
            {
                try
                {
                    this.listener.Shutdown(SocketShutdown.Both);
                }
                catch (System.Exception ex)
                {
                    this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, ex.Message);
                }
                finally
                {
                    this.listener.Close();
                    this.listener = null;
                }
            }
        }
        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
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
                return;
            }
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, portNum);

            // Create a TCP/IP socket.
            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            //Console.Write(string.Format("IP: {0} Port: {1} Is Wainting for Connection...\n", ipAddress.ToString(), portNum.ToString()));
            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut
                    , string.Format("IP: {0} Port: {1} Is Wainting for Connection...\n", ipAddress.ToString(), portNum.ToString()));
                //listener.BeginAccept(
                //    new AsyncCallback(AcceptCallback),
                //    listener);
                listener.BeginAccept(
                                            new AsyncCallback(AcceptCallback),
                                            null);
                //while (true)
                //{

                // Start an asynchronous socket to listen for connections.
                //Console.WriteLine("Waiting for a connection...");
                //Console.WriteLine("**********************************************");
                // Wait until a connection is made before continuing.
                //allDone.WaitOne();
                //Console.WriteLine("A connectoin is accepted");
                //this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut,
                //                               "A connectoin is accepted");

                //}

            }
            catch (Exception e)
            {
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
                //Console.WriteLine(e.ToString());
            }
            this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut,
                               "\nPress ENTER to continue...");
            //Console.WriteLine("\nPress ENTER to continue...");
            //Console.Read();

        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut,
                               "A connectoin is accepted");
            try
            {
            //Socket listener = (Socket)ar.AsyncState;


            // Get the socket that handles the client request.
            Socket socketClient = listener.EndAccept(ar);

            listener.BeginAccept(
                                        new AsyncCallback(AcceptCallback),
                                        null);
            this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut
                , string.Format("IP: {0} Port: {1} Is Wainting for Connection...\n", ipAddress.ToString(), portNum.ToString()));

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = socketClient;
            //Console.WriteLine("below listener.BeginReceive");
            this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut,
                   "below listener.BeginReceive.");
            socketClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

        }
            catch (System.Exception e)
            {

                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
            }

        }

        public void ReadCallback(IAsyncResult ar)
        {

            this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut,
                                       "below ReadCallback.");
            try
            {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket socketClient = state.workSocket;

            // Read data from the client socket. 
                int bytesRead = 0;// socketClient.EndReceive(ar);
                bytesRead = socketClient.EndReceive(ar);

            if (bytesRead > 0)
            {
                //    // There  might be more data, so store the data received so far.
                //    //state.sb.Append(Encoding.ASCII.GetString( state.buffer, 0, bytesRead));
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                //    socketClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                //                                            new AsyncCallback(ReadCallback), state);
                //    // Check for end-of-file tag. If it is not there, read 
                //    // more data.

                //    //if (content.IndexOf("<EOF>") > -1)
                //    //{
                //    //    // All the data has been read from the 
                //    //    // client. Display it on the console.
                //    //    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                //    //        content.Length, content);
                //    //    // Echo the data back to the client.
                //    //    Send(handler, content);
                //    //}
                //    //else
                //    //{
                //    //    // Not all data received. Get more.
                //    //    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                //    //    new AsyncCallback(ReadCallback), state);
                //    //}
                }
                //else
                {
                content = state.sb.ToString();
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut,
                            string.Format("Read {0} bytes from socket. \n Data : {1}",
                            content.Length, content));
                    allDone.Reset();
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalEndReceive, content);
                //Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                //content.Length, content);
                // Echo the data back to the client.
                allDone.WaitOne(1000,false);
                string strR = this.GetSendContent();
                if (null != strR)
                {
                    Send(socketClient, strR);
                }
                else
                {
                    Send(socketClient, content);
                }

                }
            }
            catch (System.Exception e)
            {
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
            }

        }
        string GetSendContent()
        {
            string strRtn = null;
            if (null != this.eventGetSendContent)
            {
                strRtn = this.eventGetSendContent();
            }
            return strRtn;
        }
        private void Send(Socket socketClient, String data)
        {
            try
            {            // Convert the string data to byte data using ASCII encoding.
            //byte[] byteData = Encoding.ASCII.GetBytes(data);
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.
            //Console.WriteLine("below listener.BeginSend");
            socketClient.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), socketClient);
        }
            catch (System.Exception e)
            {
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket socketClient = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                //Console.WriteLine("below listener.EndSend");
                int bytesSent = socketClient.EndSend(ar);
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketNormalOutPut,
                string.Format("Sent {0} bytes to client.", bytesSent));
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                //Console.WriteLine("below handler.Shutdown(SocketShutdown.Both);");
                socketClient.Shutdown(SocketShutdown.Both);
                socketClient.Close();

            }
            catch (Exception e)
            {
                this.ReturnProcessMsg((int)enumAsynSocketPocessCode.SocketException, e.Message);
            }
        }

    }
}
