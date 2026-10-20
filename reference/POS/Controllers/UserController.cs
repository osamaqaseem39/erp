using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;

namespace POS.Controllers
{
    public class UserController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region User
        public ActionResult UserIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "User"
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
        [HttpPost]
        public ActionResult InsertUpdateUser(clsUser user)
        {
            string message = "";
            bool status = false;
            user.Password = clsPasswordEncrypt.GetHash(user.Password);
            if (user.UserID > 0)
            {
                string password = "";
                if (user.ChangePassword == 1)
                {
                    password = user.Password;
                }
                else
                {
                    password = db.Admin_Setup_User.Where(x => x.UserID == user.UserID).Select(x => x.Password).FirstOrDefault();
                }
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                try
                {
                    db.spInsertUpdateUser(user.UserID, user.Username, password, user.Email, user.Description, user.Status, user.Multi, 0, User.UserId,0, "Update", CheckReturn);
                }
                catch (Exception ex)
                {
                    CheckReturn.Value = 0;
                    message = ex.InnerException.Message.ToString();
                }
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "User Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                try
                {
                    db.spInsertUpdateUser(user.UserID, user.Username, user.Password, user.Email, user.Description, "Active", user.Multi, 0, User.UserId,0, "Save", CheckReturn);
                }
                catch (Exception ex)
                {
                    CheckReturn.Value = 0;
                    message = ex.InnerException.Message.ToString();
                }
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "User Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult GetUserById(int id)
        {
            clsUser user = new clsUser();
            user = (from f in db.Admin_Setup_User
                    where f.UserID == id
                    select new clsUser
                    {
                        UserID = f.UserID,
                        Username = f.Username,
                        Password = f.Password,
                        Email = f.Email,
                        Description = f.Description,
                        Multi = f.Multi,
                        BusinessID = f.BusinessID,
                        Status = f.Status,
                    }).FirstOrDefault();
            var data = user;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UsersList()
        {
            return PartialView();
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
                var userName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var email = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var description = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsUser> user = (from f in db.Admin_Setup_User
                                      select new clsUser
                                      {
                                          UserID = f.UserID,
                                          Username = f.Username,
                                          Password = f.Password,
                                          Email = f.Email,
                                          Description = f.Description,
                                          Multi = f.Multi,
                                          BusinessID = f.BusinessID,
                                          Status = f.Status,
                                      }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(userName))
                {
                    user = user.Where(c => c.Username.ToLower().Contains(userName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(email))
                {
                    user = user.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    user = user.Where(c => c.Description.ToLower().Contains(description.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    user = user.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        user = user.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = user.Count();
                var data = user.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteUser(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateUser(id, "", "", "", "", "", 0, 0, User.UserId,0,  "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "User Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this user first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        #endregion
        #region Assign Role
        public ActionResult AssignUserRole(int userId)
        {
            List<clsUser> user = (from f in db.Admin_Setup_Menu
                                  orderby f.MenuGroup
                                  select new clsUser
                                  {
                                      UserID = userId,
                                      MenuName = f.MenuName,
                                      MenuTitle = f.MenuTitle,
                                      showMain = db.Admin_Setup_UserRights.Where(x => x.UserID == userId && x.MenuName == f.MenuName).Select(x => x.showMain).FirstOrDefault() ?? 0,
                                      showSave = db.Admin_Setup_UserRights.Where(x => x.UserID == userId && x.MenuName == f.MenuName).Select(x => x.showSave).FirstOrDefault() ?? 0,
                                      showChange = db.Admin_Setup_UserRights.Where(x => x.UserID == userId && x.MenuName == f.MenuName).Select(x => x.showChange).FirstOrDefault() ?? 0,
                                      showDelete = db.Admin_Setup_UserRights.Where(x => x.UserID == userId && x.MenuName == f.MenuName).Select(x => x.showDelete).FirstOrDefault() ?? 0,
                                      MenuGroup = f.MenuGroup
                                  }).ToList();
            return PartialView(user);
        }
        [HttpPost]
        public ActionResult InsertUpdateUserRole(string data)
        {
            string message = "";
            bool status = false;
            int returnId = 0;
            List<clsUser> user = JsonConvert.DeserializeObject<List<clsUser>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("MenuName");
            dt.Columns.Add("UserID");
            dt.Columns.Add("showMain");
            dt.Columns.Add("showSave");
            dt.Columns.Add("showChange");
            dt.Columns.Add("showDelete");
            for (int i = 0; i < user.Count; i++)
            {
                dt.Rows.Add(new object[] { i + 1, user[i].MenuName, user[i].UserID, user[i].showMain, user[i].showSave, user[i].showChange, user[i].showDelete });
            }
            returnId = AddEdit(dt);
            if (returnId > 0)
            {
                ModelState.Clear();
                status = true;
                message = "Roles Successfully Assigned.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        private int AddEdit(DataTable dt)
        {
            int returnId = 0;
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateUserRole", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@dtUserRole", SqlDbType.Structured).Value = dt;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        returnId = Convert.ToInt32(cmd.Parameters["@CheckReturn"].Value);
                        cmd.Dispose();
                    }
                    con.Close();
                    con.Dispose();
                }
                catch (Exception)
                { }
            }
            return returnId;

        }
        #endregion
    }
}