using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsSalaryPayment
    {
        public string NewOrEdit { get; set; }
        public string PaymentNo { get; set; }
        public DateTime? RecordDate { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Remarks { get; set; }
        public decimal? PaidFromDetailCode { get; set; }
        public string DetailName { get; set; }
        public decimal? Amount { get; set; }
        public int? SalaryId { get; set; }
        public string Status { get; set; }
        public decimal? TotalSalary { get; set; }
        public decimal? Paid { get; set; }
        public decimal? Balance { get; set; }
        public string MonthYear { get; set; }
        public DateTime? ForMonth { get; set; }
        public string ReportType { get; set; }
    }
}