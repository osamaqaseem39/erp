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
    public class SetupContentPagesController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult SetupContentPagesIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SetupContentPages"
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
        public ActionResult InsertUpdateSetupContentPages(clsSetupContentPages SetupContentPages)
        {
            string message = "";
            bool status = false;
            if (SetupContentPages.ContentID > 0)
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateContentPages(SetupContentPages.ContentID, SetupContentPages.PageName, SetupContentPages.ContentSlider, SetupContentPages.Content1, SetupContentPages.Content2, SetupContentPages.PageTitle, SetupContentPages.MetaData, SetupContentPages.MetaKeywords, SetupContentPages.SchemaHeader, SetupContentPages.URLLink, SetupContentPages.Content3, SetupContentPages.Content4, SetupContentPages.Content5, SetupContentPages.Content6, SetupContentPages.Status, SetupContentPages.ContentGroup, SetupContentPages.DisplayName, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Content Successfully Updated";
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
                db.spInsertUpdateContentPages(SetupContentPages.ContentID, SetupContentPages.PageName, SetupContentPages.ContentSlider, SetupContentPages.Content1, SetupContentPages.Content2, SetupContentPages.PageTitle, SetupContentPages.MetaData, SetupContentPages.MetaKeywords, SetupContentPages.SchemaHeader, SetupContentPages.URLLink, SetupContentPages.Content3, SetupContentPages.Content4, SetupContentPages.Content5, SetupContentPages.Content6, "Active", SetupContentPages.ContentGroup, SetupContentPages.DisplayName, User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Content Successfully Saved";
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
        public ActionResult GetSetupContentPagesById(int id)
        {
            clsSetupContentPages brand = new clsSetupContentPages();
            brand = (from f in db.Setup_Content_Pages
                        where f.ContentID == id
                     select new clsSetupContentPages
                        {
                            ContentID = f.ContentID,
                            ContentSlider = f.ContentSlider ?? "",
                            Content1 = f.Content1 ?? "",
                            Content2 = f.Content2 ?? "",
                            URLLink = f.URLLink ?? "",
                            Content3 = f.Content3 ?? "",
                            Content4 = f.Content4 ?? "",
                            Content5 = f.Content5 ?? "",
                            Content6 = f.Content6 ?? "",
                            PageName = f.PageName ?? "",
                            PageTitle = f.PageTitle ?? "",
                            MetaData = f.MetaData ?? "",
                            MetaKeywords = f.MetaKeywords ?? "",
                            ContentGroup = f.ContentGroup ?? "",
                            DisplayName = f.DisplayName ?? "",
                            SchemaHeader = f.SchemaHeader ?? "",
                            Status = f.Status
                        }).FirstOrDefault();
            var data = brand;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetupContentPagesList()
        {
            return PartialView();
        }
        public ActionResult GetAllSetupContentPagess()
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
                List<clsSetupContentPages> brand = (from f in db.Setup_Content_Pages
                                       select new clsSetupContentPages
                                         {
                                             ContentID = f.ContentID,
                                             //ContentSlider = f.ContentSlider ?? "",
                                             //Content1 = f.Content1 ?? "",
                                             //Content2 = f.Content2 ?? "",
                                             //URLLink = f.URLLink ?? "",
                                             //Content3 = f.Content3 ?? "",
                                             //Content4 = f.Content4 ?? "",
                                             //Content5 = f.Content5 ?? "",
                                             //Content6 = f.Content6 ?? "",
                                             PageName = f.PageName ?? "",
                                             PageTitle = f.PageTitle ?? "",
                                             //MetaData = f.MetaData ?? "",
                                             //MetaKeywords = f.MetaKeywords ?? "",
                                             ContentGroup = f.ContentGroup ?? "",
                                             DisplayName = f.DisplayName ?? "",
                                             //SchemaHeader = f.SchemaHeader ?? "",
                                             Status = f.Status
                                         }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(brandName))
                {
                    brand = brand.Where(c => c.PageName.ToLower().Contains(brandName.ToLower())).ToList();
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
        public ActionResult DeleteSetupContentPages(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateContentPages(id, "", "", "", "", "", "", "", "", "", "", "", "", "", "Active", "", "", User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Content Successfully Deleted";
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