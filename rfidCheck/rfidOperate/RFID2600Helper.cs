using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace RfidReader
{
//: IRFIDHelper
    public class RFID2600Helper 
    {
        public RFID2600Helper()
        {
            dicCommandSuccess.Add((int)RFIDEventType.RMU_InventorySingle, true);
        }
        #region private method
        public void SendCommand(string command, RFIDEventType type)
        {
            if (null == command || type == RFIDEventType.RMU_Unknown)
            {
                return;
            }
            //当要将某命令选入发送到接收器时，首先将该命令的标识设为false，
            //当对应该命令的返回值正确时，将该标识重置为true，
            // 以此来判定该命令的执行情况
            dicCommandSuccess[(int)type] = false;

            this.WriteData(command);

            //等待命令执行一段时间
            WaitToSee(type);
            //如果执行到期后，命令标识仍为false，说明命令执行错误
            if (!GetEventStatusValue(type))
            {
                //在响应时间内无返回，肯定出现设备异常,且仅在该种情况下使用该功能
                RaiseEvent(RFIDEventType.RMU_Exception, type.ToString());
            }
        }
        bool GetEventStatusValue(RFIDEventType type)
        {
            MRE_DicEdit.WaitOne();
            return dicCommandSuccess[(int)type];
        }
        void WaitToSee(RFIDEventType type)
        {
            _CurrentEventType = type;
            RMUStatus.Reset();
            RMUStatus.WaitOne(5000, false);
        }
        void RaiseEvent(RFIDEventType eventType, object o)
        {
            if (eventType == RFIDEventType.WriteToSerialPort)
            {
                //if (null != evtWriteToSerialPort)
                //{
                //    evtWriteToSerialPort((byte[])o);
                //}
                return;
            }
            if (bStopCallback)
            {
                return;
            }

            //if (null != evtCardState)
            //{
            //    if (this.bRaiseEvent)
            //    {
            //        evtCardState(eventType, o);
            //    }
            //    else
            //        if (eventType == RFIDEventType.RMU_Exception)
            //        {
            //            bRaiseEvent = true;
            //            evtCardState(eventType, o);
            //        }
            //}
        }
        void WriteData(string value)
        {
            value = value.Replace(" ", "");
            //string str1, str2;
            //str1 = "";
            //for (int i = 2; i < value.Length - 2; i += 2)
            //{
            //    str2 = value.Substring(i, 2);
            //    if (str2 == "FF" || str2 == "AA" || str2 == "55")
            //        str2 = "FF" + str2;
            //    str1 += str2;

            //}
            //value = "AA" + str1 + "55";

            MatchCollection mc = Regex.Matches(value, @"(?i)[\da-f]{2}");
            List<byte> buf = new List<byte>();//填充到这个临时列表中

            //依次添加到列表中,以十六进制方式发送
            foreach (Match m in mc)
            {
                buf.Add(Byte.Parse(m.ToString(), System.Globalization.NumberStyles.HexNumber));
            }

            RaiseEvent(RFIDEventType.WriteToSerialPort, buf.ToArray());
        }
        void EditEventStatusValue(RFIDEventType type, bool bValue)
        {
            MRE_DicEdit.WaitOne();
            MRE_DicEdit.Reset();
            dicCommandSuccess[(int)type] = bValue;
            MRE_DicEdit.Set();
        }
        void StopSeeing(RFIDEventType type)
        {
            if (_CurrentEventType == type || _CurrentEventType == RFIDEventType.RMU_CardIsReady)
            {
                RMUStatus.Set();
            }
        }
        void BackgroundThreadWork(object sender, DoWorkEventArgs e)
        {
            RFIDEventArg rfidArg = (RFIDEventArg)e.Argument;
            RaiseEvent(rfidArg._type, rfidArg._arg);
        }
        void HandleEventInNewThread(RFIDEventType type, object arg)
        {
            RFIDEventArg rfidArg = new RFIDEventArg(type, arg);
            BackgroundWorker backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += new DoWorkEventHandler(BackgroundThreadWork);
            backgroundWorker1.RunWorkerAsync(rfidArg);
        }
        #endregion

        #region

        bool bRaiseEvent = true;//当需要往串口发送数据，而不需要其发起事件时使用的标识
        Dictionary<int, bool> dicCommandSuccess = new Dictionary<int, bool>();
        public static ManualResetEvent MRE_DicEdit = new ManualResetEvent(true);
        bool bStopCallback = false;//停止回调，防止串口死锁 当为true时，停止回调
        //event deleVoid_RFIDEventType_Object_Func evtCardState;
        public static ManualResetEvent RMUStatus = new ManualResetEvent(true);
        //public deleVoid_Byte_Func evtWriteToSerialPort;
        RFIDEventType _CurrentEventType;
        List<byte> maxbuf = new List<byte>();

        #endregion
        public void SendCommand(List<string> commands, RFIDEventType type, int interval)
        { }
        public void SendCommand(List<string> commands, RFIDEventType type, bool bReturnInstance)
        {
            if (commands == null || commands.Count <= 0)
            {
                return;
            }
            //如果要连续发送多条命令，并且在这多条命令发送完之前不需要进行返回值的处理
            if (!bReturnInstance && commands.Count > 1)
            {
                bRaiseEvent = false;
            }
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands.Count - 1 == i)
                {
                    bRaiseEvent = true;
                }
                SendCommand(commands[i], type);
                Thread.Sleep(200);
            }
        }
        bool IsEnd(byte bValue)
        {
            bool bR = false;
            if (bValue == 0x55)
            {
                bR = true;
            }
            return bR;
        }
        string BytesToHexString(byte[] value)
        {
            string str = "";
            for (int i = 0; i < value.Length; i++)
            {
                str += " ";
                str += value[i].ToString("X2");
            }
            return str;
        }
        /// <summary>
        /// 接收数据源（串口）数据
        /// </summary>
        /// <param name="value"></param>
        public void Parse(byte[] value)
        {
            try
            {
                maxbuf.AddRange(value);//将buf数组添加到maxbuf,只管添加
                Debug.WriteLine(string.Format("Parse -> {0} {1} value = {2}"
                                            , DateTime.Now.ToLongTimeString()
                                            , DateTime.Now.Millisecond.ToString()
                                            , BytesToHexString(maxbuf.ToArray())));
                while (maxbuf.Count > 0)
                {
                    List<byte> bytesCmd = null;

                    //****************************************************************************
                    // 从整个数据源中找出一段命令
                    int nEndIndex = -1;
                    if (maxbuf.Contains((byte)0xf0))
                    {
                        int nStartFlagIndex = -1;
                        nStartFlagIndex = maxbuf.IndexOf(0XF0);
                        if (nStartFlagIndex >= 0)
                        {
                            List<byte> tmpList = maxbuf.GetRange(nStartFlagIndex, maxbuf.Count - nStartFlagIndex);
                            nEndIndex = nStartFlagIndex;//这里只是暂存一下在maxbuf里面 0xf0 的位置
                            nStartFlagIndex = tmpList.IndexOf(0xf0);
                            if (tmpList.Count > nStartFlagIndex + 1)
                            {
                                int cmdLenght = tmpList[nStartFlagIndex + 1];//该条命令之后应该附带的长度
                                if (tmpList.Count >= cmdLenght + 2)
                                {
                                    bytesCmd = tmpList.GetRange(0, cmdLenght + 2);
                                    nEndIndex += cmdLenght + 2;//这里应该把要清除的部分的长度加上
                                    maxbuf.RemoveRange(0, nEndIndex);//将取出的命令从源中清除
                                }
                            }
                        }
                    }
                    if (bytesCmd == null)
                    {
                        return;
                    }
                    //*********************************************************************************


                    //*********************************************************************************
                    // 对命令格式进行检查和处理，明显不符合直接返回
                    if (bytesCmd.Count < 4)//返回命令的最小长度是4
                    {
                        return;
                    }
                    if (bytesCmd[0] != 0xf0)
                    {
                        return;
                    }
                    // 取出多于的 FF
                    //int nFFindex = bytesCmd.FindLastIndex(bytesCmd.Count - 3, IsFF);
                    //while (nFFindex != -1)
                    //{
                    //    byte b = bytesCmd[nFFindex + 1];
                    //    if (b == 0xaa || b == 0xff || b == 0x55)//如果 FF的下一个是aa或者ff或者55，将其删除
                    //    {
                    //        bytesCmd.RemoveAt(nFFindex);
                    //    }

                    //    if (nFFindex - 1 > 0)
                    //    {
                    //        nFFindex = bytesCmd.FindLastIndex(nFFindex - 1, IsFF);
                    //    }
                    //    else
                    //    {
                    //        break;
                    //    }
                    //}
                    Debug.WriteLine(
                        string.Format("RFID2600Helper.Parse  -> cmd = {0}"
                        , BytesToHexString(bytesCmd.ToArray())));
                    //dicCommandSuccess[(int)RFIDEventType.RMU_CardIsReady] = true;//如果有其它的命令返回，那该命令当然已经成功了
                    //*********************************************************************************
                    switch (bytesCmd[2])//第3位表示的是命令
                    {
                        case 0xee:
                            EditEventStatusValue(RFIDEventType.RMU_InventorySingle, true);
                            byte[] bUII = null;
                            if (bytesCmd[1] >= 0x10)
                            {
                                bUII = new byte[12];
                                bytesCmd.CopyTo(5, bUII, 0, 12);
                            }
                            StopSeeing(RFIDEventType.RMU_InventorySingle);
                            string sT = string.Empty;
                            foreach (byte b in bUII)
                            {
                                sT += b.ToString("X2");
                            }
                            HandleEventInNewThread(RFIDEventType.RMU_InventorySingle, sT);
                            break;

                        default:
                            return;
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Parse Exception -> {0}", ex.Message));
            }
        }


    }
}
