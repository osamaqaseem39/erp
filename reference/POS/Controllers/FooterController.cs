using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;
using System.IO;

namespace POS.Controllers
{
    public class FooterController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult FooterIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "Footer"
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
        public ActionResult InsertUpdateFooter(clsFooter footer)
        {
            string message = "";
            bool status = false;
            int footerId = 0;
            if (footer.FooterId > 0)
            {
                footerId = footer.FooterId;
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateFooter(footer.FooterId,footer.FooterName, footer.Description,  "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Footer Successfully Updated";
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
                db.spInsertUpdateFooter(footer.FooterId, footer.FooterName, footer.Description, "Save", CheckReturn);
                footerId = Convert.ToInt32(CheckReturn.Value);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Footer Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, footerId = footerId } };
        }
        public ActionResult GetFooterById(int id)
        {
            clsFooter footer = new clsFooter();
            footer = (from f in db.Footers
                       where f.FooterId == id
                      select new clsFooter
                       {
                           FooterId = f.FooterId,
                           FooterName = f.FooterName,
                           Description = f.Description
                       }).FirstOrDefault();
            var data = footer;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FootersList()
        {
            return PartialView();
        }
        public ActionResult GetAllFooters()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var footerName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsFooter> footer = (from f in db.Footers
                                           select new clsFooter
                                            {
                                                FooterId = f.FooterId,
                                                FooterName = f.FooterName,
                                                Description = f.Description
                                            }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(footerName))
                {
                    footer = footer.Where(c => c.FooterName.ToLower().Contains(footerName.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        footer = footer.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = footer.Count();
                var data = footer.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteFooter(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateFooter(id, "", "", "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Footer Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this footer first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public JsonResult AttachFooter(string id)
        {
            bool status = false;
            int footerId = Convert.ToInt32(id.Split('|')[0]);
            string extension = id.Split('|')[1];
            if (extension != null && extension != "")
            {
                try
                {
                    string file_name2 = footerId + ".jpg";
                    string path2 = Server.MapPath("~/FormsDocuments/SaleFooter/" + file_name2);
                    FileInfo file2 = new FileInfo(path2);
                    if (file2.Exists)//check file exsit or not
                    {
                        file2.Delete();
                        status = true;
                    }

                    HttpPostedFileBase document = Request.Files[0] as HttpPostedFileBase;
                    if (document != null && document.ContentLength > 0)
                    {
                        // string fileName = System.IO.Path.GetFileName(doc.Document.FileName);
                        string fileName = footerId + ".jpg";
                        //Set the Image File Path.
                        string path = Server.MapPath("~/FormsDocuments/SaleFooter");
                        bool exists = Directory.Exists(path);//check folder exsit or not
                        if (!exists)
                        {
                            Directory.CreateDirectory(path);
                        }
                        string filePath = "~/FormsDocuments/SaleFooter/" + fileName;
                        //Save the Image File in Folder.
                        document.SaveAs(Server.MapPath(filePath));
                        status = true;
                    }
                }
                catch (Exception ex)
                {
                    status = true;
                }
            }
            else
            {
                status = true;
            }
            return Json(new
            {
                status = status,
            });
        }
        public ActionResult getFootersList()
        {
            List<clsFooter> footer = new List<clsFooter>();
            footer = (from c in db.Footers
                      select new clsFooter
                      {
                          FooterId = c.FooterId,
                          FooterName = c.FooterName,
                          Status = c.Status ?? "In-Active"
                      }).OrderBy(x => x.FooterId).ToList();
            var data = footer.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getActiveFooter()
        {
            int? footerId = db.Footers.Where(x => x.Status == "Active").Select(x => x.FooterId).FirstOrDefault();
            if (footerId == null)
            {
                footerId = 0;
            }
            return Json(new { footerId = footerId }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetDefaultFooter(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateFooter(id, "", "", "Update Status", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 1)
            {
                ModelState.Clear();
                status = true;
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