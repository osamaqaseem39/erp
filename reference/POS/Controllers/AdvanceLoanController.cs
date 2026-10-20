using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;

namespace POS.Controllers
{
    public class AdvanceLoanController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Advance Loan
        public ActionResult AdvanceLoanIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "AdvanceLoan"
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
                    string fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    string toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    ViewBag.FromDate = fromDate;
                    ViewBag.ToDate = toDate;
                    ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                    ViewBag.Employee = new SelectList(getEmployees(), "EmployeeId", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateAdvanceLoan(clsAdvanceLoan advance)
        {
            string message = "";
            bool status = false;
            string invoiceNo = advance.ComputerNo;
            if (advance.IsNewOrEdit == "Edit")
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(string));
                db.spInsertUpdateAdvanceLoan(advance.ComputerNo, advance.EmployeeId,advance.Type,advance.RecordDate,advance.Amount,advance.Remarks,"Unpost",advance.ApprovedBy,advance.PaidFromDetailCode, User.UserId, "Update", CheckReturn);
                string returnId = CheckReturn.Value.ToString();
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Advance Loan Successfully Updated";
                }
                else 
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(string));
                db.spInsertUpdateAdvanceLoan(advance.ComputerNo, advance.EmployeeId, advance.Type, advance.RecordDate, advance.Amount, advance.Remarks, "Unpost", advance.ApprovedBy, advance.PaidFromDetailCode, User.UserId, "Save", CheckReturn);
                string returnId = CheckReturn.Value.ToString();
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Advance Loan Successfully Saved";
                }
                else 
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, invoiceNo = invoiceNo } };
        }
        public ActionResult GetAdvanceLoanById(string id)
        {
            clsAdvanceLoan loan = new clsAdvanceLoan();
            loan = (from f in db.HRM_AdvanceLoan
                       where f.ComputerNo == id
                    select new clsAdvanceLoan
                       {
                          ComputerNo = id,
                          EmployeeId = f.EmployeeId,
                          Type = f.Type,
                          RecordDate = f.RecordDate,
                          Amount = f.Amount ?? 0,
                          Remarks = f.Remarks ?? "",
                          Status = f.Status,
                          ApprovedBy = f.ApprovedBy ?? "",
                          PaidFromDetailCode = f.PaidFromDetailCode ?? 0,
                       }).FirstOrDefault();
            var data = loan;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AdvanceLoansList()
        {
            return PartialView();
        }
        public ActionResult GetAllAdvanceLoans()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var computerNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var employeeName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsAdvanceLoan> loan = (from f in db.HRM_AdvanceLoan
                                             where f.Type == "Advance" || f.Type == "Loan"
                                            select new clsAdvanceLoan
                                            {
                                                ComputerNo = f.ComputerNo,
                                                EmployeeId = f.EmployeeId,
                                                EmployeeName = db.HRM_Employee_Main.Where(x=>x.EmployeeId == f.EmployeeId).Select(x=>x.FullName).FirstOrDefault() ?? "",
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                Amount = f.Amount ?? 0,
                                                Remarks = f.Remarks ?? "",
                                                Status = f.Status,
                                                ApprovedBy = f.ApprovedBy ?? "",
                                                PaidFromDetailCode = f.PaidFromDetailCode ?? 0,
                                                PaidFromDetailName = db.AC_Detail.Where(x => x.DetailCode == f.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                            }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(computerNo))
                {
                    loan = loan.Where(c => c.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(employeeName))
                {
                    loan = loan.Where(c => c.EmployeeName.ToLower().Contains(employeeName.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        loan = loan.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = loan.Count();
                var data = loan.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteAdvanceLoan(string id)
        {
            string message = "";
            bool status = false;
            clsAdvanceLoan advance = new clsAdvanceLoan();
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(string));
            db.spInsertUpdateAdvanceLoan(id, 0, "", DateTime.Now, 0, "", "Unpost", "",0, User.UserId, "Delete", CheckReturn);
            string returnId = CheckReturn.Value.ToString();
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Advance Loan Successfully Deleted";
            }
            else if (returnId == "-9999")
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this advance loan first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public List<clsAccountDetail> getDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      join ac in db.AC_DefaultGL on c.AccountCode equals ac.AccountCode
                      where c.Status == "Active" && (ac.Type == "Cash" || ac.Type == "Bank")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name
                      }).ToList();
            return detail;
        }
        public List<clsEmployee> getEmployees()
        {
            List<clsEmployee> employee = new List<clsEmployee>();
            employee = (from c in db.HRM_Employee_Main
                        select new clsEmployee
                      {
                          EmployeeId = c.EmployeeId,
                          FullName = c.FullName
                      }).ToList();
            return employee;
        }
        public ActionResult getMaxCodeForAdvanceLoan()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "ALN-" + currentYear + currentMonth;
            int count = db.HRM_AdvanceLoan.Where(x => x.Type == "Advance" || x.Type == "Loan").Count();
            if (count > 0)
            {
                maxCode = db.HRM_AdvanceLoan.Where(x => x.Type == "Advance" || x.Type == "Loan").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
                maxCode = maxCode.Split('-')[1];
                maxCode = maxCode.Substring(4, 4);
                int codeInIntegar = Convert.ToInt32(maxCode);
                codeInIntegar += 1;
                if (codeInIntegar.ToString().Length == 1)
                {
                    maxCode = invoiceCode + "000" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 2)
                {
                    maxCode = invoiceCode + "00" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 3)
                {
                    maxCode = invoiceCode + "0" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 4)
                {
                    maxCode = invoiceCode + codeInIntegar.ToString();
                }
            }
            else
            {
                maxCode = invoiceCode + "0001";
            }

            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Loan Return
        public ActionResult LoanReturnIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "LoanReturn"
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
                    string fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    string toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    ViewBag.FromDate = fromDate;
                    ViewBag.ToDate = toDate;
                    ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                    ViewBag.Employee = new SelectList(getEmployees(), "EmployeeId", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateLoanReturn(clsAdvanceLoan advance)
        {
            string message = "";
            bool status = false;
            string invoiceNo = advance.ComputerNo;
            if (advance.IsNewOrEdit == "Edit")
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(string));
                db.spInsertUpdateAdvanceLoan(advance.ComputerNo, advance.EmployeeId, advance.Type, advance.RecordDate, advance.Amount, advance.Remarks, "Unpost", advance.ApprovedBy, advance.PaidFromDetailCode, User.UserId, "Update", CheckReturn);
                string returnId = CheckReturn.Value.ToString();
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Loan Return Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(string));
                db.spInsertUpdateAdvanceLoan(advance.ComputerNo, advance.EmployeeId, advance.Type, advance.RecordDate, advance.Amount, advance.Remarks, "Unpost", advance.ApprovedBy, advance.PaidFromDetailCode, User.UserId, "Save", CheckReturn);
                string returnId = CheckReturn.Value.ToString();
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Loan Return Successfully Saved";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, invoiceNo = invoiceNo } };
        }
        public ActionResult GetLoanReturnById(string id)
        {
            clsAdvanceLoan loan = new clsAdvanceLoan();
            loan = (from f in db.HRM_AdvanceLoan
                    where f.ComputerNo == id
                    select new clsAdvanceLoan
                    {
                        ComputerNo = id,
                        EmployeeId = f.EmployeeId,
                        Type = f.Type,
                        RecordDate = f.RecordDate,
                        Amount = f.Amount ?? 0,
                        Remarks = f.Remarks ?? "",
                        Status = f.Status,
                        ApprovedBy = f.ApprovedBy ?? "",
                        PaidFromDetailCode = f.PaidFromDetailCode ?? 0,
                    }).FirstOrDefault();
            var data = loan;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoanReturnsList()
        {
            return PartialView();
        }
        public ActionResult GetAllLoanReturns()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var computerNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var employeeName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsAdvanceLoan> loan = (from f in db.HRM_AdvanceLoan
                                             where f.Type == "Loan Return"
                                             select new clsAdvanceLoan
                                             {
                                                 ComputerNo = f.ComputerNo,
                                                 EmployeeId = f.EmployeeId,
                                                 EmployeeName = db.HRM_Employee_Main.Where(x => x.EmployeeId == f.EmployeeId).Select(x => x.FullName).FirstOrDefault() ?? "",
                                                 Type = f.Type,
                                                 RecordDate = f.RecordDate,
                                                 Amount = f.Amount ?? 0,
                                                 Remarks = f.Remarks ?? "",
                                                 Status = f.Status,
                                                 ApprovedBy = f.ApprovedBy ?? "",
                                                 PaidFromDetailCode = f.PaidFromDetailCode ?? 0,
                                                 PaidFromDetailName = db.AC_Detail.Where(x => x.DetailCode == f.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                             }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(computerNo))
                {
                    loan = loan.Where(c => c.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(employeeName))
                {
                    loan = loan.Where(c => c.EmployeeName.ToLower().Contains(employeeName.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        loan = loan.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = loan.Count();
                var data = loan.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteLoanReturn(string id)
        {
            string message = "";
            bool status = false;
            clsAdvanceLoan advance = new clsAdvanceLoan();
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(string));
            db.spInsertUpdateAdvanceLoan(id, 0, "", DateTime.Now, 0, "", "Unpost", "", 0, User.UserId, "Delete", CheckReturn);
            string returnId = CheckReturn.Value.ToString();
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Loan Return Successfully Deleted";
            }
            else if (returnId == "-9999")
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this loan return first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult getMaxCodeForLoanReturn()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "LRN-" + currentYear + currentMonth;
            int count = db.HRM_AdvanceLoan.Where(x => x.Type == "Loan Return").Count();
            if (count > 0)
            {
                maxCode = db.HRM_AdvanceLoan.Where(x => x.Type == "Loan Return").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
                maxCode = maxCode.Split('-')[1];
                maxCode = maxCode.Substring(4, 4);
                int codeInIntegar = Convert.ToInt32(maxCode);
                codeInIntegar += 1;
                if (codeInIntegar.ToString().Length == 1)
                {
                    maxCode = invoiceCode + "000" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 2)
                {
                    maxCode = invoiceCode + "00" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 3)
                {
                    maxCode = invoiceCode + "0" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 4)
                {
                    maxCode = invoiceCode + codeInIntegar.ToString();
                }
            }
            else
            {
                maxCode = invoiceCode + "0001";
            }

            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}