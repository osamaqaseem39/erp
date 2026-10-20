using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{


    public class clsCabLocationCharges
    {
        public int LocationChargesID { get; set; }
        public string PostCode { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
    }


}