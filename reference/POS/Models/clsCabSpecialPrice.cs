using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{


    public class clsCabSpecialPrice
    {
        public int SpecialPriceID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? PriceAdjustPers { get; set; }
        public string Status { get; set; }
    }


}