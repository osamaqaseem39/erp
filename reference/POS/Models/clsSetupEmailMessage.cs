using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{


    public class clsSetupEmailMessage
    {
        public int EmailMessageID { get; set; }
        public string Description { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public int? ToCustomer { get; set; }
    }


}