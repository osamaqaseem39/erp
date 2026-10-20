using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsAccountDetail
    {
        public decimal DetailCode { get; set; }
        public decimal DetailCode2 { get; set; }
        public string Name { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string Status { get; set; }
        public string AcDescription { get; set; }
        public string AccountCodeV { get; set; }
        public decimal? AccountCode { get; set; }
        public string Type { get; set; }
        public string UrduName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Cell1 { get; set; }
        public string Cell2 { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public int? CreditDays { get; set; }
        public decimal? CreditLimit { get; set; }
        public string TaxType { get; set; }
        public string TaxTypeInString { get; set; }
        public string GST { get; set; }
        public string NTN { get; set; }
        public string CNIC { get; set; }
        public string Remarks { get; set; }
        public int? SalesmanID { get; set; }
        public string IsNeWOrEdit { get; set; }
        public string Reference { get; set; }
        public int? TaxID { get; set; }
        public string TaxName { get; set; }
        public string ThirdLevelAccountCode { get; set; }
        public decimal PreviousBalance { get; set; }


        public string ImageName { get; set; }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        public string Saturday { get; set; }
        public string Sunday { get; set; }

        public string AdminUser { get; set; }
        public string AdminPassword { get; set; }

        public string DocumentNo { get; set; }
        public int? DocumentTypeID { get; set; }
        public int? DocStatus { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public TimeSpan? WorkFrom { get; set; }
        public TimeSpan? WorkUpTo { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public string CommEmail { get; set; }
        public string BookingEmail { get; set; }
        public string AccountName { get; set; }
        public string ACNumber { get; set; }
        public string SortCode { get; set; }
        public string BankName { get; set; }
        public string BankAddress { get; set; }
        
        public int? SerialNo { get; set; }
        public int? ZoneID { get; set; }
        public string ZoneName { get; set; }


        public string RegNo { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Colour { get; set; }
        public string FuelType { get; set; }

        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public DateTime? LastDateTime { get; set; }
        public string LastDateTimeStr { get; set; }

        public int? CabTypeID { get; set; }
        public string CabTypeName { get; set; }
        public int? NoOfVeh { get; set; }

        public string UrgentPick { get; set; }
        public int? UrgentPickTime { get; set; }

        public List<clsCheque> ChequeList = new List<clsCheque>();

    }
}