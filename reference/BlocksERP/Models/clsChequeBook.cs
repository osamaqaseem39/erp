using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsChequeBook
    {
        public int ChequeBookCompNo { get; set; }
        public int ChequeInfoId { get; set; }
        public int DeleteRoleId { get; set; }
        public int DepositRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public int? PostRoleId { get; set; }
        public int? CheckedRoleId { get; set; }
        public int? ChequeBookDId { get; set; }
        public string ChequeBookNo { get; set; }
        public string FromSerialNo { get; set; }
        public string Type { get; set; }
        public string ToSerialNo { get; set; }
        public int? DetailCode { get; set; }
        public int? CreditDetailCode { get; set; }
        public int? DebitDetailCode { get; set; }
        public int? BankDetailCode { get; set; }
        public string DetailName { get; set; }
        public string ChequeNo { get; set; }
        public string BankName { get; set; }
        public string CreditName { get; set; }
        public string DebitName { get; set; }
        public string ReceivedBankName { get; set; }
        public DateTime? ChequeDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? DepositDate { get; set; }
        public string Status { get; set; }
        public string ChequeType { get; set; }
        public decimal? Amount { get; set; }
        public string ReceivedBy { get; set; }
        public string PaidBy { get; set; }
        public string Remarks { get; set; }
        public string StatusString { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedByString { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }

        public int? CancelRoleId { get; set; }
        public int? ApprovedRoleId { get; set; }
        public int? BounceRoleId { get; set; }
        public int? ClearRoleId { get; set; }
        public int? ReturnRoleId { get; set; }
        public int? ReturnApprovalRoleId { get; set; }
        public int? BounceApprovalRoleId { get; set; }
        public int? ClearApprovalRoleId { get; set; }
        public int? BounceVoucherRoleId { get; set; }
        public int? ClearVoucherRoleId { get; set; }
        public int? ReturnedRoleId { get; set; }

        public string DebitCreditType { get; set; }
        public string DepositRemarks { get; set; }
        public string ClearRemarks { get; set; }
        public string ReturnRemarks { get; set; }

        public int? CreditAccountCode { get; set; }
        public int? DebitAccountCode { get; set; }

        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }

        public string RefVoucherNo { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

    }
}