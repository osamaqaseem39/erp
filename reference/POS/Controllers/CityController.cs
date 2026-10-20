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
    public class CityController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult CityIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "City"
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
                    ViewBag.Country = new SelectList(getCountry(), "CountryId", "CountryName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateCity(clsCity city)
        {
            string message = "";
            bool status = false;
            if (city.CityId > 0)
            {
                bool check = db.Cities.Where(x => x.CityId == city.CityId).Any(x => x.CountryId == city.CountryId && x.CityName.ToLower() == city.CityName.ToLower());
                if (!check)
                {
                    bool check2 = db.Cities.Where(x => x.CountryId == city.CountryId).Any(x => x.CityName.ToLower() == city.CityName.ToLower());
                    if (check2)
                    {
                        status = false;
                        message = "Same City already exist against this country.";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }
                }
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateCity(city.CityId, city.CityName,city.CountryId, User.UserId,"Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "City Successfully Updated";
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
                bool check = db.Cities.Where(x => x.CountryId == city.CountryId).Any(x => x.CityName.ToLower() == city.CityName.ToLower());
                if (check)
                {
                    status = false;
                    message = "Same City already exist against this country.";
                    return new JsonResult { Data = new { status = status, message = message } };
                }
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateCity(city.CityId, city.CityName, city.CountryId, User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "City Successfully Saved";
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
        public ActionResult GetCityById(int id)
        {
            clsCity city = new clsCity();
            city = (from f in db.Cities
                    join fd in db.Countries on f.CountryId equals fd.CountryId
                    where f.CityId == id
                    select new clsCity
                   {
                       CityId = f.CityId,
                       CityName = f.CityName,
                       CountryId = fd.CountryId
                   }).FirstOrDefault();
            var data = city;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CitiesList()
        {
            return PartialView();
        }

        public ActionResult GetAllCities()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var cityName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var countryName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCity> city = (from f in db.Cities
                                      join fd in db.Countries on f.CountryId equals fd.CountryId
                                      select new clsCity
                                      {
                                          CityId = f.CityId,
                                          CityName = f.CityName,
                                          CountryId = fd.CountryId,
                                          CountryName = fd.CountryName
                                      }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(cityName))
                {
                    city = city.Where(c => c.CityName.ToLower().Contains(cityName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(countryName))
                {
                    city = city.Where(c => c.CountryName.ToLower().Contains(countryName.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        city = city.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = city.Count();
                var data = city.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteCity(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateCity(id, "", 0, User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "City Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this city first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public List<clsCountry> getCountry()
        {
            List<clsCountry> country = new List<clsCountry>();
            country = (from c in db.Countries
                      select new clsCountry
                      {
                          CountryId = c.CountryId,
                          CountryName = c.CountryName
                      }).ToList();
            return country;
        }
    }
}