using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace BlocksERP.Models
{
  
    public class clsModule
    {
        public int ModuleID { get; set; }
        public string Id { get; set; }
        public string ModuleName { get; set; }
        public string SectionName { get; set; }
        public string DisplayName { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public int ActionID { get; set; }
      
        public string ActionName { get; set; }
        public string AllActions { get; set; }
        public string AssignID { get; set; }
        public int UserId { get; set; }
        public string UserRoleId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string UpdatedByString { get; set; }
        public string Status { get; set; }
        public List<clsModule> ModuleList = new List<clsModule>();
        public List<clsModule> ActionList = new List<clsModule>();
        public List<clsModule> menucategoryList = new List<clsModule>();

        public int? DetailCode { get; set; }
    }
}