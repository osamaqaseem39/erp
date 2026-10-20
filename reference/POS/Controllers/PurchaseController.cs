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
    public class PurchaseController : BaseController
    {
        PosMBDb db = new PosMBDb();

        #region Goods Receipt Note
        public ActionResult GoodsReceiptNoteIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "GoodsReceiptNote"
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
                    ViewBag.ReceivedDetailCode = new SelectList(getReceivedDetailCodes(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult getMaxCodeForGoodsReceiptNote()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "GR-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Goods Receipt Note").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Goods Receipt Note").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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


        [HttpPost]
        public ActionResult InsertUpdateGoodsReceiptNote(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsInvoice> invoice = JsonConvert.DeserializeObject<List<clsInvoice>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");

            string invoiceNo = invoice[0].ComputerNo;
            if (invoice[0].IsNewOrEdit == "Edit")
            {
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID ,""});
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Goods Receipt Note", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, 0, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Updated";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
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
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, "" });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Goods Receipt Note", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName,0, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Saved";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
                    }
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



        public ActionResult DeleteGoodsReceiptNote(string id)
        {
            string message = "";
            bool status = false;
            List<clsInvoice> invoice = new List<clsInvoice>();
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0 ,""});
            string returnId = AddEditInvoice(id, "Goods Receipt Note", DateTime.Now, 0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Delete");
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Invoice Successfully Deleted";
            }
            else if (returnId == "-9999")
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this invoice first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }


        public ActionResult GoodsReceiptNoteList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }

        public ActionResult GetAllGoodsReceiptNote()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
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
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Goods Receipt Note"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                DiscountType = f.DiscountType,
                                                Discount = f.Discount,
                                                NetDiscount = f.NetDiscount,
                                                Overhead = f.Overhead,
                                                Freight = f.Freight,
                                                NetAmount = f.NetAmount,
                                                Received = f.Received,
                                                ReceivedDetailCode = f.ReceivedDetailCode,
                                                ReceivedAmount = f.ReceivedAmount,
                                                ReceivedRemarks = f.ReceivedRemarks,
                                                BalanceAmount = f.BalanceAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                PreviousBalance = f.PreviousBalance,
                                                CreditDays = f.CreditDays,
                                                FiscalYearID = f.FiscalYearID,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                                InvoiceTypeName = (f.InvoiceType == 1)?"Credit":"Cash",
                                                DetailName = (f.InvoiceType == 1) ? db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() : f.CashCustomerName,
                                                ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
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




        public ActionResult GetGoodsReceiptNoteById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           RecordDate = f.RecordDate,
                           RefrenceNo = f.RefrenceNo,
                           CreditDays = f.CreditDays,
                           DiscountType = f.DiscountType,
                           Discount = f.Discount,
                           NetDiscount = f.NetDiscount,
                           Overhead = f.Overhead,
                           Freight = f.Freight,
                           Remarks = f.Remarks,
                           Received = f.Received,
                           ReceivedDetailCode = f.ReceivedDetailCode,
                           ReceivedAmount = f.ReceivedAmount,
                           ReceivedRemarks = f.ReceivedRemarks,
                           BalanceAmount = f.BalanceAmount,
                           PreviousBalance = f.PreviousBalance,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           CashCustomerName = f.CashCustomerName,
                           InvoiceType = f.InvoiceType,
                           ShowBalance = f.ShowBalance,
                           ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetGoodsReceiptNoteDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 SerialNo = f.SerialNo,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                 ItemNote = f.ItemNote,
                                 LineDiscountType = f.DiscountType ?? string.Empty,
                                 LineDiscount = f.Discount ?? 0,
                                 Tax1ID = f.Tax1ID,
                                 Tax1Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax1ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax1Rate = f.Tax1Rate,
                                 Tax1Value = f.Tax1Value,
                                 Tax2ID = f.Tax2ID,
                                 Tax2Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax2ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax2Rate = f.Tax2Rate,
                                 Tax2Value = f.Tax2Value,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Goods Receipt Note Report
        public ActionResult GoodsReceiptNoteReport(string id)
        {
            if (User != null)
            {
                List<clsInvoice> invoice = new List<clsInvoice>();
                invoice = (from f in db.Invoice_Main
                           join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                           where f.ComputerNo == id
                           select new clsInvoice
                           {
                               ComputerNo = f.ComputerNo,
                               SerialNo = fd.SerialNo,
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
                               ReceivedAmount = f.ReceivedAmount,
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
                           }).OrderBy(x => x.SerialNo).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        
        #region Purchase Invoice
        public ActionResult PurchaseInvoiceIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "PurchaseInvoice"
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
                    ViewBag.ReceivedDetailCode = new SelectList(getReceivedDetailCodes(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }



        public ActionResult GetAllStoreNoteForInvoice(string type)
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var orderNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var referenceNo = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_StoreNoteForInvoice", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Type", SqlDbType.VarChar).Value = type;
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
                        RefrenceNo = dt["RefrenceNo"].ToString(),
                        Remarks = dt["Remarks"].ToString(),
                        NetAmount = Convert.ToDecimal(dt["NetAmount"]),
                        DetailName = dt["DetailName"].ToString(),

                    });
                }
                SqlConnection.ClearPool(con);
                //SEARCHING...
                if (!string.IsNullOrEmpty(orderNo))
                {
                    invoice = invoice.Where(c => c.ComputerNo.ToLower().Contains(orderNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(referenceNo))
                {
                    invoice = invoice.Where(c => c.RefrenceNo.ToLower().Contains(referenceNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(detailName))
                {
                    invoice = invoice.Where(c => c.DetailName.ToLower().Contains(detailName.ToLower())).ToList();
                }
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
        public ActionResult GetAllStoreNoteForInvoiceDetail(string id)
        {
            if (User != null)
            {
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_StoreNoteForInvoiceDetail", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar).Value = id;
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
                        DetailCode = Convert.ToDecimal(dt["DetailCode"]),
                        CompositKey = dt["CompositKey"].ToString(),
                        DetailName = dt["DetailName"].ToString(),
                        RecordDate = Convert.ToDateTime(dt["RecordDate"]),
                        RefrenceNo = dt["RefrenceNo"].ToString(),
                        ComputerNo = id,
                        InvoiceType = Convert.ToInt32(dt["InvoiceType"]),
                        ItemID = Convert.ToInt32(dt["ItemID"]),
                        ItemName = dt["ItemName"].ToString(),
                        ItemDescription = dt["ItemDescription"].ToString(),
                        WarehouseID = Convert.ToInt32(dt["WarehouseID"]),
                        WarehouseName = dt["WarehouseName"].ToString(),
                        Rate = Convert.ToDecimal(dt["Rate"]),
                        CostRate = Convert.ToDecimal(dt["Rate"]),
                        Qty = Convert.ToDecimal(dt["Qty"]),
                        LineDiscountType = dt["DiscountType"].ToString(),
                        LineDiscount = Convert.ToDecimal(dt["Discount"]),
                        Tax1ID = Convert.ToInt32(dt["Tax1ID"]),
                        Tax1Name = dt["TaxName"].ToString(),
                        Tax1Rate = Convert.ToDecimal(dt["Tax1Rate"]),
                        Tax1Value = Convert.ToDecimal(dt["Tax1Value"]),
                        LineNetRate = Convert.ToDecimal(dt["NetRate"]),
                        LineGrossAmount = Convert.ToDecimal(dt["GrossValue"]),
                        LineNetAmount = Convert.ToDecimal(dt["NetValue"]),
                    });
                }
                SqlConnection.ClearPool(con);
                //SEARCHING...
                var data = invoice.ToList();
                return Json(new { data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        [HttpPost]
        public ActionResult InsertUpdatePurchaseInvoice(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsInvoice> invoice = JsonConvert.DeserializeObject<List<clsInvoice>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");

            string invoiceNo = invoice[0].ComputerNo;
            if (invoice[0].IsNewOrEdit == "Edit")
            {
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, invoice[i].CostRate, invoice[i].WarehouseID, invoice[i].RefCompositKey });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Purchase", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Updated";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
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
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, invoice[i].CostRate, invoice[i].WarehouseID, invoice[i].RefCompositKey });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Purchase", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Saved";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
                    }
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
        private string AddEditInvoice(string invoiceNo, string type, DateTime? recordDate, decimal? detailCode,
                                    string referenceNo, string remarks, decimal? grossTotal, string discountType,
                                    decimal? discount, decimal? netDiscount, decimal? overHead, decimal? freight,
                                    decimal? netAmount, int? received, decimal? receivedDetailCode, decimal? receivedAmount, string receivedRemarks,
                                    decimal? balanceAmount, string status, int businessId, int? userId, int? lastUserId, DateTime? saveDate, DateTime? lastDate,
                                    decimal? previousBalance, int? creditDays, int fiscalYear, int extraOrNot, string transporter, string biltyNo, string packing,
                                    DateTime? biltyDate, int? biltyPending, int? salemanId, string reason, int? invoiceType, string cashCustomerName, int? showBalance, DataTable dt, string insertUpdateStatus)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateInvoice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ComputerNo", SqlDbType.VarChar, 25).Value = invoiceNo;
                        cmd.Parameters.Add("@Type", SqlDbType.VarChar, 25).Value = type;
                        cmd.Parameters.Add("@RecordDate", SqlDbType.Date).Value = recordDate;
                        cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = detailCode;
                        cmd.Parameters.Add("@RefrenceNo", SqlDbType.VarChar, 30).Value = referenceNo;
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 255).Value = remarks;
                        cmd.Parameters.Add("@GrossTotal", SqlDbType.Decimal).Value = grossTotal;
                        cmd.Parameters.Add("@DiscountType", SqlDbType.VarChar, 5).Value = discountType;
                        cmd.Parameters.Add("@Discount", SqlDbType.Decimal).Value = discount;
                        cmd.Parameters.Add("@NetDiscount", SqlDbType.Decimal).Value = netDiscount;
                        cmd.Parameters.Add("@Overhead", SqlDbType.Decimal).Value = overHead;
                        cmd.Parameters.Add("@Freight", SqlDbType.Decimal).Value = freight;
                        cmd.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = netAmount;
                        cmd.Parameters.Add("@Received", SqlDbType.Int).Value = received;
                        cmd.Parameters.Add("@ReceivedDetailCode", SqlDbType.Decimal).Value = receivedDetailCode;
                        cmd.Parameters.Add("@ReceivedAmount", SqlDbType.Decimal).Value = receivedAmount;
                        cmd.Parameters.Add("@ReceivedRemarks", SqlDbType.VarChar, 255).Value = receivedRemarks;
                        cmd.Parameters.Add("@BalanceAmount", SqlDbType.Decimal).Value = balanceAmount;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 30).Value = status;
                        cmd.Parameters.Add("@BusinessID", SqlDbType.Int).Value = businessId;
                        cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                        cmd.Parameters.Add("@LastUserID", SqlDbType.VarChar, 30).Value = lastUserId.ToString();
                        cmd.Parameters.Add("@SaveDateTime", SqlDbType.DateTime).Value = saveDate;
                        cmd.Parameters.Add("@LastEditDateTime", SqlDbType.DateTime).Value = lastDate;
                        cmd.Parameters.Add("@PreviousBalance", SqlDbType.Decimal).Value = previousBalance;
                        cmd.Parameters.Add("@CreditDays", SqlDbType.Int).Value = creditDays;
                        cmd.Parameters.Add("@FiscalYearID", SqlDbType.Int).Value = fiscalYear;
                        cmd.Parameters.Add("@ExtraOrNot", SqlDbType.Int).Value = extraOrNot;
                        cmd.Parameters.Add("@Transporter", SqlDbType.VarChar, 255).Value = transporter;
                        cmd.Parameters.Add("@BiltyNo", SqlDbType.VarChar, 50).Value = biltyNo;
                        cmd.Parameters.Add("@BiltyPacking", SqlDbType.VarChar, 50).Value = packing;
                        cmd.Parameters.Add("@BiltyDate", SqlDbType.Date).Value = biltyDate;
                        cmd.Parameters.Add("@BiltyPending", SqlDbType.Int).Value = biltyPending;
                        cmd.Parameters.Add("@SalesmanID", SqlDbType.Int).Value = salemanId;
                        cmd.Parameters.Add("@Reason", SqlDbType.VarChar, 200).Value = reason;
                        cmd.Parameters.Add("@InvoiceType", SqlDbType.Int).Value = invoiceType;
                        cmd.Parameters.Add("@CashCustomerName", SqlDbType.NVarChar).Value = cashCustomerName;
                        cmd.Parameters.Add("@ShowBalance", SqlDbType.Int).Value = showBalance;
                        cmd.Parameters.Add("@NoDueDateAlert", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@dtInvoiceDetail", SqlDbType.Structured).Value = dt;
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.NVarChar).Value = insertUpdateStatus;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 25).Direction = ParameterDirection.Output;
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
        public ActionResult GetPurchaseInvoiceById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           RecordDate = f.RecordDate,
                           RefrenceNo = f.RefrenceNo,
                           CreditDays = f.CreditDays,
                           DiscountType = f.DiscountType,
                           Discount = f.Discount,
                           NetDiscount = f.NetDiscount,
                           Overhead = f.Overhead,
                           Freight = f.Freight,
                           Remarks = f.Remarks,
                           Received = f.Received,
                           ReceivedDetailCode = f.ReceivedDetailCode,
                           ReceivedAmount = f.ReceivedAmount,
                           ReceivedRemarks = f.ReceivedRemarks,
                           BalanceAmount = f.BalanceAmount,
                           PreviousBalance = f.PreviousBalance,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           CashCustomerName = f.CashCustomerName,
                           InvoiceType = f.InvoiceType,
                           ShowBalance = f.ShowBalance,
                           DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                           ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPurchaseInvoiceDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 SerialNo = f.SerialNo,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 CostRate = f.CostRate,
                                 Qty = f.Qty,
                                 ItemNote = f.ItemNote,
                                 LineDiscountType = f.DiscountType ?? string.Empty,
                                 LineDiscount = f.Discount ?? 0,
                                 Tax1ID = f.Tax1ID,
                                 Tax1Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax1ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax1Rate = f.Tax1Rate,
                                 Tax1Value = f.Tax1Value,
                                 Tax2ID = f.Tax2ID,
                                 Tax2Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax2ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax2Rate = f.Tax2Rate,
                                 Tax2Value = f.Tax2Value,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue,
                                 CompositKey = f.RefCompositKey ?? "0",
                                 HiddenComputerNo = db.Invoice_Detail.Where(x => x.CompositKey == f.RefCompositKey).Select(x => x.ComputerNo).FirstOrDefault() ?? "0"
                             }).OrderBy(x=>x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetSessionsForPurchaseList(bool? customerCheckbox, decimal? customerId, bool? dateCheckbox, DateTime? fromDate,
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
        
        public ActionResult PurchaseInvoiceList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllPurchaseInvoices()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                decimal? customerId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string computerNo = "";
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                }
                if (Session["ComputerNo"] != null)
                {
                    computerNo = Session["ComputerNo"].ToString();
                }
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Purchase"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                DiscountType = f.DiscountType,
                                                Discount = f.Discount,
                                                NetDiscount = f.NetDiscount,
                                                Overhead = f.Overhead,
                                                Freight = f.Freight,
                                                NetAmount = f.NetAmount,
                                                Received = f.Received,
                                                ReceivedDetailCode = f.ReceivedDetailCode,
                                                ReceivedAmount = f.ReceivedAmount,
                                                ReceivedRemarks = f.ReceivedRemarks,
                                                BalanceAmount = f.BalanceAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                PreviousBalance = f.PreviousBalance,
                                                CreditDays = f.CreditDays,
                                                FiscalYearID = f.FiscalYearID,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                                InvoiceTypeName = (f.InvoiceType == 1) ? "Credit" : "Cash",
                                                DetailName = (f.InvoiceType == 1) ? db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() : f.CashCustomerName,
                                                ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (customerId != 0)
                {
                    invoice = invoice.Where(x => x.DetailCode == customerId).ToList();
                }
                if (!(string.IsNullOrEmpty(computerNo)))
                {
                    invoice = invoice.Where(x => x.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                if (fromDate != null && toDate != null)
                {
                    invoice = invoice.Where(x => x.RecordDate >= fromDate && x.RecordDate <= toDate).ToList();
                }
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
        public ActionResult DeletePurchaseInvoice(string id)
        {
            string message = "";
            bool status = false;
            List<clsInvoice> invoice = new List<clsInvoice>();
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0,"" });
            string returnId = AddEditInvoice(id, "Purchase", DateTime.Now, 0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Delete");
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Invoice Successfully Deleted";
            }
            else if (returnId == "-9999")
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this invoice first.";
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
                      where c.Status == "Active" && (c.Type == "Supplier" || c.Type == "Customer + Supplier")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).OrderBy(x=>x.Name).ToList();
            return detail;
        }
        public List<clsAccountDetail> getReceivedDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      join ac in db.AC_DefaultGL on c.AccountCode equals ac.AccountCode
                      where c.Status == "Active" && (ac.Type == "Cash" || ac.Type == "Bank")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name
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
        public List<clsItem> getItems()
        {
            List<clsItem> item = new List<clsItem>();
            item = (from c in db.Item_Information
                    where c.Status == "Active"
                    select new clsItem
                    {
                        ItemID = c.ItemID,
                        ItemName = c.ItemName
                    }).ToList();
            return item;
        }
        public List<clsWarehouse> getWarehouses()
        {
            List<clsWarehouse> warehouse = new List<clsWarehouse>();
            warehouse = (from c in db.Admin_Setup_Warehouse
                         where c.Status == "Active"
                         select new clsWarehouse
                         {
                             WarehouseID = c.WarehouseID,
                             WarehouseName = c.WarehouseName
                         }).ToList();
            return warehouse;
        }
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
        public ActionResult getCreditDaysAndPreviousBalance(string voucherNo,decimal detailCode, string code)
        {
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn2 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn2", typeof(decimal));
            db.sp_GetCreditDaysAndPreviousBalance(voucherNo , detailCode, DateTime.Now, code, 0, User.FiscalYearId, CheckReturn, CheckReturn2);
            decimal creditDays = Convert.ToDecimal(CheckReturn.Value);
            decimal previousBalance = Convert.ToDecimal(CheckReturn2.Value);
            return Json(new { creditDays = creditDays, previousBalance = previousBalance }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getItemInformation(int itemId)
        {
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("spGet_ItemInformation", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
            cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = 0;
            cmd.Parameters.Add("@CashName", SqlDbType.VarChar, 200).Value = "";
            SqlDataReader r = cmd.ExecuteReader();
            clsItem item = new clsItem();
            while (r.Read())
            {
                item.ItemID = Convert.ToInt32(r[0].ToString());
                item.ItemName = r[1].ToString();
                item.UrduName = r[2].ToString();
                item.CostPrice = Convert.ToDecimal(r[3].ToString());
                item.SalePrice = Convert.ToDecimal(r[4].ToString());
                item.UnitName = r[5].ToString();
                item.CategoryName = r[6].ToString();
                item.BrandName = r[7].ToString();
                item.SaleRate = Convert.ToDecimal(r[8].ToString());
                item.PurchaseRate = Convert.ToDecimal(r[9].ToString());
                item.AverageRate = Convert.ToDecimal(r[10].ToString());
                item.SaleDate = Convert.ToDateTime(r[11]);
                item.SubCategoryName = r[12].ToString();
            }
            r.Close();
            con.Close();
            SqlConnection.ClearPool(con);
            var data = item;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getItemStock(string invoiceNo, int itemId, int warehouseId, string code)
        {
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            db.sp_GetItemStock(invoiceNo,itemId, code, warehouseId, 0, User.FiscalYearId, CheckReturn);
            decimal stock = Convert.ToDecimal(CheckReturn.Value);
            return Json(new { stock = stock }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getMaxCodeForPurchaseInvoice()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "PI-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Purchase").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Purchase").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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
        #region Purchase Invoice Report
        public ActionResult PurchaseInvoiceReport(string id)
        {
            if (User != null)
            {
                List<clsInvoice> invoice = new List<clsInvoice>();
                invoice = (from f in db.Invoice_Main
                           join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                           where f.ComputerNo == id
                           select new clsInvoice
                           {
                               ComputerNo = f.ComputerNo,
                               SerialNo = fd.SerialNo,
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
                               ReceivedAmount = f.ReceivedAmount,
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
                           }).OrderBy(x => x.SerialNo).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        
        #region Purchase Return Note
        public ActionResult PurchaseReturnNoteIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "PurchaseReturn"
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
                    ViewBag.ReceivedDetailCode = new SelectList(getReceivedDetailCodes(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult getMaxCodeForPurchaseReturnNote()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "PR-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Purchase Return").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Purchase Return").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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


        [HttpPost]
        public ActionResult InsertUpdatePurchaseReturnNote(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsInvoice> invoice = JsonConvert.DeserializeObject<List<clsInvoice>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");

            string invoiceNo = invoice[0].ComputerNo;
            if (invoice[0].IsNewOrEdit == "Edit")
            {
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, "" });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Purchase Return", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, 0, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Updated";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
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
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, "" });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Purchase Return", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, 0, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Saved";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
                    }
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



        public ActionResult DeletePurchaseReturnNote(string id)
        {
            string message = "";
            bool status = false;
            List<clsInvoice> invoice = new List<clsInvoice>();
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0,"" });
            string returnId = AddEditInvoice(id, "Purchase Return", DateTime.Now, 0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Delete");
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Invoice Successfully Deleted";
            }
            else if (returnId == "-9999")
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this invoice first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }


        public ActionResult PurchaseReturnNoteList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }

        public ActionResult GetAllPurchaseReturnNote()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
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
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Purchase Return"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                DiscountType = f.DiscountType,
                                                Discount = f.Discount,
                                                NetDiscount = f.NetDiscount,
                                                Overhead = f.Overhead,
                                                Freight = f.Freight,
                                                NetAmount = f.NetAmount,
                                                Received = f.Received,
                                                ReceivedDetailCode = f.ReceivedDetailCode,
                                                ReceivedAmount = f.ReceivedAmount,
                                                ReceivedRemarks = f.ReceivedRemarks,
                                                BalanceAmount = f.BalanceAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                PreviousBalance = f.PreviousBalance,
                                                CreditDays = f.CreditDays,
                                                FiscalYearID = f.FiscalYearID,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                                InvoiceTypeName = (f.InvoiceType == 1) ? "Credit" : "Cash",
                                                DetailName = (f.InvoiceType == 1) ? db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() : f.CashCustomerName,
                                                ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
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




        public ActionResult GetPurchaseReturnNoteById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           RecordDate = f.RecordDate,
                           RefrenceNo = f.RefrenceNo,
                           CreditDays = f.CreditDays,
                           DiscountType = f.DiscountType,
                           Discount = f.Discount,
                           NetDiscount = f.NetDiscount,
                           Overhead = f.Overhead,
                           Freight = f.Freight,
                           Remarks = f.Remarks,
                           Received = f.Received,
                           ReceivedDetailCode = f.ReceivedDetailCode,
                           ReceivedAmount = f.ReceivedAmount,
                           ReceivedRemarks = f.ReceivedRemarks,
                           BalanceAmount = f.BalanceAmount,
                           PreviousBalance = f.PreviousBalance,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           CashCustomerName = f.CashCustomerName,
                           InvoiceType = f.InvoiceType,
                           ShowBalance = f.ShowBalance,
                           ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPurchaseReturnNoteDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 SerialNo = f.SerialNo,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                 ItemNote = f.ItemNote,
                                 LineDiscountType = f.DiscountType ?? string.Empty,
                                 LineDiscount = f.Discount ?? 0,
                                 Tax1ID = f.Tax1ID,
                                 Tax1Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax1ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax1Rate = f.Tax1Rate,
                                 Tax1Value = f.Tax1Value,
                                 Tax2ID = f.Tax2ID,
                                 Tax2Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax2ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax2Rate = f.Tax2Rate,
                                 Tax2Value = f.Tax2Value,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Purchase Return Note Report
        public ActionResult PurchaseReturnNoteReport(string id)
        {
            if (User != null)
            {
                List<clsInvoice> invoice = new List<clsInvoice>();
                invoice = (from f in db.Invoice_Main
                           join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                           where f.ComputerNo == id
                           select new clsInvoice
                           {
                               ComputerNo = f.ComputerNo,
                               SerialNo = fd.SerialNo,
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
                               ReceivedAmount = f.ReceivedAmount,
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
                           }).OrderBy(x => x.SerialNo).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        
        #region Debit Note
        public ActionResult DebitNoteIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "DebitNote"
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
                    ViewBag.ReceivedDetailCode = new SelectList(getReceivedDetailCodes(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        [HttpPost]
        public ActionResult InsertUpdateDebitNote(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsInvoice> invoice = JsonConvert.DeserializeObject<List<clsInvoice>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");

            string invoiceNo = invoice[0].ComputerNo;
            if (invoice[0].IsNewOrEdit == "Edit")
            {
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, invoice[i].RefCompositKey });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Debit Note", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Updated";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
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
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, invoice[i].RefCompositKey });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Debit Note", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Saved";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
                    }
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
        
        public ActionResult GetDebitNoteById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           RecordDate = f.RecordDate,
                           RefrenceNo = f.RefrenceNo,
                           CreditDays = f.CreditDays,
                           DiscountType = f.DiscountType,
                           Discount = f.Discount,
                           NetDiscount = f.NetDiscount,
                           Overhead = f.Overhead,
                           Freight = f.Freight,
                           Remarks = f.Remarks,
                           Received = f.Received,
                           ReceivedDetailCode = f.ReceivedDetailCode,
                           ReceivedAmount = f.ReceivedAmount,
                           ReceivedRemarks = f.ReceivedRemarks,
                           BalanceAmount = f.BalanceAmount,
                           PreviousBalance = f.PreviousBalance,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           CashCustomerName = f.CashCustomerName,
                           InvoiceType = f.InvoiceType,
                           ShowBalance = f.ShowBalance,
                           DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                           ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDebitNoteDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 SerialNo = f.SerialNo,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                 ItemNote = f.ItemNote,
                                 LineDiscountType = f.DiscountType ?? string.Empty,
                                 LineDiscount = f.Discount ?? 0,
                                 Tax1ID = f.Tax1ID,
                                 Tax1Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax1ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax1Rate = f.Tax1Rate,
                                 Tax1Value = f.Tax1Value,
                                 Tax2ID = f.Tax2ID,
                                 Tax2Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax2ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax2Rate = f.Tax2Rate,
                                 Tax2Value = f.Tax2Value,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue,
                                 CompositKey = f.RefCompositKey ?? "0",
                                 HiddenComputerNo = db.Invoice_Detail.Where(x => x.CompositKey == f.RefCompositKey).Select(x => x.ComputerNo).FirstOrDefault() ?? "0"
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DebitNoteList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllDebitNotes()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                decimal? customerId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string computerNo = "";
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                }
                if (Session["ComputerNo"] != null)
                {
                    computerNo = Session["ComputerNo"].ToString();
                }
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Debit Note"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                DiscountType = f.DiscountType,
                                                Discount = f.Discount,
                                                NetDiscount = f.NetDiscount,
                                                Overhead = f.Overhead,
                                                Freight = f.Freight,
                                                NetAmount = f.NetAmount,
                                                Received = f.Received,
                                                ReceivedDetailCode = f.ReceivedDetailCode,
                                                ReceivedAmount = f.ReceivedAmount,
                                                ReceivedRemarks = f.ReceivedRemarks,
                                                BalanceAmount = f.BalanceAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                PreviousBalance = f.PreviousBalance,
                                                CreditDays = f.CreditDays,
                                                FiscalYearID = f.FiscalYearID,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                                InvoiceTypeName = (f.InvoiceType == 1) ? "Credit" : "Cash",
                                                DetailName = (f.InvoiceType == 1) ? db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() : f.CashCustomerName,
                                                ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (customerId != 0)
                {
                    invoice = invoice.Where(x => x.DetailCode == customerId).ToList();
                }
                if (!(string.IsNullOrEmpty(computerNo)))
                {
                    invoice = invoice.Where(x => x.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                if (fromDate != null && toDate != null)
                {
                    invoice = invoice.Where(x => x.RecordDate >= fromDate && x.RecordDate <= toDate).ToList();
                }
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
        public ActionResult DeleteDebitNote(string id)
        {
            string message = "";
            bool status = false;
            List<clsInvoice> invoice = new List<clsInvoice>();
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0,"" });
            string returnId = AddEditInvoice(id, "Debit Note", DateTime.Now, 0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Delete");
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Invoice Successfully Deleted";
            }
            else if (returnId == "-9999")
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this invoice first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        
        public ActionResult getMaxCodeForDebitNote()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "DN-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Debit Note").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Debit Note").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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
        #region Debit Note Report
        public ActionResult DebitNoteReport(string id)
        {
            if (User != null)
            {
                List<clsInvoice> invoice = new List<clsInvoice>();
                invoice = (from f in db.Invoice_Main
                           join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                           where f.ComputerNo == id
                           select new clsInvoice
                           {
                               ComputerNo = f.ComputerNo,
                               SerialNo = fd.SerialNo,
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
                               ReceivedAmount = f.ReceivedAmount,
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
                           }).OrderBy(x => x.SerialNo).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion

        #region Reports
        #region Purchase Schedule
        public List<clsItem> getCategories()
        {
            List<clsItem> category = new List<clsItem>();
            category = (from c in db.Item_Category
                        where c.Status == "Active"
                        select new clsItem
                        {
                            CategoryID = c.CategoryID,
                            CategoryName = c.Description
                        }).ToList();
            return category;
        }
        public List<clsItem> getSubCategories()
        {
            List<clsItem> subcategory = new List<clsItem>();
            subcategory = (from c in db.Item_SubCategory
                           where c.Status == "Active"
                           select new clsItem
                           {
                               SubCategoryID = c.SubCategoryID,
                               SubCategoryName = c.Description
                           }).ToList();
            return subcategory;
        }
        public ActionResult PurchaseSchedule()
        {
            ViewBag.Suppliers = new SelectList(getSuppliers(), "DetailCode", "Name");
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.SubCategory = new SelectList(getSubCategories(), "SubCategoryID", "SubCategoryName");

            return View();
        }
        [HttpPost]
        public ActionResult PrintPurchaseScheduleReport()
        {
            if (User != null)
            {
                String[] sup = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                String[] items = Request.Form.GetValues("ItemID");
                //int itemID = Convert.ToInt32(Request.Form["ItemID"]);
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                string reportType = Request.Form["ReportType"].ToString();
                List<clsInvoice> invoiceList = new List<clsInvoice>();

                int categoryid = Convert.ToInt32(Request.Form["CategoryID"]);
                int subcategoryid = Convert.ToInt32(Request.Form["SubCategoryID"]);

                if (sup != null)
                {
                    for (int i = 0; i < sup.Length; i++)
                    {
                        decimal supplierId = Convert.ToDecimal(sup[i]);
                        List<clsInvoice> list = GetInvoicesListForPurchaseSchedule(supplierId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }
                else
                {
                    List<clsAccountDetail> suppliers = getSuppliers();
                    for (int i = 0; i < suppliers.Count; i++)
                    {
                        decimal supplierId = Convert.ToDecimal(suppliers[i].DetailCode);
                        List<clsInvoice> list = GetInvoicesListForPurchaseSchedule(supplierId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }


                List<clsInvoice> finalList = new List<clsInvoice>(); 
                if (categoryid != null && categoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = invoiceList.Where(x => x.CategoryID == categoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                else
                {
                    finalList = invoiceList;
                }
                if (subcategoryid != null && subcategoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.SubCategoryID == subcategoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                                

                invoiceList = invoiceList.OrderBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ThenBy(x => x.SerialNo).ToList();
                return View(invoiceList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetInvoicesListForPurchaseSchedule(decimal supplierId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, String[] items, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> invoiceList = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Purchase Invoice + Purchase Return")
            {
                type1 = "Purchase";
                type2 = "Purchase Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (items == null && warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == supplierId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            RecordDate = f.RecordDate,
                            RefrenceNo = f.RefrenceNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                            ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                            SerialNo = fd.SerialNo,
                            Qty = fd.Qty ?? 0,
                            UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            Rate = fd.Rate ?? 0,
                            GrossValue = fd.GrossValue ?? 0,
                            NetValue = fd.NetValue ?? 0,
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).ToList();
                invoiceList.AddRange(list);
            }
            else if (items != null && warehouseId == 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == supplierId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && (f.Type == type1 || f.Type == type2)
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                SerialNo = fd.SerialNo,
                                Qty = fd.Qty ?? 0,
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                Rate = fd.Rate ?? 0,
                                GrossValue = fd.GrossValue ?? 0,
                                NetValue = fd.NetValue ?? 0,
                                SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).ToList();
                    invoiceList.AddRange(list);
                }
            }
            else if (items == null && warehouseId != 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == supplierId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && fd.WarehouseID == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            RecordDate = f.RecordDate,
                            RefrenceNo = f.RefrenceNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                            ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                            SerialNo = fd.SerialNo,
                            Qty = fd.Qty ?? 0,
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            Rate = fd.Rate ?? 0,
                            GrossValue = fd.GrossValue ?? 0,
                            NetValue = fd.NetValue ?? 0,
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).ToList();
                invoiceList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == supplierId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && fd.WarehouseID == warehouseId
                            && (f.Type == type1 || f.Type == type2)
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                SerialNo = fd.SerialNo,
                                Qty = fd.Qty ?? 0,
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                Rate = fd.Rate ?? 0,
                                GrossValue = fd.GrossValue ?? 0,
                                NetValue = fd.NetValue ?? 0,
                                SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).ToList();
                    invoiceList.AddRange(list);
                }
            }
            if (salesmanId != 0)
            {
                invoiceList = invoiceList.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return invoiceList;
        }
        public List<clsAccountDetail> getSuppliers()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && (c.Type == "Supplier" || c.Type == "Customer + Supplier")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).OrderBy(x=>x.Name).ToList();
            return detail;
        }
        #endregion
        #region Purchase Invoice Summary
        public ActionResult PurchaseInvoiceSummary()
        {
            ViewBag.Suppliers = new SelectList(getSuppliers(), "DetailCode", "Name");
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintPurchaseInvoiceSummaryReport()
        {
            if (User != null)
            {
                String[] sup = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                string reportType = Request.Form["ReportType"].ToString();
                List<clsInvoice> invoiceList = new List<clsInvoice>();

                if (sup != null)
                {
                    for (int i = 0; i < sup.Length; i++)
                    {
                        decimal supplierId = Convert.ToDecimal(sup[i]);
                        List<clsInvoice> list = GetInvoicesListForPurchaseInvoiceSummary(supplierId, fromDate, toDate, referenceNo, computerNo, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }
                else
                {
                    List<clsAccountDetail> suppliers = getSuppliers();
                    for (int i = 0; i < suppliers.Count; i++)
                    {
                        decimal supplierId = Convert.ToDecimal(suppliers[i].DetailCode);
                        List<clsInvoice> list = GetInvoicesListForPurchaseInvoiceSummary(supplierId, fromDate, toDate, referenceNo, computerNo, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }
                invoiceList = invoiceList.OrderBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ToList();
                return View(invoiceList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetInvoicesListForPurchaseInvoiceSummary(decimal supplierId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Purchase Invoice + Purchase Return")
            {
                type1 = "Purchase";
                type2 = "Purchase Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        where
                        f.DetailCode == supplierId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            RecordDate = f.RecordDate,
                            RefrenceNo = f.RefrenceNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                            Remarks = f.Remarks,
                            BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            NetAmount = f.NetAmount,
                            ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                            ReceivedAmount = f.ReceivedAmount ?? 0,
                            BalanceAmount = f.BalanceAmount ?? 0
                        }).ToList();
            }
            else
            {
                list = (from f in db.Invoice_Main
                        where
                        f.DetailCode == supplierId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && db.Invoice_Detail.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.WarehouseID).FirstOrDefault() == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            RecordDate = f.RecordDate,
                            RefrenceNo = f.RefrenceNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                            Remarks = f.Remarks,
                            BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            NetAmount = f.NetAmount,
                            ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                            ReceivedAmount = f.ReceivedAmount ?? 0,
                            BalanceAmount = f.BalanceAmount ?? 0
                        }).ToList();
            }
            if (salesmanId != 0)
            {
                list = list.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return list;
        }
        #endregion
        #region Purchase Summary Supplier Item
        public ActionResult PurchaseSummarySupplierItem()
        {
            ViewBag.Suppliers = new SelectList(getSuppliers(), "DetailCode", "Name");
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.SubCategory = new SelectList(getSubCategories(), "SubCategoryID", "SubCategoryName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintPurchaseSummarySupplierItemReport()
        {
            if (User != null)
            {
                String[] sup = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                String[] items = Request.Form.GetValues("ItemID");
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                string reportType = Request.Form["ReportType"].ToString();


                int categoryid = Convert.ToInt32(Request.Form["CategoryID"]);
                int subcategoryid = Convert.ToInt32(Request.Form["SubCategoryID"]);

                List<clsInvoice> invoiceList = new List<clsInvoice>();
                clsInvoice invoice = new clsInvoice();
                clsAccountDetail supplier;
                if (sup != null)
                {
                    for (int i = 0; i < sup.Length; i++)
                    {
                        supplier = new clsAccountDetail();
                        decimal supplierId = Convert.ToDecimal(sup[i]);
                        supplier.DetailCode = supplierId;
                        supplier.Name = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault();
                        supplier.CityName = (from c in db.AC_Detail_Extra join city in db.Cities on c.CityId equals city.CityId where c.DetailCode == supplierId select city.CityName).FirstOrDefault();
                        invoice.DetailList.Add(supplier);
                        List<clsInvoice> list = GetInvoicesListForPurchaseSummarySupplierItem(supplierId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);


                    }
                }
                else
                {
                    List<clsAccountDetail> suppliers = getSuppliers();
                    for (int i = 0; i < suppliers.Count; i++)
                    {
                        supplier = new clsAccountDetail();
                        decimal supplierId = Convert.ToDecimal(suppliers[i].DetailCode);
                        supplier.DetailCode = supplierId;
                        supplier.Name = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault();
                        supplier.CityName = (from c in db.AC_Detail_Extra join city in db.Cities on c.CityId equals city.CityId where c.DetailCode == supplierId select city.CityName).FirstOrDefault();
                        invoice.DetailList.Add(supplier);
                        List<clsInvoice> list = GetInvoicesListForPurchaseSummarySupplierItem(supplierId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }

                List<clsInvoice> finalList = new List<clsInvoice>();
                if (categoryid != null && categoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = invoiceList.Where(x => x.CategoryID == categoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                else
                {
                    finalList = invoiceList;
                }
                if (subcategoryid != null && subcategoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.SubCategoryID == subcategoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                                

                invoice.InvoicesList = invoiceList;
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetInvoicesListForPurchaseSummarySupplierItem(decimal supplierId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, String[] items, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> invoiceList = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Purchase Invoice + Purchase Return")
            {
                type1 = "Purchase";
                type2 = "Purchase Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (items == null && warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == supplierId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = supplierId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else if (items != null && warehouseId == 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == supplierId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = supplierId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            else if (items == null && warehouseId != 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == supplierId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && fd.WarehouseID == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = supplierId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == supplierId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && fd.WarehouseID == warehouseId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = supplierId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            if (salesmanId != 0)
            {
                invoiceList = invoiceList.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return invoiceList;
        }
        #endregion
        #region Purchase Summary Item Supplier
        public ActionResult PurchaseSummaryItemSupplier()
        {
            ViewBag.Suppliers = new SelectList(getSuppliers(), "DetailCode", "Name");
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.SubCategory = new SelectList(getSubCategories(), "SubCategoryID", "SubCategoryName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintPurchaseSummaryItemSupplierReport()
        {
            if (User != null)
            {
                String[] sup = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                String[] items = Request.Form.GetValues("ItemID");
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                string reportType = Request.Form["ReportType"].ToString();

                int categoryid = Convert.ToInt32(Request.Form["CategoryID"]);
                int subcategoryid = Convert.ToInt32(Request.Form["SubCategoryID"]);

                List<clsInvoice> invoiceList = new List<clsInvoice>();
                clsInvoice invoice = new clsInvoice();
                if (sup != null)
                {
                    for (int i = 0; i < sup.Length; i++)
                    {
                        decimal supplierId = Convert.ToDecimal(sup[i]);
                        List<clsInvoice> list = GetInvoicesListForPurchaseSummaryItemSupplier(supplierId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }
                else
                {
                    List<clsAccountDetail> suppliers = getSuppliers();
                    for (int i = 0; i < suppliers.Count; i++)
                    {
                        decimal supplierId = Convert.ToDecimal(suppliers[i].DetailCode);
                        List<clsInvoice> list = GetInvoicesListForPurchaseSummaryItemSupplier(supplierId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }


                List<clsInvoice> finalList = new List<clsInvoice>();
                if (categoryid != null && categoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = invoiceList.Where(x => x.CategoryID == categoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                else
                {
                    finalList = invoiceList;
                }
                if (subcategoryid != null && subcategoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.SubCategoryID == subcategoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                                

                invoice.InvoicesList = invoiceList;
                invoice.InvoicesList2 = invoiceList.GroupBy(x => x.ItemID).Select(g => g.First()).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetInvoicesListForPurchaseSummaryItemSupplier(decimal supplierId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, String[] items, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> invoiceList = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Purchase Invoice + Purchase Return")
            {
                type1 = "Purchase";
                type2 = "Purchase Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (items == null && warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == supplierId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = supplierId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else if (items != null && warehouseId == 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == supplierId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = supplierId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            else if (items == null && warehouseId != 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == supplierId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && fd.WarehouseID == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = supplierId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == supplierId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && fd.WarehouseID == warehouseId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = supplierId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == supplierId select ct.CityName).FirstOrDefault() ?? "",
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            if (salesmanId != 0)
            {
                invoiceList = invoiceList.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return invoiceList;
        }
        #endregion
        #region Purchase Summary Item
        public ActionResult PurchaseSummaryItem()
        {
            ViewBag.Suppliers = new SelectList(getSuppliers(), "DetailCode", "Name");
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.SubCategory = new SelectList(getSubCategories(), "SubCategoryID", "SubCategoryName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintPurchaseSummaryItemReport()
        {
            if (User != null)
            {
                String[] sup = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                String[] items = Request.Form.GetValues("ItemID");
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                string reportType = Request.Form["ReportType"].ToString();

                int categoryid = Convert.ToInt32(Request.Form["CategoryID"]);
                int subcategoryid = Convert.ToInt32(Request.Form["SubCategoryID"]);

                List<clsInvoice> invoiceList = new List<clsInvoice>();
                clsInvoice invoice = new clsInvoice();
                if (sup != null)
                {
                    for (int i = 0; i < sup.Length; i++)
                    {
                        decimal supplierId = Convert.ToDecimal(sup[i]);
                        List<clsInvoice> list = GetInvoicesListForPurchaseSummaryItem(supplierId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }
                else
                {
                    List<clsAccountDetail> suppliers = getSuppliers();
                    for (int i = 0; i < suppliers.Count; i++)
                    {
                        decimal supplierId = Convert.ToDecimal(suppliers[i].DetailCode);
                        List<clsInvoice> list = GetInvoicesListForPurchaseSummaryItem(supplierId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }

                List<clsInvoice> finalList = new List<clsInvoice>();
                if (categoryid != null && categoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = invoiceList.Where(x => x.CategoryID == categoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                else
                {
                    finalList = invoiceList;
                }
                if (subcategoryid != null && subcategoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.SubCategoryID == subcategoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                                


                invoice.InvoicesList = invoiceList;
                invoice.InvoicesList2 = invoiceList.GroupBy(x => x.ItemID).Select(g => g.First()).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetInvoicesListForPurchaseSummaryItem(decimal supplierId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, String[] items, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> invoiceList = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Purchase Invoice + Purchase Return")
            {
                type1 = "Purchase";
                type2 = "Purchase Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (items == null && warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == supplierId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = supplierId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else if (items != null && warehouseId == 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == supplierId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = supplierId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            else if (items == null && warehouseId != 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == supplierId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && fd.WarehouseID == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = supplierId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == supplierId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && fd.WarehouseID == warehouseId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = supplierId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == supplierId).Select(x => x.Name).FirstOrDefault(),
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == g.FirstOrDefault().ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            if (salesmanId != 0)
            {
                invoiceList = invoiceList.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return invoiceList;
        }
        #endregion
        #endregion
    }
}