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
    public class FinancialController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Reports
        #region Ledger Report
        public ActionResult Ledger()
        {
            ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
            ViewBag.SecondLevel = new SelectList(getSecondLevels(), "AccountCode", "Description");
            ViewBag.ThirdLevel = new SelectList(getThirdLevels(), "AccountCode", "Description");
            return View();
        }
        [HttpPost]
        public ActionResult LedgerReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                String[] secondLevel = Request.Form.GetValues("SecondLevelAccountCode");
                String[] thirdLevel = Request.Form.GetValues("ThirdLevelAccountCode");
                string reportType = Request.Form["ReportType"].ToString();
                string debitCreditType = Request.Form["DebitCreditType"].ToString();
                string openingBalanceType = Request.Form["OpeningBalanceType"].ToString();
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                var checkbox = Request.Form.GetValues("DateCheckbox");
                if (checkbox == null || fromDate == null || toDate == null)
                {
                    fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
                    toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                List<clsVoucher> voucherList = new List<clsVoucher>();
                clsVoucher voucher = new clsVoucher();
                clsAccountDetail customer;
                if (cust != null)
                {
                    for (int i = 0; i < cust.Length; i++)
                    {
                        decimal customerId = Convert.ToDecimal(cust[i]);
                        customer = new clsAccountDetail();
                        customer = (from c in db.AC_Detail_Extra
                                    join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                                    where c.DetailCode == customerId
                                    select new clsAccountDetail
                                    {
                                        DetailCode = customerId,
                                        Name = cd.Name,
                                        CityName = (from s in db.Cities join sd in db.AC_Detail_Extra on s.CityId equals sd.CityId where sd.DetailCode == cd.DetailCode select s.CityName).FirstOrDefault() ?? "",
                                        Address = c.Address ?? string.Empty,
                                        Phone1 = c.Phone1 ?? string.Empty,
                                    }).FirstOrDefault();
                        System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                        System.Data.Entity.Core.Objects.ObjectParameter CheckReturn2 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn2", typeof(decimal));
                        db.sp_GetCreditDaysAndPreviousBalance("",customerId, fromDate, "", 0, User.FiscalYearId, CheckReturn, CheckReturn2);
                        decimal balance = 0;
                        if (CheckReturn2.Value.ToString() != "")
                        {
                            balance = Convert.ToDecimal(CheckReturn2.Value);
                        }
                        customer.PreviousBalance = balance;
                        customer.ChequeList = (from c in db.AC_ChequeInfo
                                               where c.Status != "Clear" && c.Status != "Return" && (c.DDetailCode == customerId || c.CDetailCode == customerId)
                                               select new clsCheque
                                               {
                                                   ChequeNo = c.ChequeNo,
                                                   ChequeDate = c.ChequeDate,
                                                   DueDate = c.DueDate,
                                                   CDetailCode = c.CDetailCode,
                                                   DDetailCode = c.DDetailCode,
                                                   Amount = c.Amount,
                                                   DebitCreditType = debitCreditType
                                               }).ToList();
                        voucher.DetailList.Add(customer);

                        List<clsVoucher> list = GetVouchersListForLedger(customerId, fromDate, toDate, secondLevel, thirdLevel, debitCreditType, openingBalanceType);
                        voucherList.AddRange(list);

                    }
                }
                else
                {
                    List<clsAccountDetail> customers = getCustomers();
                    for (int i = 0; i < customers.Count; i++)
                    {
                        customer = new clsAccountDetail();
                        decimal customerId = Convert.ToDecimal(customers[i].DetailCode);
                        customer = (from c in db.AC_Detail_Extra
                                    join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                                    where c.DetailCode == customerId
                                    select new clsAccountDetail
                                    {
                                        DetailCode = customerId,
                                        Name = cd.Name,
                                        CityName = (from s in db.Cities join sd in db.AC_Detail_Extra on s.CityId equals sd.CityId where sd.DetailCode == cd.DetailCode select s.CityName).FirstOrDefault() ?? "",
                                        Address = c.Address ?? string.Empty,
                                        Phone1 = c.Phone1 ?? string.Empty,
                                    }).FirstOrDefault();
                        System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                        System.Data.Entity.Core.Objects.ObjectParameter CheckReturn2 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn2", typeof(decimal));
                        db.sp_GetCreditDaysAndPreviousBalance("",customerId, fromDate, "", 0, User.FiscalYearId, CheckReturn, CheckReturn2);
                        decimal balance = 0;
                        if (CheckReturn2.Value.ToString() != "")
                        {
                            balance = Convert.ToDecimal(CheckReturn2.Value);
                        }
                        customer.PreviousBalance = balance;
                        customer.ChequeList = (from c in db.AC_ChequeInfo
                                               where c.Status != "Clear" && c.Status != "Return" && (c.DDetailCode == customerId || c.CDetailCode == customerId)
                                               select new clsCheque
                                               {
                                                   ChequeNo = c.ChequeNo,
                                                   ChequeDate = c.ChequeDate,
                                                   DueDate = c.DueDate,
                                                   Bank = c.Bank,
                                                   Remarks = c.Remarks,
                                                   CDetailCode = c.CDetailCode,
                                                   DDetailCode = c.DDetailCode,
                                                   Amount = c.Amount,
                                                   DebitCreditType = debitCreditType
                                               }).ToList();
                        voucher.DetailList.Add(customer);

                        List<clsVoucher> list = GetVouchersListForLedger(customerId, fromDate, toDate, secondLevel, thirdLevel, debitCreditType, openingBalanceType);
                        voucherList.AddRange(list);
                    }
                }
                if (reportType == "Ledger Report")
                {
                    voucherList = voucherList.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ToList();
                    voucher.VouchersList = voucherList;
                    return View("printledgerreport", voucher);
                }
                else
                {
                    foreach (clsVoucher v in voucherList)
                    {
                        string detailType = db.AC_Detail.Where(x => x.DetailCode == v.DetailCode).Select(x => x.Type).FirstOrDefault();
                        v.DetailType = detailType;
                        if ((v.VoucherType == "Purchase" || v.VoucherType == "Purchase Return" || v.VoucherType == "Sale" || v.VoucherType == "Sale Return")
                            && (detailType == "Customer" || detailType == "Supplier" || detailType == "Customer + Supplier"))
                        {
                            v.InvoicesList = (from fd in db.Invoice_Detail
                                              join i in db.Item_Information on fd.ItemID equals i.ItemID
                                              where fd.ComputerNo == v.VoucherNo
                                              select new clsInvoice
                                              {
                                                  ItemID = fd.ItemID,
                                                  ItemName = i.ItemName,
                                                  Qty = fd.Qty,
                                                  Rate = fd.Rate,
                                                  Tax1Value = fd.Tax1Value,
                                                  Tax2Value = fd.Tax2Value,
                                                  NetValue = fd.NetValue
                                              }).ToList();
                        }
                    }
                    voucherList = voucherList.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ToList();
                    voucher.VouchersList = voucherList;
                    return View("printledgerreportwithitemdetail", voucher);
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetVouchersListForLedger(decimal customerId, DateTime fromDate, DateTime toDate, String[] secondLevel, String[] thirdLevel, string debitCreditType, string openingBalanceType)
        {
            List<clsVoucher> list = new List<clsVoucher>();
            List<clsVoucher> list2 = new List<clsVoucher>();
            List<clsVoucher> voucherList = new List<clsVoucher>();
            if (secondLevel == null && thirdLevel == null)
            {
                list = (from f in db.AC_Voucher_Main
                        join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                        where
                        fd.DetailCode == customerId
                        && f.VoucherDate >= fromDate
                        && f.VoucherDate <= toDate
                        select new clsVoucher
                        {
                            VoucherNo = f.VoucherNo,
                            VoucherDate = f.VoucherDate,
                            VoucherType = f.VoucherType,
                            DetailCode = fd.DetailCode,
                            RefrenceNo = f.RefrenceNo,
                            SerialNo = fd.SerialNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            Remarks = fd.Remarks,
                            Debit = fd.Debit,
                            Credit = fd.Credit,
                            DebitCreditType = debitCreditType,
                            OpeningBalanceType = openingBalanceType
                        }).ToList();
                voucherList.AddRange(list);
            }
            else if (secondLevel != null && thirdLevel == null)
            {
                for (int i = 0; i < secondLevel.Length; i++)
                {
                    decimal secondLevelCode = Convert.ToDecimal(secondLevel[i]);
                    list = (from f in db.AC_Voucher_Main
                            join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                            join ca in db.AC_ChartOfAC on fd.ACCode equals ca.AccountCode
                            where
                            fd.DetailCode == customerId
                            && f.VoucherDate >= fromDate
                            && f.VoucherDate <= toDate
                            && ca.RefKey == secondLevelCode.ToString()
                            select new clsVoucher
                            {
                                VoucherNo = f.VoucherNo,
                                VoucherDate = f.VoucherDate,
                                VoucherType = f.VoucherType,
                                RefrenceNo = f.RefrenceNo,
                                DetailCode = fd.DetailCode,
                                SerialNo = fd.SerialNo,
                                FromDate = fromDate,
                                ToDate = toDate,
                                Remarks = fd.Remarks,
                                Debit = fd.Debit,
                                Credit = fd.Credit,
                                DebitCreditType = debitCreditType,
                                OpeningBalanceType = openingBalanceType
                            }).ToList();
                    voucherList.AddRange(list);
                }
            }
            else if (secondLevel == null && thirdLevel != null)
            {
                for (int i = 0; i < thirdLevel.Length; i++)
                {
                    decimal thirdLevelCode = Convert.ToDecimal(thirdLevel[i]);
                    list = (from f in db.AC_Voucher_Main
                            join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                            join ca in db.AC_ChartOfAC on fd.ACCode equals ca.AccountCode
                            where
                            fd.DetailCode == customerId
                            && f.VoucherDate >= fromDate
                            && f.VoucherDate <= toDate
                            && ca.AccountCode == thirdLevelCode
                            select new clsVoucher
                            {
                                VoucherNo = f.VoucherNo,
                                VoucherDate = f.VoucherDate,
                                VoucherType = f.VoucherType,
                                RefrenceNo = f.RefrenceNo,
                                DetailCode = fd.DetailCode,
                                SerialNo = fd.SerialNo,
                                FromDate = fromDate,
                                ToDate = toDate,
                                Remarks = fd.Remarks,
                                Debit = fd.Debit,
                                Credit = fd.Credit,
                                DebitCreditType = debitCreditType,
                                OpeningBalanceType = openingBalanceType
                            }).ToList();
                    voucherList.AddRange(list);
                }
            }
            return voucherList;
        }
        public List<clsAccountDetail> getCustomers()
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
        public List<clsChartOfAccount> getThirdLevels()
        {
            List<clsChartOfAccount> thirdLevel = new List<clsChartOfAccount>();
            thirdLevel = (from c in db.AC_ChartOfAC
                          where c.AcLevel == 3 && c.Status == "Active"
                          select new clsChartOfAccount
                          {
                              AccountCode = c.AccountCode,
                              Description = c.Description
                          }).ToList();
            return thirdLevel;
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
        #endregion
        #region Ledger Report Third Level
        public ActionResult LedgerThirdLevel()
        {
            ViewBag.SecondLevel = new SelectList(getSecondLevels(), "AccountCode", "Description");
            ViewBag.ThirdLevel = new SelectList(getThirdLevels(), "AccountCode", "Description");
            return View();
        }
        [HttpPost]
        public ActionResult PrintLedgerThirdLevelReport()
        {
            if (User != null)
            {
                String[] thirdLevel = Request.Form.GetValues("ThirdLevelAccountCode");
                String[] secondLevel = Request.Form.GetValues("SecondLevelAccountCode");
                string debitCreditType = Request.Form["DebitCreditType"].ToString();
                string openingBalanceType = Request.Form["OpeningBalanceType"].ToString();
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                var checkbox = Request.Form.GetValues("DateCheckbox");
                if (checkbox == null || fromDate == null || toDate == null)
                {
                    fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
                    toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                List<clsVoucher> voucherList = new List<clsVoucher>();
                clsVoucher voucher = new clsVoucher();
                clsChartOfAccount third;
                if (thirdLevel != null)
                {
                    for (int i = 0; i < thirdLevel.Length; i++)
                    {
                        decimal thirdLevelCode = Convert.ToDecimal(thirdLevel[i]);
                        third = new clsChartOfAccount();
                        third = (from c in db.AC_ChartOfAC
                                 where c.AccountCode == thirdLevelCode
                                 select new clsChartOfAccount
                                 {
                                     AccountCode = thirdLevelCode,
                                     Description = c.Description
                                 }).FirstOrDefault();
                        third.ChequeList = (from c in db.AC_ChequeInfo
                                            where c.Status != "Clear" && c.Status != "Return" && (c.DAccountCode == thirdLevelCode || c.CAccountCode == thirdLevelCode)
                                            select new clsCheque
                                            {
                                                ChequeNo = c.ChequeNo,
                                                ChequeDate = c.ChequeDate,
                                                DueDate = c.DueDate,
                                                DAccountCode = c.DAccountCode,
                                                CAccountCode = c.CAccountCode,
                                                DebitCreditType = debitCreditType,
                                                Amount = c.Amount,
                                            }).ToList();
                        voucher.ThirdLevelList.Add(third);

                        List<clsVoucher> list = GetVouchersListForLedgerThirdLevel(thirdLevelCode, fromDate, toDate, secondLevel, debitCreditType, openingBalanceType);
                        voucherList.AddRange(list);

                    }
                }
                else
                {
                    List<clsChartOfAccount> thirdLevels = getThirdLevels();
                    for (int i = 0; i < thirdLevels.Count; i++)
                    {
                        decimal thirdLevelCode = Convert.ToDecimal(thirdLevels[i].AccountCode);
                        third = new clsChartOfAccount();
                        third = (from c in db.AC_ChartOfAC
                                 where c.AccountCode == thirdLevelCode
                                 select new clsChartOfAccount
                                 {
                                     AccountCode = thirdLevelCode,
                                     Description = c.Description
                                 }).FirstOrDefault();
                        third.ChequeList = (from c in db.AC_ChequeInfo
                                            where c.Status != "Clear" && c.Status != "Return" && (c.DAccountCode == thirdLevelCode || c.CAccountCode == thirdLevelCode)
                                            select new clsCheque
                                            {
                                                ChequeNo = c.ChequeNo,
                                                ChequeDate = c.ChequeDate,
                                                DueDate = c.DueDate,
                                                DAccountCode = c.DAccountCode,
                                                CAccountCode = c.CAccountCode,
                                                Amount = c.Amount,
                                                DebitCreditType = debitCreditType
                                            }).ToList();
                        voucher.ThirdLevelList.Add(third);

                        List<clsVoucher> list = GetVouchersListForLedgerThirdLevel(thirdLevelCode, fromDate, toDate, secondLevel, debitCreditType, openingBalanceType);
                        voucherList.AddRange(list);
                    }
                }
                voucherList = voucherList.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ToList();
                voucher.VouchersList = voucherList;
                return View(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetVouchersListForLedgerThirdLevel(decimal thirdLevelCode, DateTime fromDate, DateTime toDate, String[] secondLevel, string debitCreditType, string openingBalanceType)
        {
            List<clsVoucher> list = new List<clsVoucher>();
            List<clsVoucher> list2 = new List<clsVoucher>();
            List<clsVoucher> voucherList = new List<clsVoucher>();
            if (secondLevel == null)
            {
                list = (from f in db.AC_Voucher_Main
                        join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                        join ca in db.AC_ChartOfAC on fd.ACCode equals ca.AccountCode
                        where
                        f.VoucherDate >= fromDate
                        && f.VoucherDate <= toDate
                        && ca.AccountCode == thirdLevelCode
                        select new clsVoucher
                        {
                            VoucherNo = f.VoucherNo,
                            VoucherDate = f.VoucherDate,
                            VoucherType = f.VoucherType,
                            ACCode = fd.ACCode,
                            DetailCode=fd.DetailCode,
                            DetailName =db.AC_Detail.Where(x=>x.DetailCode==fd.DetailCode).Select(x=>x.Name).FirstOrDefault() ?? "", 
                            RefrenceNo = f.RefrenceNo,
                            SerialNo = fd.SerialNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            Remarks = fd.Remarks,
                            Debit = fd.Debit,
                            Credit = fd.Credit,
                            OpeningBalanceType = openingBalanceType,
                            DebitCreditType = debitCreditType
                        }).ToList();
                voucherList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < secondLevel.Length; i++)
                {
                    decimal secondLevelCode = Convert.ToDecimal(secondLevel[i]);
                    list = (from f in db.AC_Voucher_Main
                            join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                            join ca in db.AC_ChartOfAC on fd.ACCode equals ca.AccountCode
                            where
                            ca.AccountCode == thirdLevelCode
                            && f.VoucherDate >= fromDate
                            && f.VoucherDate <= toDate
                            && ca.RefKey == secondLevelCode.ToString()
                            select new clsVoucher
                            {
                                VoucherNo = f.VoucherNo,
                                VoucherDate = f.VoucherDate,
                                VoucherType = f.VoucherType,
                                RefrenceNo = f.RefrenceNo,
                                ACCode = fd.ACCode,
                                SerialNo = fd.SerialNo,
                                FromDate = fromDate,
                                ToDate = toDate,
                                Remarks = fd.Remarks,
                                Debit = fd.Debit,
                                Credit = fd.Credit,
                                OpeningBalanceType = openingBalanceType,
                                DebitCreditType = debitCreditType
                            }).ToList();
                    voucherList.AddRange(list);
                }
            }
            return voucherList;
        }
        #endregion
        #region Trial Balance Report
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
        public ActionResult TrialBalance()
        {
            ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
            ViewBag.SecondLevel = new SelectList(getSecondLevels(), "AccountCode", "Description");
            ViewBag.ThirdLevel = new SelectList(getThirdLevels(), "AccountCode", "Description");
            ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintTrialBalanceReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                String[] secondLevel = Request.Form.GetValues("SecondLevelAccountCode");
                String[] thirdLevel = Request.Form.GetValues("ThirdLevelAccountCode");
                String[] City = Request.Form.GetValues("CityId");
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                List<clsVoucher> list = GetVouchersListForTrialBalance(cust, toDate, secondLevel, thirdLevel, City);
                list = list.OrderBy(x => x.DetailName).ToList();

                if (salesmanID == null)
                {
                    salesmanID = 0;
                }
                if (salesmanID != 0)
                {
                    list = list.Where(x => x.SalesmanID == salesmanID).OrderBy(x => x.DetailName).ToList();
                }

                return View(list);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetVouchersListForTrialBalance(String[] customer, DateTime toDate, String[] secondLevel, String[] thirdLevel, String[]  City)
        {
            List<clsVoucher> list = new List<clsVoucher>();
            List<clsVoucher> list2 = new List<clsVoucher>();
            List<clsVoucher> list3 = new List<clsVoucher>();
            List<clsVoucher> list4 = new List<clsVoucher>();
            List<clsVoucher> list5 = new List<clsVoucher>();
            List<clsVoucher> list6 = new List<clsVoucher>();
            if (toDate == null)
            {
                toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
            }
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_TrialBalanceDetail", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = toDate;
            cmd.Parameters.Add("@vFiscalYear", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);

            decimal customerid;
            int vsalesmanid;
            while (dt.Read())
            {
                customerid = Convert.ToDecimal(dt[0].ToString());
                vsalesmanid = db.AC_Detail_Extra.Where(x => x.DetailCode == customerid).Select(x => x.SalesmanID).FirstOrDefault() ?? 0;
                list2.Add(new clsVoucher()
                {
                    DetailCode = Convert.ToDecimal(dt[0]),
                    DetailName = dt[1].ToString(),
                    CityId = Convert.ToInt32(dt[2]),
                    Cell1 = dt[3].ToString(),
                    ToDate = toDate,
                    ThirdLevelCode = Convert.ToInt32(dt[5]),
                    SecondLevelCode= Convert.ToInt32(dt[6]),
                    BalanceAmount = Convert.ToDecimal(dt[7]),
                    SalesmanID = vsalesmanid,
                    SalesmanName = db.Admin_Setup_Salesman.Where(x => x.SalesmanID == vsalesmanid).Select(x => x.FullName).FirstOrDefault() ?? "",
                });
            }

            
            list3.AddRange(list2);
            list4.AddRange(list2);
            list5.AddRange(list2);
            list6.AddRange(list2);
            if (customer != null)
            {
                int j = 0;
                for (int i = 0; i < customer.Length; i++)
                {
                    decimal customerId = Convert.ToInt32(customer[i]);
                    //if (list2.Any(x => x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsVoucher>();
                        list4 = new List<clsVoucher>();
                        list5 = new List<clsVoucher>();
                        list6 = new List<clsVoucher>();
                    }
                    list = new List<clsVoucher>();
                    list = list2.Where(x => x.DetailCode == customerId).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                    list6.AddRange(list);
                    j++;
                    //}
                }
            }
            if (thirdLevel != null)
            {
                int j = 0;
                for (int i = 0; i < thirdLevel.Length; i++)
                {
                    decimal accoundCode = Convert.ToInt32(thirdLevel[i]);
                    //if (list2.Any(x => x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsVoucher>();
                        list5 = new List<clsVoucher>();
                        list6 = new List<clsVoucher>();
                    }
                    list = new List<clsVoucher>();
                    list = list3.Where(x => x.ThirdLevelCode == accoundCode).ToList();
                    list4.AddRange(list);
                    list5.AddRange(list);
                    list6.AddRange(list);
                    j++;
                    //}
                }
            }
            if (secondLevel != null)
            {
                int j = 0;
                for (int i = 0; i < secondLevel.Length; i++)
                {
                    decimal accoundCode = Convert.ToInt32(secondLevel[i]);
                    //if (list2.Any(x => x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list5 = new List<clsVoucher>();
                        list6 = new List<clsVoucher>();
                    }
                    list = new List<clsVoucher>();
                    list = list4.Where(x => x.SecondLevelCode == accoundCode).ToList();
                    list5.AddRange(list);
                    list6.AddRange(list);
                    j++;
                    //}
                }
            }
            if (City != null)
            {
                int j = 0;
                for (int i = 0; i < City.Length; i++)
                {
                    int cityId = Convert.ToInt32(City[i]);
                    //if (list2.Any(x => x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list6 = new List<clsVoucher>();
                    }
                    list = new List<clsVoucher>();
                    list = list5.Where(x => x.CityId == cityId).ToList();
                    list6.AddRange(list);
                    j++;
                    //}
                }
            }
            return list6;
        }
        #endregion
        #region Running Trial Balance Report
        public ActionResult RunningTrialBalance()
        {
            ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
            ViewBag.SecondLevel = new SelectList(getSecondLevels(), "AccountCode", "Description");
            ViewBag.ThirdLevel = new SelectList(getThirdLevels(), "AccountCode", "Description");
            ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintRunningTrialBalanceReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                String[] secondLevel = Request.Form.GetValues("SecondLevelAccountCode");
                String[] thirdLevel = Request.Form.GetValues("ThirdLevelAccountCode");
                String[] City = Request.Form.GetValues("CityId");
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);

                List<clsVoucher> list = GetVouchersListForRunningTrialBalance(cust, fromDate, toDate, secondLevel, thirdLevel, City);
                list = list.OrderBy(x => x.DetailName).ToList();


                if (salesmanID == null)
                {
                    salesmanID = 0;
                }
                if (salesmanID != 0)
                {
                    list = list.Where(x => x.SalesmanID == salesmanID).OrderBy(x => x.DetailName).ToList();
                }


                return View(list);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetVouchersListForRunningTrialBalance(String[] customer, DateTime fromDate, DateTime toDate, String[] secondLevel, String[] thirdLevel, String[] City)
        {
            List<clsVoucher> list = new List<clsVoucher>();
            List<clsVoucher> list2 = new List<clsVoucher>();
            List<clsVoucher> list3 = new List<clsVoucher>();
            List<clsVoucher> list4 = new List<clsVoucher>();
            List<clsVoucher> list5 = new List<clsVoucher>();
            List<clsVoucher> list6 = new List<clsVoucher>();
            if (fromDate == null)
            {
                fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
            }
            if (toDate == null)
            {
                toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
            }
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_TrialBalanceDetailRunning", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vFromDate", SqlDbType.Date).Value = toDate;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = toDate;
            cmd.Parameters.Add("@vFiscalYear", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);

            decimal customerid;
            int vsalesmanid;
            while (dt.Read())
            {
                customerid = Convert.ToDecimal(dt[0].ToString());
                vsalesmanid = db.AC_Detail_Extra.Where(x => x.DetailCode == customerid).Select(x => x.SalesmanID).FirstOrDefault() ?? 0;

                list2.Add(new clsVoucher()
                {
                    DetailCode = Convert.ToDecimal(dt[0]),
                    DetailName = dt[1].ToString(),
                    CityId = Convert.ToInt32(dt[2]),
                    Cell1 = dt[3].ToString(),
                    FromDate = fromDate,
                    ToDate = toDate,
                    ThirdLevelCode = Convert.ToInt32(dt[5]),
                    SecondLevelCode = Convert.ToInt32(dt[6]),
                    PreviousBalance = Convert.ToDecimal(dt[7]),
                    Debit = Convert.ToDecimal(dt[8]),
                    Credit = Convert.ToDecimal(dt[9]),
                    BalanceAmount = Convert.ToDecimal(dt[10]),
                    SalesmanID = vsalesmanid,
                    SalesmanName = db.Admin_Setup_Salesman.Where(x => x.SalesmanID == vsalesmanid).Select(x => x.FullName).FirstOrDefault() ?? "",
                });
            }
            list3.AddRange(list2);
            list4.AddRange(list2);
            list5.AddRange(list2);
            list6.AddRange(list2);
            if (customer != null)
            {
                int j = 0;
                for (int i = 0; i < customer.Length; i++)
                {
                    decimal customerId = Convert.ToInt32(customer[i]);
                    //if (list2.Any(x => x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsVoucher>();
                        list4 = new List<clsVoucher>();
                        list5 = new List<clsVoucher>();
                        list6 = new List<clsVoucher>();
                    }
                    list = new List<clsVoucher>();
                    list = list2.Where(x => x.DetailCode == customerId).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                    list6.AddRange(list);
                    j++;
                    //}
                }
            }
            if (thirdLevel != null)
            {
                int j = 0;
                for (int i = 0; i < thirdLevel.Length; i++)
                {
                    decimal accoundCode = Convert.ToInt32(thirdLevel[i]);
                    //if (list2.Any(x => x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsVoucher>();
                        list5 = new List<clsVoucher>();
                        list6 = new List<clsVoucher>();
                    }
                    list = new List<clsVoucher>();
                    list = list3.Where(x => x.ThirdLevelCode == accoundCode).ToList();
                    list4.AddRange(list);
                    list5.AddRange(list);
                    list6.AddRange(list);
                    j++;
                    //}
                }
            }
            if (secondLevel != null)
            {
                int j = 0;
                for (int i = 0; i < secondLevel.Length; i++)
                {
                    decimal accoundCode = Convert.ToInt32(secondLevel[i]);
                    //if (list2.Any(x => x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list5 = new List<clsVoucher>();
                        list6 = new List<clsVoucher>();
                    }
                    list = new List<clsVoucher>();
                    list = list4.Where(x => x.SecondLevelCode == accoundCode).ToList();
                    list5.AddRange(list);
                    list6.AddRange(list);
                    j++;
                    //}
                }
            }
            if (City != null)
            {
                int j = 0;
                for (int i = 0; i < City.Length; i++)
                {
                    int cityId = Convert.ToInt32(City[i]);
                    //if (list2.Any(x => x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list6 = new List<clsVoucher>();
                    }
                    list = new List<clsVoucher>();
                    list = list5.Where(x => x.CityId == cityId).ToList();
                    list6.AddRange(list);
                    j++;
                    //}
                }
            }
            return list6;
        }
        #endregion
        #region Sale Aging
        public ActionResult AgingSale()
        {
            ViewBag.Customers = new SelectList(getCustomerDetailCodes(), "DetailCode", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult PrintSaleAging()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime? untilDate = Convert.ToDateTime(Request.Form["UntilDate"]);
                var checkbox = Request.Form.GetValues("AllCheckbox");
                clsVoucher voucher = new clsVoucher();
                List<clsVoucher> voucherList = new List<clsVoucher>();
                if (cust != null)
                {
                    for (int i = 0; i < cust.Length; i++)
                    {
                        decimal customerId = Convert.ToDecimal(cust[i]);
                        List<clsVoucher> list = GetSaleAging(customerId, untilDate);
                        voucherList.AddRange(list);
                    }
                }
                else
                {
                    List<clsAccountDetail> customers = getCustomerDetailCodes();
                    for (int i = 0; i < customers.Count; i++)
                    {
                        decimal customerId = Convert.ToDecimal(customers[i].DetailCode);
                        List<clsVoucher> list = GetSaleAging(customerId, untilDate);
                        voucherList.AddRange(list);
                    }
                }
                if (checkbox == null)
                {
                    voucherList = voucherList.Where(x => x.PendingAmount != 0).ToList();
                }
                voucher.VouchersList = voucherList.GroupBy(x => x.DetailCode).Select(g => g.First()).ToList();
                voucher.VouchersList = voucher.VouchersList.OrderBy(x => x.DetailName).ToList();
                voucherList = voucherList.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ToList();
                voucher.VouchersList2 = voucherList;
                return View(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetSaleAging(decimal customerId, DateTime? untilDate)
        {
            List<clsVoucher> list = new List<clsVoucher>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_AgingSale", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vCustCode", SqlDbType.Decimal).Value = customerId;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = untilDate;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                decimal detailCode = Convert.ToDecimal(dt[0]);
                list.Add(new clsVoucher()
                {
                    DetailCode = detailCode,
                    DetailName = dt[1].ToString(),
                    Address = dt[2].ToString(),
                    Phone1 = dt[3].ToString(),
                    CreditDays = Convert.ToInt32(dt[4]),
                    DueDays = Convert.ToInt32(dt[5]),
                    VoucherNo = dt[6].ToString(),
                    VoucherDate = Convert.ToDateTime(dt[7]),
                    NetAmount = Convert.ToDecimal(dt[8]),
                    ReceivedAmount = Convert.ToDecimal(dt[9]),
                    BalanceAmount = Convert.ToDecimal(dt[10]),
                    AdjustAmount = Convert.ToDecimal(dt[11]),
                    PendingAmount = Convert.ToDecimal(dt[12]),
                    ToDate = untilDate,
                    CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == detailCode select ct.CityName).FirstOrDefault() ?? "",
                });
            }
            return list;
        }
        public List<clsAccountDetail> getCustomerDetailCodes()
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
        #region Purchase Aging
        public ActionResult AgingPurchase()
        {
            ViewBag.Supplier = new SelectList(getSupplierDetailCodes(), "DetailCode", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult PrintPurchaseAging()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime? untilDate = Convert.ToDateTime(Request.Form["UntilDate"]);
                var checkbox = Request.Form.GetValues("AllCheckbox");
                clsVoucher voucher = new clsVoucher();
                List<clsVoucher> voucherList = new List<clsVoucher>();
                if (cust != null)
                {
                    for (int i = 0; i < cust.Length; i++)
                    {
                        decimal customerId = Convert.ToDecimal(cust[i]);
                        List<clsVoucher> list = GetPurchaseAging(customerId, untilDate);
                        voucherList.AddRange(list);
                    }
                }
                else
                {
                    List<clsAccountDetail> customers = getCustomerDetailCodes();
                    for (int i = 0; i < customers.Count; i++)
                    {
                        decimal customerId = Convert.ToDecimal(customers[i].DetailCode);
                        List<clsVoucher> list = GetPurchaseAging(customerId, untilDate);
                        voucherList.AddRange(list);
                    }
                }
                if (checkbox == null)
                {
                    voucherList = voucherList.Where(x => x.PendingAmount != 0).ToList();
                }
                voucher.VouchersList = voucherList.GroupBy(x => x.DetailCode).Select(g => g.First()).ToList();
                voucher.VouchersList = voucher.VouchersList.OrderBy(x => x.DetailName).ToList();
                voucherList = voucherList.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ToList();
                voucher.VouchersList2 = voucherList;
                return View(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetPurchaseAging(decimal customerId, DateTime? untilDate)
        {
            List<clsVoucher> list = new List<clsVoucher>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_AgingPurchase", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vCustCode", SqlDbType.Decimal).Value = customerId;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = untilDate;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                decimal detailCode = Convert.ToDecimal(dt[0]);
                list.Add(new clsVoucher()
                {
                    DetailCode = detailCode,
                    DetailName = dt[1].ToString(),
                    Address = dt[2].ToString(),
                    Phone1 = dt[3].ToString(),
                    CreditDays = Convert.ToInt32(dt[4]),
                    DueDays = Convert.ToInt32(dt[5]),
                    VoucherNo = dt[6].ToString(),
                    VoucherDate = Convert.ToDateTime(dt[7]),
                    NetAmount = Convert.ToDecimal(dt[8]),
                    ReceivedAmount = Convert.ToDecimal(dt[9]),
                    BalanceAmount = Convert.ToDecimal(dt[10]),
                    AdjustAmount = Convert.ToDecimal(dt[11]),
                    PendingAmount = Convert.ToDecimal(dt[12]),
                    ToDate = untilDate,
                    CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == detailCode select ct.CityName).FirstOrDefault() ?? "",
                });
            }
            return list;
        }
        public List<clsAccountDetail> getSupplierDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && (c.Type == "Supplier" || c.Type == "Customer + Supplier")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).ToList();
            return detail;
        }
        #endregion
        #region Income Statement
        public ActionResult IncomeStatement()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PrintIncomeStatement()
        {
            if (User != null)
            {
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                if (fromDate == null || toDate == null)
                {
                    fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
                    toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                List<clsChartOfAccount> accountsList = GetIncomeStatement(fromDate, toDate);
                return View(accountsList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsChartOfAccount> GetIncomeStatement(DateTime? fromDate, DateTime? todDate)
        {
            List<clsChartOfAccount> list = new List<clsChartOfAccount>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_IncomeStatement", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vFromDate", SqlDbType.Date).Value = fromDate;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = todDate;
            cmd.Parameters.Add("@vFiscalYear", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsChartOfAccount()
                {
                    DetailName = dt["ReportGroup"].ToString(),
                    LineDetailName = dt["Description"].ToString(),
                    AccountCodeV = dt["AccountCodeV"].ToString(),
                    TotalBalance = Convert.ToDecimal(dt["TotalBalance"]),
                    FromDate = fromDate,
                    ToDate = todDate
                });
            }
            return list;
        }
        #endregion
        #region Financial Statement
        public ActionResult FinancialStatement()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PrintFinancialStatement()
        {
            if (User != null)
            {
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                if (toDate == null)
                {
                    toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                List<clsChartOfAccount> accountsList = GetFinancialStatement(toDate);
                return View(accountsList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsChartOfAccount> GetFinancialStatement(DateTime? todDate)
        {
            List<clsChartOfAccount> list = new List<clsChartOfAccount>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_FinancialStatement", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = todDate;
            cmd.Parameters.Add("@vFiscalYear", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsChartOfAccount()
                {
                    DetailName = dt["ReportGroup"].ToString(),
                    LineDetailName = dt["Description"].ToString(),
                    AccountCodeV = dt["AccountCodeV"].ToString(),
                    TotalBalance = Convert.ToDecimal(dt["TotalBalance"]),
                    ToDate = todDate
                });
            }
            return list;
        }
        #endregion 
        #endregion
    }
}