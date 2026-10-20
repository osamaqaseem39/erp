using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsCostCenter
    {
        public int CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public string Remarks { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string UpdatedByString { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
    }
}