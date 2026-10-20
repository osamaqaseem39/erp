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
using System.Xml.Linq;

namespace BlocksERP.Areas.Loan.Controllers
{
    public class LoanUserController : BaseController
    {
        ModelDb db = new ModelDb();
        MenuRightsController menurights = new MenuRightsController();

        #region New Users
        public ActionResult NewLoanUserIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmPendingLoanUser", "View") != 0)
                {
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmPendingLoanUser").ToLower()
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

        public ActionResult LoanUserDetail(int? id)
        {
            if (User != null)
            {
                //if (menurights.MenuRights(User.UserID, "frmPendingLoanUser", "View") != 0)
                //{
                //    List<clsModule> actionList = new List<clsModule>();
                //    actionList = (from r in db.tblUserMenuRoles
                //                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                //                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                //                  where r.UserID == User.UserID
                //                  && m.ModuleName.ToLower() == ("frmPendingLoanUser").ToLower()
                //                  select new clsModule
                //                  {
                //                      ActionID = a.ActionID,
                //                      ActionName = a.ActionName,
                //                  }).ToList();
                //    return View(actionList);
                //}
                //else
                //{
                //    return RedirectToAction("accessdenied", "home", new { area = "" });
                //}


                clsUser datalist = (from f in db.tblUsers
                                          join p in db.tblPersonalInfoes on f.UserId equals p.UserID
                                          where f.UserId == id
                                          select new clsUser
                                          {
                                              UserId = f.UserId,
                                              Email = f.Email ?? "",
                                              PhoneNumber = f.PhoneNumber ?? "",
                                              Status = f.Status,
                                              Type = f.type ?? "",

                                              ImagePath = ("/FormsDocuments/LoanUser/" + f.UserId + ".jpg") ?? "",
                                              CreatedDate = f.CreatedDate,
                                              UpdatedDate = f.UpdatedDate,
                                              UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                              PersonalInfoID = p.PersonalInfoID,
                                              FullName = p.FullName ?? "",
                                              SocialInsuranceNumber = p.SocialInsuranceNumber ?? "",
                                              ResidentialAddress = p.ResidentialAddress ?? "",
                                              DetailAddress = p.DetailAddress ?? "",
                                              GenderID = p.GenderID,
                                              GenderName = db.tblGenders.Where(x => x.GenderID == p.GenderID).Select(x => x.GenderName).FirstOrDefault() ?? "",
                                              EducationID = p.EducationID,
                                              EducationName = db.tblEducations.Where(x => x.EducationID == p.EducationID).Select(x => x.EducationLevel).FirstOrDefault() ?? "",
                                              MaritalStatusID = p.MaritalStatusID,
                                              MaritalStatusName = db.tblMaritalStatus.Where(x => x.MaritalStatusID == p.MaritalStatusID).Select(x => x.StatusName).FirstOrDefault() ?? "",
                                              CountryID = p.CountryID,
                                              CountryName = db.tblCountries.Where(x => x.CountryId == p.CountryID).Select(x => x.CountryName).FirstOrDefault() ?? "",
                                              ResidenceCityID = p.ResidenceCityID,
                                              ResidenceCityName = db.tblCities.Where(x => x.CityId == p.ResidenceCityID).Select(x => x.DisplayName).FirstOrDefault() ?? "",
                                              DetailedCityID = p.DetailedCityID,
                                              DetailedCityName = db.tblCities.Where(x => x.CityId == p.DetailedCityID).Select(x => x.DisplayName).FirstOrDefault() ?? "",
                                              DOB = p.DOB
                                          }).FirstOrDefault();

                List<clsDocument> documents = new List<clsDocument>();
                documents = (from d in db.tblDocuments
                             where d.userId == id
                             select new clsDocument
                             {
                                 DocumentId = d.DocumentId,
                                 userId = d.userId,
                                 Date = d.ExpiryDate,
                                 DocumentTypeId = d.DocumentTypeId,
                                 DocumentType = db.tblDocumentTypes.Where(x => x.DocumentTypeID == d.DocumentTypeId).Select(x => x.TypeName).FirstOrDefault() ?? "",
                                 DocumentPath = d.DocumentPath ?? "",
                                 DocumentExtension = d.DocumentExtension ?? "",
                                 Description = d.Description ?? "",
                                 Status = d.Status ?? "",
                             }).ToList();

                datalist.DocList = documents;


                List<clsWork> worklist = new List<clsWork>();
                worklist = (from d in db.tblWorkInfoes
                             where d.UserID == id
                             select new clsWork
                             {
                                 WorkInfoID = d.WorkInfoID,
                                 UserID = d.UserID,
                                 DOJ = d.DOJ,

                                 IncomeSourceID = d.IncomeSourceID,
                                 IncomeSourceName = db.tblIncomeSources.Where(x => x.IncomeSourceID == d.IncomeSourceID).Select(x => x.SourceName).FirstOrDefault() ?? "",
                                 EmploymentStatusID = d.EmploymentStatusID,
                                 EmploymentStatusName = db.tblEmploymentStatus.Where(x => x.EmploymentStatusID == d.EmploymentStatusID).Select(x => x.StatusName).FirstOrDefault() ?? "",

                                 OrganizationName = d.OrganizationName ?? "",
                                 OrganizationPhoneNumber = d.OrganizationPhoneNumber ?? "",
                                 OrganizationAddress = d.OrganizationAddress ?? "",
                                 MonthlyIncome = d.MonthlyIncome ?? 0,

                             }).ToList();

                datalist.WorkList = worklist;



                List<clsReferal> ReferalList = new List<clsReferal>();
                ReferalList = (from d in db.tblReferalInfoes
                            where d.UserID == id
                            select new clsReferal
                            {
                                ReferalID = d.ReferalID,
                                UserID = d.UserID,
                                ContactRelationID = d.ContactRelationID,
                                ContactRelationName = db.tblReferralRelations.Where(x => x.ReferralRelationID == d.ContactRelationID).Select(x => x.RelationName).FirstOrDefault() ?? "",
                                RefferalName = d.RefferalName ?? "",
                                RefferalPhoneNumber = d.RefferalPhoneNumber ?? "",
                                RefferalSIN = d.RefferalSIN ?? "",
                                RefferalAddress = d.RefferalAddress ?? "",

                            }).ToList();

                datalist.ReferalList = ReferalList;






                List<clsUserBankAccount> BankList = new List<clsUserBankAccount>();
                BankList = (from d in db.tblBankAccountDetails 
                               where d.UserId == id
                               select new clsUserBankAccount
                               {
                                   BankAccountId = d.BankAccountId,
                                   UserId = d.UserId,
                                   BankId = d.BankId,
                                   BankName = db.tblBanks.Where(x => x.BankId == d.BankId).Select(x => x.BankName).FirstOrDefault() ?? "",
                                   AccountNumber = d.AccountNumber ?? "",
                                   HolderName = d.HolderName ?? "",
                                   Description = d.Description ?? "",
                               }).ToList();

                datalist.BankList = BankList;




                return View(datalist);
            }
            else
            {
                return RedirectToAction("login", "account", new { area = "" });
            }
        }

        public ActionResult CheckAnyPendingDocuement(int id)
        {
            int countPending = db.tblDocuments.Where(x => x.userId == id && x.Status.ToLower() != ("verified").ToLower()).Count();

            return new JsonResult { Data = new { status = true, countPending = countPending } };
        }


        public ActionResult GetAllNewLoanUsers()
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
                                          join p in db.tblPersonalInfoes on f.UserId equals p.UserID 
                                          where f.type.ToLower() == ("User").ToLower() &&
                                          f.Status.ToLower() == ("Pending").ToLower()
                                          select new clsUser
                                          {
                                              UserId = f.UserId,
                                              Email = f.Email ?? "",
                                              PhoneNumber = f.PhoneNumber ?? "",
                                              Status = f.Status,
                                              Type = f.type ?? "",

                                              ImagePath = ("/FormsDocuments/LoanUser/" + f.UserId + ".jpg") ?? "",
                                              CreatedDate = f.CreatedDate,
                                              UpdatedDate = f.UpdatedDate,
                                              UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                              PersonalInfoID = p.PersonalInfoID,
                                              FullName = p.FullName ?? "",
                                              SocialInsuranceNumber = p.SocialInsuranceNumber ?? "",
                                              ResidentialAddress = p.ResidentialAddress ?? "",
                                              DetailAddress = p.DetailAddress ?? "",
                                              GenderID = p.GenderID,
                                              GenderName = db.tblGenders.Where(x=>x.GenderID == p.GenderID).Select(x=>x.GenderName).FirstOrDefault() ??"",
                                              EducationID = p.EducationID,
                                              EducationName = db.tblEducations.Where(x => x.EducationID == p.EducationID).Select(x => x.EducationLevel).FirstOrDefault() ?? "",
                                              MaritalStatusID = p.MaritalStatusID,
                                              MaritalStatusName = db.tblMaritalStatus.Where(x => x.MaritalStatusID == p.MaritalStatusID).Select(x => x.StatusName).FirstOrDefault() ?? "",
                                              CountryID = p.CountryID,
                                              CountryName = db.tblCountries.Where(x => x.CountryId == p.CountryID).Select(x => x.CountryName).FirstOrDefault() ?? "",
                                              ResidenceCityID = p.ResidenceCityID,
                                              ResidenceCityName = db.tblCities.Where(x => x.CityId == p.ResidenceCityID).Select(x => x.DisplayName).FirstOrDefault() ?? "",
                                              DetailedCityID = p.DetailedCityID,
                                              DetailedCityName = db.tblCities.Where(x => x.CityId == p.DetailedCityID).Select(x => x.DisplayName).FirstOrDefault() ?? "",
                                              DOB = p.DOB 
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


        public ActionResult UserDocumentList(int? id)
        {
            List<clsDocument> documents = new List<clsDocument>();
            documents = (from d in db.tblDocuments
                         where d.userId == id
                         select new clsDocument
                         {
                             DocumentId = d.DocumentId,
                             userId = d.userId,
                             Date = d.ExpiryDate,
                             DocumentTypeId = d.DocumentTypeId,
                             DocumentType = db.tblDocumentTypes.Where(x=>x.DocumentTypeID == d.DocumentTypeId).Select(x=>x.TypeName).FirstOrDefault() ?? "",
                             DocumentPath =  d.DocumentPath ?? "",
                             DocumentExtension = d.DocumentExtension ?? "",
                             Description = d.Description ?? "",
                             Status = d.Status ?? "",
                         }).ToList();

            return PartialView(documents);
        }

        public ActionResult UpdateDocumentStatus(int id,string status)
        {
            string message = "";
            bool returnstatus = false;

            clsResult result = new clsResult();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spVerifyUserDocuments", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@DocumentId", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = status;
                        cmd.Parameters.Add("@ActionByUserId", SqlDbType.Int).Value = User.UserID;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.Int).Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();
                        message = cmd.Parameters["@CheckReturn"].Value.ToString();
                        returnstatus = true;
                        cmd.Dispose();
                    }

                    con.Close();
                    con.Dispose();
                }
                catch (Exception ex)
                {
                    int? userId = User.UserID;
                    clsSqlErrorLog.InsertError(userId, ex.Message.ToString(), "Doc Status");
                    message = "Error !";
                }
            }
            
            return new JsonResult { Data = new { status = returnstatus, message = message } };
        }




        public ActionResult UserWorkList(int? id)
        {
            List<clsWork> documents = new List<clsWork>();
            documents = (from d in db.tblWorkInfoes 
                         where d.UserID == id
                         select new clsWork
                         {
                             WorkInfoID = d.WorkInfoID,
                             UserID = d.UserID,
                             DOJ = d.DOJ,

                             IncomeSourceID = d.IncomeSourceID,
                             IncomeSourceName = db.tblIncomeSources.Where(x => x.IncomeSourceID == d.IncomeSourceID).Select(x => x.SourceName).FirstOrDefault() ?? "",
                             EmploymentStatusID = d.EmploymentStatusID,
                             EmploymentStatusName = db.tblEmploymentStatus.Where(x => x.EmploymentStatusID == d.EmploymentStatusID).Select(x => x.StatusName).FirstOrDefault() ?? "",

                             OrganizationName = d.OrganizationName ?? "",
                             OrganizationPhoneNumber = d.OrganizationPhoneNumber ?? "",
                             OrganizationAddress = d.OrganizationAddress ?? "",
                             MonthlyIncome = d.MonthlyIncome ?? 0,

                         }).ToList();

            return PartialView(documents);
        }



        public ActionResult UpdateUserStatus(int id, string status)
        {
            string message = "";
            bool returnstatus = false;

            int countPending = db.tblDocuments.Where(x => x.userId == id && x.Status.ToLower() != ("verified").ToLower()).Count();

            if (countPending == 0)
            {
                clsResult result = new clsResult();
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["ADO"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("spVerifyUser", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = id;
                            cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = status;
                            cmd.Parameters.Add("@ActionByUserId", SqlDbType.Int).Value = User.UserID;
                            cmd.Parameters.Add("@CheckReturn", SqlDbType.Int).Direction = ParameterDirection.Output;

                            cmd.ExecuteNonQuery();
                            message = cmd.Parameters["@CheckReturn"].Value.ToString();
                            returnstatus = true;
                            cmd.Dispose();
                        }

                        con.Close();
                        con.Dispose();
                    }
                    catch (Exception ex)
                    {
                        int? userId = User.UserID;
                        clsSqlErrorLog.InsertError(userId, ex.Message.ToString(), "User Status");
                        message = "Error !";
                    }
                }

                return new JsonResult { Data = new { status = returnstatus, message = message } };
            }
            else
            {
                return new JsonResult { Data = new { status = false, message = "First Verify all Documents" } };
            }

            
        }

        #endregion
        #region Active Users
        public ActionResult LoanUserIndex()
        {
            if (User != null)
            {
                if (menurights.MenuRights(User.UserID, "frmLoanUser", "View") != 0)
                {
                    List<clsModule> actionList = new List<clsModule>();
                    actionList = (from r in db.tblUserMenuRoles
                                  join m in db.tblMenuModules on r.ModuleID equals m.ModuleID
                                  join a in db.tblMenuActions on r.ActionID equals a.ActionID
                                  where r.UserID == User.UserID
                                  && m.ModuleName.ToLower() == ("frmLoanUser").ToLower()
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


        public ActionResult GetAllActiveLoanUsers()
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
                                          join p in db.tblPersonalInfoes on f.UserId equals p.UserID
                                          where f.type.ToLower() == ("User").ToLower() &&
                                          f.Status.ToLower() == ("Active").ToLower()
                                          select new clsUser
                                          {
                                              UserId = f.UserId,
                                              Email = f.Email ?? "",
                                              PhoneNumber = f.PhoneNumber ?? "",
                                              Status = f.Status,
                                              Type = f.type ?? "",

                                              ImagePath = ("/FormsDocuments/LoanUser/" + f.UserId + ".jpg") ?? "",
                                              CreatedDate = f.CreatedDate,
                                              UpdatedDate = f.UpdatedDate,
                                              UpdatedByString = db.tblUsers.Where(x => x.UserId == f.UpdatedBy).Select(x => x.Email).FirstOrDefault() ?? "",
                                              PersonalInfoID = p.PersonalInfoID,
                                              FullName = p.FullName ?? "",
                                              SocialInsuranceNumber = p.SocialInsuranceNumber ?? "",
                                              ResidentialAddress = p.ResidentialAddress ?? "",
                                              DetailAddress = p.DetailAddress ?? "",
                                              GenderID = p.GenderID,
                                              GenderName = db.tblGenders.Where(x => x.GenderID == p.GenderID).Select(x => x.GenderName).FirstOrDefault() ?? "",
                                              EducationID = p.EducationID,
                                              EducationName = db.tblEducations.Where(x => x.EducationID == p.EducationID).Select(x => x.EducationLevel).FirstOrDefault() ?? "",
                                              MaritalStatusID = p.MaritalStatusID,
                                              MaritalStatusName = db.tblMaritalStatus.Where(x => x.MaritalStatusID == p.MaritalStatusID).Select(x => x.StatusName).FirstOrDefault() ?? "",
                                              CountryID = p.CountryID,
                                              CountryName = db.tblCountries.Where(x => x.CountryId == p.CountryID).Select(x => x.CountryName).FirstOrDefault() ?? "",
                                              ResidenceCityID = p.ResidenceCityID,
                                              ResidenceCityName = db.tblCities.Where(x => x.CityId == p.ResidenceCityID).Select(x => x.DisplayName).FirstOrDefault() ?? "",
                                              DetailedCityID = p.DetailedCityID,
                                              DetailedCityName = db.tblCities.Where(x => x.CityId == p.DetailedCityID).Select(x => x.DisplayName).FirstOrDefault() ?? "",
                                              DOB = p.DOB
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
        #endregion


    }
}