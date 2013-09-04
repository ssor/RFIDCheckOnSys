using System;
using System.Collections.Generic;
using System.Text;

namespace LogisTechBase.rfidCheck
{
    /// <summary>
    /// 考勤记录
    /// </summary>
    public class CheckRecord
    {
        public string id;
        //public string name;
        public string checkDate;

        public CheckRecord(string _id, string _checkDate)
        {
            this.id = _id;
            //this.name = _name;
            this.checkDate = _checkDate;
        }
    }
}
