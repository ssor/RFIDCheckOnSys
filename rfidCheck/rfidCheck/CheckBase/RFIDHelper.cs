using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

namespace LogisTechBase
{
    public delegate void deleVoid_String_Func(string value);
    public delegate void deleVoid_Byte_Func(byte[] value);
    public delegate void deleVoid_RFIDEventType_Object_Func(RFIDEventType eventType,object o);

    public enum RMU_LockType
    {

    }
    public enum RMU_CommandType
    {
        RMU_GetStatus = 0x00
        , RMU_GetPower = 0x01
            ,
        RMU_SetPower = 0x02
            ,
        RMU_GetFrequency = 0x05
            ,
        RMU_SetFrequency = 0x06
            ,
        RMU_Inventory = 0x10
            ,
        RMU_InventoryAnti = 0x11
            ,
        RMU_StopGet = 0x12
            ,
        RMU_ReadData = 0x13
            ,
        RMU_WriteData = 0x14
            ,
        RMU_EraseData = 0x15
            ,
        RMU_LockMem = 0x16
            ,
        RMU_KillTag = 0x17
            ,
        RMU_InventorySingle = 0x18
            ,
        RMU_WeigandInvetory = 0x19
            ,
        RMU_SingleReadData = 0x20
            , RMU_SingleWriteData = 0x21
    }
    public enum RFIDEventType
    {

        RMU_CardIsReady = 1
        ,RMU_GetPower 
        ,RMU_SetPower
        ,RMU_GetFrequency 
        ,RMU_SetFrequency 
        ,RMU_Inventory 
        ,RMU_InventoryAnti 
        ,RMU_StopGet 
        ,RMU_ReadData 
        ,RMU_WriteData 
        ,RMU_EraseData 
        ,RMU_LockMem 
        ,RMU_KillTag 
        ,RMU_InventorySingle 
        ,RMU_WeigandInvetory 
        ,RMU_SingleReadData 
        ,RMU_SingleWriteData//17
        ,RMU_Exception


        ,WriteToSerialPort


    }
    public class RFIDEventArg
    {
        public object _arg;
        public RFIDEventType _type;
        public RFIDEventArg(RFIDEventType type,object arg)
        {
            this._type = type;
            this._arg = arg;
        }
    }
    public class ObjCommand
    {
        public string _strCommand;
        public RFIDEventType eventType;

        public ObjCommand(string command,RFIDEventType type)
        {
            this._strCommand = command;
            this.eventType = type;
        }
    }
    /// <summary>
    /// 该类负责解析rfid命令，并根据命令定义发起相应事件
    /// </summary>
    public class RFIDHelper
    {
        #region 常用命令字符串

        public static string RFIDCommand_RMU_GetStatus = "aa 02 00 55";
        public static string RFIDCommand_RMU_GetVersion = "aa 02 07 55";
        public static string RFIDCommand_RMU_InventoryAnti3 = "aa 03 11 03 55";
        public static string RFIDCommand_RMU_Inventory = "aa 02 10 55";
        public static string RFIDCommand_RMU_InventorySingle = "aa 02 18 55";
        public static string RFIDCommand_RMU_StopGet = "aa 02 12 55";
        public static string RFIDCommand_RMU_GetPower = "aa 02 01 55";
        public static string RFIDCommand_RMU_ReadData =
            "aa [命令长度] 13 [密码] [数据块] [地址] [长度] [标签UII] 55";
        public static string RFIDCommand_RMU_ReadDataSingle =
            "aa [命令长度] 20 [密码] [数据块] [地址] [长度] 55";
        public static string RFIDCommand_RMU_WriteData =
            "aa [命令长度] 14 [密码] [数据块] [地址] [长度] [数据] [标签UII] 55";
        public static string RFIDCommand_RMU_WriteDataSingle =
            "aa [命令长度] 21 [密码] [数据块] [地址] [长度] [数据] 55";
        public static string RFIDCommand_RMU_EraseData =
            "aa [命令长度] 15 [密码] [数据块] [地址] [长度] [标签UII] 55";
        public static string RFIDCommand_RMU_LockMem =
            "aa [命令长度] 16 [密码] [锁定命令] [标签UII] 55";
        public static string RFIDCommand_RMU_KillTag =
            "aa [命令长度] 17 [销毁密码] [标签UII] 55";


        #endregion


        #region Events
        //将信息写入到串口（当作函数指针使用 ）
        public event deleVoid_Byte_Func evtWriteToSerialPort;
        public event deleVoid_RFIDEventType_Object_Func evtCardState;
        #endregion

        #region Members
        bool bStopCallback = false;//停止回调，防止串口死锁 当为true时，停止回调
        bool bRaiseEvent = true;//当需要往串口发送数据，而不需要其发起事件时使用的标识
        List<byte> maxbuf = new List<byte>();
        Dictionary<int, bool> dicCommandSuccess = new Dictionary<int, bool>();
        //当检查设备状态时，需要给设备一定的反应时间，如果超过该时间尚未返回数据，则表示设备尚未准备就绪
        public static ManualResetEvent RMUStatus = new ManualResetEvent(true);
        public static ManualResetEvent MRE_DicEdit = new ManualResetEvent(true);
        public bool RMUisReady = false;
        public bool CommandSuccess = false;
        RFIDEventType _CurrentEventType;

        //bool _bExistCardAround;//周围是否有卡存在
        //public bool bExistCardAround
        //{
        //    get { return _bExistCardAround; }
        //    set { _bExistCardAround = value; }
        //}
        #endregion

        public RFIDHelper()
        {
            for (int i = 1; i <= 17; i++)
            {
                dicCommandSuccess.Add(i, true);
            }
        }
        public void StartCallback()
        {
            this.bStopCallback = false;
        }
        public void StopCallback()
        {
            this.bStopCallback = true;
        }
        public void CheckRmuStatus()
        {
            RMUisReady = false;
            this.WriteData(RFIDHelper.RFIDCommand_RMU_GetStatus);
            RMUStatus.Reset();
            RMUStatus.WaitOne(300,false);
            if (!RMUisReady)
            {
                RaiseEvent(RFIDEventType.RMU_CardIsReady, RMUisReady.ToString());
            }

        }
        #region private method
        void WriteData(string value)
        {
            string str1, str2;
            str1 = "";
            for (int i = 2; i < value.Length - 2; i += 2)
            {
                str2 = value.Substring(i, 2);
                if (str2 == "FF" || str2 == "AA" || str2 == "55")
                    str2 = "FF" + str2;
                str1 += str2;

            }
            value = "AA" + str1 + "55";

            Debug.WriteLine(string.Format("WriteData -> {0}", value));

            MatchCollection mc = Regex.Matches(value, @"(?i)[\da-f]{2}");
            List<byte> buf = new List<byte>();//填充到这个临时列表中


            //依次添加到列表中
            foreach (Match m in mc)
            {
                buf.Add(Byte.Parse(m.ToString(), System.Globalization.NumberStyles.HexNumber));
            }

            RaiseEvent(RFIDEventType.WriteToSerialPort, buf.ToArray());
        }

        void WaitToSee(RFIDEventType type)
        {
            _CurrentEventType = type;
            RMUStatus.Reset();
            RMUStatus.WaitOne(1000,false);
        }
        void StopSeeing(RFIDEventType type)
        {
            if (_CurrentEventType == type || _CurrentEventType == RFIDEventType.RMU_CardIsReady)
            {
                RMUStatus.Set();
            }
        }
        bool GetEventStatusValue(RFIDEventType type)
        {
            MRE_DicEdit.WaitOne();
            return dicCommandSuccess[(int)type];

        }
        void EditEventStatusValue(RFIDEventType type,bool bValue)
        {
            MRE_DicEdit.WaitOne();
            MRE_DicEdit.Reset();
            dicCommandSuccess[(int)type] = bValue;
            MRE_DicEdit.Set();
        }
        void BackgroundThreadWork(object sender,DoWorkEventArgs e)
        {
            RFIDEventArg rfidArg = (RFIDEventArg)e.Argument;
            RaiseEvent(rfidArg._type, rfidArg._arg);
        }
        void HandleEventInNewThread(RFIDEventType type,object arg)
        {
            RFIDEventArg rfidArg = new RFIDEventArg(type, arg);
            BackgroundWorker backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += new DoWorkEventHandler(BackgroundThreadWork);
            backgroundWorker1.RunWorkerAsync(rfidArg);
        }
        void HandleEventInNewThread(RFIDEventType type)
        {
            HandleEventInNewThread(type, null);
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
        bool IsEnd(byte bValue)
        {
            bool bR = false;
            if (bValue == 0x55)
            {
                bR = true;
            }
            return bR;
        }
        bool IsFF(byte bValue)
        {
            bool bR = false;
            if (bValue == 0xff)
            {
                bR = true;
            }
            return bR;
        }

        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="type"></param>
        /// <param name="bReturnInstance">true，需要对每一个命令进行处理</param>
        public void SendCommand(List<string> commands, RFIDEventType type,bool bReturnInstance)
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
        public void SendCommand(List<string> commands,RFIDEventType type)
        {
            SendCommand(commands, type, true);

        }
        public void SendCommand(string command,RFIDEventType type)
        {
            Debug.WriteLine(string.Format("SendCommand -> {0} {1}", DateTime.Now.ToLongTimeString() , DateTime.Now.Millisecond.ToString()));
            dicCommandSuccess[(int)type] = false;

            this.WriteData(command);

            WaitToSee(type);
            Debug.WriteLine(string.Format("SendCommand -> {0} {1} type = {2} value = {3}",
                DateTime.Now.ToLongTimeString(),
                DateTime.Now.Millisecond.ToString(), 
                type.ToString(),
                GetEventStatusValue(type).ToString()));
            if (!GetEventStatusValue(type))
            {
                //在响应时间内无返回，肯定出现设备异常,且仅在该种情况下使用该功能
                RaiseEvent(RFIDEventType.RMU_Exception, type.ToString());
            }
        }

        //public void SendCommand(string command)
        //{
        //    CommandSuccess = false;
        //    this.WriteData(command);

        //    RMUStatus.Reset();
        //    RMUStatus.WaitOne(5000);
        //    if (!CommandSuccess)
        //    {
        //        //在响应时间内无返回，肯定出现设备异常,且仅在该种情况下使用该功能
        //        RaiseEvent(RFIDEventType.RMU_Exception, CommandSuccess.ToString());
        //    }
        //}
        /// <summary>
        /// 接收数据源（串口）数据
        /// </summary>
        /// <param name="value"></param>
        public void Parse(byte[] value)
        {
            //Debug.WriteLine(string.Format("Parse -> {0}",BytesToHexString(value)));
            try
            {
                maxbuf.AddRange(value);//将buf数组添加到maxbuf,只管添加

                Debug.WriteLine(string.Format("Parse -> {0} {1} value = {2}"
                    , DateTime.Now.ToLongTimeString()
                    , DateTime.Now.Millisecond.ToString()
                    ,BytesToHexString(maxbuf.ToArray())));
                while (maxbuf.Count > 0)
                {
                    //****************************************************************************
                    // 从整个数据源中找出一段命令
                    int nEndIndex = maxbuf.FindIndex(IsEnd);
                    while (nEndIndex != -1)
                    {
                        if (nEndIndex > 0)
                        {
                            if (maxbuf[nEndIndex - 1] != 0xff)// eg: aa 55,此时55肯定标识结尾
                            {
                                break;
                            }
                            else
                            {
                                if (nEndIndex - 2 >= 0)
                                {
                                    if (maxbuf[nEndIndex - 2] == 0xff)// eg: aa ff ff 55,此时55肯定标识结尾
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        if (nEndIndex + 1 < maxbuf.Count)
                        {
                            nEndIndex = maxbuf.FindIndex(nEndIndex + 1, IsEnd);
                        }
                        else
                        {
                            return;//没找到一个完整的命令字符串，无法继续处理，直接返回
                        }
                    }
                    //*********************************************************************************
                    List<byte> bytesCmd = maxbuf.GetRange(0, nEndIndex + 1);
                    maxbuf.RemoveRange(0, nEndIndex + 1);//将取出的命令从源中清除


                    //*********************************************************************************
                    // 对命令格式进行检查和处理，明显不符合直接返回
                    if (bytesCmd.Count < 4)//返回命令的最小长度是4
                    {
                        return;
                    }
                    if (bytesCmd[0] != 0xaa)
                    {
                        return;
                    }
                    // 取出多于的 FF
                    int nFFindex = bytesCmd.FindLastIndex(bytesCmd.Count - 3, IsFF);
                    while (nFFindex != -1)
                    {
                        byte b = bytesCmd[nFFindex + 1];
                        if (b == 0xaa || b == 0xff || b == 0x55)//如果 FF的下一个是aa或者ff或者55，将其删除
                        {
                            bytesCmd.RemoveAt(nFFindex);
                        }

                        if (nFFindex - 1 > 0)
                        {
                            nFFindex = bytesCmd.FindLastIndex(nFFindex - 1, IsFF);
                        }
                        else
                        {
                            break;
                        }
                    }

                    dicCommandSuccess[(int)RFIDEventType.RMU_CardIsReady] = true;//如果有其它的命令返回，那该命令当然已经成功了
                    //*********************************************************************************
                    switch (bytesCmd[2])//第3位表示的是命令
                    {
                        case 0:
                            EditEventStatusValue(RFIDEventType.RMU_CardIsReady, true);
                            //RMUStatus.Set();
                            StopSeeing(RFIDEventType.RMU_CardIsReady);
                            HandleEventInNewThread(RFIDEventType.RMU_CardIsReady);
                            break;
                        case 0x10:
                            EditEventStatusValue(RFIDEventType.RMU_Inventory, true);
                            StopSeeing(RFIDEventType.RMU_Inventory);
                            if (bytesCmd[3] == 1)
                            {
                                //_bExistCardAround = false;
                                HandleEventInNewThread(RFIDEventType.RMU_Inventory);
                                Debug.WriteLine(string.Format("RMU_Inventory -> {0} {1} no UII", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond.ToString()));
                            }

                            if (bytesCmd[3] == 0)
                            {
                                byte[] bUII = new byte[bytesCmd.Count - 5];
                                bytesCmd.CopyTo(4, bUII, 0, bytesCmd.Count - 5);
                                string sT = string.Empty;
                                foreach (byte b in bUII)
                                {
                                    sT += b.ToString("X2");
                                }
                                //_bExistCardAround = true;
                                HandleEventInNewThread(RFIDEventType.RMU_Inventory, sT);
                                Debug.WriteLine(string.Format("RMU_Inventory -> {0} {1} UII = {2}"
                                    , DateTime.Now.ToLongTimeString(),
                                    DateTime.Now.Millisecond.ToString()
                                    , sT));
                            }
                            break;
                        case 0x11:
                            EditEventStatusValue(RFIDEventType.RMU_InventoryAnti, true);

                            StopSeeing(RFIDEventType.RMU_InventoryAnti);
                            if (bytesCmd[3] == 1)
                            {
                                HandleEventInNewThread(RFIDEventType.RMU_InventoryAnti,"ok");
                                Debug.WriteLine(string.Format("RMU_InventoryAnti -> {0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond.ToString()));
                            }

                            if (bytesCmd[3] == 0)
                            {
                                byte[] bUII = new byte[bytesCmd.Count - 5];
                                bytesCmd.CopyTo(4, bUII, 0, bytesCmd.Count - 5);
                                string sT = string.Empty;
                                foreach (byte b in bUII)
                                {
                                    sT += b.ToString("X2");
                                }
                                HandleEventInNewThread(RFIDEventType.RMU_InventoryAnti, sT);
                                Debug.WriteLine(string.Format("RMU_Inventory -> {0} {1} UII = {2}"
                                , DateTime.Now.ToLongTimeString(),
                                DateTime.Now.Millisecond.ToString()
                                , sT));
                            }
                            break;
                        case 0x12:
                            EditEventStatusValue(RFIDEventType.RMU_StopGet, true);
                            StopSeeing(RFIDEventType.RMU_StopGet);
                            //_bExistCardAround = false;
                            Debug.WriteLine(string.Format("RMU_StopGet -> {0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond.ToString()));
                            HandleEventInNewThread(RFIDEventType.RMU_StopGet);
                            break;
                        case 0x13://指定UII读取数据
                            {
                                EditEventStatusValue(RFIDEventType.RMU_ReadData, true);
                                StopSeeing(RFIDEventType.RMU_ReadData);
                                if (bytesCmd[3] == 0)
                                {
                                    byte[] bUII = new byte[bytesCmd.Count - 5];
                                    bytesCmd.CopyTo(4, bUII, 0, bytesCmd.Count - 5);
                                    string sT = string.Empty;
                                    foreach (byte b in bUII)
                                    {
                                        sT += b.ToString("X2");
                                    }
                                    //_bExistCardAround = true;
                                    HandleEventInNewThread(RFIDEventType.RMU_ReadData, sT);
                                    Debug.WriteLine(string.Format("RMU_ReadData -> {0} {1} Data = {2}"
                                        , DateTime.Now.ToLongTimeString(),
                                        DateTime.Now.Millisecond.ToString()
                                        , sT));
                                    //DecodeFixUIIData(str0.ToString(), str1);
                                }
                                else
                                {
                                    //读取数据失败
                                    HandleEventInNewThread(RFIDEventType.RMU_ReadData);
                                }
                            }
                            break;
                        case (int)RMU_CommandType.RMU_InventorySingle:
                            EditEventStatusValue(RFIDEventType.RMU_InventorySingle, true);

                            StopSeeing(RFIDEventType.RMU_InventorySingle);
                            if (bytesCmd[3] > 0)
                            {
                                HandleEventInNewThread(RFIDEventType.RMU_InventorySingle);
                                Debug.WriteLine(string.Format("RMU_InventorySingle Failed-> {0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond.ToString()));
                            }

                            if (bytesCmd[3] == 0)
                            {
                                byte[] bUII = new byte[bytesCmd.Count - 5];
                                bytesCmd.CopyTo(4, bUII, 0, bytesCmd.Count - 5);
                                string sT = string.Empty;
                                foreach (byte b in bUII)
                                {
                                    sT += b.ToString("X2");
                                }
                                HandleEventInNewThread(RFIDEventType.RMU_InventorySingle, sT);
                                Debug.WriteLine(string.Format("RMU_InventorySingle -> {0} {1} UII = {2}"
                                                , DateTime.Now.ToLongTimeString(),
                                                DateTime.Now.Millisecond.ToString()
                                                , sT));
                            }
                            break;
                        case (int)RMU_CommandType.RMU_LockMem:
                            EditEventStatusValue(RFIDEventType.RMU_LockMem, true);

                            StopSeeing(RFIDEventType.RMU_LockMem);
                            if (bytesCmd[3] > 0)
                            {
                                HandleEventInNewThread(RFIDEventType.RMU_LockMem);
                                Debug.WriteLine(string.Format("RMU_LockMem Failed-> {0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond.ToString()));
                            }

                            if (bytesCmd[3] == 0)
                            {
                                HandleEventInNewThread(RFIDEventType.RMU_LockMem, "ok");
                                Debug.WriteLine(string.Format("RMU_LockMem -> {0} {1} OK"
                                                , DateTime.Now.ToLongTimeString(),
                                                DateTime.Now.Millisecond.ToString()
                                               ));
                            }
                            break;
                        case 0x20://不指定UII读取标签数据
                            EditEventStatusValue(RFIDEventType.RMU_SingleReadData, true);
                            StopSeeing(RFIDEventType.RMU_SingleReadData);
                            if (bytesCmd[3] == 0)
                            {
                                int nDataCount = bytesCmd.Count - 5 - 14;//14 UII length
                                int nTotalCount = bytesCmd.Count - 5 + 1;//14 UII length
                                byte[] binary_data_1 = new byte[nTotalCount];
                                byte[] bytesData = new byte[nDataCount];
                                byte[] bytesUii = new byte[14];
                                //bytesCmd.CopyTo(4, binary_data_1, 0, nDataCount);
                                bytesCmd.CopyTo(4, bytesData, 0, nDataCount);
                                bytesCmd.CopyTo(4 + nDataCount, bytesUii, 0, 14);
                                //byte[] sign = Encoding.ASCII.GetBytes("&");
                                //binary_data_1[nDataCount] = sign[0];
                                //bytesCmd.CopyTo(4 + nDataCount, binary_data_1, nDataCount, 14);
                                StringBuilder str0 = new StringBuilder();

                                foreach (byte b in bytesData)
                                {
                                    str0.Append(b.ToString("X2"));
                                }
                                str0.Append("&");
                                foreach (byte b in bytesUii)
                                {
                                    str0.Append(b.ToString("X2"));
                                }

                                //foreach (byte b in binary_data_1)
                                //{
                                //    str0.Append(b.ToString("X2"));

                                //}
                                HandleEventInNewThread(RFIDEventType.RMU_SingleReadData, str0.ToString());
                                Debug.WriteLine(string.Format("RMU_SingleReadData -> {0} {1} Data = {2}"
                                    , DateTime.Now.ToLongTimeString(),
                                    DateTime.Now.Millisecond.ToString()
                                    , str0.ToString()));
                            }
                            else
                            {
                                HandleEventInNewThread(RFIDEventType.RMU_SingleReadData);
                                Debug.WriteLine(string.Format("RMU_SingleReadData -> {0} {1}  Failed"
                                              , DateTime.Now.ToLongTimeString(),
                                                DateTime.Now.Millisecond.ToString()));
                            }
                            break;
                        case 0x21://不指定UII写入标签数据
                            {
                                EditEventStatusValue(RFIDEventType.RMU_SingleWriteData, true);
                                StopSeeing(RFIDEventType.RMU_SingleWriteData);
                                if (bytesCmd[3] == 0)
                                {
                                    int uiiLen = 14;
                                    byte[] bUII = new byte[uiiLen];
                                    bytesCmd.CopyTo(4, bUII, 0, uiiLen);
                                    string sT = string.Empty;
                                    foreach (byte b in bUII)
                                    {
                                        sT += b.ToString("X2");
                                    }
                                    HandleEventInNewThread(RFIDEventType.RMU_SingleWriteData, sT);
                                    Debug.WriteLine(string.Format("RMU_SingleWriteData -> {0} {1} UII = {2}"
                                        , DateTime.Now.ToLongTimeString(),
                                        DateTime.Now.Millisecond.ToString()
                                        , sT));
                                }
                                else
                                {
                                    HandleEventInNewThread(RFIDEventType.RMU_SingleWriteData);
                                }

                            }
                            break;
                        case (int)RMU_CommandType.RMU_WriteData:
                            {
                                EditEventStatusValue(RFIDEventType.RMU_WriteData, true);
                                StopSeeing(RFIDEventType.RMU_WriteData);
                                if (bytesCmd[3] == 0)
                                {
                                    string sT = "OK";

                                    HandleEventInNewThread(RFIDEventType.RMU_WriteData, sT);
                                    Debug.WriteLine(string.Format("RMU_WriteData -> {0} {1} UII = {2}"
                                        , DateTime.Now.ToLongTimeString(),
                                        DateTime.Now.Millisecond.ToString()
                                        , sT));
                                }
                                else
                                {
                                    HandleEventInNewThread(RFIDEventType.RMU_WriteData);
                                }
                            }
                            break;
                        case 0x17:
                            if (bytesCmd[3] == 0)
                            {
                                //strinfo = "销毁标签成功";
                            }
                            else
                            {
                                //strinfo = "销毁标签失败";
                            }
                            break;
                        default:
                            return;
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Parse Exception -> {0}",ex.Message));
            }

        }
        #region Static Methods
        public static string GetEPCFormUII(string uii)
        {
            if (uii == null || uii.Length < 4)
            {
                return null;
            }
            return uii.Substring(4);
        }
        /// <summary>
        /// 对epc进行格式化，如果编码本身不足12位，则以F和0补足
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetFormatEPC(string id)
        {
            string epc = id;
            epc += "F00000000000";
            epc = epc.Substring(0, 12);
            return epc;
        }
        public static string GetIDFromEPC(string epc)
        {
            string strID = epc;
            int nFindex = epc.IndexOf("F");
            if (nFindex != -1)
            {
                strID = epc.Substring(4, nFindex - 4);
            }
            return strID;
        }
        public static string PwdCheck(string pwd)
        {
            string strR = null;
            if (null == pwd)
            {
                strR = "00000000";
            }
            else
            {
                if (Regex.IsMatch(pwd, "[0-9]{1,8}"))
                {
                    pwd += "00000000";
                    strR = pwd;
                }
                else
                {
                    strR = "00000000";
                }
            }

            return strR.Substring(0, 8);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandtype"></param>
        /// <param name="strPwd_in">4位</param>
        /// <param name="strBank_in"></param>
        /// <param name="nAddress"></param>
        /// <param name="strUii"></param>
        /// <returns></returns>
        public static string RmuReadDataCommandComposer(RMU_CommandType commandtype
                                                       , string strPwd_in,
                                                        int Bank_in,
                                                        int nAddress,
                                                        int nCount,
                                                        string strUii)
        {
            string commandR = null;
            if (!Enum.IsDefined(typeof(RMU_CommandType), commandtype.ToString()))
            {
                return commandR;
            }
            if (Bank_in < 0 || nAddress < 0 || nCount <= 0)
            {
                return commandR;
            }
            strPwd_in = RFIDHelper.PwdCheck(strPwd_in);
            switch ((int)commandtype)
            {
                case (int)RMU_CommandType.RMU_SingleReadData:

                    commandR = "aa0920" + strPwd_in + Bank_in.ToString("X2") + nAddress.ToString("X2") + nCount.ToString("X2") + "55";
                    break;
                case (int)RMU_CommandType.RMU_ReadData:
                    if (null == strUii || !Regex.IsMatch(strUii, "[0-9a-zA-Z]{14}"))
                    {
                        return commandR;
                    }
                    int nLength = 1 + 1 + 4 + 1 + 1 + 1 + 14;
                    commandR = "aa" + nLength.ToString("X2") + "13" + strPwd_in + Bank_in.ToString("X2") + nAddress.ToString("X2") + nCount.ToString("X2") + strUii + "55";
                    break;
            }
            return commandR;
        }
        public static string passwordFormat(string pwd)
        {
            string strDefault = "00000000";
            if (null == pwd)
            {
                return strDefault;
            }
            string strCommand = null;
            byte[] byteA = Encoding.ASCII.GetBytes(pwd);
            for (int i = 0; i < byteA.Length; i++)
            {
                strCommand += byteA[i].ToString("X2");
            }
            strCommand += strDefault;
            return strCommand.Substring(0, 8);
        }
        public static string RmuLockCommandComposer(RMU_CommandType type, string pwd, string lockData, string uii)
        {
            string strCommand = "";
            if (type != RMU_CommandType.RMU_LockMem
                || lockData == null
                || uii == null)
            {
                return null;
            }
            pwd = RFIDHelper.PwdCheck(pwd);
            int nLen = 1 + 1 + 4 + 3 + 14;
            strCommand = "aa" + nLen.ToString("X2") + "16" + pwd + lockData + uii + "55";
            return strCommand;
        }
        #endregion

        /// <summary>
        /// 锁定保留内存、epc存储器和tid存储器
        /// </summary>
        public void RmuLockTagReserverdEpcTid(string pwd,string uii)
        {
            if (null == pwd || uii == null)
            {
                return;
            }
            string lockCommand = null;
            lockCommand = RFIDHelper.RmuLockCommandComposer(RMU_CommandType.RMU_LockMem, 
                                                            pwd, "0ffeac", uii);
            if (null != lockCommand)
            {
                SendCommand(lockCommand,RFIDEventType.RMU_LockMem);
            }

        }
        /// <summary>
        /// 将传入的参数组合成命令字符串
        /// </summary>
        /// <param name="strRmuCmd_in"></param>
        /// <param name="strPwd_in"></param>
        /// <param name="strBank_in"></param>
        /// <param name="nAddress"></param>
        /// <param name="strData_in"></param>
        /// <param name="strUii"></param>
        /// <returns>返回的命令可能有0或者多个，调用者应对返回值做检查</returns>
        public static List<string> RmuWriteDataCommandCompose(RMU_CommandType commandType
                                                        , string strPwd_in
                                                        , int Bank_in
                                                        , int nAddress
                                                        , string strData_in
                                                        , string strUii)
        {
            List<string> commandList = new List<string>();
            if (!Enum.IsDefined(typeof(RMU_CommandType), commandType.ToString()))
            {
                return commandList;
            }
            if (Bank_in < 0 || nAddress < 0)
            {
                return commandList;
            }

            strPwd_in = RFIDHelper.PwdCheck(strPwd_in);

            for (int i = 0; i < strData_in.Length; i += 4, nAddress++)
            {
                string strAddress_in = nAddress.ToString("X2");
                string strData2Send = strData_in.Substring(i, 4);
                string strCmd = "aa";

                int nLength = 0;// 命令长度，以byte计算
                nLength = 1 + 1 + 4 + 1 + 1 + 1 + 2;// length +cmd +pwd + bank + address + cnt + data len
                switch ((int)commandType)
                {
                case (int)RMU_CommandType.RMU_SingleWriteData:
                        strCmd += "21";
                	break;
                    case (int)RMU_CommandType.RMU_WriteData:
                    {
                        if (null == strUii)
                        {
                            return commandList;
                        }
                        nLength += 14;// strUii.Length / 2;// 加上uii的长度
                        strCmd += "14";
                    }
                    break;
                }
                strCmd = strCmd.Insert(2, nLength.ToString("X2"));
                //strCmd += nLength.ToString("X2");
                //strCmd += commandType;

                strCmd += strPwd_in;
                strCmd += Bank_in.ToString("X2");
                strCmd += strAddress_in;
                strCmd += "01";//目前硬件只支持 1
                strCmd += strData2Send;
                if (commandType == RMU_CommandType.RMU_WriteData)//指定UII
                {
                    strCmd += strUii;
                }
                strCmd += "55";
                commandList.Add(strCmd);
            }
            return commandList;
        }
        
        //public void WriteData(List<string> value)
        //{
        //    if (value == null || value.Count <= 0)
        //    {
        //        return;
        //    }
        //    for (int i = 0; i < value.Count; i++)
        //    {
        //        WriteData(value[i]);
        //        Thread.Sleep(200);
        //    }
        //}
        //void WriteData(string value)
        //{
        //    string str1, str2;
        //    str1 = "";
        //    for (int i = 2; i < value.Length - 2; i += 2)
        //    {
        //        str2 = value.Substring(i, 2);
        //        if (str2 == "FF" || str2 == "AA" || str2 == "55")
        //            str2 = "FF" + str2;
        //        str1 += str2;

        //    }
        //    value = "AA" + str1 + "55";
            
        //    Debug.WriteLine(string.Format("WriteData -> {0}", value));

        //    MatchCollection mc = Regex.Matches(value, @"(?i)[\da-f]{2}");
        //    List<byte> buf = new List<byte>();//填充到这个临时列表中


        //    //依次添加到列表中
        //    foreach (Match m in mc)
        //    {
        //        buf.Add(Byte.Parse(m.ToString(), System.Globalization.NumberStyles.HexNumber));
        //    }

        //    RaiseEvent(RFIDEventType.WriteToSerialPort, buf.ToArray());
        //}
        #region Raise Events
        public void RaiseException(object o)
        {
            RaiseEvent(RFIDEventType.RMU_Exception, o);
        }
        void RaiseEvent(RFIDEventType eventType)
        {
            if (null != evtCardState)
            {
                evtCardState(eventType, null);
            }
        }
        void RaiseEvent(RFIDEventType eventType, object o)
        {
            if (eventType == RFIDEventType.WriteToSerialPort)
            {
                if (null != evtWriteToSerialPort)
                {
                    evtWriteToSerialPort((byte[])o);
                }
                return;
            }
            if (bStopCallback)
            {
                return;
            }

            if (null != evtCardState)
            {
                if (this.bRaiseEvent)
                {
                    evtCardState(eventType, o);
                }else
                if (eventType == RFIDEventType.RMU_Exception)
                {
                    bRaiseEvent = true;
                    evtCardState(eventType, o);
                }
            }
        }

        #endregion


    }
}
