using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsWarehouse
    {
        public int WarehouseID { get; set; }
        public int? BusinessID { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseAbb { get; set; }
        public string Address { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Cell1 { get; set; }
        public string Cell2 { get; set; }
        public string Email { get; set; }
        public string WarehouseIncharge { get; set; }
        public string Status { get; set; }
    }
}