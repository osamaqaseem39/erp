using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsArea
    {
        public int AreaId { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public int? TownId { get; set; }
        public string TownName { get; set; }
        public int? DistributorId { get; set; }
        public string DistributorName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public int? Status { get; set; }
        public string ManualCode { get; set; }
        public string IsNeWOrEdit { get; set; }
        public string StatusString { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedByString { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }

    }
}