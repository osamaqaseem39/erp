using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public static class clsSqlHistoryLookup
    {
        public static DataTableReader getInvoicesList(string whereCondition, string start, string length)
        {
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
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
            int voffset = Convert.ToInt32(start) * Convert.ToInt32(length);
            cmd.CommandText = "SELECT RecordDate,ItemName,DetailName,Qty,Rate,GrossValue,UrduName,Barcode,CategoryName,BrandName FROM View_Invoices where " + whereCondition + "   " + " order by RecordDate desc,ComputerNo desc OFFSET " + voffset + " ROWS  FETCH NEXT " + length + " ROWS ONLY";
            cmd.Connection = con;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            con.Close();
            SqlConnection.ClearPool(con);
            return tdt.CreateDataReader();
        }

        public static DataTableReader getInvoicesListCount(string whereCondition)
        {
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;

            cmd.CommandText = "SELECT Count(CompositKey) as MyRowCount FROM View_Invoices where " + whereCondition;
            cmd.Connection = con;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            con.Close();
            SqlConnection.ClearPool(con);
            return tdt.CreateDataReader();
        }


        public static DataTableReader getInvoicesListForPrint(string whereCondition)
        {
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "SELECT RecordDate,ItemName,DetailName,Qty,Rate,GrossValue,UrduName,Barcode,CategoryName,BrandName FROM View_Invoices where " + whereCondition + "";
            cmd.Connection = con;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            con.Close();
            SqlConnection.ClearPool(con);
            return tdt.CreateDataReader();
        }
    }
}