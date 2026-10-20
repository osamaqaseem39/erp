using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Web;
using BlocksERP.Models;
using static log4net.Appender.RollingFileAppender;

namespace BlocksERP.Models
{
    public class clsLoanApplication
    {

        public int UserID { get; set; }
        public int LoanID { get; set; }
        public DateTime? LoanDate { get; set; }        
        public int? BankID { get; set; }
        public string BankName { get; set; }
        public int? LoanPurposeID { get; set; }
        public string LoanPurposeName { get; set; }
        public int? LoanCategoryID { get; set; }
        public string LoanCategoryName { get; set; }
        public int? InvestorID { get; set; }
        public string InvestorName { get; set; }

        public string Status { get; set; }
        public string LoanReturnType { get; set; }        
        public decimal? LoanAmount { get; set; }
        public int? LoanDays { get; set; }
        public decimal? FixedCharges { get; set; }
        public decimal? CompanyFee { get; set; }

        public DateTime? ApprovalDate { get; set; }
        public string ApprovalComment { get; set; }

        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }

        public List<clsLoanApplicationDetail> DetailList = new List<clsLoanApplicationDetail>();

        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int PersonalInfoID { get; set; }
        public string FullName { get; set; }
        public string SocialInsuranceNumber { get; set; }
        public int? GenderID { get; set; }
        public string GenderName { get; set; }
        public int? EducationID { get; set; }
        public string EducationName { get; set; }
        public int? MaritalStatusID { get; set; }
        public string MaritalStatusName { get; set; }
        public int? CountryID { get; set; }
        public string CountryName { get; set; }
        public string ResidentialAddress { get; set; }
        public string DetailAddress { get; set; }
        public int? ResidenceCityID { get; set; }
        public string ResidenceCityName { get; set; }
        public int? DetailedCityID { get; set; }
        public string DetailedCityName { get; set; }
        public DateTime? DOB { get; set; }
        public string ImagePath { get; set; }
        

    }

    public class clsLoanApplicationDetail
    {

        public int DetailID { get; set; }
        public int LoanID { get; set; }
        public DateTime? ReturnDate { get; set; }

        public string Status { get; set; }        
        public decimal? ReturnAmount { get; set; }
        public decimal? PrincipalAmount { get; set; }
        public decimal? InterestAmount { get; set; }
        public decimal? PaidAmount { get; set; }

        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }


    }

}
