using System;
using System.Collections.Generic;
using System.Text;

namespace LogisTechBase.rfidCheck
{
    public class CheckPerson
    {
        public string checkDate;
        public Person person;
 
        public CheckPerson(string _strDateTime,string _id,string _name,string _tel,string _email,string _bj,string _nj,string epc)
        {
            this.checkDate = _strDateTime;
            this.person = new Person(_id, _name, _tel, _email,_bj,_nj,epc);
        }
        public CheckPerson(string _strDateTime,Person _person)
        {
            this.checkDate = _strDateTime;
            this.person = _person;
        }
    }
}
