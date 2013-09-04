using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

namespace nsConfigDB
{
    /// <summary>
    /// 该类转为保存一些常用的设置
    /// 这个类要做到以下几点：
    /// 1 无需配置即可使用
    /// 2 根据配置名保存和读取配置
    /// </summary>
    public class ConfigDB
    {

        static bool bInitialled = false;
        static string configFile = "config.xml";
        static DataSet ds = null;
        static bool initialDB()
        {
            if (bInitialled == true)
            {
                return true;
            }
            else
            {
                try
                {
                    ds = new DataSet("nsConfig");
                    ds.Namespace = "";
                    if (!File.Exists(configFile))
                    {

                        ds.WriteXml(configFile);
                    }
                    else
                    {
                        ds.ReadXml(configFile);
                    
                    }
                    if (ds.Tables.IndexOf("config") == -1)
                    {
                        DataTable dt = new DataTable("config");
                        dt.Columns.Add("key", typeof(string));
                        dt.Columns.Add("value", typeof(object));
                        ds.Tables.Add(dt);
                    }
                    bInitialled = true;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        public static bool saveConfig(string _key, object _value)
        {
            bool bR = true;
            if (initialDB())
            {
                DataTable dt = ds.Tables["config"];
                DataRow[] rows = dt.Select("key = '" + _key + "'");
                if (rows.Length > 0)
                {
                    rows[0]["value"] = _value;
                }
                else
                {
                    dt.Rows.Add(new object[] { _key, _value });
                }
                ds.WriteXml(configFile);
            }
            return bR;
        }
        public static object getConfig(string _key)
        {
            object oR = null;
            if (initialDB())
            {
                DataTable dt = ds.Tables["config"];
                DataRow[] rows = dt.Select("key = '" + _key + "'");
                if (rows.Length > 0)
                {
                    oR = rows[0]["value"];
                }
            }
            return oR;
        }
    }
}
