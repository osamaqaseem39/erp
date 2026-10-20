using BlocksERP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using BlocksERP.Controllers;

namespace BlocksERP.Areas.Loan.Controllers
{
    public class CompanyController : BaseController
    {
        // GET: Color
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult CompanyIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmCompany", "View") != 0)
                {                    
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmCompany").ToLower()
                                  select new clsModule
                                  {
                                      ActionID = a.ActionID,
                                      ActionName = a.ActionName,
                                  }).ToList();
                    return View(actionList);
                }
                else
                {
                    return RedirectToAction("accessdenied", "home", new { area="" });
                }
            }
            else
            {
                return RedirectToAction("login", "account", new { area = "" });
            }
        }
        
        public ActionResult GetAllCompany()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                var CompanyName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                List<clsCompany> datalist = (from f in db.tblCompanies
                                              select new clsCompany
                                              {
                                                  CompanyId = f.CompanyId,
                                                  CompanyName = f.CompanyName ?? "",
                                                  Address = f.Address ?? "",
                                                  PhoneNo = f.PhoneNo ?? "",
                                                  CellNo = f.CellNo ?? "",
                                                  Email = f.Email ?? "",
                                                  Website = f.Website ?? "",
                                                  ContactPerson = f.ContactPerson ?? "",
                                                  ImagePath = db.tblDocumentOthers.Where(x => x.TypeId == f.CompanyId.ToString() && x.DocumentType == "Company").Select(x => "/FormsDocuments/Company/" + x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                                  DocumentId = db.tblDocumentOthers.Where(x => x.TypeId == f.CompanyId.ToString() && x.DocumentType == "Company").Select(x => x.DocumentId).FirstOrDefault(),
                                                  CreatedDate = f.CreatedDate,
                                                  CreatedByString = db.tblUsers.Where(x => x.UserId == f.CreatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                                  UpdatedDate = f.UpdatedDate,
                                                  UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                              }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(CompanyName))
                {
                    datalist = datalist.Where(c => c.CompanyName.ToLower().Contains(CompanyName.ToLower())).ToList();
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
        public ActionResult InsertUpdateCompany(int id)
        {
            if (User != null)
            {
                clsCompany user = new clsCompany();
                if (id > 0)
                {
                    int updateRoleId = menurights.MenuRights(User.UserID, "frmCompany", "Edit");
                    int deleteRoleId = menurights.MenuRights(User.UserID, "frmCompany", "Delete");

                    user = (from f in db.tblCompanies
                            where f.CompanyId == id
                            select new clsCompany
                            {
                                CompanyId = f.CompanyId,
                                CompanyName = f.CompanyName ?? "",
                                Address = f.Address ?? "",
                                PhoneNo = f.PhoneNo ?? "",
                                CellNo = f.CellNo ?? "",
                                Email = f.Email ?? "",
                                Website = f.Website ?? "",
                                ContactPerson = f.ContactPerson ?? "",                                
                                ImagePath = db.tblDocumentOthers.Where(x => x.TypeId == f.CompanyId.ToString() && x.DocumentType == "Company").Select(x => "/FormsDocuments/Company/" + x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                DocumentId = db.tblDocumentOthers.Where(x => x.TypeId == f.CompanyId.ToString() && x.DocumentType == "Company").Select(x => x.DocumentId).FirstOrDefault(),
                                DeleteRoleId = deleteRoleId,
                                UpdateRoleId = updateRoleId
                            }).FirstOrDefault();
                }
                else
                {
                    user.CompanyId = 0;
                    user.CompanyName = "";
                    user.Address = "";
                    user.PhoneNo = "";
                    user.CellNo = "";
                    user.Email = "";
                    user.Website = "";
                    user.ContactPerson = "";
                    user.ImagePath = "";
                    user.DocumentId = 0;
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
        public ActionResult InsertUpdateCompany(clsCompany user)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                int? CompanyId = user.CompanyId;
                if (user.CompanyId > 0)
                {
                    insertUpdateStatus = "Update";
                    bool check = db.tblCompanies.Where(x => x.CompanyId == CompanyId).Any(x => x.CompanyName.ToLower().Trim() == user.CompanyName.ToLower().Trim());
                    if (!check)
                    {
                        bool check2 = db.tblCompanies.Any(x => x.CompanyName.ToLower().Trim() == user.CompanyName.ToLower().Trim());
                        if (check2)
                        {
                            status = false;
                            message = "Company already exists.";
                            return new JsonResult { Data = new { status = status, message = message } };
                        }
                    }
                }
                else
                {
                    insertUpdateStatus = "Save";
                    bool check2 = db.tblCompanies.Any(x => x.CompanyName.ToLower().Trim() == user.CompanyName.ToLower().Trim());
                    if (check2)
                    {
                        status = false;
                        message = "Company already exists.";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }
                }
                clsResult result = InsertUpdateCompanyDb(user, insertUpdateStatus);
                if (result.Id != 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Company Successfully Updated";
                    user.CompanyId = result.Id;
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = result.ResultMessage;
                    user.CompanyId = result.Id;
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message.ToString();
            }

            return new JsonResult { Data = new { status = status, message = message, id = user.CompanyId } };
        }
        private clsResult InsertUpdateCompanyDb(clsCompany st, string insertUpdateStatus)
        {
            clsResult result = new clsResult();
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCompany", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = st.CompanyId;
                        cmd.Parameters.Add("@CompanyName", SqlDbType.NVarChar).Value = st.CompanyName;
                        cmd.Parameters.Add("@Address", SqlDbType.NVarChar).Value = st.Address;
                        cmd.Parameters.Add("@PhoneNo", SqlDbType.NVarChar).Value = st.PhoneNo;
                        cmd.Parameters.Add("@CellNo", SqlDbType.NVarChar).Value = st.CellNo;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = st.Email;
                        cmd.Parameters.Add("@Website", SqlDbType.NVarChar).Value = st.Website;
                        cmd.Parameters.Add("@ContactPerson", SqlDbType.NVarChar).Value = st.ContactPerson;
                        cmd.Parameters.Add("@ActionByUserId", SqlDbType.Int).Value = User.UserID;
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.NVarChar).Value = insertUpdateStatus;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        result.ResultMessage = cmd.Parameters["@CheckReturn"].Value.ToString();
                        result.Id = Convert.ToInt32(cmd.Parameters["@CheckReturn"].Value.ToString());
                        cmd.Dispose();


                    }
                    con.Close();
                    con.Dispose();
                }
                catch (Exception ex)
                {
                    result.ResultMessage = ex.Message.ToString();
                    result.Id = 0;
                    returnId = ex.Message.ToString();
                    int? userId = User.UserID;
                    clsSqlErrorLog.InsertError(userId, ex.Message.ToString(), "Company");
                }
            }
            return result;
        }
        public ActionResult DeleteModule(int id)
        {
            string message = "";
            bool status = false;

            clsCompany st = new clsCompany();
            st.CompanyId = id;
            st.CompanyName = "";
            st.Address = "";
            st.PhoneNo = "";

            st.CellNo = "";
            st.Email = "";
            st.Website = "";
            st.ContactPerson = "";            
            clsResult result = InsertUpdateCompanyDb(st, "Delete");
            if (result.Id == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Company Successfully Deleted";
            }
            else if (result.Id == 999)
            {
                ModelState.Clear();
                status = false;
                message = "Please delete all data against this record from its dependent forms.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = result.ResultMessage;
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }


     


    }
}