using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{


    public class clsCabTimePrice
    {
        public int TimePriceID { get; set; }
        public TimeSpan? FromTime { get; set; }
        public TimeSpan? ToTime { get; set; }
        public decimal? PriceAdjustPers { get; set; }
        public string Status { get; set; }

        public string FromTimeString { get; set; }
        public string ToTimeString { get; set; }
    }


}