using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{


    public class clsCabOfferDetail
    {
        public int PromotionID { get; set; }
        public int? OfferID { get; set; }
        public string OfferName { get; set; }
        public int? CabTypeID { get; set; }
        public string CabName { get; set; }
        public int? SequenceNo { get; set; }
        public string ToLocation { get; set; }
        public string ToPostCode { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public decimal? Price { get; set; }
    }


}