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
    public class SalesmanController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult SalesmanIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "Salesman"
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
        public ActionResult InsertUpdateSalesman(clsSalesman salesman)
        {
            string message = "";
            bool status = false;
            if (salesman.SalesmanID > 0)
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateSalesman(salesman.SalesmanID, salesman.FullName, salesman.Address, salesman.Cell1, salesman.Cell2, salesman.Email, salesman.Remarks, salesman.CNIC, salesman.Status, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Salesman Successfully Updated";
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
                db.spInsertUpdateSalesman(salesman.SalesmanID, salesman.FullName, salesman.Address, salesman.Cell1, salesman.Cell2, salesman.Email, salesman.Remarks, salesman.CNIC, "Active", User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Salesman Successfully Saved";
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
        public ActionResult GetSalesmanById(int id)
        {
            clsSalesman salesman = new clsSalesman();
            salesman = (from f in db.Admin_Setup_Salesman
                         where f.SalesmanID == id
                         select new clsSalesman
                         {
                             SalesmanID = f.SalesmanID,
                             FullName = f.FullName,
                             Address = f.Address,
                             Cell1 = f.Cell1,
                             Cell2 = f.Cell2,
                             Email = f.Email,
                             Remarks = f.Remarks,
                             CNIC = f.CNIC,
                             Status = f.Status
                         }).FirstOrDefault();
            var data = salesman;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SalesmansList()
        {
            return PartialView();
        }
        public ActionResult GetAllSalesmans()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var salesmanName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var email = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var cnic = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsSalesman> salesman = (from f in db.Admin_Setup_Salesman
                                                select new clsSalesman
                                                {
                                                    SalesmanID = f.SalesmanID,
                                                    FullName = f.FullName,
                                                    Address = f.Address,
                                                    Cell1 = f.Cell1,
                                                    Cell2 = f.Cell2,
                                                    Email = f.Email,
                                                    Remarks = f.Remarks,
                                                    CNIC = f.CNIC,
                                                    Status = f.Status
                                                }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(salesmanName))
                {
                    salesman = salesman.Where(c => c.FullName.ToLower().Contains(salesmanName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(email))
                {
                    salesman = salesman.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    salesman = salesman.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(cnic))
                {
                    salesman = salesman.Where(c => c.CNIC.Contains(cnic)).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    salesman = salesman.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        salesman = salesman.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = salesman.Count();
                var data = salesman.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteSalesman(int id)
        {
            string message = "";
            bool status = false;
            clsSalesman salesman = new clsSalesman();
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateSalesman(id, "", "", "", "", "", "", "", "Active", User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Salesman Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this salesman first.";
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