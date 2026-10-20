using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;
using System.Data;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace POS.Controllers
{
    public class DriversLocationController : BaseController
    {
        PosMBDb db = new PosMBDb();

        #region Drivers
        public ActionResult DriversList()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CustomerSupplierDetail"
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
        public ActionResult GetAllDriverDetails()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var detailCode = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var description = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var reference = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var urduName = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                var type = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
                var city = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsAccountDetail> detail = (from f in db.AC_Detail
                                                 join fr in db.AC_Detail_Extra on f.DetailCode equals fr.DetailCode
                                                 where f.Type == "Driver"
                                                 orderby f.Name
                                                 select new clsAccountDetail
                                                 {
                                                     DetailCode = f.DetailCode,
                                                     Name = f.Name,
                                                     CityId = fr.CityId,
                                                     CityName = db.Cities.Where(x => x.CityId == fr.CityId).Select(x => x.CityName).FirstOrDefault() ?? string.Empty,
                                                     Status = f.Status,
                                                     AcDescription = f.AcDescription,
                                                     AccountCodeV = f.AccountCodeV,
                                                     AccountCode = f.AccountCode,
                                                     Reference = db.AC_ChartOfAC.Where(x => x.AccountCode == f.AccountCode).Select(x => x.Description).FirstOrDefault(),
                                                     Type = f.Type,
                                                     UrduName = f.UrduName,
                                                     Phone1 = fr.Phone1,
                                                     Phone2 = fr.Phone2,
                                                     Cell1 = fr.Cell1,
                                                     Cell2 = fr.Cell2,
                                                     Fax = fr.Fax,
                                                     Email = fr.Email,
                                                     Website = fr.Website,
                                                     Address = fr.Address,
                                                     CreditDays = fr.CreditDays,
                                                     CreditLimit = fr.CreditLimit,
                                                     TaxType = fr.TaxType,
                                                     GST = fr.GST,
                                                     NTN = fr.NTN,
                                                     CNIC = fr.CNIC,
                                                     Remarks = fr.Remarks,
                                                     SalesmanID = fr.SalesmanID
                                                 }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(detailCode))
                {
                    detail = detail.Where(c => c.DetailCode.ToString().Contains(detailCode)).ToList();
                }
                else if (!string.IsNullOrEmpty(detailName))
                {
                    detail = detail.Where(c => c.Name.ToLower().Contains(detailName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    detail = detail.Where(c => c.AcDescription.ToLower().Contains(description.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    detail = detail.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(reference))
                {
                    detail = detail.Where(c => c.Reference.ToLower().Contains(reference.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(urduName))
                {
                    detail = detail.Where(c => c.UrduName.Contains(urduName)).ToList();
                }
                else if (!string.IsNullOrEmpty(type))
                {
                    detail = detail.Where(c => c.Type.ToLower().Contains(type.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(city))
                {
                    detail = detail.Where(c => c.CityName.ToLower().Contains(city.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        detail = detail.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = detail.Count();
                var data = detail.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public ActionResult GetDriverLocationById(decimal? id)
        {
            clsAccountDetail driverlocation = new clsAccountDetail();
            driverlocation = (from f in db.Driver_Location 
                              join fd in db.AC_Detail on f.DetailCode equals fd.DetailCode 
                          where f.DetailCode == id
                          select new clsAccountDetail
                          {
                              DetailCode=f.DetailCode ,
                              Name=fd.Name ?? "",
                              LastDateTime = f.LastUpdateDateTime,
                              LastDateTimeStr = ("00" + f.LastUpdateDateTime.Value.Day.ToString()).Substring(("00" + f.LastUpdateDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.LastUpdateDateTime.Value.Month.ToString()).Substring(("00" + f.LastUpdateDateTime.Value.Month.ToString()).Length - 2) + "-" + f.LastUpdateDateTime.Value.Year.ToString() + " " + ("00" + f.LastUpdateDateTime.Value.Hour.ToString()).Substring(("00" + f.LastUpdateDateTime.Value.Hour.ToString()).Length - 2) + ":" + ("00" + f.LastUpdateDateTime.Value.Minute.ToString()).Substring(("00" + f.LastUpdateDateTime.Value.Minute.ToString()).Length - 2)  + ":" + ("00" + f.LastUpdateDateTime.Value.Second.ToString()).Substring(("00" + f.LastUpdateDateTime.Value.Second.ToString()).Length - 2),
                              Longitude = f.Longitude ?? "",
                              Latitude = f.Latitude ?? "",
                          }).FirstOrDefault();
            var data = driverlocation;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}