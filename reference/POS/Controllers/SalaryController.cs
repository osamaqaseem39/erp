using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace POS.Controllers
{
    public class SalaryController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Salary
        // GET: /Salary/
        public ActionResult SalaryIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "Salary"
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
                    //string fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    //string toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    //ViewBag.FromDate = fromDate;
                    //ViewBag.ToDate = toDate;
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult getEmployeesForSalary(DateTime salaryMonth)
        {
            if (salaryMonth == null)
            {
                salaryMonth = DateTime.Now.Date;
            }
            salaryMonth = salaryMonth.Date;
            int days = DateTime.DaysInMonth(salaryMonth.Year, salaryMonth.Month);
            List<clsSalary> empList = new List<clsSalary>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("spGet_EmployeesForSalary", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@Month", SqlDbType.Int).Value = salaryMonth.Month;
            cmd.Parameters.Add("@Year", SqlDbType.Int).Value = salaryMonth.Year;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                empList.Add(new clsSalary()
                {
                    EmployeeId = Convert.ToInt32(dt["EmployeeId"]),
                    EmployeeName = dt["EmployeeName"].ToString(),
                    SalaryId = Convert.ToInt32(dt["SalaryId"]),
                    RecordDate = Convert.ToDateTime(dt["RecordDate"]),
                    TotalPresent = Convert.ToInt32(dt["TotalPresent"]),
                    TotalLeaves = Convert.ToInt32(dt["TotalLeave"]),
                    TotalAbsent = Convert.ToInt32(dt["TotalAbsent"]),
                    TotalHolidays = Convert.ToInt32(dt["TotalHolidays"]),
                    GrossPay = Convert.ToDecimal(dt["GrossSalary"]),
                    Advance = Convert.ToDecimal(dt["Advance"]),
                    AdvanceDeduction = Convert.ToDecimal(dt["AdvanceDeduction"]),
                    Loan = Convert.ToDecimal(dt["Loan"]),
                    LoanDeduction = Convert.ToDecimal(dt["LoanDeduction"]),
                    AddAdjustment = Convert.ToDecimal(dt["AddAdjustment"]),
                    LessDeduction = Convert.ToDecimal(dt["LessDeduction"]),
                    NetSalary = Convert.ToDecimal(dt["NetSalary"]),
                    Remarks = dt["Remarks"].ToString(),
                    TotalDaysOfMonth = days
                });
            }
            var data = empList.ToList();
            return new JsonResult { Data = new { data = data } };
        }
        [HttpPost]
        public ActionResult InsertUpdateSalary(string data, string print)
        {
            string message = "";
            bool status = false;
            List<clsSalary> salary = JsonConvert.DeserializeObject<List<clsSalary>>(data);
            List<clsSalary> salaryListForReport = new List<clsSalary>();
            clsSalary salarySingle;
            for (int i = 0; i < salary.Count; i++)
            {
                if (salary[i].AdvanceDeduction == null)
                {
                    salary[i].AdvanceDeduction = 0;
                }
                if (salary[i].LoanDeduction == null)
                {
                    salary[i].LoanDeduction = 0;
                }
                if (salary[i].AddAdjustment == null)
                {
                    salary[i].AddAdjustment = 0;
                }
                if (salary[i].LessDeduction == null)
                {
                    salary[i].LessDeduction = 0;
                }
                try
                {
                    System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
                    if(salary[i].SalaryId > 0)
                    {
                        db.spInsertUpdateSalary(salary[i].SalaryId, salary[i].EmployeeId, salary[i].ForMonth, salary[i].RecordDate, salary[i].Remarks, DateTime.Now, DateTime.Now, "Unpost", salary[i].TotalPresent, salary[i].TotalAbsent, salary[i].TotalLeaves, salary[i].TotalWorkingDays, salary[i].GrossPay, salary[i].Advance, salary[i].AdvanceDeduction, salary[i].Loan, salary[i].LoanDeduction, salary[i].AddAdjustment, salary[i].LessDeduction, salary[i].NetSalary, User.UserId, "Update", CheckReturn);
                    }          
                    else
                    {
                        db.spInsertUpdateSalary(salary[i].SalaryId, salary[i].EmployeeId, salary[i].ForMonth, salary[i].RecordDate, salary[i].Remarks, DateTime.Now, DateTime.Now, "Unpost", salary[i].TotalPresent, salary[i].TotalAbsent, salary[i].TotalLeaves, salary[i].TotalWorkingDays, salary[i].GrossPay, salary[i].Advance, salary[i].AdvanceDeduction, salary[i].Loan, salary[i].LoanDeduction, salary[i].AddAdjustment, salary[i].LessDeduction, salary[i].NetSalary, User.UserId, "Save", CheckReturn);
                    }
                    if(print == "Print")
                    {
                        salarySingle = new clsSalary();
                        salarySingle.SalaryId = Convert.ToInt32(CheckReturn.Value);
                        salarySingle.EmployeeId = salary[i].EmployeeId;
                        salarySingle.EmployeeName = db.HRM_Employee_Main.Where(x => x.EmployeeId == salarySingle.EmployeeId).Select(x => x.FullName).FirstOrDefault();
                        salarySingle.ForMonth = salary[i].ForMonth;
                        salarySingle.MonthYear = Convert.ToDateTime(salarySingle.ForMonth).ToString("MMMM") + " , " + Convert.ToDateTime(salarySingle.ForMonth).ToString("yyyy");
                        salarySingle.RecordDate = salary[i].RecordDate;
                        salarySingle.Remarks = salary[i].Remarks;
                        salarySingle.TotalPresent = salary[i].TotalPresent;
                        salarySingle.TotalAbsent = salary[i].TotalAbsent;
                        salarySingle.TotalLeaves = salary[i].TotalLeaves;
                        salarySingle.TotalWorkingDays = salary[i].TotalWorkingDays;
                        salarySingle.GrossPay = salary[i].GrossPay;
                        salarySingle.Advance = salary[i].Advance;
                        salarySingle.AdvanceDeduction = salary[i].AdvanceDeduction;
                        salarySingle.Loan = salary[i].Loan;
                        salarySingle.LoanDeduction = salary[i].LoanDeduction;
                        salarySingle.AddAdjustment = salary[i].AddAdjustment;
                        salarySingle.LessDeduction = salary[i].LessDeduction;
                        salarySingle.NetSalary = salary[i].NetSalary;
                        salarySingle.ReportType = "Single";
                        salaryListForReport.Add(salarySingle);
                    }
                    status = true;
                    message = "Salary saved successfully.";
                }
                catch (Exception ex)
                {
                    status = false;
                    message = "Error! Please try again.";
                }
                
            }
            if (print == "Print")
            {
                var list = salaryListForReport.ToList();
                TempData["List"] = list;
                TempData.Keep();
                //return RedirectToAction("salaryreport", "salary", new { salary = JsonConvert.SerializeObject(list) });
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        #endregion
        #region Salary Report
        //public ActionResult SalaryReport(string salary)
        public ActionResult SalaryReport()
        {
            if (User != null)
            {
                List<clsSalary> salaryList = (List<clsSalary>)TempData["List"];
                TempData.Keep();
                // List<clsSalary> salaryList = JsonConvert.DeserializeObject<List<clsSalary>>(salary);
                return View(salaryList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        #region Report
        public ActionResult SalaryMultiReport()
        {
            ViewBag.Employee = new SelectList(getEmployees(), "EmployeeId", "FullName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintSalaryReport()
        {
            if (User != null)
            {
                String[] emp = Request.Form.GetValues("EmployeeId");
                DateTime salaryMonth = Convert.ToDateTime(Request.Form["SalaryMonth"]);
                List<clsSalary> salaryList = new List<clsSalary>();

                if (emp != null)
                {
                        List<clsSalary> list = GetSalaries(1,emp, salaryMonth);
                        salaryList.AddRange(list);
                }
                else
                {
                    List<clsSalary> list = GetSalaries(0,emp, salaryMonth);
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
        public List<clsSalary> GetSalaries(int employeeId, String[] emp,DateTime salaryMonth)
        {
            List<clsSalary> list = new List<clsSalary>();
            List<clsSalary> salaryList = new List<clsSalary>();
            if (employeeId == 0)
            {
                list = (from f in db.HRM_Salary
                        join fd in db.HRM_Employee_Main on f.EmployeeId equals fd.EmployeeId
                        where f.ForMonth.Value.Month == salaryMonth.Month && f.ForMonth.Value.Year == salaryMonth.Year
                        select new clsSalary
                        {
                                SalaryId = f.SalaryId,
                                EmployeeId = f.EmployeeId,
                                EmployeeName = fd.FullName,
                                ForMonth = f.ForMonth,
                                RecordDate = f.RecordDate,
                                Remarks = f.Remarks ?? "",
                                TotalPresent = f.TotalPresent ?? 0,
                                TotalAbsent = f.TotalAbsent ?? 0,
                                TotalLeaves = f.TotalLeaves ?? 0,
                                TotalWorkingDays = f.TotalWorkingDays ?? 0,
                                GrossPay = f.GrossPay ?? 0,
                                Advance = f.Advance ?? 0,
                                AdvanceDeduction = f.AdvanceDeduction ?? 0,
                                Loan = f.Loan ?? 0,
                                LoanDeduction = f.LoanDeduction ?? 0,
                                AddAdjustment = f.AddAdjustment ?? 0,
                                LessDeduction = f.LessDeduction ?? 0,
                                NetSalary = f.NetSalary ?? 0,
                                ReportType = "Multi",
                        }).ToList();
                salaryList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < emp.Length; i++)
                {
                    int employeeId2 = Convert.ToInt32(emp[i]);
                    list = (from f in db.HRM_Salary
                            join fd in db.HRM_Employee_Main on f.EmployeeId equals fd.EmployeeId
                            where f.ForMonth.Value.Month == salaryMonth.Month && f.ForMonth.Value.Year == salaryMonth.Year && f.EmployeeId == employeeId2
                            select new clsSalary
                            {
                                SalaryId = f.SalaryId,
                                EmployeeId = f.EmployeeId,
                                EmployeeName = fd.FullName,
                                ForMonth = f.ForMonth,
                                RecordDate = f.RecordDate,
                                Remarks = f.Remarks ?? "",
                                TotalPresent = f.TotalPresent ?? 0,
                                TotalAbsent = f.TotalAbsent ?? 0,
                                TotalLeaves = f.TotalLeaves ?? 0,
                                TotalWorkingDays = f.TotalWorkingDays ?? 0,
                                GrossPay = f.GrossPay ?? 0,
                                Advance = f.Advance ?? 0,
                                AdvanceDeduction = f.AdvanceDeduction ?? 0,
                                Loan = f.Loan ?? 0,
                                LoanDeduction = f.LoanDeduction ?? 0,
                                AddAdjustment = f.AddAdjustment ?? 0,
                                LessDeduction = f.LessDeduction ?? 0,
                                NetSalary = f.NetSalary ?? 0,
                                ReportType = "Multi",
                            }).ToList();
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