using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsCheque
    {
        public int ChequeBookCompNo { get; set; }
        public string ChequeBookNo { get; set; }
        public string FromSerialNo { get; set; }
        public string ToSerialNo { get; set; }
        public DateTime? IssuedDate { get; set; }
        public decimal? DetailCode { get; set; }
        public string Status { get; set; }
        public int? ChequeBookDId { get; set; }
        public string ChequeNo { get; set; }
        public int ChequeComputerNo { get; set; }
        public int ChequeInfoId { get; set; }
        public string Type { get; set; }
        public DateTime? ChequeDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Bank { get; set; }
        public string ChequeType { get; set; }
        public decimal? Amount { get; set; }
        public decimal? DAccountCode { get; set; }
        public decimal? DDetailCode { get; set; }
        public string DDetailName { get; set; }
        public decimal? CAccountCode { get; set; }
        public decimal? CDetailCode { get; set; }
        public string CDetailName { get; set; }
        public string Remarks { get; set; }
        public string DepositRemarks { get; set; }
        public DateTime? DepositDate { get; set; }
        public string ClearRemarks { get; set; }
        public DateTime? ClearDate { get; set; }
        public string ReturnRemarks { get; set; }
        public string ReceivedBy { get; set; }
        public string PaidBy { get; set; }
        public string DepositReceivedBy { get; set; }
        public string RefVoucherNo { get; set; }
        public int? UserID { get; set; }
        public string LastUserID { get; set; }
        public DateTime? SaveDateTime { get; set; }
        public DateTime? LastEditDateTime { get; set; }
        public int? BusinessID { get; set; }
        public int? FiscalYearID { get; set; }
        public string ContainFromZero { get; set; }
        public string ContainToZero { get; set; }
        public string DetailName { get; set; }
        public string PaidTo { get; set; }
        public string StatusCountDays { get; set; }
        public string DebitCreditType { get; set; }
    }
}