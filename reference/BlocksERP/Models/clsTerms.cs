using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BlocksERP.Models;

namespace BlocksERP.Models
{
    public class clsTerms
    {
        public int TermsId { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }

        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedByString { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }

    }
}