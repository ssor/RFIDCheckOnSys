using System;
using System.Collections.Generic;
using System.Text;

namespace AsynchronousSocket
{
    public enum enumAsynSocketPocessCode : int
    {
        SocketException = 1
        ,
        SocketNormalOutPut 
            ,
        SocketNormalBeginReceive
        ,
        SocketNormalEndReceive
            ,
        SocketNormalBeginSend
            , SocketNormalEndSend
    }
}
