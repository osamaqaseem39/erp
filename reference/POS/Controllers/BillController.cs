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
using System.IO;
using System.Net;

namespace POS.Controllers
{
    public class BillController : BaseController
    {
        PosMBDb db = new PosMBDb();


        #region Invoice
        public ActionResult BillIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SaleInvoice"
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
                    //ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                    ViewBag.ReceivedDetailCode = new SelectList(getReceivedDetailCodes(), "DetailCode", "Name");
                    //ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    //ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public ActionResult GetAllDoneJobs()
        {
            if (User != null)
            {
                string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
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

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                                            where f.Status.ToLower() == "done" && fs.Status.ToLower() == "done"
                                            select new clsBooking
                                            {
                                                RecordID = fs.RecordID,
                                                BookingId = f.BookingId,
                                                WorkSection = f.WorkSection ?? "",
                                                JobNo = f.JobNo ?? "",
                                                PaymentStatus = ((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus,
                                                PaybalEmail = f.PaybalEmail ?? "",
                                                CreditCardNo = f.CreditCardNo ?? "",
                                                PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
                                                CabId = f.CabId,
                                                CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabId).Select(x => x.Description).FirstOrDefault(),
                                                Startpoint = f.PickupAddress,
                                                Startpoint1 = f.PickupAddress1,
                                                Startpoint2 = f.PickupAddress2,
                                                Startpoint3 = f.PickupAddress3,
                                                Endposition = f.DropOffAddress,
                                                Endposition1 = f.DropOffAddress1,
                                                Endposition2 = f.DropOffAddress2,
                                                Endposition3 = f.DropOffAddress3,
                                                ManualPickupAddress = f.ManualPickupAddress ?? "",
                                                ManualDropOffAddress = f.ManualDropOffAddress ?? "",


                                                ServiceProviderID = f.ServiceProviderID ?? 0,
                                                ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                                PaymentMode = f.PaymentMode ?? "",
                                                SPRefNo = f.SPRefNo ?? "",
                                                SPAmount = f.SPAmount ?? 0,

                                                PickupDateTime = f.PickupDateTime,
                                                PickupTimeSpan = f.PickupTime,
                                                PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2) + "-" + f.PickupDateTime.Value.Year.ToString(),
                                                PickupTimeString = ("00" + f.PickupTime.Value.Hours.ToString()).Substring(("00" + f.PickupTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.PickupTime.Value.Minutes.ToString()).Substring(("00" + f.PickupTime.Value.Minutes.ToString()).Length - 2),
                                                NoOfAdult = f.NoofPassenger,
                                                NoOfChild = f.NoOfChild,
                                                SurName = f.SurName ?? "",
                                                PasengerName = f.PasengerName,
                                                Cellno = f.Cellno,
                                                Email = f.Email,
                                                Amount = f.Amount,
                                                EstimateFareNet = f.EstimateFareNet,
                                                TotalKm = f.TotalKm,
                                                ChildSeatStr = (f.ChildSeat == 1) ? "Y" : "N",
                                                DisableSeatStr = (f.DisableSeat == 1) ? "Y" : "N",
                                                AdditionalInfo = f.AdditionalInformation,
                                                ReturnTripStr = (f.ReturnTrip == 1) ? "Y" : "N",
                                                FlightNo = f.FlightNo,
                                                ArrivalFrom = f.ArrivalFrom,
                                                Terminal = f.FlightTerminal,
                                                FlightTimeString = ("00" + f.FlightTime.Value.Hours.ToString()).Substring(("00" + f.FlightTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.FlightTime.Value.Minutes.ToString()).Substring(("00" + f.FlightTime.Value.Minutes.ToString()).Length - 2),
                                                DoorNo = f.DoorNo,
                                                DetailName = db.AC_Detail.Where(x => x.DetailCode == fs.DetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                                
                                                MaxAmount = f.BidAmount ?? 0,
                                                SuppBookStatus = fs.Status,
                                                CreateDateTime = fs.CreateDateTime,
                                                BookDateTime = fs.BidDateTime,
                                                SuppBidAmount = fs.BidAmount ?? 0,
                                                SuppBidRemarks = fs.Remarks ?? "",
                                                CancelDateTime = fs.CancelDateTime,

                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                            }).OrderByDescending(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();


                //SEARCHING...
                if (!string.IsNullOrEmpty(orderNo))
                {
                    invoice = invoice.Where(c => c.JobNo.ToLower().Contains(orderNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(referenceNo))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(referenceNo.ToLower()) || c.Endposition.ToLower().Contains(referenceNo.ToLower())).ToList();
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



        public ActionResult BookingDetail(int? id)
        {
            
            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                             where fs.RecordID == id
                             select new clsBooking
                             {
                                 BookingId = f.BookingId,
                                 RecordID= fs.RecordID,
                                 WorkSection = f.WorkSection ?? "",
                                 JobNo = f.JobNo ?? "",
                                 PaymentStatus = ((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus,
                                 PaybalEmail = f.PaybalEmail ?? "",
                                 CreditCardNo = f.CreditCardNo ?? "",
                                 PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
                                 CabId = f.CabId,
                                 CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabId).Select(x => x.Description).FirstOrDefault(),
                                 Startpoint = f.PickupAddress,
                                 Endposition = f.DropOffAddress,
                                 PickupDateTime = f.PickupDateTime,
                                 PickupTimeSpan = f.PickupTime,
                                 PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2) + "-" + f.PickupDateTime.Value.Year.ToString(),
                                 PickupTimeString = ("00" + f.PickupTime.Value.Hours.ToString()).Substring(("00" + f.PickupTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.PickupTime.Value.Minutes.ToString()).Substring(("00" + f.PickupTime.Value.Minutes.ToString()).Length - 2),
                                 NoOfAdult = f.NoofPassenger,
                                 NoOfChild = f.NoOfChild,
                                 SurName = f.SurName ?? "",
                                 PasengerName = f.PasengerName,
                                 Cellno = f.Cellno,
                                 Email = f.Email,
                                 Amount = f.Amount,
                                 EstimateFareNet = f.EstimateFareNet,
                                 TotalKm = f.TotalKm,
                                 ChildSeat = f.ChildSeat,
                                 DisableSeat = f.DisableSeat,
                                 AdditionalInfo = f.AdditionalInformation,
                                 ReturnTrip = f.ReturnTrip,
                                 FlightNo = f.FlightNo,
                                 ArrivalFrom = f.ArrivalFrom,
                                 Terminal = f.FlightTerminal,
                                 FlightTimeString = ("00" + f.FlightTime.Value.Hours.ToString()).Substring(("00" + f.FlightTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.FlightTime.Value.Minutes.ToString()).Substring(("00" + f.FlightTime.Value.Minutes.ToString()).Length - 2),
                                 DoorNo = f.DoorNo,
                                 MeetGreet = f.MeetGreet ?? 0,

                                 MaxAmount = f.BidAmount ?? 0,
                                 ReturnDate = f.ReturnDate,
                                 ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                 LuggageID = f.LuggageID ?? 0,
                                 LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                 BidCloseDate = f.BidCloseDate,
                                 BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                 BidPostRemarks = f.BidPostRemarks ?? "",

                                 DetailCode=fs.DetailCode ?? 0,
                                 DetailName = db.AC_Detail.Where(x => x.DetailCode == fs.DetailCode).Select(x => x.Name).FirstOrDefault() ?? "",

                                 SuppBookStatus = fs.Status,
                                 CreateDateTime = fs.CreateDateTime,
                                 BookDateTime = fs.BidDateTime,
                                 SuppBidAmount = fs.BidAmount ?? 0,
                                 SuppBidRemarks = fs.Remarks ?? "",
                                 CancelDateTime = fs.CancelDateTime,
                             }).FirstOrDefault();

            return Json(new { data = bookingdetail },
                    JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult InsertUpdateBill(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsInvoice> invoice = JsonConvert.DeserializeObject<List<clsInvoice>>(data);
            
            string invoiceNo = invoice[0].ComputerNo;

            
                if (invoice[0].IsNewOrEdit == "Edit")
                {
                    returnId = AddEditInvoice(invoice[0].ComputerNo, "Invoice", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, "Update");
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
                    returnId = AddEditInvoice(invoice[0].ComputerNo, "Invoice", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, "Save");
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


        public ActionResult GetBillById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           SubDetailCode = f.SubDetailCode,
                           SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
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
        
        public ActionResult BillList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllBills()
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
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Invoice"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                SubDetailCode = f.SubDetailCode,
                                                SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
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
                                                Status = f.Status,

                                                PickupLocation=db.Cab_BookingMain.Where(x=>x.BookingId == f.CreditDays).Select(x=>x.PickupAddress).FirstOrDefault () ?? "",
                                                DropOffLocation = db.Cab_BookingMain.Where(x => x.BookingId == f.CreditDays).Select(x => x.DropOffAddress).FirstOrDefault() ?? "",

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
        public ActionResult DeleteDeleteBill(string id)
        {
            string message = "";
            bool status = false;
            List<clsInvoice> invoice = new List<clsInvoice>();

            string returnId = AddEditInvoice(id, "Invoice", DateTime.Now, 0, 0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, "Delete");
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

        public ActionResult getMaxCodeForBill()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "BL-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Invoice").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Invoice").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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

        #region Invoice Report
        public ActionResult BillReport(string id)
        {
            if (User != null)
            {
                List<clsInvoice> invoice = new List<clsInvoice>();
                invoice = (from f in db.Invoice_Main
                           join fd in db.Invoice_Extra on f.ComputerNo equals fd.ComputerNo
                           where f.ComputerNo == id
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
                               ReceivedAmount = f.ReceivedAmount,
                               ReceivedRemarks = f.ReceivedRemarks,
                               BalanceAmount = f.BalanceAmount ?? 0,
                               PreviousBalance = f.PreviousBalance ?? 0,
                               GrossTotal = f.GrossTotal,
                               NetAmount = f.NetAmount,
                               Status = f.Status,
                               SubDetailCode = f.SubDetailCode,
                               SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                               DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                               Address = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Address).FirstOrDefault() ?? "",
                               Phone = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Phone1).FirstOrDefault() ?? "",
                               InvoiceType = f.InvoiceType,
                               CashCustomerName = f.CashCustomerName ?? "",
                               ShowBalance = f.ShowBalance,
                               ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                               ExtraOrNot = 1,
                               Transporter = fd.Transporter ?? "",
                               BiltyNo = fd.BiltyNo ?? "",
                               BiltyPacking = fd.BiltyPacking ?? "",
                               BiltyDate = fd.BiltyDate ?? f.RecordDate,
                               BiltyPending = fd.BiltyPending ?? 0,

                               PickupLocation = db.Cab_BookingMain.Where(x => x.BookingId == f.CreditDays).Select(x => x.PickupAddress).FirstOrDefault() ?? "",
                               DropOffLocation = db.Cab_BookingMain.Where(x => x.BookingId == f.CreditDays).Select(x => x.DropOffAddress).FirstOrDefault() ?? "",

                           }).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion

        #region Misc
        private string AddEditInvoice(string invoiceNo, string type, DateTime? recordDate, decimal? detailCode,decimal? SubDetailCode,
                                   string referenceNo, string remarks, decimal? grossTotal, string discountType,
                                   decimal? discount, decimal? netDiscount, decimal? overHead, decimal? freight,
                                   decimal? netAmount, int? received, decimal? receivedDetailCode, decimal? receivedAmount, string receivedRemarks,
                                   decimal? balanceAmount, string status, int businessId, int? userId, int? lastUserId, DateTime? saveDate, DateTime? lastDate,
                                   decimal? previousBalance, int? creditDays, int fiscalYear, int extraOrNot, string transporter, string biltyNo, string packing,
                                   DateTime? biltyDate, int? biltyPending, int? salemanId, string reason, int? invoiceType, string cashCustomerName, int? showBalance, string insertUpdateStatus)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCabInvoice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ComputerNo", SqlDbType.VarChar, 25).Value = invoiceNo;
                        cmd.Parameters.Add("@Type", SqlDbType.VarChar, 25).Value = type;
                        cmd.Parameters.Add("@RecordDate", SqlDbType.Date).Value = recordDate;
                        cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = detailCode;
                        cmd.Parameters.Add("@SubDetailCode", SqlDbType.Decimal).Value = SubDetailCode;
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

        public ActionResult SetSessionsForSaleList(bool? customerCheckbox, decimal? customerId, bool? dateCheckbox, DateTime? fromDate,
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


        public List<clsAccountDetail> getDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && (c.Type == "Customer" || c.Type == "Customer + Supplier" || c.Type == "Supplier" || c.Type == "Driver")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).OrderBy(x => x.Name).ToList();
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
        public List<clsFooter> getFooters()
        {
            List<clsFooter> footer = new List<clsFooter>();
            footer = (from c in db.Footers
                      select new clsFooter
                      {
                          FooterId = c.FooterId,
                          FooterName = c.FooterName,
                          Status = c.Status ?? "In-Active"
                      }).OrderBy(x => x.Status).ToList();
            return footer;
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
        public ActionResult getCreditDaysAndPreviousBalance(string voucherNo, decimal detailCode, string code)
        {
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn2 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn2", typeof(decimal));
            db.sp_GetCreditDaysAndPreviousBalance(voucherNo, detailCode, DateTime.Now, code, 0, User.FiscalYearId, CheckReturn, CheckReturn2);
            decimal creditDays = Convert.ToDecimal(CheckReturn.Value);
            decimal previousBalance = Convert.ToDecimal(CheckReturn2.Value);
            return Json(new { creditDays = creditDays, previousBalance = previousBalance }, JsonRequestBehavior.AllowGet);
        }
        
        #endregion

    }
}