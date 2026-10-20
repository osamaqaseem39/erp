using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class Cabcategory
    {
        public int CabCategoryID { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public Cabcategoryfare categoryfare { get; set; }
        public int FareId { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public Nullable<decimal> Uptomile { get; set; }
        public Nullable<decimal> Fare { get; set; }
            

    }
}