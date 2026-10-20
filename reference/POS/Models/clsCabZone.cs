using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsCabZone
    {
        public int CabZoneID { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public int? SerialNo { get; set; }

        public int? ZoneID { get; set; }
        public string AreaCode { get; set; }
        public string Status { get; set; }
        public List<clsCabZone> AreaModelList = new List<clsCabZone>();
    }


    public class clsCabServiceProvider
    {
        public int ServiceProviderID { get; set; }
        public string ServierProviderName { get; set; }
        public decimal? DetailCode { get; set; }
        public string DetailName { get; set; }
        public string Status { get; set; }
    }
}