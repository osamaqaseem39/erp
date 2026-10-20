using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlocksERP.Models;
using Newtonsoft.Json;
using BlocksERP.Controllers;

namespace BlocksERP.Areas.Loan.Controllers
{
    public class AdminEmailController : BaseController
    {
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult AdminEmailIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmAdminEmail", "View") != 0)
                {
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmAdminEmail").ToLower()
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


        public ActionResult GetAllAdminEmail()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                var AdminEmail = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                string whereCondition = "";
                string sorting = "";
                if (!(string.IsNullOrEmpty(sortColumn) && !(string.IsNullOrEmpty(sortColumnDir))))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        sorting = " Order by " + sortColumn + " " + sortColumnDir + "";
                    }
                }
                else
                {
                    sorting = " Order by s.AdminEmailID asc";
                }
                if (!(string.IsNullOrEmpty(AdminEmail)))
                {
                    whereCondition = " LOWER(s.AdminEmail) like ('%" + AdminEmail + "%')";
                }
                else
                {
                    whereCondition = " LOWER(s.AdminEmail) like ('%%')";
                }

                List<clsAdminEmail> listsub = new List<clsAdminEmail>();
                if (menurights.MenuRights(User.UserID, "frmAdminEmail", "View") != 0)
                {
                    DataTableReader dtr = clsSqlBrand.getAdminEmailListCount(whereCondition);
                    while (dtr.Read())
                    {
                        recordsTotal = Convert.ToInt32(dtr["MyRowCount"]);
                    }
                    DataTableReader dt = clsSqlBrand.getAdminEmailList(whereCondition, start, length, sorting);
                    int i = 0;
                    while (dt.Read())
                    {
                        listsub.Add(new clsAdminEmail()
                        {
                            AdminEmailID = Convert.ToInt32(dt["AdminEmailID"]),
                            AdminEmail = dt["AdminEmail"].ToString(),
                            Type = dt["Type"].ToString(),
                            StatusString = dt["StatusString"].ToString(),
                            CreatedDate = Convert.ToDateTime(dt["CreatedDate"].ToString()),
                            CreatedByString = dt["CreatedBy"].ToString(),
                            UpdatedDate = Convert.ToDateTime(dt["UpdatedDate"].ToString()),
                            UpdatedByString = dt["UpdatedBy"].ToString(),
                        }); ;
                    }
                }

                var data = listsub.ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("login", "account", new { area = "" });
            }
        }

        public ActionResult InsertUpdateAdminEmail(int id)
        {
            if (User != null)
            {
                clsAdminEmail user = new clsAdminEmail();
                if (id > 0)
                {
                    int updateRoleId = menurights.MenuRights(User.UserID, "frmAdminEmail", "Edit");
                    int deleteRoleId = menurights.MenuRights(User.UserID, "frmAdminEmail", "Delete");

                    user = (from f in db.tblAdminEmails
                            where f.AdminEmailID == id
                            select new clsAdminEmail
                            {
                                AdminEmailID = f.AdminEmailID,
                                AdminEmail = f.AdminEmail,
                                Type= f.Type,
                                Status = f.Status,
                                DeleteRoleId = deleteRoleId,
                                UpdateRoleId = updateRoleId
                            }).FirstOrDefault();
                }
                else
                {
                    user.AdminEmailID = 0;
                    user.AdminEmail = "";
                    user.Type = "";
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
        public ActionResult InsertUpdateAdminEmail(clsAdminEmail user)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                int? AdminEmailID = user.AdminEmailID;
                if (user.AdminEmailID > 0)
                {
                    insertUpdateStatus = "Update";
                    bool check = db.tblAdminEmails.Where(x => x.AdminEmailID == AdminEmailID).Any(x => x.AdminEmail.ToLower().Trim() == user.AdminEmail.ToLower().Trim());
                    if (!check)
                    {
                        bool check2 = db.tblAdminEmails.Any(x => x.AdminEmail.ToLower().Trim() == user.AdminEmail.ToLower().Trim());
                        if (check2)
                        {
                            status = false;
                            message = "Admin Email already exist.";
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
                    bool check2 = db.tblAdminEmails.Any(x => x.AdminEmail.ToLower().Trim() == user.AdminEmail.ToLower().Trim());
                    if (check2)
                    {
                        status = false;
                        message = "Admin Email already exist.";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }
                    user.Status = 1;
                }
                returnId = InsertUpdateAdminEmailDb(user, insertUpdateStatus);
                if (returnId == "Success")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Brand Successfully Updated";
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
        private string InsertUpdateAdminEmailDb(clsAdminEmail st, string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateAdminEmail", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@AdminEmailID", SqlDbType.Int).Value = st.AdminEmailID;
                        cmd.Parameters.Add("@AdminEmail", SqlDbType.NVarChar).Value = st.AdminEmail;
                        cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = st.Type;
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
                    clsSqlErrorLog.InsertError(userId, ex.Message.ToString(), "AdminEmail");
                }
            }
            return returnId;
        }
        public ActionResult DeleteAdminEmail(int id)
        {
            string message = "";
            bool status = false;

            clsAdminEmail st = new clsAdminEmail();
            st.AdminEmailID = id;
            st.AdminEmail = "";
            st.Type = "";
            st.Status = 0;
            string returnId = InsertUpdateAdminEmailDb(st, "Delete");
            if (returnId == "Success")
            {
                ModelState.Clear();
                status = true;
                message = "AdminEmail Successfully Deleted";
            }
            else if (returnId == "Fail")
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