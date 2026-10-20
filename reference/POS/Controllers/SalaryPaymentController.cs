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

namespace POS.Controllers
{
    public class SalaryPaymentController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Salary Payment
        public ActionResult SalaryPaymentIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SalaryPayment"
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
                    ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetAllPendingSalaries()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var employeName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var monthYear = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsSalaryPayment> payment = new List<clsSalaryPayment>();
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_EmployeesForSalaryPayment", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FiscalYearId", SqlDbType.Int).Value = User.FiscalYearId;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                DataTableReader dt = tdt.CreateDataReader();
                con.Close();
                SqlConnection.ClearPool(con);
                while (dt.Read())
                {
                    payment.Add(new clsSalaryPayment()
                    {
                        SalaryId = Convert.ToInt32(dt["SalaryId"]),
                        MonthYear = dt["MonthYear"].ToString(),
                        ForMonth = Convert.ToDateTime(dt["ForMonth"]),
                        EmployeeId = Convert.ToInt32(dt["EmployeeId"]),
                        EmployeeName = dt["EmployeeName"].ToString(),
                        TotalSalary = Convert.ToDecimal(dt["TotalSalary"]),
                        Paid = Convert.ToDecimal(dt["Paid"]),
                        Balance = Convert.ToDecimal(dt["Balance"]),
                    });
                }
                //SEARCHING...
                if (!string.IsNullOrEmpty(employeName))
                {
                    payment = payment.Where(c => c.EmployeeName.ToLower().Contains(employeName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(monthYear))
                {
                    payment = payment.Where(c => c.MonthYear.ToLower().Contains(monthYear.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        payment = payment.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = payment.Count();
                var data = payment.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateSalaryPayment(clsSalaryPayment formData)
        {
            string message = "";
            bool status = false;
            string voucherNo = formData.PaymentNo;
            string returnId = "";
            if (formData.NewOrEdit == "Edit")
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(string));
                db.spInsertUpdateSalaryPayment(formData.PaymentNo, formData.RecordDate, formData.EmployeeId, formData.Remarks, formData.PaidFromDetailCode, formData.Amount, formData.SalaryId, "Unpost",formData.Balance,"", User.UserId, "Update", CheckReturn);
                returnId = CheckReturn.Value.ToString();
                if (returnId == voucherNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Salary Paid Successfully";
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
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(string));
                db.spInsertUpdateSalaryPayment(formData.PaymentNo, formData.RecordDate, formData.EmployeeId, formData.Remarks, formData.PaidFromDetailCode, formData.Amount, formData.SalaryId, "Unpost", formData.Balance, "", User.UserId, "Save", CheckReturn);
                returnId = CheckReturn.Value.ToString();
                voucherNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Salary Paid Successfully";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, voucherNo = voucherNo } };
        }
        public ActionResult GetSalaryPaymentById(string id)
        {
            clsSalaryPayment payment = new clsSalaryPayment();
            payment = (from f in db.HRM_SalaryPayment
                       where f.PaymentNo == id
                       select new clsSalaryPayment
                       {
                           PaymentNo = f.PaymentNo,
                           RecordDate = f.RecordDate,
                           EmployeeId = f.EmployeeId ?? 0,
                           EmployeeName = db.HRM_Employee_Main.Where(x=>x.EmployeeId == f.EmployeeId).Select(x=>x.FullName).FirstOrDefault() ?? "",
                           Remarks = f.Remarks ?? "",
                           PaidFromDetailCode = f.PaidFromDetailCode ?? 0,
                           Amount = f.Amount ?? 0,
                           SalaryId =  f.SalaryId ?? 0,
                           Status = f.Status,
                           Balance = f.Balance
                       }).FirstOrDefault();
            var data = payment;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SalaryPaymentsList()
        {
            return PartialView();
        }
        public ActionResult GetAllSalaryPayments()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var employeeName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var paymentNo = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsSalaryPayment> country = (from f in db.HRM_SalaryPayment
                                                  join s in db.HRM_Salary on f.SalaryId equals s.SalaryId
                                                  select new clsSalaryPayment
                                            {
                                                PaymentNo = f.PaymentNo,
                                                RecordDate = f.RecordDate,
                                                EmployeeId = f.EmployeeId ?? 0,
                                                ForMonth = s.ForMonth,
                                                EmployeeName = db.HRM_Employee_Main.Where(x => x.EmployeeId == f.EmployeeId).Select(x => x.FullName).FirstOrDefault() ?? "",
                                                Remarks = f.Remarks ?? "",
                                                PaidFromDetailCode = f.PaidFromDetailCode ?? 0,
                                                DetailName = db.AC_Detail.Where(x => x.DetailCode == f.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                                Amount = f.Amount ?? 0,
                                                SalaryId = f.SalaryId ?? 0,
                                                Status = f.Status,
                                                Balance = f.Balance
                                            }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(employeeName))
                {
                    country = country.Where(c => c.EmployeeName.ToLower().Contains(employeeName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(employeeName))
                {
                    country = country.Where(c => c.PaymentNo.ToLower().Contains(paymentNo.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        country = country.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = country.Count();
                var data = country.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteSalaryPayment(string id)
        {
            string message = "";
            bool status = false;
            clsSalaryPayment payment = new clsSalaryPayment();
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(string));
            db.spInsertUpdateSalaryPayment(id, DateTime.Now, 0, "", 0, 0, 0, "Unpost",0, "", User.UserId, "Delete", CheckReturn);
            string returnId = CheckReturn.Value.ToString();
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Salary Payment Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this payment first.";
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
        public ActionResult getMaxCodeForSalaryPayment()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "SP-" + currentYear + currentMonth;
            int count = db.HRM_SalaryPayment.Count();
            if (count > 0)
            {
                maxCode = db.HRM_SalaryPayment.OrderByDescending(x => x.PaymentNo).Select(x => x.PaymentNo).FirstOrDefault();
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
        #region Salary Payment Report
        public ActionResult SalaryPaymentReport(string id)
        {
            if (User != null)
            {
                List<clsSalaryPayment> payment = new List<clsSalaryPayment>();
                payment = (from p in db.HRM_SalaryPayment
                           join s in db.HRM_Salary on p.SalaryId equals s.SalaryId
                           join e in db.HRM_Employee_Main on p.EmployeeId equals e.EmployeeId
                           where p.PaymentNo == id
                           select new clsSalaryPayment
                           {
                               PaymentNo = p.PaymentNo,
                               EmployeeName = e.FullName,
                               TotalSalary = s.NetSalary,
                               ForMonth = s.ForMonth,
                               RecordDate = p.RecordDate,
                               DetailName = db.AC_Detail.Where(x=>x.DetailCode == p.PaidFromDetailCode).Select(x=>x.Name).FirstOrDefault() ?? "",
                               Amount = p.Amount,
                               Remarks = p.Remarks,
                               Balance = p.Balance,
                               ReportType = "Single",
                           }).ToList();
                foreach (clsSalaryPayment p in payment)
                {
                    p.MonthYear = Convert.ToDateTime(p.ForMonth).ToString("MMMM") + " , " + Convert.ToDateTime(p.ForMonth).ToString("yyyy");
                }
                return View(payment);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        #region Report
        public ActionResult SalaryPaymentMultiReport()
        {
            ViewBag.Employee = new SelectList(getEmployees(), "EmployeeId", "FullName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintSalaryPaymentReport()
        {
            if (User != null)
            {
                String[] emp = Request.Form.GetValues("EmployeeId");
                DateTime salaryMonth = Convert.ToDateTime(Request.Form["SalaryMonth"]);
                List<clsSalaryPayment> salaryList = new List<clsSalaryPayment>();
                DateTime? fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime? toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                var checkbox = Request.Form.GetValues("DateCheckbox");
                if (checkbox == null)
                {
                    fromDate = null;
                    toDate = null;
                }
                if (emp != null)
                {
                    List<clsSalaryPayment> list = GetSalaryPayments(1, emp, salaryMonth, fromDate, toDate);
                    salaryList.AddRange(list);
                }
                else
                {
                    List<clsSalaryPayment> list = GetSalaryPayments(0, emp, salaryMonth, fromDate, toDate);
                    salaryList.AddRange(list);
                }
                salaryList = salaryList.OrderBy(x => x.EmployeeName).ToList();
                return View(salaryList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsSalaryPayment> GetSalaryPayments(int employeeId, String[] emp, DateTime salaryMonth, DateTime? fromDate, DateTime? toDate)
        {
            List<clsSalaryPayment> list = new List<clsSalaryPayment>();
            List<clsSalaryPayment> salaryList = new List<clsSalaryPayment>();
            if (employeeId == 0)
            {
                if(salaryMonth != null && fromDate != null && toDate != null)
                {
                    list = (from p in db.HRM_SalaryPayment
                            join s in db.HRM_Salary on p.SalaryId equals s.SalaryId
                            join e in db.HRM_Employee_Main on p.EmployeeId equals e.EmployeeId
                            where s.ForMonth.Value.Month == salaryMonth.Month
                            && s.ForMonth.Value.Year == salaryMonth.Year
                            && p.RecordDate >= fromDate
                            && p.RecordDate <= toDate
                            select new clsSalaryPayment
                            {
                                PaymentNo = p.PaymentNo,
                                EmployeeName = e.FullName,
                                TotalSalary = s.NetSalary,
                                ForMonth = s.ForMonth,
                                RecordDate = p.RecordDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == p.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                Amount = p.Amount,
                                Remarks = p.Remarks,
                                Balance = p.Balance,
                                ReportType = "Single",
                            }).ToList();
                }
                else if (salaryMonth == null && fromDate != null && toDate != null)
                {
                    list = (from p in db.HRM_SalaryPayment
                            join s in db.HRM_Salary on p.SalaryId equals s.SalaryId
                            join e in db.HRM_Employee_Main on p.EmployeeId equals e.EmployeeId
                            where p.RecordDate >= fromDate
                            && p.RecordDate <= toDate
                            select new clsSalaryPayment
                            {
                                PaymentNo = p.PaymentNo,
                                EmployeeName = e.FullName,
                                TotalSalary = s.NetSalary,
                                ForMonth = s.ForMonth,
                                RecordDate = p.RecordDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == p.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                Amount = p.Amount,
                                Remarks = p.Remarks,
                                Balance = p.Balance,
                                ReportType = "Single",
                            }).ToList();
                }
                else if (salaryMonth != null && fromDate == null && toDate == null)
                {
                    list = (from p in db.HRM_SalaryPayment
                            join s in db.HRM_Salary on p.SalaryId equals s.SalaryId
                            join e in db.HRM_Employee_Main on p.EmployeeId equals e.EmployeeId
                            where s.ForMonth.Value.Month == salaryMonth.Month
                            && s.ForMonth.Value.Year == salaryMonth.Year
                            select new clsSalaryPayment
                            {
                                PaymentNo = p.PaymentNo,
                                EmployeeName = e.FullName,
                                TotalSalary = s.NetSalary,
                                ForMonth = s.ForMonth,
                                RecordDate = p.RecordDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == p.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                Amount = p.Amount,
                                Remarks = p.Remarks,
                                Balance = p.Balance,
                                ReportType = "Single",
                            }).ToList();
                }
                else
                {
                    list = (from p in db.HRM_SalaryPayment
                            join s in db.HRM_Salary on p.SalaryId equals s.SalaryId
                            join e in db.HRM_Employee_Main on p.EmployeeId equals e.EmployeeId
                            select new clsSalaryPayment
                            {
                                PaymentNo = p.PaymentNo,
                                EmployeeName = e.FullName,
                                TotalSalary = s.NetSalary,
                                ForMonth = s.ForMonth,
                                RecordDate = p.RecordDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == p.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                Amount = p.Amount,
                                Remarks = p.Remarks,
                                Balance = p.Balance,
                                ReportType = "Single",
                            }).ToList();
                }
                salaryList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < emp.Length; i++)
                {
                    int employeeId2 = Convert.ToInt32(emp[i]);
                    if (salaryMonth != null && fromDate != null && toDate != null)
                    {
                        list = (from p in db.HRM_SalaryPayment
                                join s in db.HRM_Salary on p.SalaryId equals s.SalaryId
                                join e in db.HRM_Employee_Main on p.EmployeeId equals e.EmployeeId
                                where p.EmployeeId == employeeId2
                                && s.ForMonth.Value.Month == salaryMonth.Month
                                && s.ForMonth.Value.Year == salaryMonth.Year
                                && p.RecordDate >= fromDate
                                && p.RecordDate <= toDate
                                select new clsSalaryPayment
                                {
                                    PaymentNo = p.PaymentNo,
                                    EmployeeName = e.FullName,
                                    TotalSalary = s.NetSalary,
                                    ForMonth = s.ForMonth,
                                    RecordDate = p.RecordDate,
                                    DetailName = db.AC_Detail.Where(x => x.DetailCode == p.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                    Amount = p.Amount,
                                    Remarks = p.Remarks,
                                    Balance = p.Balance,
                                    ReportType = "Single",
                                }).ToList();
                    }
                    else if (salaryMonth == null && fromDate != null && toDate != null)
                    {
                        list = (from p in db.HRM_SalaryPayment
                                join s in db.HRM_Salary on p.SalaryId equals s.SalaryId
                                join e in db.HRM_Employee_Main on p.EmployeeId equals e.EmployeeId
                                where p.EmployeeId == employeeId2
                                && p.RecordDate >= fromDate
                                && p.RecordDate <= toDate
                                select new clsSalaryPayment
                                {
                                    PaymentNo = p.PaymentNo,
                                    EmployeeName = e.FullName,
                                    TotalSalary = s.NetSalary,
                                    ForMonth = s.ForMonth,
                                    RecordDate = p.RecordDate,
                                    DetailName = db.AC_Detail.Where(x => x.DetailCode == p.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                    Amount = p.Amount,
                                    Remarks = p.Remarks,
                                    Balance = p.Balance,
                                    ReportType = "Single",
                                }).ToList();
                    }
                    else if (salaryMonth != null && fromDate == null && toDate == null)
                    {
                        list = (from p in db.HRM_SalaryPayment
                                join s in db.HRM_Salary on p.SalaryId equals s.SalaryId
                                join e in db.HRM_Employee_Main on p.EmployeeId equals e.EmployeeId
                                where p.EmployeeId == employeeId2
                                && s.ForMonth.Value.Month == salaryMonth.Month
                                && s.ForMonth.Value.Year == salaryMonth.Year
                                select new clsSalaryPayment
                                {
                                    PaymentNo = p.PaymentNo,
                                    EmployeeName = e.FullName,
                                    TotalSalary = s.NetSalary,
                                    ForMonth = s.ForMonth,
                                    RecordDate = p.RecordDate,
                                    DetailName = db.AC_Detail.Where(x => x.DetailCode == p.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                    Amount = p.Amount,
                                    Remarks = p.Remarks,
                                    Balance = p.Balance,
                                    ReportType = "Single",
                                }).ToList();
                    }
                    else
                    {
                        list = (from p in db.HRM_SalaryPayment
                                join s in db.HRM_Salary on p.SalaryId equals s.SalaryId
                                join e in db.HRM_Employee_Main on p.EmployeeId equals e.EmployeeId
                                where p.EmployeeId == employeeId2
                                select new clsSalaryPayment
                                {
                                    PaymentNo = p.PaymentNo,
                                    EmployeeName = e.FullName,
                                    TotalSalary = s.NetSalary,
                                    ForMonth = s.ForMonth,
                                    RecordDate = p.RecordDate,
                                    DetailName = db.AC_Detail.Where(x => x.DetailCode == p.PaidFromDetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                    Amount = p.Amount,
                                    Remarks = p.Remarks,
                                    Balance = p.Balance,
                                    ReportType = "Single",
                                }).ToList();
                    }
                    
                    salaryList.AddRange(list);
                }
            }
            return salaryList;
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
        #endregion
    }
}