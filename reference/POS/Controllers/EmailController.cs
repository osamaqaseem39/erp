using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using POS.Models;
using System.Linq.Dynamic;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;


namespace POS.Controllers
{
    public class EmailController : BaseController
    {
        PosMBDb db = new PosMBDb();

        public string CreateEmailFunction(int? id, string emailtype, decimal? detailcode, string bidtype)
        {
            bool vresult = false;
            int tocustomer = db.Setup_EmailMessage.Where(x => x.Purpose.ToLower() == emailtype.ToLower()).Select(x => x.ToCustomer).FirstOrDefault() ?? 0;
            if (emailtype.ToLower() == "for bid" || emailtype.ToLower() == "assign job" || emailtype.ToLower() == "confirmed" || emailtype.ToLower() == "cancelled")
            {
                vresult = CreateEmailInfoMaterial(id, emailtype, "supplier", detailcode, bidtype);
            }
            else
            {
                vresult = CreateEmail(id, emailtype, "admin", detailcode);
            }
            if (tocustomer == 1)
            {
                vresult = CreateEmail(id, emailtype, "customer", detailcode);
            }            
            return "OK";
        }
        public clsBooking BookingDetail(int? id,decimal? detailcode)
        {

            TimeSpan PickupTime = db.Cab_BookingMain.Where(x => x.BookingId == id).Select(x => x.PickupTime).FirstOrDefault() ?? DateTime.Now.TimeOfDay;
            TimeSpan FlightTime = db.Cab_BookingMain.Where(x => x.BookingId == id).Select(x => x.FlightTime).FirstOrDefault() ?? DateTime.Now.TimeOfDay;
            TimeSpan ReturnTime = db.Cab_BookingMain.Where(x => x.BookingId == id).Select(x => x.ReturnTime).FirstOrDefault() ?? DateTime.Now.TimeOfDay;
            TimeSpan BidCloseTime = db.Cab_BookingMain.Where(x => x.BookingId == id).Select(x => x.BidCloseTime).FirstOrDefault() ?? DateTime.Now.TimeOfDay;
            DateTime BidCloseDate = db.Cab_BookingMain.Where(x => x.BookingId == id).Select(x => x.BidCloseDate).FirstOrDefault() ?? DateTime.Now;

            int cabtypeid = db.Cab_BookingMain.Where(x => x.BookingId == id).Select(x => x.CabId).FirstOrDefault() ?? 0;

            string PickupTimeString = ("00" + PickupTime.Hours.ToString()).Substring(("00" + PickupTime.Hours.ToString()).Length - 2) + ":" + ("00" + PickupTime.Minutes.ToString()).Substring(("00" + PickupTime.Minutes.ToString()).Length - 2);
            string FlightTimeString = ("00" + FlightTime.Hours.ToString()).Substring(("00" + FlightTime.Hours.ToString()).Length - 2) + ":" + ("00" + FlightTime.Minutes.ToString()).Substring(("00" + FlightTime.Minutes.ToString()).Length - 2);
            string ReturnTimeString = ("00" + ReturnTime.Hours.ToString()).Substring(("00" + ReturnTime.Hours.ToString()).Length - 2) + ":" + ("00" + ReturnTime.Minutes.ToString()).Substring(("00" + ReturnTime.Minutes.ToString()).Length - 2);
            string BidCloseTimeString = BidCloseDate.ToString("dd-MM-yyyy") + " " +  ("00" + BidCloseTime.Hours.ToString()).Substring(("00" + BidCloseTime.Hours.ToString()).Length - 2) + ":" + ("00" + BidCloseTime.Minutes.ToString()).Substring(("00" + BidCloseTime.Minutes.ToString()).Length - 2);
            string WaitTime = db.Setup_Content.Where(x => x.Purpose.ToLower() == "waiting time").Select(x => x.Description).FirstOrDefault() ?? "";



            TimeSpan ConfirmCloseTime = db.Cab_Bid.Where(x => x.BookingId == id && x.DetailCode == detailcode).Select(x => x.ConfirmUptoTime).FirstOrDefault() ?? DateTime.Now.TimeOfDay;
            DateTime ConfirmCloseDate = db.Cab_Bid.Where(x => x.BookingId == id && x.DetailCode == detailcode).Select(x => x.ConfirmUptoDate).FirstOrDefault() ?? DateTime.Now;
            string ConfirmCloseDateTime = ConfirmCloseDate.ToString("dd-MM-yyyy") + " " +  ("00" + ConfirmCloseTime.Hours.ToString()).Substring(("00" + ConfirmCloseTime.Hours.ToString()).Length - 2) + ":" + ("00" + ConfirmCloseTime.Minutes.ToString()).Substring(("00" + ConfirmCloseTime.Minutes.ToString()).Length - 2);
            string CancelRemarks= db.Cab_Bid.Where(x => x.BookingId == id && x.DetailCode == detailcode).Select(x => x.CancelRemarks).FirstOrDefault() ?? "";

            decimal suppbidamount=0;
            string suppbidremarks = "";

            string suppperson = "";
            string suppphone = "";
            string vehicleno = "";
            string vehiclemodel = "";
            string vehiclemake = "";

            if(detailcode == null){detailcode=0;}
            if(detailcode !=0){
                suppbidamount = db.Cab_Bid.Where(x => x.BookingId == id && x.DetailCode == detailcode).Select(x => x.BidAmount).FirstOrDefault() ?? 0;
                suppbidremarks = db.Cab_Bid.Where(x => x.BookingId == id && x.DetailCode == detailcode).Select(x => x.Remarks).FirstOrDefault() ?? "";
                suppperson = db.AC_Detail.Where(x => x.DetailCode == detailcode).Select(x => x.Name).FirstOrDefault() ?? "";
                suppphone = db.AC_Detail_Extra.Where(x => x.DetailCode == detailcode).Select(x => x.Cell1).FirstOrDefault() ?? "";
                vehicleno = db.AC_Detail_NoOfVeh.Where(x => x.DetailCode == detailcode && x.CabTypeID == cabtypeid).Select(x => x.RegNo).FirstOrDefault() ?? "";
                vehiclemake = db.AC_Detail_NoOfVeh.Where(x => x.DetailCode == detailcode && x.CabTypeID == cabtypeid).Select(x => x.Make).FirstOrDefault() ?? "";
                vehiclemodel = db.AC_Detail_NoOfVeh.Where(x => x.DetailCode == detailcode && x.CabTypeID == cabtypeid).Select(x => x.Model).FirstOrDefault() ?? "";
            }
            
            string luggagedetail = "";
            List<clsCabLuggage> detailluggage = new List<clsCabLuggage>();
            detailluggage = (from c in db.Cab_BookingLuggage
                             join cd in db.Cab_Luggage on c.LuggageID equals cd.LuggageID
                             where c.BookingID == id
                             select new clsCabLuggage
                             {
                                 LuggageID = c.LuggageID ?? 0,
                                 Description = cd.Description,
                             }).OrderBy(x => x.Description).ToList();


            foreach (clsCabLuggage vsublist in detailluggage)
            {
                if (luggagedetail == "")
                {
                    luggagedetail = vsublist.Description;
                }
                else
                {
                    luggagedetail = luggagedetail + " , " + vsublist.Description;
                }

            }


            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             where f.BookingId == id
                             select new clsBooking
                             {                               
                                 RecordID = db.Cab_Bid.Where(x=>x.BookingId == id && x.DetailCode == detailcode).Select(x=>x.RecordID).FirstOrDefault(),
                                 JobNo = f.JobNo ?? "",
                                 LuggageDesc = luggagedetail,
                                 WaitTime = WaitTime,
                                 BookingId = f.BookingId,
                                 CabId = f.CabId,
                                 CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabId).Select(x => x.Description).FirstOrDefault(),
                                 Startpoint = f.PickupAddress ?? "",
                                 Startpoint1 = f.PickupAddress1 ?? "",
                                 Startpoint2 = f.PickupAddress2 ?? "",
                                 Startpoint3 = f.PickupAddress3 ?? "",
                                 Endposition = f.DropOffAddress ?? "",
                                 Endposition1 = f.DropOffAddress1 ?? "",
                                 Endposition2 = f.DropOffAddress2 ?? "",
                                 Endposition3 = f.DropOffAddress3 ?? "",

                                 SupplierPerson=suppperson?? "",
                                 SupplierPhone = suppphone ?? "",
                                 VehicleNo = vehicleno ?? "",
                                 VehicleMake = vehiclemake ?? "",
                                 VehicleModel = vehiclemodel ?? "",

                                 PickupPostalCode = f.PickupPostalCode ?? "",
                                 PickupPostalCode1 = f.PickupPostalCode1 ?? "",
                                 PickupPostalCode2 = f.PickupPostalCode2 ?? "",
                                 PickupPostalCode3 = f.PickupPostalCode3 ?? "",
                                 DropoffPostalCode = f.DropoffPostalCode ?? "",
                                 DropoffPostalCode1 = f.DropoffPostalCode1 ?? "",
                                 DropoffPostalCode2 = f.DropoffPostalCode2 ?? "",
                                 DropoffPostalCode3 = f.DropoffPostalCode3 ?? "",

                                 PickupDateTime = f.PickupDateTime,
                                 PickupTimeString = PickupTimeString, //("00" + f.PickupTime.Value.Hours.ToString()).Substring(("00" + f.PickupTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.PickupTime.Value.Minutes.ToString()).Substring(("00" + f.PickupTime.Value.Minutes.ToString()).Length - 2),
                                 NoOfAdult = f.NoofPassenger,
                                 NoOfChild = f.NoOfChild,
                                 SurName = f.SurName ?? "",
                                 PasengerName = f.PasengerName,
                                 Cellno = f.Cellno,
                                 Email = f.Email ?? "",
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
                                 FlightTimeString = FlightTimeString, //("00" + f.FlightTime.Value.Hours.ToString()).Substring(("00" + f.FlightTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.FlightTime.Value.Minutes.ToString()).Substring(("00" + f.FlightTime.Value.Minutes.ToString()).Length - 2),
                                 DoorNo = f.DoorNo,
                                 MeetGreet = f.MeetGreet ?? 0,
                                 MaxAmount = f.BidAmount ?? 0,
                                 BidCloseTimeStr = BidCloseTimeString ?? "",
                                 ReturnDate = f.ReturnDate,
                                 ReturnTimeStr = ReturnTimeString,
                                 LuggageID = f.LuggageID ?? 0,
                                 //LuggageDesc = db.Cab_Luggage.Where(x => x.LuggageID == f.LuggageID).Select(x => x.Description).FirstOrDefault() ?? "",
                                 BidPostRemarks=f.BidPostRemarks ?? "",
                                 
                                 SuppBidAmount= suppbidamount,
                                 SuppBidRemarks = suppbidremarks ?? "",
                                 CancelRemarks=CancelRemarks ?? "",
                                 ConfirmCloseTimeStr=ConfirmCloseDateTime ?? "",

                                 PaybalEmail = f.PaybalEmail ?? "",
                                 CreditCardNo = f.CreditCardNo ?? "",
                                 Status = f.PaymentStatus ?? "",
                             }).FirstOrDefault();

            return bookingdetail;
        }

        public string SendEmail(string receiver, string receivername, string sendername, string subject, string message)
        {
            string vresult = "";
            try
            {
                if (ModelState.IsValid)
                {
                    //var senderEmail = new MailAddress("tiklacarsuk@gmail.com", sendername);
                    var receiverEmail = new MailAddress(receiver, receivername);

                    //var vsendere = System.Configuration.ConfigurationManager.ConnectionStrings["BookingEmail"].ConnectionString;
                    var vsendere= db.Setup_Email.Where(x=>x.Status.ToLower() == "active" && x.Purpose.ToLower() == "outgoing").Select(x=>x.Description).FirstOrDefault() ?? "";
                    var senderEmail = new MailAddress(vsendere, sendername);

                    //var password = System.Configuration.ConfigurationManager.ConnectionStrings["BookingPassword"].ConnectionString;
                    var password = db.Setup_Email.Where(x => x.Status.ToLower() == "active" && x.Purpose.ToLower() == "outgoing").Select(x => x.Password).FirstOrDefault() ?? "";
                    var sub = subject;
                    var body = message;
                    var smtp = new SmtpClient
                    {
                        //Host = "smtp.gmail.com",
                        //Port = 587,
                        Host = "tiklacars.com",
                        Port = 587,
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = true,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };
                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        smtp.Send(mess);
                    }


                    vresult = "OK";
                }
            }
            catch (Exception)
            {
                vresult = "Error";
            }
            return vresult;
        }


        public string SendEmailCustomer(string receiver, string receivername, string sendername, string subject, string message)
        {
            string vresult = "";
            try
            {
                //receiver = "qeplahore@hotmail.com";
                //sendername = "Tikla Bookings";
                if (ModelState.IsValid)
                {
                    //var senderEmail = new MailAddress("tcbookings@outlook.com", sendername);
                    //var senderEmail = new MailAddress("qeplahore@outlook.com", sendername);

                    //var vsendere = System.Configuration.ConfigurationManager.ConnectionStrings["BookingEmail"].ConnectionString;
                    var vsendere = db.Setup_Email.Where(x => x.Status.ToLower() == "active" && x.Purpose.ToLower() == "outgoing").Select(x => x.Description).FirstOrDefault() ?? "";
                    var senderEmail = new MailAddress(vsendere, sendername);

                    var receiverEmail = new MailAddress(receiver, receivername);
                    //var password = System.Configuration.ConfigurationManager.ConnectionStrings["BookingPassword"].ConnectionString;
                    var password = db.Setup_Email.Where(x => x.Status.ToLower() == "active" && x.Purpose.ToLower() == "outgoing").Select(x => x.Password).FirstOrDefault() ?? "";
                    var sub = subject;
                    var body = message;

                    //var smtp = new SmtpClient("tiklacars.com", 465);
                    //smtp.Credentials =new System.Net.NetworkCredential(vsendere, password);
                    //smtp.EnableSsl = true;
                    //smtp.Send(vsendere, receiver, subject, body);


                    var smtp = new SmtpClient
                    {
                        Host = "tiklacars.com",
                        Port = 587,
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = true,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };
                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        smtp.Send(mess);
                    }


                    vresult = "OK";
                }
            }
            catch (Exception ex)
            {
                vresult = "Error";
            }
            return vresult;
        }


        public bool CreateEmail(int? id, string emailtype,string emailtotype,decimal? detailcode)
        {

            string data = "OK";
            if (id != 0)
            {
                string emailsubject = db.Setup_EmailMessage.Where(x => x.Purpose.ToLower() == emailtype.ToLower()).Select(x => x.Subject).FirstOrDefault() ?? "";

                clsBooking bookingdetail = new clsBooking();
                bookingdetail = BookingDetail(id, detailcode);
                string message = "";
                string vpassemail = bookingdetail.Email;
                string vpassname = bookingdetail.PasengerName;

                message += "<div class='row'>";
                
                message += "<div class='col-12 text-center' style='text-align: center; margin-top: 10px; margin-bottom: 10px; color: #000 !important; '>";
                message += "<div class='col-12' style='background-color: #2A2B4B; color:white; text-align: left; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                message += "<div class='col-4' style='width:33%'>";
                message += "<span style='font-weight:bold;'>Ref. : </span>";
                message += "<span>" + bookingdetail.JobNo + "</span><br />";
                message += "<span style='font-weight:bold;'>Pickup Date : </span>";
                message += "<span>" + Convert.ToString(string.Format("{0:dd-MM-yyyy}", bookingdetail.PickupDateTime)) + "</span><br />";
                message += "<span style='font-weight:bold;'>Pickup Time : </span>";
                message += "<span>" + bookingdetail.PickupTimeString + "</span><br />";
                message += "<span style='font-weight:bold;'>Return Trip : </span>";
                message += "<span>" + (bookingdetail.ReturnTrip == 1 ? "Y" : "N") + "</span>";
                message += "</div>";
                message += "<div class='col-8' style='width:67%'>";
                message += "<span style='font-weight:bold;'>Pickup Address : </span>";

                if (emailtotype.ToLower() == "supplier")
                {
                    if (emailtype.ToLower() == "confirmed")
                    {
                        message += "<span>" + bookingdetail.Startpoint + "(" + bookingdetail.PickupPostalCode + ")</span><br />";
                        if (!String.IsNullOrEmpty(bookingdetail.Startpoint1))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.Startpoint1 + "(" + bookingdetail.PickupPostalCode1 + ")</span><br />";
                        }
                        if (!String.IsNullOrEmpty(bookingdetail.Startpoint2))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.Startpoint2 + "(" + bookingdetail.PickupPostalCode2 + ")</span><br />";
                        }
                        if (!String.IsNullOrEmpty(bookingdetail.Startpoint3))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.Startpoint3 + "(" + bookingdetail.PickupPostalCode3 + ")</span><br />";
                        }
                        if (!String.IsNullOrEmpty(bookingdetail.Endposition1))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.Endposition1 + "(" + bookingdetail.DropoffPostalCode1 + ")</span><br />";
                        }
                        if (!String.IsNullOrEmpty(bookingdetail.Endposition2))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.Endposition2 + "(" + bookingdetail.DropoffPostalCode2 + ")</span><br />";
                        }
                        if (!String.IsNullOrEmpty(bookingdetail.Endposition3))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.Endposition3 + "(" + bookingdetail.DropoffPostalCode3 + ")</span><br />";
                        }
                        message += "<span style='font-weight:bold;'>Dropoff Address : </span>";
                        message += "<span>" + bookingdetail.Endposition + "(" + bookingdetail.DropoffPostalCode + ")</span>";
                    }
                    else
                    {
                        message += "<span>" + bookingdetail.PickupPostalCode + "</span><br />";
                        if (!String.IsNullOrEmpty(bookingdetail.Startpoint1))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.PickupPostalCode1 + "</span><br />";
                        }
                        if (!String.IsNullOrEmpty(bookingdetail.Startpoint2))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.PickupPostalCode2 + "</span><br />";
                        }
                        if (!String.IsNullOrEmpty(bookingdetail.Startpoint3))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.PickupPostalCode3 + "</span><br />";
                        }
                        if (!String.IsNullOrEmpty(bookingdetail.Endposition1))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.DropoffPostalCode1 + "</span><br />";
                        }
                        if (!String.IsNullOrEmpty(bookingdetail.Endposition2))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.DropoffPostalCode2 + "</span><br />";
                        }
                        if (!String.IsNullOrEmpty(bookingdetail.Endposition3))
                        {
                            message += "<span style='font-weight:bold;'>Via : </span>";
                            message += "<span>" + bookingdetail.DropoffPostalCode3 + "</span><br />";
                        }
                        message += "<span style='font-weight:bold;'>Dropoff Address : </span>";
                        message += "<span>" + bookingdetail.DropoffPostalCode + "</span>";
                    }
                }
                else if (emailtotype.ToLower() == "admin" || emailtotype.ToLower() == "customer")
                {
                    message += "<span>" + bookingdetail.Startpoint + "(" + bookingdetail.PickupPostalCode + ")</span><br />";
                    if (!String.IsNullOrEmpty(bookingdetail.Startpoint1))
                    {
                        message += "<span style='font-weight:bold;'>Via : </span>";
                        message += "<span>" + bookingdetail.Startpoint1 + "(" + bookingdetail.PickupPostalCode1 + ")</span><br />";
                    }
                    if (!String.IsNullOrEmpty(bookingdetail.Startpoint2))
                    {
                        message += "<span style='font-weight:bold;'>Via : </span>";
                        message += "<span>" + bookingdetail.Startpoint2 + "(" + bookingdetail.PickupPostalCode2 + ")</span><br />";
                    }
                    if (!String.IsNullOrEmpty(bookingdetail.Startpoint3))
                    {
                        message += "<span style='font-weight:bold;'>Via : </span>";
                        message += "<span>" + bookingdetail.Startpoint3 + "(" + bookingdetail.PickupPostalCode3 + ")</span><br />";
                    }
                    if (!String.IsNullOrEmpty(bookingdetail.Endposition1))
                    {
                        message += "<span style='font-weight:bold;'>Via : </span>";
                        message += "<span>" + bookingdetail.Endposition1 + "(" + bookingdetail.DropoffPostalCode1 + ")</span><br />";
                    }
                    if (!String.IsNullOrEmpty(bookingdetail.Endposition2))
                    {
                        message += "<span style='font-weight:bold;'>Via : </span>";
                        message += "<span>" + bookingdetail.Endposition2 + "(" + bookingdetail.DropoffPostalCode2 + ")</span><br />";
                    }
                    if (!String.IsNullOrEmpty(bookingdetail.Endposition3))
                    {
                        message += "<span style='font-weight:bold;'>Via : </span>";
                        message += "<span>" + bookingdetail.Endposition3 + "(" + bookingdetail.DropoffPostalCode3 + ")</span><br />";
                    }
                    message += "<span style='font-weight:bold;'>Dropoff Address : </span>";
                    message += "<span>" + bookingdetail.Endposition + "(" + bookingdetail.DropoffPostalCode + ")</span>";
                }


                
                message += "</div>";
                message += "</div>";


                message += "<div class='col-12' style='text-align: left; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                message += "<div class='col-3' style='width:34%'>";
                message += "<span style='font-weight:bold;font-size:15px;'>Fare Detail :- </span><br />";
                message += "<span style='font-weight:bold;'>Distance : </span>";
                message += "<span>" + bookingdetail.TotalKm + " mile(s)</span><br />";

                if (emailtotype.ToLower() == "supplier")
                {
                    if (emailtype.ToLower() == "confirmed" || emailtype.ToLower() == "assign job")
                    {
                        message += "<span style='font-weight:bold;'>Fare Amount : </span>";
                        message += "<span>" + bookingdetail.SuppBidAmount + "</span><br />";
                    }
                    else
                    {
                        message += "<span style='font-weight:bold;'>Bid Upto Amount : </span>";
                        message += "<span>" + bookingdetail.MaxAmount + "</span><br />";
                        message += "<span style='font-weight:bold;'>Bid Close Time : </span>";
                        message += "<span>" + bookingdetail.BidCloseTimeStr + "</span><br />";
                    }
                }else if (emailtotype.ToLower() == "admin")
                {
                    message += "<span style='font-weight:bold;'>Customer Fare Amount : </span>";
                    message += "<span>" + bookingdetail.EstimateFareNet + "</span><br />";

                    if (emailtype.ToLower() == "confirmed" || emailtype.ToLower() == "assign job")
                    {
                        message += "<span style='font-weight:bold;'>Supplier Bid Amount : </span>";
                        message += "<span>" + bookingdetail.SuppBidAmount + "</span><br />";
                    }
                    else
                    {
                        message += "<span style='font-weight:bold;'>Bid Upto Amount : </span>";
                        message += "<span>" + bookingdetail.MaxAmount + "</span><br />";
                        message += "<span style='font-weight:bold;'>Bid Close Time : </span>";
                        message += "<span>" + bookingdetail.BidCloseTimeStr + "</span><br />";
                    }
                }
                else
                {
                    message += "<span style='font-weight:bold;'>Fare Amount : </span>";
                    message += "<span>" + bookingdetail.EstimateFareNet + "</span><br />";
                }
                
                //message += "<span style='font-weight:bold;'>Paid Through : </span>";
                //if (!String.IsNullOrEmpty(bookingdetail.Status))
                //{
                //    if (bookingdetail.Status.ToLower() == "paid")
                //    {
                //        if (!String.IsNullOrEmpty(bookingdetail.CreditCardNo))
                //        {
                //            message += "<span>Credit Card XXXX-XXXX-XXXX-" + bookingdetail.CreditCardNo + "</span><br />";
                //        }
                //        else
                //        {
                //            message += "<span>Paypal " + bookingdetail.PaybalEmail + "</span><br />";
                //        }

                //    }
                //    else
                //    {
                //        message += "<span>Unpaid</span><br />";
                //    }
                //}
                //else
                //{
                //    message += "<span>Unpaid</span><br />";
                //}

                message += "</div>";
                message += "<div class='col-4' style='width:33%'>";
                message += "<span style='font-weight:bold;font-size:15px;'>Cab Detail :- </span><br />";
                message += "<span style='font-weight:bold;'>Cab Type : </span>";
                message += "<span>" + bookingdetail.CabTypeName + "</span><br />";
                message += "<span style='font-weight:bold;'>No. of Passengers : </span>";
                message += "<span>" + bookingdetail.NoOfAdult + "</span><br />";
                message += "<span style='font-weight:bold;'>Luggage : </span>";
                message += "<span>" + bookingdetail.LuggageDesc + "</span>";
                message += "</div>";
                message += "<div class='col-5' style='width:33%'>";
                

                if (emailtotype.ToLower() == "supplier")
                {
                    if (emailtype.ToLower() == "confirmed")
                    {
                        message += "<span style='font-weight:bold;font-size:15px;'>Passenger Detail :- </span><br />";
                        message += "<span style='font-weight:bold;'>Full Name : </span>";
                        message += "<span>" + bookingdetail.PasengerName + "</span><br />";
                        message += "<span style='font-weight:bold;'>Mobile No. : </span>";
                        message += "<span>" + bookingdetail.Cellno + "</span><br />";
                        //message += "<span style='font-weight:bold;'>Email : </span>";
                        //message += "<span>" + bookingdetail.Email + "</span><br />";
                    }
                }
                else
                {
                    message += "<span style='font-weight:bold;font-size:15px;'>Passenger Detail :- </span><br />";
                    message += "<span style='font-weight:bold;'>Full Name : </span>";
                    message += "<span>" + bookingdetail.PasengerName + "</span><br />";
                    message += "<span style='font-weight:bold;'>Mobile No. : </span>";
                    message += "<span>" + bookingdetail.Cellno + "</span><br />";
                    message += "<span style='font-weight:bold;'>Email : </span>";
                    message += "<span>" + bookingdetail.Email + "</span><br />";
                    message += "<span style='font-weight:bold;'>Driver : </span>";
                    message += "<span>" + bookingdetail.SupplierPerson + "</span><br />";
                    message += "<span style='font-weight:bold;'>Driver Mobile No. : </span>";
                    message += "<span>" + bookingdetail.SupplierPhone + "</span><br />";

                    if (bookingdetail.VehicleNo != "")
                    {
                        message += "<span style='font-weight:bold;'>Vehicle No. : </span>";
                        message += "<span>" + bookingdetail.VehicleNo + "</span><br />";
                    }
                    if (bookingdetail.VehicleMake != "")
                    {
                        message += "<span style='font-weight:bold;'>Vehicle Make : </span>";
                        message += "<span>" + bookingdetail.VehicleMake + "</span><br />";
                    }
                    if (bookingdetail.VehicleModel != "")
                    {
                        message += "<span style='font-weight:bold;'>Vehicle Model : </span>";
                        message += "<span>" + bookingdetail.VehicleModel + "</span><br />";
                    }
                    
                }
                message += "</div>";

                message += "</div>";

                if (emailtype.ToLower() == "confirmed")
                {
                    if (!String.IsNullOrEmpty(bookingdetail.FlightNo))
                    {
                        message += "<div class='col-12' style='text-align: left; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                        message += "<span style='font-weight:bold;font-size:15px;'>Flight Detail :- </span><br />";
                        message += "<div class='col-4' style='width:33%'>";
                        message += "<span style='font-weight:bold;'>Flight No. : </span>";
                        message += "<span>" + bookingdetail.FlightNo + "</span><br />";
                        message += "<span style='font-weight:bold;'>Flight Time : </span>";
                        message += "<span>" + bookingdetail.FlightTimeString + "</span>";
                        message += "</div>";
                        message += "<div class='col-4' style='width:33%'>";
                        message += "<span style='font-weight:bold;'>Arrival From : </span>";
                        message += "<span>" + bookingdetail.ArrivalFrom + "</span><br />";
                        message += "<span style='font-weight:bold;'>Terminal : </span>";
                        message += "<span>" + bookingdetail.Terminal + "</span>";
                        message += "</div>";
                        message += "<div class='col-4' style='width:33%'>";
                        message += "<span style='font-weight:bold;'>Meet &amp; Greet : </span>";
                        message += "<span>" + (bookingdetail.MeetGreet == 1 ? "Y" : "N") + "</span><br />";
                        message += "</div>";
                        message += "</div>";
                    }
                }

                if (!String.IsNullOrEmpty(bookingdetail.AdditionalInfo))
                {
                    message += "<div class='col-12' style='text-align: left; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                    message += "<span style='font-weight:bold;font-size:17px;'>Additional Informations :- </span><br />";
                    message += "<span>" + bookingdetail.AdditionalInfo + "</span><br/>";
                    if (emailtotype.ToLower() == "admin" || emailtotype.ToLower() == "supplier")
                    {
                        message += "<span>" + bookingdetail.BidPostRemarks + "</span>";
                    }
                    message += "</div>";
                }


                if (emailtotype.ToLower() != "customer")
                {
                    message += "<div class='col-12' style='text-align: left; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                    message += "<span style='font-weight:bold;'>Waiting Time : </span>";
                    message += "<span>" + bookingdetail.WaitTime + "</span>";
                    message += "</div>";
                }
                

                //message += "<div class='col-12' style='text-align: left; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                //message += "Note : Thank you very much for your booking. Please note, this is an automatic response to your booking, you will receive a journey/booking confirmation shortly.";
                //message += "</div>";

                
                
                string emailresult = "";
                if (emailtotype.ToLower() == "admin")
                {
                    List<clsSetupEmail> adminemail = new List<clsSetupEmail>();
                    adminemail = (from c in db.Setup_Email
                                  where c.Status == "Active" && c.Purpose.ToLower() == "admin"
                                  select new clsSetupEmail
                                  {
                                      Description = c.Description,
                                      Password = c.Password
                                  }).ToList();                    
                    foreach (clsSetupEmail vrow in adminemail)
                    {
                        message += setemailfooter(emailtype);
                        message = setemailmessage(message);
                        emailresult = SendEmail(vrow.Description, "Admin", "Tiklacars.com", emailsubject, message);
                    }
                }else if (emailtotype.ToLower() == "supplier")
                {
                    if (emailtype.ToLower() == "for bid")
                    {
                        List<clsSetupEmail> suppemail = new List<clsSetupEmail>();
                        suppemail = (from c in db.Cab_Bid
                                     join su in db.AC_Detail_Extra on c.DetailCode equals su.DetailCode
                                     where (c.Status.ToLower() != "cancel") && c.BookingId == id
                                     select new clsSetupEmail
                                     {
                                         DetailCode=su.DetailCode,
                                         Description = su.CommEmail,
                                         Name=db.AC_Detail.Where(x=>x.DetailCode == c.DetailCode).Select(x=>x.Name).FirstOrDefault() ?? "",
                                         
                                     }).ToList();

                        string vsubmessage = "";
                        foreach (clsSetupEmail vrow in suppemail)
                        {
                            vsubmessage = "";
                            if (emailtype.ToLower() == "for bid")
                            {
                                string vhref = "https://sp.tiklacars.com/CabBooking/EmailBid?id=" + bookingdetail.BookingId.ToString() + "@" + bookingdetail.JobNo + "@" + bookingdetail.PasengerName + "@" + vrow.DetailCode.ToString() + "@" + Convert.ToString(string.Format("{0:ddMMyyyy}", bookingdetail.PickupDateTime));
                                vsubmessage += "<div class='col-12' style='text-align: center; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                                vsubmessage += "<a title='Bid' href='" + vhref + "' class='btn btn-success' style='width: 100px; height: 28px; color: white; font-size: 16px; vertical-align: middle; border-radius: 5px; background-color: rgb(243,113,1) !important; '>";
                                vsubmessage += "Bid";
                                vsubmessage += "</a>";
                                vsubmessage += "</div>";
                            }
                            else if (emailtype.ToLower() == "assign job")
                            {
                                string vhref = "https://sp.tiklacars.com/CabBooking/emailConfirm?id=" + bookingdetail.RecordID.ToString() + "@" + bookingdetail.JobNo + "@" + bookingdetail.PasengerName + "@" + vrow.DetailCode.ToString() + "@" + Convert.ToString(string.Format("{0:ddMMyyyy}", bookingdetail.PickupDateTime));
                                vsubmessage += "<div class='col-12' style='text-align: center; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                                vsubmessage += "<a title='Confirm' href='" + vhref + "' class='btn btn-success' style='width: 100px; height: 28px; color: white; font-size: 16px; vertical-align: middle; border-radius: 5px; background-color: rgb(243,113,1) !important; '>";
                                vsubmessage += "Confirm";
                                vsubmessage += "</a>";
                                vsubmessage += "</div>";
                            }
                            else if (emailtype.ToLower() == "confirmed")
                            {
                                string vhref = "https://sp.tiklacars.com/CabBooking/EmailJobDetail?id=" + bookingdetail.RecordID.ToString() + "@" + bookingdetail.JobNo + "@" + bookingdetail.PasengerName + "@" + vrow.DetailCode.ToString() + "@" + Convert.ToString(string.Format("{0:ddMMyyyy}", bookingdetail.PickupDateTime));
                                //vsubmessage += "<div class='col-12' style='text-align: center; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                                //vsubmessage += "<a title='Job Detail' href='" + vhref + "' class='btn btn-success' style='width: 100px; height: 28px; color: white; font-size: 16px; vertical-align: middle; border-radius: 5px; background-color: rgb(243,113,1) !important; '>";
                                //vsubmessage += "Job Detail";
                                //vsubmessage += "</a>";
                                //vsubmessage += "</div>";
                            }
                            vsubmessage = message + vsubmessage + setemailfooter(emailtype);
                            vsubmessage = setemailmessage(vsubmessage);
                            emailresult = SendEmail(vrow.Description, vrow.Name, "Tiklacars.com", emailsubject, vsubmessage);
                        }
                    }
                    else if (emailtype.ToLower() == "assign job" || emailtype.ToLower() == "confirmed")
                    {
                        string vsuppemail = db.AC_Detail_Extra.Where(x => x.DetailCode == detailcode).Select(x => x.CommEmail).FirstOrDefault() ?? "";
                        string vname = db.AC_Detail.Where(x => x.DetailCode == detailcode).Select(x => x.Name).FirstOrDefault() ?? "";

                        message += setemailfooter(emailtype);
                        message = setemailmessage(message);
                        emailresult = SendEmail(vsuppemail, vname, "Tiklacars.com", emailsubject, message);   
                    } 
                }
                else
                {
                    emailresult = SendEmailCustomer(vpassemail, vpassname, "Tiklacars.com", emailsubject, message);
                }
                

            }

            return true;
        }

        public bool CreateEmailInfoMaterial(int? id, string emailtype, string emailtotype, decimal? detailcode, string bidtype)
        {

            string data = "OK";
            if (id != 0)
            {
                string emailsubject = db.Setup_EmailMessage.Where(x => x.Purpose.ToLower() == emailtype.ToLower()).Select(x => x.Subject).FirstOrDefault() ?? "";

                clsBooking bookingdetail = new clsBooking();
                bookingdetail = BookingDetail(id, detailcode);
                string message = "";
                string vpassemail = bookingdetail.Email;
                string vpassname = bookingdetail.PasengerName;

                message += "<div class='row'>";

                message += "<div class='col-12 text-center' style='text-align: center; margin-top: 10px; margin-bottom: 10px; color: #000 !important; '>";
                //message += "<div class='col-12' style='background-color: #2A2B4B; color:white; text-align: left; font-size: 14px; display: block; padding-top: 10px; padding-bottom: 10px;'>";
                message += "<div class='col-12' style='text-align: left; font-size: 16px; display: block; padding-top: 10px; padding-bottom: 10px;'>";
                
                message += "<span style='font-weight:bold;'>Journey Ref. : </span>";
                message += "<span>" + bookingdetail.JobNo + "</span><br />";
                if (emailtype.ToLower() == "confirmed")
                {
                    message += "<span style='font-weight:bold;'>Pickup Date : </span>";
                    message += "<span>" + Convert.ToString(string.Format("{0:dd-MM-yyyy}", bookingdetail.PickupDateTime)) + "</span><br />";
                    message += "<span style='font-weight:bold;'>Pickup Time : </span>";
                    message += "<span>" + bookingdetail.PickupTimeString + "</span><br />";
                    message += "<span style='font-weight:bold;'>Pickup Address : </span>";
                    message += "<span>" + bookingdetail.PickupPostalCode + "</span><br />";
                    message += "<span style='font-weight:bold;'>Dropoff Address : </span>";
                    message += "<span>" + bookingdetail.DropoffPostalCode + "</span><br />";
                    message += "<span style='font-weight:bold;'>Fare Amount : </span>";
                    message += "<span>" + bookingdetail.SuppBidAmount + "</span><br />";

                }
                else if (emailtype.ToLower() == "assign job")
                {
                    message += "<span style='font-weight:bold;'>Confirmation Close Time : </span>";
                    message += "<span>" + bookingdetail.ConfirmCloseTimeStr + "</span><br />";
                    message += "<span style='font-weight:bold;'>Fare Amount : </span>";
                    message += "<span>" + bookingdetail.SuppBidAmount + "</span><br />";
                }
                else if (emailtype.ToLower() == "for bid")
                {
                    message += "<span style='font-weight:bold;'>Bid Close Time : </span>";
                    message += "<span>" + bookingdetail.BidCloseTimeStr + "</span><br />";
                }
                else if (emailtype.ToLower() == "cancelled")
                {
                    //message += "<span style='font-weight:bold;'>Bid Close Time : </span>";
                    //message += "<span>" + bookingdetail.BidCloseTimeStr + "</span><br />";
                    message += "<span style='font-weight:bold;'>Reason for Cancellation : </span>";
                    message += "<span>" + bookingdetail.CancelRemarks + "</span><br />";
                }

                

                message += "</div>";
                message += "</div>";
                message += "</div>";


                string emailresult = "";
                
                if (emailtype.ToLower() == "for bid")
                {
                    List<clsSetupEmail> suppemail = new List<clsSetupEmail>();
                    suppemail = (from c in db.Cab_Bid
                                    join su in db.AC_Detail_Extra on c.DetailCode equals su.DetailCode
                                    where (c.Status.ToLower() != "cancel") && c.BookingId == id
                                    select new clsSetupEmail
                                    {
                                        DetailCode = su.DetailCode,
                                        Description = su.CommEmail,
                                        Name = db.AC_Detail.Where(x => x.DetailCode == c.DetailCode).Select(x => x.Name).FirstOrDefault() ?? "",

                                    }).ToList();

                    string vsubmessage = "";
                    foreach (clsSetupEmail vrow in suppemail)
                    {
                        vsubmessage = "";
                        if (emailtype.ToLower() == "for bid")
                        {
                            string vhref = "https://sp.tiklacars.com/CabBooking/EmailBid?id=" + bookingdetail.BookingId.ToString() + "@" + bookingdetail.JobNo + "@" + bookingdetail.PasengerName + "@" + vrow.DetailCode.ToString() + "@" + Convert.ToString(string.Format("{0:ddMMyyyy}", bookingdetail.PickupDateTime));
                            vsubmessage += "<div class='col-12' style='text-align: center; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                            vsubmessage += "<a title='Bid' href='" + vhref + "' class='btn btn-success' style='width: 100px; height: 28px; color: white; font-size: 16px; vertical-align: middle; border-radius: 5px; background-color: rgb(243,113,1) !important; '>";
                            vsubmessage += "Bid";
                            vsubmessage += "</a>";
                            vsubmessage += "</div>";
                        }
                        else if (emailtype.ToLower() == "assign job")
                        {
                            string vhref = "https://sp.tiklacars.com/CabBooking/emailConfirm?id=" + bookingdetail.RecordID.ToString() + "@" + bookingdetail.JobNo + "@" + bookingdetail.PasengerName + "@" + vrow.DetailCode.ToString() + "@" + Convert.ToString(string.Format("{0:ddMMyyyy}", bookingdetail.PickupDateTime));
                            vsubmessage += "<div class='col-12' style='text-align: center; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                            vsubmessage += "<a title='Confirm' href='" + vhref + "' class='btn btn-success' style='width: 100px; height: 28px; color: white; font-size: 16px; vertical-align: middle; border-radius: 5px; background-color: rgb(243,113,1) !important; '>";
                            vsubmessage += "Confirm";
                            vsubmessage += "</a>";
                            vsubmessage += "</div>";
                        }
                        else if (emailtype.ToLower() == "confirmed")
                        {
                            string vhref = "https://sp.tiklacars.com/CabBooking/EmailJobDetail?id=" + bookingdetail.RecordID.ToString() + "@" + bookingdetail.JobNo + "@" + bookingdetail.PasengerName + "@" + vrow.DetailCode.ToString() + "@" + Convert.ToString(string.Format("{0:ddMMyyyy}", bookingdetail.PickupDateTime));
                            //vsubmessage += "<div class='col-12' style='text-align: center; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                            //vsubmessage += "<a title='Job Detail' href='" + vhref + "' class='btn btn-success' style='width: 100px; height: 28px; color: white; font-size: 16px; vertical-align: middle; border-radius: 5px; background-color: rgb(243,113,1) !important; '>";
                            //vsubmessage += "Job Detail";
                            //vsubmessage += "</a>";
                            //vsubmessage += "</div>";
                        }
                        vsubmessage = message + vsubmessage + setemailfooter(emailtype);
                        vsubmessage = setemailmessage(vsubmessage);
                        emailresult = SendEmail(vrow.Description, vrow.Name, "Tiklacars.com", emailsubject, vsubmessage);
                    }
                }
                else if (emailtype.ToLower() == "assign job" || emailtype.ToLower() == "confirmed" || emailtype.ToLower() == "cancelled")
                {
                    string vsuppemail = db.AC_Detail_Extra.Where(x => x.DetailCode == detailcode).Select(x => x.CommEmail).FirstOrDefault() ?? "";
                    string vname = db.AC_Detail.Where(x => x.DetailCode == detailcode).Select(x => x.Name).FirstOrDefault() ?? "";

                    if (emailtype.ToLower() == "assign job")
                    {
                        string vhref = "https://sp.tiklacars.com/CabBooking/emailConfirm?id=" + bookingdetail.RecordID.ToString() + "@" + bookingdetail.JobNo + "@" + bookingdetail.PasengerName + "@" + detailcode.ToString() + "@" + Convert.ToString(string.Format("{0:ddMMyyyy}", bookingdetail.PickupDateTime));
                        message += "<div class='col-12' style='text-align: center; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                        message += "<a title='Confirm' href='" + vhref + "' class='btn btn-success' style='width: 100px; height: 28px; color: white; font-size: 16px; vertical-align: middle; border-radius: 5px; background-color: rgb(243,113,1) !important; '>";
                        message += "Confirm";
                        message += "</a>";
                        message += "</div>";
                    }
                    else if (emailtype.ToLower() == "confirmed")
                    {
                        string vhref = "https://sp.tiklacars.com/CabBooking/EmailJobDetail?id=" + bookingdetail.RecordID.ToString() + "@" + bookingdetail.JobNo + "@" + bookingdetail.PasengerName + "@" + detailcode.ToString() + "@" + Convert.ToString(string.Format("{0:ddMMyyyy}", bookingdetail.PickupDateTime));
                        //message += "<div class='col-12' style='text-align: center; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                        //message += "<a title='Job Detail' href='" + vhref + "' class='btn btn-success' style='width: 100px; height: 28px; color: white; font-size: 16px; vertical-align: middle; border-radius: 5px; background-color: rgb(243,113,1) !important; '>";
                        //message += "Job Detail";
                        //message += "</a>";
                        //message += "</div>";
                    }
                    else if (emailtype.ToLower() == "cancelled")
                    {
                        string vhref = "https://sp.tiklacars.com/CabBooking/EmailJobDetail?id=" + bookingdetail.RecordID.ToString() + "@" + bookingdetail.JobNo + "@" + bookingdetail.PasengerName + "@" + detailcode.ToString() + "@" + Convert.ToString(string.Format("{0:ddMMyyyy}", bookingdetail.PickupDateTime));
                        //message += "<div class='col-12' style='text-align: center; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                        //message += "<a title='Job Detail' href='" + vhref + "' class='btn btn-success' style='width: 100px; height: 28px; color: white; font-size: 16px; vertical-align: middle; border-radius: 5px; background-color: rgb(243,113,1) !important; '>";
                        //message += "Job Detail";
                        //message += "</a>";
                        //message += "</div>";
                    }


                    if (emailtotype.ToLower() != "customer")
                    {
                        message += setemailfooter("");
                    }
                    else
                    {
                        message += setemailfooter(emailtype);
                    }
                    
                    message = setemailmessage(message);
                    emailresult = SendEmail(vsuppemail, vname, "Tiklacars.com", emailsubject, message);
                }

            }

            return true;
        }

        string setemailfooter(string emailtype)
        {
            string message="";
            string emailfooter = db.Setup_EmailMessage.Where(x => x.Purpose.ToLower() == emailtype.ToLower()).Select(x => x.Description).FirstOrDefault() ?? "";

            if (!String.IsNullOrEmpty(emailfooter))
            {
                message += "<div class='col-12' style='text-align: left; font-size: 14px; display: flex; padding-top: 10px; padding-bottom: 10px;'>";
                message += emailfooter;
                message += "</div>";
            }
            message += "<div class='col-12' style='text-align: left; font-size: 14px; padding-top: 10px; padding-bottom: 10px;'>";
            message += "Warm regards<br />";
            message += "<span style='font-weight:bold;font-size:16px;'>Tikla Cars</span><br />";
            message += "Phone No.: +44 208 925 3849<br />";
            message += "Mobile No.: +44 7480 798651<br />";
            message += "WhatsApp: +44 7309 764 662<br />";
            //message += "Address: 39 Springfield Drive, Ilford, Essex, IG2 6pt<br />";
            message += "Email: info@tiklacars.com<br />";
            message += "Web: www.tiklacars.com<br />";
            message += "</div>";
            message += "</div>";

            return message;
        }


        string setemailmessage(string data)
        {
            string message = "<html><head><script src='https://code.jquery.com/jquery-3.5.1.min.js' integrity='sha256-9/aliU8dGd2tb6OSsuzixeV4y/faTqgFtohetphbbj0=' crossorigin='anonymous'></script><link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css' integrity='sha384-JcKb8q3iqJ61gNV9KGb8thSsNjpSL0n8PARn9HuZOnIxN0hoP+VmmDGMN5t9UJ0Z' crossorigin='anonymous'><script src='https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js' integrity='sha384-B4gt1jrGC7Jh4AgTPSdUtOBvfO8shuf57BaghqFfPlYxofvL8/KUEfYiJOMMV+rV' crossorigin='anonymous'></script><script src='https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.bundle.min.js' integrity='sha384-LtrjvnR4Twt/qOuYxE721u19sVFLVSA4hf/rRt6PrZTmiPltdZcI7q7PXQBYTKyf' crossorigin='anonymous'></script></head><body style='display:inline-block;'>" + data + "</body></html>";
            return message;
        }
    }
    
}
