using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using BlocksERP.Models;

namespace BlocksERP.Models
{
    public class clsSqlErrorLog
    {
        public static string InsertError(int? userId, string errorDescription, string formName)
        {
            string vRes = "OK";
            int statusId = 0;
            //try
            //{
            //    errorDescription = GetValue(errorDescription);
            //    string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            //    SqlConnection con = new SqlConnection(connection);
            //    con.Open();
            //    SqlCommand cmd = new SqlCommand();
            //    cmd.Connection = con;

            //    cmd.CommandType = System.Data.CommandType.Text;
            //    cmd.CommandText = "insert into tblErrorsLog(ErrorLineNumber,ErrorDescription,ErrorSource,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy) values('Code Exception','" + errorDescription + "','" + formName + "',CURRENT_TIMESTAMP," + userId + ",CURRENT_TIMESTAMP," + userId + ")";
            //    cmd.ExecuteNonQuery();

            //    con.Close();
            //    SqlConnection.ClearPool(con);
            //    vRes = "OK";
            //}
            //catch (Exception ex)
            //{
            //    vRes = "False";
            //}
            return vRes;
        }
        public static string GetValue(string value)
        {
            value = value.Replace("'", "''");
            return value;
        }
    }
}