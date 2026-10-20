using BlocksERP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using BlocksERP.Controllers;

namespace BlocksERP.Areas.Users.Controllers
{
    public class UserRoleController : BaseController
    {
        // GET: UserRole
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        public ActionResult UserRoleIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmUserRole", "View") != 0)
                {
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmUserRole").ToLower()
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


        public ActionResult AssignModule(int userId)
        {

            List<clsModule> module = (from f in db.tblMenuModules 
                                      orderby f.SectionName
                                      select new clsModule
                                      {                                          
                                          SectionName = f.SectionName,
                                      }).Distinct().ToList();
            foreach (clsModule c in module)
            {
                c.ModuleList = (from f in db.tblMenuModules
                                orderby f.ModuleName
                                select new clsModule
                                {
                                    ModuleID = f.ModuleID,
                                    ModuleName = f.ModuleName,
                                    DisplayName = f.DisplayName ?? "",
                                    SectionName = f.SectionName,
                                    UserId = userId,
                                    UserRoleId = db.tblUserMenuRoles.Where(x => x.UserID == userId && x.ModuleID == f.ModuleID).Select(x => x.UserRoleID).FirstOrDefault() ?? "",
                                }).ToList();

            }
            foreach (clsModule c in module)
            {
                c.ActionList = (from f in db.tblMenuAssignActions 
                                join a in db.tblMenuActions on f.ActionID equals a.ActionID
                                orderby a.ActionName
                                select new clsModule
                                {
                                    ActionID = a.ActionID,
                                    ActionName = a.ActionName,
                                    ModuleID = f.ModuleID ?? 0,
                                    UserId = userId,
                                    UserRoleId = db.tblUserMenuRoles.Where(x => x.UserID == userId && x.ModuleID == f.ModuleID && x.ActionID == f.ActionID).Select(x => x.UserRoleID).FirstOrDefault() ?? "",
                                }).ToList();

                if (c.ActionList.Count == 0)
                {
                    if (!String.IsNullOrEmpty(c.UserRoleId))
                    {
                        c.AllActions = "Yes";
                    }
                    else
                    {
                        c.AllActions = "Not";
                    }
                }
                else
                {
                    if (c.ActionList.Any(x => x.UserRoleId == ""))
                    {
                        c.AllActions = "Not";
                    }
                    else
                    {
                        c.AllActions = "Yes";
                    }
                }
            }

            return PartialView(module);
        }

        public ActionResult AssignAction(int userId)
        {
            List<clsModule> user = (from f in db.tblMenuActions
                                    orderby f.ActionName
                                    select new clsModule
                                    {
                                        ActionID = f.ActionID,
                                        ActionName = f.ActionName,
                                        UserId = userId,
                                        UserRoleId = db.tblUserMenuRoles.Where(x => x.UserID == userId && x.ActionID == f.ActionID).Select(x => x.UserRoleID).FirstOrDefault() ?? "",
                                    }).ToList();
            return PartialView(user);
        }
        [HttpPost]
        public ActionResult InsertUpdateAssignModule(int userId, string assignType, string data)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                List<clsModule> module = JsonConvert.DeserializeObject<List<clsModule>>(data);
                DataTable dt = new DataTable();
                dt.Columns.Add("Id");
                dt.Columns.Add("ModuleId");
                dt.Columns.Add("ActionId");
                if (module.Count != 0)
                {
                    int counterLoop = 0;
                    foreach (clsModule c in module)
                    {
                        counterLoop++;
                        int moduleId = c.ModuleID;
                        dt.Rows.Clear();
                        for (int i = 0; i < c.ActionList.Count; i++)
                        {
                            dt.Rows.Add(new object[] { i + 1, moduleId, c.ActionList[i].ActionID });
                        }
                        returnId = InsertUpdateAssignDb(userId, moduleId, assignType, counterLoop, dt);
                        if (returnId == "Success")
                        {
                            ModelState.Clear();
                            status = true;
                            message = "Module Assigned Successfully Updated.";
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
                    }
                }
                else
                {
                    dt.Rows.Add(new object[] { 0, 0, 0 });
                    returnId = InsertUpdateAssignDb(userId, 0, assignType, 1, dt);
                    if (returnId == "Success")
                    {
                        ModelState.Clear();
                        status = true;
                        message = "Module Assigned Successfully Updated.";
                    }
                    else
                    {
                        ModelState.Clear();
                        status = false;
                        message = returnId;
                    }
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message.ToString();
            }

            return new JsonResult { Data = new { status = status, message = message } };
        }
        [HttpPost]
        public ActionResult InsertUpdateAssignAction(int userId, string assignType, string data)
        {
            string message = "";
            bool status = false;
            try
            {
                string returnId = "0";
                List<clsModule> action = JsonConvert.DeserializeObject<List<clsModule>>(data);
                DataTable dt = new DataTable();
                dt.Columns.Add("Id");
                dt.Columns.Add("ModuleId");
                dt.Columns.Add("ActionId");
                if (action.Count != 0)
                {
                    for (int i = 0; i < action.Count; i++)
                    {
                        dt.Rows.Add(new object[] { i + 1, 0, action[i].ActionID });
                    }
                    returnId = InsertUpdateAssignDb(userId, 0, assignType, 1, dt);
                    if (returnId == "Success")
                    {
                        ModelState.Clear();
                        status = true;
                        message = "Assigned Successfully Updated.";
                    }
                    else
                    {
                        ModelState.Clear();
                        status = false;
                        message = returnId;
                    }
                }
                else
                {
                    dt.Rows.Add(new object[] { 0, 0, 0 });
                    returnId = InsertUpdateAssignDb(userId, 0, assignType, 1, dt);
                    if (returnId == "Success")
                    {
                        ModelState.Clear();
                        status = true;
                        message = "Assigned Successfully Updated.";
                    }
                    else
                    {
                        ModelState.Clear();
                        status = false;
                        message = returnId;
                    }
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message.ToString();
            }


            return new JsonResult { Data = new { status = status, message = message } };
        }
        private string InsertUpdateAssignDb(int userId, int moduleId, string assignType, int counter, DataTable dt)
        {
            string returnId = "0";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateUserRole", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        cmd.Parameters.Add("@ModuleId", SqlDbType.Int).Value = moduleId;
                        cmd.Parameters.Add("@ActionByUserId", SqlDbType.Int).Value = userId;
                        cmd.Parameters.Add("@AssignType", SqlDbType.NVarChar).Value = assignType;
                        cmd.Parameters.Add("@dtUserRole", SqlDbType.Structured).Value = dt;
                        cmd.Parameters.Add("@CounterLoop", SqlDbType.Int).Value = counter;
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.NVarChar).Value = "Save";
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
                    int? userId2 = userId;
                    clsSqlErrorLog.InsertError(userId2, ex.Message.ToString(), "User Role");
                }
            }
            return returnId;
        }

    }



}
