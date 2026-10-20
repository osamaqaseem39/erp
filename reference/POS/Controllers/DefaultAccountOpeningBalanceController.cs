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
    public class DefaultAccountOpeningBalanceController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult DefaultAccountOpeningBalanceIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "A/COpeningBalance"
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
        public ActionResult GetAllDefaultAccountOpeningBalances()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsDefaultAccountOpeningBalance> d = (from f in db.AC_Detail
                                                           where f.Status == "Active"
                                                           select new clsDefaultAccountOpeningBalance
                                                          {
                                                              DetailCode = f.DetailCode,
                                                              DetailName =f.Name,
                                                              Debit = db.AC_Detail_Opening.Where(x => x.DetailCode == f.DetailCode && x.FiscalYearID == User.FiscalYearId && x.BusinessID == 0).Select(x => x.Debit).FirstOrDefault() ?? 0,
                                                              Credit = db.AC_Detail_Opening.Where(x => x.DetailCode == f.DetailCode && x.FiscalYearID == User.FiscalYearId && x.BusinessID == 0).Select(x => x.Credit).FirstOrDefault() ?? 0,
                                                          }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(detailName))
                {
                    d = d.Where(c => c.DetailName.ToLower().Contains(detailName.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        d = d.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = d.Count();
                var data = d.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateDefaultAccountOpeningBalance(clsDefaultAccountOpeningBalance d)
        {
            string message = "";
            bool status = false;
            bool check = db.AC_Detail_Opening.Any(x => x.DetailCode == d.DetailCode && x.FiscalYearID == User.FiscalYearId && x.BusinessID == 0);
            if(check)
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAC_Detail_Opening(d.DetailCode, User.FiscalYearId, 0, d.Debit, d.Credit, d.CurrentBalance, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Default A/C Opening Balance Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAC_Detail_Opening(d.DetailCode, User.FiscalYearId, 0, d.Debit, d.Credit, d.CurrentBalance, User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Default A/C Opening Balance Successfully Saved";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
	}
}