using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BlocksERP.Models;
using BlocksERP.Models.DAL.Security;
using System.Web.Security;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Linq.Dynamic;
using System.Data;

namespace BlocksERP.Controllers
{
    public class MenuRightsController : Controller
    {
        ModelDb db = new ModelDb();

        public int MenuRights(int? UserID,string menuname,string actionname)
        {
            int vright = (from r in db.tblUserMenuRoles
                          join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                          join a in db.tblMenuActions on r.ActionID equals a.ActionID
                          where r.UserID == UserID 
                          && m.ModuleName.ToLower() == menuname.ToLower()
                          && a.ActionName.ToLower() == actionname.ToLower()
                          select r.ModuleID).FirstOrDefault() ?? 0;
            return vright;
        }
    }
}