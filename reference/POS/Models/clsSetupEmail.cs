using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{


    public class clsSetupEmail
    {
        public int EmailID { get; set; }
        public string Description { get; set; }
        public string Password { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; }
        public decimal DetailCode { get; set; }
        public string Name { get; set; }
    }


}