using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Security.Principal;
namespace BlocksERP.Models.DAL.Security
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

        public int? UserID { get; set; }
        public string Name { get; set; }
        public string[] roles { get; set; }
        public int View { get; set; }
        public int Save { get; set; }
        public int Change { get; set; }
        public int Delete { get; set; }
        public string Email { get; set; }
        public int FiscalYearID { get; set; }
        public string modulename { get; set; }
        public string UserImage { get; set; }
        public int IsSalesman { get; set; }
        public int SalesmanID { get; set; }

        public int DefaultWarehouseID { get; set; }
        public string DefaultWarehouseName { get; set; }

        public int DistributorID { get; set; }
        public string DistributorName { get; set; }



    }

    public class CustomPrincipalSerializeModel
    {
        public int? UserID { get; set; }
        public string Name { get; set; }
        public string[] roles { get; set; }
        public int View { get; set; }
        public int Save { get; set; }
        public int Change { get; set; }
        public int Delete { get; set; }
        public string Email { get; set; }
        public int FiscalYearID { get; set; }
        public int IsSalesman { get; set; }
        public int SalesmanID { get; set; }
        public string modulename { get; set; }
        public string UserImage { get; set; }

        public int DefaultWarehouseID { get; set; }
        public string DefaultWarehouseName { get; set; }
        public int DistributorID { get; set; }
        public string DistributorName { get; set; }

    }
}