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
    public class CashController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Cash Payment
        // GET: /Cash/
        public ActionResult CashPaymentIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CashPayment"
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
                    ViewBag.DetailCodeLine = new SelectList(getDetailCodesForLine(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdatePaymentVoucher(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsVoucher> voucher = JsonConvert.DeserializeObject<List<clsVoucher>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("DetailCode");
            dt.Columns.Add("Debit");
            dt.Columns.Add("Credit");
            dt.Columns.Add("Remarks");
            dt.Columns.Add("ChequeNo");
            dt.Columns.Add("ChequeComputerNo");
            dt.Columns.Add("TaxId");
            dt.Columns.Add("TaxRate");
            dt.Columns.Add("TaxValue");
            dt.Columns.Add("NetDebit");
            dt.Columns.Add("NetCredit");
            if (voucher[0].Debit == null)
            {
                voucher[0].Debit = 0;
            }
            if (voucher[0].Credit == null)
            {
                voucher[0].Credit = 0;
            }
            if (voucher[0].ChequeNo == null)
            {
                voucher[0].ChequeNo = "";
            }
            if (voucher[0].ChequeComputerNo == null)
            {
                voucher[0].ChequeComputerNo = 0;
            }
            string voucherNo = voucher[0].VoucherNo;
            if (voucher[0].IsNewOrEdit == "Edit")
            {
                for (int i = 0; i < voucher.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, voucher[i].LineDetailCode, voucher[i].LineDebit, voucher[i].LineCredit, voucher[i].LineRemarks, voucher[i].LineChequeNo, voucher[i].LineChequeComputerNo, voucher[i].TaxID, voucher[i].TaxRate, voucher[i].TaxValue, voucher[i].NetDebit, voucher[i].NetCredit });
                }
                returnId = AddEditVoucher(voucher[0].VoucherNo, "Cash Payment", voucher[0].VoucherDate, voucher[0].DetailCode, voucher[0].Debit, voucher[0].Credit, voucher[0].RefrenceNo, voucher[0].Remarks, voucher[0].PaidBy, voucher[0].ReceivedBy, voucher[0].ChequeComputerNo, voucher[0].ChequeNo, "Unpost", User.UserId, User.UserId, DateTime.Now, DateTime.Now, voucher[0].DefaultVoucher, 0, User.FiscalYearId, dt, "Update");
                if (returnId == voucherNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Voucher Successfully Updated";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(voucherNo);
                    }
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
                for (int i = 0; i < voucher.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, voucher[i].LineDetailCode, voucher[i].LineDebit, voucher[i].LineCredit, voucher[i].LineRemarks, voucher[i].LineChequeNo, voucher[i].LineChequeComputerNo, voucher[i].TaxID, voucher[i].TaxRate, voucher[i].TaxValue, voucher[i].NetDebit, voucher[i].NetCredit });
                }
                returnId = AddEditVoucher(voucher[0].VoucherNo, "Cash Payment", voucher[0].VoucherDate, voucher[0].DetailCode, voucher[0].Debit, voucher[0].Credit, voucher[0].RefrenceNo, voucher[0].Remarks, voucher[0].PaidBy, voucher[0].ReceivedBy, voucher[0].ChequeComputerNo, voucher[0].ChequeNo, "Unpost", User.UserId, User.UserId, DateTime.Now, DateTime.Now, voucher[0].DefaultVoucher, 0, User.FiscalYearId, dt, "Save");
                voucherNo = returnId;
                if (returnId != "")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Voucher Successfully Saved";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(voucherNo);
                    }
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, voucherNo = voucherNo } };
        }
        private string AddEditVoucher(string voucherNo, string voucherType, DateTime? voucherDate, decimal? detailCode, decimal? debit, decimal? credit, string referenceNo, string remarks, string paidBy, string receivedBy, int? chequeCompNo, string chequeNo, string status, int? userId, int? lastUserId, DateTime? saveDate, DateTime? lastDate, int? defaultVoucher, int businessId, int fiscalYear, DataTable dt, string insertUpdateStatus)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateVoucher", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@VoucherNo", SqlDbType.VarChar, 20).Value = voucherNo;
                        cmd.Parameters.Add("@VoucherType", SqlDbType.VarChar, 20).Value = voucherType;
                        cmd.Parameters.Add("@VoucherDate", SqlDbType.DateTime).Value = voucherDate;
                        cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = detailCode;
                        cmd.Parameters.Add("@Debit", SqlDbType.Decimal).Value = debit;
                        cmd.Parameters.Add("@Credit", SqlDbType.Decimal).Value = credit;
                        cmd.Parameters.Add("@RefrenceNo", SqlDbType.VarChar, 50).Value = referenceNo;
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 255).Value = remarks;
                        cmd.Parameters.Add("@PaidBy", SqlDbType.VarChar, 50).Value = paidBy;
                        cmd.Parameters.Add("@ReceivedBy", SqlDbType.VarChar, 50).Value = receivedBy;
                        cmd.Parameters.Add("@ChequeComputerNo", SqlDbType.Int).Value = chequeCompNo;
                        cmd.Parameters.Add("@ChequeNo", SqlDbType.VarChar, 40).Value = chequeNo;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 30).Value = status;
                        cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                        cmd.Parameters.Add("@LastUserID", SqlDbType.VarChar, 30).Value = lastUserId.ToString();
                        cmd.Parameters.Add("@SaveDateTime", SqlDbType.DateTime).Value = saveDate;
                        cmd.Parameters.Add("@LastEditDateTime", SqlDbType.DateTime).Value = lastDate;
                        cmd.Parameters.Add("@DefaultVoucher", SqlDbType.Int).Value = defaultVoucher;
                        cmd.Parameters.Add("@BusinessID", SqlDbType.Int).Value = businessId;
                        cmd.Parameters.Add("@FiscalYearID", SqlDbType.Int).Value = fiscalYear;
                        cmd.Parameters.Add("@dtVoucherDetail", SqlDbType.Structured).Value = dt;
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.NVarChar).Value = insertUpdateStatus;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        returnId = cmd.Parameters["@CheckReturn"].Value.ToString();
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
        public ActionResult DeleteCashPayment(string id)
        {
            string message = "";
            bool status = false;
            DataTable dt = new DataTable();
            List<clsVoucher> voucher = new List<clsVoucher>();
            dt.Columns.Add("Id");
            dt.Columns.Add("DetailCode");
            dt.Columns.Add("Debit");
            dt.Columns.Add("Credit");
            dt.Columns.Add("Remarks");
            dt.Columns.Add("ChequeNo");
            dt.Columns.Add("ChequeComputerNo");
            dt.Columns.Add("TaxId");
            dt.Columns.Add("TaxRate");
            dt.Columns.Add("TaxValue");
            dt.Columns.Add("NetDebit");
            dt.Columns.Add("NetCredit");
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "","", 0, 0, 0, 0, 0,0 });
            string returnId = AddEditVoucher(id, "Cash Payment", DateTime.Now, 0, 0,0, "", "","", "", 0, "", "Active",0, 0, DateTime.Now, DateTime.Now, 0, 0, 0, dt, "Delete");
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Voucher Successfully Deleted";
            }
            else if (returnId == "-9999")
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this voucher first.";
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
                      where c.Status == "Active" && ac.Type == "Cash"
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name
                      }).OrderBy(x=>x.Name).ToList();
            return detail;
        }
        public List<clsAccountDetail> getDetailCodesForLine()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active"
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).OrderBy(x => x.Name).ToList();
            return detail;
        }
        public List<clsVoucher> getTaxes()
        {
            List<clsVoucher> tax = new List<clsVoucher>();
            tax = (from c in db.Admin_Setup_Tax
                   where c.Status == "Active"
                   select new clsVoucher
                      {
                          TaxID = c.TaxID,
                          TaxName = c.TaxName
                      }).ToList();
            return tax;
        }
        public ActionResult getMaxCodeForCashPayment()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string voucherNo = "CP-" + currentYear + currentMonth;
            int count = db.AC_CashBank_Main.Where(x => x.VoucherType == "Cash Payment").Count();
            if (count > 0)
            {
                maxCode = db.AC_CashBank_Main.Where(x => x.VoucherType == "Cash Payment").OrderByDescending(x => x.VoucherNo).Select(x => x.VoucherNo).FirstOrDefault();

                maxCode = maxCode.Split('-')[1];
                maxCode = maxCode.Substring(4, 4);
                int codeInIntegar = Convert.ToInt32(maxCode);
                codeInIntegar += 1;
                if (codeInIntegar.ToString().Length == 1)
                {
                    maxCode = voucherNo + "000" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 2)
                {
                    maxCode = voucherNo + "00" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 3)
                {
                    maxCode = voucherNo + "0" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 4)
                {
                    maxCode = voucherNo + codeInIntegar.ToString();
                }
            }
            else
            {
                maxCode = voucherNo + "0001";
            }

            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getTaxRate(int id)
        {
            decimal taxRate = db.Admin_Setup_Tax.Where(x => x.TaxID == id).Select(x => x.TaxRate).FirstOrDefault() ?? 0;
            return Json(taxRate, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetSessionsForCashList(bool? cashCheckbox, decimal? cashCustomerId, bool? detailCheckbox,
                                        decimal? detailAccountId, bool? dateCheckbox, DateTime? fromDate, DateTime? toDate,
                                        bool? voucherNoCheckbox, string voucherNo, bool? referenceNoCheckbox,
                                        string referenceNo)
        {
            bool status = true;
            if (cashCheckbox == true && cashCustomerId != null)
            {
                Session["CashCustomerId"] = cashCustomerId;
            }
            else
            {
                Session["CashCustomerId"] = null;
            }
            if (detailCheckbox == true && detailAccountId != null)
            {
                Session["DetailAccountId"] = detailAccountId;
            }
            else
            {
                Session["DetailAccountId"] = null;
            }
            if (dateCheckbox == true && fromDate != null && toDate != null)
            {
                Session["FromDate"] = fromDate;
                Session["ToDate"] = toDate;
            }
            else
            {
                Session["FromDate"] = null;
                Session["ToDate"] = null;
            }
            
            //////////////////
            if (voucherNoCheckbox == true && !(string.IsNullOrEmpty(voucherNo)))
            {
                Session["VoucherNo"] = voucherNo;
            }
            else
            {
                Session["VoucherNo"] = null;
            }
            if (referenceNoCheckbox == true && !(string.IsNullOrEmpty(referenceNo)))
            {
                Session["ReferenceNo"] = referenceNo;
            }
            else
            {
                Session["ReferenceNo"] = null;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CashPaymentsList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            ViewBag.AllDetailCode = new SelectList(getAllDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllCashPayments()
        {
            if (User != null)
            {
                decimal? cashCustomerId = 0;
                decimal? detailAccountId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string voucherNo = "";
                string referenceNo = "";
                string whereCondition = " VoucherType = 'Cash Payment'";
                if (Session["CashCustomerId"] != null)
                {
                    cashCustomerId = Convert.ToDecimal(Session["CashCustomerId"]);
                    whereCondition += " and m.DetailCode = " + cashCustomerId + "";
                }
                if (Session["VoucherNo"] != null)
                {
                    voucherNo = Session["VoucherNo"].ToString();
                    whereCondition += " and m.VoucherNo like '%" + voucherNo + "%'";
                }
                if (Session["ReferenceNo"] != null)
                {
                    referenceNo = Session["ReferenceNo"].ToString();
                    whereCondition += " and m.RefrenceNo like '%" + referenceNo + "%'";
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                    whereCondition += " and m.VoucherDate >= '" + fromDate + "' and m.VoucherDate <= '" + toDate + "'";
                }
                if (Session["DetailAccountId"] != null)
                {
                    detailAccountId = Convert.ToDecimal(Session["DetailAccountId"]);
                    whereCondition += " and m.VoucherNo in (Select ad.VoucherNo from AC_CashBank_Detail ad inner join AC_Detail adl on ad.DetailCode=adl.DetailCode and adl.DetailCode = "+detailAccountId+")  ";
                }
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsVoucher> list = new List<clsVoucher>();
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "select m.VoucherNo, isnull(m.VoucherDate,'') as 'VoucherDate',isnull(m.DetailCode,0) as 'DetailCode',isnull(m.Debit,0) as 'Debit',isnull(m.Credit,0) as 'Credit',isnull(m.Remarks,'') as 'Remarks',isnull(m.RefrenceNo,'') as 'RefrenceNo',isnull(m.PaidBy,'') as 'PaidBy',isnull(m.ReceivedBy,'') as 'ReceivedBy',(case when isnull(m.DetailCode , 0) = 0 then '' else (select d.Name from AC_Detail d where d.DetailCode = m.DetailCode) end) as 'DetailName' from AC_CashBank_Main m where " + whereCondition + "   order by m.VoucherDate desc,m.VoucherNo desc ";
                cmd.Connection = con;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                DataTableReader dt = tdt.CreateDataReader();
                con.Close();
                SqlConnection.ClearPool(con);
                while (dt.Read())
                {
                    list.Add(new clsVoucher()
                    {
                        VoucherNo = dt["VoucherNo"].ToString(),
                        VoucherDate = Convert.ToDateTime(dt["VoucherDate"]),
                        RefrenceNo = dt["RefrenceNo"].ToString(),
                        Remarks = dt["Remarks"].ToString(),
                        Debit = Convert.ToDecimal(dt["Debit"]),
                        Credit = Convert.ToDecimal(dt["Credit"]),
                        DetailCode = Convert.ToDecimal(dt["DetailCode"]),
                        PaidBy = dt["PaidBy"].ToString(),
                        ReceivedBy = dt["ReceivedBy"].ToString(),
                        DetailName = dt["DetailName"].ToString(),
                    });
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if(!(string.IsNullOrEmpty(sortColumn)))
                    {
                        list = list.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = list.Count();
                var data = list.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetCashPaymentById(string id)
        {
            clsVoucher voucher = new clsVoucher();
            voucher = (from f in db.AC_CashBank_Main
                       where f.VoucherNo == id
                       select new clsVoucher
                       {
                         VoucherNo = f.VoucherNo,
                         VoucherDate = f.VoucherDate,
                         RefrenceNo = f.RefrenceNo,
                         Remarks = f.Remarks,
                         DefaultVoucher = f.DefaultVoucher,
                         Credit = f.Credit,
                         DetailCode = f.DetailCode,
                         PaidBy = f.PaidBy,
                         ReceivedBy = f.ReceivedBy,
                         DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                         Status = f.Status
                       }).FirstOrDefault();
            var data = voucher;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCashPaymentDetailById(string id)
        {
            List<clsVoucher> voucherDetail = new List<clsVoucher>();
            voucherDetail = (from f in db.AC_CashBank_Detail
                       where f.VoucherNo == id
                       select new clsVoucher
                       {
                           VoucherNo = f.VoucherNo,
                           LineRemarks = f.Remarks ?? string.Empty,
                           LineDebit = f.Debit,
                           LineCredit = f.Credit,
                           LineDetailCode = f.DetailCode,
                           ChequeNo = f.ChequeNo ?? string.Empty,
                           ChequeComputerNo = f.ChequeComputerNo,
                           DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == f.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                           TaxID = f.TaxId,
                           TaxName = db.Admin_Setup_Tax.Where(x=>x.TaxID == f.TaxId).Select(x=>x.TaxName).FirstOrDefault() ?? string.Empty,
                           TaxRate = f.TaxRate,
                           TaxValue = f.TaxValue,
                           NetDebit = f.NetDebit,
                           NetCredit = f.NetCredit,
                           SerialNo = f.SerialNo
                       }).OrderBy(x => x.SerialNo).ToList();
            var data = voucherDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public List<clsAccountDetail> getAllDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active"
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).ToList();
            return detail;
        }
        #endregion
        #region Cash Payment Report
        public ActionResult CashPaymentReport(string id)
        {
            if (User != null)
            {
                List<clsVoucher> voucher = new List<clsVoucher>();
                voucher = (from f in db.AC_CashBank_Main
                           join fd in db.AC_CashBank_Detail on f.VoucherNo equals fd.VoucherNo
                           where f.VoucherNo == id
                           select new clsVoucher
                           {
                               DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault()?? string.Empty,
                               VoucherNo = f.VoucherNo,
                               VoucherDate = f.VoucherDate,
                               RefrenceNo = f.RefrenceNo?? string.Empty,
                               PaidBy = f.PaidBy?? string.Empty,
                               ReceivedBy = f.ReceivedBy?? string.Empty,
                               Remarks = f.Remarks ?? string.Empty,
                               Debit = f.Debit,
                               Credit = f.Credit,
                               //Line Data
                               SerialNo = fd.SerialNo,
                               LineDetailName = db.AC_Detail.Where(x => x.DetailCode == fd.DetailCode).Select(x => x.Name).FirstOrDefault()?? string.Empty,
                               LineChequeNo = fd.ChequeNo,
                               LineDebit = fd.Debit,
                               LineCredit = fd.Credit,
                               LineRemarks = fd.Remarks ?? string.Empty,
                           }).OrderBy(x=>x.SerialNo).ToList();
                voucher[0].AmountInString = NumberToWords(Convert.ToInt32(voucher[0].Credit));
                return View(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
        #endregion
        #region Cash Receipt
        // GET: /Cash/
        public ActionResult CashReceiptIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CashReceipt"
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
                    ViewBag.DetailCodeLine = new SelectList(getDetailCodesForLine(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateReceiptVoucher(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsVoucher> voucher = JsonConvert.DeserializeObject<List<clsVoucher>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("DetailCode");
            dt.Columns.Add("Debit");
            dt.Columns.Add("Credit");
            dt.Columns.Add("Remarks");
            dt.Columns.Add("ChequeNo");
            dt.Columns.Add("ChequeComputerNo");
            dt.Columns.Add("TaxId");
            dt.Columns.Add("TaxRate");
            dt.Columns.Add("TaxValue");
            dt.Columns.Add("NetDebit");
            dt.Columns.Add("NetCredit");
            if (voucher[0].Debit == null)
            {
                voucher[0].Debit = 0;
            }
            if (voucher[0].Credit == null)
            {
                voucher[0].Credit = 0;
            }
            if (voucher[0].ChequeNo == null)
            {
                voucher[0].ChequeNo = "";
            }
            if (voucher[0].ChequeComputerNo == null)
            {
                voucher[0].ChequeComputerNo = 0;
            }
            string voucherNo = voucher[0].VoucherNo;
            if (voucher[0].IsNewOrEdit == "Edit")
            {
                for (int i = 0; i < voucher.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, voucher[i].LineDetailCode, voucher[i].LineDebit, voucher[i].LineCredit, voucher[i].LineRemarks, voucher[i].LineChequeNo, voucher[i].LineChequeComputerNo, voucher[i].TaxID, voucher[i].TaxRate, voucher[i].TaxValue, voucher[i].NetDebit, voucher[i].NetCredit });
                }
                returnId = AddEditVoucher(voucher[0].VoucherNo, "Cash Receipt", voucher[0].VoucherDate, voucher[0].DetailCode, voucher[0].Debit, voucher[0].Credit, voucher[0].RefrenceNo, voucher[0].Remarks, voucher[0].PaidBy, voucher[0].ReceivedBy, voucher[0].ChequeComputerNo, voucher[0].ChequeNo, "Unpost", User.UserId, User.UserId, DateTime.Now, DateTime.Now, voucher[0].DefaultVoucher, 0, User.FiscalYearId, dt, "Update");
                if (returnId == voucherNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Voucher Successfully Updated";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(voucherNo);
                    }
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
                for (int i = 0; i < voucher.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, voucher[i].LineDetailCode, voucher[i].LineDebit, voucher[i].LineCredit, voucher[i].LineRemarks, voucher[i].LineChequeNo, voucher[i].LineChequeComputerNo, voucher[i].TaxID, voucher[i].TaxRate, voucher[i].TaxValue, voucher[i].NetDebit, voucher[i].NetCredit });
                }
                returnId = AddEditVoucher(voucher[0].VoucherNo, "Cash Receipt", voucher[0].VoucherDate, voucher[0].DetailCode, voucher[0].Debit, voucher[0].Credit, voucher[0].RefrenceNo, voucher[0].Remarks, voucher[0].PaidBy, voucher[0].ReceivedBy, voucher[0].ChequeComputerNo, voucher[0].ChequeNo, "Unpost", User.UserId, User.UserId, DateTime.Now, DateTime.Now, voucher[0].DefaultVoucher, 0, User.FiscalYearId, dt, "Save");
                voucherNo = returnId;
                if (returnId != "")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Voucher Successfully Saved";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(voucherNo);
                    }
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, voucherNo = voucherNo } };
        }
        public ActionResult DeleteCashReceipt(string id)
        {
            string message = "";
            bool status = false;
            DataTable dt = new DataTable();
            List<clsVoucher> voucher = new List<clsVoucher>();
            dt.Columns.Add("Id");
            dt.Columns.Add("DetailCode");
            dt.Columns.Add("Debit");
            dt.Columns.Add("Credit");
            dt.Columns.Add("Remarks");
            dt.Columns.Add("ChequeNo");
            dt.Columns.Add("ChequeComputerNo");
            dt.Columns.Add("TaxId");
            dt.Columns.Add("TaxRate");
            dt.Columns.Add("TaxValue");
            dt.Columns.Add("NetDebit");
            dt.Columns.Add("NetCredit");
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", "", 0, 0, 0, 0, 0, 0 });
            string returnId = AddEditVoucher(id, "Cash Receipt", DateTime.Now, 0, 0, 0, "", "", "", "", 0, "", "Active", 0, 0, DateTime.Now, DateTime.Now, 0, 0, 0, dt, "Delete");
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Voucher Successfully Deleted";
            }
            else if (returnId == "-9999")
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this voucher first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult getMaxCodeForCashReceipt()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string voucherNo = "CR-" + currentYear + currentMonth;
            int count = db.AC_CashBank_Main.Where(x => x.VoucherType == "Cash Receipt").Count();
            if (count > 0)
            {
                maxCode = db.AC_CashBank_Main.Where(x => x.VoucherType == "Cash Receipt").OrderByDescending(x => x.VoucherNo).Select(x => x.VoucherNo).FirstOrDefault();

                maxCode = maxCode.Split('-')[1];
                maxCode = maxCode.Substring(4, 4);
                int codeInIntegar = Convert.ToInt32(maxCode);
                codeInIntegar += 1;
                if (codeInIntegar.ToString().Length == 1)
                {
                    maxCode = voucherNo + "000" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 2)
                {
                    maxCode = voucherNo + "00" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 3)
                {
                    maxCode = voucherNo + "0" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 4)
                {
                    maxCode = voucherNo + codeInIntegar.ToString();
                }
            }
            else
            {
                maxCode = voucherNo + "0001";
            }

            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CashReceiptsList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            ViewBag.AllDetailCode = new SelectList(getAllDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllCashReceipts()
        {
            if (User != null)
            {
                string type = "";
                decimal? cashCustomerId = 0;
                decimal? detailAccountId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string voucherNo = "";
                string referenceNo = "";
                string whereCondition = " VoucherType = 'Cash Receipt'";
                if (Session["CashCustomerId"] != null)
                {
                    cashCustomerId = Convert.ToDecimal(Session["CashCustomerId"]);
                    whereCondition += " and m.DetailCode = " + cashCustomerId + "";
                }
                if (Session["VoucherNo"] != null)
                {
                    voucherNo = Session["VoucherNo"].ToString();
                    whereCondition += " and m.VoucherNo like '%" + voucherNo + "%'";
                }
                if (Session["ReferenceNo"] != null)
                {
                    referenceNo = Session["ReferenceNo"].ToString();
                    whereCondition += " and m.RefrenceNo like '%" + referenceNo + "%'";
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                    whereCondition += " and m.VoucherDate >= '" + fromDate + "' and m.VoucherDate <= '" + toDate + "'";
                }
                if (Session["DetailAccountId"] != null)
                {
                    detailAccountId = Convert.ToDecimal(Session["DetailAccountId"]);
                    whereCondition += " and m.VoucherNo in (Select ad.VoucherNo from AC_CashBank_Detail ad inner join AC_Detail adl on ad.DetailCode=adl.DetailCode and adl.DetailCode = " + detailAccountId + ")  ";
                }
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsVoucher> list = new List<clsVoucher>();
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "select m.VoucherNo, isnull(m.VoucherDate,'') as 'VoucherDate',isnull(m.DetailCode,0) as 'DetailCode',isnull(m.Debit,0) as 'Debit',isnull(m.Credit,0) as 'Credit',isnull(m.Remarks,'') as 'Remarks',isnull(m.RefrenceNo,'') as 'RefrenceNo',isnull(m.PaidBy,'') as 'PaidBy',isnull(m.ReceivedBy,'') as 'ReceivedBy',(case when isnull(m.DetailCode , 0) = 0 then '' else (select d.Name from AC_Detail d where d.DetailCode = m.DetailCode) end) as 'DetailName' from AC_CashBank_Main m where " + whereCondition + "   order by m.VoucherDate desc,m.VoucherNo desc ";
                cmd.Connection = con;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                DataTableReader dt = tdt.CreateDataReader();
                con.Close();
                SqlConnection.ClearPool(con);
                while (dt.Read())
                {
                    list.Add(new clsVoucher()
                    {
                        VoucherNo = dt["VoucherNo"].ToString(),
                        VoucherDate = Convert.ToDateTime(dt["VoucherDate"]),
                        RefrenceNo = dt["RefrenceNo"].ToString(),
                        Remarks = dt["Remarks"].ToString(),
                        Debit = Convert.ToDecimal(dt["Debit"]),
                        Credit = Convert.ToDecimal(dt["Credit"]),
                        DetailCode = Convert.ToDecimal(dt["DetailCode"]),
                        PaidBy = dt["PaidBy"].ToString(),
                        ReceivedBy = dt["ReceivedBy"].ToString(),
                        DetailName = dt["DetailName"].ToString(),
                    });
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        list = list.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = list.Count();
                var data = list.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetCashReceiptById(string id)
        {
            clsVoucher voucher = new clsVoucher();
            voucher = (from f in db.AC_CashBank_Main
                       where f.VoucherNo == id
                       select new clsVoucher
                       {
                           VoucherNo = f.VoucherNo,
                           VoucherDate = f.VoucherDate,
                           RefrenceNo = f.RefrenceNo,
                           Remarks = f.Remarks,
                           DefaultVoucher = f.DefaultVoucher,
                           Debit = f.Debit,
                           Credit = f.Credit,
                           DetailCode = f.DetailCode,
                           PaidBy = f.PaidBy,
                           ReceivedBy = f.ReceivedBy,
                           DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                           Status = f.Status
                       }).FirstOrDefault();
            var data = voucher;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCashReceiptDetailById(string id)
        {
            List<clsVoucher> voucherDetail = new List<clsVoucher>();
            voucherDetail = (from f in db.AC_CashBank_Detail
                             where f.VoucherNo == id
                             select new clsVoucher
                             {
                                 VoucherNo = f.VoucherNo,
                                 LineRemarks = f.Remarks ?? string.Empty,
                                 LineDebit = f.Debit,
                                 LineCredit = f.Credit,
                                 LineDetailCode = f.DetailCode,
                                 ChequeNo = f.ChequeNo ?? string.Empty,
                                 ChequeComputerNo = f.ChequeComputerNo,
                                 DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == f.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                                 TaxID = f.TaxId,
                                 TaxName = db.Admin_Setup_Tax.Where(x => x.TaxID == f.TaxId).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 TaxRate = f.TaxRate,
                                 TaxValue = f.TaxValue,
                                 NetDebit = f.NetDebit,
                                 NetCredit = f.NetCredit,
                                 SerialNo = f.SerialNo
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = voucherDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Cash Receipt Report
        public ActionResult CashReceiptReport(string id)
        {
            if (User != null)
            {
                List<clsVoucher> voucher = new List<clsVoucher>();
                voucher = (from f in db.AC_CashBank_Main
                           join fd in db.AC_CashBank_Detail on f.VoucherNo equals fd.VoucherNo
                           where f.VoucherNo == id
                           select new clsVoucher
                           {
                               DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                               VoucherNo = f.VoucherNo,
                               VoucherDate = f.VoucherDate,
                               RefrenceNo = f.RefrenceNo ?? string.Empty,
                               PaidBy = f.PaidBy ?? string.Empty,
                               ReceivedBy = f.ReceivedBy ?? string.Empty,
                               Remarks = f.Remarks ?? string.Empty,
                               Debit = f.Debit,
                               Credit = f.Credit,
                               //Line Data
                               SerialNo = fd.SerialNo,
                               LineDetailName = db.AC_Detail.Where(x => x.DetailCode == fd.DetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                               LineChequeNo = fd.ChequeNo,
                               LineDebit = fd.Debit,
                               LineCredit = fd.Credit,
                               LineRemarks = fd.Remarks ?? string.Empty,
                           }).OrderBy(x => x.SerialNo).ToList();
                voucher[0].AmountInString = NumberToWords(Convert.ToInt32(voucher[0].Debit));
                return View(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion


        #region Debit Credit Note
        // GET: /Cash/
        public ActionResult DebitCreditNoteIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CreditBreakage"
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
                    ViewBag.DetailCode = new SelectList(getCustomerAndSuppliers(), "DetailCode", "Name");
                    ViewBag.DetailCodeLine = new SelectList(getDetailCodesForLine(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateDebitCreditNote(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsVoucher> voucher = JsonConvert.DeserializeObject<List<clsVoucher>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("DetailCode");
            dt.Columns.Add("Debit");
            dt.Columns.Add("Credit");
            dt.Columns.Add("Remarks");
            dt.Columns.Add("ChequeNo");
            dt.Columns.Add("ChequeComputerNo");
            dt.Columns.Add("TaxId");
            dt.Columns.Add("TaxRate");
            dt.Columns.Add("TaxValue");
            dt.Columns.Add("NetDebit");
            dt.Columns.Add("NetCredit");
            if (voucher[0].Debit == null)
            {
                voucher[0].Debit = 0;
            }
            if (voucher[0].Credit == null)
            {
                voucher[0].Credit = 0;
            }
            if (voucher[0].ChequeNo == null)
            {
                voucher[0].ChequeNo = "";
            }
            if (voucher[0].ChequeComputerNo == null)
            {
                voucher[0].ChequeComputerNo = 0;
            }
            string voucherNo = voucher[0].VoucherNo;
            string type = "";
            
            type = "Credit Breakage";
            
                for (int i = 0; i < voucher.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, voucher[i].LineDetailCode, voucher[i].LineDebit, voucher[i].LineCredit, voucher[i].LineRemarks, voucher[i].LineChequeNo, voucher[i].LineChequeComputerNo, voucher[i].TaxID, voucher[i].TaxRate, voucher[i].TaxValue, voucher[i].NetDebit, voucher[i].NetCredit });
                }
            
            if (voucher[0].IsNewOrEdit == "Edit")
            {
                returnId = AddEditVoucher(voucher[0].VoucherNo, type, voucher[0].VoucherDate, voucher[0].DetailCode, voucher[0].Debit, voucher[0].Credit, voucher[0].RefrenceNo, voucher[0].Remarks, voucher[0].PaidBy, voucher[0].ReceivedBy, voucher[0].ChequeComputerNo, voucher[0].ChequeNo, "Unpost", User.UserId, User.UserId, DateTime.Now, DateTime.Now, voucher[0].DefaultVoucher, 0, User.FiscalYearId, dt, "Update");
                if (returnId == voucherNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Voucher Successfully Updated";
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
                returnId = AddEditVoucher(voucher[0].VoucherNo, type, voucher[0].VoucherDate, voucher[0].DetailCode, voucher[0].Debit, voucher[0].Credit, voucher[0].RefrenceNo, voucher[0].Remarks, voucher[0].PaidBy, voucher[0].ReceivedBy, voucher[0].ChequeComputerNo, voucher[0].ChequeNo, "Unpost", User.UserId, User.UserId, DateTime.Now, DateTime.Now, voucher[0].DefaultVoucher,0, User.FiscalYearId, dt, "Save");
                voucherNo = returnId;
                if (returnId != "")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Voucher Successfully Saved";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, voucherNo = voucherNo } };
        }
        public ActionResult DeleteDebitCreditNote(string id)
        {
            string message = "";
            bool status = false;
            DataTable dt = new DataTable();
            List<clsVoucher> voucher = new List<clsVoucher>();
            dt.Columns.Add("Id");
            dt.Columns.Add("DetailCode");
            dt.Columns.Add("Debit");
            dt.Columns.Add("Credit");
            dt.Columns.Add("Remarks");
            dt.Columns.Add("ChequeNo");
            dt.Columns.Add("ChequeComputerNo");
            dt.Columns.Add("TaxId");
            dt.Columns.Add("TaxRate");
            dt.Columns.Add("TaxValue");
            dt.Columns.Add("NetDebit");
            dt.Columns.Add("NetCredit");
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", "", 0, 0, 0, 0, 0, 0 });
            string returnId = AddEditVoucher(id, "", DateTime.Now, 0, 0, 0, "", "", "", "", 0, "", "Active", 0, 0, DateTime.Now, DateTime.Now, 0, 0, 0, dt, "Delete");
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Voucher Successfully Deleted";
            }
            else if (returnId == "-9999")
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this voucher first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public List<clsAccountDetail> getCustomerAndSuppliers()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && (c.Type == "Customer" || c.Type == "Supplier" || c.Type == "Customer + Supplier")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).OrderBy(x => x.Name).ToList();
            return detail;
        }
        public ActionResult getMaxCodeForDebitCreditNote()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string voucherNo = "";
            voucherNo = "CB-" + currentYear + currentMonth;
            string type = "Credit Breakage";
            
            int count = db.AC_CashBank_Main.Where(x => x.VoucherType == type).Count();
            if (count > 0)
            {
                maxCode = db.AC_CashBank_Main.Where(x => x.VoucherType == type).OrderByDescending(x => x.VoucherNo).Select(x => x.VoucherNo).FirstOrDefault();

                maxCode = maxCode.Split('-')[1];
                maxCode = maxCode.Substring(4, 4);
                int codeInIntegar = Convert.ToInt32(maxCode);
                codeInIntegar += 1;
                if (codeInIntegar.ToString().Length == 1)
                {
                    maxCode = voucherNo + "000" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 2)
                {
                    maxCode = voucherNo + "00" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 3)
                {
                    maxCode = voucherNo + "0" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 4)
                {
                    maxCode = voucherNo + codeInIntegar.ToString();
                }
            }
            else
            {
                maxCode = voucherNo + "0001";
            }
            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DebitCreditNotesList()
        {
            ViewBag.DetailCode = new SelectList(getCustomerAndSuppliers(), "DetailCode", "Name");
            ViewBag.AllDetailCode = new SelectList(getAllDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllDebitCreditNotes()
        {
            if (User != null)
            {
                decimal? cashCustomerId = 0;
                decimal? detailAccountId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string voucherNo = "";
                string referenceNo = "";
                string whereCondition = " (VoucherType = 'Credit Breakage')";
                if (Session["CashCustomerId"] != null)
                {
                    cashCustomerId = Convert.ToDecimal(Session["CashCustomerId"]);
                    whereCondition += " and m.DetailCode = " + cashCustomerId + "";
                }
                if (Session["VoucherNo"] != null)
                {
                    voucherNo = Session["VoucherNo"].ToString();
                    whereCondition += " and m.VoucherNo like '%" + voucherNo + "%'";
                }
                if (Session["ReferenceNo"] != null)
                {
                    referenceNo = Session["ReferenceNo"].ToString();
                    whereCondition += " and m.RefrenceNo like '%" + referenceNo + "%'";
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                    whereCondition += " and m.VoucherDate >= '" + fromDate + "' and m.VoucherDate <= '" + toDate + "'";
                }
                if (Session["DetailAccountId"] != null)
                {
                    detailAccountId = Convert.ToDecimal(Session["DetailAccountId"]);
                    whereCondition += " and m.VoucherNo in (Select ad.VoucherNo from AC_CashBank_Detail ad inner join AC_Detail adl on ad.DetailCode=adl.DetailCode and adl.DetailCode = " + detailAccountId + ")  ";
                }
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsVoucher> list = new List<clsVoucher>();
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "select m.VoucherNo, isnull(m.VoucherDate,'') as 'VoucherDate',isnull(m.DetailCode,0) as 'DetailCode',isnull(m.Debit,0) as 'Debit',isnull(m.Credit,0) as 'Credit',isnull(m.Remarks,'') as 'Remarks',isnull(m.RefrenceNo,'') as 'RefrenceNo',isnull(m.PaidBy,'') as 'PaidBy',isnull(m.ReceivedBy,'') as 'ReceivedBy',(case when isnull(m.DetailCode , 0) = 0 then '' else (select d.Name from AC_Detail d where d.DetailCode = m.DetailCode) end) as 'DetailName' from AC_CashBank_Main m where " + whereCondition + "   order by m.VoucherDate desc,m.VoucherNo desc ";
                cmd.Connection = con;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                DataTableReader dt = tdt.CreateDataReader();
                con.Close();
                SqlConnection.ClearPool(con);
                while (dt.Read())
                {
                    list.Add(new clsVoucher()
                    {
                        VoucherNo = dt["VoucherNo"].ToString(),
                        VoucherDate = Convert.ToDateTime(dt["VoucherDate"]),
                        RefrenceNo = dt["RefrenceNo"].ToString(),
                        Remarks = dt["Remarks"].ToString(),
                        Debit = Convert.ToDecimal(dt["Debit"]),
                        Credit = Convert.ToDecimal(dt["Credit"]),
                        DetailCode = Convert.ToDecimal(dt["DetailCode"]),
                        PaidBy = dt["PaidBy"].ToString(),
                        ReceivedBy = dt["ReceivedBy"].ToString(),
                        DetailName = dt["DetailName"].ToString()
                    });
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        list = list.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = list.Count();
                var data = list.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetDebitCreditNoteById(string id)
        {
            clsVoucher voucher = new clsVoucher();
            voucher = (from f in db.AC_CashBank_Main
                       where f.VoucherNo == id
                       select new clsVoucher
                       {
                           VoucherNo = f.VoucherNo,
                           VoucherDate = f.VoucherDate,
                           RefrenceNo = f.RefrenceNo,
                           Remarks = f.Remarks,
                           DefaultVoucher = f.DefaultVoucher,
                           Credit = f.Credit,
                           DetailCode = f.DetailCode,
                           PaidBy = f.PaidBy,
                           ReceivedBy = f.ReceivedBy,
                           LineDetailCode = db.AC_CashBank_Detail.Where(x => x.VoucherNo == f.VoucherNo).Select(x => x.DetailCode).FirstOrDefault() ?? 0,
                           DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                           Status = f.Status
                       }).FirstOrDefault();
            var data = voucher;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDebitCreditNoteDetailById(string id)
        {
            List<clsVoucher> voucherDetail = new List<clsVoucher>();
            voucherDetail = (from f in db.AC_CashBank_Detail
                             where f.VoucherNo == id
                             select new clsVoucher
                             {
                                 VoucherNo = f.VoucherNo,
                                 LineRemarks = f.Remarks ?? string.Empty,
                                 LineDebit = f.Debit,
                                 LineCredit = f.Credit,
                                 LineDetailCode = f.DetailCode,
                                 ChequeNo = f.ChequeNo ?? string.Empty,
                                 ChequeComputerNo = f.ChequeComputerNo,
                                 DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                 TaxID = f.TaxId,
                                 TaxName = db.Admin_Setup_Tax.Where(x => x.TaxID == f.TaxId).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 TaxRate = f.TaxRate,
                                 TaxValue = f.TaxValue,
                                 NetDebit = f.NetDebit,
                                 NetCredit = f.NetCredit
                             }).ToList();
            var data = voucherDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Debit Credit Note Report
        public ActionResult DebitCreditNoteReport(string id)
        {
            if (User != null)
            {
                List<clsVoucher> voucher = new List<clsVoucher>();
                voucher = (from f in db.AC_CashBank_Main
                           join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                           where f.VoucherNo == id
                           select new clsVoucher
                           {
                               //CityName
                               DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                               CityName = (from c in db.AC_Detail_Extra join cc in db.Cities on c.CityId equals cc.CityId where c.DetailCode == f.DetailCode select cc.CityName).FirstOrDefault() ?? string.Empty,
                               VoucherNo = f.VoucherNo,
                               VoucherDate = f.VoucherDate,
                               RefrenceNo = f.RefrenceNo ?? string.Empty,
                               PaidBy = f.PaidBy ?? string.Empty,
                               ReceivedBy = f.ReceivedBy ?? string.Empty,
                               Remarks = f.Remarks ?? string.Empty,
                               Debit = f.Debit,
                               Credit = f.Credit,
                               LineDetailCode = fd.DetailCode ?? 0,
                               SerialNo = fd.SerialNo ?? 0,
                               //LineDetailCode =db.AC_CashBank_Detail.Where(x=>x.VoucherNo == f.VoucherNo).Select(x=>x.DetailCode).FirstOrDefault() ?? 0,
                               //LineDetailName = (from a in db.AC_CashBank_Detail join d in db.AC_Detail on a.DetailCode equals d.DetailCode where a.VoucherNo == f.VoucherNo select d.Name).FirstOrDefault() ?? "",
                               LineDetailName = db.AC_Detail.Where(x => x.DetailCode == fd.DetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                               //LineChequeNo = fd.ChequeNo,
                               LineDebit = fd.Debit,
                               LineCredit = fd.Credit,
                               LineRemarks = fd.Remarks ?? string.Empty,
                           }).OrderBy(x => x.SerialNo).ToList();
                voucher[0].AmountInString = NumberToWords(Convert.ToInt32(voucher[0].Credit));
                return View(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion

        #region Daybook
        public ActionResult VouchersDaybook()
        {
            ViewBag.VoucherType = new SelectList(getVoucherTypes(), "VoucherType", "VoucherType");
            return View();
        }
        [HttpPost]
        public ActionResult PrintVouchersDaybookReport()
        {
            if (User != null)
            {
                String[] type = Request.Form.GetValues("VoucherType");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);

                clsVoucher voucher = new clsVoucher();
                List<clsVoucher> vouchersList = new List<clsVoucher>();

                if (type != null)
                {
                    for (int i = 0; i < type.Length; i++)
                    {
                        string voucherType = type[i].ToString();
                        List<clsVoucher> vouchers = GetVouchersList(voucherType, fromDate, toDate);
                        vouchersList.AddRange(vouchers);
                    }
                }
                else
                {
                    List<clsVoucher> types = getVoucherTypes();
                    for (int i = 0; i < types.Count; i++)
                    {
                        string voucherType = types[i].VoucherType.ToString();
                        List<clsVoucher> list = GetVouchersList(voucherType, fromDate, toDate);
                        vouchersList.AddRange(list);
                    }
                }
                vouchersList = vouchersList.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ThenBy(x => x.SerialNo).ToList();
                voucher.VouchersList = vouchersList;
                voucher.VouchersList2 = vouchersList.GroupBy(x => x.VoucherNo).Select(g => g.First()).ToList();
                return View(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetVouchersList(string voucherType, DateTime fromDate, DateTime toDate)
        {
            List<clsVoucher> vouchersList = new List<clsVoucher>();
            vouchersList = (from f in db.AC_Voucher_Main
                            join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                            where f.VoucherDate >= fromDate
                             && f.VoucherDate <= toDate
                              && f.VoucherType == voucherType
                            select new clsVoucher
                            {
                                VoucherNo = f.VoucherNo,
                                VoucherDate = f.VoucherDate,
                                FromDate = fromDate,
                                ToDate = toDate,
                                RefrenceNo = f.RefrenceNo ?? string.Empty,
                                //Line Data
                                SerialNo = fd.SerialNo,
                                LineDetailName = db.AC_Detail.Where(x => x.DetailCode == fd.DetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == fd.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                                LineChequeNo = fd.ChequeNo,
                                LineDebit = fd.Debit,
                                LineCredit = fd.Credit,
                                LineRemarks = fd.Remarks ?? string.Empty,
                            }).OrderBy(x => x.SerialNo).ToList();
            return vouchersList;
        }
        public List<clsVoucher> getVoucherTypes()
        {
            List<clsVoucher> type = new List<clsVoucher>();
            type = (from c in db.AC_Voucher_Main
                    select new clsVoucher
                    {
                        VoucherType = c.VoucherType,
                    }).Distinct().ToList();
            return type;
        }
        #endregion
    }
}