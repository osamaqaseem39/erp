using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class Cabcategoryfare
    {
        public int FareId { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public Nullable<decimal> Uptomile { get; set; }
        public Nullable<decimal> Fare { get; set; }
    }
}