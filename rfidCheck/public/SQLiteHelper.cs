using System;
using System.Data;
using System.Text.RegularExpressions ;
using System.Xml;
using System.IO;
using System.Collections ;
using System.Data.SQLite;

namespace LogisTechBase.rfidCheck
{

    public class SQLiteParameterCollectionSub : IDataParameterCollection, IList
    {
        #region Fields

        ArrayList numeric_param_list = new ArrayList();
        Hashtable named_param_hash = new Hashtable();

        #endregion

        #region Private Methods

        private void CheckSqliteParam(object value)
        {
            if (!(value is SQLiteParameter))
                throw new InvalidCastException("Can only use SQLiteParameter objects");
            SQLiteParameter sqlp = value as SQLiteParameter;
            if (sqlp.ParameterName == null || sqlp.ParameterName.Length == 0)
                sqlp.ParameterName = this.GenerateParameterName();
        }

        private void RecreateNamedHash()
        {
            for (int i = 0; i < numeric_param_list.Count; i++)
            {
                named_param_hash[((SQLiteParameter)numeric_param_list[i]).ParameterName] = i;
            }
        }

        //FIXME: if the user is calling Insert at various locations with unnamed parameters, this is not going to work....
        private string GenerateParameterName()
        {
            int index = this.Count + 1;
            string name = String.Empty;

            while (index > 0)
            {
                name = ":" + index.ToString();
                if (this.IndexOf(name) == -1)
                    index = -1;
                else
                    index++;
            }
            return name;
        }

        #endregion

        #region Properties

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                CheckSqliteParam(value);
                this[index] = (SQLiteParameter)value;
            }
        }

        object IDataParameterCollection.this[string parameterName]
        {
            get
            {
                return this[parameterName];
            }
            set
            {
                CheckSqliteParam(value);
                this[parameterName] = (SQLiteParameter)value;
            }
        }

        private bool isPrefixed(string parameterName)
        {
            return parameterName.Length > 1 && (parameterName[0] == ':' || parameterName[0] == '$');
        }

        SQLiteParameter GetParameter(int parameterIndex)
        {
            if (this.Count >= parameterIndex + 1)
                return (SQLiteParameter)numeric_param_list[parameterIndex];
            else
                throw new IndexOutOfRangeException("The specified parameter index does not exist: " + parameterIndex.ToString());
        }

        SQLiteParameter GetParameter(string parameterName)
        {
            if (this.Contains(parameterName))
                return this[(int)named_param_hash[parameterName]];
            else if (isPrefixed(parameterName) && this.Contains(parameterName.Substring(1)))
                return this[(int)named_param_hash[parameterName.Substring(1)]];
            else
                throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
        }

        void SetParameter(int parameterIndex, SQLiteParameter parameter)
        {
            if (this.Count >= parameterIndex + 1)
                numeric_param_list[parameterIndex] = parameter;
            else
                throw new IndexOutOfRangeException("The specified parameter index does not exist: " + parameterIndex.ToString());
        }

        void SetParameter(string parameterName, SQLiteParameter parameter)
        {
            if (this.Contains(parameterName))
                numeric_param_list[(int)named_param_hash[parameterName]] = parameter;
            else if (parameterName.Length > 1 && this.Contains(parameterName.Substring(1)))
                numeric_param_list[(int)named_param_hash[parameterName.Substring(1)]] = parameter;
            else
                throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
        }

        public SQLiteParameter this[string parameterName]
        {
            get { return GetParameter(parameterName); }
            set { SetParameter(parameterName, value); }
        }

        public SQLiteParameter this[int parameterIndex]
        {
            get { return GetParameter(parameterIndex); }
            set { SetParameter(parameterIndex, value); }
        }

        public int Count
        {
            get { return this.numeric_param_list.Count; }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return this.numeric_param_list.IsFixedSize;
            }
        }

        bool IList.IsReadOnly
        {
            get { return this.numeric_param_list.IsReadOnly; }
        }


        bool ICollection.IsSynchronized
        {
            get { return this.numeric_param_list.IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return this.numeric_param_list.SyncRoot; }
        }

        #endregion

        #region Public Methods

        public int Add(object value)
        {
            CheckSqliteParam(value);
            SQLiteParameter sqlp = value as SQLiteParameter;
            if (named_param_hash.Contains(sqlp.ParameterName))
                throw new DuplicateNameException("Parameter collection already contains the a SQLiteParameter with the given ParameterName.");
            named_param_hash[sqlp.ParameterName] = numeric_param_list.Add(value);
            return (int)named_param_hash[sqlp.ParameterName];
        }

        public SQLiteParameter Add(SQLiteParameter param)
        {
            Add((object)param);
            return param;
        }

        public SQLiteParameter Add(string name, object value)
        {
            return Add(new SQLiteParameter(name, value));
        }

        public SQLiteParameter Add(string name, DbType type)
        {
            return Add(new SQLiteParameter(name, type));
        }

        public void Clear()
        {
            numeric_param_list.Clear();
            named_param_hash.Clear();
        }

        public void CopyTo(Array array, int index)
        {
            this.numeric_param_list.CopyTo(array, index);
        }

        bool IList.Contains(object value)
        {
            return Contains((SQLiteParameter)value);
        }

        public bool Contains(string parameterName)
        {
            return named_param_hash.Contains(parameterName);
        }

        public bool Contains(SQLiteParameter param)
        {
            return Contains(param.ParameterName);
        }

        public IEnumerator GetEnumerator()
        {
            return this.numeric_param_list.GetEnumerator();
        }

        int IList.IndexOf(object param)
        {
            return IndexOf((SQLiteParameter)param);
        }

        public int IndexOf(string parameterName)
        {
            if (isPrefixed(parameterName))
            {
                string sub = parameterName.Substring(1);
                if (named_param_hash.Contains(sub))
                    return (int)named_param_hash[sub];
            }
            if (named_param_hash.Contains(parameterName))
                return (int)named_param_hash[parameterName];
            else
                return -1;
        }

        public int IndexOf(SQLiteParameter param)
        {
            return IndexOf(param.ParameterName);
        }

        public void Insert(int index, object value)
        {
            CheckSqliteParam(value);
            if (numeric_param_list.Count == index)
            {
                Add(value);
                return;
            }

            numeric_param_list.Insert(index, value);
            RecreateNamedHash();
        }

        public void Remove(object value)
        {
            CheckSqliteParam(value);
            RemoveAt((SQLiteParameter)value);
        }

        public void RemoveAt(int index)
        {
            RemoveAt(((SQLiteParameter)numeric_param_list[index]).ParameterName);
        }

        public void RemoveAt(string parameterName)
        {
            if (!named_param_hash.Contains(parameterName))
                throw new ApplicationException("Parameter " + parameterName + " not found");

            numeric_param_list.RemoveAt((int)named_param_hash[parameterName]);
            named_param_hash.Remove(parameterName);

            RecreateNamedHash();
        }

        public void RemoveAt(SQLiteParameter param)
        {
            RemoveAt(param.ParameterName);
        }

        #endregion
    }
	/// <summary>
	/// SQLiteHelper is a utility class similar to "SQLHelper" in MS
	/// Data Access Application Block and follows similar pattern.
	/// </summary>
	public class SQLiteHelper
	{
		/// <summary>
		/// Creates a new <see cref="SQLiteHelper"/> instance. The ctor is marked private since all members are static.
		/// </summary>
		private SQLiteHelper()
		{			 
		}
		///<overloads></overloads>
		/// <summary>
		/// Creates a command.
		/// </summary>
		/// <param name="connection">Connection.</param>
		/// <param name="commandText">Command text.</param>
		/// <param name="commandParameters">Command parameters.</param>
		/// <returns>SQLite Command</returns>
		public static SQLiteCommand CreateCommand(SQLiteConnection connection, string commandText,  params SQLiteParameter[] commandParameters) 
		{
			SQLiteCommand cmd= new SQLiteCommand(commandText,connection);
			if(commandParameters.Length>0)
				
			{
				foreach(SQLiteParameter parm in commandParameters)
					cmd.Parameters.Add(parm);
			}
			return cmd;		 
		}

		/// <summary>
		/// Creates the command.
		/// </summary>
		/// <param name="connectionString">Connection string.</param>
		/// <param name="commandText">Command text.</param>
		/// <param name="commandParameters">Command parameters.</param>
		/// <returns>SQLite Command</returns>
		public static SQLiteCommand CreateCommand(string connectionString, string commandText,  params SQLiteParameter[] commandParameters) 
		{
			SQLiteConnection cn=new SQLiteConnection(connectionString);
			 
			SQLiteCommand cmd= new SQLiteCommand(commandText,cn);
			 		
			if(commandParameters.Length>0)
				
			{
				foreach(SQLiteParameter parm in commandParameters)
					cmd.Parameters.Add(parm);
			}
			return cmd;		 
		}
		/// <summary>
		/// Creates the parameter.
		/// </summary>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="parameterType">Parameter type.</param>
		/// <param name="parameterValue">Parameter value.</param>
		/// <returns>SQLiteParameter</returns>
		public static SQLiteParameter CreateParameter(string parameterName, System.Data.DbType parameterType , object parameterValue)
		{
			SQLiteParameter parameter = new SQLiteParameter();
			parameter.DbType=parameterType;
			parameter.ParameterName =parameterName;
			parameter.Value =parameterValue;
			return parameter;
		}

		/// <summary>
		/// Shortcut method to execute dataset from SQL Statement and object[] arrray of parameter values
		/// </summary>
		/// <param name="connectionString">SQLite Connection string</param>
		/// <param name="commandText">SQL Statement with embedded "@param" style parameter names</param>
		/// <param name="paramList">object[] array of parameter values</param>
		/// <returns></returns>
		public static DataSet ExecuteDataSet(string connectionString, string commandText, object[] paramList)
		{
			SQLiteConnection cn = new SQLiteConnection(connectionString);
			SQLiteCommand cmd =cn.CreateCommand();
			 

			cmd.CommandText =commandText;
			if(paramList!=null)
			{
                SQLiteParameterCollectionSub parms = DeriveParameters(commandText, paramList);
				foreach (SQLiteParameter p in parms)
					cmd.Parameters.Add(p);
			}
			DataSet ds = new DataSet();
			if(cn.State ==ConnectionState.Closed)
				cn.Open();
			SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
			da.Fill(ds);
			da.Dispose();
			cmd.Dispose();
			cn.Close();
			return ds;
		}
		///<overloads></overloads>
		/// <summary>
		/// Shortcut method to execute dataset from SQL Statement and object[] arrray of            /// parameter values
		/// </summary>
		/// <param name="cn">Connection.</param>
		/// <param name="commandText">Command text.</param>
		/// <param name="paramList">Param list.</param>
		/// <returns></returns>
		public static DataSet ExecuteDataSet(SQLiteConnection cn, string commandText, object[] paramList)
		{
			
			SQLiteCommand cmd =cn.CreateCommand();
			 

			cmd.CommandText =commandText;
			if(paramList!=null)
			{
                SQLiteParameterCollectionSub parms = DeriveParameters(commandText, paramList);
				foreach (SQLiteParameter p in parms)
					cmd.Parameters.Add(p);
			}
			DataSet ds = new DataSet();
			if(cn.State ==ConnectionState.Closed)
				cn.Open();
			SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
			da.Fill(ds);
			da.Dispose();
			cmd.Dispose();
			cn.Close();
			return ds;
		}
		/// <summary>
		/// Executes the dataset from a populated Command object.
		/// </summary>
		/// <param name="cmd">Fully populated SQLiteCommand</param>
		/// <returns>DataSet</returns>
		public static DataSet ExecuteDataSet(SQLiteCommand cmd)
		{
			if(cmd.Connection.State ==ConnectionState.Closed)
				cmd.Connection.Open();
			DataSet ds = new DataSet();
			SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
			da.Fill(ds);
			da.Dispose();
			cmd.Connection.Close();
			cmd.Dispose();			
			return ds;			
		}

		/// <summary>
		/// Executes the dataset in a SQLite Transaction
		/// </summary>
		/// <param name="transaction">SQLiteTransaction. Transaction consists of Connection, Transaction, and Command, all of which must be created prior to making this method call. </param>
		/// <param name="commandText">Command text.</param>
		/// <param name="commandParameters">Sqlite Command parameters.</param>
		/// <returns>DataSet</returns>
		/// <remarks>user must examine Transaction Object and handle transaction.connection .Close, etc.</remarks>
		public static DataSet ExecuteDataSet(SQLiteTransaction transaction,  string commandText, params SQLiteParameter[] commandParameters)
		{
			
			if( transaction == null ) throw new ArgumentNullException( "transaction" );
			if( transaction != null && transaction.Connection == null ) throw new ArgumentException( "The transaction was rolled back or committed, please provide an open transaction.", "transaction" );
			IDbCommand cmd = transaction.Connection.CreateCommand();
			cmd.CommandText =commandText;
			foreach( SQLiteParameter parm in commandParameters)
			{
				cmd.Parameters.Add(parm);
			}
			if(transaction.Connection.State ==ConnectionState.Closed)
				transaction.Connection.Open();
			DataSet ds=	ExecuteDataSet((SQLiteCommand)cmd);
			return ds;
		}

		/// <summary>
		/// Executes the dataset with Transaction and object array of parameter values.
		/// </summary>
		/// <param name="transaction">SQLiteTransaction. Transaction consists of Connection, Transaction, and Command, all of which must be created prior to making this method call. </param>
		/// <param name="commandText">Command text.</param>
		/// <param name="commandParameters">object[] array of parameter values.</param>
		/// <returns>DataSet</returns>
		/// <remarks>user must examine Transaction Object and handle transaction.connection .Close, etc.</remarks>
		public static DataSet ExecuteDataSet(SQLiteTransaction transaction,  string commandText,   object[] commandParameters)
		{
			
			if( transaction == null ) throw new ArgumentNullException( "transaction" );
			if( transaction != null && transaction.Connection == null ) throw new ArgumentException( "The transaction was rolled back or committed, please provide an open transaction.", "transaction" );
			IDbCommand cmd = transaction.Connection.CreateCommand();
			cmd.CommandText =commandText;
            SQLiteParameterCollectionSub coll = DeriveParameters(cmd.CommandText, commandParameters);
			foreach(SQLiteParameter parm in coll)
				cmd.Parameters.Add(parm);
			if(transaction.Connection.State ==ConnectionState.Closed)
				transaction.Connection.Open();

			DataSet ds=	ExecuteDataSet((SQLiteCommand)cmd);
			return ds;
		}

		#region UpdateDataset
		/// <summary>
		/// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
		/// </remarks>
		/// <param name="insertCommand">A valid SQL statement  to insert new records into the data source</param>
		/// <param name="deleteCommand">A valid SQL statement to delete records from the data source</param>
		/// <param name="updateCommand">A valid SQL statement used to update records in the data source</param>
		/// <param name="dataSet">The DataSet used to update the data source</param>
		/// <param name="tableName">The DataTable used to update the data source.</param>
		public static void UpdateDataset(SQLiteCommand insertCommand, SQLiteCommand deleteCommand, SQLiteCommand updateCommand, DataSet dataSet, string tableName)
		{
			if( insertCommand == null ) throw new ArgumentNullException( "insertCommand" );
			if( deleteCommand == null ) throw new ArgumentNullException( "deleteCommand" );
			if( updateCommand == null ) throw new ArgumentNullException( "updateCommand" );
			if( tableName == null || tableName.Length == 0 ) throw new ArgumentNullException( "tableName" ); 

			// Create a SQLiteDataAdapter, and dispose of it after we are done
			using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter())
			{
				// Set the data adapter commands
				dataAdapter.UpdateCommand = updateCommand;
				dataAdapter.InsertCommand = insertCommand;
				dataAdapter.DeleteCommand = deleteCommand;

				// Update the dataset changes in the data source
				dataAdapter.Update (dataSet, tableName); 

				// Commit all the changes made to the DataSet
				dataSet.AcceptChanges();
			}
		}
		#endregion




		/// <summary>
		/// ShortCut method to return IDataReader
		/// NOTE: You should explicitly close the Command.connection you passed in as
		/// well as call Dispose on the Command  after reader is closed.
		/// We do this because IDataReader has no underlying Connection Property.
		/// </summary>
		/// <param name="cmd">SQLiteCommand Object</param>
		/// <param name="commandText">SQL Statement with optional embedded "@param" style parameters</param>
		/// <param name="paramList">object[] array of parameter values</param>
		/// <returns>IDataReader</returns>
		public static IDataReader ExecuteReader(SQLiteCommand cmd, string commandText, object[] paramList)
		{
			if(cmd.Connection==null)
				throw new ArgumentException("Command must have live connection attached.","cmd");
			cmd.CommandText =commandText;
            if (null != paramList)
            {

                SQLiteParameterCollectionSub parms = DeriveParameters(commandText, paramList);
                foreach (SQLiteParameter p in parms)
                    cmd.Parameters.Add(p);
            }
			if(cmd.Connection.State ==ConnectionState.Closed)
				cmd.Connection.Open();
			IDataReader rdr=cmd.ExecuteReader(CommandBehavior.CloseConnection);			 
			return rdr;
		}

		/// <summary>
		/// Shortcut to ExecuteNonQuery with SqlStatement and object[] param values
		/// </summary>
		/// <param name="connectionString">SQLite Connection String</param>
		/// <param name="commandText">Sql Statement with embedded "@param" style parameters</param>
		/// <param name="paramList">object[] array of parameter values</param>
		/// <returns></returns>
		public static int ExecuteNonQuery(string connectionString, string commandText, object[] paramList)
		{
			SQLiteConnection cn = new SQLiteConnection(connectionString);
			SQLiteCommand cmd =cn.CreateCommand();
			cmd.CommandText =commandText;
            if (null != paramList)
            {
                SQLiteParameterCollectionSub parms = DeriveParameters(commandText, paramList);
                if (null != parms)
                {
                    foreach (SQLiteParameter p in parms)
                        cmd.Parameters.Add(p);
                }
            }
			if(cn.State ==ConnectionState.Closed)
				cn.Open();
			int result = cmd.ExecuteNonQuery();
			cmd.Dispose();
			cn.Close();
			 
			return result;
		}



		public static int ExecuteNonQuery(SQLiteConnection cn, string commandText, object[] paramList)
		{
			 
			SQLiteCommand cmd =cn.CreateCommand();
			cmd.CommandText =commandText;
            if (null != paramList)
            {
                SQLiteParameterCollectionSub parms = DeriveParameters(commandText, paramList);
                foreach (SQLiteParameter p in parms)
                    cmd.Parameters.Add(p);
            }
			if(cn.State ==ConnectionState.Closed)
				cn.Open();
			int result = cmd.ExecuteNonQuery();
			cmd.Dispose();
			cn.Close();
			 
			return result;
		}

		/// <summary>
		/// Executes  non-query sql Statment with Transaction
		/// </summary>
		/// <param name="transaction">SQLiteTransaction. Transaction consists of Connection, Transaction, and Command, all of which must be created prior to making this method call. </param>
		/// <param name="commandText">Command text.</param>
		/// <param name="paramList">Param list.</param>
		/// <returns>Integer</returns>
		/// <remarks>user must examine Transaction Object and handle transaction.connection .Close, etc.</remarks>
		public static int ExecuteNonQuery(SQLiteTransaction transaction, string commandText, object[] paramList)
		{					
			if( transaction == null ) throw new ArgumentNullException( "transaction" );
			if( transaction != null && transaction.Connection == null ) throw new ArgumentException( "The transaction was rolled back or committed, please provide an open transaction.", "transaction" );
			IDbCommand cmd = transaction.Connection.CreateCommand();
			cmd.CommandText =commandText;
            if (null != paramList)
            {
                SQLiteParameterCollectionSub coll = DeriveParameters(cmd.CommandText, paramList);
                foreach (SQLiteParameter parm in coll)
                    cmd.Parameters.Add(parm);
            }
			if(transaction.Connection.State ==ConnectionState.Closed)
				transaction.Connection.Open();
			int result = cmd.ExecuteNonQuery();
			cmd.Dispose();			 
			return result;
		}


		///<overloads></overloads> 
		/// <summary>
		/// Executes the non query.
		/// </summary>
		/// <param name="cmd">CMD.</param>
		/// <returns></returns>
		public static int ExecuteNonQuery(IDbCommand cmd)
		{					 
			if(cmd.Connection.State ==ConnectionState.Closed)
				cmd.Connection.Open();
			int result = cmd.ExecuteNonQuery();
			cmd.Connection.Close();
			cmd.Dispose();			 
			return result;
		}

		/// <summary>
		/// Shortcut to ExecuteScalar with Sql Statement embedded params and object[] param values
		/// </summary>
		/// <param name="connectionString">SQLite Connection String</param>
		/// <param name="commandText">SQL statment with embedded "@param" style parameters</param>
		/// <param name="paramList">object[] array of param values</param>
		/// <returns></returns>
		public static object ExecuteScalar(string connectionString, string commandText, object[] paramList)
		{
			SQLiteConnection cn = new SQLiteConnection(connectionString);
			SQLiteCommand cmd =cn.CreateCommand();
			cmd.CommandText =commandText;
            SQLiteParameterCollectionSub parms = DeriveParameters(commandText, paramList);
            if (null!=parms)
            {
                foreach (SQLiteParameter p in parms)
                    cmd.Parameters.Add(p);
            }
			if(cn.State ==ConnectionState.Closed)
				cn.Open();
			object result = cmd.ExecuteScalar();
			cmd.Dispose();
			cn.Close();
			 
			return result;
		}

		/// <summary>
		/// Execute XmlReader with complete Command
		/// </summary>
		/// <param name="command">SQLite Command</param>
		/// <returns>XmlReader</returns>
		public static XmlReader ExecuteXmlReader(IDbCommand command)
		{	// open the connection if necessary, but make sure we 
			// know to close it when we’re done.
			if (command.Connection.State != ConnectionState.Open) 
			{
				command.Connection.Open();				 
			}		 

			// get a data adapter  
			SQLiteDataAdapter da = new SQLiteDataAdapter((SQLiteCommand)command);
			DataSet ds = new DataSet();
			// fill the data set, and return the schema information
			da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
			da.Fill(ds);
			// convert our dataset to XML
			StringReader stream = new StringReader(ds.GetXml());			 
			command.Connection.Close();
			// convert our stream of text to an XmlReader
			return new XmlTextReader(stream);
		}

		 

		/// <summary>
		/// Parses parameter names from SQL Statement, assigns values from object array , and returns fully populated ParameterCollection
		/// </summary>
		/// <param name="commandText">Sql Statement with SQL Server style "@param" style embedded parameters only [no OleDb style "?" placeholders]</param>
		/// <param name="paramList">object[] array of parameter values</param>
		/// <returns>SQLiteParameterCollection</returns>
		/// <remarks>Status experimental. Regex appears to be handling most issues. Note that parameter object array must be in same order as parameter names appear in SQL statement.</remarks>
        public static SQLiteParameterCollectionSub DeriveParameters(string commandText, object[] paramList)
		{
            if (paramList == null) return null;
            //SQLiteParameterCollection coll = new SQLiteParameterCollection();
            SQLiteParameterCollectionSub coll = new SQLiteParameterCollectionSub();
            string parmString = commandText.Substring(commandText.IndexOf("@"));
            // pre-process the string so always at least 1 space after a comma.
            parmString = parmString.Replace(",", " ,");
            // get the named parameters into a match collection
            string pattern = @"(@)\S*(.*?)\b";
            Regex ex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection mc = ex.Matches(parmString);
            string[] paramNames = new string[mc.Count];
            int i = 0;
            foreach (Match m in mc)
            {
                paramNames[i] = m.Value;
                i++;
            }

            // now let's type the parameters
            int j = 0;
            Type t = null;
            foreach (object o in paramList)
            {
                t = o.GetType();

                SQLiteParameter parm = new SQLiteParameter();
                switch (t.ToString())
                {

                    case ("DBNull"):
                    case ("Char"):
                    case ("SByte"):
                    case ("UInt16"):
                    case ("UInt32"):
                    case ("UInt64"):
                        throw new SystemException("Invalid data type");


                    case ("System.String"):
                        parm.DbType = DbType.String;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (string)paramList[j];
                        coll.Add(parm);
                        break;

                    case ("System.Byte[]"):
                        parm.DbType = DbType.Binary;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (byte[])paramList[j];
                        coll.Add(parm);
                        break;

                    case ("System.Int32"):
                        parm.DbType = DbType.Int32;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (int)paramList[j];
                        coll.Add(parm);
                        break;

                    case ("System.Boolean"):
                        parm.DbType = DbType.Boolean;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (bool)paramList[j];
                        coll.Add(parm);
                        break;

                    case ("System.DateTime"):
                        parm.DbType = DbType.DateTime;
                        parm.ParameterName = paramNames[j];
                        parm.Value = Convert.ToDateTime(paramList[j]);
                        coll.Add(parm);
                        break;

                    case ("System.Double"):
                        parm.DbType = DbType.Double;
                        parm.ParameterName = paramNames[j];
                        parm.Value = Convert.ToDouble(paramList[j]);
                        coll.Add(parm);
                        break;

                    case ("System.Decimal"):
                        parm.DbType = DbType.Decimal;
                        parm.ParameterName = paramNames[j];
                        parm.Value = Convert.ToDecimal(paramList[j]);
                        break;

                    case ("System.Guid"):
                        parm.DbType = DbType.Guid;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (System.Guid)(paramList[j]);
                        break;

                    case ("System.Object"):

                        parm.DbType = DbType.Object;
                        parm.ParameterName = paramNames[j];
                        parm.Value = paramList[j];
                        coll.Add(parm);
                        break;

                    default:
                        throw new SystemException("Value is of unknown data type");

                } // end switch

                j++;
            }
            return coll;
            //return null;
		}

		/// <summary>
		/// Executes non query typed params from a DataRow
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="dataRow">Data row.</param>
		/// <returns>Integer result code</returns>
		public static int ExecuteNonQueryTypedParams(IDbCommand command, DataRow dataRow)
		{
			int retVal = 0;

			// If the row has values, the store procedure parameters must be initialized
			if( dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// Set the parameters values
				AssignParameterValues(command.Parameters, dataRow);
		        
				retVal =  ExecuteNonQuery(command);
			}
			else
			{
				retVal =  ExecuteNonQuery(command);
			}
			
			return retVal;
		}

		///<overloads>Assigns Parameter Values</overloads>
		/// <summary>
		/// This method assigns dataRow column values to an IDataParameterCollection
		/// </summary>
		/// <param name="commandParameters">The IDataParameterCollection to be assigned values</param>
		/// <param name="dataRow">The dataRow used to hold the command's parameter values</param>
		/// <exception cref="System.InvalidOperationException">Thrown if any of the parameter names are invalid.</exception>
		 
		protected internal static void AssignParameterValues(IDataParameterCollection commandParameters, DataRow dataRow)
		{
			if (commandParameters == null || dataRow == null)
			{
				// Do nothing if we get no data
				return;
			}

			DataColumnCollection columns = dataRow.Table.Columns;

			int i = 0;
			// Set the parameters values
			foreach(IDataParameter commandParameter in commandParameters)
			{
				// Check the parameter name
				if( commandParameter.ParameterName == null || 
					commandParameter.ParameterName.Length <= 1 )
					throw new InvalidOperationException( string.Format( 
						"Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", 
						i, commandParameter.ParameterName ) );

				if (columns.Contains( commandParameter.ParameterName ) )
					commandParameter.Value = dataRow[commandParameter.ParameterName];
				else if(columns.Contains( commandParameter.ParameterName.Substring(1) ) )
					commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
	
				i++;
			}
		}

		/// <summary>
		/// This method assigns dataRow column values to an array of IDataParameters
		/// </summary>
		/// <param name="commandParameters">Array of IDataParameters to be assigned values</param>
		/// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
		/// <exception cref="System.InvalidOperationException">Thrown if any of the parameter names are invalid.</exception>
		protected void AssignParameterValues(IDataParameter[] commandParameters, DataRow dataRow)
		{
			if ((commandParameters == null) || (dataRow == null)) 
			{
				// Do nothing if we get no data
				return;
			}

			DataColumnCollection columns = dataRow.Table.Columns;

			int i = 0;
			// Set the parameters values
			foreach(IDataParameter commandParameter in commandParameters)
			{
				// Check the parameter name
				if( commandParameter.ParameterName == null || 
					commandParameter.ParameterName.Length <= 1 )
					throw new InvalidOperationException( string.Format( 
						"Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", 
						i, commandParameter.ParameterName ) );

				if (columns.Contains( commandParameter.ParameterName ) )
					commandParameter.Value = dataRow[commandParameter.ParameterName];
				else if(columns.Contains( commandParameter.ParameterName.Substring(1) ) )
					commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
	
				i++;
			}
		}

		/// <summary>
		/// This method assigns an array of values to an array of IDataParameters
		/// </summary>
		/// <param name="commandParameters">Array of IDataParameters to be assigned values</param>
		/// <param name="parameterValues">Array of objects holding the values to be assigned</param>
		/// <exception cref="System.ArgumentException">Thrown if an incorrect number of parameters are passed.</exception>
		protected void AssignParameterValues(IDataParameter[] commandParameters, object[] parameterValues)
		{
			if ((commandParameters == null) || (parameterValues == null)) 
			{
				// Do nothing if we get no data
				return;
			}

			// We must have the same number of values as we pave parameters to put them in
			if (commandParameters.Length != parameterValues.Length)
			{
				throw new ArgumentException("Parameter count does not match Parameter Value count.");
			}

			// Iterate through the IDataParameters, assigning the values from the corresponding position in the 
			// value array
			for (int i = 0, j = commandParameters.Length, k = 0; i < j; i++)
			{
				if (commandParameters[i].Direction != ParameterDirection.ReturnValue)
				{
					// If the current array value derives from IDataParameter, then assign its Value property
					if (parameterValues[k] is IDataParameter)
					{
						IDataParameter paramInstance;
						paramInstance = (IDataParameter)parameterValues[k];
						if (paramInstance.Direction == ParameterDirection.ReturnValue)
						{
							paramInstance = (IDataParameter)parameterValues[++k];
						}
						if( paramInstance.Value == null )
						{
							commandParameters[i].Value = DBNull.Value; 
						}
						else
						{
							commandParameters[i].Value = paramInstance.Value;
						}
					}
					else if (parameterValues[k] == null)
					{
						commandParameters[i].Value = DBNull.Value;
					}
					else
					{
						commandParameters[i].Value = parameterValues[k];
					}
					k++;
				}
			}
		}
	}
}