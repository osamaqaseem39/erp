using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsWarehouse
    {
        public int WarehouseId { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseAbb { get; set; }
        public string Address { get; set; }

        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Cell1 { get; set; }
        public string Cell2 { get; set; }

        public string Email { get; set; }
        public string WarehouseIncharge { get; set; }

       
        public bool? Status { get; set; }
        public string StatusString { get; set; }
        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
    }
}