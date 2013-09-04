using System;
using System.Collections.Generic;
using System.Text;

namespace LogisTechBase.rfidCheck
{
    public class Person
    {
      public string id_num;
      public string name;
      public string telephone;
      public string email;
      public string bj;
      public string nj;
      public string epc;

      public Person(string _id,string _name,string _tel,string _email,string _bj,string _nj,string _epc)
      {
          this.id_num = _id;
          this.name = _name;
          this.telephone = _tel;
          this.email = _email;
          this.bj = _bj;
          this.nj = _nj;
          this.epc = _epc;
      }
      public Person(string _id)
      {
          this.id_num = _id;
      }

    }
}
