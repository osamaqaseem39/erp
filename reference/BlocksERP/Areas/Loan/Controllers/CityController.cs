using BlocksERP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using BlocksERP.Controllers;

namespace BlocksERP.Areas.Loan.Controllers
{
    public class CityController : BaseController
    {
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult CityIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmCity", "View") != 0)
                {
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmCity").ToLower()
                                  select new clsModule
                                  {
                                      ActionID = a.ActionID,
                                      ActionName = a.ActionName,
                                  }).ToList();
                    return View(actionList);
                }
                else
                {
                    return RedirectToAction("accessdenied", "home", new { area = "" });
                }
            }
            else
            {
                return RedirectToAction("login", "account", new { area = "" });
            }
        }

        public ActionResult GetAllCity()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                var CityName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                List<clsCity> datalist = (from f in db.tblCities
                                          orderby f.CityName
                                          select new clsCity
                                                   {

                                                       CityId = f.CityId,
                                                       CityName = f.CityName ?? "",
                                                       CountryId = f.CountryId ?? 0,
                                                       CountryName = db.tblCountries.Where(x => x.CountryId == f.CountryId).Select(x => x.CountryName).FirstOrDefault() ?? "",
                                                       Code = f.Code ?? "",
                                                       DisplayName = f.DisplayName ?? "",
                                                       Status = f.Status ?? 0,
                                                       StatusString = ((f.Status ?? 0) == 1) ? "Active" : "Inactive",
                                                       
                                                       CreatedDate = f.CreatedDate,
                                                       CreatedByString = db.tblUsers.Where(x => x.UserId == f.CreatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                                       UpdatedDate = f.UpdatedDate,
                                                       UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                                   }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(CityName))
                {
                    datalist = datalist.Where(c => c.CityName.ToLower().Contains(CityName.ToLower())).ToList();
                }

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        //datalist = datalist.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }

                var data = datalist.ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("login", "account", new { area = "" });
            }
        }

        public ActionResult InsertUpdateCity(int id)
        {
            if (User != null)
            {
                clsCity user = new clsCity();
                if (id > 0)
                {

                    int updateRoleId = menurights.MenuRights(User.UserID, "frmCity", "Edit");
                    int deleteRoleId = menurights.MenuRights(User.UserID, "frmCity", "Delete");

                    user = (from f in db.tblCities
                            where f.CityId == id
                            select new clsCity
                            {
                                CityId = f.CityId,
                                CityName = f.CityName ?? "",
                                CountryId = f.CountryId ?? 0,
                                CountryName = db.tblCountries.Where(x => x.CountryId == f.CountryId).Select(x => x.CountryName).FirstOrDefault() ?? "",
                                Code = f.Code ?? "",
                                DisplayName = f.DisplayName ?? "",
                                Status = f.Status ?? 0,
                                StatusString = ((f.Status ?? 0) == 1) ? "Active" : "Inactive",

                                UpdateRoleId = updateRoleId,
                                DeleteRoleId = deleteRoleId

                            }).FirstOrDefault();
                }
                else
                {
                    user.CityId = 0;
                    user.CityName = "";
                    user.Code = "";
                    user.Status = 0;
                    user.CountryId = 0;
                    user.CountryName = "";
                    user.DeleteRoleId = 0;
                    user.UpdateRoleId = 0;
                }
                return PartialView(user);
            }
            else
            {
                return RedirectToAction("login", "account", new { area = "" });
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateCity(clsCity user)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                int? CityId = user.CityId;
                if (user.CityId > 0)
                {
                    insertUpdateStatus = "Update";
                    bool check = db.tblCities.Where(x => x.CityId == CityId).Any(x => x.CityName.ToLower().Trim() == user.CityName.ToLower().Trim());
                    bool check5 = db.tblCities.Where(x => x.CityId == CityId).Any(x => x.Code.ToLower().Trim() == user.Code.ToLower().Trim());
                    if (!check)
                    {
                        bool check2 = db.tblCities.Any(x => x.CityName.ToLower().Trim() == user.CityName.ToLower().Trim());
                        if (check2)
                        {
                            status = false;
                            message = "City Name already exists.";
                            return new JsonResult { Data = new { status = status, message = message } };
                        }

                    }

                    if (!check5)
                    {
                        bool check3 = db.tblCities.Any(x => x.Code.ToLower().Trim() == user.Code.ToLower().Trim());

                        if (check3)
                        {
                            status = false;
                            message = "Code already exists.";
                            return new JsonResult { Data = new { status = status, message = message } };
                        }
                    }
                    if (user.StatusString == "Active")
                    {
                        user.Status = 1;
                    }
                    else
                    {
                        user.Status = 0;
                    }
                }
                else
                {
                    insertUpdateStatus = "Save";
                    bool check2 = db.tblCities.Any(x => x.CityName.ToLower().Trim() == user.CityName.ToLower().Trim());
                    if (check2)
                    {
                        status = false;
                        message = "City Name already exists.";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }

                    bool check3 = db.tblCities.Any(x => x.Code.ToLower().Trim() == user.Code.ToLower().Trim());

                    if (check3)
                    {
                        status = false;
                        message = "Code already exists.";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }

                    user.Status = 1;
                }
                returnId = InsertUpdateCityDb(user, insertUpdateStatus);
                if (Convert.ToInt32(returnId) != 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "City Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = returnId;
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message.ToString();
            }

            return new JsonResult { Data = new { status = status, message = message } };
        }
        private string InsertUpdateCityDb(clsCity st, string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCity", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@CityId", SqlDbType.Int).Value = st.CityId;
                        cmd.Parameters.Add("@CountryId", SqlDbType.Int).Value = st.CountryId;
                        cmd.Parameters.Add("@CityName", SqlDbType.NVarChar).Value = st.CityName;
                        cmd.Parameters.Add("@Code", SqlDbType.NVarChar).Value = st.Code;
                        cmd.Parameters.Add("@Status", SqlDbType.Int).Value = st.Status;
                        cmd.Parameters.Add("@ActionByUserId", SqlDbType.Int).Value = User.UserID;
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.NVarChar).Value = insertUpdateStatus;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.NVarChar, 300).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        returnId = cmd.Parameters["@CheckReturn"].Value.ToString();
                        cmd.Dispose();
                    }
                    con.Close();
                    con.Dispose();
                }
                catch (Exception ex)
                {
                    returnId = ex.Message.ToString();
                    int? userId = User.UserID;
                    clsSqlErrorLog.InsertError(userId, ex.Message.ToString(), "User Type");
                }
            }
            return returnId;
        }
        public ActionResult DeleteModule(int id)
        {
            string message = "";
            bool status = false;

            clsCity st = new clsCity();
            st.CityId = id;
            st.CityName = "";
            st.CountryId = 0;
            st.Code = "";
            st.Status = null;
            string returnId = InsertUpdateCityDb(st, "Delete");
            if (Convert.ToInt32(returnId) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Record Successfully Deleted";
            }
            else if (Convert.ToInt32(returnId) == 0)
            {
                ModelState.Clear();
                status = false;
                message = "Please delete all data against this record from its dependent forms.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = returnId;
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }

    }
}