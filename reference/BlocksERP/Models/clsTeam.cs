using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsTeam
    {
        public int TeamID { get; set; }
        public string TeamName { get; set; }
        public int? TeamLeaderUserID { get; set; }
        public string TeamLeaderUserName { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }


        public int? SerialNo { get; set; }
        public int? MemberUserID { get; set; }
        public string MemberUserName { get; set; }
        public int? SequenceNo { get; set; }


        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string UpdatedByString { get; set; }

        public List<clsTeam> detaillist = new List<clsTeam>();

        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }

    }

}

