using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace LogisTechBase
{

    public class RFID_EPCWriter
    {
        string _initialPwd = "00000000";
        string _strEPC = null;
        string _strPwd = null;
        string _uii = null;//保存读取到的标签的uii
        //区分操作类型，可能是初始化标签，也可能是修改标签
        // 0,initial;1,edit epc
        int OperationType = 0;
        RFIDHelper _helper;
        public RFID_EPCWriter(RFIDHelper helper)
        {
            _helper = helper;
            //_helper.evtCardState += new deleVoid_RFIDEventType_Object_Func(_helper_evtCardState);
        }
        void WriterClear(string value)
        {
            _helper.evtCardState -= _helper_evtCardState;
            //_helper.RaiseException(value);
            MessageBox.Show(value);
        }

        void _helper_evtCardState(RFIDEventType eventType, object o)
        {
            switch ((int)eventType)
            {
                case (int)RFIDEventType.RMU_Exception:
                    WriterClear("写入数据失败");
                    break;
                case (int)RFIDEventType.RMU_CardIsReady:
                    //设备准备就绪，查找周围是否有标签存在 
                    _helper.SendCommand(RFIDHelper.RFIDCommand_RMU_InventoryAnti3, RFIDEventType.RMU_InventoryAnti);
                    break;
                case (int)RFIDEventType.RMU_InventoryAnti:
                    if (null == o)
                    {
                        WriterClear("请将标签防止读卡器作用范围内！");
                    }
                    else
                        if ((string)o != "ok")
                        {
                            Debug.WriteLine(string.Format("_helper_evtCardState ->RMU_InventoryAnti {0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond.ToString()));
                            _uii = (string)o;
                            _helper.SendCommand(RFIDHelper.RFIDCommand_RMU_StopGet, RFIDEventType.RMU_StopGet);
                        }
                    break;
                case (int)RFIDEventType.RMU_StopGet:
                    {
                        Debug.WriteLine(string.Format("_helper_evtCardState ->RMU_StopGet {0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond.ToString()));
                        //首先进行锁定
                        if (OperationType == 0)
                        {
                            _helper.RmuLockTagReserverdEpcTid(_initialPwd, _uii);
                        }
                        else
                            if (OperationType == 1)
                            {
                                _helper.RmuLockTagReserverdEpcTid(_strPwd, _uii);
                            }
                    }
                    break;
                case (int)RFIDEventType.RMU_LockMem:
                    {
                        if (null == o)
                        {
                            WriterClear("标签锁定失败 ");
                        }
                        else if ((string)o == "ok")
                        {
                            Debug.WriteLine(string.Format("_helper_evtCardState ->RMU_LockMem {0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond.ToString()));

                            List<string> commands = null;
                            if (OperationType == 0)// initial tag
                            {
                                //周围有标签存在，可以写入数据 
                                commands = RFIDHelper.RmuWriteDataCommandCompose(RMU_CommandType.RMU_SingleWriteData, _initialPwd, 1, 2, GetWriteCommand(), null);
                                string strPwdT = RFIDHelper.PwdCheck(_strPwd);
                                string pwdTH4 = strPwdT.Substring(0,4);//前四位 
                                string pwdTT4 = strPwdT.Substring(4,4);//后四位 
                                List<string> commandSetSecret1 = RFIDHelper.RmuWriteDataCommandCompose(RMU_CommandType.RMU_SingleWriteData, _initialPwd, 0, 2, pwdTH4, null);
                                List<string> commandSetSecret2 = RFIDHelper.RmuWriteDataCommandCompose(RMU_CommandType.RMU_SingleWriteData, pwdTH4 + _initialPwd, 0, 3, pwdTT4, null);
                                commands.AddRange(commandSetSecret1);
                                commands.AddRange(commandSetSecret2);
                            }
                            if (OperationType == 1)
                            {
                                commands = RFIDHelper.RmuWriteDataCommandCompose(RMU_CommandType.RMU_SingleWriteData, RFIDHelper.PwdCheck(_strPwd), 1, 2, GetWriteCommand(), null);
                            }
                            _helper.SendCommand(commands, RFIDEventType.RMU_SingleWriteData, false);
                        }
                    }
                    break;
                case (int)RFIDEventType.RMU_SingleWriteData:
                    if (null != o)
                    {
                        WriterClear("标签成功 ");
                    }
                    else
                    {
                        WriterClear("标签写入数据失败，请检查设备并将标签放在读卡器范围内！");
                    }
                    break;
            }
        }
        /// <summary>
        /// 检查要写入标签的epc，如果不足12位，以00补足
        /// </summary>
        /// <returns></returns>
        string GetWriteCommand()
        {
            string strR = null;
            strR += _strEPC;
            for (int i = 1; i <= 12; i++)
            {
                strR += "00";
            }
            strR = strR.Substring(0, 24);
            return strR;
        }
        /// <summary>
        /// 重设tag的EPC
        /// </summary>
        /// <param name="strEpc"></param>
        /// <param name="strPwd"></param>
        void WriteEPC(string strEpc, string strPwd)
        {
            if (null == _helper)
            {
                MessageBox.Show("初始化错误！");
                return;
            }
            _strPwd = strPwd;
            _strEPC = strEpc;
            _helper.evtCardState += new deleVoid_RFIDEventType_Object_Func(_helper_evtCardState);
            _helper.SendCommand(RFIDHelper.RFIDCommand_RMU_GetStatus, RFIDEventType.RMU_CardIsReady);

        }
        public void InitialTag(string strEpc,string newPwd)
        {
            OperationType = 0;
            WriteEPC(strEpc, newPwd);
            //_strPwd = newPwd;
            //_strEPC = strEpc;
            //_helper.evtCardState += new deleVoid_RFIDEventType_Object_Func(_helper_evtCardState);
            //_helper.SendCommand(RFIDHelper.RFIDCommand_RMU_GetStatus, RFIDEventType.RMU_CardIsReady);
        }
        public void EditTagEpc(string strEpc,string pwd)
        {
            OperationType = 1;
            WriteEPC(strEpc, pwd);
        }
        //public void WriteEPC(string strEpc)
        //{
        //    WriteEPC(strEpc, null);

        //}
    }
}
