using System;
using System.Collections.Generic;
using System.Text;

namespace AsynchronousSocket
{
    public class AsynSocketProcessMsg
    {
        public int nCode;
        public string strMsg;
        public AsynSocketProcessMsg(int nCode, string msg)
        {
            this.nCode = nCode;
            this.strMsg = msg;
        }
    }
}
