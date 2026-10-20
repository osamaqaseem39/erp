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
    public class EmployeeController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult EmployeeIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "Employee"
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
                    ViewBag.Religion = new SelectList(getReligions(), "ReligionId", "ReligionName");
                    ViewBag.Country = new SelectList(getCountries(), "CountryId", "CountryName");
                    //ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
                    ViewBag.Relation = new SelectList(getRelations(), "RelationId", "RelationName");
                    ViewBag.Department = new SelectList(getDepartments(), "DepartmentId", "DepartmentName");
                    ViewBag.WorkNature = new SelectList(getWorkNatures(), "WorkNatureId", "Description");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateEmployee(string data)
        {
            string message = "";
            bool status = false;
            int returnId = 0;
            List<clsEmployee> employee = JsonConvert.DeserializeObject<List<clsEmployee>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("WorkExperienceId");
            dt.Columns.Add("EmployeeId");
            dt.Columns.Add("JobDescription");
            dt.Columns.Add("Organization");
            dt.Columns.Add("FromDate");
            dt.Columns.Add("ToDate");
            int employeeId = 0;
            string edit = "";
            if (employee[0].EmployeeId > 0)
            {
                employeeId = employee[0].EmployeeId;
                edit = "Edit";
                //if (employee[0].JobDescription == null)
                //{
                //    dt.Rows.Add(new object[] { 0, 0, 0, "", "", DateTime.Now, DateTime.Now });
                //}
                //else
                //{
                    for (int i = 0; i < employee.Count; i++)
                    {
                        dt.Rows.Add(new object[] { i + 1, 0, 0, employee[i].JobDescription, employee[i].Organization, employee[i].FromDate, employee[i].ToDate });
                    }
                //}
                returnId = AddEditEmployee(employee[0] , dt, "Update");
                if (returnId == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Employee Successfully Updated";
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
                edit = "New";
                //if (employee[0].JobDescription == null)
                //{
                //    dt.Rows.Add(new object[] { 0, 0, 0, "", "", DateTime.Now, DateTime.Now });
                //}
                //else
                //{
                    for (int i = 0; i < employee.Count; i++)
                    {
                        dt.Rows.Add(new object[] { i + 1, 0, 0, employee[i].JobDescription, employee[i].Organization, employee[i].FromDate, employee[i].ToDate });
                    }
                //}
                returnId = AddEditEmployee(employee[0], dt, "Save");
                if (returnId > 0)
                {
                    employeeId = returnId;
                    ModelState.Clear();
                    status = true;
                    message = "Employee Successfully Saved";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, employeeId = employeeId, edit = edit } };
        }
        private int AddEditEmployee(clsEmployee emp, DataTable dt, string insertUpdateStatus)
        {
            int returnId = 0;
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            if(emp.Gender == null)
            {
                emp.Gender = "";
            }
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateEmployee", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = emp.EmployeeId;
                        cmd.Parameters.Add("@EmployeeRefId", SqlDbType.NVarChar).Value = emp.EmployeeRefId;
                        cmd.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = emp.FullName;
                        cmd.Parameters.Add("@LastNameType", SqlDbType.NVarChar).Value = emp.LastNameType;
                        cmd.Parameters.Add("@LastName", SqlDbType.NVarChar).Value = emp.LastName;
                        cmd.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value = emp.DateOfBirth;
                        cmd.Parameters.Add("@Gender", SqlDbType.NVarChar).Value = emp.Gender;
                        cmd.Parameters.Add("@ReligionId", SqlDbType.Int).Value = emp.ReligionId;
                        cmd.Parameters.Add("@MaritalStatus", SqlDbType.NVarChar).Value = emp.MaritalStatus;
                        cmd.Parameters.Add("@CountryId", SqlDbType.Int).Value = emp.CountryId;
                        cmd.Parameters.Add("@CityId", SqlDbType.Int).Value = emp.CityId;
                        cmd.Parameters.Add("@CNIC", SqlDbType.NVarChar).Value = emp.CNIC;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = emp.Email;
                        cmd.Parameters.Add("@PresentAddress", SqlDbType.NVarChar).Value = emp.PresentAddress;
                        cmd.Parameters.Add("@PermanentAddressSame", SqlDbType.NVarChar).Value = emp.PermanentAddressSame;
                        cmd.Parameters.Add("@PermanentAddress", SqlDbType.NVarChar).Value = emp.PermanentAddress;
                        cmd.Parameters.Add("@ContactNo1", SqlDbType.NVarChar).Value = emp.ContactNo1;
                        cmd.Parameters.Add("@ContactNo2", SqlDbType.NVarChar).Value = emp.ContactNo2;
                        cmd.Parameters.Add("@EmgContactNo", SqlDbType.NVarChar).Value = emp.EmgContactNo;
                        cmd.Parameters.Add("@EmgContactName", SqlDbType.NVarChar).Value = emp.EmgContactName;
                        cmd.Parameters.Add("@RelationId", SqlDbType.Int).Value = emp.RelationId;
                        cmd.Parameters.Add("@AdditionalNote", SqlDbType.NVarChar).Value = emp.AdditionalNote;
                        cmd.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = emp.DepartmentId;
                        cmd.Parameters.Add("@WorkNatureId", SqlDbType.Int).Value = emp.WorkNatureId;
                        cmd.Parameters.Add("@ImmediateBossId", SqlDbType.Int).Value = emp.ImmediateBossId;
                        cmd.Parameters.Add("@GrossSalary", SqlDbType.Int).Value = emp.GrossSalary;
                        cmd.Parameters.Add("@DailyAllowance", SqlDbType.Int).Value = emp.DailyAllowance;
                        cmd.Parameters.Add("@TimeRecord", SqlDbType.NVarChar).Value = emp.TimeRecord;
                        cmd.Parameters.Add("@Qualification", SqlDbType.NVarChar).Value = emp.Qualification;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                        cmd.Parameters.Add("@dtHRM_WorkExperienceType", SqlDbType.Structured).Value = dt;
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.NVarChar).Value = insertUpdateStatus;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
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
        public ActionResult DeleteEmployee(int id)
        {
            string message = "";
            bool status = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("WorkExperienceId");
            dt.Columns.Add("EmployeeId");
            dt.Columns.Add("JobDescription");
            dt.Columns.Add("Organization");
            dt.Columns.Add("FromDate");
            dt.Columns.Add("ToDate");
            dt.Rows.Add(new object[] { 0, 0, 0, "", "", DateTime.Now,DateTime.Now });
            clsEmployee emp = new clsEmployee();
            emp.EmployeeId = id;
            emp.DateOfBirth = DateTime.Now;
            int returnId = AddEditEmployee(emp, dt, "Delete");
            if (returnId == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Employee Successfully Deleted";
            }
            else if (returnId == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this employee first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult EmployeesList()
        {
            return PartialView();
        }
        public ActionResult GetAllEmployees()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var fullName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var country = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var department = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var workNature = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsEmployee> employees = (from f in db.HRM_Employee_Main
                                              select new clsEmployee
                                              {
                                                  EmployeeId = f.EmployeeId,
                                                  EmployeeRefId = f.EmployeeRefId,
                                                  FullName = f.FullName + " " + f.LastNameType + " " + f.LastName ?? "",
                                                  //LastNameType = f.LastNameType,
                                                  //LastName = f.LastName,
                                                  DateOfBirth = f.DateOfBirth,
                                                  Gender = f.Gender,
                                                  ReligionId = f.ReligionId,
                                                  ReligionName = db.HRM_Religion.Where(x=>x.ReligionId == f.ReligionId).Select(x=>x.ReligionName).FirstOrDefault() ?? "",
                                                  CNIC = f.CNIC,
                                                  CountryName = db.Countries.Where(x => x.CountryId == f.CountryId).Select(x => x.CountryName).FirstOrDefault() ?? "",
                                                  DepartmentName = db.HRM_Department.Where(x => x.DepartmentId == f.DepartmentId).Select(x => x.DepartmentName).FirstOrDefault() ?? "",
                                                  WorkNatureName = db.HRM_WorkNature.Where(x => x.WorkNatureId == f.WorkNatureId).Select(x => x.Description).FirstOrDefault() ?? "",
                                                  //MaritalStatus = f.MaritalStatus,
                                                  //CountryId = f.CountryId,
                                                  //CityId = f.CityId,
                                                  //Email = f.Email,
                                                  //PresentAddress = f.PresentAddress,
                                                  //PermanentAddressSame = f.PermanentAddressSame,
                                                  //PermanentAddress = f.PermanentAddress,
                                                  //ContactNo1 = f.ContactNo1,
                                                  //ContactNo2 = f.ContactNo2,
                                                  //EmgContactNo = f.EmgContactNo,
                                                  //EmgContactName = f.EmgContactName,
                                                  //RelationId = f.RelationId,
                                                  //AdditionalNote = f.AdditionalNote,
                                                  //DepartmentId = f.DepartmentId,
                                                  //WorkNatureId = f.WorkNatureId,
                                                  //ImmediateBossId = f.ImmediateBossId,
                                                  //GrossSalary = f.GrossSalary,
                                                  //DailyAllowance = f.DailyAllowance,
                                                  //TimeRecord = f.TimeRecord,
                                                  //Qualification = f.Qualification
                                              }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(fullName))
                {
                    employees = employees.Where(c => c.FullName.ToLower().Contains(fullName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(country))
                {
                    employees = employees.Where(c => c.CountryName.ToLower().Contains(country.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(department))
                {
                    employees = employees.Where(c => c.DepartmentName.ToLower().Contains(department.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(workNature))
                {
                    employees = employees.Where(c => c.WorkNatureName.ToLower().Contains(workNature.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        employees = employees.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = employees.Count();
                var data = employees.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsReligion> getReligions()
        {
            List<clsReligion> religion = new List<clsReligion>();
            religion = (from c in db.HRM_Religion
                        select new clsReligion
                      {
                          ReligionId = c.ReligionId,
                          ReligionName = c.ReligionName
                      }).OrderBy(x=>x.ReligionName).ToList();
            return religion;
        }
        
        public List<clsCountry> getCountries()
        {
            List<clsCountry> country = new List<clsCountry>();
            country = (from c in db.Countries
                       select new clsCountry
                        {
                            CountryId = c.CountryId,
                            CountryName = c.CountryName
                        }).OrderBy(x => x.CountryName).ToList();
            return country;
        }
        public ActionResult getCities(int id)
        {
            List<clsCity> city = new List<clsCity>();
            city = (from c in db.Cities
                    where c.CountryId == id
                    select new clsCity
                       {
                           CityId = c.CityId,
                           CityName = c.CityName
                       }).OrderBy(x => x.CityName).ToList();
            var data = city.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public List<clsRelation> getRelations()
        {
            List<clsRelation> relation = new List<clsRelation>();
            relation = (from c in db.HRM_Relation
                        select new clsRelation
                        {
                            RelationId = c.RelationId,
                            RelationName = c.RelationName
                        }).OrderBy(x => x.RelationName).ToList();
            return relation;
        }
        public List<clsDepartment> getDepartments()
        {
            List<clsDepartment> department = new List<clsDepartment>();
            department = (from c in db.HRM_Department
                          select new clsDepartment
                        {
                            DepartmentId = c.DepartmentId,
                            DepartmentName = c.DepartmentName
                        }).OrderBy(x => x.DepartmentName).ToList();
            return department;
        }
        public List<clsWorkNature> getWorkNatures()
        {
            List<clsWorkNature> workNature = new List<clsWorkNature>();
            workNature = (from c in db.HRM_WorkNature
                          select new clsWorkNature
                          {
                              WorkNatureId = c.WorkNatureId,
                              Description = c.Description
                          }).OrderBy(x => x.Description).ToList();
            return workNature;
        }
        public ActionResult GetEmployeeById(int id)
        {
            clsEmployee relation = new clsEmployee();
            relation = (from f in db.HRM_Employee_Main
                        where f.EmployeeId == id
                        select new clsEmployee
                        {
                            EmployeeId = f.EmployeeId,
                            EmployeeRefId = f.EmployeeRefId,
                            FullName = f.FullName,
                            LastNameType = f.LastNameType,
                            LastName = f.LastName,
                            DateOfBirth = f.DateOfBirth,
                            Gender = f.Gender,
                            ReligionId = f.ReligionId,
                            MaritalStatus = f.MaritalStatus,
                            CountryId = f.CountryId,
                            CityId = f.CityId,
                            CNIC = f.CNIC,
                            Email = f.Email,
                            PresentAddress = f.PresentAddress,
                            PermanentAddressSame = f.PermanentAddressSame,
                            PermanentAddress = f.PermanentAddress,
                            ContactNo1 = f.ContactNo1,
                            ContactNo2 = f.ContactNo2,
                            EmgContactNo = f.EmgContactNo,
                            EmgContactName = f.EmgContactName,
                            RelationId = f.RelationId,
                            AdditionalNote = f.AdditionalNote,
                            DepartmentId = f.DepartmentId,
                            WorkNatureId = f.WorkNatureId,
                            ImmediateBossId = f.ImmediateBossId,
                            GrossSalary = f.GrossSalary,
                            DailyAllowance = f.DailyAllowance,
                            TimeRecord = f.TimeRecord,
                            Qualification = f.Qualification,
                            ImmediateBossName = db.HRM_Employee_Main.Where(x=>x.EmployeeId == f.ImmediateBossId).Select(x=>x.FullName).FirstOrDefault()
                        }).FirstOrDefault();
            var data = relation;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetWorkExperienceByEmployeeId(int id)
        {
            List<clsEmployee> experience = new List<clsEmployee>();
            experience = (from f in db.HRM_WorkExperience
                        where f.EmployeeId == id
                        select new clsEmployee
                        {
                            WorkExperienceId = f.WorkExperienceId,
                            EmployeeId = id,
                            JobDescription = f.JobDescription,
                            Organization = f.Organization,
                            FromDate = f.FromDate,
                            ToDate = f.ToDate
                        }).ToList();
            var data = experience.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SelectBoss(int id)
        {
            List<clsEmployee> employees = new List<clsEmployee>();
            employees = (from f in db.HRM_Employee_Main
                        where f.EmployeeId != id
                        select new clsEmployee
                        {
                            EmployeeId = f.EmployeeId,
                            FullName = f.FullName,
                            LastName = f.LastName,
                        }).ToList();
            return PartialView(employees);
        }
    }
}