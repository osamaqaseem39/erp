using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsSalary
    {
        public int SalaryId { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? ForMonth { get; set; }
        public DateTime? RecordDate { get; set; }
        public string Remarks { get; set; }
        public DateTime? SaveDateTime { get; set; }
        public DateTime? ChangeDateTime { get; set; }
        public string Status { get; set; }
        public int? TotalPresent { get; set; }
        public int? TotalAbsent { get; set; }
        public int? TotalLeaves { get; set; }
        public int? TotalHolidays { get; set; }
        public int? TotalWorkingDays { get; set; }
        public decimal? GrossPay { get; set; }
        public decimal? Advance { get; set; }
        public decimal? AdvanceDeduction { get; set; }
        public decimal? Loan { get; set; }
        public decimal? LoanDeduction { get; set; }
        public decimal? AddAdjustment { get; set; }
        public decimal? LessDeduction { get; set; }
        public decimal? NetSalary { get; set; }
        public int? UserId { get; set; }
        public int TotalDaysOfMonth { get; set; }
        public string MonthYear { get; set; }
        public string ReportType { get; set; }
    }
}