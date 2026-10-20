using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsACDetailSub
    {
        public int SubDetailCode { get; set; }
        public Nullable<decimal> DetailCode { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Cell1 { get; set; }
        public string Cell2 { get; set; }
        public string Email { get; set; }
        public string Remarks { get; set; }
        public string UrduName { get; set; }
        public string Status { get; set; }


        public int id { get; set; }
        public string text { get; set; }
    }


    public class Select2PagedSubDetailResult
    {
        public int Total { get; set; }

        public List<clsACDetailSub> Results { get; set; }
    }
}