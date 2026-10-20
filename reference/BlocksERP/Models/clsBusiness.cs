using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsBusiness
    {
        public int BusinessId { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public string BusinessName { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public string CellNo { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string ContactPerson { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }
        public int? DocumentId { get; set; }
        public string ImagePath { get; set; }
        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
    }
}