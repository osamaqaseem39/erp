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
    public class AttendanceController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Attendance
        public ActionResult AttendanceIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "Attendance"
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
        public ActionResult getEmployeesForAttendance(DateTime attendanceDate)
        {
            if (attendanceDate == null)
            {
                attendanceDate = DateTime.Now.Date;
            }
            attendanceDate = attendanceDate.Date;
            List<clsEmployee> empList = new List<clsEmployee>();
            empList = (from c in db.HRM_Employee_Main
                                select new clsEmployee
                                {
                                  EmployeeId = c.EmployeeId,
                                  FullName = c.FullName,
                                  Attendance = db.HRM_Attendance.Where(x => x.AttendanceDate == attendanceDate && x.EmployeeId == c.EmployeeId).Select(x => x.Attendance).FirstOrDefault() ?? "P",
                                  AttendanceId = db.HRM_Attendance.Where(x => x.AttendanceDate == attendanceDate && x.EmployeeId == c.EmployeeId).Select(x => x.AttendanceId).FirstOrDefault(),
                                  InTime = db.HRM_Attendance.Where(x => x.AttendanceDate == attendanceDate && x.EmployeeId == c.EmployeeId).Select(x => x.InTime).FirstOrDefault(),
                                  OutTime = db.HRM_Attendance.Where(x => x.AttendanceDate == attendanceDate && x.EmployeeId == c.EmployeeId).Select(x => x.OutTime).FirstOrDefault(),
                                  AttendanceDate = attendanceDate,
                                  TimeRecord = c.TimeRecord ?? "N"
                                }).ToList();
            DateTime endDate = attendanceDate;
            DateTime startDate = new DateTime(endDate.Year, endDate.Month, 1);
            List<clsEmployee> DatesList = new List<clsEmployee>();
            for (DateTime dt = startDate; dt < endDate; dt = dt.AddDays(1))
            {
                DateTime date = dt;
                clsEmployee dateObj = new clsEmployee();
                dateObj.AttendanceDate = date;
                DatesList.Add(dateObj);
            }
            foreach (clsEmployee e in empList)
            {
                if(e.InTime != null)
                {
                    DateTime? dt = new DateTime(2050, 01, 01);
                    dt = dt + e.InTime;
                    e.InTimeDateTime = dt;
                }
                if (e.OutTime != null)
                {
                    DateTime? dt = new DateTime(2050, 01, 01);
                    dt = dt + e.OutTime;
                    e.OutTimeDateTime = dt;
                }
                e.DateList = new List<clsEmployee>();
                clsEmployee empp;
                foreach (clsEmployee d in DatesList)
                {
                    empp = new clsEmployee();
                    empp.AttendanceDate = d.AttendanceDate;
                    empp.Attendance = db.HRM_Attendance.Where(x => x.AttendanceDate == d.AttendanceDate && x.EmployeeId == e.EmployeeId).Select(x => x.Attendance).FirstOrDefault() ?? "";
                    e.DateList.Add(empp);
                }
                e.DateList = e.DateList.OrderByDescending(x => x.AttendanceDate).ToList();
            }

            var data = empList.ToList();
            return new JsonResult { Data = new { data = data } };
        }
        [HttpPost]
        public ActionResult InsertUpdateAttendance(string data)
        {
            string message = "";
            bool status = false;
            int returnId = 0;
            List<clsEmployee> attendance = JsonConvert.DeserializeObject<List<clsEmployee>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("AttendanceId");
            dt.Columns.Add("EmployeeId");
            dt.Columns.Add("AttendanceDate");
            dt.Columns.Add("Attendance");
            dt.Columns.Add("InTime");
            dt.Columns.Add("OutTime");
            dt.Columns.Add("Minutes");
            for (int i = 0; i < attendance.Count; i++)
            {
                dt.Rows.Add(new object[] { i + 1, attendance[i].AttendanceId, attendance[i].EmployeeId, attendance[i].AttendanceDate, attendance[i].Attendance, attendance[i].InTime, attendance[i].OutTime, attendance[i].Minutes });
            }
            returnId = AddEditAttendance(attendance[0].AttendanceDate, dt);
            if (returnId > 0)
            {
                ModelState.Clear();
                status = true;
                message = "Attendance Successfully Saved";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        private int AddEditAttendance(DateTime? attendanceDate, DataTable dt)
        {
            int returnId = 0;
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateAttendance", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@AttendanceDate", SqlDbType.Date).Value = attendanceDate;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                        cmd.Parameters.Add("@dtAttendance", SqlDbType.Structured).Value = dt;
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
        #region Attendance Report
        public ActionResult AttendanceReport(DateTime id)
        {
            if (User != null)
            {
                if (id == null)
                {
                    id = DateTime.Now.Date;
                }
                id = id.Date;
                List<clsEmployee> empList = new List<clsEmployee>();
                empList = (from c in db.HRM_Employee_Main
                           select new clsEmployee
                           {
                               EmployeeId = c.EmployeeId,
                               FullName = c.FullName,
                           }).ToList();
                DateTime endDate = id;
                DateTime startDate = new DateTime(endDate.Year, endDate.Month, 1);
                List<clsEmployee> DatesList = new List<clsEmployee>();
                for (DateTime dt = startDate; dt <= endDate; dt = dt.AddDays(1))
                {
                    DateTime date = dt;
                    clsEmployee dateObj = new clsEmployee();
                    dateObj.AttendanceDate = date;
                    DatesList.Add(dateObj);
                }
                foreach (clsEmployee e in empList)
                {
                    if (e.InTime != null)
                    {
                        DateTime? dt = new DateTime(2050, 01, 01);
                        dt = dt + e.InTime;
                        e.InTimeDateTime = dt;
                    }
                    if (e.OutTime != null)
                    {
                        DateTime? dt = new DateTime(2050, 01, 01);
                        dt = dt + e.OutTime;
                        e.OutTimeDateTime = dt;
                    }
                    e.DateList = new List<clsEmployee>();
                    clsEmployee empp;
                    foreach (clsEmployee d in DatesList)
                    {
                        empp = new clsEmployee();
                        empp.AttendanceDate = d.AttendanceDate;
                        empp.AttendanceDay = d.AttendanceDate.Value.Day.ToString();
                        empp.Attendance = db.HRM_Attendance.Where(x => x.AttendanceDate == d.AttendanceDate && x.EmployeeId == e.EmployeeId).Select(x => x.Attendance).FirstOrDefault() ?? "";
                        e.DateList.Add(empp);
                    }
                    e.DateList = e.DateList.OrderByDescending(x => x.AttendanceDate).ToList();
                }
                return View(empList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        #region Reports
        public ActionResult AttendanceMultiReport()
        {
            ViewBag.Employee = new SelectList(getEmployees(), "EmployeeId", "FullName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintAttendanceReport()
        {
            if (User != null)
            {
                String[] emp = Request.Form.GetValues("EmployeeId");
                List<clsEmployee> empList = new List<clsEmployee>();
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                if (emp != null)
                {
                    List<clsEmployee> list = GetAttendance(1, emp, fromDate, toDate);
                    empList.AddRange(list);
                }
                else
                {
                    List<clsEmployee> list = GetAttendance(0, emp, fromDate, toDate);
                    empList.AddRange(list);
                }
                empList = empList.OrderBy(x => x.FullName).ToList();
                return View(empList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsEmployee> GetAttendance(int employeeId, String[] emp, DateTime fromDate, DateTime toDate)
        {
            List<clsEmployee> list = new List<clsEmployee>();
            List<clsEmployee> empList = new List<clsEmployee>();
            if (employeeId == 0)
            {
                list = (from c in db.HRM_Employee_Main
                           select new clsEmployee
                           {
                               EmployeeId = c.EmployeeId,
                               FullName = c.FullName,
                               FromDate = fromDate,
                               ToDate = toDate
                           }).ToList();
                empList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < emp.Length; i++)
                {
                    int employeeId2 = Convert.ToInt32(emp[i]);
                    list = (from c in db.HRM_Employee_Main
                            where c.EmployeeId == employeeId2
                            select new clsEmployee
                            {
                                EmployeeId = c.EmployeeId,
                                FullName = c.FullName,
                                FromDate = fromDate,
                                ToDate = toDate
                            }).ToList();
                    empList.AddRange(list);
                }
            }
            DateTime endDate = toDate;
                DateTime startDate = fromDate;
                List<clsEmployee> DatesList = new List<clsEmployee>();
                for (DateTime dt = startDate; dt <= endDate; dt = dt.AddDays(1))
                {
                    DateTime date = dt;
                    clsEmployee dateObj = new clsEmployee();
                    dateObj.AttendanceDate = date;
                    DatesList.Add(dateObj);
                }
                foreach (clsEmployee e in empList)
                {
                    if (e.InTime != null)
                    {
                        DateTime? dt = new DateTime(2050, 01, 01);
                        dt = dt + e.InTime;
                        e.InTimeDateTime = dt;
                    }
                    if (e.OutTime != null)
                    {
                        DateTime? dt = new DateTime(2050, 01, 01);
                        dt = dt + e.OutTime;
                        e.OutTimeDateTime = dt;
                    }
                    e.DateList = new List<clsEmployee>();
                    clsEmployee empp;
                    foreach (clsEmployee d in DatesList)
                    {
                        empp = new clsEmployee();
                        empp.AttendanceDate = d.AttendanceDate;
                        empp.AttendanceDay = d.AttendanceDate.Value.Day.ToString();
                        empp.Attendance = db.HRM_Attendance.Where(x => x.AttendanceDate == d.AttendanceDate && x.EmployeeId == e.EmployeeId).Select(x => x.Attendance).FirstOrDefault() ?? "";
                        e.DateList.Add(empp);
                    }
                    e.DateList = e.DateList.OrderByDescending(x => x.AttendanceDate).ToList();
                }
            return empList;
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