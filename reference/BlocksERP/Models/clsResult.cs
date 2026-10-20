using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsResult
    {
        public int Id { get; set; }
        public string ComputerNo { get; set; }
        public string ResultMessage { get; set; }
        public bool? status { get; set; }
    }
}