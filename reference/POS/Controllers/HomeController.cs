using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using POS.Models;
using POS.Models.DAL.Security;
using System.Web.Security;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Linq.Dynamic;
using System.Data;

namespace POS.Controllers
{
    public class HomeController : BaseController
    {
        PosMBDb db = new PosMBDb();
        // GET: /Home/
        public ActionResult Index()
        {
            if (User != null)
            {
                clsDashboard dashboard = new clsDashboard();
                dashboard.ShowCashBankBalance = (from r in db.Admin_Setup_UserRights
                                                 where r.UserID == User.UserId && r.MenuName == "CashBankBalanceForDashboard"
                                                 select r.showMain).FirstOrDefault() ?? 0;
                dashboard.ShowTopSupplierBalance = (from r in db.Admin_Setup_UserRights
                                                    where r.UserID == User.UserId && r.MenuName == "TopSupplierBalanceForDashboard"
                                                    select r.showMain).FirstOrDefault() ?? 0;
                dashboard.ShowTopCustomerBalance = (from r in db.Admin_Setup_UserRights
                                                    where r.UserID == User.UserId && r.MenuName == "TopCustomerBalanceForDashboard"
                                                    select r.showMain).FirstOrDefault() ?? 0;
                dashboard.ShowAllCustomersBalance = (from r in db.Admin_Setup_UserRights
                                                     where r.UserID == User.UserId && r.MenuName == "AllCustomerBalanceForDashboard"
                                                     select r.showMain).FirstOrDefault() ?? 0;
                dashboard.ShowSalePieChart = (from r in db.Admin_Setup_UserRights
                                              where r.UserID == User.UserId && r.MenuName == "SalePieChartForDashboard"
                                              select r.showMain).FirstOrDefault() ?? 0;
                dashboard.ShowReminder = (from r in db.Admin_Setup_UserRights
                                          where r.UserID == User.UserId && r.MenuName == "ReminderForDashboard"
                                          select r.showMain).FirstOrDefault() ?? 0;
                dashboard.ShowSaleInvoiceCreditDays = (from r in db.Admin_Setup_UserRights
                                                       where r.UserID == User.UserId && r.MenuName == "SaleInvoiceDueForDashboard"
                                          select r.showMain).FirstOrDefault() ?? 0;
                dashboard.ShowLastYearCurrentMonthSale = (from r in db.Admin_Setup_UserRights
                                                          where r.UserID == User.UserId && r.MenuName == "LastSaleAmount"
                                          select r.showMain).FirstOrDefault() ?? 0;
                dashboard.ShowCurrentMonthSale = (from r in db.Admin_Setup_UserRights
                                                  where r.UserID == User.UserId && r.MenuName == "CurrentSaleAmount"
                                          select r.showMain).FirstOrDefault() ?? 0;
                dashboard.ShowLastYearCurrentMonthReceived = (from r in db.Admin_Setup_UserRights
                                                              where r.UserID == User.UserId && r.MenuName == "LastReceivedAmount"
                                          select r.showMain).FirstOrDefault() ?? 0;
                dashboard.ShowCurrentMonthReceived = (from r in db.Admin_Setup_UserRights
                                                      where r.UserID == User.UserId && r.MenuName == "CurrentReceivedmount"
                                          select r.showMain).FirstOrDefault() ?? 0;

                //Get Pie Chart
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn2 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn2", typeof(decimal));
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn3 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn3", typeof(decimal));
                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;
                db.sp_GetTotalSaleAndValuesForPieChart(year, month, User.FiscalYearId, CheckReturn, CheckReturn2, CheckReturn3);
                decimal totalSale = 0;
                decimal cashSale = 0;
                decimal creditSale = 0;
                if (CheckReturn.Value.ToString() != "")
                {
                    totalSale = Convert.ToDecimal(CheckReturn.Value);
                }
                if (CheckReturn2.Value.ToString() != "")
                {
                    cashSale = Convert.ToDecimal(CheckReturn2.Value);
                }
                if (CheckReturn3.Value.ToString() != "")
                {
                    creditSale = Convert.ToDecimal(CheckReturn3.Value);
                }
                ViewBag.TotalSale = totalSale;
                ViewBag.CashSale = cashSale;
                ViewBag.CreditSale = creditSale;

                //Get Last Month and Current Month Values
                CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                CheckReturn2 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn2", typeof(decimal));
                CheckReturn3 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn3", typeof(decimal));
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn4 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn4", typeof(decimal));
                db.sp_GetLastYearAndCurrentMonthValuesForDashboard(year, month, User.FiscalYearId, CheckReturn, CheckReturn2, CheckReturn3, CheckReturn4);
                decimal lastMonthSale = 0;
                decimal currentMonthSale = 0;
                decimal lastMonthReceived = 0;
                decimal currentMonthReceived = 0;
                if (CheckReturn.Value.ToString() != "")
                {
                    lastMonthSale = Convert.ToDecimal(CheckReturn.Value);
                }
                if (CheckReturn2.Value.ToString() != "")
                {
                    currentMonthSale = Convert.ToDecimal(CheckReturn2.Value);
                }
                if (CheckReturn3.Value.ToString() != "")
                {
                    lastMonthReceived = Convert.ToDecimal(CheckReturn3.Value);
                }
                if (CheckReturn4.Value.ToString() != "")
                {
                    currentMonthReceived = Convert.ToDecimal(CheckReturn4.Value);
                }
                ViewBag.LastMonthSale = lastMonthSale;
                ViewBag.CurrentMonthSale = currentMonthSale;
                ViewBag.LastMonthReceived = lastMonthReceived;
                ViewBag.CurrentMonthReceived = currentMonthReceived;
                return View(dashboard);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetCashBankBalance()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_CashBankBalanceForDashboard", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID ", SqlDbType.Int).Value = User.UserId;
                cmd.Parameters.Add("@FiscalYearId ", SqlDbType.Int).Value = User.FiscalYearId;
                cmd.Parameters.Add("@BusinessId ", SqlDbType.Int).Value = 0;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                con.Close();
                DataTableReader dt = tdt.CreateDataReader();
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        DetailName = dt["Name"].ToString(),
                        BalanceAmountString = Math.Abs(Convert.ToDecimal(dt["Balance"])).ToString("#,##0.00"),
                    });
                }
                SqlConnection.ClearPool(con);
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = invoice.Count();
                var data = invoice.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetTopSupplierBalance()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_TopSuppliersBalanceForDashboard", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID ", SqlDbType.Int).Value = User.UserId;
                cmd.Parameters.Add("@FiscalYearId ", SqlDbType.Int).Value = User.FiscalYearId;
                cmd.Parameters.Add("@BusinessId ", SqlDbType.Int).Value = 0;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                con.Close();
                DataTableReader dt = tdt.CreateDataReader();
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        DetailName = dt["Name"].ToString(),
                        BalanceAmountString = Math.Abs(Convert.ToDecimal(dt["Balance"])).ToString("#,##0.00"),
                    });
                }
                SqlConnection.ClearPool(con);
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = invoice.Count();
                var data = invoice.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetTopCustomerBalance()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_TopCustomersBalanceForDashboard", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID ", SqlDbType.Int).Value = User.UserId;
                cmd.Parameters.Add("@FiscalYearId ", SqlDbType.Int).Value = User.FiscalYearId;
                cmd.Parameters.Add("@BusinessId ", SqlDbType.Int).Value = 0;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                con.Close();
                DataTableReader dt = tdt.CreateDataReader();
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        DetailName = dt["Name"].ToString(),
                        BalanceAmountString = Math.Abs(Convert.ToDecimal(dt["Balance"])).ToString("#,##0.00"),
                    });
                }
                SqlConnection.ClearPool(con);
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = invoice.Count();
                var data = invoice.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetAllCustomerBalance()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_AllCustomersBalanceForDashboard", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID ", SqlDbType.Int).Value = User.UserId;
                cmd.Parameters.Add("@FiscalYearId ", SqlDbType.Int).Value = User.FiscalYearId;
                cmd.Parameters.Add("@BusinessId ", SqlDbType.Int).Value = 0;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                con.Close();
                DataTableReader dt = tdt.CreateDataReader();
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        DetailName = dt["Name"].ToString(),
                        BalanceAmountString = Convert.ToDecimal(dt["Balance"]).ToString("#,##0.00"),
                        CreditLimitString = Convert.ToDecimal(dt["CreditLimit"]).ToString("#,##0.00"),
                        OverDueString = Convert.ToDecimal(dt["Overdue"]).ToString("#,##0.00"),
                    });
                }
                SqlConnection.ClearPool(con);
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = invoice.Count();
                var data = invoice.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        
        public ActionResult GetSaleInvoicesCreditDays()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_SaleInvoiceCreditDaysForDashboard", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FiscalYearId ", SqlDbType.Int).Value = User.FiscalYearId;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                con.Close();
                DataTableReader dt = tdt.CreateDataReader();
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        ComputerNo = dt["ComputerNo"].ToString(),
                        RecordDate = Convert.ToDateTime(dt["RecordDate"]),
                        ByDate = Convert.ToDateTime(dt["DueDate"]),
                        DetailName = dt["Customer"].ToString(),
                        BalanceAmountString = Convert.ToDecimal(dt["Amount"]).ToString("#,##0.00"),
                    });
                }
                SqlConnection.ClearPool(con);
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = invoice.Count();
                var data = invoice.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #region Reminder
        public ActionResult GetAllReminders()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_Reminders", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID ", SqlDbType.Int).Value = User.UserId;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                con.Close();
                DataTableReader dt = tdt.CreateDataReader();
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        LogId = Convert.ToInt32(dt["LogID"]),
                        DetailName = dt["Name"].ToString(),
                        RecordDate = Convert.ToDateTime(dt["DueDate"]),
                        Remarks = dt["Remarks"].ToString(),
                    });
                }
                SqlConnection.ClearPool(con);
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = invoice.Count();
                var data = invoice.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult InsertReminder()
        {
            if (User != null)
            {
                ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                return PartialView();
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertReminder(clsReminder reminder)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            db.spInsertUpdateReminder(reminder.LogID, User.UserId, reminder.DetailCode, reminder.DueDate, reminder.Remarks, reminder.DueDateAlert, reminder.Status, "Save", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) > 0)
            {
                ModelState.Clear();
                status = true;
                message = "Reminder Successfully Saved";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult UpdateReminder(int id)
        {
            if (User != null)
            {
                ViewBag.LogID = id;
                return PartialView();
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult UpdateReminder(clsReminder reminder)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            db.spInsertUpdateReminder(reminder.LogID, User.UserId, reminder.DetailCode, reminder.DueDate, reminder.Remarks, reminder.DueDateAlert, reminder.Status, "Update", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) > 0)
            {
                ModelState.Clear();
                status = true;
                message = "Reminder Successfully Updated";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        //public ActionResult DeleteReminder(int id)
        //{
        //    string message = "";
        //    bool status = false;
        //    System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
        //    db.spInsertUpdateCity(id, "", User.UserId, "Delete", CheckReturn);
        //    if (Convert.ToDecimal(CheckReturn.Value) == 2)
        //    {
        //        ModelState.Clear();
        //        status = true;
        //        message = "City Successfully Deleted";
        //    }
        //    else if (Convert.ToInt32(CheckReturn.Value) == -9999)
        //    {
        //        ModelState.Clear();
        //        status = false;
        //        message = "Error! Please delete all data against this city first.";
        //    }
        //    else
        //    {
        //        ModelState.Clear();
        //        status = false;
        //        message = "Error! Please try again.";
        //    }
        //    return new JsonResult { Data = new { status = status, message = message } };
        //}
        public List<clsAccountDetail> getDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && (c.Type == "Customer" || c.Type == "Customer + Supplier")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).ToList();
            return detail;
        }
        
        #endregion
        public ActionResult DeleteDueDateAlert(string id)
        {
            string message = "";
            bool status = false;
            try
            {
                var invoice = db.Invoice_Main.Where(x => x.ComputerNo == id).FirstOrDefault();
                invoice.NoDueDateAlert = 1;
                db.SaveChanges();
                status = true;
            }
            catch (Exception ex)
            {
                status = false;
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
    }
}