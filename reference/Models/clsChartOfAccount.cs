using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class clsChartOfAccount
    {
        public decimal AccountCode { get; set; }
        public string AccountCodeV { get; set; }
        public string RefKey { get; set; }
        public string Description { get; set; }
        public int? AcLevel { get; set; }
        public string Status { get; set; }
        public string ReportType { get; set; }
        public string Remarks { get; set; }
        public int? CombineAC { get; set; }
        public int? BusinessID { get; set; }
        public string IsNeWOrEdit { get; set; }
        public string FirstLevelAccountCode { get; set; }
        public string SecondLevelAccountCode { get; set; }
        public string ThirdLevelAccountCode { get; set; }
        public decimal? TotalBalance { get; set; }
        public string DetailName { get; set; }
        public string LineDetailName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}