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
using System.Net;
using System.Net.Mail;

using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace POS.Controllers
{
    public class CabBookingController : BaseController
    {
        PosMBDb db = new PosMBDb();

        public string createemails(int? id, string emailtype, decimal? detailcode, string bidtype)
        {
            EmailController ec=new EmailController();
            string vresult= ec.CreateEmailFunction(id, emailtype, detailcode, bidtype);
            return vresult;
        }



        public ActionResult CalcCabFare(int? CabTypeID, decimal? vDistance)
        {
            decimal vdown = db.Cab_CabType_Fare.Where(x => x.CabTypeID == CabTypeID && x.UptoKm >= (vDistance ?? 0)).OrderBy(x => x.UptoKm).Select(x => x.UptoKm).FirstOrDefault() ?? 0;
            decimal vFare = 0;
            decimal balancekm = vDistance ?? 0;
            decimal lastuptokm = 0;
            decimal diffkm = 0;

            List<clsCabType> list = new List<clsCabType>();
            list = (from a in db.Cab_CabType_Fare
                    where a.CabTypeID == CabTypeID && a.UptoKm <= vdown
                    select new clsCabType 
                    {
                        UptoKm = a.UptoKm,
                        FarePrice = a.FarePrice ?? 0
                    }).OrderBy(x => x.UptoKm).ToList();

            foreach (var vsub in list)
            {

                diffkm = (vsub.UptoKm ?? 0) - lastuptokm;
                if (balancekm == 0)
                {
                    vFare = vFare;

                }
                else if (balancekm >= diffkm)
                {
                    vFare += ((vsub.FarePrice ?? 0) * diffkm);
                    balancekm -= diffkm;
                }
                else
                {
                    vFare += ((vsub.FarePrice ?? 0) * balancekm);
                    balancekm = 0;
                }
                lastuptokm = vsub.UptoKm ?? 0;
            }

            vFare = Math.Round(vFare, 2);

            return Json(new { vFare = vFare}, JsonRequestBehavior.AllowGet);
        }


        #region firebase
        //private static string ApiKey = "AIzaSyCeyer7TZdWKbkAMBB7nU3_ujIbipKqU0w";
        //private static string Bucket = "tiklacars-f91e6.appspot.com";
        //private const string WEB_ADDRESS = "https://fcm.googleapis.com/fcm/send";
        //private string SENDER_ID = "618720629906";
        //private string SERVER_KEY = "AAAAkA6flJI:APA91bGJx4ynSWJhEJwviqzuwH3JbZXw9ZG3X2xeOutinQWXsISIWLICXCJo1IvUu6dvesSevMwJQTSoIebKjKx-wkVdBk7h71LgcLC-3aEIQazxHIJfMNLSZmjb0vG8wJ-4J5er-y7U";

        //public void SendNotification(string title, string message, string token)
        //{

        //    try
        //    {
        //        dynamic data = new
        //        {
        //            // Uncoment this if you want to test for single device
        //            //to = "erc_jmkK4qY:APA91bGH8hQnR9OoopQaZWU2r8hyFmfHuROqy8aq-dh-VGCrVjH5xlA3PJXXHoe4ufnlbcbGX3bp7ikwYfrB9om4IBqNYlMxnIKftCpQDkyP8cHvppW3Cr4zunRodAjsFIs0Dsdhjhnz",
        //            to = token,

        //            // this is for multiple user. It Accepts Array
        //            //registration_ids = devices,
        //            notification = new
        //            {
        //                //title = "Test Notification",                           // Notification title
        //                title = title,                           // Notification title
        //                body = message,                         // Notification body data
        //                //link = "http://www.google.com"       // When click on notification user redirect to this link
        //            },
        //            //data = data                              // Any json data which you want to appear in notification
        //        };
        //        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //        var json = serializer.Serialize(data);
        //        Byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);
        //        WebRequest tRequest;
        //        tRequest = WebRequest.Create(WEB_ADDRESS);
        //        tRequest.Method = "post";
        //        tRequest.ContentType = "application/json";
        //        tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_KEY));
        //        tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
        //        tRequest.ContentLength = byteArray.Length;
        //        Stream dataStream = tRequest.GetRequestStream();
        //        dataStream.Write(byteArray, 0, byteArray.Length);
        //        dataStream.Close();
        //        WebResponse tResponse = tRequest.GetResponse();
        //        dataStream = tResponse.GetResponseStream();
        //        StreamReader tReader = new StreamReader(dataStream);
        //        String sResponseFromServer = tReader.ReadToEnd();
        //        tReader.Close();
        //        dataStream.Close();
        //        tResponse.Close();
        //    }
        //    catch
        //    { }

        //}
        public clsFirebase PushNotificationToDriver(string deviceId, string messagetype)
        {
            string str = "";
            bool status = false;

            if (!String.IsNullOrEmpty(deviceId)) { 
                try
                {    
                    string projectId= "tiklacars-f91e6";
                    string authDomain= "tiklacars-f91e6.firebaseapp.com";

                    string authkeyWeb = "AIzaSyBqUpBt9RVgOnUxH7lMWOCNZYdouKI9ygs";
                    string authkey = "AIzaSyCeyer7TZdWKbkAMBB7nU3_ujIbipKqU0w";
                    string authkeyIOS = "AIzaSyA7aQutUBHAlYO6MSHheepgsnURjQhXFsA";

                    string applicationIDWeb = "1:618720629906:web:4f9cfe50ca6166335b424b";
                    string applicationIDIOS = "1:618720629906:ios:708fc96f68504a425b424b";
                    string applicationID = "1:618720629906:android:9e4feb52e0bb23185b424b";
                
                    string SERVER_KEY = "AAAAkA6flJI:APA91bGJx4ynSWJhEJwviqzuwH3JbZXw9ZG3X2xeOutinQWXsISIWLICXCJo1IvUu6dvesSevMwJQTSoIebKjKx-wkVdBk7h71LgcLC-3aEIQazxHIJfMNLSZmjb0vG8wJ-4J5er-y7U";    
                    var senderId = "618720629906";
                    //string deviceId = "eXuwEIPYjoY:APA91bHlMQammcUPcg8uerjoBze6z92g7YLvmpBQvnlysJPtjVszccXfhqsbsiKE5mZd0pJa6A9bCUTWEFumaFuWTZnggqWn03AFcmTXX6vBtZAq352qVxX4wYoqvZFxAiYMKzu0vJh8";
                    WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                    tRequest.Method = "post";
                    tRequest.ContentType = "application/json";
                

                    string Message = db.Setup_Content.Where(x=>x.Purpose.ToLower() == messagetype.ToLower()).Select(x=>x.Description).FirstOrDefault() ?? "";
                    string Title = "Tikla Cars"; // messagetype;
                    var data = new
                    {                   
                        to = deviceId,
                        content_available=true,
                        notification = new
                        {
                            body = Message,
                            title = Title,
                            icon = "myicon"
                        },
                        data= new
                        {
                            priority="high",
                            sound = "app_sound.wav",
                            content_available= true,
                            bodyText = messagetype,
                            organization ="Tikla Cars"
                        }
                    };       

                    var serializer = new JavaScriptSerializer();
                    var json = serializer.Serialize(data);
                    Byte[] byteArray = Encoding.UTF8.GetBytes(json);
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_KEY));
                    tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
                    tRequest.ContentLength = byteArray.Length; 

                    using (Stream dataStream = tRequest.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);   
                        using (WebResponse tResponse = tRequest.GetResponse())
                        {
                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            {
                                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    String sResponseFromServer = tReader.ReadToEnd();
                                    str = sResponseFromServer;
                                    status = true;
                                }    
                            }    
                        }    
                    }    
                }        

                catch (Exception ex)
                {

                    str = ex.Message;
                    status = false;

                }
            }
            else
            {
                str="Invalid Device";
                status = false;
            }


            clsFirebase cfb = new clsFirebase();
            cfb.status = status;
            cfb.message = str;

            return cfb;
        }   
        #endregion

        #region Common

        public static string Right(string original, int numberCharacters)
        {
            return original.Substring(original.Length - numberCharacters);
        }

        public ActionResult GetAllActiveBookingsByFilter(string status, string passengername, string pickupaddress)
        {
            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();                
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                                            where f.Status.ToLower() == status.ToLower() && fs.DetailCode == User.UserId
                                            select new clsBooking
                                            {
                                                RecordID=fs.RecordID,
                                                BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "",
                                                PaymentStatus = f.PaymentStatus ?? "",
                                                PaybalEmail = f.PaybalEmail ?? "",
                                                CreditCardNo = f.CreditCardNo ?? "",
                                                PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
                                                
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
                                                ManualPickupAddress = f.ManualPickupAddress ?? "",
                                                ManualDropOffAddress = f.ManualDropOffAddress ?? "",

                                                ServiceProviderID = f.ServiceProviderID ?? 0,
                                                ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                                PaymentMode = f.PaymentMode ?? "",
                                                SPRefNo = f.SPRefNo ?? "",
                                                SPAmount = f.SPAmount ?? 0,

                                                PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                            }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                //SEARCHING...

                //if (!string.IsNullOrEmpty(passengername))
                //{
                //    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                //}
                if (!string.IsNullOrEmpty(pickupaddress))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower()) || c.Endposition.ToLower().Contains(pickupaddress.ToLower())).ToList();
                }



                //if (!string.IsNullOrEmpty(email))
                //{
                //    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                //}
                //if (!string.IsNullOrEmpty(cell1))
                //{
                //    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                //}

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


        public ActionResult GetAllActiveBookingsByStatusAssign(string status, string passengername, string pickupaddress)
        {
            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                //var passengername = ""; //Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                //var pickupaddress = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                                            where f.Status == status && fs.Status == status
                                            select new clsBooking
                                            {
                                                RecordID = fs.RecordID,
                                                BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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

                                                PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                                MeetGreetStr = (f.MeetGreet == 1) ? "Y" : "N",
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
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                
                                                ConfirmCloseDate = fs.ConfirmUptoDate ?? DateTime.Now,
                                                ConfirmCloseTime = fs.ConfirmUptoTime,
                                                ConfirmCloseTimeStr = fs.ConfirmUptoTime != null ? ("00" + fs.ConfirmUptoTime.Value.Hours.ToString()).Substring(("00" + fs.ConfirmUptoTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + fs.ConfirmUptoTime.Value.Minutes.ToString()).Substring(("00" + fs.ConfirmUptoTime.Value.Minutes.ToString()).Length - 2) : "",
                                                //ConfirmStatus = fs.ConfirmUptoTime != null ? (fs.ConfirmUptoDate.Value.Add(fs.ConfirmUptoTime ?? DateTime.Now.TimeOfDay) > DateTime.Now ? "Pending" : "Expired") : "Pending",

                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                            }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                //SEARCHING...
                if (!string.IsNullOrEmpty(passengername))
                {
                    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                }
                //else if (!string.IsNullOrEmpty(email))
                //{
                //    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                //}
                //else if (!string.IsNullOrEmpty(cell1))
                //{
                //    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                //}
                else if (!string.IsNullOrEmpty(pickupaddress))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                //{
                //    if (!(string.IsNullOrEmpty(sortColumn)))
                //    {
                //        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                //    }
                //}

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

        #endregion

        #region No Pay Booking

        public ActionResult NoPayBookingList()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "NoPayBooking"
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
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult NoPayBookingPost(int? id)
        {

            ViewBag.BookID = id;

            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             where f.BookingId == id
                             select new clsBooking
                             {

                                 BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
                                 CabId = f.CabId,
                                 CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabId).Select(x => x.Description).FirstOrDefault(),
                                 Startpoint = f.PickupAddress,
                                 Endposition = f.DropOffAddress,
                                 PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                             }).FirstOrDefault();

            return PartialView(bookingdetail);
        }

        public ActionResult GetAllNoPayActiveBookingsByFilterActive(string status, string passengername, string pickupaddress)
        {
            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();                
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            where f.Status.ToLower() == "active" && ((f.PaymentStatus.ToLower() ?? "") == ("NoPay").ToLower() || (f.PaymentStatus.ToLower() ?? "") == "")
                                            orderby f.BookingId descending 
                                            select new clsBooking
                                            {

                                                BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "",
                                                PaymentStatus = ((f.PaymentStatus.ToLower() ?? "") == "") ? "NoPay" : f.PaymentStatus,
                                                PaybalEmail = f.PaybalEmail ?? "",
                                                CreditCardNo = f.CreditCardNo ?? "",
                                                PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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
                                                ManualPickupAddress = f.ManualPickupAddress ?? "",
                                                ManualDropOffAddress = f.ManualDropOffAddress ?? "",

                                                ServiceProviderID = f.ServiceProviderID ?? 0,
                                                ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                                PaymentMode = f.PaymentMode ?? "",
                                                SPRefNo = f.SPRefNo ?? "",
                                                SPAmount = f.SPAmount ?? 0,

                                                PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime,
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

                                                MaxAmount = f.BidAmount ?? 0,

                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                            }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                //SEARCHING...

                //if (!string.IsNullOrEmpty(status))
                //{
                //    if (status.ToLower() == "unquoted")
                //    {
                //        invoice = invoice.Where(c => c.SuppBookStatus.ToLower().Equals("unread") || c.SuppBookStatus.ToLower().Equals("read")).ToList();
                //    }
                //    else if (status.ToLower() == "quoted")
                //    {
                //        invoice = invoice.Where(c => c.SuppBookStatus.ToLower().Equals("bid")).ToList();
                //    }
                //    else if (status.ToLower() == "cancel")
                //    {
                //        invoice = invoice.Where(c => c.SuppBookStatus.ToLower().Equals("cancel")).ToList();
                //    }
                //}


                //if (!string.IsNullOrEmpty(passengername))
                //{
                //    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                //}
                if (!string.IsNullOrEmpty(pickupaddress))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower()) || c.Endposition.ToLower().Contains(pickupaddress.ToLower())).ToList();
                }



                //if (!string.IsNullOrEmpty(email))
                //{
                //    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                //}
                //if (!string.IsNullOrEmpty(cell1))
                //{
                //    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                //}

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                //{
                //    if (!(string.IsNullOrEmpty(sortColumn)))
                //    {
                //        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                //    }
                //}

                invoice = invoice.OrderByDescending(x => x.BookingId).ToList();
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
        #endregion

        #region Unpaid Booking

        public ActionResult UnpaidBookingList()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "UnpaidBooking"
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
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult UnpaidBookingPost(int? id)
        {

            ViewBag.BookID = id;

            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             where f.BookingId == id
                             select new clsBooking
                             {

                                 BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
                                 CabId = f.CabId,
                                 CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabId).Select(x => x.Description).FirstOrDefault(),
                                 Startpoint = f.PickupAddress,
                                 Endposition = f.DropOffAddress,
                                 PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                             }).FirstOrDefault();

            return PartialView(bookingdetail);
        }

        public ActionResult GetAllUnpaidActiveBookingsByFilterActive(string status, string passengername, string pickupaddress)
        {
            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();                
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            where f.Status.ToLower() == "active" && ((f.PaymentStatus.ToLower() ?? "") != "paid" && (f.PaymentStatus.ToLower() ?? "") != "supplier pending" && (f.PaymentStatus.ToLower() ?? "") != "nopay")
                                            orderby f.BookingId descending 
                                            select new clsBooking
                                            {

                                                BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "",
                                                PaymentStatus = ((f.PaymentStatus.ToLower() ?? "") == "") ? "NoPay" : f.PaymentStatus,
                                                PaybalEmail = f.PaybalEmail ?? "",
                                                CreditCardNo = f.CreditCardNo ?? "",
                                                PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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
                                                ManualPickupAddress = f.ManualPickupAddress ?? "",
                                                ManualDropOffAddress = f.ManualDropOffAddress ?? "",

                                                ServiceProviderID = f.ServiceProviderID ?? 0,
                                                ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                                PaymentMode = f.PaymentMode ?? "",
                                                SPRefNo = f.SPRefNo ?? "",
                                                SPAmount = f.SPAmount ?? 0,

                                                PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime,
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

                                                MaxAmount = f.BidAmount ?? 0,

                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                            }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                //SEARCHING...

                //if (!string.IsNullOrEmpty(status))
                //{
                //    if (status.ToLower() == "unquoted")
                //    {
                //        invoice = invoice.Where(c => c.SuppBookStatus.ToLower().Equals("unread") || c.SuppBookStatus.ToLower().Equals("read")).ToList();
                //    }
                //    else if (status.ToLower() == "quoted")
                //    {
                //        invoice = invoice.Where(c => c.SuppBookStatus.ToLower().Equals("bid")).ToList();
                //    }
                //    else if (status.ToLower() == "cancel")
                //    {
                //        invoice = invoice.Where(c => c.SuppBookStatus.ToLower().Equals("cancel")).ToList();
                //    }
                //}


                //if (!string.IsNullOrEmpty(passengername))
                //{
                //    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                //}
                if (!string.IsNullOrEmpty(pickupaddress))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower()) || c.Endposition.ToLower().Contains(pickupaddress.ToLower())).ToList();
                }



                //if (!string.IsNullOrEmpty(email))
                //{
                //    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                //}
                //if (!string.IsNullOrEmpty(cell1))
                //{
                //    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                //}

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                //{
                //    if (!(string.IsNullOrEmpty(sortColumn)))
                //    {
                //        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                //    }
                //}

                invoice = invoice.OrderByDescending(x => x.BookingId).ToList();
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



        public ActionResult PaidBooking(int? id, string creditcardno, string paypalemail)
        {
            string message = "";
            bool status = false;

            string result = spUpdatePaidBooking(id, creditcardno, paypalemail);
            if (Convert.ToInt32(result) != 0)
            {
                ModelState.Clear();
                status = true;
                message = "OK";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }

        private string spUpdatePaidBooking(int? BookingID, string creditcardno, string paypalemail)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spUpdateBookingPaid", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BookingId", SqlDbType.Int).Value = BookingID;
                        cmd.Parameters.Add("@CreditCardNo", SqlDbType.VarChar, 50).Value = creditcardno;
                        cmd.Parameters.Add("@Paypalemail", SqlDbType.VarChar, 255).Value = paypalemail;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
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
        #endregion

        #region New Booking
        public ActionResult PendingBookingList()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "BookingListPending"
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
                    Response.AddHeader("Refresh", "300000");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public ActionResult getNewBooking()
        {
            int totalnotif = db.Cab_BookingMain.Where(x => x.Status.ToLower() == "active" && ((x.PaymentStatus.ToLower() ?? "") == "paid" || (x.PaymentStatus.ToLower() ?? "") == "supplier pending")).Count();
            return Json(new { result = totalnotif }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getUnpaidNewBooking()
        {
            int totalnotif = db.Cab_BookingMain.Where(x => x.Status.ToLower() == "active" && ((x.PaymentStatus.ToLower() ?? "") != "paid" && (x.PaymentStatus.ToLower() ?? "") != "nopay" && (x.PaymentStatus.ToLower() ?? "") != "supplier pending")).Count();
            return Json(new { result = totalnotif }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult getNoPayNewBooking()
        {
            int totalnotif = db.Cab_BookingMain.Where(x => x.Status.ToLower() == "active" && ((x.PaymentStatus.ToLower() ?? "") == "" || (x.PaymentStatus.ToLower() ?? "") == "nopay")).Count();
            return Json(new { result = totalnotif }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getNewConfirmForAvail()
        {
            List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                        join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                                        where f.Status.ToLower() == "confirmforemail" && fs.Status.ToLower() == "confirmforemail"
                                        select new clsBooking
                                        {
                                            BookingId = f.BookingId, WorkSection = f.WorkSection ?? "",
                                            PickupDateTime = f.PickupDateTime,
                                            PickupTimeSpan = f.PickupTime,
                                            BidCloseDate = f.BidCloseDate,
                                            BidCloseTimeSpan = f.BidCloseTime,
                                            ConfirmCloseDate = fs.ConfirmUptoDate ?? DateTime.Now,
                                            ConfirmCloseTime = fs.ConfirmUptoTime,
                                            ConfirmCloseTimeStr = fs.ConfirmUptoTime != null ? ("00" + fs.ConfirmUptoTime.Value.Hours.ToString()).Substring(("00" + fs.ConfirmUptoTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + fs.ConfirmUptoTime.Value.Minutes.ToString()).Substring(("00" + fs.ConfirmUptoTime.Value.Minutes.ToString()).Length - 2) : "",
                                        }).ToList();

            invoice = invoice.Where(x => x.PickupDateTime.Value.Add(x.PickupTimeSpan ?? DateTime.Now.TimeOfDay) >= DateTime.Now.Date).ToList();
            invoice = invoice.Where(x => x.ConfirmCloseDate >= DateTime.Now.Date).ToList();
            invoice = invoice.Where(x => x.ConfirmCloseTime > DateTime.Now.TimeOfDay).ToList();

            int totalnotif = invoice.Count();
            return Json(new { result = totalnotif }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetAllActiveBookingsByFilterActive(string status, string passengername, string pickupaddress)
        {

            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();                
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            where f.Status.ToLower() == "active" && ((f.PaymentStatus.ToLower() ?? "") == "paid" || (f.PaymentStatus.ToLower() ?? "") == "supplier pending")
                                            orderby f.BookingId descending
                                            select new clsBooking
                                            {

                                                BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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
                                                ManualPickupAddress=f.ManualPickupAddress ?? "",
                                                ManualDropOffAddress=f.ManualDropOffAddress ?? "",

                                                ServiceProviderID = f.ServiceProviderID ?? 0,
                                                ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                                PaymentMode = f.PaymentMode ?? "",
                                                SPRefNo = f.SPRefNo ?? "",
                                                SPAmount = f.SPAmount ?? 0,

                                                PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                                MeetGreetStr = (f.MeetGreet == 1) ? "Y" : "N",
                                                FlightNo = f.FlightNo,
                                                ArrivalFrom = f.ArrivalFrom,
                                                Terminal = f.FlightTerminal,
                                                FlightTimeString = ("00" + f.FlightTime.Value.Hours.ToString()).Substring(("00" + f.FlightTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.FlightTime.Value.Minutes.ToString()).Substring(("00" + f.FlightTime.Value.Minutes.ToString()).Length - 2),
                                                DoorNo = f.DoorNo,

                                                MaxAmount = f.BidAmount ?? 0,

                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                            }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                //SEARCHING...

                //if (!string.IsNullOrEmpty(status))
                //{
                //    if (status.ToLower() == "unquoted")
                //    {
                //        invoice = invoice.Where(c => c.SuppBookStatus.ToLower().Equals("unread") || c.SuppBookStatus.ToLower().Equals("read")).ToList();
                //    }
                //    else if (status.ToLower() == "quoted")
                //    {
                //        invoice = invoice.Where(c => c.SuppBookStatus.ToLower().Equals("bid")).ToList();
                //    }
                //    else if (status.ToLower() == "cancel")
                //    {
                //        invoice = invoice.Where(c => c.SuppBookStatus.ToLower().Equals("cancel")).ToList();
                //    }
                //}


                //if (!string.IsNullOrEmpty(passengername))
                //{
                //    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                //}
                if (!string.IsNullOrEmpty(pickupaddress))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower()) || c.Endposition.ToLower().Contains(pickupaddress.ToLower())).ToList();
                }

                foreach (clsBooking vrow in invoice)
                {
                    DateTime ServerDate = DateTime.Now;
                    DateTime JobDateTime = new DateTime(vrow.PickupDateTime.Value.Year, vrow.PickupDateTime.Value.Month, vrow.PickupDateTime.Value.Day, vrow.PickupTimeSpan.Value.Hours, vrow.PickupTimeSpan.Value.Minutes, vrow.PickupTimeSpan.Value.Seconds);

                    if (JobDateTime < ServerDate)
                    {
                        vrow.ExpireStatus ="Expired";
                    }else{
                        vrow.ExpireStatus ="";
                    }
                }


                //if (!string.IsNullOrEmpty(email))
                //{
                //    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                //}
                //if (!string.IsNullOrEmpty(cell1))
                //{
                //    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                //}

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                //{
                //    if (!(string.IsNullOrEmpty(sortColumn)))
                //    {
                //        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                //    }
                //}

                //invoice = invoice.OrderByDescending(x=>x.BookingId).ToList();
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


        public List<clsBooking> getDriverList()
        {
            List<clsBooking> detail = new List<clsBooking>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && c.Type.ToLower() == "driver"
                      select new clsBooking
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name 
                      }).ToList();
            return detail;
        }

        public ActionResult BookingPost(int? id)
        {

            ViewBag.DriverList = new SelectList(getDriverList(), "DetailCode", "Name");
            ViewBag.BookID = id;

            ViewBag.ServerDate = DateTime.Today.Date.ToString("dd-MM-yyyy");
            ViewBag.ServerTime = DateTime.Now.ToString("HH:mm:ss");

            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             where f.BookingId == id
                             select new clsBooking
                             {

                                 BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
                                 CabId = f.CabId,
                                 CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabId).Select(x => x.Description).FirstOrDefault(),
                                 Startpoint = f.PickupAddress,
                                 Endposition = f.DropOffAddress,
                                 PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                             }).FirstOrDefault();

            return PartialView(bookingdetail);
        }


        public ActionResult PostBid(int? id, decimal? amount, string remk, DateTime? closedate, DateTime? closetime, string bidstatus, decimal? vdetailcode, string data, string bidtype)
        {
            string message = "";
            bool status = false;

            if (vdetailcode == null) { vdetailcode = 0; }

            List<clsCabZone> zonelist;
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ZoneID");
            if (vdetailcode == 0)
            {
                if (data != null)
                {
                    if (data != "")
                    {
                        zonelist = JsonConvert.DeserializeObject<List<clsCabZone>>(data);
                        for (int i = 0; i < zonelist.Count; i++)
                        {
                            dt.Rows.Add(new object[] { i + 1, zonelist[i].ZoneID });
                        }
                    }

                }
            }
            
            if (dt.Rows.Count == 0)
            {
                dt.Rows.Add(new object[] { 0, 0});
            }

            string result = spUpdatePostBooking(id, bidstatus, amount, remk, closedate, closetime, vdetailcode, dt, bidtype);
            if (Convert.ToInt32(result) != 0)
            {
                ModelState.Clear();

                if (bidtype.ToLower() != ("Driver").ToLower())
                {
                    string emailresult = createemails(id, "for bid", 0, bidtype);
                }
                

                clsFirebase cfb = new clsFirebase();
                
                string vjobstatus="";
                string vbidtypeString="";
                if(bidtype.ToLower() == ("DriverOpen").ToLower() || bidtype.ToLower() == ("Bid Driver").ToLower()){
                    vjobstatus="pending";
                    vbidtypeString=bidtype;
                }

                List<clsBooking> bookingdetail = new List<clsBooking>();
                bookingdetail = (from fs in db.Cab_Bid
                                 join fd in db.AC_Detail on fs.DetailCode equals fd.DetailCode 
                                 where fs.Status.ToLower() == vjobstatus.ToLower() && fs.BookingId == id
                                 select new clsBooking
                                 {
                                     DetailCode=fs.DetailCode,
                                     FirebaseTokenAndroid=fd.FirebaseToken ?? "", 
                                     FirebaseTokenIOS=fd.FirebaseTokenIOS ?? "",
                                 }).OrderBy(x => x.DetailCode).ToList();

                foreach(clsBooking vrow in bookingdetail){
                    if (!String.IsNullOrEmpty(vrow.FirebaseTokenAndroid))
                    {
                        cfb = PushNotificationToDriver(vrow.FirebaseTokenAndroid, vbidtypeString);
                    }
                }
                status = true;
                message = "OK";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }

        private string spUpdatePostBooking(int? BookingID, string Status, decimal? Amount, string Remarks, DateTime? closedate, DateTime? closetime, decimal? vdetailcode, DataTable dt, string bidtype)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spUpdatePostBid", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BookingId", SqlDbType.Int).Value = BookingID;
                        cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = vdetailcode;
                        cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = Amount;
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 1000).Value = Remarks;
                        cmd.Parameters.Add("@CloseDate", SqlDbType.DateTime).Value = closedate;
                        cmd.Parameters.Add("@CloseTime", SqlDbType.DateTime).Value = closetime;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 50).Value = Status;
                        cmd.Parameters.Add("@BidType", SqlDbType.VarChar, 50).Value = bidtype.ToLower();
                        cmd.Parameters.Add("@dtZone", SqlDbType.Structured).Value = dt;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
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


        #endregion
        #region Active Booking List

        public ActionResult BookingList()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "BookingList"
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
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public ActionResult GetAllActiveBookingsByFilterPost(string status, string passengername, string pickupaddress)
        {
            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();                
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            where f.Status.ToLower() == "post" || f.Status.ToLower() == "postdriver"
                                            orderby f.BookingId descending 
                                            select new clsBooking
                                            {

                                                BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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
                                                ManualPickupAddress = f.ManualPickupAddress ?? "",
                                                ManualDropOffAddress = f.ManualDropOffAddress ?? "",

                                                ServiceProviderID = f.ServiceProviderID ?? 0,
                                                ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                                PaymentMode = f.PaymentMode ?? "",
                                                SPRefNo = f.SPRefNo ?? "",
                                                SPAmount = f.SPAmount ?? 0,

                                                PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                                MeetGreetStr = (f.MeetGreet == 1) ? "Y" : "N",
                                                FlightNo = f.FlightNo,
                                                ArrivalFrom = f.ArrivalFrom,
                                                Terminal = f.FlightTerminal,
                                                FlightTimeString = ("00" + f.FlightTime.Value.Hours.ToString()).Substring(("00" + f.FlightTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.FlightTime.Value.Minutes.ToString()).Substring(("00" + f.FlightTime.Value.Minutes.ToString()).Length - 2),
                                                DoorNo = f.DoorNo,
                                                MaxAmount = f.BidAmount ?? 0,
                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                                TotalNoOfBid = (from bid in db.Cab_Bid where bid.BookingId == f.BookingId && bid.Status.ToLower() == "bid" select bid).Count(),


                                            }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                //SEARCHING...

                //if (!string.IsNullOrEmpty(passengername))
                //{
                //    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                //}
                if (!string.IsNullOrEmpty(pickupaddress))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower()) || c.Endposition.ToLower().Contains(pickupaddress.ToLower())).ToList();
                }

                if (status == "compare")
                {
                    invoice = invoice.Where(x => x.PickupDateTime.Value.Add(x.PickupTimeSpan ?? DateTime.Now.TimeOfDay) >= DateTime.Now.Date).OrderByDescending(x => x.PickupDateTime.Value.Add(x.PickupTimeSpan ?? DateTime.Now.TimeOfDay)).ToList();
                }


                //if (!string.IsNullOrEmpty(email))
                //{
                //    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                //}
                //if (!string.IsNullOrEmpty(cell1))
                //{
                //    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                //}

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                //{
                //    if (!(string.IsNullOrEmpty(sortColumn)))
                //    {
                //        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                //    }
                //}
                //invoice = invoice.OrderByDescending(x => x.BookingId).ToList();

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

        public ActionResult GetAllActiveBookingsByFilterPostBidClose(string status, string passengername, string pickupaddress)
        {
            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();                
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            where f.Status.ToLower() == "post"
                                            select new clsBooking
                                            {

                                                BookingId = f.BookingId, WorkSection = f.WorkSection ?? "",
                                                JobNo = f.JobNo ?? "",
                                                PaymentStatus = ((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus,
                                                PaybalEmail = f.PaybalEmail ?? "",
                                                CreditCardNo = f.CreditCardNo ?? "",
                                                PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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
                                                ManualPickupAddress = f.ManualPickupAddress ?? "",
                                                ManualDropOffAddress = f.ManualDropOffAddress ?? "",

                                                ServiceProviderID = f.ServiceProviderID ?? 0,
                                                ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                                PaymentMode = f.PaymentMode ?? "",
                                                SPRefNo = f.SPRefNo ?? "",
                                                SPAmount = f.SPAmount ?? 0,

                                                PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime,
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
                                                MaxAmount = f.BidAmount ?? 0,
                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeSpan=f.BidCloseTime,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                                TotalNoOfBid = (from bid in db.Cab_Bid where bid.BookingId == f.BookingId && bid.Status.ToLower() == "bid" select bid).Count(),


                                            }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                //SEARCHING...

                //if (!string.IsNullOrEmpty(passengername))
                //{
                //    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                //}
                if (!string.IsNullOrEmpty(pickupaddress))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower()) || c.Endposition.ToLower().Contains(pickupaddress.ToLower())).ToList();
                }



                //if (!string.IsNullOrEmpty(email))
                //{
                //    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                //}
                //if (!string.IsNullOrEmpty(cell1))
                //{
                //    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                //}

                List<clsBooking> invoicefinal = new List<clsBooking>();
                //invoice = invoice.Where(c => c.BidCloseTime < DateTime.Now).ToList();

                foreach (clsBooking f in invoice)
                {
                    DateTime? BidCloseDate;
                    TimeSpan? BidCloseTime;
                    if (f.BidCloseDate == null)
                    {
                        BidCloseDate = new DateTime(2000, 1, 1);
                    }
                    else
                    {
                        BidCloseDate = f.BidCloseDate;
                    }
                    if (f.BidCloseTimeSpan == null)
                    {
                        BidCloseTime = new TimeSpan(0, 0, 1);
                    }
                    else
                    {
                        BidCloseTime = f.BidCloseTimeSpan;
                    }
                    DateTime CloseBidDateTime = new DateTime(BidCloseDate.Value.Year, BidCloseDate.Value.Month, BidCloseDate.Value.Day, BidCloseTime.Value.Hours, BidCloseTime.Value.Minutes, BidCloseTime.Value.Seconds);

                    if (CloseBidDateTime > DateTime.Now)
                    {
                        clsBooking sublist = new clsBooking();
                        sublist.BookingId = f.BookingId;
                        sublist.JobNo = f.JobNo ?? "";
                        sublist.PaymentStatus = f.PaymentStatus.ToLower();
                        sublist.PaybalEmail = f.PaybalEmail ?? "";
                        sublist.CreditCardNo = f.CreditCardNo ?? "";
                        sublist.PaymentNo = f.PaymentNo;
                        sublist.CabId = f.CabId;
                        sublist.CabTypeName = f.CabTypeName;
                        sublist.Startpoint = f.Startpoint ?? "";
                        sublist.Startpoint1 = f.Startpoint1 ?? "";
                        sublist.Startpoint2 = f.Startpoint2 ?? "";
                        sublist.Startpoint3 = f.Startpoint3 ?? "";
                        sublist.Endposition = f.Endposition ?? "";
                        sublist.Endposition1 = f.Endposition1 ?? "";
                        sublist.Endposition2 = f.Endposition2 ?? "";
                        sublist.Endposition3 = f.Endposition3 ?? "";
                        sublist.ManualPickupAddress = f.ManualPickupAddress ?? "";
                        sublist.ManualDropOffAddress = f.ManualDropOffAddress ?? "";
                        sublist.PickupDateTime = f.PickupDateTime;
                        sublist.PickupTimeSpan = f.PickupTimeSpan;
                        sublist.PickupDateTimeStr = f.PickupDateTimeStr;
                        sublist.PickupTimeString = f.PickupTimeString;
                        sublist.NoOfAdult = f.NoOfAdult;
                        sublist.NoOfChild = f.NoOfChild;
                        sublist.SurName = f.SurName ?? "";
                        sublist.PasengerName = f.PasengerName;
                        sublist.Cellno = f.Cellno;
                        sublist.Email = f.Email;
                        sublist.Amount = f.Amount;
                        sublist.EstimateFareNet = f.EstimateFareNet;
                        sublist.TotalKm = f.TotalKm;
                        sublist.ChildSeatStr = f.ChildSeatStr;
                        sublist.DisableSeatStr = f.DisableSeatStr;
                        sublist.AdditionalInfo = f.AdditionalInfo;
                        sublist.ReturnTripStr = f.ReturnTripStr;
                        sublist.MeetGreetStr = f.MeetGreetStr;
                        sublist.FlightNo = f.FlightNo;
                        sublist.ArrivalFrom = f.ArrivalFrom;
                        sublist.Terminal = f.Terminal;
                        sublist.FlightTimeString = f.FlightTimeString;
                        sublist.DoorNo = f.DoorNo;
                        sublist.MaxAmount = f.MaxAmount;
                        //sublist.SuppBookStatus = f.SuppBookStatus;
                        //sublist.CreateDateTime = f.CreateDateTime;
                        //sublist.BookDateTime = f.BookDateTime;
                        //sublist.SuppBidAmount = f.SuppBidAmount;
                        //sublist.SuppBidRemarks = f.SuppBidRemarks;
                        //sublist.CancelDateTime = f.CancelDateTime;
                        sublist.ReturnDate = f.ReturnDate;
                        sublist.ReturnTimeStr = f.ReturnTimeStr;
                        sublist.LuggageID = f.LuggageID;
                        sublist.LuggageDesc = f.LuggageDesc;
                        sublist.BidCloseDate = f.BidCloseDate;
                        sublist.BidCloseTimeSpan = f.BidCloseTimeSpan;
                        sublist.BidCloseTimeStr = f.BidCloseTimeStr;
                        sublist.BidPostRemarks = f.BidPostRemarks ?? "";
                        sublist.imageName = f.imageName;
                        sublist.TotalNoOfBid = f.TotalNoOfBid;
                        sublist.ServiceProviderID = f.ServiceProviderID ?? 0;
                        sublist.ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "";
                        sublist.PaymentMode = f.PaymentMode ?? "";
                        sublist.SPRefNo = f.SPRefNo ?? "";
                        sublist.SPAmount = f.SPAmount ?? 0;

                        invoicefinal.Add(sublist);

                    }
                }



                //if (!string.IsNullOrEmpty(email))
                //{
                //    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                //}
                //if (!string.IsNullOrEmpty(cell1))
                //{
                //    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                //}

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                //{
                //    if (!(string.IsNullOrEmpty(sortColumn)))
                //    {
                //        invoicefinal = invoicefinal.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                //    }
                //}
                invoice = invoice.OrderByDescending(x => x.BookingId).ToList();
                recordsTotal = invoicefinal.Count();
                var data = invoicefinal.Skip(skip).Take(pageSize).ToList();
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
            ViewBag.BookID = id;

            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             where f.BookingId == id
                             select new clsBooking
                             {
                                 BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
                                 CabId = f.CabId,
                                 CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabId).Select(x => x.Description).FirstOrDefault(),
                                 Startpoint = f.PickupAddress,
                                 Endposition = f.DropOffAddress,
                                 PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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

                                 ServiceProviderID = f.ServiceProviderID ?? 0,
                                 ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                 PaymentMode = f.PaymentMode ?? "",
                                 SPRefNo = f.SPRefNo ?? "",
                                 SPAmount = f.SPAmount ?? 0,

                                 MaxAmount = f.BidAmount ?? 0,
                                 ReturnDate = f.ReturnDate,
                                 ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                 LuggageID = f.LuggageID ?? 0,
                                 LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                 BidCloseDate = f.BidCloseDate,
                                 BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                 BidPostRemarks = f.BidPostRemarks ?? "",
                             }).FirstOrDefault();

            return PartialView(bookingdetail);
        }



        public ActionResult CancelBid(int? id, string remk)
        {
            string message = "";
            bool status = false;

            string result = spUpdateCancelBooking(id, remk);
            if (Convert.ToInt32(result) != 0)
            {
                ModelState.Clear();
                status = true;
                message = "OK";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }

        private string spUpdateCancelBooking(int? BookingID, string Remarks)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spUpdateCancelBid", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BookingId", SqlDbType.Int).Value = BookingID;
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 1000).Value = Remarks;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
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

        public ActionResult GetAllCabZone()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int recordsTotal = 0;
                List<clsCabZone> CabZone = (from f in db.Cab_Zone
                                            where f.Status.ToLower() == "active"
                                            select new clsCabZone
                                            {
                                                CabZoneID = f.ZoneID,
                                                Description = f.Description,
                                                Remarks = f.Remarks,
                                                Status = f.Status
                                            }).OrderBy(x => x.Description).ThenBy(x => x.CabZoneID).ToList();
                
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        CabZone = CabZone.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = CabZone.Count();
                //.Skip(skip).Take(pageSize)
                var data = CabZone.ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult GetAllDrivers()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int recordsTotal = 0;

                List<clsAccountDetail> detail = (from f in db.AC_Detail
                                            join fr in db.AC_Detail_Extra on f.DetailCode equals fr.DetailCode
                                            where f.Status.ToLower() == "active" && f.Type.ToLower() == "driver" 
                                            select new clsAccountDetail
                                            {
                                                DetailCode = f.DetailCode,
                                                Name = f.Name ?? "",
                                                BookingEmail = fr.BookingEmail ?? "",
                                                CommEmail = fr.BookingEmail ?? "",
                                                Cell1 = fr.Cell1 ?? "",
                                                Phone1 = fr.Phone1 ?? "",
                                                Remarks = fr.Remarks ?? "",
                                            }).OrderBy(x => x.Name).ThenBy(x => x.DetailCode).ToList();

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        detail = detail.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = detail.Count();
                //.Skip(skip).Take(pageSize)
                var data = detail.ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        #region Compare and Assign

        public ActionResult BookingComarisonList()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "BookingComparison"
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
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }





        public ActionResult BookingComparisonDetail(int? id, string FormType)
        {
            ViewBag.BookID = id;

            ViewBag.ServerDate = DateTime.Today.Date.ToString("dd-MM-yyyy");
            ViewBag.ServerTime = DateTime.Now.ToString("HH:mm:ss");

            if (String.IsNullOrEmpty(FormType)) { FormType = ""; }

            List<clsBooking> bookingdetail = new List<clsBooking>();
            bookingdetail = (from fs in db.Cab_Bid
                             join f in db.Cab_BookingMain on fs.BookingId equals f.BookingId
                             where fs.Status.ToLower() == "bid" && fs.BookingId == id
                             select new clsBooking
                             {
                                 RecordID=fs.RecordID,
                                 BookingId = fs.BookingId ?? 0,
                                 PickupDateTime = f.PickupDateTime,
                                 PickupTimeSpan = f.PickupTime,
                                 PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2) + "-" + f.PickupDateTime.Value.Year.ToString(),
                                 PickupTimeString = ("00" + f.PickupTime.Value.Hours.ToString()).Substring(("00" + f.PickupTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.PickupTime.Value.Minutes.ToString()).Substring(("00" + f.PickupTime.Value.Minutes.ToString()).Length - 2),
                                 DetailName = db.AC_Detail.Where(x => x.DetailCode == fs.DetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                 SuppBookStatus = fs.Status,
                                 CreateDateTime = fs.CreateDateTime,
                                 BookDateTime = fs.BidDateTime,
                                 SuppBidAmount = fs.BidAmount ?? 0,
                                 SuppBidRemarks = fs.Remarks ?? "",
                                 CancelDateTime = fs.CancelDateTime,
                                 FormType = FormType,
                             }).OrderBy(x => x.SuppBidAmount).ThenBy(x => x.RecordID).ToList();

            clsBooking bookingdetailfinal = new clsBooking();
            bookingdetailfinal.BookingList = bookingdetail.ToList();
            return PartialView(bookingdetailfinal);
        }



        public ActionResult GetBidDetail(int? id)
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var detailname = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                                            where f.Status == "Post" && fs.Status == "Bid" && f.BookingId == id
                                            select new clsBooking
                                            {
                                                RecordID = fs.RecordID,
                                                BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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

                                                ServiceProviderID = f.ServiceProviderID ?? 0,
                                                ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                                PaymentMode = f.PaymentMode ?? "",
                                                SPRefNo = f.SPRefNo ?? "",
                                                SPAmount = f.SPAmount ?? 0,

                                                PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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

                                                MaxAmount = f.Amount - ((f.Amount / 100) * 20),
                                                SuppBookStatus = fs.Status,
                                                CreateDateTime = fs.CreateDateTime,
                                                BookDateTime = fs.BidDateTime,
                                                SuppBidAmount = fs.BidAmount ?? 0,
                                                SuppBidRemarks = fs.Remarks ?? "",
                                                CancelDateTime = fs.CancelDateTime,

                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                            }).OrderBy(x => x.SuppBidAmount).ThenBy(x => x.RecordID).ToList();

                //SEARCHING...
                if (!string.IsNullOrEmpty(detailname))
                {
                    invoice = invoice.Where(c => c.DetailName.ToLower().Contains(detailname.ToLower())).ToList();
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



        public ActionResult FinalBid(int? id, DateTime? closedate, DateTime? closetime)
        {
            string message = "";
            bool status = false;

            string result = spUpdateFinalBooking(id, "Final", closedate, closetime);
            if (Convert.ToInt32(result) != 0)
            {
                ModelState.Clear();
                int bookid = db.Cab_Bid.Where(x => x.RecordID == id).Select(x => x.BookingId).FirstOrDefault() ?? 0;
                decimal detailcode = db.Cab_Bid.Where(x => x.RecordID == id).Select(x => x.DetailCode).FirstOrDefault() ?? 0;
                string emailresult = createemails(bookid, "assign job", detailcode,"");
                status = true;
                message = "OK";

                clsFirebase cfb = new clsFirebase();
                List<clsBooking> bookingdetail = new List<clsBooking>();
                bookingdetail = (from fs in db.Cab_Bid
                                 join fd in db.AC_Detail on fs.DetailCode equals fd.DetailCode
                                 where fs.RecordID == id
                                 select new clsBooking
                                 {
                                     DetailCode = fs.DetailCode,
                                     FirebaseTokenAndroid = fd.FirebaseToken ?? "",
                                     FirebaseTokenIOS = fd.FirebaseTokenIOS ?? "",
                                 }).OrderBy(x => x.DetailCode).ToList();

                foreach (clsBooking vrow in bookingdetail)
                {
                    if (!String.IsNullOrEmpty(vrow.FirebaseTokenAndroid))
                    {
                        cfb = PushNotificationToDriver(vrow.FirebaseTokenAndroid, "assignnotification");
                    }
                }
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }


        public ActionResult FinalBidConfirm(int? id)
        {
            string message = "";
            bool status = false;

            string result = spUpdateFinalBooking(id, "Confirm",DateTime.Now,DateTime.Now);
            if (Convert.ToInt32(result) != 0)
            {
                ModelState.Clear();
                string emailresult = createemails(id, "confirmed", 0, "");
                status = true;
                message = "OK";

                clsFirebase cfb = new clsFirebase();
                List<clsBooking> bookingdetail = new List<clsBooking>();
                bookingdetail = (from fs in db.Cab_Bid
                                 join fd in db.AC_Detail on fs.DetailCode equals fd.DetailCode
                                 where fs.RecordID == id 
                                 select new clsBooking
                                 {
                                     DetailCode = fs.DetailCode,
                                     FirebaseTokenAndroid = fd.FirebaseToken ?? "",
                                     FirebaseTokenIOS = fd.FirebaseTokenIOS ?? "",
                                 }).OrderBy(x => x.DetailCode).ToList();

                foreach (clsBooking vrow in bookingdetail)
                {
                    if (!String.IsNullOrEmpty(vrow.FirebaseTokenAndroid))
                    {
                        cfb = PushNotificationToDriver(vrow.FirebaseTokenAndroid, "confirmnotification");
                    }
                }
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }

        private string spUpdateFinalBooking(int? RecordID, string Status, DateTime? closedate, DateTime? closetime)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spUpdateFinalBid", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@RecordID", SqlDbType.Int).Value = RecordID;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 50).Value = Status;
                        cmd.Parameters.Add("@CloseDate", SqlDbType.DateTime).Value = closedate;
                        cmd.Parameters.Add("@CloseTime", SqlDbType.DateTime).Value = closetime;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
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

        
        #endregion
        #region Supplier Assigned Job List
            public ActionResult AssignedList()
            {
                if (User != null)
                {
                    clsUserRight rights = new clsUserRight();
                    rights = (from r in db.Admin_Setup_UserRights
                              where r.UserID == User.UserId && r.MenuName == "AssignedList"
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
                        return View();
                    }
                }
                else
                {
                    return RedirectToAction("Index", "account");
                }
            }


            public ActionResult AssignedBookingDetail(int? id)
            {
                ViewBag.BookID = id;
                ViewBag.ServerDate = DateTime.Today.Date.ToString("dd-MM-yyyy");
                ViewBag.ServerTime = DateTime.Now.ToString("HH:mm:ss");

                clsBooking bookingdetail = new clsBooking();
                bookingdetail = (from f in db.Cab_BookingMain
                                 join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                                 where fs.RecordID == id
                                 select new clsBooking
                                 {
                                     RecordID=fs.RecordID,
                                     BookingId = f.BookingId, WorkSection = f.WorkSection ?? "",
                                     Status=fs.Status ?? "",
                                     PickupDateTime = f.PickupDateTime,
                                     PickupTimeSpan = f.PickupTime,
                                     PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2) + "-" + f.PickupDateTime.Value.Year.ToString(),
                                     PickupTimeString = ("00" + f.PickupTime.Value.Hours.ToString()).Substring(("00" + f.PickupTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.PickupTime.Value.Minutes.ToString()).Substring(("00" + f.PickupTime.Value.Minutes.ToString()).Length - 2),
                                 }).FirstOrDefault();

                return PartialView(bookingdetail);
            }


            public ActionResult CancelBidOnly(int? id, string remk)
            {
                string message = "";
                bool status = false;

                string result = spUpdateCancelBidOnly(id, remk);
                if (Convert.ToInt32(result) != 0)
                {
                    ModelState.Clear();
                    int bookid = db.Cab_Bid.Where(x => x.RecordID == id).Select(x => x.BookingId).FirstOrDefault() ?? 0;
                    decimal detailcode = db.Cab_Bid.Where(x => x.RecordID == id).Select(x => x.DetailCode).FirstOrDefault() ?? 0;
                    string emailresult = createemails(bookid, "cancelled", detailcode, "");
                    status = true;
                    message = "OK";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
                return new JsonResult { Data = new { status = status, message = message } };
            }

            private string spUpdateCancelBidOnly(int? BookingID, string Remarks)
            {
                string returnId = "";
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("spUpdateCancelBidOnly", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@RecordID", SqlDbType.Int).Value = BookingID;
                            cmd.Parameters.Add("@CancelRemarks", SqlDbType.VarChar, 255).Value = Remarks;
                            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                            cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
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



            public ActionResult BackToNewBid(int? id, string remk)
            {
                string message = "";
                bool status = false;

                string result = spUpdateBackToNewBid(id, remk);
                if (Convert.ToInt32(result) != 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "OK";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
                return new JsonResult { Data = new { status = status, message = message } };
            }

            private string spUpdateBackToNewBid(int? BookingID, string Remarks)
            {
                string returnId = "";
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("spUpdateBidToNew", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = BookingID;
                            cmd.Parameters.Add("@CancelRemarks", SqlDbType.VarChar, 255).Value = Remarks;
                            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                            cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
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
        #endregion

        #region Confirm Job List

            public ActionResult ConfirmList()
            {
                if (User != null)
                {
                    clsUserRight rights = new clsUserRight();
                    rights = (from r in db.Admin_Setup_UserRights
                              where r.UserID == User.UserId && r.MenuName == "UpcommingJob"
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
                        return View();
                    }
                }
                else
                {
                    return RedirectToAction("Index", "account");
                }
            }



            public ActionResult GetAllConfirmJobList(string status, string passengername, string pickupaddress)
            {
                string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
                if (User != null)
                {
                    var draw = Request.Form.GetValues("draw").FirstOrDefault();
                    var start = Request.Form.GetValues("start").FirstOrDefault();
                    var length = Request.Form.GetValues("length").FirstOrDefault();
                    var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                            + "][name]").FirstOrDefault();
                    var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                    //var passengername = ""; //Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                    var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                    var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                    //var pickupaddress = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt16(start) : 0;
                    int recordsTotal = 0;

                    List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                                join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                                                where (f.Status.ToLower() == ("Confirm").ToLower() || f.Status.ToLower() == ("Driver").ToLower()) && (fs.Status.ToLower() == ("Confirm").ToLower() || fs.Status.ToLower() == ("Driver").ToLower())
                                                select new clsBooking
                                                {
                                                    RecordID = fs.RecordID,
                                                    BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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

                                                    PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                                    MeetGreetStr = (f.MeetGreet == 1) ? "Y" : "N",
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
                                                    LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                    BidCloseDate = f.BidCloseDate,
                                                    BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                    BidPostRemarks = f.BidPostRemarks ?? "",
                                                    imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                                }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                    //SEARCHING...
                    if (!string.IsNullOrEmpty(passengername))
                    {
                        invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                    }
                    //else if (!string.IsNullOrEmpty(email))
                    //{
                    //    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                    //}
                    //else if (!string.IsNullOrEmpty(cell1))
                    //{
                    //    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                    //}
                    else if (!string.IsNullOrEmpty(pickupaddress))
                    {
                        invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower())).ToList();
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
        #endregion

            #region ConfirmForEmailList

            public ActionResult ConfirmForEmailList()
            {
                if (User != null)
                {
                    clsUserRight rights = new clsUserRight();
                    rights = (from r in db.Admin_Setup_UserRights
                              where r.UserID == User.UserId && r.MenuName == "ConfirmForEmailList"
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
                        return View();
                    }
                }
                else
                {
                    return RedirectToAction("Index", "account");
                }
            }



            public ActionResult GetAllConfirmForEmailJobList(string status, string passengername, string pickupaddress)
            {
                string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
                if (User != null)
                {
                    var draw = Request.Form.GetValues("draw").FirstOrDefault();
                    var start = Request.Form.GetValues("start").FirstOrDefault();
                    var length = Request.Form.GetValues("length").FirstOrDefault();
                    var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                            + "][name]").FirstOrDefault();
                    var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                    //var passengername = ""; //Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                    var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                    var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                    //var pickupaddress = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt16(start) : 0;
                    int recordsTotal = 0;

                    List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                                join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                                                where (f.Status == "ConfirmForEmail" || f.Status == "DriverForEmail") && (fs.Status == "ConfirmForEmail" || fs.Status == "DriverForEmail")
                                                select new clsBooking
                                                {
                                                    RecordID = fs.RecordID,
                                                    BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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


                                                    PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                                    MeetGreetStr = (f.MeetGreet == 1) ? "Y" : "N",
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
                                                    LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                    BidCloseDate = f.BidCloseDate,
                                                    BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                    BidPostRemarks = f.BidPostRemarks ?? "",
                                                    imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",
                                                }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                    //SEARCHING...
                    if (!string.IsNullOrEmpty(passengername))
                    {
                        invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                    }
                    //else if (!string.IsNullOrEmpty(email))
                    //{
                    //    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                    //}
                    //else if (!string.IsNullOrEmpty(cell1))
                    //{
                    //    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                    //}
                    else if (!string.IsNullOrEmpty(pickupaddress))
                    {
                        invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower())).ToList();
                    }
                    //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                    //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                    //{
                    //    if (!(string.IsNullOrEmpty(sortColumn)))
                    //    {
                    //        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    //    }
                    //}


                    //invoice = invoice.OrderByDescending(x => x.BookingId).ToList();
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


            public ActionResult ConfirmedJobForEmail(int? id, string remk)
            {
                string message = "";
                bool status = false;

                string result = spUpdateConfirmedJob(id, remk, "ConfirmForEmail");
                if (Convert.ToInt32(result) != 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "OK";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
                return new JsonResult { Data = new { status = status, message = message } };
            }

            public ActionResult ConfirmedJob(int? id, string remk)
            {
                string message = "";
                bool status = false;

                string result = spUpdateConfirmedJob(id, remk, "Confirm");
                if (Convert.ToInt32(result) != 0)
                {
                    ModelState.Clear();
                    int bookid = db.Cab_Bid.Where(x => x.RecordID == id).Select(x => x.BookingId).FirstOrDefault() ?? 0;
                    decimal detailcode = db.Cab_Bid.Where(x => x.RecordID == id).Select(x => x.DetailCode).FirstOrDefault() ?? 0;
                    string emailresult = createemails(bookid, "confirmed", detailcode, "");
                    status = true;
                    message = "OK";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
                return new JsonResult { Data = new { status = status, message = message } };
            }


            public ActionResult ConfirmedJobDone(int? id, string remk)
            {
                string message = "";
                bool status = false;

                string result = spUpdateConfirmedJob(id, remk, "Done");
                if (Convert.ToInt32(result) != 0)
                {
                    ModelState.Clear();
                    //int bookid = db.Cab_Bid.Where(x => x.RecordID == id).Select(x => x.BookingId).FirstOrDefault() ?? 0;
                    //decimal detailcode = db.Cab_Bid.Where(x => x.RecordID == id).Select(x => x.DetailCode).FirstOrDefault() ?? 0;
                    //string emailresult = createemails(bookid, "confirmed", detailcode);
                    status = true;
                    message = "OK";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
                return new JsonResult { Data = new { status = status, message = message } };
            }

            private string spUpdateConfirmedJob(int? BookingID, string Remarks, string Status)
            {
                string returnId = "";
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("spUpdateConfirmedJob", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@RecordID", SqlDbType.Int).Value = BookingID;
                            cmd.Parameters.Add("@Status", SqlDbType.VarChar, 50).Value =Status;
                            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                            cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
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
            #endregion

            #region Manual Booking


            public List<clsCabServiceProvider> getCabServiceProvider()
            {
                List<clsCabServiceProvider> detail = new List<clsCabServiceProvider>();
                detail = (from c in db.Cab_ServiceProvider
                          select new clsCabServiceProvider
                        {
                            ServiceProviderID = c.ServiceProviderID,
                            ServierProviderName = c.ServierProviderName
                        }).ToList();
                return detail;
            }

            public List<clsCabType> getCabType()
            {
                List<clsCabType> detail = new List<clsCabType>();
                detail = (from c in db.Cab_CabType
                          select new clsCabType
                        {
                            CabTypeID = c.CabTypeID,
                            Description = c.Description
                        }).ToList();
                return detail;
            }
            public List<clsCabLuggage> getLuggage()
            {
                List<clsCabLuggage> detail = new List<clsCabLuggage>();
                detail = (from c in db.Cab_Luggage
                          select new clsCabLuggage
                          {
                              LuggageID = c.LuggageID,
                              Description = c.Description
                          }).ToList();
                return detail;
            }
            public ActionResult ManualBooking(int? id)
            {
                if (User != null)
                {
                    clsUserRight rights = new clsUserRight();
                    rights = (from r in db.Admin_Setup_UserRights
                              where r.UserID == User.UserId && r.MenuName == "ManualBooking"
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
                        string idstr = "";
                        if (id == null)
                        {
                            idstr = "0";
                        }
                        else
                        {
                            idstr = id.ToString();
                        }
                        ViewBag.ID = idstr;
                        ViewBag.CabType = new SelectList(getCabType(), "CabTypeID", "Description");
                        ViewBag.Luggage = new SelectList(getLuggage(), "LuggageID", "Description");
                        ViewBag.ServiceProvider = new SelectList(getCabServiceProvider(), "ServiceProviderID", "ServierProviderName");
                        return View();
                    }
                }
                else
                {
                    return RedirectToAction("Index", "account");
                }
            }


            [HttpPost]
            public ActionResult InsertUpdateBooking(string model)
            {
                string data = "";
                if (ModelState.IsValid)
                {
                    clsBooking Obj = JsonConvert.DeserializeObject<clsBooking>(model);
                    int vrow = 0;
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Id");
                    dt.Columns.Add("LuggageId");

                    if (Obj.LuggageDetail != null)
                    {
                        if (Obj.LuggageDetail != "")
                        {
                            string[] multiArray = Obj.LuggageDetail.Split(new Char[] { '^' });
                            foreach (string sublist in multiArray)
                            {
                                if (sublist.Trim() != "")
                                {
                                    int vlug = Convert.ToInt32(sublist);
                                    vrow = vrow + 1;
                                    dt.Rows.Add(new object[] { vrow, vlug });
                                }
                            }
                        }
                        else
                        {
                            dt.Rows.Add(new object[] { 0, 0 });
                        }
                    }
                    else
                    {
                        dt.Rows.Add(new object[] { 0, 0 });
                    }


                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("spInsertBooking", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@BookingId", SqlDbType.Int).Value = Obj.BookingId;
                            cmd.Parameters.Add("@PickupDateTime", SqlDbType.DateTime).Value = Obj.PickupDateTime;
                            cmd.Parameters.Add("@PickupTime", SqlDbType.DateTime).Value = Obj.PickupTime;
                            cmd.Parameters.Add("@PickupPostalCode", SqlDbType.NVarChar, 50).Value = Obj.PickupPostalCode;
                            cmd.Parameters.Add("@PickupPostalCode1", SqlDbType.NVarChar, 50).Value = Obj.PickupPostalCode1;
                            cmd.Parameters.Add("@PickupPostalCode2", SqlDbType.NVarChar, 50).Value = Obj.PickupPostalCode2;
                            cmd.Parameters.Add("@PickupPostalCode3", SqlDbType.NVarChar, 50).Value = Obj.PickupPostalCode3;
                            cmd.Parameters.Add("@PickupLongitude", SqlDbType.NVarChar, 50).Value = Obj.PickupLongitude;
                            cmd.Parameters.Add("@PickupLongitude1", SqlDbType.NVarChar, 50).Value = Obj.PickupLongitude1;
                            cmd.Parameters.Add("@PickupLongitude2", SqlDbType.NVarChar, 50).Value = Obj.PickupLongitude2;
                            cmd.Parameters.Add("@PickupLongitude3", SqlDbType.NVarChar, 50).Value = Obj.PickupLongitude3;
                            cmd.Parameters.Add("@PickupLatitude", SqlDbType.NVarChar, 50).Value = Obj.PickupLatitude;
                            cmd.Parameters.Add("@PickupLatitude1", SqlDbType.NVarChar, 50).Value = Obj.PickupLatitude1;
                            cmd.Parameters.Add("@PickupLatitude2", SqlDbType.NVarChar, 50).Value = Obj.PickupLatitude2;
                            cmd.Parameters.Add("@PickupLatitude3", SqlDbType.NVarChar, 50).Value = Obj.PickupLatitude3;
                            cmd.Parameters.Add("@DropoffPostalCode", SqlDbType.NVarChar, 50).Value = Obj.DropoffPostalCode;
                            cmd.Parameters.Add("@DropoffPostalCode1", SqlDbType.NVarChar, 50).Value = Obj.DropoffPostalCode1;
                            cmd.Parameters.Add("@DropoffPostalCode2", SqlDbType.NVarChar, 50).Value = Obj.DropoffPostalCode2;
                            cmd.Parameters.Add("@DropoffPostalCode3", SqlDbType.NVarChar, 50).Value = Obj.DropoffPostalCode3;
                            cmd.Parameters.Add("@DropoffLongitude", SqlDbType.NVarChar, 50).Value = Obj.DropoffLongitude;
                            cmd.Parameters.Add("@DropoffLongitude1", SqlDbType.NVarChar, 50).Value = Obj.DropoffLongitude1;
                            cmd.Parameters.Add("@DropoffLongitude2", SqlDbType.NVarChar, 50).Value = Obj.DropoffLongitude2;
                            cmd.Parameters.Add("@DropoffLongitude3", SqlDbType.NVarChar, 50).Value = Obj.DropoffLongitude3;
                            cmd.Parameters.Add("@DropoffLatitude", SqlDbType.NVarChar, 50).Value = Obj.DropoffLatitude;
                            cmd.Parameters.Add("@DropoffLatitude1", SqlDbType.NVarChar, 50).Value = Obj.DropoffLatitude1;
                            cmd.Parameters.Add("@DropoffLatitude2", SqlDbType.NVarChar, 50).Value = Obj.DropoffLatitude2;
                            cmd.Parameters.Add("@DropoffLatitude3", SqlDbType.NVarChar, 50).Value = Obj.DropoffLatitude3;
                            cmd.Parameters.Add("@CabId", SqlDbType.Int).Value = Obj.CabId;
                            cmd.Parameters.Add("@NoofPassenger", SqlDbType.Int).Value = Obj.NoofPassenger;
                            cmd.Parameters.Add("@Luggagekg", SqlDbType.Decimal).Value = Obj.Luggagekg;
                            cmd.Parameters.Add("@LuggageID", SqlDbType.Int).Value = 0;
                            cmd.Parameters.Add("@ChildSeat", SqlDbType.Int).Value = Obj.ChildSeat;
                            cmd.Parameters.Add("@DisableSeat", SqlDbType.Int).Value = Obj.DisableSeat;
                            cmd.Parameters.Add("@Surname", SqlDbType.NVarChar, 50).Value = Obj.SurName;
                            cmd.Parameters.Add("@PasengerName", SqlDbType.NVarChar, 50).Value = Obj.PasengerName;
                            cmd.Parameters.Add("@Cellno", SqlDbType.NVarChar, 50).Value = Obj.Cellno;
                            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = Obj.Email;
                            cmd.Parameters.Add("@Payment", SqlDbType.NVarChar, 50).Value = Obj.Payment;
                            cmd.Parameters.Add("@PaybalEmail", SqlDbType.NVarChar, 150).Value = Obj.PaybalEmail;
                            cmd.Parameters.Add("@CreditCvvCode", SqlDbType.NVarChar, 50).Value = Obj.CreditCvvCode;
                            cmd.Parameters.Add("@CardFirstName", SqlDbType.NVarChar, 50).Value = Obj.CardFirstName;
                            cmd.Parameters.Add("@CreditCardNo", SqlDbType.NVarChar, 50).Value = Obj.CreditCardNo;
                            cmd.Parameters.Add("@CreditMonth", SqlDbType.Int).Value = Obj.CreditMonth;
                            cmd.Parameters.Add("@CreditYear", SqlDbType.Int).Value = Obj.CreditYear;
                            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = "Active";
                            cmd.Parameters.Add("@TotalKm", SqlDbType.Decimal).Value = Obj.TotalKm;
                            cmd.Parameters.Add("@ReturnTrip", SqlDbType.Int).Value = Obj.ReturnTrip;
                            cmd.Parameters.Add("@ReturnDate", SqlDbType.DateTime).Value = Obj.ReturnDate;
                            cmd.Parameters.Add("@ReturnTime", SqlDbType.DateTime).Value = Obj.ReturnTime;
                            cmd.Parameters.Add("@MeetGreet", SqlDbType.Int).Value = Obj.MeetGreet;
                            cmd.Parameters.Add("@TermsCheck", SqlDbType.Int).Value = Obj.TermsCheck;
                            cmd.Parameters.Add("@NoOfChild", SqlDbType.Int).Value = Obj.NoOfChild;
                            cmd.Parameters.Add("@ArrivalFrom", SqlDbType.NVarChar, 100).Value = Obj.ArrivalFrom;
                            cmd.Parameters.Add("@FlightNo", SqlDbType.NVarChar, 50).Value = Obj.FlightNo;
                            cmd.Parameters.Add("@FlightTime", SqlDbType.DateTime).Value = Obj.FlightTime;
                            cmd.Parameters.Add("@Terminal", SqlDbType.NVarChar, 50).Value = Obj.Terminal;
                            cmd.Parameters.Add("@DoorNo", SqlDbType.NVarChar, 50).Value = Obj.DoorNo;
                            cmd.Parameters.Add("@AdditionalInfo", SqlDbType.NVarChar, 500).Value = Obj.AdditionalInfo;
                            cmd.Parameters.Add("@calculatedfare", SqlDbType.Decimal).Value = Obj.calculatedfare;
                            cmd.Parameters.Add("@EstimateFareNet", SqlDbType.Decimal).Value = Obj.EstimateFareNet;
                            cmd.Parameters.Add("@PickupAddress", SqlDbType.NVarChar, 255).Value = Obj.Startpoint;
                            cmd.Parameters.Add("@PickupAddress1", SqlDbType.NVarChar, 255).Value = Obj.Startpoint1;
                            cmd.Parameters.Add("@PickupAddress2", SqlDbType.NVarChar, 255).Value = Obj.Startpoint2;
                            cmd.Parameters.Add("@PickupAddress3", SqlDbType.NVarChar, 255).Value = Obj.Startpoint3;
                            cmd.Parameters.Add("@DropOffAddress", SqlDbType.NVarChar, 255).Value = Obj.Endposition;
                            cmd.Parameters.Add("@DropOffAddress1", SqlDbType.NVarChar, 255).Value = Obj.Endposition1;
                            cmd.Parameters.Add("@DropOffAddress2", SqlDbType.NVarChar, 255).Value = Obj.Endposition2;
                            cmd.Parameters.Add("@DropOffAddress3", SqlDbType.NVarChar, 255).Value = Obj.Endposition3;
                            cmd.Parameters.Add("@ManualPickupAddress", SqlDbType.NVarChar, 255).Value = Obj.ManualPickupAddress;
                            cmd.Parameters.Add("@ManualDropOffAddress", SqlDbType.NVarChar, 255).Value = Obj.ManualDropOffAddress;
                            cmd.Parameters.Add("@dtDetail", SqlDbType.Structured).Value = dt;
                            cmd.Parameters.Add("@PromotionID", SqlDbType.Int).Value = Obj.PromotionID;

                            cmd.Parameters.Add("@ServiceProviderID", SqlDbType.Int).Value = Obj.ServiceProviderID;
                            cmd.Parameters.Add("@SPRefNo", SqlDbType.NVarChar, 100).Value = Obj.SPRefNo;
                            cmd.Parameters.Add("@PaymentMode ", SqlDbType.NVarChar, 50).Value = Obj.PaymentMode ;
                            cmd.Parameters.Add("@SPAmount ", SqlDbType.Decimal).Value = Obj.SPAmount ;
                            cmd.Parameters.Add("@PaymentStatus ", SqlDbType.NVarChar, 50).Value = Obj.PaymentStatus ;
                            
                            cmd.Parameters.Add("@CheckReturn", SqlDbType.Int).Direction = ParameterDirection.Output;
                            cmd.ExecuteNonQuery();
                            //data = Convert.ToInt32(cmd.Parameters["@CheckReturn"].Value);
                            data = cmd.Parameters["@CheckReturn"].Value.ToString();
                            cmd.Dispose();
                        }
                        con.Close();
                        con.Dispose();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }



                if (data != "0")
                {
                    string receiver = System.Configuration.ConfigurationManager.ConnectionStrings["AdminEmail"].ConnectionString;

                    clsBooking bookingdetail = new clsBooking();
                    bookingdetail = BookingDetailEmail(Convert.ToInt32(data));
                    string message = "";
                    string vpassemail = bookingdetail.Email;
                    string vpassname = bookingdetail.PasengerName;


                    string pickuptime = ("00" + (bookingdetail.PickupTimeT.Value.Hours > 12 ? bookingdetail.PickupTimeT.Value.Hours - 12 : bookingdetail.PickupTimeT.Value.Hours).ToString()).Substring(("00" + (bookingdetail.PickupTimeT.Value.Hours > 12 ? bookingdetail.PickupTimeT.Value.Hours - 12 : bookingdetail.PickupTimeT.Value.Hours).ToString()).Length - 2) + ":" + ("00" + bookingdetail.PickupTimeT.Value.Minutes.ToString()).Substring(("00" + bookingdetail.PickupTimeT.Value.Minutes.ToString()).Length - 2) + " " + (bookingdetail.PickupTimeT.Value.Hours >= 12 ? "PM" : "AM");
                    string flighttime = ("00" + (bookingdetail.FlightTimeT.Value.Hours > 12 ? bookingdetail.FlightTimeT.Value.Hours - 12 : bookingdetail.FlightTimeT.Value.Hours).ToString()).Substring(("00" + (bookingdetail.FlightTimeT.Value.Hours > 12 ? bookingdetail.FlightTimeT.Value.Hours - 12 : bookingdetail.FlightTimeT.Value.Hours).ToString()).Length - 2) + ":" + ("00" + bookingdetail.FlightTimeT.Value.Minutes.ToString()).Substring(("00" + bookingdetail.FlightTimeT.Value.Minutes.ToString()).Length - 2) + " " + (bookingdetail.FlightTimeT.Value.Hours >= 12 ? "PM" : "AM");

                    message += "<span style='font-size:12px;'>Booking No. :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.BookingId.ToString() + "</span>";
                    message += "<br/><span style='font-size:12px;'>Booked By :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.PasengerName + "</span>";
                    message += "<br/><span style='font-size:12px;'>Email :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.Email + "</span>";
                    message += "<br/><span style='font-size:12px;'>Mobile No. :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.Cellno + "</span>";
                    message += "<br/><span style='font-size:12px;'>Pickup Date-Time :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.PickupDateTime.Value.ToString("dd-MM-yyyy") + " " + pickuptime + "</span>";
                    message += "<br/><span style='font-size:12px;'>Pickup from :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.Startpoint + "</span>";
                    message += "<br/><span style='font-size:12px;'>Door No. :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.DoorNo + "</span>";
                    message += "<br/><span style='font-size:12px;'>Dropoff at :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.Endposition + "</span>";
                    message += "<br/><span style='font-size:12px;'>Adult(s) :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.NoOfAdult.ToString() + "</span>";
                    message += "<br/><span style='font-size:12px;'>Child(s) :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.NoOfChild.ToString() + "</span>";
                    message += "<br/><span style='font-size:12px;'>Distance(m) :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.TotalKm.ToString() + "</span>";
                    message += "<br/><span style='font-size:12px;'>Est. Fare :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.EstimateFareNet.ToString() + "</span>";
                    message += "<br/><span style='font-size:12px;'>Cab Type :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.CabTypeName + "</span>";
                    message += "<br/><span style='font-size:12px;'>Flight No. :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.FlightNo + "</span>";
                    message += "<br/><span style='font-size:12px;'>Arrival From :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.ArrivalFrom + "</span>";
                    message += "<br/><span style='font-size:12px;'>Terminal :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.Terminal + "</span>";
                    message += "<br/><span style='font-size:12px;'>Flight Time :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + flighttime + "</span>";

                    //message += "Booking No. : " + bookingdetail.BookingId.ToString();
                    //message += "\nBooked By : " + bookingdetail.PasengerName;
                    //message += "\nEmail : " + bookingdetail.Email;
                    //message += "\nMobile No. : " + bookingdetail.Cellno;
                    //message += "\nPickup Date-Time : " + bookingdetail.PickupDateTime.Value.ToString("dd-MM-yyyy") + " " + pickuptime;
                    //message += "\nPickup from : " + bookingdetail.Startpoint;
                    //message += "\nDoor No. : " + bookingdetail.DoorNo;
                    //message += "\nDropoff at : " + bookingdetail.Endposition;
                    //message += "\nAdult(s) : " + bookingdetail.NoOfAdult.ToString();
                    //message += "\nChild(s) : " + bookingdetail.NoOfChild.ToString();
                    //message += "\nDistance(m) : " + bookingdetail.TotalKm.ToString();
                    //message += "\nEst. Fare : " + bookingdetail.EstimateFareNet.ToString();
                    //message += "\nCab Type : " + bookingdetail.CabTypeName;

                    //message += "\nFlight No. : " + bookingdetail.FlightNo;
                    //message += "\nArrival From : " + bookingdetail.ArrivalFrom;
                    //message += "\nTerminal : " + bookingdetail.Terminal;
                    //message += "\nFlight Time : " + flighttime;
                                        
                    string emailresult = SendEmail(receiver, "Admin","New Booking", "Booking from Tiklacars.com", message);
                }


                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public clsBooking BookingDetailEmail(int? id)
        {
            
            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             where f.BookingId == id
                             select new clsBooking
                             {
                                 BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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
                                 ManualPickupAddress = f.ManualPickupAddress ?? "",
                                 ManualDropOffAddress = f.ManualDropOffAddress ?? "",


                                 ServiceProviderID = f.ServiceProviderID ?? 0,
                                 ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                 PaymentMode = f.PaymentMode ?? "",
                                 SPRefNo = f.SPRefNo ?? "",
                                 SPAmount = f.SPAmount ?? 0,


                                 PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
                                 PickupTimeT = f.PickupTime,
                                 //PickupTimeString = ("00" + f.PickupTime.Value.Hours.ToString()).Substring(("00" + f.PickupTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.PickupTime.Value.Minutes.ToString()).Substring(("00" + f.PickupTime.Value.Minutes.ToString()).Length - 2),
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
                                 FlightTimeT=f.FlightTime,
                                 //FlightTimeString = ("00" + f.FlightTime.Value.Hours.ToString()).Substring(("00" + f.FlightTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.FlightTime.Value.Minutes.ToString()).Substring(("00" + f.FlightTime.Value.Minutes.ToString()).Length - 2),
                                 DoorNo = f.DoorNo,

                                 ReturnDate = f.ReturnDate,
                                 //ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                 LuggageID = f.LuggageID ?? 0,
                                 LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                 BidCloseDate = f.BidCloseDate,
                                 //BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                 BidPostRemarks = f.BidPostRemarks ?? "",
                             }).FirstOrDefault();

            return bookingdetail;
        }
        
        public string SendEmail(string receiver, string receivername,string sendername, string subject, string message)
        {
            string vresult="";
            try
            {
                if (ModelState.IsValid)
                {
                    //var senderEmail = new MailAddress("tiklacarsuk@gmail.com", sendername);
                    var senderEmail = new MailAddress("qeplahore@hotmail.com", sendername);
                    var receiverEmail = new MailAddress(receiver, receivername);
                    var password = System.Configuration.ConfigurationManager.ConnectionStrings["EmailPassword"].ConnectionString;
                    var sub = subject;
                    var body = message;
                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };
                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = subject,
                        Body = body ,
                        IsBodyHtml = true
                    })
                    {
                        smtp.Send(mess);
                    }
                    vresult="OK";
                }
            }
            catch (Exception)
            {
                vresult="Error";
            }
            return vresult;
        }


        public ActionResult DirectEmail(int? id)
        {

            string data = "OK";
            if (id != 0)
            {
                string receiver = System.Configuration.ConfigurationManager.ConnectionStrings["AdminEmail"].ConnectionString;

                clsBooking bookingdetail = new clsBooking();
                bookingdetail = BookingDetailEmail(id);
                string message = "";
                string vpassemail = bookingdetail.Email;
                string vpassname = bookingdetail.PasengerName;

                string pickuptime = ("00" + (bookingdetail.PickupTimeT.Value.Hours > 12 ? bookingdetail.PickupTimeT.Value.Hours - 12 : bookingdetail.PickupTimeT.Value.Hours).ToString()).Substring(("00" + (bookingdetail.PickupTimeT.Value.Hours > 12 ? bookingdetail.PickupTimeT.Value.Hours - 12 : bookingdetail.PickupTimeT.Value.Hours).ToString()).Length - 2) + ":" + ("00" + bookingdetail.PickupTimeT.Value.Minutes.ToString()).Substring(("00" + bookingdetail.PickupTimeT.Value.Minutes.ToString()).Length - 2) +  " " +(bookingdetail.PickupTimeT.Value.Hours >= 12 ? "PM" : "AM");
                string flighttime = ("00" + (bookingdetail.FlightTimeT.Value.Hours > 12 ? bookingdetail.FlightTimeT.Value.Hours - 12 : bookingdetail.FlightTimeT.Value.Hours).ToString()).Substring(("00" + (bookingdetail.FlightTimeT.Value.Hours > 12 ? bookingdetail.FlightTimeT.Value.Hours - 12 : bookingdetail.FlightTimeT.Value.Hours).ToString()).Length - 2) + ":" + ("00" + bookingdetail.FlightTimeT.Value.Minutes.ToString()).Substring(("00" + bookingdetail.FlightTimeT.Value.Minutes.ToString()).Length - 2) + " " + (bookingdetail.FlightTimeT.Value.Hours >= 12 ? "PM" : "AM");

                message += "<span style='font-size:12px;'>Booking No. :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.BookingId.ToString() + "</span>";
                message += "<br/><span style='font-size:12px;'>Booked By :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.PasengerName + "</span>";
                message += "<br/><span style='font-size:12px;'>Email :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.Email + "</span>";
                message += "<br/><span style='font-size:12px;'>Mobile No. :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.Cellno + "</span>";
                message += "<br/><span style='font-size:12px;'>Pickup Date-Time :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.PickupDateTime.Value.ToString("dd-MM-yyyy") + " " + pickuptime + "</span>";
                message += "<br/><span style='font-size:12px;'>Pickup from :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.Startpoint + "</span>";
                message += "<br/><span style='font-size:12px;'>Door No. :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.DoorNo + "</span>";
                message += "<br/><span style='font-size:12px;'>Dropoff at :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.Endposition + "</span>";
                message += "<br/><span style='font-size:12px;'>Adult(s) :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.NoOfAdult.ToString() + "</span>";
                message += "<br/><span style='font-size:12px;'>Child(s) :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.NoOfChild.ToString() + "</span>";
                message += "<br/><span style='font-size:12px;'>Distance(m) :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.TotalKm.ToString() + "</span>";
                message += "<br/><span style='font-size:12px;'>Est. Fare :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.EstimateFareNet.ToString() + "</span>";
                message += "<br/><span style='font-size:12px;'>Cab Type :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.CabTypeName + "</span>";
                message += "<br/><span style='font-size:12px;'>Flight No. :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.FlightNo + "</span>";
                message += "<br/><span style='font-size:12px;'>Arrival From :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.ArrivalFrom + "</span>";
                message += "<br/><span style='font-size:12px;'>Terminal :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + bookingdetail.Terminal + "</span>";
                message += "<br/><span style='font-size:12px;'>Flight Time :</span>&nbsp;&nbsp;<span style='font-size:14px;font-weight:bold;'>" + flighttime + "</span>";

                //message += "Booking No. : " + bookingdetail.BookingId.ToString();
                //message += "\nBooked By : " + bookingdetail.PasengerName;
                //message += "\nEmail : " + bookingdetail.Email;
                //message += "\nMobile No. : " + bookingdetail.Cellno;
                //message += "\nPickup Date-Time : " + bookingdetail.PickupDateTime.Value.ToString("dd-MM-yyyy") + " " + pickuptime;
                //message += "\nPickup from : " + bookingdetail.Startpoint;
                //message += "\nDoor No. : " + bookingdetail.DoorNo;
                //message += "\nDropoff at : " + bookingdetail.Endposition;
                //message += "\nAdult(s) : " + bookingdetail.NoOfAdult.ToString();
                //message += "\nChild(s) : " + bookingdetail.NoOfChild.ToString();
                //message += "\nDistance(m) : " + bookingdetail.TotalKm.ToString();
                //message += "\nEst. Fare : " + bookingdetail.EstimateFareNet.ToString();
                //message += "\nCab Type : " + bookingdetail.CabTypeName;

                //message += "\nFlight No. : " + bookingdetail.FlightNo;
                //message += "\nArrival From : " + bookingdetail.ArrivalFrom;
                //message += "\nTerminal : " + bookingdetail.Terminal;
                //message += "\nFlight Time : " + flighttime;

                string emailresult = SendEmail(receiver, "Admin", "New Booking", "Booking from Tiklacars.com", message);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AllBookingDetail()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "AllBookingList"
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
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult GetAllBookingsByFilter(string status, string passengername, string pickupaddress)
        {
            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();                
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            where (f.Status.ToLower() != "cancel" && f.Status.ToLower() != "complete")  
                                            select new clsBooking
                                            {

                                                BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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

                                                ServiceProviderID= f.ServiceProviderID ?? 0,
                                                ServierProviderName =db.Cab_ServiceProvider.Where(x=>x.ServiceProviderID == f.ServiceProviderID).Select(x=>x.ServierProviderName).FirstOrDefault() ?? "",
                                                PaymentMode=f.PaymentMode ?? "",
                                                SPRefNo =f.SPRefNo ?? "",
                                                SPAmount=f.SPAmount ?? 0,

                                                PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                                Status=f.Status, 
                                                MaxAmount = f.BidAmount ?? 0,

                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",

                                                LogStatus=f.LogStatus ?? "",
                                                DriverArrivedDateTime=f.DriverArrivedDateTime,
                                                POBDateTime=f.POBDateTime,
                                                DoneDateTime=f.DoneDateTime,

                                            }).OrderByDescending(x => x.PickupDateTime).ThenByDescending(x => x.PickupTimeSpan).ThenByDescending(x => x.BookingId).ToList();

                
                if (!string.IsNullOrEmpty(pickupaddress))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower()) || c.Endposition.ToLower().Contains(pickupaddress.ToLower())).ToList();
                }

                if (!string.IsNullOrEmpty(passengername))
                {
                    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                }

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                //{
                //    if (!(string.IsNullOrEmpty(sortColumn)))
                //    {
                //        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                //    }
                //}
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
       #endregion





        #region InProcess Job

        public ActionResult AllBookingDetailInProcess()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "AllBookingDetailInProcess"
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
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult GetAllBookingsProcessByFilter(string status, string passengername, string pickupaddress)
        {
            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();                
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            //join fs in db.Cab_Bid on f.BookingId equals fs.BookingId 
                                            //join a in db.AC_Detail on fs.DetailCode equals a.DetailCode
                                            where (f.Status.ToLower() != "cancel" && f.Status.ToLower() != "complete" && f.Status.ToLower() != "done")
                                            //&& (fs.Status.ToLower() != "na" && fs.Status.ToLower() != "bid" && fs.Status.ToLower() != "pending" && fs.Status.ToLower() != "cancel")
                                            select new clsBooking
                                            {

                                                BookingId = f.BookingId,
                                                WorkSection = f.WorkSection ?? "",
                                                JobNo = f.JobNo ?? "",
                                                PaymentStatus = ((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus,
                                                PaybalEmail = f.PaybalEmail ?? "",
                                                CreditCardNo = f.CreditCardNo ?? "",
                                                PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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
                                                Status = f.Status.ToUpper(),
                                                MaxAmount = f.BidAmount ?? 0,

                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",


                                                SuppBidAmount = db.Cab_Bid.Where(x => x.RecordID == f.RecordID).Select(x => x.BidAmount).FirstOrDefault() ?? 0,
                                                DetailCode = db.Cab_Bid.Where(x => x.RecordID == f.RecordID).Select(x => x.DetailCode).FirstOrDefault() ?? 0,
                                                DetailName = (from b in db.Cab_Bid join a in db.AC_Detail on b.DetailCode equals a.DetailCode where b.RecordID == f.RecordID select a.Name).FirstOrDefault() ?? "",

                                                
                                                LogStatus = f.LogStatus ?? "",
                                                DriverArrivedDateTime = f.DriverArrivedDateTime,
                                                POBDateTime = f.POBDateTime,
                                                DoneDateTime = f.DoneDateTime,


                                                DriverArrivedDateTimeStr = f.DriverArrivedDateTime != null ? ("00" + f.DriverArrivedDateTime.Value.Day.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.DriverArrivedDateTime.Value.Month.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Month.ToString()).Length - 2) + "-" + f.DriverArrivedDateTime.Value.Year.ToString() + " " + ("00" + f.DriverArrivedDateTime.Value.Hour.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Hour.ToString()).Length - 2) + ":" + ("00" + f.DriverArrivedDateTime.Value.Minute.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Minute.ToString()).Length - 2) : "",
                                                POBDateTimeStr = f.POBDateTime != null ? ("00" + f.POBDateTime.Value.Day.ToString()).Substring(("00" + f.POBDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.POBDateTime.Value.Month.ToString()).Substring(("00" + f.POBDateTime.Value.Month.ToString()).Length - 2) + "-" + f.POBDateTime.Value.Year.ToString() + " " + ("00" + f.POBDateTime.Value.Hour.ToString()).Substring(("00" + f.POBDateTime.Value.Hour.ToString()).Length - 2) + ":" + ("00" + f.POBDateTime.Value.Minute.ToString()).Substring(("00" + f.POBDateTime.Value.Minute.ToString()).Length - 2) : "",
                                                DoneDateTimeStr = f.DoneDateTime != null ? ("00" + f.DoneDateTime.Value.Day.ToString()).Substring(("00" + f.DoneDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.DoneDateTime.Value.Month.ToString()).Substring(("00" + f.DoneDateTime.Value.Month.ToString()).Length - 2) + "-" + f.DoneDateTime.Value.Year.ToString() + " " + ("00" + f.DoneDateTime.Value.Hour.ToString()).Substring(("00" + f.DoneDateTime.Value.Hour.ToString()).Length - 2) + ":" + ("00" + f.DoneDateTime.Value.Minute.ToString()).Substring(("00" + f.DoneDateTime.Value.Minute.ToString()).Length - 2) : "",


                                            }).OrderBy(x => x.PickupDateTime).ThenBy(x => x.PickupTimeSpan).ThenBy(x => x.BookingId).ToList();


                if (!string.IsNullOrEmpty(pickupaddress))
                {
                    //invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower()) || c.Endposition.ToLower().Contains(pickupaddress.ToLower())).ToList();
                    invoice = invoice.Where(c => c.JobNo.ToLower().Contains(pickupaddress.ToLower())).ToList();
                    
                }

                if (!string.IsNullOrEmpty(passengername))
                {
                    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
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
        #endregion








        #region All Boooking List Job

        public ActionResult AllBookingList()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "AllJobsList"
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
                    ViewBag.Status = new SelectList(getStatus(), "Status", "Status");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public List<clsItem> getStatus()
        {
            List<clsItem> detail = new List<clsItem>();
            detail = (from c in db.Cab_BookingMain
                      select new clsItem
                      {
                          Status = c.Status.ToUpper()
                      }).Distinct().ToList();
            return detail;
        }

        public ActionResult GetAllBookingsListByFilter(string status, string passengername, string pickupaddress)
        {
            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var cell1 = ""; // Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = ""; //Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();                
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                
                

                //join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                //                            where (fs.Status.ToLower() != "na" && fs.Status.ToLower() != "bid" && fs.Status.ToLower() != "pending" && fs.Status.ToLower() != "cancel")


                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            
                                            select new clsBooking
                                            {
                                                BookingId = f.BookingId,
                                                WorkSection = f.WorkSection ?? "",
                                                JobNo = f.JobNo ?? "",
                                                PaymentStatus = ((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus,
                                                PaybalEmail = f.PaybalEmail ?? "",
                                                CreditCardNo = f.CreditCardNo ?? "",
                                                PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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
                                                Status = f.Status.ToUpper(),
                                                MaxAmount = f.BidAmount ?? 0,

                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",


                                                SuppBidAmount = db.Cab_Bid.Where(x => x.RecordID == f.RecordID).Select(x => x.BidAmount).FirstOrDefault() ?? 0,
                                                DetailCode = db.Cab_Bid.Where(x=>x.RecordID == f.RecordID).Select(x=>x.DetailCode).FirstOrDefault() ?? 0,
                                                DetailName = (from b in db.Cab_Bid join a in db.AC_Detail on b.DetailCode equals a.DetailCode where b.RecordID == f.RecordID select a.Name).FirstOrDefault() ?? "",

                                                LogStatus = f.LogStatus ?? "",
                                                DriverArrivedDateTime = f.DriverArrivedDateTime,
                                                POBDateTime = f.POBDateTime,
                                                DoneDateTime = f.DoneDateTime,


                                                DriverArrivedDateTimeStr = f.DriverArrivedDateTime != null ? ("00" + f.DriverArrivedDateTime.Value.Day.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.DriverArrivedDateTime.Value.Month.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Month.ToString()).Length - 2) + "-" + f.DriverArrivedDateTime.Value.Year.ToString() + " " + ("00" + f.DriverArrivedDateTime.Value.Hour.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Hour.ToString()).Length - 2) + ":" + ("00" + f.DriverArrivedDateTime.Value.Minute.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Minute.ToString()).Length - 2) : "",
                                                POBDateTimeStr = f.POBDateTime != null ? ("00" + f.POBDateTime.Value.Day.ToString()).Substring(("00" + f.POBDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.POBDateTime.Value.Month.ToString()).Substring(("00" + f.POBDateTime.Value.Month.ToString()).Length - 2) + "-" + f.POBDateTime.Value.Year.ToString() + " " + ("00" + f.POBDateTime.Value.Hour.ToString()).Substring(("00" + f.POBDateTime.Value.Hour.ToString()).Length - 2) + ":" + ("00" + f.POBDateTime.Value.Minute.ToString()).Substring(("00" + f.POBDateTime.Value.Minute.ToString()).Length - 2) : "",
                                                DoneDateTimeStr = f.DoneDateTime != null ? ("00" + f.DoneDateTime.Value.Day.ToString()).Substring(("00" + f.DoneDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.DoneDateTime.Value.Month.ToString()).Substring(("00" + f.DoneDateTime.Value.Month.ToString()).Length - 2) + "-" + f.DoneDateTime.Value.Year.ToString() + " " + ("00" + f.DoneDateTime.Value.Hour.ToString()).Substring(("00" + f.DoneDateTime.Value.Hour.ToString()).Length - 2) + ":" + ("00" + f.DoneDateTime.Value.Minute.ToString()).Substring(("00" + f.DoneDateTime.Value.Minute.ToString()).Length - 2) : "",



                                            }).OrderByDescending(x => x.PickupDateTime).ThenByDescending(x => x.PickupTimeSpan).ThenBy(x => x.BookingId).ToList();


                if (!string.IsNullOrEmpty(pickupaddress))
                {
                    //invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower()) || c.Endposition.ToLower().Contains(pickupaddress.ToLower())).ToList();
                    invoice = invoice.Where(c => c.JobNo.ToLower().Contains(pickupaddress.ToLower())).ToList();

                }

                if (!string.IsNullOrEmpty(passengername))
                {
                    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                }

                List<clsBooking> invoicefinal = new List<clsBooking>();

                if (!String.IsNullOrEmpty(status))
                {
                    List<clsBooking> invoicesub = new List<clsBooking>();
                    string[] multiArray = status.Split(new Char[] { ',' });
                    foreach (string sublist in multiArray)
                    {
                        if (sublist.Trim() != "")
                        {
                            invoicesub = invoice.Where(x => x.Status.ToLower() == sublist.Trim().ToLower()).ToList();
                            invoicefinal.AddRange(invoicesub);
                        }
                    }
                }
                else
                {
                    invoicefinal = invoice.ToList();
                }


                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        invoicefinal = invoicefinal.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = invoicefinal.Count();
                var data = invoicefinal.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion



















        public ActionResult GetAllActiveBookings(string status)
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var passengername = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var cell1 = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var pickupaddress = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                             where f.Status.ToLower()==status.ToLower()
                                            select new clsBooking
                                             {       
                              
                                                 BookingId=f.BookingId,
                                                 CabId=f.CabId,
                                                 CabTypeName=db.Cab_CabType.Where(x=>x.CabTypeID == f.CabId).Select(x=>x.Description).FirstOrDefault(),
                                                 Startpoint=f.PickupAddress,
                                                 Endposition=f.DropOffAddress,
                                                 PickupDateTime=f.PickupDateTime,
                                                 PickupTimeString = ("00" + f.PickupTime.Value.Hours.ToString()).Substring(("00" + f.PickupTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.PickupTime.Value.Minutes.ToString()).Substring(("00" + f.PickupTime.Value.Minutes.ToString()).Length - 2),
                                                 NoOfAdult=f.NoofPassenger,
                                                 NoOfChild=f.NoOfChild,
                                                 SurName = f.SurName ?? "",
                                                 PasengerName=f.PasengerName,
                                                 Cellno=f.Cellno,
                                                 Email=f.Email,
                                                 Amount=f.Amount,
                                                 MaxAmount = f.BidAmount ?? 0,
                                                 EstimateFareNet=f.EstimateFareNet,
                                                 TotalKm=f.TotalKm,
                                                 ChildSeat=f.ChildSeat,
                                                 DisableSeat=f.DisableSeat,
                                                 AdditionalInfo=f.AdditionalInformation,
                                                 ReturnTrip=f.ReturnTrip,
                                                 FlightNo=f.FlightNo,
                                                 ArrivalFrom=f.ArrivalFrom,
                                                 Terminal=f.FlightTerminal,
                                                 FlightTimeString = ("00" + f.FlightTime.Value.Hours.ToString()).Substring(("00" + f.FlightTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.FlightTime.Value.Minutes.ToString()).Substring(("00" + f.FlightTime.Value.Minutes.ToString()).Length - 2),
                                                 DoorNo=f.DoorNo,
                                                 ReturnDate = f.ReturnDate,
                                                 ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                 LuggageID = f.LuggageID ?? 0,
                                                 LuggageDesc=db.Cab_Luggage.Where(x=>x.LuggageID == f.LuggageID).Select(x=>x.Description ).FirstOrDefault() ?? "",
                                                 BidCloseDate = f.BidCloseDate,
                                                 BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",

                                             }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                //SEARCHING...
                if (!string.IsNullOrEmpty(passengername))
                {
                    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(email))
                {
                    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(cell1))
                {
                    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(pickupaddress))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower())).ToList();
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



        
        public ActionResult PostBookingDetail(int? id)
        {
            ViewBag.BookID = id;

            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             where f.BookingId == id
                             select new clsBooking
                             {
                                 BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
                                 CabId = f.CabId,
                                 CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabId).Select(x => x.Description).FirstOrDefault(),
                                 Startpoint = f.PickupAddress,
                                 Endposition = f.DropOffAddress,
                                 PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                 BidPostRemarks=f.BidPostRemarks ?? "",

                             }).FirstOrDefault();

            return PartialView(bookingdetail);
        }


        public ActionResult PendingBookingDetail(int? id)
        {
            ViewBag.BookID = id;

            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             where f.BookingId == id
                             select new clsBooking
                             {
                                 BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
                                 CabId = f.CabId,
                                 CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabId).Select(x => x.Description).FirstOrDefault(),
                                 Startpoint = f.PickupAddress,
                                 Endposition = f.DropOffAddress,
                                 PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                 MaxAmount = f.EstimateFareNet - ((f.EstimateFareNet / 100) * 20),
                                 ReturnDate = f.ReturnDate,
                                 ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                 LuggageID = f.LuggageID ?? 0,
                                 LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                             }).FirstOrDefault();

            return PartialView(bookingdetail);
        }



        public ActionResult FinalBookingDetail(int? id)
        {
            ViewBag.BookID = id;

            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                             where fs.RecordID == id
                             select new clsBooking
                             {
                                 RecordID = fs.RecordID,
                                 BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
                                 CabId = f.CabId,
                                 CabTypeName = db.Cab_CabType.Where(x => x.CabTypeID == f.CabId).Select(x => x.Description).FirstOrDefault(),
                                 Startpoint = f.PickupAddress,
                                 Endposition = f.DropOffAddress,
                                 PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                 LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                 BidCloseDate = f.BidCloseDate,
                                 BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                 BidPostRemarks = f.BidPostRemarks ?? "",

                             }).FirstOrDefault();

            return PartialView(bookingdetail);
        }

        

        public ActionResult SaveBid(int? id, decimal? amount, string remk, string bidstatus)
        {
            string message = "";
            bool status = false;

            string result = spUpdateBooking(id, User.UserId, bidstatus, amount, remk);
            if (Convert.ToInt32(result) != 0)
            {
                ModelState.Clear();
                status = true;
                message = "OK";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }

        private string spUpdateBooking(int? BookingID, decimal? DetailCode, string Status, decimal? Amount, string Remarks)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spUpdateBid", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@BookingId", SqlDbType.Int).Value = BookingID;
                        cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = DetailCode;
                        cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = Amount;
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 255).Value = Remarks;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 50).Value = Status;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
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


        public ActionResult BidList()
        {
            if (User != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public ActionResult CancelList()
        {
            if (User != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        



        public ActionResult GetAllActiveBookingsByStatus(string status)
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var passengername = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var cell1 = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var email = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var pickupaddress = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;

                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            join fs in db.Cab_Bid on f.BookingId equals fs.BookingId
                                            where f.Status.ToLower() == "post" && fs.Status.ToLower() == status.ToLower()
                                            select new clsBooking
                                            {
                                                RecordID = fs.RecordID,
                                                BookingId = f.BookingId, WorkSection = f.WorkSection ?? "", JobNo=f.JobNo ?? "", PaymentStatus=((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus , PaybalEmail=f.PaybalEmail ?? "",CreditCardNo = f.CreditCardNo ?? "",   PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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

                                                ServiceProviderID = f.ServiceProviderID ?? 0,
                                                ServierProviderName = db.Cab_ServiceProvider.Where(x => x.ServiceProviderID == f.ServiceProviderID).Select(x => x.ServierProviderName).FirstOrDefault() ?? "",
                                                PaymentMode = f.PaymentMode ?? "",
                                                SPRefNo = f.SPRefNo ?? "",
                                                SPAmount = f.SPAmount ?? 0,

                                                PickupDateTime = f.PickupDateTime,   PickupTimeSpan = f.PickupTime, PickupDateTimeStr = ("00" + f.PickupDateTime.Value.Day.ToString()).Substring(("00" + f.PickupDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.PickupDateTime.Value.Month.ToString()).Substring(("00" + f.PickupDateTime.Value.Month.ToString()).Length - 2)  + "-" + f.PickupDateTime.Value.Year.ToString(),
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
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                            }).OrderBy(x => x.PickupDateTime).ThenByDescending(x => x.BookingId).ToList();

                //SEARCHING...
                if (!string.IsNullOrEmpty(passengername))
                {
                    invoice = invoice.Where(c => c.PasengerName.ToLower().Contains(passengername.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(email))
                {
                    invoice = invoice.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(cell1))
                {
                    invoice = invoice.Where(c => c.Cellno.ToLower().Contains(cell1.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(pickupaddress))
                {
                    invoice = invoice.Where(c => c.Startpoint.ToLower().Contains(pickupaddress.ToLower())).ToList();
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





        public ActionResult BookingDetailByID(int? id)
        {
            
            clsBooking bookingdetail = new clsBooking();
            bookingdetail = (from f in db.Cab_BookingMain
                             where f.BookingId == id
                             select new clsBooking
                             {
                                 BookingId = f.BookingId,
                                 WorkSection = f.WorkSection ?? "",
                                 JobNo = f.JobNo ?? "",
                                 PaymentStatus = ((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus,
                                 PaybalEmail = f.PaybalEmail ?? "",
                                 CreditCardNo = f.CreditCardNo ?? "",
                                 PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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


                                 PickupPostalCode= f.PickupPostalCode ?? "",
                                 PickupPostalCode1 = f.PickupPostalCode1 ?? "",
                                 PickupPostalCode2 = f.PickupPostalCode2 ?? "",
                                 PickupPostalCode3 = f.PickupPostalCode3 ?? "",

                                 PickupLongitude= f.PickupLongitude ?? "",
                                 PickupLongitude1 = f.PickupLongitude1 ?? "",
                                 PickupLongitude2 = f.PickupLongitude2 ?? "",
                                 PickupLongitude3 = f.PickupLongitude3 ?? "",

                                 PickupLatitude= f.PickupLatitude ?? "",
                                 PickupLatitude1 = f.PickupLatitude1 ?? "",
                                 PickupLatitude2 = f.PickupLatitude2 ?? "",
                                 PickupLatitude3 = f.PickupLatitude3 ?? "",

                                 DropoffPostalCode= f.DropoffPostalCode ?? "",
                                 DropoffPostalCode1= f.DropoffPostalCode1 ?? "",
                                 DropoffPostalCode2= f.DropoffPostalCode2 ?? "",
                                 DropoffPostalCode3= f.DropoffPostalCode3 ?? "",

                                DropoffLongitude= f.DropoffLongitude ?? "",
                                DropoffLongitude1= f.DropoffLongitude1 ?? "",
                                DropoffLongitude2= f.DropoffLongitude2 ?? "",
                                DropoffLongitude3= f.DropoffLongitude3 ?? "",

                                DropoffLatitude= f.DropoffLatitude ?? "",
                                DropoffLatitude1= f.DropoffLatitude1 ?? "",
                                DropoffLatitude2= f.DropoffLatitude2 ?? "",
                                DropoffLatitude3 = f.DropoffLatitude3 ?? "",





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
                                 ChildSeat = f.ChildSeat ?? 0,
                                 MeetGreet = f.MeetGreet ?? 0,
                                 DisableSeat= f.DisableSeat ?? 0,
                                 ChildSeatStr = (f.ChildSeat == 1) ? "Y" : "N",
                                 DisableSeatStr = (f.DisableSeat == 1) ? "Y" : "N",
                                 AdditionalInfo = f.AdditionalInformation,
                                 ReturnTripStr = (f.ReturnTrip == 1) ? "Y" : "N",
                                 FlightNo = f.FlightNo,
                                 ArrivalFrom = f.ArrivalFrom,
                                 Terminal = f.FlightTerminal,
                                 FlightTimeString = ("00" + f.FlightTime.Value.Hours.ToString()).Substring(("00" + f.FlightTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.FlightTime.Value.Minutes.ToString()).Substring(("00" + f.FlightTime.Value.Minutes.ToString()).Length - 2),
                                 DoorNo = f.DoorNo,
                                 Status = f.Status,
                                 MaxAmount = f.BidAmount ?? 0,

                                 ReturnDate = f.ReturnDate,
                                 ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                 LuggageID = f.LuggageID ?? 0,
                                 LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.LuggageID).FirstOrDefault() ?? "",
                                 BidCloseDate = f.BidCloseDate,
                                 BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                 BidPostRemarks = f.BidPostRemarks ?? "",
                                 LogStatus = f.LogStatus ?? "",
                                 DriverArrivedDateTime = f.DriverArrivedDateTime,
                                 POBDateTime = f.POBDateTime,
                                 DoneDateTime = f.DoneDateTime,
                             }).FirstOrDefault();

            return Json(new { data = bookingdetail }, JsonRequestBehavior.AllowGet);           
        }






        [HttpPost]
        public ActionResult UpdateBooking(string model)
        {
            string data = "";
            if (ModelState.IsValid)
            {
                clsBooking Obj = JsonConvert.DeserializeObject<clsBooking>(model);
                int vrow = 0;
                DataTable dt = new DataTable();
                dt.Columns.Add("Id");
                dt.Columns.Add("LuggageId");

                if (Obj.LuggageDetail != null)
                {
                    if (Obj.LuggageDetail != "")
                    {
                        string[] multiArray = Obj.LuggageDetail.Split(new Char[] { '^' });
                        foreach (string sublist in multiArray)
                        {
                            if (sublist.Trim() != "")
                            {
                                int vlug = Convert.ToInt32(sublist);
                                vrow = vrow + 1;
                                dt.Rows.Add(new object[] { vrow, vlug });
                            }
                        }
                    }
                    else
                    {
                        dt.Rows.Add(new object[] { 0, 0 });
                    }
                }
                else
                {
                    dt.Rows.Add(new object[] { 0, 0 });
                }


                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("spUpdateBooking", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@BookingId", SqlDbType.Int).Value = Obj.BookingId;
                            cmd.Parameters.Add("@PickupDateTime", SqlDbType.DateTime).Value = Obj.PickupDateTime;
                            cmd.Parameters.Add("@PickupTime", SqlDbType.DateTime).Value = Obj.PickupTime;
                            cmd.Parameters.Add("@PickupPostalCode", SqlDbType.NVarChar, 50).Value = Obj.PickupPostalCode;
                            cmd.Parameters.Add("@PickupPostalCode1", SqlDbType.NVarChar, 50).Value = Obj.PickupPostalCode1;
                            cmd.Parameters.Add("@PickupPostalCode2", SqlDbType.NVarChar, 50).Value = Obj.PickupPostalCode2;
                            cmd.Parameters.Add("@PickupPostalCode3", SqlDbType.NVarChar, 50).Value = Obj.PickupPostalCode3;
                            cmd.Parameters.Add("@PickupLongitude", SqlDbType.NVarChar, 50).Value = Obj.PickupLongitude;
                            cmd.Parameters.Add("@PickupLongitude1", SqlDbType.NVarChar, 50).Value = Obj.PickupLongitude1;
                            cmd.Parameters.Add("@PickupLongitude2", SqlDbType.NVarChar, 50).Value = Obj.PickupLongitude2;
                            cmd.Parameters.Add("@PickupLongitude3", SqlDbType.NVarChar, 50).Value = Obj.PickupLongitude3;
                            cmd.Parameters.Add("@PickupLatitude", SqlDbType.NVarChar, 50).Value = Obj.PickupLatitude;
                            cmd.Parameters.Add("@PickupLatitude1", SqlDbType.NVarChar, 50).Value = Obj.PickupLatitude1;
                            cmd.Parameters.Add("@PickupLatitude2", SqlDbType.NVarChar, 50).Value = Obj.PickupLatitude2;
                            cmd.Parameters.Add("@PickupLatitude3", SqlDbType.NVarChar, 50).Value = Obj.PickupLatitude3;
                            cmd.Parameters.Add("@DropoffPostalCode", SqlDbType.NVarChar, 50).Value = Obj.DropoffPostalCode;
                            cmd.Parameters.Add("@DropoffPostalCode1", SqlDbType.NVarChar, 50).Value = Obj.DropoffPostalCode1;
                            cmd.Parameters.Add("@DropoffPostalCode2", SqlDbType.NVarChar, 50).Value = Obj.DropoffPostalCode2;
                            cmd.Parameters.Add("@DropoffPostalCode3", SqlDbType.NVarChar, 50).Value = Obj.DropoffPostalCode3;
                            cmd.Parameters.Add("@DropoffLongitude", SqlDbType.NVarChar, 50).Value = Obj.DropoffLongitude;
                            cmd.Parameters.Add("@DropoffLongitude1", SqlDbType.NVarChar, 50).Value = Obj.DropoffLongitude1;
                            cmd.Parameters.Add("@DropoffLongitude2", SqlDbType.NVarChar, 50).Value = Obj.DropoffLongitude2;
                            cmd.Parameters.Add("@DropoffLongitude3", SqlDbType.NVarChar, 50).Value = Obj.DropoffLongitude3;
                            cmd.Parameters.Add("@DropoffLatitude", SqlDbType.NVarChar, 50).Value = Obj.DropoffLatitude;
                            cmd.Parameters.Add("@DropoffLatitude1", SqlDbType.NVarChar, 50).Value = Obj.DropoffLatitude1;
                            cmd.Parameters.Add("@DropoffLatitude2", SqlDbType.NVarChar, 50).Value = Obj.DropoffLatitude2;
                            cmd.Parameters.Add("@DropoffLatitude3", SqlDbType.NVarChar, 50).Value = Obj.DropoffLatitude3;
                            cmd.Parameters.Add("@CabId", SqlDbType.Int).Value = Obj.CabId;
                            cmd.Parameters.Add("@NoofPassenger", SqlDbType.Int).Value = Obj.NoofPassenger;
                            cmd.Parameters.Add("@Luggagekg", SqlDbType.Decimal).Value = Obj.Luggagekg;
                            cmd.Parameters.Add("@LuggageID", SqlDbType.Int).Value = 0;
                            cmd.Parameters.Add("@ChildSeat", SqlDbType.Int).Value = Obj.ChildSeat;
                            cmd.Parameters.Add("@DisableSeat", SqlDbType.Int).Value = Obj.DisableSeat;
                            cmd.Parameters.Add("@Surname", SqlDbType.NVarChar, 50).Value = Obj.SurName;
                            cmd.Parameters.Add("@PasengerName", SqlDbType.NVarChar, 50).Value = Obj.PasengerName;
                            cmd.Parameters.Add("@Cellno", SqlDbType.NVarChar, 50).Value = Obj.Cellno;
                            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = Obj.Email;
                            cmd.Parameters.Add("@Payment", SqlDbType.NVarChar, 50).Value = Obj.Payment;
                            cmd.Parameters.Add("@PaybalEmail", SqlDbType.NVarChar, 150).Value = Obj.PaybalEmail;
                            cmd.Parameters.Add("@CreditCvvCode", SqlDbType.NVarChar, 50).Value = Obj.CreditCvvCode;
                            cmd.Parameters.Add("@CardFirstName", SqlDbType.NVarChar, 50).Value = Obj.CardFirstName;
                            cmd.Parameters.Add("@CreditCardNo", SqlDbType.NVarChar, 50).Value = Obj.CreditCardNo;
                            cmd.Parameters.Add("@CreditMonth", SqlDbType.Int).Value = Obj.CreditMonth;
                            cmd.Parameters.Add("@CreditYear", SqlDbType.Int).Value = Obj.CreditYear;
                            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = "Active";
                            cmd.Parameters.Add("@TotalKm", SqlDbType.Decimal).Value = Obj.TotalKm;
                            cmd.Parameters.Add("@ReturnTrip", SqlDbType.Int).Value = Obj.ReturnTrip;
                            cmd.Parameters.Add("@ReturnDate", SqlDbType.DateTime).Value = Obj.ReturnDate;
                            cmd.Parameters.Add("@ReturnTime", SqlDbType.DateTime).Value = Obj.ReturnTime;
                            cmd.Parameters.Add("@MeetGreet", SqlDbType.Int).Value = Obj.MeetGreet;
                            cmd.Parameters.Add("@TermsCheck", SqlDbType.Int).Value = Obj.TermsCheck;
                            cmd.Parameters.Add("@NoOfChild", SqlDbType.Int).Value = Obj.NoOfChild;
                            cmd.Parameters.Add("@ArrivalFrom", SqlDbType.NVarChar, 100).Value = Obj.ArrivalFrom;
                            cmd.Parameters.Add("@FlightNo", SqlDbType.NVarChar, 50).Value = Obj.FlightNo;
                            cmd.Parameters.Add("@FlightTime", SqlDbType.DateTime).Value = Obj.FlightTime;
                            cmd.Parameters.Add("@Terminal", SqlDbType.NVarChar, 50).Value = Obj.Terminal;
                            cmd.Parameters.Add("@DoorNo", SqlDbType.NVarChar, 50).Value = Obj.DoorNo;
                            cmd.Parameters.Add("@AdditionalInfo", SqlDbType.NVarChar, 500).Value = Obj.AdditionalInfo;
                            cmd.Parameters.Add("@calculatedfare", SqlDbType.Decimal).Value = Obj.calculatedfare;
                            cmd.Parameters.Add("@EstimateFareNet", SqlDbType.Decimal).Value = Obj.EstimateFareNet;
                            cmd.Parameters.Add("@PickupAddress", SqlDbType.NVarChar, 255).Value = Obj.Startpoint;
                            cmd.Parameters.Add("@PickupAddress1", SqlDbType.NVarChar, 255).Value = Obj.Startpoint1;
                            cmd.Parameters.Add("@PickupAddress2", SqlDbType.NVarChar, 255).Value = Obj.Startpoint2;
                            cmd.Parameters.Add("@PickupAddress3", SqlDbType.NVarChar, 255).Value = Obj.Startpoint3;
                            cmd.Parameters.Add("@DropOffAddress", SqlDbType.NVarChar, 255).Value = Obj.Endposition;
                            cmd.Parameters.Add("@DropOffAddress1", SqlDbType.NVarChar, 255).Value = Obj.Endposition1;
                            cmd.Parameters.Add("@DropOffAddress2", SqlDbType.NVarChar, 255).Value = Obj.Endposition2;
                            cmd.Parameters.Add("@DropOffAddress3", SqlDbType.NVarChar, 255).Value = Obj.Endposition3;
                            cmd.Parameters.Add("@ManualPickupAddress", SqlDbType.NVarChar, 255).Value = Obj.ManualPickupAddress;
                            cmd.Parameters.Add("@ManualDropOffAddress", SqlDbType.NVarChar, 255).Value = Obj.ManualDropOffAddress;
                            cmd.Parameters.Add("@dtDetail", SqlDbType.Structured).Value = dt;
                            cmd.Parameters.Add("@PromotionID", SqlDbType.Int).Value = Obj.PromotionID;

                            cmd.Parameters.Add("@ServiceProviderID", SqlDbType.Int).Value = Obj.ServiceProviderID;
                            cmd.Parameters.Add("@SPRefNo", SqlDbType.NVarChar, 100).Value = Obj.SPRefNo;
                            cmd.Parameters.Add("@PaymentMode ", SqlDbType.NVarChar, 50).Value = Obj.PaymentMode;
                            cmd.Parameters.Add("@SPAmount ", SqlDbType.Decimal).Value = Obj.SPAmount;
                            cmd.Parameters.Add("@PaymentStatus ", SqlDbType.NVarChar, 50).Value = Obj.PaymentStatus;

                            cmd.Parameters.Add("@CheckReturn", SqlDbType.Int).Direction = ParameterDirection.Output;
                            cmd.ExecuteNonQuery();
                            //data = Convert.ToInt32(cmd.Parameters["@CheckReturn"].Value);
                            data = cmd.Parameters["@CheckReturn"].Value.ToString();
                            cmd.Dispose();
                        }
                        con.Close();
                        con.Dispose();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }



        #region Booking Report
        public List<clsItem> getBookingStatus()
        {
            List<clsItem> subcategory = new List<clsItem>();
            subcategory = (from c in db.Cab_BookingMain
                           select new clsItem
                           {
                               Status = c.Status ?? ""
                           }).Distinct().ToList();
            return subcategory;
        }
        public List<clsItem> getBookingPaymentStatus()
        {
            List<clsItem> subcategory = new List<clsItem>();
            subcategory = (from c in db.Cab_BookingMain
                           select new clsItem
                           {
                               Status = c.PaymentStatus ?? ""
                           }).Distinct().ToList();
            return subcategory;
        }
        public List<clsAccountDetail> getDrivers()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Type.ToLower() == ("Driver").ToLower()
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name 
                      }).OrderBy(x => x.Name).ToList();
            return detail;
        }
        public List<clsAccountDetail> getSuppliers()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Type.ToLower() == ("Supplier").ToLower()
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name
                      }).OrderBy(x => x.Name).ToList();
            return detail;
        }

        public ActionResult BookingReport()
        {
            ViewBag.Status = new SelectList(getBookingStatus(), "Status", "Status");
            ViewBag.PaymentStatus = new SelectList(getBookingPaymentStatus(), "Status", "Status");
            ViewBag.Drivers = new SelectList(getDrivers(), "DetailCode", "Name");
            ViewBag.Suppliers = new SelectList(getSuppliers(), "DetailCode", "Name");
            ViewBag.CabType = new SelectList(getCabType(), "CabTypeID", "Description");
            ViewBag.ServiceProvider = new SelectList(getCabServiceProvider(), "ServiceProviderID", "ServierProviderName");

            return View();
        }
        
        [HttpPost]
        public ActionResult printbookingreport()
        {
            if (User != null)
            {
                String[] driver = Request.Form.GetValues("DriverDetailCode");
                String[] supplier = Request.Form.GetValues("SupplierDetailCode");

                String[] status = Request.Form.GetValues("Status");
                String[] paymentstatus = Request.Form.GetValues("PaymentStatus");

                String[] cabtypeid = Request.Form.GetValues("CabTypeID");
                String[] serviceproviderid = Request.Form.GetValues("ServiceProviderID");

                var checkbox = Request.Form.GetValues("DateCheckbox");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                TimeSpan fromTime = Convert.ToDateTime(Request.Form["FromTime"]).TimeOfDay;
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                TimeSpan toTime = Convert.ToDateTime(Request.Form["ToTime"]).TimeOfDay;

                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                string reportType = Request.Form["ReportType"].ToString();

                List<clsBooking> invoice = new List<clsBooking>();
                invoice = GetBookingReports();

                if (driver != null)
                {
                    List<clsBooking> invoicetemp = new List<clsBooking>();
                    for (int i = 0; i < driver.Length; i++)
                    {
                        decimal subid = Convert.ToDecimal(driver[i]);
                        List<clsBooking> list = invoice.Where(x => x.DetailCode == subid).ToList();
                        invoicetemp.AddRange(list);
                    }

                    invoice.Clear();
                    invoice = invoicetemp.ToList();
                }

                if (supplier != null)
                {
                    List<clsBooking> invoicetemp = new List<clsBooking>();
                    for (int i = 0; i < supplier.Length; i++)
                    {
                        decimal subid = Convert.ToDecimal(supplier[i]);
                        List<clsBooking> list = invoice.Where(x => x.DetailCode == subid).ToList();
                        invoicetemp.AddRange(list);
                    }

                    invoice.Clear();
                    invoice = invoicetemp.ToList();
                }


                if (cabtypeid != null)
                {
                    List<clsBooking> invoicetemp = new List<clsBooking>();
                    for (int i = 0; i < cabtypeid.Length; i++)
                    {
                        decimal subid = Convert.ToDecimal(cabtypeid[i]);
                        List<clsBooking> list = invoice.Where(x => x.CabId == subid).ToList();
                        invoicetemp.AddRange(list);
                    }

                    invoice.Clear();
                    invoice = invoicetemp.ToList();
                }

                if (serviceproviderid != null)
                {
                    List<clsBooking> invoicetemp = new List<clsBooking>();
                    for (int i = 0; i < serviceproviderid.Length; i++)
                    {
                        decimal subid = Convert.ToDecimal(serviceproviderid[i]);
                        List<clsBooking> list = invoice.Where(x => x.ServiceProviderID == subid).ToList();
                        invoicetemp.AddRange(list);
                    }

                    invoice.Clear();
                    invoice = invoicetemp.ToList();
                }

                if (!String.IsNullOrEmpty(computerNo))
                {
                    invoice=invoice.Where(x => x.BookingId.ToString().ToLower() == computerNo.ToLower()).ToList();
                }
                if (!String.IsNullOrEmpty(referenceNo))
                {
                    invoice = invoice.Where(x => x.SPRefNo.ToLower() == referenceNo.ToLower()).ToList();
                }

                int showdate = 0;
                if (checkbox != null)
                {
                    if (checkbox.Length > 0)
                    {
                        if (checkbox[0].ToLower() == "on")
                        {
                            showdate = 1;
                            invoice = invoice.Where(x => x.PickupDateTime.Value.Add(x.PickupTimeSpan ?? DateTime.Now.TimeOfDay) >= fromDate.Add(fromTime) && x.PickupDateTime.Value.Add(x.PickupTimeSpan ?? DateTime.Now.TimeOfDay) <= toDate.Add(toTime)).ToList();
                        }
                    }
                }

                clsBooking booking = new clsBooking();
                booking.FromDateTime = fromDate.Add(fromTime);
                booking.ToDateTime = fromDate.Add(toTime);
                booking.BookingList2 = invoice.GroupBy(x => new { x.DetailCode, x.DetailName }).Select(g => g.First()).ToList();
                booking.BookingList1 = invoice.GroupBy(x => new { x.ServiceProviderID, x.ServierProviderName }).Select(g => g.First()).ToList();


                booking.showDate = showdate;
                booking.BookingList = invoice.ToList();

                string reportname = "printbookingdetail";

                if (reportType.ToLower() == ("Booking Detail").ToLower())
                {
                    reportname = "printbookingdetail";
                }
                else if (reportType.ToLower() == ("Service Provider Invoice").ToLower())
                {
                    reportname = "PrintSPBookingDetail";
                }
                else if (reportType.ToLower() == ("Driver Invoice").ToLower())
                {
                    reportname = "PrintDriverBookingDetail";
                }

                
                return View(reportname, booking);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public List<clsBooking> GetBookingReports()
        {
            string imagepath = "/FormsDocuments/Cab/"; //System.Configuration.ConfigurationManager.ConnectionStrings["CabImage"].ConnectionString;


                List<clsBooking> invoice = (from f in db.Cab_BookingMain
                                            select new clsBooking
                                            {
                                                BookingId = f.BookingId,
                                                WorkSection = f.WorkSection ?? "",
                                                JobNo = f.JobNo ?? "",
                                                PaymentStatus = ((f.PaymentStatus.ToLower() ?? "") == "") ? "Unpaid" : f.PaymentStatus,
                                                PaybalEmail = f.PaybalEmail ?? "",
                                                CreditCardNo = f.CreditCardNo ?? "",
                                                PaymentNo = ((f.PaymentStatus.ToLower() ?? "") == "paid") ? (((f.CreditCardNo ?? "") != "") ? "Credit Card" : (f.PaybalEmail ?? "")) : "",
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
                                                Status = f.Status.ToUpper(),
                                                MaxAmount = f.BidAmount ?? 0,

                                                ReturnDate = f.ReturnDate,
                                                ReturnTimeStr = f.ReturnTime != null ? ("00" + f.ReturnTime.Value.Hours.ToString()).Substring(("00" + f.ReturnTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.ReturnTime.Value.Minutes.ToString()).Substring(("00" + f.ReturnTime.Value.Minutes.ToString()).Length - 2) : "",
                                                LuggageID = f.LuggageID ?? 0,
                                                LuggageDesc = db.View_Booking_Luggage.Where(x => x.BookingID == f.BookingId).Select(x => x.Luggage).FirstOrDefault() ?? "",
                                                BidCloseDate = f.BidCloseDate,
                                                BidCloseTimeStr = f.BidCloseTime != null ? ("00" + f.BidCloseTime.Value.Hours.ToString()).Substring(("00" + f.BidCloseTime.Value.Hours.ToString()).Length - 2) + ":" + ("00" + f.BidCloseTime.Value.Minutes.ToString()).Substring(("00" + f.BidCloseTime.Value.Minutes.ToString()).Length - 2) : "",
                                                BidPostRemarks = f.BidPostRemarks ?? "",
                                                imageName = imagepath + "/" + db.Form_Document.Where(x => x.TypeId == f.CabId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "",


                                                SuppBidAmount = db.Cab_Bid.Where(x => x.RecordID == f.RecordID).Select(x => x.BidAmount).FirstOrDefault() ?? 0,
                                                DetailCode = db.Cab_Bid.Where(x => x.RecordID == f.RecordID).Select(x => x.DetailCode).FirstOrDefault() ?? 0,
                                                DetailName = (from b in db.Cab_Bid join a in db.AC_Detail on b.DetailCode equals a.DetailCode where b.RecordID == f.RecordID select a.Name).FirstOrDefault() ?? "",

                                                LogStatus = f.LogStatus ?? "",
                                                DriverArrivedDateTime = f.DriverArrivedDateTime,
                                                POBDateTime = f.POBDateTime,
                                                DoneDateTime = f.DoneDateTime,


                                                DriverArrivedDateTimeStr = f.DriverArrivedDateTime != null ? ("00" + f.DriverArrivedDateTime.Value.Day.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.DriverArrivedDateTime.Value.Month.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Month.ToString()).Length - 2) + "-" + f.DriverArrivedDateTime.Value.Year.ToString() + " " + ("00" + f.DriverArrivedDateTime.Value.Hour.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Hour.ToString()).Length - 2) + ":" + ("00" + f.DriverArrivedDateTime.Value.Minute.ToString()).Substring(("00" + f.DriverArrivedDateTime.Value.Minute.ToString()).Length - 2) : "",
                                                POBDateTimeStr = f.POBDateTime != null ? ("00" + f.POBDateTime.Value.Day.ToString()).Substring(("00" + f.POBDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.POBDateTime.Value.Month.ToString()).Substring(("00" + f.POBDateTime.Value.Month.ToString()).Length - 2) + "-" + f.POBDateTime.Value.Year.ToString() + " " + ("00" + f.POBDateTime.Value.Hour.ToString()).Substring(("00" + f.POBDateTime.Value.Hour.ToString()).Length - 2) + ":" + ("00" + f.POBDateTime.Value.Minute.ToString()).Substring(("00" + f.POBDateTime.Value.Minute.ToString()).Length - 2) : "",
                                                DoneDateTimeStr = f.DoneDateTime != null ? ("00" + f.DoneDateTime.Value.Day.ToString()).Substring(("00" + f.DoneDateTime.Value.Day.ToString()).Length - 2) + "-" + ("00" + f.DoneDateTime.Value.Month.ToString()).Substring(("00" + f.DoneDateTime.Value.Month.ToString()).Length - 2) + "-" + f.DoneDateTime.Value.Year.ToString() + " " + ("00" + f.DoneDateTime.Value.Hour.ToString()).Substring(("00" + f.DoneDateTime.Value.Hour.ToString()).Length - 2) + ":" + ("00" + f.DoneDateTime.Value.Minute.ToString()).Substring(("00" + f.DoneDateTime.Value.Minute.ToString()).Length - 2) : "",



                                            }).OrderBy(x => x.PickupDateTime).ThenBy(x => x.PickupTimeSpan).ThenBy(x => x.BookingId).ToList();

                return invoice;

           
        }
        #endregion

    }
}