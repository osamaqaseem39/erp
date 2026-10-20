using System;

namespace BlocksERP.Models
{
    public class clsCompanyBankAccount
    {
        public int AccountId { get; set; }
        public int? BankId { get; set; }
        public string AccountNumber { get; set; }
        public string HolderName { get; set; }
        public string BankName { get; set; }
        public string Description { get; set; }


        public string StatusString { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }

    }

}