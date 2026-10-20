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
    public class CabOfferDetailController : BaseController
    {
        PosMBDb db = new PosMBDb();


        public List<clsCabOfferMain> getOfferList()
        {
            List<clsCabOfferMain> firstLevel = new List<clsCabOfferMain>();
            firstLevel = (from c in db.Cab_OfferMain
                          where c.Status == "Active"
                          select new clsCabOfferMain
                          {
                              OfferID = c.OfferID,
                              FromLocation = c.FromLocation.Replace("<br/>", " ")
                          }).ToList();
            return firstLevel;
        }


        public List<clsCabType> getCabTypeList()
        {
            List<clsCabType> firstLevel = new List<clsCabType>();
            firstLevel = (from c in db.Cab_CabType
                          where c.Status == "Active"
                          select new clsCabType
                          {
                              CabTypeID = c.CabTypeID,
                              Description = c.Description
                          }).ToList();
            return firstLevel;
        }

        public ActionResult CabOfferDetailIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CabOfferDetail"
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

                    ViewBag.OfferList = new SelectList(getOfferList(), "OfferID", "FromLocation");
                    ViewBag.CabTypeList = new SelectList(getCabTypeList(), "CabTypeID", "Description");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateCabOfferDetail(clsCabOfferDetail CabOffer)
        {
            string message = "";
            bool status = false;
            if (CabOffer.PromotionID > 0)
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateCabOfferDetail(CabOffer.PromotionID, CabOffer.OfferID, CabOffer.CabTypeID,CabOffer.SequenceNo, CabOffer.ToLocation, CabOffer.ToPostCode, CabOffer.Remarks, CabOffer.Status,CabOffer.Price, User.UserId, "Update", CheckReturn);
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
                db.spInsertUpdateCabOfferDetail(CabOffer.PromotionID, CabOffer.OfferID, CabOffer.CabTypeID, CabOffer.SequenceNo, CabOffer.ToLocation, CabOffer.ToPostCode, CabOffer.Remarks, "Active", CabOffer.Price, User.UserId, "Save", CheckReturn);
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
        public ActionResult GetCabOfferDetailById(int id)
        {
            clsCabOfferDetail brand = new clsCabOfferDetail();
            brand = (from f in db.Cab_OfferDetail
                     where f.PromotionID == id
                     select new clsCabOfferDetail
                        {
                            PromotionID = f.PromotionID,
                            OfferID = f.OfferID,
                            OfferName=db.Cab_OfferMain.Where(x=>x.OfferID == f.OfferID).Select(x=>x.FromLocation).FirstOrDefault().Replace("<br/>"," ") ?? "",
                            CabTypeID = f.CabTypeID,
                            CabName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabTypeID).Select(x => x.Description).FirstOrDefault() ?? "",
                            SequenceNo=f.SequenceNo,
                            ToLocation = f.ToLocation,
                            ToPostCode = f.ToPostCode,
                            Price=f.Price,
                            Remarks =f.Remarks,
                            Status = f.Status
                        }).FirstOrDefault();
            var data = brand;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CabOfferDetailList()
        {
            return PartialView();
        }
        public ActionResult GetAllCabOfferDetail()
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
                var tolocation = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCabOfferDetail> brand = (from f in db.Cab_OfferDetail
                                               select new clsCabOfferDetail
                                         {
                                             PromotionID = f.PromotionID,
                                             OfferID = f.OfferID,
                                             OfferName = db.Cab_OfferMain.Where(x => x.OfferID == f.OfferID).Select(x => x.FromLocation).FirstOrDefault().Replace("<br/>", " ") ?? "",
                                             CabTypeID = f.CabTypeID,
                                             CabName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabTypeID).Select(x => x.Description).FirstOrDefault() ?? "",
                                             SequenceNo = f.SequenceNo,
                                             ToLocation = f.ToLocation,
                                             ToPostCode = f.ToPostCode,
                                             Price = f.Price,
                                             Remarks = f.Remarks,
                                             Status = f.Status
                                         }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(brandName))
                {
                    brand = brand.Where(c => c.OfferName.ToLower().Contains(brandName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    brand = brand.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(tolocation))
                {
                    brand = brand.Where(c => c.ToLocation.ToLower().Contains(tolocation.ToLower())).ToList();
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
        public ActionResult DeleteCabOfferDetail(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateCabOfferDetail(id, 0, 0, 0, "", "", "", "", 0, User.UserId, "Delete", CheckReturn);
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
                message = "Error! Please delete all data against this record first.";
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