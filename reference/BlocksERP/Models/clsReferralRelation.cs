using System;

namespace BlocksERP.Models
{
    public class clsReferralRelation
    {
        public int ReferralRelationID { get; set; }
        public string RelationName { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }
        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }

    }
}