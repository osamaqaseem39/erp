using BlocksERP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using BlocksERP.Controllers;

namespace BlocksERP.Areas.Users.Controllers
{
    public class UsersController : BaseController
    {
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult UserIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmUser", "View") != 0)
                {
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmUser").ToLower()
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

        
        public ActionResult GetAllUsers()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var email = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var userType = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                List<clsUser> datalist = (from f in db.tblUsers
                                          where f.type.ToLower() == "admin"
                                          select new clsUser
                                          {
                                              UserId = f.UserId,
                                              Email = f.Email ?? "",
                                              PhoneNumber = f.PhoneNumber ?? "",
                                              Status = f.Status,
                                              Type = f.type ?? "",

                                              ImagePath = db.tblDocumentOthers.Where(x => x.TypeId == f.UserId.ToString() && x.DocumentType == "User").Select(x => "/FormsDocuments/User/" + x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                              CreatedDate = f.CreatedDate,
                                              UpdatedDate = f.UpdatedDate,
                                              UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                          }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(email))
                {
                    datalist = datalist.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(userType))
                {
                    datalist = datalist.Where(c => c.Type.ToLower().Contains(userType.ToLower())).ToList();
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

        public ActionResult InsertUpdateUser(int id)
        {
            if (User != null)
            {
                clsUser user = new clsUser();
                if (id > 0)
                {
                    int updateRoleId = menurights.MenuRights(User.UserID, "frmUser", "Edit");
                    int deleteRoleId = menurights.MenuRights(User.UserID, "frmUser", "Delete");

                    user = (from f in db.tblUsers
                            where f.UserId == id
                            select new clsUser
                            {
                                UserId = f.UserId,
                                Email = f.Email ?? "",
                                PhoneNumber = f.PhoneNumber ?? "",
                                Status = f.Status,
                                Type = f.type ?? "",

                                ImagePath = db.tblDocumentOthers.Where(x => x.TypeId == f.UserId.ToString() && x.DocumentType == "User").Select(x => "/FormsDocuments/User/" + x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                CreatedDate = f.CreatedDate,
                                UpdatedDate = f.UpdatedDate,
                                UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                DeleteRoleId = deleteRoleId,
                                UpdateRoleId = updateRoleId
                            }).FirstOrDefault();

                }
                else
                {
                    user.UserId = 0;
                    user.FirstName = "";
                    user.LastName = "";
                    user.PhoneNumber = "";
                    user.Status = "Active";
                    user.Email = "";
                    user.Type = "ADMIN";
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
        public ActionResult InsertUpdateUser(clsUser user)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                string insertUpdateStatus = "";
                int? UserId = user.UserId;
                if (user.UserId > 0)
                {
                    insertUpdateStatus = "Update";
                    bool check = db.tblUsers.Where(x => x.UserId == UserId).Any(x => x.Email.ToLower().Trim() == user.Email.ToLower().Trim());
                    if (!check)
                    {
                        bool check2 = db.tblUsers.Any(x => x.Email.ToLower().Trim() == user.Email.ToLower().Trim());
                        if (check2)
                        {
                            status = false;
                            message = "Email already exists.";
                            return new JsonResult { Data = new { status = status, message = message } };
                        }
                    }
                    if (user.IsChangePassword == 1)
                    {
                        if (!(string.IsNullOrEmpty(user.Password)))
                        {
                            user.Password = clsPasswordEncrypt.GetHash(user.Password);
                        }
                    }
                    else
                    {
                        user.Password = db.tblUsers.Where(x => x.UserId == user.UserId).Select(x => x.Password).FirstOrDefault() ?? "";
                    }
                }
                else
                {
                    insertUpdateStatus = "Save";
                    bool check2 = db.tblUsers.Any(x => x.Email.ToLower().Trim() == user.Email.ToLower().Trim());
                    if (check2)
                    {
                        status = false;
                        message = "Email already exists.";
                        return new JsonResult { Data = new { status = status, message = message } };
                    }

                    user.Status = "Active";
                    if (!(string.IsNullOrEmpty(user.Password)))
                    {
                        user.Password = clsPasswordEncrypt.GetHash(user.Password);
                    }
                }


                clsResult result = InsertUpdateUserDb(user, insertUpdateStatus);
                if (result.Id != 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "User Successfully Updated";
                    user.UserId = result.Id;
                }


                else
                {
                    ModelState.Clear();
                    status = false;
                    message = result.ResultMessage;
                    user.UserId = 0;
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message.ToString();
            }

            return new JsonResult { Data = new { status = status, message = message, id = user.UserId } };
        }
        private clsResult InsertUpdateUserDb(clsUser st, string insertUpdateStatus)
        {
            clsResult result = new clsResult();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateUsers", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = st.UserId;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = st.Email;
                        cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = st.Password;
                        cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = st.PhoneNumber;
                        cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = st.Status;
                        cmd.Parameters.Add("@Otp", SqlDbType.NVarChar).Value = "";
                        cmd.Parameters.Add("@OtpCreatedAt", SqlDbType.DateTime).Value = DateTime.Now;
                        cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = st.Type;
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
                    int? userId = User.UserID;
                    clsSqlErrorLog.InsertError(userId, ex.Message.ToString(), "User Type");
                    result.ResultMessage = ex.Message.ToString();
                    result.Id = 0;
                }
            }
            return result;
        }
        public ActionResult DeleteUser(int id)
        {
            string message = "";
            bool status = false;

            clsUser st = new clsUser();
            st.UserId = id;
            
            clsResult result = InsertUpdateUserDb(st, "Delete");
            if (result.Id == 2)
            {
                ModelState.Clear();
                status = true;
                message = "User Successfully Deleted";

                var itemDocument = db.tblDocumentOthers.Where(x => x.TypeId == id.ToString() && x.DocumentType == "User").FirstOrDefault();
                if (itemDocument != null)
                {
                    try
                    {
                        int documentsId = itemDocument.DocumentId;
                        string documentExtension = itemDocument.DocumentExtension;
                        db.tblDocumentOthers.Remove(itemDocument);
                        db.SaveChanges();
                        string fileName = documentsId + "." + documentExtension;
                        string path1 = Server.MapPath("~/FormsDocuments/User/" + fileName);
                        FileInfo file = new FileInfo(path1);



                        if (file.Exists)//check file exsit or not
                        {
                            file.Delete();
                        }

                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            else if (result.Id == 999)
            {

                status = false;
                message = "Please delete all data against this record from its dependent forms.";
                id = 0;
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }


    }
}