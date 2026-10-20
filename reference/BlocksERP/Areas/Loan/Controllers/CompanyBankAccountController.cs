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
    public class CompanyBankAccountController : BaseController
    {
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult CompanyBankAccountIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmCompanyBankAccount", "View") != 0)
                {
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmCompanyBankAccount").ToLower()
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

        public ActionResult GetAllCompanyBankAccount()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                var AccountNumber = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                List<clsCompanyBankAccount> datalist = (from f in db.tblCompanyBankAccountDetails 
                                          orderby f.AccountNumber
                                          select new clsCompanyBankAccount
                                                   {

                                                       AccountId = f.AccountId,
                                                       AccountNumber = f.AccountNumber ?? "",
                                                       BankId = f.BankId,
                                                       BankName = db.tblBanks.Where(x => x.BankId == f.BankId).Select(x => x.BankName).FirstOrDefault() ?? "",
                                                       Description = f.Description ?? "",
                                                       HolderName = f.HolderName ?? "",
                                                       StatusString = f.Status ?? "",
                                                       
                                                       CreatedDate = f.CreatedAt,
                                                       CreatedByString = db.tblUsers.Where(x => x.UserId == f.CreatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                                       UpdatedDate = f.UpdatedAt,
                                                       UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                                   }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(AccountNumber))
                {
                    datalist = datalist.Where(c => c.AccountNumber.ToLower().Contains(AccountNumber.ToLower())).ToList();
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

        public ActionResult InsertUpdateCompanyBankAccount(int id)
        {
            if (User != null)
            {
                clsCompanyBankAccount user = new clsCompanyBankAccount();
                if (id > 0)
                {

                    int updateRoleId = menurights.MenuRights(User.UserID, "frmCompanyBankAccount", "Edit");
                    int deleteRoleId = menurights.MenuRights(User.UserID, "frmCompanyBankAccount", "Delete");

                    user = (from f in db.tblCompanyBankAccountDetails
                            where f.AccountId == id
                            select new clsCompanyBankAccount
                            {
                                AccountId = f.AccountId,
                                AccountNumber = f.AccountNumber ?? "",
                                BankId = f.BankId,
                                BankName = db.tblBanks.Where(x => x.BankId == f.BankId).Select(x => x.BankName).FirstOrDefault() ?? "",
                                Description = f.Description ?? "",
                                HolderName = f.HolderName ?? "",
                                StatusString = f.Status ?? "",

                                UpdateRoleId = updateRoleId,
                                DeleteRoleId = deleteRoleId

                            }).FirstOrDefault();
                }
                else
                {
                    user.AccountId = 0;
                    user.AccountNumber = "";
                    user.Description = "";
                    user.StatusString = "Active";
                    user.BankId = 0;
                    user.BankName = "";
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
        public ActionResult InsertUpdateCompanyBankAccount(clsCompanyBankAccount user)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                int? AccountId = user.AccountId;
                if (user.AccountId > 0)
                {
                    insertUpdateStatus = "Update";
                    bool check = db.tblCompanyBankAccountDetails.Where(x => x.AccountId == AccountId).Any(x => x.AccountNumber.ToLower().Trim() == user.AccountNumber.ToLower().Trim());
                    if (!check)
                    {
                        bool check2 = db.tblCompanyBankAccountDetails.Any(x => x.AccountNumber.ToLower().Trim() == user.AccountNumber.ToLower().Trim());
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
                    bool check2 = db.tblCompanyBankAccountDetails.Any(x => x.AccountNumber.ToLower().Trim() == user.AccountNumber.ToLower().Trim());
                    if (check2)
                    {
                        status = false;
                        message = "Record already exists.";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }

                    user.StatusString = "Active";
                }
                returnId = InsertUpdateCompanyBankAccountDb(user, insertUpdateStatus);
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
        private string InsertUpdateCompanyBankAccountDb(clsCompanyBankAccount st, string insertUpdateStatus)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCompanyBankAccountDetails", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@AccountId", SqlDbType.Int).Value = st.AccountId;
                        cmd.Parameters.Add("@BankId", SqlDbType.Int).Value = st.BankId;
                        cmd.Parameters.Add("@AccountNumber", SqlDbType.NVarChar).Value = st.AccountNumber;
                        cmd.Parameters.Add("@HolderName", SqlDbType.NVarChar).Value = st.HolderName;
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = st.Description;
                        cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = st.StatusString;
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

            clsCompanyBankAccount st = new clsCompanyBankAccount();
            st.AccountId = id;
            st.AccountNumber = "";
            st.BankId = 0;
            st.Status = null;
            string returnId = InsertUpdateCompanyBankAccountDb(st, "Delete");
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