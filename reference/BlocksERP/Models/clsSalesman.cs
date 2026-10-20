using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsSalesman
    {
        public int SalesmanId { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Cell1 { get; set; }
        public string Cell2 { get; set; }
        public string Email { get; set; }
        public string Remarks { get; set; }
        public string CNIC { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }
        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }

        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }

    }
}