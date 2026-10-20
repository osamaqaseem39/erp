using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{


    public class clsCabServicePause
    {
        public int PauseID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public TimeSpan? FromTime { get; set; }
        public TimeSpan? ToTime { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }

        public string FromTimeString { get; set; }
        public string ToTimeString { get; set; }
    }


}