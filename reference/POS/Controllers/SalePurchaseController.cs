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
using Newtonsoft.Json;

namespace POS.Controllers
{
    public class SalePurchaseController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Sale Purchase Lookup
        public ActionResult SalePurchaseLookup()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "History"
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
        public List<clsAccountDetail> getDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && (c.Type == "Supplier" || c.Type == "Customer" || c.Type == "Customer + Supplier")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).ToList();
            return detail;
        }
        public ActionResult SetSessions(string type, bool? customerCheckbox, decimal? customerId,
                                        bool? itemCheckbox, int? itemId, bool? dateCheckbox, DateTime? fromDate,
                                        DateTime? toDate,bool? cashCustomerCheckbox, string cashCustomerName,
                                        bool? cashCreditTypeCheckbox, string cashCreditType,
                                        bool? brandCheckbox, int? brandId,bool? codeCheckbox, string code)
        {
            bool status = true;
            Session["Type"] = type;
            if(customerCheckbox == true && customerId != null)
            {
                Session["CustomerId"] = customerId;
            }
            else
            {
                Session["CustomerId"] = null;
            }

            if(itemCheckbox == true && itemId != null)
            {
                Session["ItemId"] = itemId;
            }
            else
            {
                Session["ItemId"] = null; 
            }

            if(dateCheckbox == true && fromDate != null && toDate != null)
            {
                Session["FromDate"] = fromDate;
                Session["ToDate"] = toDate;
            }
            else
            {
                Session["FromDate"] = null;
                Session["ToDate"] = null; 
            }
            if (cashCustomerCheckbox == true && !(string.IsNullOrEmpty(cashCustomerName)))
            {
                Session["CashCustomerName"] = cashCustomerName;
            }
            else
            {
                Session["CashCustomerName"] = null;
            }
            //////////////////
            if (cashCreditTypeCheckbox == true && !(string.IsNullOrEmpty(cashCreditType)))
            {
                Session["CashCreditType"] = cashCreditType;
            }
            else
            {
                Session["CashCreditType"] = null;
            }
            ////////////////
            if (brandCheckbox == true && brandId != null)
            {
                Session["Brand"] = brandId;
            }
            else
            {
                Session["Brand"] = null;
            }
            /////////////////////
            if (codeCheckbox == true && !(string.IsNullOrEmpty(code)))
            {
                Session["Code"] = code;
            }
            else
            {
                Session["Code"] = null;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllItems()
        {
            if (User != null)
            {
                string type = "";
                decimal? customerId = 0;
                int? itemId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string cashCustomerName = "";
                string cashCreditType = "";
                int brandId = 0;
                string code = "";
                int invoiceType = 0;
                string whereCondition = "";
                if (Session["Type"] != null)
                {
                    type = Session["Type"].ToString();
                    whereCondition = " Type = '" + type + "'";
                }
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                    whereCondition += " and DetailCode= " + customerId + "";
                }
                if (Session["ItemId"] != null)
                {
                    itemId = Convert.ToInt32(Session["ItemId"]);
                    whereCondition += " and ItemID= " + itemId + "";
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                    whereCondition += " and RecordDate >= '" + fromDate + "' and RecordDate <= '" + toDate + "'";
                }
                if (Session["CashCustomerName"] != null)
                {
                    cashCustomerName = Session["CashCustomerName"].ToString();
                    whereCondition += " and DetailName LIKE '%" + cashCustomerName + "%'";
                }
                if (Session["Brand"] != null)
                {
                    brandId = Convert.ToInt32(Session["Brand"]);
                    whereCondition += " and BrandID= " + brandId + "";
                }
                if (Session["Code"] != null)
                {
                    code = Session["Code"].ToString();
                    whereCondition += " and UrduName LIKE '%" + code + "%'";
                }
                if (Session["CashCreditType"] != null)
                {
                    cashCreditType = Session["CashCreditType"].ToString();
                    if (cashCreditType == "Credit")
                    {
                        invoiceType = 1;
                    }
                    else
                    {
                        invoiceType = 0;
                    }
                    whereCondition += " and InvoiceType= " + invoiceType + "";
                }
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                DataTableReader dtr = clsSqlHistoryLookup.getInvoicesListCount(whereCondition);
                while (dtr.Read())
                {
                    recordsTotal = Convert.ToInt32(dtr["MyRowCount"]);
                }
                DataTableReader dt = clsSqlHistoryLookup.getInvoicesList(whereCondition, start, length);
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        ItemName = dt["ItemName"].ToString(),
                        RecordDate = Convert.ToDateTime(dt["RecordDate"]),
                        DetailName = dt["DetailName"].ToString(),
                        Barcode = dt["UrduName"].ToString(),
                        BrandName = dt["BrandName"].ToString(),
                        Qty = Convert.ToDecimal(dt["Qty"]),
                        Rate = Convert.ToDecimal(dt["Rate"]),
                        GrossValue = Convert.ToDecimal(dt["GrossValue"]),
                    });
                }
                invoice = invoice.OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                //recordsTotal = invoice.Count();
                //var data = invoice.Skip(skip).Take(pageSize).ToList();
                var data = invoice.ToList();
                return Json(new { draw = 0, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult PrintSalePurchaseLookup()
        {
            if (User != null)
            {
                string type = "";
                decimal? customerId = 0;
                int? itemId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string cashCustomerName = "";
                string cashCreditType = "";
                int brandId = 0;
                string code = "";
                int invoiceType = 0;
                string whereCondition = "";
                if (Session["Type"] != null)
                {
                    type = Session["Type"].ToString();
                    whereCondition = " Type = '" + type + "'";
                }
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                    whereCondition += " and DetailCode= " + customerId + "";
                }
                if (Session["ItemId"] != null)
                {
                    itemId = Convert.ToInt32(Session["ItemId"]);
                    whereCondition += " and ItemID= " + itemId + "";
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                    whereCondition += " and RecordDate >= '" + fromDate + "' and RecordDate <= '" + toDate + "'";
                }
                if (Session["CashCustomerName"] != null)
                {
                    cashCustomerName = Session["CashCustomerName"].ToString();
                    whereCondition += " and DetailName LIKE '%" + cashCustomerName + "%'";
                }
                if (Session["Brand"] != null)
                {
                    brandId = Convert.ToInt32(Session["Brand"]);
                    whereCondition += " and BrandID= " + brandId + "";
                }
                if (Session["Code"] != null)
                {
                    code = Session["Code"].ToString();
                    whereCondition += " and UrduName LIKE '%" + code + "%'";
                }
                if (Session["CashCreditType"] != null)
                {
                    cashCreditType = Session["CashCreditType"].ToString();
                    if (cashCreditType == "Credit")
                    {
                        invoiceType = 1;
                    }
                    else
                    {
                        invoiceType = 0;
                    }
                    whereCondition += " and InvoiceType= " + invoiceType + "";
                }
                DataTableReader dt = clsSqlHistoryLookup.getInvoicesListForPrint(whereCondition);
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        ItemName = dt["ItemName"].ToString(),
                        RecordDate = Convert.ToDateTime(dt["RecordDate"]),
                        DetailName = dt["DetailName"].ToString(),
                        Barcode = dt["UrduName"].ToString(),
                        BrandName = dt["BrandName"].ToString(),
                        Qty = Convert.ToDecimal(dt["Qty"]),
                        Rate = Convert.ToDecimal(dt["Rate"]),
                        GrossValue = Convert.ToDecimal(dt["GrossValue"]),
                    });
                }
                invoice = invoice.OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                return View(invoice);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion

        #region Sale History Lookup
        public ActionResult SaleHistoryLookup()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SaleHistory"
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

        public ActionResult PrintSaleHistoryLookup()
        {
            if (User != null)
            {
                string type = "";
                decimal? customerId = 0;
                int? itemId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string cashCustomerName = "";
                string cashCreditType = "";
                int brandId = 0;
                string code = "";
                int invoiceType = 0;
                string whereCondition = "";
                if (Session["Type"] != null)
                {
                    type = Session["Type"].ToString();
                    whereCondition = " Type = '" + type + "'";
                }
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                    whereCondition += " and DetailCode= " + customerId + "";
                }
                if (Session["ItemId"] != null)
                {
                    itemId = Convert.ToInt32(Session["ItemId"]);
                    whereCondition += " and ItemID= " + itemId + "";
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                    whereCondition += " and RecordDate >= '" + fromDate + "' and RecordDate <= '" + toDate + "'";
                }
                if (Session["CashCustomerName"] != null)
                {
                    cashCustomerName = Session["CashCustomerName"].ToString();
                    whereCondition += " and DetailName LIKE '%" + cashCustomerName + "%'";
                }
                if (Session["Brand"] != null)
                {
                    brandId = Convert.ToInt32(Session["Brand"]);
                    whereCondition += " and BrandID= " + brandId + "";
                }
                if (Session["Code"] != null)
                {
                    code = Session["Code"].ToString();
                    whereCondition += " and UrduName LIKE '%" + code + "%'";
                }
                if (Session["CashCreditType"] != null)
                {
                    cashCreditType = Session["CashCreditType"].ToString();
                    if (cashCreditType == "Credit")
                    {
                        invoiceType = 1;
                    }
                    else
                    {
                        invoiceType = 0;
                    }
                    whereCondition += " and InvoiceType= " + invoiceType + "";
                }
                DataTableReader dt = clsSqlHistoryLookup.getInvoicesListForPrint(whereCondition);
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        ItemName = dt["ItemName"].ToString(),
                        RecordDate = Convert.ToDateTime(dt["RecordDate"]),
                        DetailName = dt["DetailName"].ToString(),
                        Barcode = dt["UrduName"].ToString(),
                        BrandName = dt["BrandName"].ToString(),
                        Qty = Convert.ToDecimal(dt["Qty"]),
                        Rate = Convert.ToDecimal(dt["Rate"]),
                        GrossValue = Convert.ToDecimal(dt["GrossValue"]),
                    });
                }
                invoice = invoice.OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                return View(invoice);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion

        #region Sale Report
        public ActionResult SaleReportIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SaleInvoiceReport"
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
        public ActionResult SetSessionsForSaleReport(bool? customerCheckbox, decimal? customerId,bool? dateCheckbox, DateTime? fromDate,
                                        DateTime? toDate, bool? cashCustomerCheckbox, string cashCustomerName,
                                        bool? cashCreditTypeCheckbox, string cashCreditType, bool? computerNoCheckbox, string computerNo)
        {
            bool status = true;
            if (customerCheckbox == true && customerId != null)
            {
                Session["CustomerId"] = customerId;
            }
            else
            {
                Session["CustomerId"] = null;
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
            if (cashCustomerCheckbox == true && !(string.IsNullOrEmpty(cashCustomerName)))
            {
                Session["CashCustomerName"] = cashCustomerName;
            }
            else
            {
                Session["CashCustomerName"] = null;
            }
            //////////////////
            if (cashCreditTypeCheckbox == true && !(string.IsNullOrEmpty(cashCreditType)))
            {
                Session["CashCreditType"] = cashCreditType;
            }
            else
            {
                Session["CashCreditType"] = null;
            }
            if (computerNoCheckbox == true && !(string.IsNullOrEmpty(computerNo)))
            {
                Session["ComputerNo"] = computerNo;
            }
            else
            {
                Session["ComputerNo"] = null;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PrintSaleInvoiceReports()
        {
            if (User != null)
            {
                decimal? customerId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string cashCustomerName = "";
                string cashCreditType = "";
                string computerNo = "";
                int invoiceType = 0;
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                }
                if (Session["CashCustomerName"] != null)
                {
                    cashCustomerName = Session["CashCustomerName"].ToString();
                }
                if (Session["CashCreditType"] != null)
                {
                    cashCreditType = Session["CashCreditType"].ToString();
                    if (cashCreditType == "Credit")
                    {
                        invoiceType = 1;
                    }
                    else
                    {
                        invoiceType = 0;
                    }
                }
                if (Session["ComputerNo"] != null)
                {
                    computerNo = Session["ComputerNo"].ToString();
                }
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Sale"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                if (customerId != 0)
                {
                    invoice = invoice.Where(x => x.DetailCode == customerId).ToList();
                }
                if (!(string.IsNullOrEmpty(cashCustomerName)))
                {
                    invoice = invoice.Where(x => x.CashCustomerName.ToLower().Contains(cashCustomerName.ToLower())).ToList();
                }
                if (!(string.IsNullOrEmpty(cashCreditType)))
                {
                    invoice = invoice.Where(x => x.InvoiceType == invoiceType).ToList();
                }
                if (!(string.IsNullOrEmpty(computerNo)))
                {
                    invoice = invoice.Where(x => x.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                if (fromDate != null && toDate != null)
                {
                    invoice = invoice.Where(x => x.RecordDate >= fromDate && x.RecordDate <= toDate).ToList();
                }
                invoice = invoice.OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
               foreach(clsInvoice j in invoice)
               {
                   j.InvoicesList = (from f in db.Invoice_Main
                                     join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                                     where f.ComputerNo == j.ComputerNo
                                     select new clsInvoice
                                     {
                                         ComputerNo = f.ComputerNo,
                                         RecordDate = f.RecordDate,
                                         RefrenceNo = f.RefrenceNo,
                                         CreditDays = f.CreditDays,
                                         DiscountType = f.DiscountType ?? "",
                                         Discount = f.Discount,
                                         NetDiscount = f.NetDiscount,
                                         Overhead = f.Overhead,
                                         Freight = f.Freight,
                                         Remarks = f.Remarks,
                                         Received = f.Received,
                                         ReceivedDetailCode = f.ReceivedDetailCode,
                                         ReceivedAmount = f.ReceivedAmount ?? 0,
                                         ReceivedRemarks = f.ReceivedRemarks,
                                         BalanceAmount = f.BalanceAmount ?? 0,
                                         PreviousBalance = f.PreviousBalance ?? 0,
                                         GrossTotal = f.GrossTotal,
                                         NetAmount = f.NetAmount,
                                         Status = f.Status,
                                         DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                         Address = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Address).FirstOrDefault() ?? "",
                                         Phone = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Phone1).FirstOrDefault() ?? "",
                                         InvoiceType = f.InvoiceType,
                                         CashCustomerName = f.CashCustomerName ?? "",
                                         ShowBalance = f.ShowBalance,
                                         ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                         ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                                         Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                                         BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                                         BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                                         BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                                         //BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                                         //SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,

                                         //////////--------------Line Data-----------------/////////
                                         ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                         //Barcode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.UrduName).FirstOrDefault() ?? "",
                                         //BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                                         WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                         Rate = fd.Rate,
                                         Qty = fd.Qty,
                                         ItemNote = fd.ItemNote,
                                         LineDiscountType = fd.DiscountType ?? string.Empty,
                                         LineDiscount = fd.Discount ?? 0,
                                         Tax1Value = fd.Tax1Value,
                                         Tax2Value = fd.Tax2Value,
                                         LineNetRate = fd.NetRate,
                                         LineGrossAmount = fd.GrossValue,
                                         LineNetAmount = fd.NetValue
                                     }).ToList();
               }
                return View(invoice);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        
        #region Purchase Report
        public ActionResult PurchaseReportIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "PurchaseInvoiceReport"
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
        public ActionResult PrintPurchaseInvoiceReports()
        {
            if (User != null)
            {
                decimal? customerId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string cashCustomerName = "";
                string cashCreditType = "";
                string computerNo = "";
                int invoiceType = 0;
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                }
                if (Session["CashCustomerName"] != null)
                {
                    cashCustomerName = Session["CashCustomerName"].ToString();
                }
                if (Session["CashCreditType"] != null)
                {
                    cashCreditType = Session["CashCreditType"].ToString();
                    if (cashCreditType == "Credit")
                    {
                        invoiceType = 1;
                    }
                    else
                    {
                        invoiceType = 0;
                    }
                }
                if (Session["ComputerNo"] != null)
                {
                    computerNo = Session["ComputerNo"].ToString();
                }
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Purchase"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                if (customerId != 0)
                {
                    invoice = invoice.Where(x => x.DetailCode == customerId).ToList();
                }
                if (!(string.IsNullOrEmpty(cashCustomerName)))
                {
                    invoice = invoice.Where(x => x.CashCustomerName.ToLower().Contains(cashCustomerName.ToLower())).ToList();
                }
                if (!(string.IsNullOrEmpty(cashCreditType)))
                {
                    invoice = invoice.Where(x => x.InvoiceType == invoiceType).ToList();
                }
                if (!(string.IsNullOrEmpty(computerNo)))
                {
                    invoice = invoice.Where(x => x.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                if (fromDate != null && toDate != null)
                {
                    invoice = invoice.Where(x => x.RecordDate >= fromDate && x.RecordDate <= toDate).ToList();
                }
                invoice = invoice.OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                foreach (clsInvoice j in invoice)
                {
                    j.InvoicesList = (from f in db.Invoice_Main
                                      join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                                      where f.ComputerNo == j.ComputerNo
                                      select new clsInvoice
                                      {
                                          ComputerNo = f.ComputerNo,
                                          RecordDate = f.RecordDate,
                                          RefrenceNo = f.RefrenceNo,
                                          CreditDays = f.CreditDays,
                                          DiscountType = f.DiscountType ?? "",
                                          Discount = f.Discount,
                                          NetDiscount = f.NetDiscount,
                                          Overhead = f.Overhead,
                                          Freight = f.Freight,
                                          Remarks = f.Remarks,
                                          Received = f.Received,
                                          ReceivedDetailCode = f.ReceivedDetailCode,
                                          ReceivedAmount = f.ReceivedAmount ?? 0,
                                          ReceivedRemarks = f.ReceivedRemarks,
                                          BalanceAmount = f.BalanceAmount ?? 0,
                                          PreviousBalance = f.PreviousBalance ?? 0,
                                          GrossTotal = f.GrossTotal,
                                          NetAmount = f.NetAmount,
                                          Status = f.Status,
                                          DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                          Address = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Address).FirstOrDefault() ?? "",
                                          Phone = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Phone1).FirstOrDefault() ?? "",
                                          InvoiceType = f.InvoiceType,
                                          CashCustomerName = f.CashCustomerName ?? "",
                                          ShowBalance = f.ShowBalance,
                                          ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                          ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                                          Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                                          BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                                          BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                                          BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                                          //BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                                          //SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,

                                          //////////--------------Line Data-----------------/////////
                                          ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                          //Barcode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.UrduName).FirstOrDefault() ?? "",
                                          //BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                                          WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                          Rate = fd.Rate,
                                          Qty = fd.Qty,
                                          ItemNote = fd.ItemNote,
                                          LineDiscountType = fd.DiscountType ?? string.Empty,
                                          LineDiscount = fd.Discount ?? 0,
                                          Tax1Value = fd.Tax1Value,
                                          Tax2Value = fd.Tax2Value,
                                          LineNetRate = fd.NetRate,
                                          LineGrossAmount = fd.GrossValue,
                                          LineNetAmount = fd.NetValue
                                      }).ToList();
                }
                return View(invoice);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
    }
}