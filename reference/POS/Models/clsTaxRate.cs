using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsTaxRate
    {
        public int TaxID { get; set; }
        public string TaxName { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? DetailCode { get; set; }
        public string DetailName { get; set; }
        public string Status { get; set; }
        public string IsNeWOrEdit { get; set; }
    }
}