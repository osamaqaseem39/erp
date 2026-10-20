using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;

namespace POS.Controllers
{
    public class TaxRateController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult TaxRateIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "TaxRateIndex"
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
                    ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsAccountDetail> getDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      join ac in db.AC_DefaultGL on c.AccountCode equals ac.AccountCode
                      where c.Status == "Active" && ac.Type == "Tax"
                      select new clsAccountDetail
                          {
                              DetailCode = c.DetailCode,
                              Name = c.Name
                          }).ToList();
            return detail;
        }
        public ActionResult getMaxCodeForTaxRate()
        {
            decimal maxCode = db.Admin_Setup_Tax.OrderByDescending(x => x.TaxID).Select(x => x.TaxID).FirstOrDefault();
            if (maxCode == 0)
            {
                maxCode = 10;
            }
            else
            {
                maxCode += 1;
            }
            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult InsertUpdateTaxRate(clsTaxRate tax)
        {
            string message = "";
            bool status = false;
            if (tax.IsNeWOrEdit == "Edit")
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateTax(tax.TaxID, tax.TaxName, tax.TaxRate, tax.DetailCode,tax.Status,User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Tax Successfully Updated";
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
                db.spInsertUpdateTax(tax.TaxID, tax.TaxName, tax.TaxRate, tax.DetailCode, "Active", User.UserId, "Save", CheckReturn);
                if (Convert.ToInt32(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Tax Successfully Saved";
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
        public ActionResult GetTaxRateById(int id)
        {
            clsTaxRate tax = new clsTaxRate();
            tax = (from f in db.Admin_Setup_Tax
                   where f.TaxID == id
                   select new clsTaxRate
                   {
                       TaxID = f.TaxID,
                       TaxName = f.TaxName,
                       TaxRate = f.TaxRate,
                       DetailCode = f.DetailCode,
                       DetailName = db.AC_Detail.Where(x=>x.DetailCode == f.DetailCode).Select(x=>x.Name).FirstOrDefault(),
                       Status = f.Status
                   }).FirstOrDefault();
            var data = tax;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TaxRatesList()
        {
            return PartialView();
        }
        public ActionResult GetAllTaxRates()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var taxName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var detail = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsTaxRate> tax = (from f in db.Admin_Setup_Tax
                                        select new clsTaxRate
                                        {
                                            TaxID = f.TaxID,
                                            TaxName = f.TaxName,
                                            TaxRate = f.TaxRate,
                                            DetailCode = f.DetailCode,
                                            DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                            Status = f.Status
                                        }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(taxName))
                {
                    tax = tax.Where(c => c.TaxName.ToLower().Contains(taxName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(detail))
                {
                    tax = tax.Where(c => c.DetailName.ToLower().Contains(detail.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    tax = tax.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        tax = tax.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = tax.Count();
                var data = tax.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteTax(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateTax(id, "", 0, 0, "Active", User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Tax Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this tax first.";
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