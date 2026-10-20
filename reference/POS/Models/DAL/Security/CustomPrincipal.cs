using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Security.Principal;
namespace POS.Models.DAL.Security
{
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }
        public bool IsInRole(string role)
        {
            if (roles.Any(r => role.Contains(r)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public CustomPrincipal(string Username)
        {
            this.Identity = new GenericIdentity(Username);
        }

        public int? UserId { get; set; }
        public string Name { get; set; }
        public string[] roles { get; set; }
        public int Main { get; set; }
        public int Save { get; set; }
        public int Change { get; set; }
        public int Delete { get; set; }
        public int FiscalYearId { get; set; }
        public int? WarehouseId { get; set; }
    }

    public class CustomPrincipalSerializeModel
    {
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string[] roles { get; set; }
        public int Main { get; set; }
        public int Save { get; set; }
        public int Change { get; set; }
        public int Delete { get; set; }
        public int FiscalYearId { get; set; }
        public int? WarehouseId { get; set; }
    }
}