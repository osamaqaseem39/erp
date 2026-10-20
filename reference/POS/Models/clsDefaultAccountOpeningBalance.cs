using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsDefaultAccountOpeningBalance
    {
        public string NewOrEdit { get; set; }
        public decimal? DetailCode { get; set; }
        public string DetailName { get; set; }
        public int? FiscalYearID { get; set; }
        public int? BusinessID { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal? CurrentBalance { get; set; }
    }
}