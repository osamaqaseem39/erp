using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;

namespace POS.Controllers
{
    public class ReportsController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult ReportsIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "Report"
                          select new clsUserRight
                          {
                              showMain = r.showMain,
                              showChange = r.showChange,
                              showSave = r.showSave,
                              showDelete = r.showDelete
                          }).FirstOrDefault();
                if (rights == null)
                {
                    User.Main = 0;
                    User.Change = 0;
                    User.Save = 0;
                    User.Delete = 0;
                }
                else
                {
                    User.Main = Convert.ToInt32(rights.showMain);
                    User.Change = Convert.ToInt32(rights.showChange);
                    User.Save = Convert.ToInt32(rights.showSave);
                    User.Delete = Convert.ToInt32(rights.showDelete);
                }
                if (User.Main == 0)
                {
                    return RedirectToAction("index", "home");
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        #region Calculate Average
        public ActionResult CalculateAverage()
        {
            if (User != null)
            {
                clsFinancialReports calculate = new clsFinancialReports();
                calculate.ByDate = db.Item_CostDateLog.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.CloseDate).Max();
                if (calculate.ByDate == null)
                {
                    calculate.ByDate = DateTime.Now;
                }
                return PartialView(calculate);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult CalculateAveragePost(clsFinancialReports reminder)
        {
            string message = "";
            bool status = false;
            //System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            db.spCalculateAverage(reminder.FromDate, reminder.ToDate, User.FiscalYearId, reminder.Permanent);
            return new JsonResult { Data = new { status = status, message = message } };
        }
        #endregion
	}
}