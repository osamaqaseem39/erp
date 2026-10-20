using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsUserRight
    {
        public string MenuName { get; set; }
        public int UserID { get; set; }
        public int? showMain { get; set; }
        public int? showSave { get; set; }
        public int? showChange { get; set; }
        public int? showDelete { get; set; }
    }
}