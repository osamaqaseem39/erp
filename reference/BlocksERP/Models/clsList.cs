using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsList
    {
        public int id { get; set; }
        public string text { get; set; }
    }

    public class clsStringList
    {
        public string id { get; set; }
        public string text { get; set; }
    }

    public class clsCashList
    {
        public string id { get; set; }
        public string text { get; set; }
        public string Cashid { get; set; }
    }
}