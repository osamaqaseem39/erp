using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class CabInfo
    {
        public int CabID { get; set; }
        public string VehicleNo { get; set; }
        public Nullable<int> CarTypeID { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string EngineNo { get; set; }
        public string ChassisNo { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> ChildSeat { get; set; }
        public Nullable<int> DisableSeat { get; set; }
        public string OwnerType { get; set; }
        public string OwnerName { get; set; }
        public Nullable<int> SupplierID { get; set; }
    }
}