using System;
using System.Collections.Generic;

namespace BlocksERP.Models
{
    public class clsCashbank
    {
        public decimal? TaxRate { get; set; }
        public decimal? TaxValue { get; set; }
        public decimal? PrevBalance { get; set; }

        public string IsNewOrEdit { get; set; }

        public string ComputerNo { get; set; }
        public string Type { get; set; }

        public string VoucherNo { get; set; }
        public string VoucherType { get; set; }
        public DateTime? VoucherDate { get; set; }
        public DateTime? ReferenceDate { get; set; }

        public string DocumentNo { get; set; }
        

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int? SortType { get; set; }
        public int? DebitCreditTypeInt { get; set; }

        public string DebitCreditType { get; set; }
        public string OpeningBalanceType { get; set; }
        public string DetailType { get; set; }


        public decimal? AccountCode { get; set; }
        public decimal? LineDetailCode { get; set; }
        public decimal? LineDebit { get; set; }
        public decimal? LineCredit { get; set; }
        public decimal? LineCurrencyDebit { get; set; }
        public decimal? LineCurrencyCredit { get; set; }
        public int? LineCurrencyId { get; set; }

        public int? SerialNo { get; set; }
        public string AmountInString { get; set; }


        public string InvoiceComputerNo { get; set; }
        public string LineInvoiceComputerNo { get; set; }

        public decimal? LineExchangeRate { get; set; }
        public string LineRemarks { get; set; }
        public string LineChequeNo { get; set; }
        public string Tax { get; set; }
        public string TaxType { get; set; }
        public decimal? NetDebit { get; set; }
        public decimal? NetCredit { get; set; }
        public int? DetailCode1 { get; set; }
        public string DetailName1 { get; set; }


        public int? TaxForDetailCode { get; set; }
        public string TaxForDetailName { get; set; }

        public int? DetailCode { get; set; }
        public int? DefaultCurrencyId { get; set; }
        public string DetailName { get; set; }
        public string CompositeKey { get; set; }
        
        //public int? CurrencyId { get; set; }
        public decimal? CurrencyCredit { get; set; }
        public decimal? CurrencyDebit { get; set; }
        public decimal? Amount { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? Credit { get; set; }
        public decimal? Debit { get; set; }
        public string Remarks { get; set; }
        public string PaidBy { get; set; }
        public string RecievedBy { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Status { get; set; }
        public int? LineProjectId { get; set; }
        public string LineProjectName { get; set; }
        public bool? DefaultVoucher { get; set; }
        public string DefaultVoucherString { get; set; }

        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public int? LineCostCenterId { get; set; }
        public string LineCostCenterName { get; set; }

        public string ChequeNo { get; set; }
        public int? FinancialYearId { get; set; }
        public string Description { get; set; }
        public int? BranchId { get; set; }
        public string BranchName { get; set; }

        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public string UpdatedByString { get; set; }
        public int? TaxId { get; set; }
        public string TaxName { get; set; }

        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ReferenceNo { get; set; }

        public bool? ShowProjectinVoucher { get; set; }
        public bool? MultiCurrency { get; set; }
        public List<clsCashbank> VoucherDetailLine = new List<clsCashbank>();
        public List<clsAccountDetail> DetailList = new List<clsAccountDetail>();
        public List<clsCashbank> VouchersList = new List<clsCashbank>();
        public List<clsInvoice> InvoicesList = new List<clsInvoice>();
        public List<clsChartofAccount> ThirdLevelList = new List<clsChartofAccount>();


        public List<clsCashbank> OppositList = new List<clsCashbank>();
        public List<clsCashbank> CashBankList = new List<clsCashbank>();
        public List<clsCashbank> CashBankGroupList = new List<clsCashbank>();

        public List<clsCashbank> FirstLevelListV = new List<clsCashbank>();
        public List<clsCashbank> SecondLevelListV = new List<clsCashbank>();
        public List<clsCashbank> ThirdLevelListV = new List<clsCashbank>();
        public string FirstLevelType { get; set; }
        

        public int? DeleteRoleId { get; set; }
        public int? UpdateRoleId { get; set; }

        public int? ApprovedRoleId { get; set; }
        public int? CancelRoleId { get; set; }
        public int? CheckedRoleId { get; set; }
        public int? RecheckRoleId { get; set; }

        public int? PostRoleId { get; set; }

        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }


        public int? ThirdLevelCode { get; set; }
        public string ThirdLevelName { get; set; }
        public int? SecondLevelCode { get; set; }
        public string SecondLevelName { get; set; }
        public int? FirstLevelCode { get; set; }
        public string FirstLevelName { get; set; }
        
        public decimal? Sales { get; set; }
        public decimal? RecAmount { get; set; }
        public decimal? Purchase { get; set; }
        public decimal? Payment { get; set; }
        public decimal? DebitOther { get; set; }
        public decimal? CreditOther { get; set; }
        public decimal? ClosingBalance { get; set; }

        public decimal? TotalGrossWeight { get; set; }
        public decimal? TotalCoreWeight { get; set; }
        public decimal? TotalNetWeight { get; set; }
        public decimal? Qty { get; set; }
        

        public int? RegionID { get; set; }
        public int? ZoneID { get; set; }
        public int? CityID { get; set; }

        public string CityName { get; set; }
        public string Zone { get; set; }        
        public string Region { get; set; }

        public int? SalesmanId { get; set; }
        public int? CustomerCategId { get; set; }
        public int? DiscCategId { get; set; }



        public int? MainDetailCode { get; set; }
        public string MainDetailName { get; set; }

        public int? VoucherMonthInt { get; set; }
        public string VoucherMonth { get; set; }
        
        public int? CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public decimal? CurrencyValue { get; set; }
        
    }
}