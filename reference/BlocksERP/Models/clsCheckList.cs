using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsCheckList
    {
        public int CheckListID { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public string CheckListName { get; set; }
        public string CheckListDescription { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }

        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedByString { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }

    }
}