using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BlocksERP.Models;

namespace BlocksERP.Models
{
    public class Select2PagedResult
    {
        public int Total { get; set; }
        public List<clsList> Results { get; set; }
    }

    public class Select2PagedStringResult
    {
        public int Total { get; set; }
        public List<clsStringList> Results { get; set; }
    }
}