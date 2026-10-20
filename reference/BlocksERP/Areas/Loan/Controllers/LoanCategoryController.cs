using BlocksERP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using BlocksERP.Controllers;
using Microsoft.AspNet.Identity;

namespace BlocksERP.Areas.Loan.Controllers
{
    public class LoanCategoryController : BaseController
    {
        // GET: Color
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult LoanCategoryIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmLoanCategory", "View") != 0)
                {
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmLoanCategory").ToLower()
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

        public ActionResult GetAllLoanCategory()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                var Name = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                List<clsLoanCategory> datalist = (from f in db.tblLoanCategories
                                              orderby f.CategoryName
                                             select new clsLoanCategory
                                             {

                                                 LoanCategoryID = f.LoanCategoryID,
                                                 CategoryName = f.CategoryName ?? "",
                                                 CategoryStatus = f.CategoryStatus ?? "",
                                                 LoanType = f.LoanType ?? "",

                                                 InterestRatePercentage = f.InterestRatePercentage,
                                                 MaxDays = f.MaxDays,
                                                 MaxLimit = f.MaxLimit,
                                                 CompanyFee = f.CompanyFee,

                                                 CreatedDate = f.CreatedAt,
                                                CreatedByString = db.tblUsers.Where(x => x.UserId == f.CreatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                                UpdatedDate = f.UpdatedAt,
                                                UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                             }).ToList();


                //SEARCHING...
                if (!string.IsNullOrEmpty(Name))
                {
                    datalist = datalist.Where(c => c.CategoryName.ToLower().Contains(Name.ToLower())).ToList();
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
        public ActionResult InsertUpdateLoanCategory(int id)
        {
            if (User != null)
            {
                clsLoanCategory user = new clsLoanCategory();
                if (id > 0)
                {
                    int updateRoleId = menurights.MenuRights(User.UserID, "frmLoanCategory", "Edit");
                    int deleteRoleId = menurights.MenuRights(User.UserID, "frmLoanCategory", "Delete");
                    user = (from f in db.tblLoanCategories
                            where f.LoanCategoryID == id
                            select new clsLoanCategory
                            {
                                LoanCategoryID = f.LoanCategoryID,
                                CategoryName = f.CategoryName ?? "",
                                CategoryStatus = f.CategoryStatus ?? "",
                                LoanType = f.LoanType ?? "",

                                InterestRatePercentage = f.InterestRatePercentage,
                                MaxDays = f.MaxDays,
                                MaxLimit = f.MaxLimit,
                                CompanyFee = f.CompanyFee,

                                UpdateRoleId = updateRoleId,
                                DeleteRoleId = deleteRoleId
                            }).FirstOrDefault();
                }
                else
                {
                    user.LoanCategoryID = 0;
                    user.CategoryName = "";
                    user.CategoryStatus = "Active";
                    user.LoanType = "";
                    user.InterestRatePercentage = 0;
                    user.MaxDays = 0;
                    user.MaxLimit = 0;
                    user.CompanyFee = 0;

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
        public ActionResult InsertUpdateLoanCategory(clsLoanCategory user)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                int? Id = user.LoanCategoryID;
                if (user.LoanCategoryID > 0)
                {
                    insertUpdateStatus = "Update";
                    bool check = db.tblLoanCategories.Where(x => x.LoanCategoryID == Id).Any(x => x.CategoryName.ToLower().Trim() == user.CategoryName.ToLower().Trim());
                    if (!check)
                    {
                        bool check2 = db.tblLoanCategories.Any(x => x.CategoryName.ToLower().Trim() == user.CategoryName.ToLower().Trim());
                        if (check2)
                        {
                            status = false;
                            message = "Record already exists.";
                            return new JsonResult { Data = new { status = status, message = message } };
                        }
                    }

                }
                else
                {
                    insertUpdateStatus = "Save";
                    bool check2 = db.tblLoanCategories.Any(x => x.CategoryName.ToLower().Trim() == user.CategoryName.ToLower().Trim());
                    if (check2)
                    {
                        status = false;
                        message = "Record already exists.";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }

                    user.CategoryStatus = "Active";
                }
                returnId = InsertUpdateLoanCategoryDb(user, insertUpdateStatus);
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
        private string InsertUpdateLoanCategoryDb(clsLoanCategory st, string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateLoanCategory", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add("@LoanCategoryID", SqlDbType.Int).Value = st.LoanCategoryID;
                        cmd.Parameters.Add("@CategoryName", SqlDbType.NVarChar).Value = st.CategoryName;
                        cmd.Parameters.Add("@LoanType", SqlDbType.NVarChar).Value = st.LoanType;
                        cmd.Parameters.Add("@CategoryStatus", SqlDbType.NVarChar).Value = st.CategoryStatus;
                        cmd.Parameters.Add("@InterestRatePercentage", SqlDbType.Decimal).Value = st.InterestRatePercentage;
                        cmd.Parameters.Add("@MaxDays", SqlDbType.Int).Value = st.MaxDays;
                        cmd.Parameters.Add("@MaxLimit", SqlDbType.Decimal).Value = st.MaxLimit;
                        cmd.Parameters.Add("@CompanyFee", SqlDbType.Decimal).Value = st.CompanyFee;
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
                    clsSqlErrorLog.InsertError(userId, ex.Message.ToString(), "LoanCategory");
                }
            }
            return returnId;
        }
        public ActionResult DeleteModule(int id)
        {
            string message = "";
            bool status = false;

            clsLoanCategory st = new clsLoanCategory();
            st.LoanCategoryID = id;
            string returnId = InsertUpdateLoanCategoryDb(st, "Delete");
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