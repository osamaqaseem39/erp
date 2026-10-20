using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsCategory
    {
        public int CategoryId { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public string CategoryName { get; set; }
        public int? InventoryDetailId { get; set; }
        public string InventoryDetailName { get; set; }
        public int? IncomeDetailId { get; set; }
        public string IncomeDetailName { get; set; }
        public int? COSDetailId { get; set; }
        public string COSDetailName { get; set; }
        public string Type { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedByString { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
    }
}