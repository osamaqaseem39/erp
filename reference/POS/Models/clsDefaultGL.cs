using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsDefaultGL
    {
        public int DefaultID { get; set; }
        public string Type { get; set; }
        public decimal? AccountCode { get; set; }
        public decimal? DetailCode { get; set; }
        public int? BusinessID { get; set; }
        public int? DefualtSet { get; set; }
        public string ThirdLevelAccountCode { get; set; }
        public string Reference { get; set; }
    }
}