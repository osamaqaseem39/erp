using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsCabType
    {
        public int CabTypeID { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public int? SeatCapacity { get; set; }
        public int? SerialNo { get; set; }
        public decimal? PricePers { get; set; }
        

        public decimal? UptoKm { get; set; }
        public decimal? FarePrice { get; set; }
        public string CarModel { get; set; }
        public string Status { get; set; }
        public string ImageName { get; set; }
        

        public List<clsCabType> FareList = new List<clsCabType>();
        public List<clsCabType> CarModelList = new List<clsCabType>();
    }

    public class clsFirebase
    {
        public bool status { get; set; }
        public string message { get; set; }
    }
}