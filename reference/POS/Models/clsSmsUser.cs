using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsSmsUser
    {
        public int SmsUserId { get; set; }
        public string Name { get; set; }
        public string CellNo { get; set; }
        public string Status { get; set; }
    }
}