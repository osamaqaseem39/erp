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
    public class HomeController : BaseController
    {
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult MainMenu()
        {
            if (User != null)
            {
                //if (menurights.MenuRights(User.UserID, "frmMainMenu", "View") != 0)
                //{
                //&& m.SectionName.ToLower() == ("Menu").ToLower()

                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  orderby m.ModuleName
                                  select new clsModule
                                  {
                                      ModuleID = m.ModuleID,
                                      ModuleName = m.ModuleName,
                                  }).ToList();

                    return View(actionList);
                //}
                //else
                //{
                //    return RedirectToAction("accessdenied", "home");
                //}
            }
            else
            {

                return RedirectToAction("login", "account", new { area = "" });
            }
        }
        public ActionResult AccessDenied()
        {

            if (User != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("login", "account", new { area = "" });
            }
        }

        public ActionResult Index()
        {

            if (User != null)
            {
                //clsSqlUser role = new clsSqlUser();
                ////Check if user has access of this module or not
                //if (role.CheckModuleRoleId(User.UserId, ("MainUser").ToLower()) > 0)
                //{
                //    List<clsModule> actionList = new List<clsModule>();
                //    actionList = role.ActionsList(User.UserId, ("MainUser").ToLower());
                //    return View();
                //}
                //else
                //{
                    return RedirectToAction("accessdenied", "home");
                //}
            }
            else
            {
             
                return RedirectToAction("login", "account", new { area = "" });
            }
        }
    }
}