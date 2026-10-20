using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class clsUser
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int? Multi { get; set; }
        public int? BusinessID { get; set; }
        public int ChangePassword { get; set; }
        public string MenuName { get; set; }
        public string MenuGroup { get; set; }
        public string MenuTitle { get; set; }
        public int? showMain { get; set; }
        public int? showSave { get; set; }
        public int? showChange { get; set; }
        public int? showDelete { get; set; }
    }
}