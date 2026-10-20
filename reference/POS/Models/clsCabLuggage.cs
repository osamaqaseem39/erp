using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    

    public class clsCabLuggage
    {
        public int LuggageID { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int BookingID { get; set; }
    }


}