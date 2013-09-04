using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Media;

namespace RfidReader
{
    public interface IParser
    {
        void Parse(byte[] value);
    }
    public interface IRFIDHelperSubscriber
    {
        void NewMessageArrived();
    }
    public class RFIDEventArg
    {
        public object _arg;
        public RFIDEventType _type;
        public RFIDEventArg(RFIDEventType type, object arg)
        {
            this._type = type;
            this._arg = arg;
        }
    }
    public enum RFIDEventType
    {
        RMU_Unknown = 0
      ,
        RMU_CardIsReady = 1
            ,
        RMU_GetPower
            ,
        RMU_SetPower
            ,
        RMU_GetFrequency
            ,
        RMU_SetFrequency
            ,
        RMU_Inventory
            ,
        RMU_InventoryAnti
            ,
        RMU_StopGet
            ,
        RMU_ReadData
            ,
        RMU_WriteData
            ,
        RMU_EraseData
            ,
        RMU_LockMem
            ,
        RMU_KillTag
            ,
        RMU_InventorySingle
            ,
        RMU_WeigandInvetory
            ,
        RMU_SingleReadData
            ,
        RMU_SingleWriteData//17
            ,
        RMU_getVersioin//18
            ,
        RMU_InventoryAnti_noTag,//当读取标签成功但是没有标签数据的时候使用
        RMU_Inventory_noTag,
        RMU_InventorySingle_noTag,
        RMU_LockMem_fail,
        RMU_SingleReadData_fail,
        RMU_WriteData_fail,

        RMU_SingleWriteData_fail,
        RMU_Exception


            , WriteToSerialPort


    }
    public enum RMU_CommandType
    {
        RMU_GetStatus = 0x00
        ,
        RMU_GetPower = 0x01
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
            ,
        RMU_SingleWriteData = 0x21

    }
    /// <summary>
    /// 该类负责解析rfid命令，并根据命令定义发起相应事件
    /// </summary>
    public class Rmu900RFIDHelper : IParser
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



        #region Members
        List<byte> maxbuf = new List<byte>();
        //当检查设备状态时，需要给设备一定的反应时间，如果超过该时间尚未返回数据，则表示设备尚未准备就绪
        IDataTransfer dataTransfer = null;
        IRFIDHelperSubscriber subscriber = null;
        List<RFIDEventArg> returned_commmand_list = new List<RFIDEventArg>();
        #endregion

        #region Public Methods
        public void Subscribe(IRFIDHelperSubscriber subscriber)
        {
            this.subscriber = subscriber;
        }

        public Rmu900RFIDHelper(IDataTransfer _dataTransfer)
        //public Rmu900RFIDHelper()
        {
            this.dataTransfer = _dataTransfer;

        }
        public string CheckWriteEpc()
        {
            foreach (RFIDEventArg a in this.returned_commmand_list)
            {
                if (a._type == RFIDEventType.RMU_SingleWriteData && ((string)a._arg) != string.Empty)
                {
                    return a._arg as string;
                }
            }
            return string.Empty;
        }
        public void StartWriteEpc(string epc)
        {
            List<string> commands = Rmu900RFIDHelper.RmuWriteDataCommandCompose(RMU_CommandType.RMU_SingleWriteData,
                                                                                null,
                                                                                1,
                                                                                2,
                                                                                epc,
                                                                                null);
            this.SendCommand(commands);
        }
        public void StopInventory()
        {
            this.SendCommand(RFIDCommand_RMU_StopGet);
        }
        public void StartInventory()
        {
            this.SendCommand(RFIDCommand_RMU_Inventory);
        }
        public string CheckInventory()
        {
            string r = string.Empty;
            foreach (RFIDEventArg a in this.returned_commmand_list)
            {
                if (a._type == RFIDEventType.RMU_Inventory && ((string)a._arg) != string.Empty)
                {
                    r = Rmu900RFIDHelper.GetEPCFormUII(a._arg as string);
                    break;
                }
            }
            this.returned_commmand_list.Clear();

            return r;
        }
        public void StartInventoryOnce()
        {
            this.SendCommand(RFIDCommand_RMU_InventorySingle);
        }
        public string ChekcInventoryOnce()
        {
            foreach (RFIDEventArg a in this.returned_commmand_list)
            {
                if (a._type == RFIDEventType.RMU_InventorySingle && ((string)a._arg) != string.Empty)
                {
                    return Rmu900RFIDHelper.GetEPCFormUII(a._arg as string);
                }
            }
            return string.Empty;

        }
        public void StartCheckRmuStatus()
        {
            this.SendCommand(Rmu900RFIDHelper.RFIDCommand_RMU_GetStatus);

        }
        public string CheckRmuStatus()
        {
            foreach (RFIDEventArg a in this.returned_commmand_list)
            {
                if (a._type == RFIDEventType.RMU_CardIsReady && ((string)a._arg) == "ok")
                {
                    return "ok";
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="type"></param>
        /// <param name="bReturnInstance">true，需要对每一个命令进行处理</param>
        public void SendCommand(List<string> commands)
        {
            if (commands == null || commands.Count <= 0)
            {
                return;
            }
            string strTmp = string.Empty;
            for (int i = 0; i < commands.Count; i++)
            {
                SendCommand(commands[i]);
                Thread.Sleep(400);
            }
        }
        public void SendCommand(List<string> commands, int interval)
        {
            if (commands == null || commands.Count <= 0)
            {
                return;
            }
            string strTmp = string.Empty;
            for (int i = 0; i < commands.Count; i++)
            {
                SendCommand(commands[i]);
                Thread.Sleep(interval);
            }
        }

        public void SendCommand(string command)
        {
            if (null == command)
            {
                return;
            }
            Debug.WriteLine(string.Format("SendCommand -> {0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond.ToString()));

            this.returned_commmand_list.Clear();
            this.WriteData(command);
        }

        /// <summary>
        /// 锁定保留内存、epc存储器和tid存储器
        /// </summary>
        public void RmuLockTagReserverdEpcTid(string pwd, string uii)
        {
            if (null == pwd || uii == null)
            {
                return;
            }
            string lockCommand = null;
            lockCommand = Rmu900RFIDHelper.RmuLockCommandComposer(RMU_CommandType.RMU_LockMem,
                                                            pwd, "0ffeac", uii);
            if (null != lockCommand)
            {
                //SendCommand(lockCommand, RFIDEventType.RMU_LockMem);
            }

        }
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
                    , BytesToHexString(maxbuf.ToArray())));
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

                    //dicCommandSuccess[(int)RFIDEventType.RMU_CardIsReady] = true;//如果有其它的命令返回，那该命令当然已经成功了
                    //*********************************************************************************
                    switch (bytesCmd[2])//第3位表示的是命令
                    {
                        case 0:
                            //EditEventStatusValue(RFIDEventType.RMU_CardIsReady, true);
                            //RMUStatus.Set();
                            //StopSeeing(RFIDEventType.RMU_CardIsReady);
                            HandleEventInNewThread(RFIDEventType.RMU_CardIsReady, "ok");
                            break;
                        case 0x7:
                            //EditEventStatusValue(RFIDEventType.RMU_getVersioin, true);
                            //RMUStatus.Set();
                            //StopSeeing(RFIDEventType.RMU_getVersioin);
                            string strVersion = null;
                            if (bytesCmd.Count >= 11)
                            {
                                strVersion = bytesCmd[11].ToString();
                            }

                            HandleEventInNewThread(RFIDEventType.RMU_getVersioin, strVersion);
                            break;
                        case 0x10:
                            //EditEventStatusValue(RFIDEventType.RMU_Inventory, true);
                            //StopSeeing(RFIDEventType.RMU_Inventory);
                            if (bytesCmd[3] == 1)
                            {
                                //_bExistCardAround = false;
                                HandleEventInNewThread(RFIDEventType.RMU_Inventory_noTag);
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
                            //EditEventStatusValue(RFIDEventType.RMU_InventoryAnti, true);

                            //StopSeeing(RFIDEventType.RMU_InventoryAnti);
                            if (bytesCmd[3] == 1)
                            {
                                HandleEventInNewThread(RFIDEventType.RMU_InventoryAnti_noTag, "ok");
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
                            //_bExistCardAround = false;
                            Debug.WriteLine(string.Format("RMU_StopGet -> {0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond.ToString()));
                            HandleEventInNewThread(RFIDEventType.RMU_StopGet);
                            break;
                        case 0x13://指定UII读取数据
                            {
                                //EditEventStatusValue(RFIDEventType.RMU_ReadData, true);
                                //StopSeeing(RFIDEventType.RMU_ReadData);
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
                            //EditEventStatusValue(RFIDEventType.RMU_InventorySingle, true);

                            //StopSeeing(RFIDEventType.RMU_InventorySingle);
                            if (bytesCmd[3] > 0)
                            {
                                HandleEventInNewThread(RFIDEventType.RMU_InventorySingle_noTag);
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
                            //EditEventStatusValue(RFIDEventType.RMU_LockMem, true);

                            //StopSeeing(RFIDEventType.RMU_LockMem);
                            if (bytesCmd[3] > 0)
                            {
                                HandleEventInNewThread(RFIDEventType.RMU_LockMem_fail);
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
                            //EditEventStatusValue(RFIDEventType.RMU_SingleReadData, true);
                            //StopSeeing(RFIDEventType.RMU_SingleReadData);
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
                                HandleEventInNewThread(RFIDEventType.RMU_SingleReadData_fail);
                                Debug.WriteLine(string.Format("RMU_SingleReadData -> {0} {1}  Failed"
                                              , DateTime.Now.ToLongTimeString(),
                                                DateTime.Now.Millisecond.ToString()));
                            }
                            break;
                        case 0x21://不指定UII写入标签数据
                            {
                                //EditEventStatusValue(RFIDEventType.RMU_SingleWriteData, true);
                                //StopSeeing(RFIDEventType.RMU_SingleWriteData);
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
                                    HandleEventInNewThread(RFIDEventType.RMU_SingleWriteData_fail);
                                }

                            }
                            break;
                        case (int)RMU_CommandType.RMU_WriteData:
                            {
                                //EditEventStatusValue(RFIDEventType.RMU_WriteData, true);
                                //StopSeeing(RFIDEventType.RMU_WriteData);
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
                                    HandleEventInNewThread(RFIDEventType.RMU_WriteData_fail);
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
                Debug.WriteLine(string.Format("Parse Exception -> {0}", ex.Message));
            }

        }
        #endregion

        #region private method
        void WriteData(string value)
        {
            value = value.Replace(" ", "");
            string str1, str2;
            str1 = "";
            //循环每两位检查，发现 ff 或者 aa 或者 55 ，则前面加 ff 作为转义标记
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
            if (this.dataTransfer != null)
            {
                dataTransfer.writeData(buf.ToArray());
            }
        }

        void BackgroundThreadWork(object sender, DoWorkEventArgs e)
        {
            RFIDEventArg rfidArg = (RFIDEventArg)e.Argument;
            this.returned_commmand_list.Add(rfidArg);
            if (this.subscriber != null)
            {
                subscriber.NewMessageArrived();
            }
            //RaiseEvent(rfidArg._type, rfidArg._arg);
        }
        void HandleEventInNewThread(RFIDEventType type, object arg)
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
            strPwd_in = Rmu900RFIDHelper.PwdCheck(strPwd_in);
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
            pwd = Rmu900RFIDHelper.PwdCheck(pwd);
            int nLen = 1 + 1 + 4 + 3 + 14;
            strCommand = "aa" + nLen.ToString("X2") + "16" + pwd + lockData + uii + "55";
            return strCommand;
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

            strPwd_in = Rmu900RFIDHelper.PwdCheck(strPwd_in);

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

                //string str1, str2;
                //str1 = "";
                ////循环每两位检查，发现 ff 或者 aa 或者 55 ，则前面加 ff 作为转义标记
                //for (int j = 2; j < strCmd.Length - 2; j += 2)
                //{
                //    str2 = strCmd.Substring(j, 2);
                //    if (str2 == "FF" || str2 == "AA" || str2 == "55")
                //        str2 = "FF" + str2;
                //    str1 += str2;

                //}
                //strCmd = "AA" + str1 + "55";

                commandList.Add(strCmd);
            }
            return commandList;
        }

        #endregion


    }
}
