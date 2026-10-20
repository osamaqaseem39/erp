using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsVoucher
    {
        public string IsNewOrEdit { get; set; }
        public string VoucherNo { get; set; }
        public string VoucherType { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string Status { get; set; }
        public string RefrenceNo { get; set; }
        public int? UserID { get; set; }
        public string LastUserID { get; set; }
        public DateTime? SaveDateTime { get; set; }
        public DateTime? LastEditDateTime { get; set; }
        public int? CityId { get; set; }
        public int? BusinessID { get; set; }
        public int? FiscalYearID { get; set; }
        public string CompositKey { get; set; }
        public int? SerialNo { get; set; }
        public string ACDescription { get; set; }
        public decimal? ACCode { get; set; }
        public string ACCodeV { get; set; }
        public string LineACCodeV { get; set; }
        public string DetailName { get; set; }
        public string LineDetailName { get; set; }
        public decimal? DetailCode { get; set; }
        public decimal? LineDetailCode { get; set; }
        public decimal? LineDebit { get; set; }
        public decimal? LineCredit { get; set; }
        public string Remarks { get; set; }
        public string LineRemarks { get; set; }
        public string ChequeNo { get; set; }
        public int? ChequeComputerNo { get; set; }
        public string LineChequeNo { get; set; }
        public int? LineChequeComputerNo { get; set; }
        public int? TaxID { get; set; }
        public string TaxName { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? TaxValue { get; set; }
        public decimal? NetDebit { get; set; }
        public decimal? NetCredit { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string Cell1 { get; set; }
        public string PaidBy { get; set; }
        public string ReceivedBy { get; set; }
        public int? DefaultVoucher { get; set; }
        public decimal? PaidToDetailCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? ByDate { get; set; }
        public string DetailType { get; set; }
        public decimal PreviousBalance { get; set; }
        public List<clsAccountDetail> DetailList = new List<clsAccountDetail>();
        public List<clsVoucher> VouchersList = new List<clsVoucher>();
        public List<clsVoucher> VouchersList2 = new List<clsVoucher>();
        public List<clsInvoice> InvoicesList = new List<clsInvoice>();
        public List<clsCheque> ChequeList = new List<clsCheque>();
        public List<clsChartOfAccount> ThirdLevelList = new List<clsChartOfAccount>();
        public string AmountInString { get; set; }
        public string CityName { get; set; }

        public string Address { get; set; }
        public string Phone1 { get; set; }
        public int CreditDays { get; set; }
        public int DueDays { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? ReceivedAmount { get; set; }
        public decimal? BalanceAmount { get; set; }
        public decimal? AdjustAmount { get; set; }
        public decimal? PendingAmount { get; set; }
        public string DebitCreditType { get; set; }
        public string OpeningBalanceType { get; set; }
        public decimal? SecondLevelCode { get; set; }
        public decimal? ThirdLevelCode { get; set; }
        public decimal? Bellow30 { get; set; }
        public decimal? TO60 { get; set; }
        public decimal? TO90 { get; set; }
        public decimal? Above90 { get; set; }
        public int SalesmanID { get; set; }
        public decimal? CashSale { get; set; }
        public decimal? CreditSale { get; set; }
        public decimal? TotalSale { get; set; }
        public decimal? CashSaleReturn { get; set; }
        public decimal? CreditSaleReturn { get; set; }
        public decimal? TotalSaleReturn { get; set; }
        public decimal? NetSale { get; set; }
        public int Counts { get; set; }
        public string SalesmanName { get; set; }
        

    }
}