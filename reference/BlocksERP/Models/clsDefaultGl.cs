using System;

namespace BlocksERP.Models
{
    public class clsDefaultGl
    {
        public int DefaultId { get; set; }

        public string Type { get; set; }

        public int AccountCode { get; set; }



        public string ThirdLevelAccountCode { get; set; }

        public int? ParentLevelCode { get; set; }
        public string ParentLevelDescription { get; set; }

        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }

        public string CreatedByString { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        public string UpdatedByString { get; set; }

        public int DeleteRoleId { get; set; }
        public int UpdatedRoleId { get; set; }

    }
}