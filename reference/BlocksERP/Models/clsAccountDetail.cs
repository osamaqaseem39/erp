using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BlocksERP;

namespace BlocksERP.Models
{
    public class clsAccountDetail
    {
        public int DetailCode { get; set; }
        public decimal? DetaillCode { get; set; }


        public string VoucherType { get; set; }
        public string Name { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }
        public int? ParentLevelCode { get; set; }
        public string ParentLevelDescription { get; set; }
        public string Type { get; set; }
        public string UrduName { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Cell1 { get; set; }
        public string Cell2 { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public int? CreditDays { get; set; }
        public int? CreditLimit { get; set; }
        public string TaxType { get; set; }
        public string GST { get; set; }
        public string NTN { get; set; }
        public string CNIC { get; set; }
        public string Remarks { get; set; }
        public string ContactPerson { get; set; } 
        public string Terms { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? SalesmanId { get; set; }
        public string FullName { get; set; }

        public int? CustomerCategID { get; set; }
        public string CustomerCategName { get; set; }

        public int? RouteID { get; set; }
        public string RouteName { get; set; }

        public int? DiscCategID { get; set; }
        public string DiscCategName { get; set; }

        public int? ZoneId { get; set; }
        public string ZoneName { get; set; }
        

        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string SalesmanName { get; set; }

        


        public decimal? ClosingBalance { get; set; }
        public decimal? PreviousBalance { get; set; }

        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }

        public string IsNeWOrEdit { get; set; }
        public string ThirdLevelAccountCode { get; set; }
        public string SecondLevelAccountCode { get; set; }
        public int AccountCode { get; set; }
        public string Description { get; set; }

        public List<clsChequeBook> ChequeList = new List<clsChequeBook>();

        public int? RegionID { get; set; }       
        public string Region { get; set; }
        public string Zone { get; set; }


        public decimal? OrderAmount { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string CompanyName { get; set; }
        public string EmiratesID { get; set; }
        public int? RatingStar { get; set; }

        public int? OtherCurrency { get; set; }
        public int? CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        
        public decimal? SO { get; set; }
        public decimal? DA { get; set; }
        public decimal? DN { get; set; }
        public decimal? SI { get; set; }
        public decimal? SR { get; set; }
        public decimal? CN { get; set; }
        public decimal? SOValue { get; set; }
        public decimal? SIValue { get; set; }
        public decimal? CNValue { get; set; }
        
    }
}