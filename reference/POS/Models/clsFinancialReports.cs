using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsFinancialReports
    {
        public decimal? DetailCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? ByDate { get; set; }
        public string FirstLevelAccountCode { get; set; }
        public string SecondLevelAccountCode { get; set; }
        public string ThirdLevelAccountCode { get; set; }
        public int Permanent { get; set; }
        public int? CityId { get; set; }
        public int? SalesmanID { get; set; }
        public string SalesmanName { get; set; }
    }
}