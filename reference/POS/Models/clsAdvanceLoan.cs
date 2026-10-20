using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsAdvanceLoan
    {
        public string IsNewOrEdit { get; set; }
        public string ComputerNo { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Type { get; set; }
        public DateTime? RecordDate { get; set; }
        public decimal? Amount { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public string ApprovedBy { get; set; }
        public decimal? DetailCode { get; set; }
        public decimal? PaidFromDetailCode { get; set; }
        public string PaidFromDetailName { get; set; }
    }
}