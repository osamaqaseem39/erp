    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    namespace POS.Models
    {
        public class clsBooking
        {
            public int BookingId { get; set; }
            public string AddDedLeg { get; set; }
            public string FormType { get; set; }
            public string JobNo { get; set; }
            public string WaitTime { get; set; }

            public string SupplierPerson { get; set; }
            public string SupplierPhone { get; set; }

            public string FirebaseTokenAndroid { get; set; }
            public string FirebaseTokenIOS { get; set; }

            public string VehicleNo { get; set; }
            public string VehicleMake { get; set; }
            public string VehicleModel { get; set; }

            public int? ServiceProviderID { get; set; }
            public string ServierProviderName { get; set; }
            public string SPRefNo { get; set; }
            public string PaymentMode { get; set; }
            public decimal? SPAmount { get; set; }

            public int DriverID { get; set; }
            public int? CabTypeID { get; set; }
            public int DedLegBookingId { get; set; }
            public int FromBookingId { get; set; }
            public Nullable<System.DateTime> PickupDateTime { get; set; }
            public string PickupDateTimeStr { get; set; }
            public Nullable<System.DateTime> PickupTime { get; set; }
            public Nullable<System.TimeSpan> PickupTimeSpan { get; set; }

            public DateTime? PickupDateWithTime { get; set; }
        
            public string PickupPostalCode { get; set; }
            public string PickupPostalCode1 { get; set; }
            public string PickupPostalCode2 { get; set; }
            public string PickupPostalCode3 { get; set; }
            public string PickupLongitude { get; set; }
            public string PickupLongitude1 { get; set; }
            public string PickupLongitude2 { get; set; }
            public string PickupLongitude3 { get; set; }
            public string PickupLatitude { get; set; }
            public string PickupLatitude1 { get; set; }
            public string PickupLatitude2 { get; set; }
            public string PickupLatitude3 { get; set; }
            public Nullable<System.DateTime> DropoffDateTime { get; set; }
            public string DropoffPostalCode { get; set; }
            public string DropoffPostalCode1 { get; set; }
            public string DropoffPostalCode2 { get; set; }
            public string DropoffPostalCode3 { get; set; }
            public string DropoffLongitude { get; set; }
            public string DropoffLongitude1 { get; set; }
            public string DropoffLongitude2 { get; set; }
            public string DropoffLongitude3 { get; set; }
            public string DropoffLatitude { get; set; }
            public string DropoffLatitude1 { get; set; }
            public string DropoffLatitude2 { get; set; }
            public string DropoffLatitude3 { get; set; }

            public string Payment { get; set; }
            public string PasengerName { get; set; }
            public string Cellno { get; set; }
            public string Email { get; set; }
            public Nullable<int> CabId { get; set; }
            public string CabTypeName { get; set; }
            public Nullable<decimal> Amount { get; set; }
            public Nullable<decimal> Miles { get; set; }
            public string Status { get; set; }
            public Nullable<System.DateTime> SaveDateTime { get; set; }
            public string PaybalEmail { get; set; }
            public string CardFirstName { get; set; }
            public string CreditCardNo { get; set; }
            public Nullable<int> CreditCvvCode { get; set; }
            public Nullable<int> CreditMonth { get; set; }
            public Nullable<int> CreditYear { get; set; }
            public Nullable<System.DateTime> StartDateTime { get; set; }
            public Nullable<System.DateTime> CloseDateTime { get; set; }
            public string StartingMeterKm { get; set; }
            public string ClosingMeterKm { get; set; }
            public decimal? TotalKm { get; set; }
            public string LuggageDetail { get; set; }

            public Nullable<int> NoofPassenger { get; set; }
            public Nullable<decimal> Luggagekg { get; set; }
            public Nullable<int> ChildSeat { get; set; }
            public Nullable<int> DisableSeat { get; set; }
            public int? ReturnTrip { get; set; }
            public int? MeetGreet { get; set; }
            public int? TermsCheck { get; set; }
            public int? NoOfAdult { get; set; }
            public int? NoOfChild { get; set; }
            public string ArrivalFrom { get; set; }
            public string FlightNo { get; set; }
            public DateTime? FlightTime { get; set; }
            public string Terminal { get; set; }
            public string DoorNo { get; set; }
            public string AdditionalInfo { get; set; }
            public decimal? calculatedfare { get; set; }
            public decimal? EstimateFareNet { get; set; }

            public string PickupTimeString { get; set; }
            public string FlightTimeString { get; set; }

            public string Startpoint { get; set; }
            public string Startpoint1 { get; set; }
            public string Startpoint2 { get; set; }
            public string Startpoint3 { get; set; }
            public string Endposition { get; set; }
            public string Endposition1 { get; set; }
            public string Endposition2 { get; set; }
            public string Endposition3 { get; set; }

            public string PaymentNo { get; set; }

            public string SurName { get; set; }
            public string MeetGreetStr { get; set; }

            public decimal? MaxAmount { get; set; }
            public string SuppBookStatus { get; set; }
            public string SuppBidRemarks { get; set; }
        
            public decimal? SuppBidAmount { get; set; }
            public DateTime? ViewDateTime { get; set; }
            public DateTime? CreateDateTime { get; set; }
            public DateTime? CancelDateTime { get; set; }
            public DateTime? BookDateTime { get; set; }


            public string WorkSection { get; set; }
            public string LogStatus { get; set; }
            public DateTime? DriverArrivedDateTime { get; set; }
            public DateTime? POBDateTime { get; set; }
            public DateTime? DoneDateTime { get; set; }

            public DateTime? ReturnDate { get; set; }
            public DateTime? ReturnTime { get; set; }
            public string ReturnTimeStr { get; set; }
            public string ReturnTripStr { get; set; }
            public string ChildSeatStr { get; set; }
            public string DisableSeatStr { get; set; }

            public int? RecordID { get; set; }
            public string DetailName { get; set; }

            public int? LuggageID { get; set; }
            public string LuggageDesc { get; set; }

            public DateTime? BidCloseDate { get; set; }
            public DateTime? BidCloseTime { get; set; }
            public TimeSpan? BidCloseTimeSpan { get; set; }
            public string BidCloseTimeStr { get; set; }

            public DateTime? ConfirmCloseDate { get; set; }
            public TimeSpan? ConfirmCloseTime { get; set; }
            public string ConfirmCloseTimeStr { get; set; }
            public string ConfirmStatus { get; set; }

            public string BidPostRemarks { get; set; }

            public string imageName { get; set; }

            public decimal? DetailCode { get; set; }
            public string Name { get; set; }
            public int? TotalNoOfBid { get; set; }

            public string DriverArrivedDateTimeStr { get; set; }
            public string POBDateTimeStr { get; set; }
            public string DoneDateTimeStr { get; set; }

        

            public string ManualPickupAddress { get; set; }
            public string ManualDropOffAddress { get; set; }
            public int? PromotionID { get; set; }

            public TimeSpan? PickupTimeT { get; set; }
            public TimeSpan? FlightTimeT { get; set; }

            public string CancelRemarks { get; set; }

            public string PaymentStatus { get; set; }
            public string ExpireStatus { get; set; }

            public int? showDate { get; set; }
            public DateTime? FromDateTime { get; set; }
            public DateTime? ToDateTime { get; set; }

            public List<clsBooking> BookingList = new List<clsBooking>();
            public List<clsBooking> BookingList1 = new List<clsBooking>();
            public List<clsBooking> BookingList2 = new List<clsBooking>();
        }
    }
