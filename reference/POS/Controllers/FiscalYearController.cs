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
    public class FiscalYearController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult FiscalYearIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "FiscalYear"
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
        [HttpPost]
        public ActionResult InsertUpdateFiscalYear(clsFiscalYear fiscalYear)
        {
            string message = "";
            bool status = false;
            if (fiscalYear.FiscalYearID > 0)
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateFiscalYear(fiscalYear.FiscalYearID, fiscalYear.Description,fiscalYear.FromDate,fiscalYear.ToDate, fiscalYear.Status, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Fiscal Year Successfully Updated";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateFiscalYear(fiscalYear.FiscalYearID, fiscalYear.Description, fiscalYear.FromDate, fiscalYear.ToDate, "Active", User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Fiscal Year Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult GetFiscalYearById(int id)
        {
            clsFiscalYear fiscalYear = new clsFiscalYear();
            fiscalYear = (from f in db.Admin_Setup_FiscalYear
                     where f.FiscalYearID == id
                          select new clsFiscalYear
                     {
                         FiscalYearID = f.FiscalYearID,
                         Description = f.Description,
                         FromDate = f.FromDate,
                         ToDate = f.ToDate,
                         Status = f.Status
                     }).FirstOrDefault();
            var data = fiscalYear;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FiscalYearsList()
        {
            return PartialView();
        }
        public ActionResult GetAllFiscalYears()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var fiscalYearName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsFiscalYear> fiscalyears = (from f in db.Admin_Setup_FiscalYear
                                             select new clsFiscalYear
                                       {
                                           FiscalYearID = f.FiscalYearID,
                                           Description = f.Description,
                                           FromDate = f.FromDate,
                                           ToDate = f.ToDate,
                                           Status = f.Status
                                       }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(fiscalYearName))
                {
                    fiscalyears = fiscalyears.Where(c => c.Description.ToLower().Contains(fiscalYearName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    fiscalyears = fiscalyears.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        fiscalyears = fiscalyears.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = fiscalyears.Count();
                var data = fiscalyears.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteFiscalYear(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateFiscalYear(id, "", DateTime.Now, DateTime.Now, "Active", User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Fiscal Year Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this fiscal year first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
	}
}