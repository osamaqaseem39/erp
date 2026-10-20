using System;

namespace BlocksERP.Models
{
    public class clsLoanCategory
    {
        public int LoanCategoryID { get; set; }
        public string CategoryName { get; set; }
        public string CategoryStatus { get; set; }
        public string LoanType { get; set; }
        public decimal? InterestRatePercentage { get; set; }
        public int? MaxDays { get; set; }
        public decimal? MaxLimit { get; set; }
        public decimal? CompanyFee { get; set; }

        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }

    }
}