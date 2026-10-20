using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;
using System.Data;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace POS.Controllers
{
    public class ChartOfAccountController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region First Level
        // GET: /ChartOfAccount/
        public ActionResult FirstLevel()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "FirstLevel"
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
        public ActionResult getMaxCodeForFirstLevel()
        {
            decimal maxCode = db.AC_ChartOfAC.Where(x => x.AcLevel == 1).OrderByDescending(x => x.AccountCode).Select(x => x.AccountCode).FirstOrDefault();
            if (maxCode == 0)
            {
                maxCode = 10;
            }
            else
            {
                maxCode += 1;
            }
            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult InsertUpdateFirstLevel(clsChartOfAccount firstForm)
        {
            string message = "";
            bool status = false;
            if (firstForm.IsNeWOrEdit == "Edit")
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAccountLevel(firstForm.AccountCode, firstForm.AccountCode.ToString(), "", firstForm.Description, 1, "", firstForm.ReportType, "", 0, 0,User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "First Level Successfully Updated";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAccountLevel(firstForm.AccountCode, firstForm.AccountCode.ToString(), "", firstForm.Description, 1, "", firstForm.ReportType, "", 0, 0, User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "First Level Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult GetFirstLevelById(decimal id)
        {
            clsChartOfAccount acc = new clsChartOfAccount();
            acc = (from f in db.AC_ChartOfAC
                   where f.AccountCode == id
                   select new clsChartOfAccount
                   {
                       AccountCode = f.AccountCode,
                       AccountCodeV = f.AccountCodeV,
                       RefKey = f.RefKey,
                       Description = f.Description,
                       AcLevel = f.AcLevel,
                       Status = f.Status,
                       Remarks = f.Remarks,
                       ReportType = f.ReportType
                   }).FirstOrDefault();
            var data = acc;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FirstLevelsList()
        {
            return PartialView();
        }
        public ActionResult GetAllFirstLevels()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var accountCode = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var reportType = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var description = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsChartOfAccount> firstLevel = (from f in db.AC_ChartOfAC
                                                      where f.AcLevel == 1
                                                      select new clsChartOfAccount
                                                      {
                                                        AccountCode = f.AccountCode,
                                                        AccountCodeV = f.AccountCodeV,
                                                        RefKey = f.RefKey,
                                                        Description = f.Description,
                                                        AcLevel = f.AcLevel,
                                                        Status = f.Status,
                                                        Remarks = f.Remarks,
                                                        ReportType = f.ReportType
                                                      }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(accountCode))
                {
                    firstLevel = firstLevel.Where(c => c.AccountCodeV.Contains(accountCode)).ToList();
                }
                else if (!string.IsNullOrEmpty(reportType))
                {
                    firstLevel = firstLevel.Where(c => c.ReportType.ToLower().Contains(reportType.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    firstLevel = firstLevel.Where(c => c.Description.ToLower().Contains(description.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        firstLevel = firstLevel.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = firstLevel.Count();
                var data = firstLevel.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteFirstLevel(decimal id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateAccountLevel(id, "", "", "", 1, "", "", "", 0, 0, User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "First Level Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this level first.";
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
        #region Second Level
        public ActionResult SecondLevel()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SecondLevel"
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
                    ViewBag.FirstLevel = new SelectList(getFirstLevels(), "AccountCode", "Description");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public List<clsChartOfAccount> getFirstLevels()
        {
            List<clsChartOfAccount> firstLevel = new List<clsChartOfAccount>();
            firstLevel = (from c in db.AC_ChartOfAC
                          where c.AcLevel == 1
                          select new clsChartOfAccount
                       {
                           AccountCode = c.AccountCode,
                           Description = c.Description
                       }).ToList();
            return firstLevel;
        }
        public ActionResult getMaxCodeForSecondLevel(decimal accountCode)
        {
            string accountCodeV = db.AC_ChartOfAC.Where(x => x.AccountCode == accountCode).Select(x => x.AccountCodeV).FirstOrDefault();
            string secondLevelCode = "";
            string secondLevelCodeV = "";
            decimal? maxCode = db.AC_ChartOfAC.Where(x => x.AcLevel == 2 && x.RefKey == accountCode.ToString()).OrderByDescending(x => x.AccountCode).Select(x => x.AccountCode).FirstOrDefault();
            if (maxCode == 0)
            {
                secondLevelCode = accountCode + "01";
                secondLevelCodeV = accountCode + "-" + "01";
            }
            else
            {
                string accV = db.AC_ChartOfAC.Where(x => x.AccountCode == maxCode).Select(x => x.AccountCodeV).FirstOrDefault().ToString().Split('-')[1];
                decimal code = Convert.ToDecimal(accV) + 1;
                if (code.ToString().Length == 1)
                {
                    accV = "0" + code.ToString();
                }
                else
                {
                    accV = code.ToString();
                }
                maxCode = maxCode + 1;
                secondLevelCode = maxCode.ToString();
                secondLevelCodeV = accountCodeV + "-" + accV;
            }
            return new JsonResult { Data = new { secondLevelCode = secondLevelCode, secondLevelCodeV = secondLevelCodeV } };
        }
        [HttpPost]
        public ActionResult InsertUpdateSecondLevel(clsChartOfAccount firstForm)
        {
            string message = "";
            bool status = false;
            if (firstForm.IsNeWOrEdit == "Edit")
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAccountLevel(firstForm.AccountCode, firstForm.AccountCodeV, firstForm.FirstLevelAccountCode, firstForm.Description, 2, "", "", "", 0, 0, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Second Level Successfully Updated";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAccountLevel(firstForm.AccountCode, firstForm.AccountCodeV, firstForm.FirstLevelAccountCode, firstForm.Description, 2, "", "", "", 0, 0, User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Second Level Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult GetSecondLevelById(decimal id)
        {
            clsChartOfAccount acc = new clsChartOfAccount();
            acc = (from f in db.AC_ChartOfAC
                   where f.AccountCode == id
                   select new clsChartOfAccount
                   {
                       AccountCode = f.AccountCode,
                       AccountCodeV = f.AccountCodeV,
                       RefKey = f.RefKey,
                       Description = f.Description,
                       AcLevel = f.AcLevel,
                       Status = f.Status,
                       Remarks = f.Remarks,
                       ReportType = f.ReportType
                   }).FirstOrDefault();
            var data = acc;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SecondLevelsList()
        {
            return PartialView();
        }
        public ActionResult GetAllSecondLevels()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var accountCode = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var reference = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var description = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsChartOfAccount> secondLevel = (from f in db.AC_ChartOfAC
                                                       where f.AcLevel == 2
                                                       select new clsChartOfAccount
                                                       {
                                                           AccountCode = f.AccountCode,
                                                           AccountCodeV = f.AccountCodeV,
                                                           RefKey = db.AC_ChartOfAC.Where(x => x.AccountCode.ToString() == f.RefKey).Select(x => x.Description).FirstOrDefault(),
                                                           Description = f.Description,
                                                           AcLevel = f.AcLevel,
                                                           Status = f.Status,
                                                           Remarks = f.Remarks,
                                                           ReportType = f.ReportType
                                                       }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(accountCode))
                {
                    secondLevel = secondLevel.Where(c => c.AccountCodeV.Contains(accountCode)).ToList();
                }
                else if (!string.IsNullOrEmpty(reference))
                {
                    secondLevel = secondLevel.Where(c => c.RefKey.ToLower().Contains(reference.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    secondLevel = secondLevel.Where(c => c.Description.ToLower().Contains(description.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        secondLevel = secondLevel.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = secondLevel.Count();
                var data = secondLevel.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteSecondLevel(decimal id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateAccountLevel(id, "", "", "", 2, "", "", "", 0, 0, User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Second Level Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this level first.";
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
        #region Third Level
        public ActionResult ThirdLevel()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "ThirdLevel"
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
                    ViewBag.SecondLevel = new SelectList(getSecondLevels(), "AccountCode", "Description");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public List<clsChartOfAccount> getSecondLevels()
        {
            List<clsChartOfAccount> firstLevel = new List<clsChartOfAccount>();
            firstLevel = (from c in db.AC_ChartOfAC
                          where c.AcLevel == 2
                          select new clsChartOfAccount
                          {
                              AccountCode = c.AccountCode,
                              Description = c.Description
                          }).ToList();
            return firstLevel;
        }
        public ActionResult getMaxCodeForThirdLevel(decimal accountCode)
        {
            string accountCodeV = db.AC_ChartOfAC.Where(x => x.AccountCode == accountCode).Select(x => x.AccountCodeV).FirstOrDefault();
            string thirdLevelCode = "";
            string thirdLevelCodeV = "";
            decimal? maxCode = db.AC_ChartOfAC.Where(x => x.AcLevel == 3 && x.RefKey == accountCode.ToString()).OrderByDescending(x => x.AccountCode).Select(x => x.AccountCode).FirstOrDefault();
            if (maxCode == 0)
            {
                thirdLevelCode = accountCode + "0001";
                thirdLevelCodeV = accountCodeV + "-" + "0001";
            }
            else
            {
                string accV = db.AC_ChartOfAC.Where(x => x.AccountCode == maxCode).Select(x => x.AccountCodeV).FirstOrDefault().ToString().Split('-')[2];
                decimal code = Convert.ToDecimal(accV) + 1;
                if (code.ToString().Length == 1)
                {
                    accV = "000" + code.ToString();
                }
                else if (code.ToString().Length == 1)
                {
                    accV = "00" + code.ToString();
                }
                else if (code.ToString().Length == 1)
                {
                    accV = "0" + code.ToString();
                }
                maxCode = maxCode + 1;
                thirdLevelCode = maxCode.ToString();
                thirdLevelCodeV = accountCodeV + "-" + accV;
            }
            return new JsonResult { Data = new { thirdLevelCode = thirdLevelCode, thirdLevelCodeV = thirdLevelCodeV } };
        }
        [HttpPost]
        public ActionResult InsertUpdateThirdLevel(clsChartOfAccount firstForm)
        {
            string message = "";
            bool status = false;
            if (firstForm.IsNeWOrEdit == "Edit")
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAccountLevel(firstForm.AccountCode, firstForm.AccountCodeV, firstForm.SecondLevelAccountCode, firstForm.Description, 3, firstForm.Status, "", "", 0, 0, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Third Level Successfully Updated";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAccountLevel(firstForm.AccountCode, firstForm.AccountCodeV, firstForm.SecondLevelAccountCode, firstForm.Description, 3, "Active", "", "", 0, 0, User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Third Level Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult GetThirdLevelById(decimal id)
        {
            clsChartOfAccount acc = new clsChartOfAccount();
            acc = (from f in db.AC_ChartOfAC
                   where f.AccountCode == id
                   select new clsChartOfAccount
                   {
                       AccountCode = f.AccountCode,
                       AccountCodeV = f.AccountCodeV,
                       RefKey = f.RefKey,
                       Description = f.Description,
                       AcLevel = f.AcLevel,
                       Status = f.Status,
                       Remarks = f.Remarks,
                       ReportType = f.ReportType
                   }).FirstOrDefault();
            var data = acc;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ThirdLevelsList()
        {
            return PartialView();
        }
        public ActionResult GetAllThirdLevels()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var accountCode = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var reference = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var description = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsChartOfAccount> thirdLevel = (from f in db.AC_ChartOfAC
                                                      where f.AcLevel == 3
                                                      select new clsChartOfAccount
                                                      {
                                                          AccountCode = f.AccountCode,
                                                          AccountCodeV = f.AccountCodeV,
                                                          RefKey = db.AC_ChartOfAC.Where(x => x.AccountCode.ToString() == f.RefKey).Select(x => x.Description).FirstOrDefault(),
                                                          Description = f.Description,
                                                          AcLevel = f.AcLevel,
                                                          Status = f.Status,
                                                          Remarks = f.Remarks,
                                                          ReportType = f.ReportType
                                                      }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(accountCode))
                {
                    thirdLevel = thirdLevel.Where(c => c.AccountCodeV.Contains(accountCode)).ToList();
                }
                else if (!string.IsNullOrEmpty(reference))
                {
                    thirdLevel = thirdLevel.Where(c => c.RefKey.ToLower().Contains(reference.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    thirdLevel = thirdLevel.Where(c => c.Description.ToLower().Contains(description.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    thirdLevel = thirdLevel.Where(c => c.Status.ToLower() == status.ToLower()).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        thirdLevel = thirdLevel.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = thirdLevel.Count();
                var data = thirdLevel.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteThirdLevel(decimal id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateAccountLevel(id, "", "", "", 3, "", "", "", 0, 0,User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Third Level Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this level first.";
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
        #region Account Detail
        public ActionResult AccountDetail()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "AccountDetail"
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
                    ViewBag.ThirdLevel = new SelectList(getThirdLevels(), "AccountCode", "Description");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult getMaxCodeForDetail()
        {
            decimal maxCode = db.AC_Detail.OrderByDescending(x => x.DetailCode).Select(x => x.DetailCode).FirstOrDefault();
            if (maxCode == 0)
            {
                maxCode = 10;
            }
            else
            {
                maxCode += 1;
            }
            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }
        public List<clsChartOfAccount> getThirdLevels()
        {
            List<clsChartOfAccount> firstLevel = new List<clsChartOfAccount>();
            firstLevel = (from c in db.AC_ChartOfAC
                          where c.AcLevel == 3 & c.Status == "Active"
                          select new clsChartOfAccount
                          {
                              AccountCode = c.AccountCode,
                              Description = c.Description
                          }).ToList();
            return firstLevel;
        }
        [HttpPost]
        public ActionResult InsertUpdateAccountDetail(clsAccountDetail detail)
        {
            string message = "";
            bool status = false;
            if (detail.CityId == null)
            {
                detail.CityId = 0;
            }
            if (detail.IsNeWOrEdit == "Edit")
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAcDetail(detail.DetailCode, detail.Name, 0, detail.Status, detail.AcDescription, "", detail.AccountCode, "Detail", detail.UrduName, detail.Phone1, detail.Phone2, detail.Cell1, detail.Cell2, detail.Fax, detail.Email, detail.Website, detail.Address, detail.CreditDays, detail.CreditLimit, detail.TaxType, detail.GST, detail.NTN, detail.CNIC, "Short", detail.Remarks, User.UserId, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Account Detail Successfully Updated";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAcDetail(detail.DetailCode, detail.Name, 0, "Active", detail.AcDescription, "", detail.AccountCode, "Detail", detail.UrduName, detail.Phone1, detail.Phone2, detail.Cell1, detail.Cell2, detail.Fax, detail.Email, detail.Website, detail.Address, detail.CreditDays, detail.CreditLimit, detail.TaxType, detail.GST, detail.NTN, detail.CNIC, "Short", detail.Remarks, User.UserId, User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Account Detail Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult GetAccountDetailById(decimal id)
        {
            clsAccountDetail accDetail = new clsAccountDetail();
            clsAccountDetail accDetailExtra = new clsAccountDetail();
            DateTime defaultdate = DateTime.Now.Date;

            accDetail = (from f in db.AC_Detail
                         join fr in db.AC_Detail_Extra on f.DetailCode equals fr.DetailCode
                         where f.DetailCode == id
                         select new clsAccountDetail
                         {
                             DetailCode = f.DetailCode,
                             Name = f.Name,
                             CityId = fr.CityId,
                             CityName = db.Cities.Where(x=>x.CityId == fr.CityId).Select(x=>x.CityName).FirstOrDefault(),
                             Status = f.Status,
                             AcDescription = f.AcDescription,
                             AccountCodeV = f.AccountCodeV,
                             AccountCode = f.AccountCode,
                             Type = f.Type,
                             UrduName = f.UrduName,
                             Phone1 = fr.Phone1,
                             Phone2 = fr.Phone2,
                             Cell1 = fr.Cell1,
                             Cell2 = fr.Cell2,
                             Fax = fr.Fax,
                             Email = fr.Email,
                             Website = fr.Website,
                             Address = fr.Address,
                             CreditDays = fr.CreditDays,
                             CreditLimit = fr.CreditLimit,
                             TaxType = fr.TaxType,
                             GST = fr.GST,
                             NTN = fr.NTN,
                             CNIC = fr.CNIC,
                             Remarks = fr.Remarks,
                             SalesmanID = fr.SalesmanID,
                             AdminUser=fr.AdminUser ?? "",
                             AdminPassword = fr.AdminPassword ?? "",

                             IssueDate = fr.IssueDate ?? defaultdate,
                             ExpiryDate = fr.ExpiryDate ?? defaultdate,
                            FirstName=fr.FirstName,
                            LastName=fr.LastName,
                            Position=fr.Position,
                            CommEmail=fr.CommEmail,
                            BookingEmail=fr.BookingEmail,
                            AccountName=fr.AccountName,
                            ACNumber=fr.ACNumber,
                            SortCode=fr.SortCode,
                             BankName = fr.BankName,

                             UrgentPick=fr.UrgentPick ?? "No",
                             UrgentPickTime = fr.UrgentPickTime ?? 0,

                             BankAddress =fr.BankAddress ?? "",
                             WorkFrom = fr.WorkFrom,
                             WorkUpTo = fr.WorkUpTo,

                             Monday = fr.Monday ?? "N",
                             Tuesday = fr.Tuesday ?? "N",
                             Wednesday = fr.Wednesday ?? "N",
                             Thursday = fr.Thursday ?? "N",
                             Friday = fr.Friday ?? "N",
                             Saturday = fr.Saturday ?? "N",
                             Sunday = fr.Sunday ?? "N",

                             ImageName = db.Form_Document.Where(x => x.TypeId == f.DetailCode.ToString() && x.DocumentType == "Driver" && x.Remarks == "Driver Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "Nothing",
                         }).FirstOrDefault();
            var data = accDetail;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AccountDetailsList()
        {
            return PartialView();
        }
        public ActionResult GetAllAccountDetails()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var detailCode = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var description = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var reference = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var urduName = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsAccountDetail> detail = (from f in db.AC_Detail
                                                 join fr in db.AC_Detail_Extra on f.DetailCode equals fr.DetailCode
                                                 where f.Type == "Detail"
                                                 orderby f.Name
                                                 select new clsAccountDetail
                                                 {
                                                     DetailCode = f.DetailCode,
                                                     Name = f.Name,
                                                     CityId = fr.CityId,
                                                     CityName = db.Cities.Where(x => x.CityId == fr.CityId).Select(x => x.CityName).FirstOrDefault(),
                                                     Status = f.Status,
                                                     AcDescription = f.AcDescription,
                                                     AccountCodeV = f.AccountCodeV,
                                                     AccountCode = f.AccountCode,
                                                     Reference = db.AC_ChartOfAC.Where(x => x.AccountCode == f.AccountCode).Select(x => x.Description).FirstOrDefault(),
                                                     Type = f.Type,
                                                     UrduName = f.UrduName,
                                                     Phone1 = fr.Phone1,
                                                     Phone2 = fr.Phone2,
                                                     Cell1 = fr.Cell1,
                                                     Cell2 = fr.Cell2,
                                                     Fax = fr.Fax,
                                                     Email = fr.Email,
                                                     Website = fr.Website,
                                                     Address = fr.Address,
                                                     CreditDays = fr.CreditDays,
                                                     CreditLimit = fr.CreditLimit,
                                                     TaxType = fr.TaxType,
                                                     //TaxTypeInString = db
                                                     GST = fr.GST,
                                                     NTN = fr.NTN,
                                                     CNIC = fr.CNIC,
                                                     Remarks = fr.Remarks,
                                                     SalesmanID = fr.SalesmanID
                                                 }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(detailCode))
                {
                    detail = detail.Where(c => c.DetailCode.ToString().Contains(detailCode)).ToList();
                }
                else if (!string.IsNullOrEmpty(detailName))
                {
                    detail = detail.Where(c => c.Name.ToLower().Contains(detailName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    detail = detail.Where(c => c.AcDescription.ToLower().Contains(description.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    detail = detail.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(reference))
                {
                    detail = detail.Where(c => c.Reference.ToLower().Contains(reference.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(urduName))
                {
                    detail = detail.Where(c => c.UrduName.Contains(urduName)).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        detail = detail.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = detail.Count();
                var data = detail.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteAccountDetail(decimal id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateAcDetail(id, "", 0, "", "", "", 0, "Detail", "", "", "", "", "", "", "", "", "", 0, 0, "", "", "", "", "Short", "", User.UserId, User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Account Detail Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this account first.";
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
        #region Customer/Supplier Detail
        public ActionResult CustomerSupplierDetail()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CustomerSupplierDetail"
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
                    //ViewBag.ThirdLevel = new SelectList(getThirdLevels(), "AccountCode", "Description");
                    ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public List<clsCabType> getCabType()
        {
            List<clsCabType> cabtype = new List<clsCabType>();
            cabtype = (from c in db.Cab_CabType 
                    select new clsCabType
                    {
                        CabTypeID = c.CabTypeID,
                        Description = c.Description
                    }).ToList();
            return cabtype;
        }
        public List<clsCabZone> getZone()
        {
            List<clsCabZone> city = new List<clsCabZone>();
            city = (from c in db.Cab_Zone
                    select new clsCabZone
                    {
                        ZoneID = c.ZoneID,
                        Description = c.Description
                    }).ToList();
            return city;
        }
        public List<clsCity> getCities()
        {
            List<clsCity> city = new List<clsCity>();
            city = (from c in db.Cities
                    select new clsCity
                          {
                              CityId = c.CityId,
                              CityName = c.CityName
                          }).ToList();
            return city;
        }
        public List<clsSalesman> getSalesman()
        {
            List<clsSalesman> salesman = new List<clsSalesman>();
            salesman = (from c in db.Admin_Setup_Salesman
                        where c.Status == "Active"
                        select new clsSalesman
                        {
                            SalesmanID = c.SalesmanID,
                            FullName = c.FullName
                        }).ToList();
            return salesman;
            //return Json(new { data = data, data2 = data2 }, JsonRequestBehavior.AllowGet);
        }
        public List<clsTaxRate> getTaxes()
        {
            List<clsTaxRate> tax = new List<clsTaxRate>();
            tax = (from c in db.Admin_Setup_Tax
                   where c.Status == "Active"
                   select new clsTaxRate
                    {
                        TaxID = c.TaxID,
                        TaxName = c.TaxName
                    }).ToList();
            return tax;
        }
        public ActionResult getThirdLevelsList(string type)
        {
            List<clsChartOfAccount> thirdLevel = new List<clsChartOfAccount>();
            if(type == "Customer + Supplier")
            {
                thirdLevel = (from c in db.AC_DefaultGL
                              join ac in db.AC_ChartOfAC on c.AccountCode equals ac.AccountCode
                              where c.Type == "Customer" || c.Type == "Supplier"
                              select new clsChartOfAccount
                              {
                                  AccountCode = ac.AccountCode,
                                  Description = ac.Description
                              }).ToList();
            }
            else
            {
                thirdLevel = (from c in db.AC_DefaultGL
                              join ac in db.AC_ChartOfAC on c.AccountCode equals ac.AccountCode
                              where c.Type == type
                              select new clsChartOfAccount
                              {
                                  AccountCode = ac.AccountCode,
                                  Description = ac.Description
                              }).ToList();
            }
            var data = thirdLevel.ToList();
            return new JsonResult { Data = new { data = data } };
        }
        [HttpPost]
        public ActionResult InsertUpdateCustomerSupplierDetail(clsAccountDetail detail)
        {
            string message = "";
            bool status = false;
            if (detail.CityId == null)
            {
                detail.CityId = 0;
            }
            if (detail.IsNeWOrEdit == "Edit")
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAcDetail(detail.DetailCode, detail.Name, detail.CityId, detail.Status, detail.AcDescription, "", detail.AccountCode, detail.Type, detail.UrduName, detail.Phone1, detail.Phone2, detail.Cell1, detail.Cell2, detail.Fax, detail.Email, detail.Website, detail.Address, detail.CreditDays, detail.CreditLimit, detail.TaxType, detail.GST, detail.NTN, detail.CNIC, "Long", detail.Remarks, detail.SalesmanID, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Account Detail Successfully Updated";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateAcDetail(detail.DetailCode, detail.Name, detail.CityId, "Active", detail.AcDescription, "", detail.AccountCode, detail.Type, detail.UrduName, detail.Phone1, detail.Phone2, detail.Cell1, detail.Cell2, detail.Fax, detail.Email, detail.Website, detail.Address, detail.CreditDays, detail.CreditLimit, detail.TaxType, detail.GST, detail.NTN, detail.CNIC, "Long", detail.Remarks, detail.SalesmanID, User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Account Detail Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult CustomerSupplierDetailsList()
        {
            return PartialView();
        }
        public ActionResult GetAllCustomerSupplierDetails()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var detailCode = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var description = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var reference = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var urduName = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                var type = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
                var city = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsAccountDetail> detail = (from f in db.AC_Detail
                                                 join fr in db.AC_Detail_Extra on f.DetailCode equals fr.DetailCode
                                                 where f.Type == "Customer" || f.Type == "Supplier" || f.Type == "Customer + Supplier"
                                                 orderby f.Name
                                                 select new clsAccountDetail
                                                 {
                                                     DetailCode = f.DetailCode,
                                                     Name = f.Name,
                                                     CityId = fr.CityId,
                                                     CityName = db.Cities.Where(x => x.CityId == fr.CityId).Select(x => x.CityName).FirstOrDefault() ??  string.Empty,
                                                     Status = f.Status,
                                                     AcDescription = f.AcDescription,
                                                     AccountCodeV = f.AccountCodeV,
                                                     AccountCode = f.AccountCode,
                                                     Reference = db.AC_ChartOfAC.Where(x => x.AccountCode == f.AccountCode).Select(x => x.Description).FirstOrDefault(),
                                                     Type = f.Type,
                                                     UrduName = f.UrduName,
                                                     Phone1 = fr.Phone1,
                                                     Phone2 = fr.Phone2,
                                                     Cell1 = fr.Cell1,
                                                     Cell2 = fr.Cell2,
                                                     Fax = fr.Fax,
                                                     Email = fr.Email,
                                                     Website = fr.Website,
                                                     Address = fr.Address,
                                                     CreditDays = fr.CreditDays,
                                                     CreditLimit = fr.CreditLimit,
                                                     TaxType = fr.TaxType,
                                                     GST = fr.GST,
                                                     NTN = fr.NTN,
                                                     CNIC = fr.CNIC,
                                                     Remarks = fr.Remarks,
                                                     SalesmanID = fr.SalesmanID
                                                 }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(detailCode))
                {
                    detail = detail.Where(c => c.DetailCode.ToString().Contains(detailCode)).ToList();
                }
                else if (!string.IsNullOrEmpty(detailName))
                {
                    detail = detail.Where(c => c.Name.ToLower().Contains(detailName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    detail = detail.Where(c => c.AcDescription.ToLower().Contains(description.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    detail = detail.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(reference))
                {
                    detail = detail.Where(c => c.Reference.ToLower().Contains(reference.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(urduName))
                {
                    detail = detail.Where(c => c.UrduName.Contains(urduName)).ToList();
                }
                else if (!string.IsNullOrEmpty(type))
                {
                    detail = detail.Where(c => c.Type.ToLower().Contains(type.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(city))
                {
                    detail = detail.Where(c => c.CityName.ToLower().Contains(city.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        detail = detail.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = detail.Count();
                var data = detail.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult ItemsListForPrice(decimal detailCode)
        {
            List<clsItem> items = (from f in db.Item_Information
                                        where f.Status == "Active"
                                        orderby f.ItemName
                                        select new clsItem
                                        {
                                            ItemID = f.ItemID,
                                            Description = f.ItemName,
                                            PriceOrInch = db.AC_Detail_ItemPrice.Where(x => x.DetailCode == detailCode && x.ItemID == f.ItemID).Select(x => x.PriceOrInch).FirstOrDefault() ?? 0,
                                            DetailCode = detailCode
                                        }).ToList();
            return PartialView(items);
        }
        [HttpPost]
        public ActionResult InsertUpdateItemPrice(string data)
        {
            string message = "";
            bool status = false;
            int returnId = 0;
            List<clsItem> category = JsonConvert.DeserializeObject<List<clsItem>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("DetailCode");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("PriceOrInch");
            for (int i = 0; i < category.Count; i++)
            {
                dt.Rows.Add(new object[] { i + 1, category[i].DetailCode, category[i].ItemID, category[i].PriceOrInch });
            }
            returnId = AddEdit(dt);
            if (returnId > 0)
            {
                ModelState.Clear();
                status = true;
                message = "Record Successfully Updated";
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
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateItemPrice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@dtItemPrice", SqlDbType.Structured).Value = dt;
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
        #region AC Detail Sub

        public ActionResult CustomerSupplierDetailSub(decimal detailCode)
        {
            ViewBag.DetailCode = detailCode;

            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CustomerSupplierDetail"
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
        public ActionResult insertupdateAcDetailSub(clsACDetailSub acdetailsub)
        {
            string message = "";
            bool status = false;
            if (acdetailsub.SubDetailCode > 0)
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateACDetailSub(acdetailsub.SubDetailCode, acdetailsub.DetailCode, acdetailsub.Name, acdetailsub.Address, acdetailsub.Cell1, acdetailsub.Cell2, acdetailsub.Email, acdetailsub.Remarks, acdetailsub.UrduName, acdetailsub.Status, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Sub Customer Successfully Updated";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateACDetailSub(acdetailsub.SubDetailCode,acdetailsub.DetailCode, acdetailsub.Name, acdetailsub.Address, acdetailsub.Cell1, acdetailsub.Cell2, acdetailsub.Email, acdetailsub.Remarks, acdetailsub.UrduName, "Active", User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Sub Customer Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult GetACDetailSubById(int id)
        {
            clsACDetailSub acdetailsub = new clsACDetailSub();
            acdetailsub = (from f in db.AC_Detail_Sub 
                        where f.SubDetailCode == id
                           select new clsACDetailSub
                        {
                            SubDetailCode = f.SubDetailCode,
                            DetailCode = f.DetailCode,
                            Name = f.Name,
                            Address = f.Address,
                            Cell1 = f.Cell1,
                            Cell2 = f.Cell2,
                            Email = f.Email,
                            Remarks = f.Remarks,
                            UrduName = f.UrduName,
                            Status = f.Status
                        }).FirstOrDefault();
            var data = acdetailsub;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllACDetailSub(decimal detailCode)
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var salesmanName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var email = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsACDetailSub> salesman = (from f in db.AC_Detail_Sub
                                                 where f.DetailCode == detailCode
                                                 select new clsACDetailSub
                                              {
                                                  SubDetailCode = f.SubDetailCode,
                                                  DetailCode = f.DetailCode,
                                                  Name = f.Name,
                                                  Address = f.Address,
                                                  Cell1 = f.Cell1,
                                                  Cell2 = f.Cell2,
                                                  Email = f.Email,
                                                  Remarks = f.Remarks,
                                                  UrduName = f.UrduName,
                                                  Status = f.Status
                                              }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(salesmanName))
                {
                    salesman = salesman.Where(c => c.Name.ToLower().Contains(salesmanName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(email))
                {
                    salesman = salesman.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    salesman = salesman.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    salesman = salesman.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        salesman = salesman.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = salesman.Count();
                var data = salesman.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteACDetailSub(int id)
        {
            string message = "";
            bool status = false;
            clsACDetailSub salesman = new clsACDetailSub();
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateACDetailSub(id,0, "", "", "", "", "", "", "", "Active", User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Sub Customer Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this sub customer first.";
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



        #region Supplier Detail
        public ActionResult SupplierDetail()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CustomerSupplierDetail"
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
                    //ViewBag.ThirdLevel = new SelectList(getThirdLevels(), "AccountCode", "Description");
                    ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    ViewBag.Zones = new SelectList(getZone(), "ZoneID", "Description");
                    
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        
        [HttpPost]
        public ActionResult InsertUpdateSupplierDetail(clsAccountDetail detail, string data, string vehdata, string docdata)
        {
            string message = "";
            bool status = false;
            if (detail.CityId == null)
            {
                detail.CityId = 0;
            }

            List<clsAccountDetail> invoice=new List<clsAccountDetail>();
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ZoneID");

            if (data == null || data == "")
            {
                dt.Rows.Add(new object[] { 0, 0});
            }
            else
            {
                invoice = JsonConvert.DeserializeObject<List<clsAccountDetail>>(data);
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ZoneID });
                }
            }

            

            List<clsAccountDetail> vehlist = new List<clsAccountDetail>();
            DataTable vehdt = new DataTable();
            vehdt.Columns.Add("Id");
            vehdt.Columns.Add("CabTypeID");
            vehdt.Columns.Add("NoOfVeh");
            vehdt.Columns.Add("RegNo");
            vehdt.Columns.Add("Make");
            vehdt.Columns.Add("Model");
            vehdt.Columns.Add("Colour");
            vehdt.Columns.Add("FuelType");
            
            if (vehdata == null || vehdata == "")
            {
                vehdt.Rows.Add(new object[] {0, 0, 0,"","","","",""});
            }
            else
            {
                vehlist = JsonConvert.DeserializeObject<List<clsAccountDetail>>(vehdata);
                for (int i = 0; i < vehlist.Count; i++)
                {
                    vehdt.Rows.Add(new object[] { i + 1, vehlist[i].CabTypeID, vehlist[i].NoOfVeh 
                    , vehlist[i].RegNo, vehlist[i].Make, vehlist[i].Model, vehlist[i].Colour, vehlist[i].FuelType});
                }
            }



            List<clsAccountDetail> doclist = new List<clsAccountDetail>();
            DataTable docdt = new DataTable();
            docdt.Columns.Add("Id");
            docdt.Columns.Add("DocumentTypeID");
            docdt.Columns.Add("DocumentNo");
            docdt.Columns.Add("ExpiryDate");
            docdt.Columns.Add("Status");

            if (docdata == null || docdata == "")
            {
                docdt.Rows.Add(new object[] { 0, 0,"", null, 0 });
            }
            else
            {
                doclist = JsonConvert.DeserializeObject<List<clsAccountDetail>>(docdata);
                for (int i = 0; i < doclist.Count; i++)
                {
                    docdt.Rows.Add(new object[] { i + 1, doclist[i].DocumentTypeID, doclist[i].DocumentNo, doclist[i].ExpiryDate, doclist[i].DocStatus });
                }
            }


            detail.AccountCode = (from c in db.AC_DefaultGL
                                    join ac in db.AC_ChartOfAC on c.AccountCode equals ac.AccountCode
                                    where c.Type == "Supplier"
                                    select ac.AccountCode).FirstOrDefault();

            string returnId = "";
            if (detail.IsNeWOrEdit == "Edit")
            {
                returnId = AddEditSupplier(detail.DetailCode, detail.Name, detail.CityId, detail.Status, detail.AcDescription, "", detail.AccountCode, detail.Type, detail.UrduName, detail.Phone1, detail.Phone2, detail.Cell1, detail.Cell2, detail.Fax, detail.Email, detail.Website, detail.Address, "", detail.CreditDays, detail.CreditLimit, detail.TaxType, detail.GST, detail.NTN, detail.CNIC, "Long", detail.Remarks, detail.SalesmanID, User.UserId, "Update", dt,detail.AdminUser,detail.AdminPassword ,
                        detail.IssueDate, detail.ExpiryDate, detail.FirstName, detail.LastName,
                        detail.Position, detail.CommEmail, detail.BookingEmail, detail.AccountName,
                        detail.ACNumber, detail.SortCode, detail.BankName, vehdt, detail.UrgentPick, detail.UrgentPickTime,
                        detail.WorkFrom, detail.WorkUpTo, detail.BankAddress, docdt
                        ,detail.Monday, detail.Tuesday, detail.Wednesday, detail.Thursday, detail.Friday, detail.Saturday, detail.Sunday);
                if (returnId == "1")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Suppliers Successfully Updated";
                }
                else if (returnId == "-9999")
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {

                returnId = AddEditSupplier(detail.DetailCode, detail.Name, detail.CityId, detail.Status, detail.AcDescription, "", detail.AccountCode, detail.Type, detail.UrduName, detail.Phone1, detail.Phone2, detail.Cell1, detail.Cell2, detail.Fax, detail.Email, detail.Website, detail.Address, "", detail.CreditDays, detail.CreditLimit, detail.TaxType, detail.GST, detail.NTN, detail.CNIC, "Long", detail.Remarks, detail.SalesmanID, User.UserId, "Save", dt, detail.AdminUser, detail.AdminPassword,
                                            detail.IssueDate, detail.ExpiryDate, detail.FirstName, detail.LastName,
                                            detail.Position, detail.CommEmail, detail.BookingEmail, detail.AccountName,
                                            detail.ACNumber, detail.SortCode, detail.BankName, vehdt, detail.UrgentPick, detail.UrgentPickTime,
                                            detail.WorkFrom, detail.WorkUpTo, detail.BankAddress, docdt
                                            , detail.Monday, detail.Tuesday, detail.Wednesday, detail.Thursday, detail.Friday, detail.Saturday, detail.Sunday);
                if (Convert.ToDecimal(returnId.ToString()) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Suppliers Successfully Saved";
                }
                else if (Convert.ToInt32(returnId.ToString()) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }


            return new JsonResult { Data = new { status = status, message = message } };
        }




        private string AddEditSupplier(decimal? detailCode, string name, int? cityId, string status, string acDescription, string accountCodeV, decimal? accountCode,
            string type, string urduName, string phone1, string phone2, string cell1, string cell2, string fax, string email, string website,
            string address, string taxaddress, int? creditDays, decimal? creditLimit, string taxType, string gST, string nTN, string cNIC, string shortOrLongDetail, string remarks, int? salesmanID,
            int? userId, string insertUpdateStatus, DataTable dt,string adminuser,string adminpassword,
            DateTime? IssueDate,DateTime? ExpiryDate,string FirstName,string LastName,
            string Position, string CommEmail, string BookingEmail,string AccountName,
            string ACNumber, string SortCode, string BankName, DataTable vehdt, string UrgentPick, int? UrgentPickTime,
            TimeSpan? WorkFrom, TimeSpan? WorkUpTo, string BankAddress, DataTable docdt,
            string Monday, string Tuesday, string Wednesday, string Thursday, string Friday, string Saturday, string Sunday)
        {





            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateAcDetailSupplier", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = detailCode;
                        cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = name;
                        cmd.Parameters.Add("@CityId", SqlDbType.Int).Value = cityId;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar).Value = status;
                        cmd.Parameters.Add("@AcDescription", SqlDbType.VarChar).Value = acDescription;
                        cmd.Parameters.Add("@AccountCodeV", SqlDbType.VarChar).Value = accountCodeV;
                        cmd.Parameters.Add("@AccountCode", SqlDbType.Decimal).Value = accountCode;
                        cmd.Parameters.Add("@Type", SqlDbType.VarChar).Value = type;
                        cmd.Parameters.Add("@UrduName", SqlDbType.VarChar).Value = urduName;
                        cmd.Parameters.Add("@Phone1", SqlDbType.VarChar).Value = phone1;
                        cmd.Parameters.Add("@Phone2", SqlDbType.VarChar).Value = phone2;
                        cmd.Parameters.Add("@Cell1", SqlDbType.VarChar).Value = cell1;
                        cmd.Parameters.Add("@Cell2", SqlDbType.VarChar).Value = cell2;
                        cmd.Parameters.Add("@Fax", SqlDbType.VarChar).Value = fax;
                        cmd.Parameters.Add("@Email", SqlDbType.VarChar).Value = email;
                        cmd.Parameters.Add("@Website", SqlDbType.VarChar).Value = website;
                        cmd.Parameters.Add("@Address", SqlDbType.VarChar).Value = address;
                        cmd.Parameters.Add("@CreditDays", SqlDbType.Int).Value = creditDays;
                        cmd.Parameters.Add("@CreditLimit", SqlDbType.Decimal).Value = creditLimit;
                        cmd.Parameters.Add("@TaxType", SqlDbType.VarChar).Value = taxType;
                        cmd.Parameters.Add("@GST", SqlDbType.VarChar).Value = gST;
                        cmd.Parameters.Add("@NTN", SqlDbType.VarChar).Value = nTN;
                        cmd.Parameters.Add("@CNIC", SqlDbType.VarChar).Value = cNIC;
                        cmd.Parameters.Add("@ShortOrLongDetail", SqlDbType.VarChar).Value = shortOrLongDetail;
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar).Value = remarks;
                        cmd.Parameters.Add("@AdminUser", SqlDbType.VarChar).Value = adminuser;
                        cmd.Parameters.Add("@AdminPassword", SqlDbType.VarChar).Value = adminpassword;
                        cmd.Parameters.Add("@SalesmanID", SqlDbType.Int).Value = salesmanID;

                        cmd.Parameters.Add("@IssueDate", SqlDbType.Date).Value =IssueDate ;
                        cmd.Parameters.Add("@ExpiryDate", SqlDbType.Date).Value =ExpiryDate ;
                        cmd.Parameters.Add("@FirstName", SqlDbType.VarChar).Value =FirstName ;
                        cmd.Parameters.Add("@LastName", SqlDbType.VarChar).Value = LastName;
                        cmd.Parameters.Add("@Position", SqlDbType.VarChar).Value = Position;
                        cmd.Parameters.Add("@CommEmail", SqlDbType.VarChar).Value = CommEmail;
                        cmd.Parameters.Add("@BookingEmail", SqlDbType.VarChar).Value = BookingEmail;
                        cmd.Parameters.Add("@AccountName", SqlDbType.VarChar).Value = AccountName;
                        cmd.Parameters.Add("@ACNumber", SqlDbType.VarChar).Value = ACNumber;
                        cmd.Parameters.Add("@SortCode", SqlDbType.VarChar).Value = SortCode;
                        cmd.Parameters.Add("@BankName", SqlDbType.VarChar).Value = BankName;
                        cmd.Parameters.Add("@BankAddress", SqlDbType.VarChar).Value = BankAddress;
                        cmd.Parameters.Add("@UrgentPick", SqlDbType.VarChar).Value = UrgentPick;
                        cmd.Parameters.Add("@UrgentPickTime", SqlDbType.Int).Value = UrgentPickTime;

                        cmd.Parameters.Add("@WorkFrom", SqlDbType.Time).Value = WorkFrom;
                        cmd.Parameters.Add("@WorkUpTo", SqlDbType.Time).Value = WorkUpTo;

                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        cmd.Parameters.Add("@dtInvoiceZone", SqlDbType.Structured).Value = dt;
                        cmd.Parameters.Add("@dtInvoiceVeh", SqlDbType.Structured).Value = vehdt;
                        cmd.Parameters.Add("@dtDocument", SqlDbType.Structured).Value = docdt;
                        
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.VarChar).Value = insertUpdateStatus;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();
                        returnId = cmd.Parameters["@CheckReturn"].Value.ToString();
                        cmd.Dispose();
                    }
                    con.Close();
                    con.Dispose();
                }
                catch (Exception ex)
                { }
            }
            return returnId;
        }



        public ActionResult GetAllCabType()
        {
            if (User != null)
            {
                List<clsCabType> cabtype = (from f in db.Cab_CabType
                                            where f.Status.ToLower() == "active"
                                            select new clsCabType
                                            {
                                                CabTypeID = f.CabTypeID,
                                                Description = f.Description,
                                                SeatCapacity = f.SeatCapacity ?? 0,
                                                Remarks = f.Remarks,
                                                Status = f.Status
                                            }).ToList();


                cabtype = cabtype.OrderBy(x=>x.Description).ToList();
                var data = cabtype.ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public ActionResult GetAllDocType()
        {
            if (User != null)
            {
                List<clsCabType> cabtype = (from f in db.AC_Setup_Documents
                                            where f.Status.ToLower() == "active"
                                            select new clsCabType
                                            {
                                                CabTypeID = f.DocumentTypeID,
                                                Description = f.Description,
                                                Status = f.NeedExpiry ?? "N",
                                            }).ToList();


                cabtype = cabtype.OrderBy(x => x.Description).ToList();
                var data = cabtype.ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult SupplierDetailsList()
        {
            return PartialView();
        }
        public ActionResult GetAllSupplierDetails()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var detailCode = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var description = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var reference = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var urduName = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                var type = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
                var city = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsAccountDetail> detail = (from f in db.AC_Detail
                                                 join fr in db.AC_Detail_Extra on f.DetailCode equals fr.DetailCode
                                                 where f.Type == "Supplier"
                                                 orderby f.Name
                                                 select new clsAccountDetail
                                                 {
                                                     DetailCode = f.DetailCode,
                                                     Name = f.Name,
                                                     CityId = fr.CityId,
                                                     CityName = db.Cities.Where(x => x.CityId == fr.CityId).Select(x => x.CityName).FirstOrDefault() ?? string.Empty,
                                                     Status = f.Status,
                                                     AcDescription = f.AcDescription,
                                                     AccountCodeV = f.AccountCodeV,
                                                     AccountCode = f.AccountCode,
                                                     Reference = db.AC_ChartOfAC.Where(x => x.AccountCode == f.AccountCode).Select(x => x.Description).FirstOrDefault(),
                                                     Type = f.Type,
                                                     UrduName = f.UrduName,
                                                     Phone1 = fr.Phone1,
                                                     Phone2 = fr.Phone2,
                                                     Cell1 = fr.Cell1,
                                                     Cell2 = fr.Cell2,
                                                     Fax = fr.Fax,
                                                     Email = fr.Email,
                                                     Website = fr.Website,
                                                     Address = fr.Address,
                                                     CreditDays = fr.CreditDays,
                                                     CreditLimit = fr.CreditLimit,
                                                     TaxType = fr.TaxType,
                                                     GST = fr.GST,
                                                     NTN = fr.NTN,
                                                     CNIC = fr.CNIC,
                                                     Remarks = fr.Remarks,
                                                     SalesmanID = fr.SalesmanID
                                                 }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(detailCode))
                {
                    detail = detail.Where(c => c.DetailCode.ToString().Contains(detailCode)).ToList();
                }
                else if (!string.IsNullOrEmpty(detailName))
                {
                    detail = detail.Where(c => c.Name.ToLower().Contains(detailName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    detail = detail.Where(c => c.AcDescription.ToLower().Contains(description.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    detail = detail.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(reference))
                {
                    detail = detail.Where(c => c.Reference.ToLower().Contains(reference.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(urduName))
                {
                    detail = detail.Where(c => c.UrduName.Contains(urduName)).ToList();
                }
                else if (!string.IsNullOrEmpty(type))
                {
                    detail = detail.Where(c => c.Type.ToLower().Contains(type.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(city))
                {
                    detail = detail.Where(c => c.CityName.ToLower().Contains(city.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        detail = detail.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = detail.Count();
                var data = detail.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public ActionResult GetACZoneDetailById(int id)
        {
            List<clsAccountDetail> fareDetail = new List<clsAccountDetail>();
            fareDetail = (from f in db.AC_Detail_Zone 
                          where f.DetailCode == id
                          select new clsAccountDetail
                          {
                              SerialNo = f.SerialNo,
                              ZoneID = f.ZoneID ?? 0,
                              ZoneName= db.Cab_Zone.Where(x=>x.ZoneID == f.ZoneID).Select(x=>x.Description).FirstOrDefault() ?? "",
                          }).OrderBy(x => x.SerialNo).ToList();
            var data = fareDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetACVehicleDetailById(int id)
        {
            List<clsAccountDetail> fareDetail = new List<clsAccountDetail>();
            List<clsAccountDetail> cabtype = (from f in db.Cab_CabType
                                        where f.Status.ToLower() == "active"
                                        select new clsAccountDetail
                                        {
                                            CabTypeID = f.CabTypeID,
                                            CabTypeName = f.Description,
                                            NoOfVeh = db.AC_Detail_NoOfVeh.Where(x => x.CabTypeID == f.CabTypeID && x.DetailCode == id).Select(x => x.NoOfVehicle).FirstOrDefault() ?? 0,
                                        }).ToList();

            cabtype = cabtype.OrderBy(x => x.CabTypeName).ToList();
            return Json(new { data = cabtype }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetACVehicleDetailDriverById(int id)
        {
            List<clsAccountDetail> fareDetail = new List<clsAccountDetail>();
            List<clsAccountDetail> cabtype = (from f in db.AC_Detail_NoOfVeh
                                              where f.DetailCode ==  id
                                              select new clsAccountDetail
                                              {
                                                  CabTypeID = f.CabTypeID,
                                                  CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabTypeID).Select(x => x.Description).FirstOrDefault() ?? "",
                                                  NoOfVeh = 1,
                                                  RegNo = f.RegNo ?? "",
                                                  Make = f.Make ?? "",
                                                  Model = f.Model ?? "",
                                                  Colour = f.Colour ?? "",
                                                  FuelType=f.FuelType ?? "",
                                                  SerialNo= f.SerialNo,
                                              }).OrderBy(x => x.SerialNo).ToList();
            return Json(new { data = cabtype }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetACDocDetailById(int id)
        {
            List<clsAccountDetail> fareDetail = new List<clsAccountDetail>();
            List<clsAccountDetail> cabtype = (from f in db.AC_Setup_Documents
                                              where f.Status.ToLower() == "active"
                                              select new clsAccountDetail
                                              {
                                                  CabTypeID = f.DocumentTypeID,
                                                  CabTypeName = f.Description,
                                                  DocumentNo = db.AC_Detail_Document.Where(x => x.DocumentTypeID == f.DocumentTypeID && x.DetailCode == id).Select(x => x.DocumentNo).FirstOrDefault() ?? "",
                                                  ExpiryDate = db.AC_Detail_Document.Where(x => x.DocumentTypeID == f.DocumentTypeID && x.DetailCode == id).Select(x => x.ExpiryDate).FirstOrDefault() ?? DateTime.Now,
                                                  DocStatus = db.AC_Detail_Document.Where(x => x.DocumentTypeID == f.DocumentTypeID && x.DetailCode == id).Select(x => x.Status).FirstOrDefault() ?? 0,
                                                  Status = f.NeedExpiry ?? "N",
                                              }).ToList();

            cabtype = cabtype.OrderBy(x => x.CabTypeName).ToList();
            return Json(new { data = cabtype }, JsonRequestBehavior.AllowGet);
        }
        
        #endregion
        #region Driver Detail
        public ActionResult DriverDetail()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CustomerSupplierDetail"
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
                    //ViewBag.ThirdLevel = new SelectList(getThirdLevels(), "AccountCode", "Description");
                    ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
                    ViewBag.CabType = new SelectList(getCabType(), "CabTypeID", "Description");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    ViewBag.Zones = new SelectList(getZone(), "ZoneID", "Description");

                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        [HttpPost]
        public ActionResult InsertUpdateDriverDetail(clsAccountDetail detail, string data, string vehdata, string docdata)
        {
            string message = "";
            bool status = false;
            if (detail.CityId == null)
            {
                detail.CityId = 0;
            }

            int detailid = 0;

            List<clsAccountDetail> invoice = new List<clsAccountDetail>();
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ZoneID");

            if (data == null || data == "")
            {
                dt.Rows.Add(new object[] { 0, 0 });
            }
            else
            {
                invoice = JsonConvert.DeserializeObject<List<clsAccountDetail>>(data);
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ZoneID });
                }
            }

            List<clsAccountDetail> vehlist = new List<clsAccountDetail>();
            DataTable vehdt = new DataTable();
            vehdt.Columns.Add("Id");
            vehdt.Columns.Add("CabTypeID");
            vehdt.Columns.Add("NoOfVeh");
            vehdt.Columns.Add("RegNo");
            vehdt.Columns.Add("Make");
            vehdt.Columns.Add("Model");
            vehdt.Columns.Add("Colour");
            vehdt.Columns.Add("FuelType");
            if (vehdata == null || vehdata == "")
            {
                vehdt.Rows.Add(new object[] { 0, 0, 0, "", "", "", "", "" });
            }
            else
            {
                vehlist = JsonConvert.DeserializeObject<List<clsAccountDetail>>(vehdata);
                for (int i = 0; i < vehlist.Count; i++)
                {
                    vehdt.Rows.Add(new object[] { i + 1, vehlist[i].CabTypeID, vehlist[i].NoOfVeh 
                    , vehlist[i].RegNo, vehlist[i].Make, vehlist[i].Model, vehlist[i].Colour, vehlist[i].FuelType});
                }
            }


            List<clsAccountDetail> doclist = new List<clsAccountDetail>();
            DataTable docdt = new DataTable();
            docdt.Columns.Add("Id");
            docdt.Columns.Add("DocumentTypeID");
            docdt.Columns.Add("DocumentNo");
            docdt.Columns.Add("ExpiryDate");
            docdt.Columns.Add("Status");

            if (docdata == null || docdata == "")
            {
                docdt.Rows.Add(new object[] { 0, 0,"", null, 0 });
            }
            else
            {
                doclist = JsonConvert.DeserializeObject<List<clsAccountDetail>>(docdata);
                for (int i = 0; i < doclist.Count; i++)
                {
                    docdt.Rows.Add(new object[] { i + 1, doclist[i].DocumentTypeID, doclist[i].DocumentNo, doclist[i].ExpiryDate, doclist[i].DocStatus });
                }
            }

            if (detail.Fax == null)
            {
                detail.Fax = "";
            }
            else if (detail.Fax != "")
            {
                detail.Fax = clsPasswordEncrypt.GetHash(detail.Fax);
            }


            detail.AccountCode = (from c in db.AC_DefaultGL
                                  join ac in db.AC_ChartOfAC on c.AccountCode equals ac.AccountCode
                                  where c.Type == "Driver"
                                  select ac.AccountCode).FirstOrDefault();

            string returnId = "";
            if (detail.IsNeWOrEdit == "Edit")
            {
                

                returnId = AddEditSupplier(detail.DetailCode, detail.Name, detail.CityId, detail.Status, detail.AcDescription, "", detail.AccountCode, detail.Type, detail.UrduName, detail.Phone1, detail.Phone2, detail.Cell1, detail.Cell2, detail.Fax, detail.Email, detail.Website, detail.Address, "", detail.CreditDays, detail.CreditLimit, detail.TaxType, detail.GST, detail.NTN, detail.CNIC, "Long", detail.Remarks, detail.SalesmanID, User.UserId, "Update", dt,"","",
                            detail.IssueDate, detail.ExpiryDate, detail.FirstName, detail.LastName,
                            detail.Position, detail.CommEmail, detail.BookingEmail, detail.AccountName,
                            detail.ACNumber, detail.SortCode, detail.BankName, vehdt, detail.UrgentPick, detail.UrgentPickTime,
                            detail.WorkFrom, detail.WorkUpTo, detail.BankAddress, docdt
                            , detail.Monday, detail.Tuesday, detail.Wednesday, detail.Thursday, detail.Friday, detail.Saturday, detail.Sunday);
                if (returnId == "1")
                {
                    detailid = Convert.ToInt32(detail.DetailCode);
                    ModelState.Clear();
                    status = true;
                    message = "Drivers Successfully Updated";
                }
                else if (returnId == "-9999")
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {

                returnId = AddEditSupplier(detail.DetailCode, detail.Name, detail.CityId, "Active", detail.AcDescription, "", detail.AccountCode, detail.Type, detail.UrduName, detail.Phone1, detail.Phone2, detail.Cell1, detail.Cell2, detail.Fax, detail.Email, detail.Website, detail.Address, "", detail.CreditDays, detail.CreditLimit, detail.TaxType, detail.GST, detail.NTN, detail.CNIC, "Long", detail.Remarks, detail.SalesmanID, User.UserId, "Save", dt, "", "",
                            detail.IssueDate, detail.ExpiryDate, detail.FirstName, detail.LastName,
                            detail.Position, detail.CommEmail, detail.BookingEmail, detail.AccountName,
                            detail.ACNumber, detail.SortCode, detail.BankName, vehdt, detail.UrgentPick, detail.UrgentPickTime,
                            detail.WorkFrom, detail.WorkUpTo, detail.BankAddress, docdt
                            , detail.Monday, detail.Tuesday, detail.Wednesday, detail.Thursday, detail.Friday, detail.Saturday, detail.Sunday);
                if (Convert.ToDecimal(returnId.ToString()) > 0)
                {
                    detailid = Convert.ToInt32(returnId.ToString());
                    ModelState.Clear();
                    status = true;
                    message = "Drivers Successfully Saved";
                }
                else if (Convert.ToInt32(returnId.ToString()) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }


            return new JsonResult { Data = new { status = status, message = message, detailid = detailid } };
        }


        public ActionResult DriverDetailsList()
        {
            return PartialView();
        }
        public ActionResult GetAllDriverDetails()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var detailCode = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var description = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var reference = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var urduName = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                var type = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
                var city = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsAccountDetail> detail = (from f in db.AC_Detail
                                                 join fr in db.AC_Detail_Extra on f.DetailCode equals fr.DetailCode
                                                 where f.Type == "Driver"
                                                 orderby f.Name
                                                 select new clsAccountDetail
                                                 {
                                                     DetailCode = f.DetailCode,
                                                     Name = f.Name,
                                                     CityId = fr.CityId,
                                                     CityName = db.Cities.Where(x => x.CityId == fr.CityId).Select(x => x.CityName).FirstOrDefault() ?? string.Empty,
                                                     Status = f.Status,
                                                     AcDescription = f.AcDescription,
                                                     AccountCodeV = f.AccountCodeV,
                                                     AccountCode = f.AccountCode,
                                                     Reference = db.AC_ChartOfAC.Where(x => x.AccountCode == f.AccountCode).Select(x => x.Description).FirstOrDefault(),
                                                     Type = f.Type,
                                                     UrduName = f.UrduName,
                                                     Phone1 = fr.Phone1,
                                                     Phone2 = fr.Phone2,
                                                     Cell1 = fr.Cell1,
                                                     Cell2 = fr.Cell2,
                                                     Fax = fr.Fax,
                                                     Email = fr.Email,
                                                     Website = fr.Website,
                                                     Address = fr.Address,
                                                     CreditDays = fr.CreditDays,
                                                     CreditLimit = fr.CreditLimit,
                                                     TaxType = fr.TaxType,
                                                     GST = fr.GST,
                                                     NTN = fr.NTN,
                                                     CNIC = fr.CNIC,
                                                     Remarks = fr.Remarks,
                                                     SalesmanID = fr.SalesmanID
                                                 }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(detailCode))
                {
                    detail = detail.Where(c => c.DetailCode.ToString().Contains(detailCode)).ToList();
                }
                else if (!string.IsNullOrEmpty(detailName))
                {
                    detail = detail.Where(c => c.Name.ToLower().Contains(detailName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    detail = detail.Where(c => c.AcDescription.ToLower().Contains(description.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    detail = detail.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(reference))
                {
                    detail = detail.Where(c => c.Reference.ToLower().Contains(reference.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(urduName))
                {
                    detail = detail.Where(c => c.UrduName.Contains(urduName)).ToList();
                }
                else if (!string.IsNullOrEmpty(type))
                {
                    detail = detail.Where(c => c.Type.ToLower().Contains(type.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(city))
                {
                    detail = detail.Where(c => c.CityName.ToLower().Contains(city.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        detail = detail.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = detail.Count();
                var data = detail.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        #endregion

        #region Reports
        #region Chart of Account
        public ActionResult ChartOfAccountReport()
        {
            List<clsChartOfAccount> chart = new List<clsChartOfAccount>();
            chart = (from f in db.AC_ChartOfAC
                     select new clsChartOfAccount
                     {
                         AccountCode = f.AccountCode,
                         AccountCodeV = f.AccountCodeV,
                         RefKey = f.RefKey,
                         Description = f.Description,
                         AcLevel = f.AcLevel,
                         Status = f.Status,
                     }).ToList();
            chart = chart.OrderBy(x => x.AccountCode).ToList();
            return View(chart);
        }
        #endregion
        #region Chart of Account With Detail
        public ActionResult ChartOfAccountWithDetailReport()
        {
            List<clsChartOfAccount> mainList = new List<clsChartOfAccount>();
            List<clsChartOfAccount> chart = new List<clsChartOfAccount>();
            List<clsChartOfAccount> detail = new List<clsChartOfAccount>();
            chart = (from f in db.AC_ChartOfAC
                     select new clsChartOfAccount
                     {
                         AccountCode = f.AccountCode,
                         AccountCodeV = f.AccountCodeV,
                         RefKey = f.RefKey,
                         Description = f.Description,
                         AcLevel = f.AcLevel,
                         Status = f.Status,
                     }).ToList();
            mainList.AddRange(chart);

            detail = (from f in db.AC_Detail
                      join a in db.AC_ChartOfAC on f.AccountCode equals a.AccountCode
                      where f.Status == "Active"
                     select new clsChartOfAccount
                     {
                         AccountCode = a.AccountCode,
                         AccountCodeV = f.AccountCodeV,
                         AcLevel = 4,
                         Description = f.Name,
                     }).ToList();
            mainList.AddRange(detail);
            mainList = mainList.OrderBy(x => x.AccountCode).ToList();
            return View(mainList);
        }
        #endregion
        #endregion
    }
}