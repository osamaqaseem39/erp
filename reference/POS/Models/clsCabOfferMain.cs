using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{


    public class clsCabOfferMain
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Unique Identifier for the fare entry

        // ✅ Offer Details
        public int? OfferID { get; set; }
        public string OfferName { get; set; }

        // ✅ Vehicle Type
        public string VehicleType { get; set; } // Example: Sedan, SUV, Minivan

        // ✅ Location Details
        public string FromLocation { get; set; }
        public string FromPostCode { get; set; }
        public string ToLocation { get; set; }
        public string ToPostCode { get; set; }

        // ✅ Pricing and Sequence
        public int? SequenceNo { get; set; } // Optional ordering field
        public decimal? BasePrice { get; set; } // Default base fare
        public decimal? Price { get; set; } // Final fare after discounts/offers

        // ✅ Status & Miscellaneous
        public bool Status { get; set; } // Active (true) / Inactive (false)
        public string Remarks { get; set; }

    }


}