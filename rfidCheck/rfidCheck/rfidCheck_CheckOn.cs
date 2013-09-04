using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LogisTechBase.rfidCheck
{
    public enum CheckProtocol
    {
        Success = 1
        , Failed
    }

    public class rfidCheck_CheckOn
    {
        #region Static Memebers
        static string dbFile = "personCheck.db3";
        static bool bDBInitialized = false;
        public static string dbPath = "Data Source=personCheck.db3";
        static string tableSerialPortConfigsExist =
            @"SELECT count(*) FROM sqlite_master where type = 'table' and tbl_name = 'tbSerialPortConfigs'";
        static string tableSerialPortConfigsCreate =
            @"CREATE TABLE tbSerialPortConfigs(name varchar(20) primary key
                                    ,PortName varchar(50)
                                    ,BaudRate varchar(50)
                                    ,Parity varchar(50)
                                    ,StopBits varchar(50)
                                    ,DataBits varchar(50)
                                     );";
        static string tableConfigExist =
            @"SELECT count(*) FROM sqlite_master where type = 'table' and tbl_name = 'tbConfig'";
        static string tableConfigCreate =
            @"CREATE TABLE tbConfig(key varchar(20) primary key
                                    ,value varchar(100) );";
        static string tablePersonExist =
            "SELECT count(*) FROM sqlite_master where type = \"table\" and tbl_name = \"person\"";
        static string tablePersonCreate =
            @"CREATE TABLE person(xh varchar(20) primary key
                    ,xm varchar(30)
                    ,nj char(4)
                    ,bj char(10)
                    ,tel varchar(20)
                    ,email varchar(100)
                    ,uniqueID varchar(30) unique);";
        static string SqlInsertPerson =
            "insert into person(xh,xm,tel,email,nj,bj) values(@xh,@xm,@tel,@email,@nj,@bj);";
        static string SqlSelectSpecialPerson =
            @"select  p.xh as 学号,p.xm as 姓名 ,p.nj as 年级
            ,p.bj as 班级,p.tel as 电话,p.email as 邮箱 FROM person as p where xh = @xh;";
        static string SqlSelectAllPerson =
            @"select  p.xh as 学号,p.xm as 姓名 ,p.nj as 年级,p.bj as 班级
            ,p.tel as 电话,p.email as 邮箱,p.uniqueID as 考勤号
             FROM person as p;";
        static string SqlCheckSpecialPersonExist =
            "SELECT count(*) FROM person where xh = @xh;";
        static string SqlUpdatePerson =
            "update person set xm = @xm,tel=@tel,email= @email,nj=@nj,bj=@bj where xh= @xh;";
        static string SqlDeletePerson =
            "delete from person where xh= @xh;";
        static string SqlUpdatePersonEPC =
            @"update person set uniqueID = @uniqueID where xh= @xh;";
        static string SqlDeletePersonEPC =
            @"update person set uniqueID = "" where xh= @xh;";
        static string SqlCheckEPCUsed =
            @"select count(xh) from person where uniqueID = @uniqueID";
        static string SqlGetnj =
            @"select distinct nj from person";
        static string SqlGetbj =
            @"select distinct bj from person";

        static string tableCheckRecordExist =
            "SELECT count(*) FROM sqlite_master where type = \"table\" and tbl_name = \"tbCheckRecords\"";
        static string TableCheckRecordCreate =
            @"CREATE TABLE tbCheckRecords(uniqueID varchar(30)
                                        ,checkDate varchar(10) not null
                                        , primary key(uniqueID,checkDate));";
        static string SqlInsertCheckRecord =
            "insert into tbCheckRecords(uniqueID,checkDate) values(@uniqueID,@checkDate);";
        static string SqlSelectAllRecords =
            "SELECT * FROM tbCheckRecords";

        static string SqlSelectCheckedPersonInPeriod =
            @"SELECT newT.xh as 学号,newT.xm as 姓名,newT.nj as 年级,newT.bj as 班级 
            ,newT.tel as 电话,newT.email as 邮箱,newT.checkDate as 考勤日期
             FROM (
             SELECT  p.xh,p.xm,p.nj,p.bj,p.tel,p.email,r.checkDate 
             FROM
             person as p join tbCheckRecords as r on r.uniqueID=p.uniqueID
             where r.checkDate between @startDate and @endDate)
             as newT GROUP BY xh ";
        static string SqlSelectCheckedPersonWithPara_head =
            @" SELECT newT.xh as 学号,newT.xm as 姓名,newT.nj as 年级,newT.bj as 班级 
            ,newT.checkDate as 考勤日期,newT.tel as 电话,newT.email as 邮箱
             FROM (
             SELECT  p.xh,p.xm,p.nj,p.bj,p.tel,p.email,r.checkDate 
             FROM
             person as p join tbCheckRecords as r on r.uniqueID=p.uniqueID
             where r.checkDate between @startDate and @endDate ";
        //        static string SqlSelectCheckedPersonWithPara_head =
        //    @" SELECT newT.xh as 学号,newT.xm as 姓名,newT.nj as 年级,newT.bj as 班级 
        //            ,newT.tel as 电话,newT.email as 邮箱,newT.checkDate as 考勤日期
        //             FROM (
        //             SELECT  p.xh,p.xm,p.nj,p.bj,p.tel,p.email,r.checkDate 
        //             FROM
        //             person as p join tbCheckRecords as r on r.uniqueID=p.uniqueID
        //             where r.checkDate between @startDate and @endDate ";
        static string SqlSelectCheckedPersonWithPara_nj =
            @" and p.nj = @nj ";
        static string SqlSelectCheckedPersonWithPara_bj =
            @"  and p.bj = @bj";
        static string SqlSelectCheckedPersonWithPara_tail =
            @" ) as newT GROUP BY xh";


        static string SqlSelectUnCheckedPersonInPeriod =
            @"select  p.xh as 学号,p.xm as 姓名 ,p.nj as 年级
            ,p.bj as 班级,p.tel as 电话,p.email as 邮箱 FROM person as p
             where p.uniqueID not in(
             SELECT distinct r.uniqueID FROM tbCheckRecords as r 
             where r.checkDate between @startDate and @endDate )";

        static string FileCheckName = "FilecheckR.xml";
        static string FilePersonName = "person.xml";

        static string regexString = @"2\d{3}[-/](1[0-2]|0?[1-9])[-/](3[0-1]|[0-2]?\d)";
        #endregion
        public static string GetFormatDateTimeString(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public static string GetDateSubString(string strDateTime)
        {
            string strDate = null;
            if (null == strDateTime)
            {
                return strDate;
            }
            if (Regex.IsMatch(strDateTime, regexString))
            {
                strDate = Regex.Match(strDateTime, regexString).ToString();
            }
            return strDate;
        }
        public static bool InitialDB()
        {
            if (!bDBInitialized)
            {
                InitialCheckDB();
                InitialPersonDB();
                InitialConfigDB();


                bDBInitialized = true;
            }
            return true;
        }
        #region Person Management

        public static void AddStudentTableToPersonDS(ref DataSet ds)
        {

            DataTable table = new DataTable("student");
            DataColumn idColumn = new DataColumn("id_no");
            DataColumn nameColumn = new DataColumn("name");
            DataColumn telColumn = new DataColumn("tel");
            DataColumn mailColumn = new DataColumn("mail");
            table.Columns.Add(idColumn);
            table.Columns.Add(nameColumn);
            table.Columns.Add(telColumn);
            table.Columns.Add(mailColumn);
            ds.Tables.Add(table);
        }
        public static void InitialSerialPortConfigsDB()
        {
            try
            {
                //int result = 0;
                //result = int.Parse(SQLiteHelper.ExecuteScalar(dbPath, tableSerialPortConfigsExist, null).ToString());
                //if (!(result >= 1))
                //{
                //    result = int.Parse(SQLiteHelper.ExecuteNonQuery(dbPath, tableSerialPortConfigsCreate, null).ToString());
                //    if (result == 0)
                //    {
                //        //MessageBox.Show("Create table success");
                //    }
                //    else
                //    {
                //        MessageBox.Show("初始化数据库时出现错误！");
                //        return;
                //    }
                //}
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("初始化数据库时出现错误！" + ex.Message);
            }
        }
        public static void InitialConfigDB()
        {
            try
            {
                int result = 0;
                //result = int.Parse(SQLiteHelper.ExecuteScalar(dbPath, tableConfigExist, null).ToString());
                if (!(result >= 1))
                {
                    //result = int.Parse(SQLiteHelper.ExecuteNonQuery(dbPath, tableConfigCreate, null).ToString());
                    if (result == 0)
                    {
                        //MessageBox.Show("Create table success");
                    }
                    else
                    {
                        MessageBox.Show("初始化数据库时出现错误！");
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("初始化数据库时出现错误！" + ex.Message);
            }
        }
        public static void InitialPersonDB()
        {
            try
            {
                int result = 0;
                //result = int.Parse(SQLiteHelper.ExecuteScalar(dbPath, tablePersonExist, null).ToString());
                if (!(result >= 1))
                {
                    //result = int.Parse(SQLiteHelper.ExecuteNonQuery(dbPath, tablePersonCreate, null).ToString());
                    if (result == 0)
                    {
                        //MessageBox.Show("Create table success");
                    }
                    else
                    {
                        MessageBox.Show("初始化学生数据库时出现错误！");
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("初始化学生数据库时出现错误！" + ex.Message);
            }
        }
        public static DataSet GetPersonDataSet()
        {
            DataSet myDataSetPerson = null;
            if (InitialDB())
            {
                try
                {
                    if (!File.Exists(FilePersonName))
                    {
                        rfidCheck_CheckOn.InitialPersonDB();
                    }
                    myDataSetPerson = SQLiteHelper.ExecuteDataSet(dbPath, SqlSelectAllPerson, null);

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("读取学生数据库时出现错误！" + ex.Message);
                    return null;
                }
            }

            return myDataSetPerson;
        }
        public static bool PersonExist(Person p)
        {
            bool bR = false;
            if (InitialDB())
            {
                int result = 0;
                result = int.Parse(
                    SQLiteHelper.ExecuteScalar(dbPath, SqlCheckSpecialPersonExist, new object[1] { p.id_num }).ToString());
                if (result >= 1)
                {
                    bR = true;
                }
            }
            /*
            List<Person> lp = rfidCheck_CheckOn.GetPersonList();
            if (lp != null)
            {

                for (int i = 0; i < lp.Count; i++)
                {
                    if (p.id_num == lp[i].id_num)
                    {
                        bR = true;
                        break;
                    }
                }
            }
             */
            return bR;
        }
        public static bool PersonDelete(Person p)
        {
            bool bR = false;
            try
            {
                if (InitialDB())
                {
                    int result = 0;
                    object[] pars = new object[1]
	                    {
	                        p.id_num
	                    };

                    result = int.Parse(SQLiteHelper.ExecuteNonQuery(dbPath, SqlDeletePerson, pars).ToString());
                    if (result >= 1)
                    {
                        bR = true;
                    }
                    else
                    {
                        bR = false;
                    }
                }
                /* 
                
                DataSet myDataSet = GetPersonDataSet();
                DataTable dt = myDataSet.Tables["student"];
                if (dt == null)
                {
                    rfidCheck_CheckOn.AddStudentTableToPersonDS(ref myDataSet);
                    dt = myDataSet.Tables["student"];
                }
                if (null != dt)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["id_no"].ToString() == p.id_num)
                        {
                            dt.Rows[i].Delete();
                            myDataSet.WriteXml(FilePersonName);
                            bR = true;
                            break;
                        }
                    }
                }
                
                */
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("删除记录出现错误：" + ex.Message);
            }
            return bR;
        }
        public static bool PersonUpdate(Person p)
        {
            bool bR = false;
            try
            {
                if (InitialDB())
                {
                    int result = 0;
                    object[] pars = new object[6]
	                    {
	                        p.name
	                        ,p.telephone
	                        ,p.email
                            ,p.nj
                            ,p.bj
	                        ,p.id_num
	                    };

                    result = int.Parse(SQLiteHelper.ExecuteNonQuery(dbPath, SqlUpdatePerson, pars).ToString());
                    if (result >= 1)
                    {
                        bR = true;
                    }
                    else
                    {
                        bR = false;
                    }
                }
                /* 
                
                DataSet myDataSet = GetPersonDataSet();
                DataTable dt = myDataSet.Tables["student"];
                if (dt == null)
                {
                    rfidCheck_CheckOn.AddStudentTableToPersonDS(ref myDataSet);
                    dt = myDataSet.Tables["student"];
                }
                if (null != dt)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["id_no"].ToString() == p.id_num)
                        {
                            dt.Rows[i]["name"] = p.name;
                            dt.Rows[i]["tel"] = p.telephone;
                            dt.Rows[i]["mail"] = p.email;
                            myDataSet.WriteXml(FilePersonName);
                            bR = true;
                            break;
                        }
                    }
                }
                
                */
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("更新记录出现错误：" + ex.Message);
            }
            return bR;
        }
        public static bool PersonAdd(Person p)
        {
            bool bR = false;
            try
            {
                if (InitialDB())
                {
                    int result = 0;
                    object[] pars = new object[6];
                    pars[0] = p.id_num;
                    pars[1] = p.name;
                    pars[2] = p.telephone;
                    pars[3] = p.email;
                    pars[4] = p.nj;
                    pars[5] = p.bj;
                    result = int.Parse(SQLiteHelper.ExecuteNonQuery(dbPath, SqlInsertPerson, pars).ToString());
                    if (result >= 1)
                    {
                        bR = true;
                    }
                    else
                    {
                        bR = false;
                    }
                }

                /*
                DataSet myDataSet = GetPersonDataSet();
                DataTable dt = myDataSet.Tables["student"];
                if (dt == null)
                {
                    rfidCheck_CheckOn.AddStudentTableToPersonDS(ref myDataSet);
                    dt = myDataSet.Tables["student"];
                }
                if (null != dt)
                {
                    DataRow myRow = dt.NewRow();
                    myRow["id_no"] = p.id_num;
                    myRow["name"] = p.name;
                    myRow["tel"] = p.telephone;
                    myRow["mail"] = p.email;
                    dt.Rows.Add(myRow);
                    myDataSet.WriteXml(FilePersonName);
                    bR = true;
                }
                 */
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("添加新记录出现错误：" + ex.Message);
            }
            return bR;
        }
        public static List<Person> GetPersonList()
        {
            List<Person> personList = new List<Person>();
            DataSet myDataSetPerson = rfidCheck_CheckOn.GetPersonDataSet();
            if (null == myDataSetPerson)
            {
                return null;
            }
            DataTable tbPerson = myDataSetPerson.Tables[0];
            /* 
            
            if (null == tbPerson)
            {
                rfidCheck_CheckOn.AddStudentTableToPersonDS(ref myDataSetPerson);
                tbPerson = myDataSetPerson.Tables["student"];
            }
            */
            if (tbPerson != null)
            {
                for (int i = 0; i < tbPerson.Rows.Count; i++)
                {
                    Person p = new Person(
                                          tbPerson.Rows[i]["学号"].ToString(),
                                          tbPerson.Rows[i]["姓名"].ToString(),
                                          tbPerson.Rows[i]["电话"].ToString(),
                                          tbPerson.Rows[i]["邮箱"].ToString(),
                                          tbPerson.Rows[i]["班级"].ToString(),
                                          tbPerson.Rows[i]["年级"].ToString(),
                                          tbPerson.Rows[i]["考勤号"].ToString()
                                          );
                    personList.Add(p);
                }
            }
            return personList;
        }
        /// <summary>
        /// 删除学号与EPC的关联
        /// </summary>
        /// <param name="xh"></param>
        /// <returns></returns>
        public static bool DeletePersonEPC(string xh)
        {
            bool bR = false;
            try
            {
                if (InitialDB())
                {
                    int result = 0;
                    object[] pars = new object[1]
	                    {
	                        xh
	                    };
                    result = int.Parse(SQLiteHelper.ExecuteNonQuery(dbPath, SqlDeletePersonEPC, pars).ToString());
                    if (result >= 1)
                    {
                        bR = true;
                    }
                    else
                    {
                        bR = false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("更新记录出现错误：" + ex.Message);
            }
            return bR;
        }
        /// <summary>
        /// 将学号和epc相关联
        /// </summary>
        /// <param name="xh"></param>
        /// <param name="epc"></param>
        /// <returns></returns>
        public static bool UpdatePersonEPC(string xh, string epc)
        {
            bool bR = false;
            try
            {
                if (InitialDB())
                {
                    int result = 0;
                    object[] pars = new object[2]
	                    {
	                        epc
	                        ,xh
	                    };
                    result = int.Parse(SQLiteHelper.ExecuteNonQuery(dbPath, SqlUpdatePersonEPC, pars).ToString());
                    if (result >= 1)
                    {
                        bR = true;
                    }
                    else
                    {
                        bR = false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("更新记录出现错误：" + ex.Message);
            }
            return bR;
        }
        #endregion
        public static void AddCheckRecordTableToCheckDS(ref DataSet ds)
        {
            DataTable table = new DataTable("CheckRecords");
            DataColumn idColumn = new DataColumn("id_no");
            DataColumn DateColumn = new DataColumn("Date");
            table.Columns.Add(idColumn);
            table.Columns.Add(DateColumn);
            ds.Tables.Add(table);
        }
        public static void InitialCheckDB()
        {
            try
            {

                int result = 0;
                //result = int.Parse(SQLiteHelper.ExecuteScalar(dbPath, tableCheckRecordExist, null).ToString());
                if (!(result == 1))
                {
                    //result = int.Parse(SQLiteHelper.ExecuteNonQuery(dbPath, TableCheckRecordCreate, null).ToString());
                    if (result == 0)
                    {
                        //MessageBox.Show("Create table success");
                    }
                    else
                    {
                        MessageBox.Show("初始化学生数据库时出现错误！");
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("初始化考勤数据库时出现错误！" + ex.Message);
            }
        }
        public static List<string> GetbjList()
        {
            List<string> list = new List<string>();
            DataSet ds = null;
            try
            {
                if (InitialDB())
                {
                    ds = SQLiteHelper.ExecuteDataSet(dbPath, SqlGetbj, null);
                }
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        list.Add(dt.Rows[i][0].ToString());
                    }
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("读取学生数据库时出现错误！" + ex.Message);
                return list;
            }
            return list;
        }
        public static List<string> GetnjList()
        {
            List<string> list = new List<string>();
            DataSet ds = null;
            try
            {
                if (InitialDB())
                {
                    ds = SQLiteHelper.ExecuteDataSet(dbPath, SqlGetnj, null);
                }
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        list.Add(dt.Rows[i][0].ToString());
                    }
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("读取学生数据库时出现错误！" + ex.Message);
                return list;
            }
            return list;
        }
        public static DataSet GetStatisticCheckInfoDataSet(string startDate, string endDate
                                                    , string nj, string bj)
        {
            DataSet ds = null;
            if (null == startDate || null == endDate)
            {
                return ds;
            }
            int paraCount = 2;
            string sql = SqlSelectCheckedPersonWithPara_head;
            if (null != nj)
            {
                paraCount++;
                sql += SqlSelectCheckedPersonWithPara_nj;
            }
            if (null != bj)
            {
                paraCount++;
                sql += SqlSelectCheckedPersonWithPara_bj;
            }
            sql += SqlSelectCheckedPersonWithPara_tail;
            Object[] paras = new Object[paraCount];
            paras[0] = startDate;
            paras[1] = endDate;
            if (null != nj)
            {
                paras[2] = nj;
            }
            if (null != bj)
            {
                if (paras[2] == null)
                {
                    paras[2] = bj;
                }
                else
                {
                    paras[3] = bj;
                }
            }
            try
            {
                if (InitialDB())
                {
                    ds = SQLiteHelper.ExecuteDataSet(dbPath, sql, paras);
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("读取学生数据库时出现错误！" + ex.Message);
                return null;
            }
            return ds;

        }
        public static DataSet GetCheckDataSet()
        {
            DataSet dsCheckRecords = null;
            try
            {
                if (InitialDB())
                {
                    dsCheckRecords = SQLiteHelper.ExecuteDataSet(dbPath, SqlSelectAllRecords, null);
                }
                /* 
                
                if (!File.Exists(FileCheckName))
                {
                    rfidCheck_CheckOn.InitialCheckDB();
                }
                dsCheckRecords = new DataSet();
                dsCheckRecords.ReadXml(FileCheckName);
                */

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("读取学生数据库时出现错误！" + ex.Message);
                return null;
            }

            return dsCheckRecords;
        }
        public static DataSet GetUnCheckedPersonDataSet(string dateStart, string dateEnd)
        {
            DataSet ds = null;
            try
            {
                if (InitialDB())
                {
                    ds = SQLiteHelper.ExecuteDataSet(dbPath,
                                                    SqlSelectUnCheckedPersonInPeriod
                                                    , new object[2] { dateStart, dateEnd });
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("读取学生数据库时出现错误！" + ex.Message);
                return null;
            }
            return ds;
        }
        public static DataSet GetCheckedPersonDataSet(string dateStart, string dateEnd)
        {
            DataSet ds = null;
            try
            {
                if (InitialDB())
                {
                    ds = SQLiteHelper.ExecuteDataSet(dbPath,
                                                    SqlSelectCheckedPersonInPeriod
                                                    , new object[2] { dateStart, dateEnd });
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("读取学生数据库时出现错误！" + ex.Message);
                return null;
            }
            return ds;
        }
        /// <summary>
        /// 获取指定时间考勤过的人员列表
        /// </summary>
        /// <param name="strCheckDate"></param>
        /// <returns></returns>
        public static List<CheckPerson> GetCheckedPersonList(string strCheckDate)
        {
            List<CheckPerson> personList = new List<CheckPerson>();
            ///* 


            //if (null == strCheckDate)
            //{
            //    return null;
            //}
            //DataSet myDataSetPerson = rfidCheck_CheckOn.GetPersonDataSet();
            //DataSet myDataSetRecords = rfidCheck_CheckOn.GetCheckDataSet();
            //DataTable tbPerson = myDataSetPerson.Tables["student"];
            //if (tbPerson == null)
            //{
            //    rfidCheck_CheckOn.AddStudentTableToPersonDS(ref myDataSetPerson);
            //    tbPerson = myDataSetPerson.Tables["student"];
            //}
            //DataTable tbRecords = myDataSetRecords.Tables["CheckRecords"];
            //if (null == tbRecords)
            //{
            //    rfidCheck_CheckOn.AddCheckRecordTableToCheckDS(ref myDataSetRecords);
            //    tbRecords = myDataSetRecords.Tables["CheckRecords"];
            //}
            //string strDate = null;
            //strDate = rfidCheck_CheckOn.GetDateSubString(strCheckDate);
            //if (null != strDate)
            //{
            //    for (int i = 0; i < tbRecords.Rows.Count; i++)
            //    {
            //        string strDateTemp = rfidCheck_CheckOn.GetDateSubString(tbRecords.Rows[i]["Date"].ToString()); ;

            //        if (null != strDateTemp && strDateTemp == strDate)// 保证获得的记录的时间和要求的时间一致
            //        {
            //            string tempID = tbRecords.Rows[i]["id_no"].ToString();
            //            string tempDate = tbRecords.Rows[i]["Date"].ToString();
            //            for (int j = 0; j < tbPerson.Rows.Count; j++)
            //            {
            //                if (tbPerson.Rows[j]["id_no"].ToString() == tempID)
            //                {
            //                    CheckRecord cr = new CheckRecord(tempID, tbPerson.Rows[j]["name"].ToString(), tbRecords.Rows[i]["Date"].ToString());
            //                    Person p = new Person(tbPerson.Rows[j]["id_no"].ToString(), tbPerson.Rows[j]["name"].ToString(), tbPerson.Rows[j]["tel"].ToString(), tbPerson.Rows[j]["mail"].ToString());
            //                    CheckPerson cp = new CheckPerson(tempDate, p);
            //                    personList.Add(cp);
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}

            return personList;
        }
        public static List<CheckRecord> GetCheckRecords(string startTime, string endTime)
        {
            List<CheckRecord> records = new List<CheckRecord>();

            return records;
        }
        /// <summary>
        /// 获取考勤记录
        /// </summary>
        /// <param name="strCheckDate">需要获取的考勤记录的时间</param>
        /// <returns>考勤记录的集合</returns>
        public static List<CheckRecord> GetCheckRecords(string strCheckDate)
        {
            if (null == strCheckDate)
            {
                return null;
            }
            List<CheckRecord> records = new List<CheckRecord>();
            DataSet myDataSetRecords = rfidCheck_CheckOn.GetCheckDataSet();
            DataTable tbRecords = myDataSetRecords.Tables[0];
            if (null != tbRecords)
            {
            }
            /* 
            
            if (null == tbRecords)
            {
                rfidCheck_CheckOn.AddCheckRecordTableToCheckDS(ref myDataSetRecords);
                tbRecords = myDataSetRecords.Tables["CheckRecords"];
            }
            string strDate = null;
            strDate = rfidCheck_CheckOn.GetDateSubString(strCheckDate);
            if (null!=strDate)
            {
                for (int i = 0; i < tbRecords.Rows.Count; i++)
                {
                    string strDateTemp = rfidCheck_CheckOn.GetDateSubString(tbRecords.Rows[i]["Date"].ToString()); ;

                    if (null != strDateTemp && strDateTemp == strDate)// 保证获得的记录的时间和要求的时间一致
                    {
                        string tempID = tbRecords.Rows[i]["id_no"].ToString();
                        string tempDate=tbRecords.Rows[i]["Date"].ToString();
                        CheckRecord cr = new CheckRecord(tempID, tempDate);
                        records.Add(cr);
                    }
                }
            }
            */
            return records;
        }

        public static bool CheckEPCUsed(string epc)
        {
            bool bR = false;
            try
            {
                if (InitialDB())
                {
                    int result = 0;
                    object[] pars = new object[1]
	                    {
	                       epc
	                    };

                    result = int.Parse(SQLiteHelper.ExecuteScalar(dbPath, SqlCheckEPCUsed, pars).ToString());
                    if (result >= 1)
                    {
                        bR = true;
                    }
                    else
                    {
                        bR = false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("更新记录出现错误：" + ex.Message);
            }
            return bR;
        }
        /// <summary>
        /// 检查指定ID是否已经在考勤记录中存在
        /// </summary>
        /// <param name="id">学生id</param>
        /// <returns></returns>
        public static bool CheckIDExist(string id)
        {
            bool bR = false;
            List<string> list = rfidCheck_CheckOn.GetCheckedIDList();
            if (list == null)
            {
                return bR;
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == id)
                {
                    bR = true;
                    break;
                }
            }
            return bR;
        }

        /// <summary>
        /// 获取已经考勤的id记录,将由 GetCheckRecords 代替
        /// </summary>
        /// <returns></returns>
        public static List<string> GetCheckedIDList()
        {
            List<string> listRtn = new List<string>();

            DataSet CheckRecords = rfidCheck_CheckOn.GetCheckDataSet();
            if (CheckRecords.Tables.Count <= 0)
            {
                rfidCheck_CheckOn.AddCheckRecordTableToCheckDS(ref CheckRecords);
                //return listRtn;
            }
            for (int i = 0; i < CheckRecords.Tables["CheckRecords"].Rows.Count; i++)
            {
                listRtn.Add(CheckRecords.Tables["CheckRecords"].Rows[i][0].ToString());
            }
            return listRtn;
        }
        //暂时不用
        public static int CheckOn(string id, string strDate)
        {
            bool bwriteData = true;
            DataSet CheckRecords = new DataSet();
            try
            {
                if (!File.Exists(FileCheckName))
                {
                    DataSet dsCheckRecords = new DataSet("checkR");
                    dsCheckRecords.WriteXml(FileCheckName);
                }
                if (null != id && null != strDate)
                {
                    CheckRecords.ReadXml(FileCheckName);
                    if (CheckRecords.Tables.Count <= 0)
                    {
                        DataTable table = new DataTable("CheckRecords");
                        DataColumn idColumn = new DataColumn("id_no");
                        DataColumn DateColumn = new DataColumn("Date");
                        table.Columns.Add(idColumn);
                        table.Columns.Add(DateColumn);
                        CheckRecords.Tables.Add(table);
                    }
                    for (int i = 0; i < CheckRecords.Tables["CheckRecords"].Rows.Count; i++)
                    {
                        if (id == CheckRecords.Tables["CheckRecords"].Rows[i][0].ToString()
                            && strDate == CheckRecords.Tables["CheckRecords"].Rows[i][1].ToString())
                        {
                            bwriteData = false;
                            break;
                        }
                    }
                    if (bwriteData)
                    {
                        DataRow myRow = CheckRecords.Tables["CheckRecords"].NewRow();
                        myRow["id_no"] = id;
                        myRow["Date"] = DateTime.Today.ToShortDateString() + " " + DateTime.Today.ToShortTimeString();
                        CheckRecords.Tables["CheckRecords"].Rows.Add(myRow);
                        CheckRecords.WriteXml(FileCheckName);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return 0;
        }
        /// <summary>
        /// 将考勤记录至数据库中
        /// </summary>
        /// <param name="_cr"></param>
        public static bool AddCheckRecord(CheckRecord _cr)
        {
            bool bR = false;
            try
            {
                if (InitialDB())
                {
                    int result = 0;
                    object[] pars = new object[2]
                        {
                            _cr.id,
                            _cr.checkDate
                        };

                    result = int.Parse(SQLiteHelper.ExecuteNonQuery(dbPath, SqlInsertCheckRecord, pars).ToString());
                    if (result >= 1)
                    {
                        bR = true;
                    }
                    else
                    {
                        bR = false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("添加新考勤记录出现错误：" + ex.Message);
            }
            return bR;
            /* 
            
            string strDate = rfidCheck_CheckOn.GetDateSubString(_cr.checkDate);
            bool bwriteData = true;
            DataSet CheckRecords = rfidCheck_CheckOn.GetCheckDataSet();
            try
            {
                if (null != _cr.id && null != strDate)
                {
                    if (CheckRecords.Tables.Count <= 0)
                    {
                        rfidCheck_CheckOn.AddCheckRecordTableToCheckDS(ref CheckRecords);
                    }
                    for (int i = 0; i < CheckRecords.Tables["CheckRecords"].Rows.Count; i++)
                    {
                        if (_cr.id == CheckRecords.Tables["CheckRecords"].Rows[i][0].ToString()
                            && strDate == rfidCheck_CheckOn.GetDateSubString(CheckRecords.Tables["CheckRecords"].Rows[i][1].ToString()))
                        {
                            bwriteData = false;
                            break;
                        }
                    }
                    if (bwriteData)
                    {
                        DataRow myRow = CheckRecords.Tables["CheckRecords"].NewRow();
                        myRow["id_no"] = _cr.id;
                        myRow["Date"] = strDate;
                        CheckRecords.Tables["CheckRecords"].Rows.Add(myRow);
                        CheckRecords.WriteXml(FileCheckName);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            
            }
*/
        }
        /// <summary>
        /// 将id记录至数据库中
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int CheckOn(string id)
        {
            string strDate = DateTime.Today.ToShortDateString();
            bool bwriteData = true;
            DataSet CheckRecords = rfidCheck_CheckOn.GetCheckDataSet();
            try
            {
                //if (!File.Exists(FileCheckName))
                //{
                //    DataSet dsCheckRecords = new DataSet("checkR");
                //    dsCheckRecords.WriteXml(FileCheckName);
                //}
                if (null != id && null != strDate)
                {
                    //CheckRecords.ReadXml(FileCheckName);
                    if (CheckRecords.Tables.Count <= 0)
                    {
                        rfidCheck_CheckOn.AddCheckRecordTableToCheckDS(ref CheckRecords);
                        //DataTable table = new DataTable("CheckRecords");
                        //DataColumn idColumn = new DataColumn("id_no");
                        //DataColumn DateColumn = new DataColumn("Date");
                        //table.Columns.Add(idColumn);
                        //table.Columns.Add(DateColumn);
                        //CheckRecords.Tables.Add(table);
                    }
                    for (int i = 0; i < CheckRecords.Tables["CheckRecords"].Rows.Count; i++)
                    {
                        if (id == CheckRecords.Tables["CheckRecords"].Rows[i][0].ToString()
                            && strDate == rfidCheck_CheckOn.GetDateSubString(CheckRecords.Tables["CheckRecords"].Rows[i][1].ToString()))
                        //&& strDate ==Regex.Match(CheckRecords.Tables["CheckRecords"].Rows[i][1].ToString(),@"\d{4}[-/](1[1-2]|0?\d)[-/](3[0-1]|[0-2]?\d\s)").ToString() )
                        {
                            bwriteData = false;
                            break;
                        }
                    }
                    if (bwriteData)
                    {
                        DataRow myRow = CheckRecords.Tables["CheckRecords"].NewRow();
                        myRow["id_no"] = id;
                        myRow["Date"] = DateTime.Today.ToShortDateString() + " " + DateTime.Today.ToShortTimeString();
                        CheckRecords.Tables["CheckRecords"].Rows.Add(myRow);
                        CheckRecords.WriteXml(FileCheckName);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return 0;
        }
    }
}
