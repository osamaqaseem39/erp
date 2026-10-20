using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BlocksERP.Models;
using BlocksERP.Models.DAL.Security;
using BlocksERP.Controllers;
using System.Web.Security;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Linq.Dynamic;
using System.Data;

namespace BlocksERP.Areas.Loan.Controllers
{
    public class LoanDashboardController : BaseController
    {
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult AsideMenu()
        {
            List<clsModule> moduleList = new List<clsModule>();
            moduleList = (from r in db.tblUserMenuRoles
                          join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                          join a in db.tblMenuActions on r.ActionID equals a.ActionID
                          where r.UserID == User.UserID
                          orderby m.ModuleName
                          select new clsModule
                          {
                              ModuleID = m.ModuleID,
                              ModuleName = m.ModuleName,
                          }).ToList();
            return PartialView(moduleList);
        }

        public ActionResult Index()
        {

            if (User != null)
            {
                clsModule moduleListMain = new clsModule();

                List<clsModule> moduleList = new List<clsModule>();
                moduleList = (from r in db.tblUserMenuRoles
                              join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                              join a in db.tblMenuActions on r.ActionID equals a.ActionID
                              where r.UserID == User.UserID && a.ActionName.ToLower() == "view"
                              orderby m.ModuleName
                              select new clsModule
                              {
                                  ModuleID = m.ModuleID,
                                  ModuleName = m.ModuleName,
                              }).ToList();

                moduleListMain.ModuleList = moduleList;

                return View(moduleListMain);
            }
            else
            {

                return RedirectToAction("login", "account", new { area = "" });
            }
        }


    }
}