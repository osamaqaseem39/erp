using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class CabBookingMain
    {
        public int BookingId { get; set; }
        public Nullable<System.DateTime> PickupDateTime { get; set; }
        public string PickupPostalCode { get; set; }
        public string PickupLongitude { get; set; }
        public string PickupLatitude { get; set; }
        public Nullable<System.DateTime> DropoffDateTime { get; set; }
        public string DropoffPostalCode { get; set; }
        public string DropoffLongitude { get; set; }
        public string DropoffLatitude { get; set; }
        public Nullable<int> NoofPassenger { get; set; }
        public Nullable<decimal> Luggagekg { get; set; }
        public Nullable<int> ChildSeat { get; set; }
        public Nullable<int> DisableSeat { get; set; }
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
        public Nullable<int> CreditCardNo { get; set; }
        public Nullable<int> CreditCvvCode { get; set; }
        public Nullable<int> CreditMonth { get; set; }
        public Nullable<int> CreditYear { get; set; }

    }
}
