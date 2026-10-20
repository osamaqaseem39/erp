using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{


    public class clsSetupContentFAQ
    {
        public int ContentID { get; set; }
        public string Description { get; set; }
        public string Question { get; set; }
        public int? SerialNo { get; set; }
        public string Status { get; set; }
    }


}