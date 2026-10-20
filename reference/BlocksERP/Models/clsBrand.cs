using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsBrand
    {
        public int BrandId { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public string BrandName { get; set; }
        public string BrandDescription { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedByString { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }

    }
}