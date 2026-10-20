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
    public class IncomeSourceController : BaseController
    {
        // GET: Color
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult IncomeSourceIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmIncomeSource", "View") != 0)
                {
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmIncomeSource").ToLower()
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

        public ActionResult GetAllIncomeSource()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                var SourceName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                List<clsIncomeSource> datalist = (from f in db.tblIncomeSources
                                              orderby f.SourceName
                                             select new clsIncomeSource
                                          {

                                              IncomeSourceID = f.IncomeSourceID,
                                              SourceName = f.SourceName ?? "",
                                              Status = f.Status ?? 0,
                                              StatusString = ((f.Status ?? 0) == 1) ? "Active" : "Inactive",

                                            CreatedDate = f.CreatedAt,
                                            CreatedByString = db.tblUsers.Where(x => x.UserId == f.CreatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                            UpdatedDate = f.UpdatedAt,
                                            UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                             }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(SourceName))
                {
                    datalist = datalist.Where(c => c.SourceName.ToLower().Contains(SourceName.ToLower())).ToList();
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
        public ActionResult InsertUpdateIncomeSource(int id)
        {
            if (User != null)
            {
                clsIncomeSource user = new clsIncomeSource();
                if (id > 0)
                {
                    int updateRoleId = menurights.MenuRights(User.UserID, "frmIncomeSource", "Edit");
                    int deleteRoleId = menurights.MenuRights(User.UserID, "frmIncomeSource", "Delete");
                    user = (from f in db.tblIncomeSources
                            where f.IncomeSourceID == id
                            select new clsIncomeSource
                            {
                                IncomeSourceID = f.IncomeSourceID,
                                SourceName = f.SourceName ?? "",
                                Status = f.Status ?? 0,
                                StatusString = ((f.Status ?? 0) == 1) ? "Active" : "Inactive",
                                UpdateRoleId = updateRoleId,
                                DeleteRoleId = deleteRoleId
                            }).FirstOrDefault();
                }
                else
                {
                    user.IncomeSourceID = 0;
                    user.SourceName = "";
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
        public ActionResult InsertUpdateIncomeSource(clsIncomeSource user)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                int? IncomeSourceID = user.IncomeSourceID;
                if (user.IncomeSourceID > 0)
                {
                    insertUpdateStatus = "Update";
                    bool check = db.tblIncomeSources.Where(x => x.IncomeSourceID == IncomeSourceID).Any(x => x.SourceName.ToLower().Trim() == user.SourceName.ToLower().Trim());
                    if (!check)
                    {
                        bool check2 = db.tblIncomeSources.Any(x => x.SourceName.ToLower().Trim() == user.SourceName.ToLower().Trim());
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
                    bool check2 = db.tblIncomeSources.Any(x => x.SourceName.ToLower().Trim() == user.SourceName.ToLower().Trim());
                    if (check2)
                    {
                        status = false;
                        message = "Record already exists.";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }

                    user.Status = 1;
                }
                returnId = InsertUpdateIncomeSourceDb(user, insertUpdateStatus);
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
        private string InsertUpdateIncomeSourceDb(clsIncomeSource st, string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateIncomeSource", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add("@IncomeSourceID", SqlDbType.Int).Value = st.IncomeSourceID;
                        cmd.Parameters.Add("@SourceName", SqlDbType.NVarChar).Value = st.SourceName;
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
                    clsSqlErrorLog.InsertError(userId, ex.Message.ToString(), "IncomeSource");
                }
            }
            return returnId;
        }
        public ActionResult DeleteModule(int id)
        {
            string message = "";
            bool status = false;

            clsIncomeSource st = new clsIncomeSource();
            st.IncomeSourceID = id;
            st.SourceName = "";
            st.Status = null;
            string returnId = InsertUpdateIncomeSourceDb(st, "Delete");
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