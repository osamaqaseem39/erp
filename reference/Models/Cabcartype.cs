using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class Cabcartype
    {
        public int CabTypeID { get; set; }
        public string CabTypeIDStr { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> ManufactureId { get; set; }
        public Nullable<int> SeatingCapacity { get; set; }
        public Nullable<int> Disableseat { get; set; }
        public Nullable<int> Childseat { get; set; }
        public Nullable<decimal> Luggagekg { get; set; }
        public Nullable<int> CabcategoryId { get; set; }
        public Nullable<decimal> Estperlitermilage { get; set; }
        public string Status { get; set; }
        public Nullable<decimal> Fare { get; set; }
        public Nullable<decimal> ReturnFare { get; set; }
        public Nullable<decimal> UptoKm { get; set; }

        public string imageName { get; set; }
        public int? LuggageID { get; set; }

        public bool? ServicePause { get; set; }
        public string ServicePauseMessage { get; set; }
    }
}