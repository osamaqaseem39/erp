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
    public class CabServicePauseController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult CabServicePauseIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CabServicePause"
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
        public ActionResult InsertUpdateCabServicePause(clsCabServicePause CabServicePause)
        {
            string message = "";
            bool status = false;
            if (CabServicePause.PauseID > 0)
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateCabServicePause(CabServicePause.PauseID, CabServicePause.FromDate, CabServicePause.ToDate, CabServicePause.FromTime, CabServicePause.ToTime, CabServicePause.Remarks, CabServicePause.Status, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Record Successfully Updated";
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
                db.spInsertUpdateCabServicePause(CabServicePause.PauseID, CabServicePause.FromDate, CabServicePause.ToDate, CabServicePause.FromTime, CabServicePause.ToTime, CabServicePause.Remarks, "Active", User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Record Successfully Saved";
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
        public ActionResult GetCabServicePauseById(int id)
        {
            clsCabServicePause brand = new clsCabServicePause();
            brand = (from f in db.Cab_ServicePause 
                        where f.PauseID == id
                     select new clsCabServicePause
                        {
                            PauseID = f.PauseID,
                            FromDate = f.FromDate,
                            ToDate = f.ToDate,
                            FromTime = f.FromTime ,
                            ToTime =f.ToTime ,
                            Remarks = f.Remarks ?? "",
                            Status = f.Status
                        }).FirstOrDefault();

            brand.FromTimeString = brand.FromTime.Value.ToString();
            brand.ToTimeString = brand.ToTime.Value.ToString();

            var data = brand;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CabServicePauseList()
        {
            return PartialView();
        }
        public ActionResult GetAllCabServicePauses()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var brandName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCabServicePause> brand = (from f in db.Cab_ServicePause
                                       select new clsCabServicePause
                                         {
                                             PauseID = f.PauseID,
                                             FromDate = f.FromDate,
                                             ToDate = f.ToDate,
                                             FromTime = f.FromTime,
                                             ToTime = f.ToTime,
                                             Remarks = f.Remarks ?? "",
                                             Status = f.Status
                                         }).ToList();

                foreach (clsCabServicePause vrow in brand)
                {
                    vrow.FromTimeString = vrow.FromTime.Value.ToString();
                    vrow.ToTimeString = vrow.ToTime.Value.ToString();
                }

                //SEARCHING...
                if (!string.IsNullOrEmpty(brandName))
                {
                    brand = brand.Where(c => c.FromDate.Value.Date.ToString("dd-MM-yyyy").StartsWith(brandName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    brand = brand.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        brand = brand.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = brand.Count();
                var data = brand.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteCabServicePause(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateCabServicePause(id, null, null, null, null, "", "Active", User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Record Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this Content first.";
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