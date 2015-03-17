namespace XWSJ.DataBase
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Configuration;

    public class SQLServerHelper2
    {
        public static string dbServer = "";
        public static string dbUser = "";
        public static string dbPwd = "";
        public static string url = "";
        private static string conStr = "Data Source=.\\SQL2008;Initial Catalog=wyxt;User ID=sa;Password=123;Max Pool Size = 512";//ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        public static void AddParamInCmd(SqlCommand cmd, string paramName, SqlDbType type, int size, object value)
        {
            SqlParameter param = new SqlParameter {
                ParameterName = paramName,
                SqlDbType = type,
                Size = size,
                Value = value
            };
            cmd.Parameters.Add(param);
        }

        private static SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }

        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection) {
                CommandType = CommandType.StoredProcedure
            };
            foreach (SqlParameter parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }

        public static int ExecuteNonQuery(SqlTransaction transaction, string commandText, params SqlParameter[] commandParameters)
        {
            SqlCommand objCommand = new SqlCommand();
            PrepareCommand(objCommand, transaction.Connection, transaction, commandText, commandParameters);
            int iNum1 = objCommand.ExecuteNonQuery();
            objCommand.Parameters.Clear();
            return iNum1;
        }

        public static SqlDataReader ExecuteReader(string sqlString)
        {
            SqlDataReader sdr;
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(sqlString, connection);
            SqlDataReader myReader = null;
            try
            {
                connection.Open();
                myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                sdr = myReader;
            }
            catch (SqlException e)
            {
                connection.Close();
                throw new Exception(e.Message);
            }
            finally
            {
                if (myReader == null)
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            return sdr;
        }

        public static SqlDataReader ExecuteReader(string sqlString, params SqlParameter[] cmdParms)
        {
            SqlDataReader sdr;
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader myReader = null;
            try
            {
                PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                sdr = myReader;
            }
            catch (SqlException e)
            {
                connection.Close();
                throw new Exception(e.Message);
            }
            finally
            {
                if (myReader == null)
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            return sdr;
        }

        public static int ExecuteSql(string sqlString)
        {
            int sdr;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        return cmd.ExecuteNonQuery();
                    }
                    catch (SqlException E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
            return sdr;
        }

        public static int ExecuteSql(string sqlString, string content)
        {
            int sdr;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(sqlString, connection);
                SqlParameter myParameter = new SqlParameter("@content", SqlDbType.NText) {
                    Value = content
                };
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    sdr = cmd.ExecuteNonQuery();
                }
                catch (SqlException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            return sdr;
        }

        public static int ExecuteSql(string sqlString, params SqlParameter[] cmdParms)
        {
            int sdr;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (SqlException E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
            return sdr;
        }

        public static int ExecuteSqlInsertImg(string sqlString, byte[] fs)
        {
            int sdr;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(sqlString, connection);
                SqlParameter myParameter = new SqlParameter("@fs", SqlDbType.Image) {
                    Value = fs
                };
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    sdr = cmd.ExecuteNonQuery();
                }
                catch (SqlException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            return sdr;
        }

        public static void ExecuteSqlTran(ArrayList sqlStringList)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand {
                    Connection = conn
                };
                SqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < sqlStringList.Count; n++)
                    {
                        string sqlString = sqlStringList[n].ToString();
                        if (sqlString.Trim().Length > 1)
                        {
                            cmd.CommandText = sqlString;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (SqlException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    conn.Close();
                }
            }
        }

        public static void ExecuteSqlTran(Hashtable sqlStringList)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        foreach (DictionaryEntry myDE in sqlStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[]) myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            trans.Commit();
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        public static bool Exists(string sqlString, params SqlParameter[] cmdParms)
        {
            int cmdResult;
            object obj = GetSingle(sqlString, cmdParms);
            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                cmdResult = 0;
            }
            else
            {
                cmdResult = int.Parse(obj.ToString());
            }
            if (cmdResult == 0)
            {
                return false;
            }
            return true;
        }

        public static int GetMaxID(string fieldName, string tableName)
        {
            object obj = GetSingle("select max(" + fieldName + ")+1 from " + tableName);
            if (obj == null)
            {
                return 1;
            }
            return int.Parse(obj.ToString());
        }

        public static object GetSingle(string sqlString)
        {
            object sdr;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
                        {
                            return null;
                        }
                        return obj;
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
            return sdr;
        }

        public static object GetSingle(string sqlString, params SqlParameter[] cmdParms)
        {
            object sdr;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
                        {
                            return null;
                        }
                        return obj;
                    }
                    catch (SqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
            return sdr;
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
            {
                cmd.Transaction = trans;
            }
            cmd.CommandType = CommandType.Text;
            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                {
                    cmd.Parameters.Add(parm);
                }
            }
        }

        public static DataSet Query(string sqlString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    new SqlDataAdapter(sqlString, connection).Fill(ds, "ds");
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                
                return ds;
            }
        }

        public static DataSet Query(string sqlString, params SqlParameter[] cmdParms)
        {
            DataSet sdr;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                    sdr = ds;
                }
            }
            return sdr;
        }

        public static SqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            SqlDataReader sdr;
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
                command.CommandType = CommandType.StoredProcedure;
                sdr = command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return sdr;
        }

        public static int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            int sdr;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                    rowsAffected = command.ExecuteNonQuery();
                    int result = (int) command.Parameters["ReturnValue"].Value;
                    sdr = result;
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            return sdr;
        }

        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                new SqlDataAdapter { SelectCommand = BuildQueryCommand(connection, storedProcName, parameters) }.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }

        public static SqlConnection Connection
        {
            get
            {
                SqlConnection conn = new SqlConnection(connectionString);
                if (conn == null)
                {
                    conn.Open();
                    return conn;
                }
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                    return conn;
                }
                if (conn.State == ConnectionState.Broken)
                {
                    conn.Close();
                    conn.Open();
                }
                return conn;
            }
        }

        private static string connectionString
        {
            get
            {
                return "Data Source="+dbServer+";Initial Catalog=wyxt;User ID="+dbUser+";Password="+dbPwd+";Max Pool Size = 512";
            }
        }
    }
}

