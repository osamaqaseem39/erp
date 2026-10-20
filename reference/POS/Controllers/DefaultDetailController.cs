using System;
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
    public class DefaultDetailController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult DefaultDetailIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "DefaultDetailIndex"
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
                    User.Save = 0;
                    User.Delete = 0;
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
            List<clsAccountDetail> firstLevel = new List<clsAccountDetail>();
            firstLevel = (from c in db.AC_Detail
                          where c.Status == "Active"
                          select new clsAccountDetail
                          {
                              DetailCode = c.DetailCode,
                              Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                          }).ToList();
            return firstLevel;
        }
        [HttpPost]
        public ActionResult InsertUpdateDefaultDetail(clsDefaultGL d)
        {
            string message = "";
            bool status = false;
            if (d.DefaultID > 0)
            {
                bool check = db.AC_DefaultDetail.Where(x => x.DefaultID == d.DefaultID).Any(x => x.DetailCode == d.DetailCode && x.Type == d.Type);
                if (!check)
                {
                    bool check2 = db.AC_DefaultDetail.Where(x => x.Type == d.Type).Any(x => x.DetailCode == d.DetailCode);
                    if (check2)
                    {
                        status = false;
                        message = "Error! An entry already exist with this type and account";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }
                }
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAC_DefaultDetail(d.DefaultID, d.Type, d.DetailCode, 0, 1, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Default Detail Successfully Updated";
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
                bool check = db.AC_DefaultDetail.Where(x => x.Type == d.Type).Any(x => x.DetailCode == d.DetailCode);
                if (check)
                {
                    status = false;
                    message = "Error! An entry already exist with this type and account";
                    return new JsonResult { Data = new { status = status, message = message } };
                }
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAC_DefaultDetail(d.DefaultID, d.Type, d.DetailCode, 0, 1, User.UserId, "Save", CheckReturn);
                if (Convert.ToInt32(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Default Detail Successfully Saved";
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
        public ActionResult GetDefaultDetailById(int id)
        {
            clsDefaultGL dd = new clsDefaultGL();
            dd = (from f in db.AC_DefaultDetail
                  join ac in db.AC_Detail on f.DetailCode equals ac.DetailCode
                   where f.DefaultID == id
                  select new clsDefaultGL
                   {
                       DefaultID = f.DefaultID,
                       Type = f.Type,
                       DetailCode = f.DetailCode,
                       BusinessID = f.BusinessID,
                       Reference = ac.Name,
                       DefualtSet = f.DefualtSet
                   }).FirstOrDefault();
            var data = dd;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DefaultDetailsList()
        {
            return PartialView();
        }
        public ActionResult GetAllDefaultDetails()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var type = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var reference = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsDefaultGL> dd = (from f in db.AC_DefaultDetail
                                          join ac in db.AC_Detail on f.DetailCode equals ac.DetailCode
                                          select new clsDefaultGL
                                          {
                                              DefaultID = f.DefaultID,
                                              Type = f.Type,
                                              DetailCode = f.DetailCode,
                                              BusinessID = f.BusinessID,
                                              Reference = ac.Name,
                                              DefualtSet = f.DefualtSet
                                          }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(type))
                {
                    dd = dd.Where(c => c.Type.ToLower().Contains(type.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(reference))
                {
                    dd = dd.Where(c => c.Reference.ToLower().Contains(reference.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        dd = dd.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = dd.Count();
                var data = dd.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeletedDefaultDetail(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateAC_DefaultDetail(id, "", 0, 0, 0, User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Default Detail Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this default detail first.";
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