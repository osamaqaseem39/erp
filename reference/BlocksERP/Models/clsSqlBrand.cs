using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace BlocksERP.Models
{
    public class clsSqlBrand
    {

        ModelDb db = new ModelDb();
        #region Admin Email
        public static DataTableReader getAdminEmailList(string whereCondition, string start, string length, string sorting)
        {
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            if (string.IsNullOrEmpty(start))
            {
                start = "0";
            }
            if (string.IsNullOrEmpty(length))
            {
                length = "0";
            }
            int voffset = (Convert.ToInt32(start) / 10) * Convert.ToInt32(length);
            cmd.CommandText = "  select s.AdminEmailID,isnull(s.AdminEmail,'') as 'AdminEmail',ISNULL(s.Type,'') as 'Type',(case when ISNULL(s.Status,0) = 0 then 'In-Active' else 'Active' end) as 'StatusString' , isnull(s.CreatedDate,'') as 'CreatedDate',   (case when isnull(s.CreatedBy,0) = 0 then '' else isnull((select isnull(u.Email,'') from tblUsers u where u.UserId = s.CreatedBy) , 'Record Deleted')End) as 'CreatedBy', isnull(s.UpdatedDate,'') as  'UpdatedDate', (case when isnull(s.UpdatedBy,0) = 0 then '' else isnull((select isnull(u.Email,'')  from tblUsers u where u.UserId = s.UpdatedBy) ,'Record Deleted')End) as 'UpdatedBy' from tblAdminEmail s  where" + whereCondition + " " + sorting + " OFFSET " + voffset + " ROWS  FETCH NEXT " + length + " ROWS ONLY";
            cmd.Connection = con;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            con.Close();
            SqlConnection.ClearPool(con);
            return tdt.CreateDataReader();
        }

        public static DataTableReader getAdminEmailListCount(string whereCondition)
        {
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "select Count(s.AdminEmailID) MyRowCount from tblAdminEmail s where" + whereCondition;
            cmd.Connection = con;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            con.Close();
            SqlConnection.ClearPool(con);
            return tdt.CreateDataReader();
        }
        #endregion
        
    }
}