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
    public class EducationController : BaseController
    {
        // GET: Color
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult EducationIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmEducation", "View") != 0)
                {
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmEducation").ToLower()
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

        public ActionResult GetAllEducation()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                var EducationLevel = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                List<clsEducation> datalist = (from f in db.tblEducations 
                                              orderby f.EducationLevel
                                             select new clsEducation
                                          {

                                              EducationID = f.EducationID,
                                              EducationLevel = f.EducationLevel ?? "",
                                              Status = f.Status ?? 0,
                                              StatusString = ((f.Status ?? 0) == 1) ? "Active" : "Inactive",

                                            CreatedDate = f.CreatedAt,
                                            CreatedByString = db.tblUsers.Where(x => x.UserId == f.CreatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                            UpdatedDate = f.UpdatedAt,
                                            UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                             }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(EducationLevel))
                {
                    datalist = datalist.Where(c => c.EducationLevel.ToLower().Contains(EducationLevel.ToLower())).ToList();
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
        public ActionResult InsertUpdateEducation(int id)
        {
            if (User != null)
            {
                clsEducation user = new clsEducation();
                if (id > 0)
                {
                    int updateRoleId = menurights.MenuRights(User.UserID, "frmEducation", "Edit");
                    int deleteRoleId = menurights.MenuRights(User.UserID, "frmEducation", "Delete");
                    user = (from f in db.tblEducations
                            where f.EducationID == id
                            select new clsEducation
                            {
                                EducationID = f.EducationID,
                                EducationLevel = f.EducationLevel ?? "",
                                Status = f.Status ?? 0,
                                StatusString = ((f.Status ?? 0) == 1) ? "Active" : "Inactive",
                                UpdateRoleId = updateRoleId,
                                DeleteRoleId = deleteRoleId
                            }).FirstOrDefault();
                }
                else
                {
                    user.EducationID = 0;
                    user.EducationLevel = "";
                    user.Status = 0;
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
        public ActionResult InsertUpdateEducation(clsEducation user)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                int? EducationID = user.EducationID;
                if (user.EducationID > 0)
                {
                    insertUpdateStatus = "Update";
                    bool check = db.tblEducations.Where(x => x.EducationID == EducationID).Any(x => x.EducationLevel.ToLower().Trim() == user.EducationLevel.ToLower().Trim());
                    if (!check)
                    {
                        bool check2 = db.tblEducations.Any(x => x.EducationLevel.ToLower().Trim() == user.EducationLevel.ToLower().Trim());
                        if (check2)
                        {
                            status = false;
                            message = "Record already exists.";
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
                    bool check2 = db.tblEducations.Any(x => x.EducationLevel.ToLower().Trim() == user.EducationLevel.ToLower().Trim());
                    if (check2)
                    {
                        status = false;
                        message = "Record already exists.";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }

                    user.Status = 1;
                }
                returnId = InsertUpdateEducationDb(user, insertUpdateStatus);
                if (Convert.ToInt32(returnId) != 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Record Successfully Updated";
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
        private string InsertUpdateEducationDb(clsEducation st, string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateEducation", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add("@EducationID", SqlDbType.Int).Value = st.EducationID;
                        cmd.Parameters.Add("@EducationLevel", SqlDbType.NVarChar).Value = st.EducationLevel;
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
                    clsSqlErrorLog.InsertError(userId, ex.Message.ToString(), "Education");
                }
            }
            return returnId;
        }
        public ActionResult DeleteModule(int id)
        {
            string message = "";
            bool status = false;

            clsEducation st = new clsEducation();
            st.EducationID = id;
            st.EducationLevel = "";
            st.Status = null;
            string returnId = InsertUpdateEducationDb(st, "Delete");
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