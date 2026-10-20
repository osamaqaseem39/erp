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
    public class DefaultGLController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult DefaultGLIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "DefaultGLIndex"
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
                    ViewBag.ThirdLevel = new SelectList(getThirdLevels(), "AccountCode", "Description");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsChartOfAccount> getThirdLevels()
        {
            List<clsChartOfAccount> firstLevel = new List<clsChartOfAccount>();
            firstLevel = (from c in db.AC_ChartOfAC
                          where c.AcLevel == 3 & c.Status == "Active"
                          select new clsChartOfAccount
                          {
                              AccountCode = c.AccountCode,
                              Description = c.Description
                          }).ToList();
            return firstLevel;
        }
        [HttpPost]
        public ActionResult InsertUpdateDefaultGL(clsDefaultGL GL)
        {
            string message = "";
            bool status = false;
            if (GL.DefaultID > 0)
            {
                bool check = db.AC_DefaultGL.Where(x => x.DefaultID == GL.DefaultID).Any(x => x.AccountCode == GL.AccountCode && x.Type == GL.Type);
                if (!check)
                {
                    bool check2 = db.AC_DefaultGL.Where(x => x.Type == GL.Type).Any(x => x.AccountCode == GL.AccountCode);
                    if (check2)
                    {
                        status = false;
                        message = "Error! An entry already exist with this type and reference";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }
                }
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateDefaultGL(GL.DefaultID, GL.Type, GL.AccountCode, 0, 1, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Default GL Successfully Updated";
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
                bool check = db.AC_DefaultGL.Where(x => x.Type == GL.Type).Any(x => x.AccountCode == GL.AccountCode);
                if(check)
                {
                    status = false;
                    message = "Error! An entry already exist with this type and reference";
                    return new JsonResult { Data = new { status = status, message = message } };
                }
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateDefaultGL(GL.DefaultID, GL.Type, GL.AccountCode, 0, 1, User.UserId, "Save", CheckReturn);
                if (Convert.ToInt32(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Default GL Successfully Saved";
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
        public ActionResult GetDefaultGLById(int id)
        {
            clsDefaultGL tax = new clsDefaultGL();
            tax = (from f in db.AC_DefaultGL
                   join ac in db.AC_ChartOfAC on f.AccountCode equals ac.AccountCode
                   where f.DefaultID == id
                   select new clsDefaultGL
                   {
                       DefaultID = f.DefaultID,
                       Type = f.Type,
                       AccountCode = f.AccountCode,
                       BusinessID = f.BusinessID,
                       Reference = ac.Description,
                       DefualtSet = f.DefualtSet
                   }).FirstOrDefault();
            var data = tax;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DefaultGLsList()
        {
            return PartialView();
        }
        public ActionResult GetAllDefaultGLs()
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
                List<clsDefaultGL> dd = (from f in db.AC_DefaultGL
                                          join ac in db.AC_ChartOfAC on f.AccountCode equals ac.AccountCode
                                          select new clsDefaultGL
                                          {
                                              DefaultID = f.DefaultID,
                                              Type = f.Type,
                                              AccountCode = f.AccountCode,
                                              BusinessID = f.BusinessID,
                                              Reference = ac.Description,
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
        public ActionResult DeletedDefaultGL(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateDefaultGL(id, "", 0, 0, 0, User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Default GL Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this default GL first.";
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