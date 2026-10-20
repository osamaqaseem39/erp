using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{


    public class clsSetupContentBlogs
    {
        public int ContentID { get; set; }
        public string Description { get; set; }
        public string Subject { get; set; }
        public int? SerialNo { get; set; }
        public string Status { get; set; }

        public DateTime? BlogDate { get; set; }
        public string BlogUser { get; set; }
    }


}