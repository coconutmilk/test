namespace XWSJ.DataBase
{
    using XWSJ.Common;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;

    public class DataHelper2
    {
        public static string GetContent(string table, string value)
        {
            return GetContent(table, "code1", "content2", value);
        }

        public static string GetContent(string table, string code, string content, string codevalue)
        {
            return StringHelper.ToString(SQLServerHelper.GetSingle("select " + content + " from " + table + " where " + code + "='" + codevalue + "'"));
            
        }
        public static string GetContent2(string table, string code, string content, string codevalue)
        {
            int t;
            try
            {
                t = Convert.ToInt322(codevalue);
            }
            catch
            {
                t=0;
            }
            return StringHelper.ToString(SQLServerHelper.GetSingle("select " + content + " from " + table + " where " + code + "=" + t));
        }
        public static DataTable GetDataTable()
        {
            using (SqlConnection con = SQLServerHelper.Connection)
            {
                return con.GetSchema("Tables");
            }
        }

        public DataTable GetDataTable(string tablename)
        {
            StringBuilder sqlstr = new StringBuilder();
            sqlstr.Append("SELECT A.name AS col, C.name AS type, A.length");
            sqlstr.Append(" FROM syscolumns A LEFT OUTER JOIN  sysobjects B ON A.id = B.id LEFT OUTER JOIN");
            sqlstr.Append(" systypes C ON A.xtype = C.xtype");
            sqlstr.Append(" WHERE (B.name ='" + tablename + "')");
            return SQLServerHelper.Query(sqlstr.ToString()).Tables[0];
        }

        public static DataTable GetDataTable(string sql, params SqlParameter[] values)
        {
            SqlConnection con = SQLServerHelper.Connection;
            DataSet ds = new DataSet();
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddRange(values);
            new SqlDataAdapter(cmd).Fill(ds);
            con.Close();
            con.Dispose();
            return ds.Tables[0];
        }

        public static DataTable GetDataTableBySql(string sql)
        {
            SqlConnection con = SQLServerHelper.Connection;
            DataSet ds = new DataSet();
            SqlCommand cmd = new SqlCommand(sql, con);
            new SqlDataAdapter(cmd).Fill(ds);
            con.Close();
            con.Dispose();
            return ds.Tables[0];
        }
    }
}

