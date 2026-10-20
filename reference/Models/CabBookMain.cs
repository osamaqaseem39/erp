using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class CabBookMain
    {
        public int BookingId { get; set; }
        public string AddDedLeg { get; set; }
        public int DriverID { get; set; }
        public int DedLegBookingId { get; set; }
        public int FromBookingId { get; set; }
        public Nullable<System.DateTime> PickupDateTime { get; set; }
        public Nullable<System.DateTime> PickupTime { get; set; }

        public Nullable<System.DateTime> ReturnDate { get; set; }
        public Nullable<System.DateTime> ReturnTime { get; set; }

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


        public int? PromotionID { get; set; }
        public int? CustomerID { get; set; }
        

        public string ManualPickupAddress { get; set; }
        public string ManualDropOffAddress { get; set; }

        public string Surname { get; set; }
        public string Payment { get; set; }
        public string PasengerName { get; set; }
        public string Cellno { get; set; }
        public string Email { get; set; }
        public Nullable<int> CabId { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<decimal> Miles { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> SaveDateTime { get; set; }
        public string PaybalEmail { get; set; }
        public string CardFirstName { get; set; }
        public string CreditCardNo { get; set; }
        public string CreditCvvCode { get; set; }
        public Nullable<int> CreditMonth { get; set; }
        public Nullable<int> CreditYear { get; set; }
        public Nullable<System.DateTime> StartDateTime { get; set; }
        public Nullable<System.DateTime> CloseDateTime { get; set; }
        public string StartingMeterKm { get; set; }
        public string ClosingMeterKm { get; set; }
        public decimal? TotalKm { get; set; }

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

        public decimal? Fare { get; set; }
        public decimal? ReturnFare { get; set; }

        

        public int? LuggageID { get; set; }
        public string LuggageDetail { get; set; }


        public string  Startpoint { get; set; }
        public string Startpoint1 { get; set; }
        public string Startpoint2 { get; set; }
        public string Startpoint3 { get; set; }
        public string Endposition { get; set; }
        public string Endposition1 { get; set; }
        public string Endposition2 { get; set; }
        public string Endposition3 { get; set; }
    }


    public class clsLuggage
    {
        public int LuggageID { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}