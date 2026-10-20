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
    public class JournalVoucherController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Journal Voucher
        public ActionResult JournalVoucherIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "JournalVoucher"
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
        public ActionResult InsertUpdateVoucher(string data)
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
            
            voucher[0].DetailCode = 0;
            voucher[0].PaidBy = "";
            voucher[0].ReceivedBy = "";
            voucher[0].DefaultVoucher = 1;
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
                returnId = AddEditVoucher(voucher[0].VoucherNo, "Journal Voucher", voucher[0].VoucherDate, voucher[0].DetailCode, voucher[0].Debit, voucher[0].Credit, voucher[0].RefrenceNo, voucher[0].Remarks, voucher[0].PaidBy, voucher[0].ReceivedBy, voucher[0].ChequeComputerNo, voucher[0].ChequeNo, "Unpost", User.UserId, User.UserId, DateTime.Now, DateTime.Now, voucher[0].DefaultVoucher, 0, User.FiscalYearId, dt, "Update");
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
                for (int i = 0; i < voucher.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, voucher[i].LineDetailCode, voucher[i].LineDebit, voucher[i].LineCredit, voucher[i].LineRemarks, voucher[i].LineChequeNo, voucher[i].LineChequeComputerNo, voucher[i].TaxID, voucher[i].TaxRate, voucher[i].TaxValue, voucher[i].NetDebit, voucher[i].NetCredit });
                }
                returnId = AddEditVoucher(voucher[0].VoucherNo, "Journal Voucher", voucher[0].VoucherDate, voucher[0].DetailCode, voucher[0].Debit, voucher[0].Credit, voucher[0].RefrenceNo, voucher[0].Remarks, voucher[0].PaidBy, voucher[0].ReceivedBy, voucher[0].ChequeComputerNo, voucher[0].ChequeNo, "Unpost", User.UserId, User.UserId, DateTime.Now, DateTime.Now, voucher[0].DefaultVoucher, 0, User.FiscalYearId, dt, "Save");
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
        public ActionResult DeleteJournalVoucher(string id)
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
            string returnId = AddEditVoucher(id, "Journal Voucher", DateTime.Now, 0, 0, 0, "", "", "", "", 0, "", "Unpost", 0, 0, DateTime.Now, DateTime.Now, 0, 0, 0, dt, "Delete");
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
        public ActionResult getMaxCodeForJournalVoucher()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string voucherNo = "JV-" + currentYear + currentMonth;
            int count = db.AC_CashBank_Main.Where(x => x.VoucherType == "Journal Voucher").Count();
            if (count > 0)
            {
                maxCode = db.AC_CashBank_Main.Where(x => x.VoucherType == "Journal Voucher").OrderByDescending(x => x.VoucherNo).Select(x => x.VoucherNo).FirstOrDefault();

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
        public ActionResult SetSessionsForCashList(bool? detailCheckbox,
                                        decimal? detailAccountId, bool? dateCheckbox, DateTime? fromDate, DateTime? toDate,
                                        bool? voucherNoCheckbox, string voucherNo, bool? referenceNoCheckbox,
                                        string referenceNo)
        {
            bool status = true;
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
        public ActionResult JournalVouchersList()
        {
            ViewBag.AllDetailCode = new SelectList(getAllDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllJournalVouchers()
        {
            if (User != null)
            {
                decimal? detailAccountId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string voucherNo = "";
                string referenceNo = "";
                string whereCondition = " VoucherType = 'Journal Voucher'";
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
        public ActionResult GetJournalVoucherById(string id)
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
        public ActionResult GetJournalVoucherDetailById(string id)
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
        #region Journal Voucher Report
        public ActionResult JournalVoucherReport(string id)
        {
            if (User != null)
            {
                List<clsVoucher> voucher = new List<clsVoucher>();
                voucher = (from f in db.AC_CashBank_Main
                           join fd in db.AC_CashBank_Detail on f.VoucherNo equals fd.VoucherNo
                           where f.VoucherNo == id
                           select new clsVoucher
                           {
                               VoucherNo = f.VoucherNo,
                               VoucherDate = f.VoucherDate,
                               RefrenceNo = f.RefrenceNo ?? string.Empty,
                               Remarks = f.Remarks ?? string.Empty,
                               //Line Data
                               SerialNo = fd.SerialNo,
                               LineDetailName = db.AC_Detail.Where(x => x.DetailCode == fd.DetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                               LineChequeNo = fd.ChequeNo,
                               LineDebit = fd.Debit,
                               LineCredit = fd.Credit,
                               LineRemarks = fd.Remarks ?? string.Empty,
                           }).OrderBy(x => x.SerialNo).ToList();
                return View(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        #region Ledger Voucher
        public ActionResult LedgerVoucherIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "LedgerVoucher"
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
        public ActionResult InsertUpdateLedgerVoucher(string data)
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

            voucher[0].DetailCode = 0;
            voucher[0].PaidBy = "";
            voucher[0].ReceivedBy = "";
            voucher[0].DefaultVoucher = 1;
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
                returnId = AddEditVoucher(voucher[0].VoucherNo, "Ledger", voucher[0].VoucherDate, voucher[0].DetailCode, voucher[0].Debit, voucher[0].Credit, voucher[0].RefrenceNo, voucher[0].Remarks, voucher[0].PaidBy, voucher[0].ReceivedBy, voucher[0].ChequeComputerNo, voucher[0].ChequeNo, "Unpost", User.UserId, User.UserId, DateTime.Now, DateTime.Now, voucher[0].DefaultVoucher, 0, User.FiscalYearId, dt, "Update");
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
                for (int i = 0; i < voucher.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, voucher[i].LineDetailCode, voucher[i].LineDebit, voucher[i].LineCredit, voucher[i].LineRemarks, voucher[i].LineChequeNo, voucher[i].LineChequeComputerNo, voucher[i].TaxID, voucher[i].TaxRate, voucher[i].TaxValue, voucher[i].NetDebit, voucher[i].NetCredit });
                }
                returnId = AddEditVoucher(voucher[0].VoucherNo, "Ledger", voucher[0].VoucherDate, voucher[0].DetailCode, voucher[0].Debit, voucher[0].Credit, voucher[0].RefrenceNo, voucher[0].Remarks, voucher[0].PaidBy, voucher[0].ReceivedBy, voucher[0].ChequeComputerNo, voucher[0].ChequeNo, "Unpost", User.UserId, User.UserId, DateTime.Now, DateTime.Now, voucher[0].DefaultVoucher, 0, User.FiscalYearId, dt, "Save");
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
       
        public ActionResult DeleteLedgerVoucher(string id)
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
            string returnId = AddEditVoucher(id, "Ledger", DateTime.Now, 0, 0, 0, "", "", "", "", 0, "", "Unpost", 0, 0, DateTime.Now, DateTime.Now, 0, 0, 0, dt, "Delete");
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
        public ActionResult getMaxCodeForLedgerVoucher()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string voucherNo = "LG-" + currentYear + currentMonth;
            int count = db.AC_CashBank_Main.Where(x => x.VoucherType == "Ledger").Count();
            if (count > 0)
            {
                maxCode = db.AC_CashBank_Main.Where(x => x.VoucherType == "Ledger").OrderByDescending(x => x.VoucherNo).Select(x => x.VoucherNo).FirstOrDefault();

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

        public ActionResult LedgerVouchersList()
        {
            ViewBag.AllDetailCode = new SelectList(getAllDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllLedgerVouchers()
        {
            if (User != null)
            {
                decimal? detailAccountId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string voucherNo = "";
                string referenceNo = "";
                string whereCondition = " VoucherType = 'Ledger'";
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
        public ActionResult GetLedgerVoucherById(string id)
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
        public ActionResult GetLedgerVoucherDetailById(string id)
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
        #region Ledger Voucher Report
        public ActionResult LedgerVoucherReport(string id)
        {
            if (User != null)
            {
                List<clsVoucher> voucher = new List<clsVoucher>();
                voucher = (from f in db.AC_CashBank_Main
                           join fd in db.AC_CashBank_Detail on f.VoucherNo equals fd.VoucherNo
                           where f.VoucherNo == id
                           select new clsVoucher
                           {
                               VoucherNo = f.VoucherNo,
                               VoucherDate = f.VoucherDate,
                               RefrenceNo = f.RefrenceNo ?? string.Empty,
                               Remarks = f.Remarks ?? string.Empty,
                               //Line Data
                               SerialNo = fd.SerialNo,
                               LineDetailName = db.AC_Detail.Where(x => x.DetailCode == fd.DetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                               LineChequeNo = fd.ChequeNo,
                               LineDebit = fd.Debit,
                               LineCredit = fd.Credit,
                               LineRemarks = fd.Remarks ?? string.Empty,
                           }).OrderBy(x => x.SerialNo).ToList();
                return View(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion


        #region Voucher Report
        public ActionResult VoucherReport(string id)
        {
            if (User != null)
            {
                List<clsVoucher> voucher = new List<clsVoucher>();
                voucher = (from f in db.AC_Voucher_Main
                           join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                           where f.VoucherNo == id
                           select new clsVoucher
                           {
                               VoucherNo = f.VoucherNo,
                               VoucherDate = f.VoucherDate,
                               RefrenceNo = f.RefrenceNo ?? string.Empty,
                               //Line Data
                               SerialNo = fd.SerialNo,
                               LineDetailName = db.AC_Detail.Where(x => x.DetailCode == fd.DetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                               LineChequeNo = db.AC_ChequeInfo.Where(x => x.ChequeInfoId == fd.ChequeComputerNo).Select(x => x.ChequeNo).FirstOrDefault() ?? fd.ChequeNo,
                               LineDebit = fd.Debit,
                               LineCredit = fd.Credit,
                               LineRemarks = fd.Remarks ?? string.Empty,
                           }).OrderBy(x => x.SerialNo).ToList();
                return View(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
    }
}