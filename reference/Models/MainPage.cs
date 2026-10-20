using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class MainPage
    {
        public List<clsOffersMain> offermainlist = new List<clsOffersMain>();
        public List<clsRating> ratinglist = new List<clsRating>();        
    }

    public class clsAdminEmail
    {
        public string Email { get; set; }
    }

    public class clsRating
    {
        public int ComputerNo { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string Message { get; set; }
        public DateTime? SaveDateTime { get; set; }
        public decimal? Rating { get; set; }
    }

    public class clsOffersMain
    {
        public int OfferID { get; set; }
        public string FromLocation { get; set; }
        public string FromPostCode { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }

    public class clsOffersDetail
    {
        public int PromotionID { get; set; }
        public int OfferID { get; set; }
        public int SequenceNo { get; set; }
        public int? CabTypeID { get; set; }
        public string FromLocation { get; set; }
        public string FromPostCode { get; set; }
        public string ToLocation { get; set; }
        public string ToPostCode { get; set; }
        public string Remarks { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
        public string CabTypeName { get; set; }
    }
}
