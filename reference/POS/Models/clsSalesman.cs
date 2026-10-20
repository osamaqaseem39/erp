using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsSalesman
    {
        public int SalesmanID { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Cell1 { get; set; }
        public string Cell2 { get; set; }
        public string Email { get; set; }
        public string Remarks { get; set; }
        public string CNIC { get; set; }
        public string Status { get; set; }
    }
}