using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsBDEmployee
    {
        public int BDEmployeeID { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Contact1 { get; set; }
        public string Contact2 { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        //public string TypeName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
        
        public bool? IsActive { get; set; }
        public string StatusString { get; set; }
        public string StatusStringDummyUser { get; set; }
        public int IsChangePassword { get; set; }

        public int? DocumentId { get; set; }
        public string ImagePath { get; set; }

        public bool? IsDummyUser { get; set; }

        public string emailname { get; set; }

        public string emailmail { get; set; }

        public string emailmessage { get; set; }

        public int UnReadMessagesCount { get; set; }

        public int TotalUnReadMessagesCount { get; set; }

        public List<clsBDEmployee> UsersList = new List<clsBDEmployee>();

    }
}