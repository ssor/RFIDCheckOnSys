using System;
using System.Collections.Generic;
using System.Windows.Forms;
using nsConfigDB;

namespace rfidCheck
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            initialSystem();

            //Application.Run(new frmStudent());//学生端
            Application.Run(new Form1());//教师端
        }

        static void initialSystem()
        {
            object oPortName = ConfigDB.getConfig("comportName");
            if (oPortName != null)
            {
                sysConfig.comportName = (string)oPortName;
            }
            object oBautrate = ConfigDB.getConfig("baudRate");
            if (oBautrate != null)
            {
                sysConfig.baudRate = (string)oBautrate;
            }
            object oTcpPort = ConfigDB.getConfig("tcp_port");
            if (oTcpPort != null)
            {
                sysConfig.tcp_port = int.Parse((string)oTcpPort);
            }
        }
    }
}
