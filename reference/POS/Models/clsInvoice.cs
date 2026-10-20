using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsInvoice
    {
        public string IsNewOrEdit { get; set; }
        public string ComputerNo { get; set; }
        public string Type { get; set; }
        public DateTime? RecordDate { get; set; }
        public string RecordDateStr { get; set; }
        public decimal? DetailCode { get; set; }
        public decimal? SubDetailCode { get; set; }
        public string RefrenceNo { get; set; }
        public string Remarks { get; set; }
        public decimal? GrossTotal { get; set; }
        public string DiscountType { get; set; }
        public string LineDiscountType { get; set; }
        public decimal? Discount { get; set; }
        public decimal? LineDiscount { get; set; }
        public decimal? NetDiscount { get; set; }
        public decimal? Overhead { get; set; }
        public decimal? Freight { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? LineNetAmount { get; set; }
        public decimal? LineNetRate { get; set; }
        public decimal? LineGrossAmount { get; set; }
        public int? Received { get; set; }
        public decimal? ReceivedDetailCode { get; set; }
        public decimal? ReceivedAmount { get; set; }
        public string ReceivedRemarks { get; set; }
        public decimal? BalanceAmount { get; set; }
        public string Status { get; set; }
        public int? BusinessID { get; set; }
        public int? UserID { get; set; }
        public string LastUserID { get; set; }
        public DateTime? SaveDateTime { get; set; }
        public DateTime? LastEditDateTime { get; set; }
        public decimal? PreviousBalance { get; set; }
        public int? CreditDays { get; set; }
        public int? FiscalYearID { get; set; }
        public string Transporter { get; set; }
        public string BiltyNo { get; set; }
        public string BiltyPacking { get; set; }
        public DateTime? BiltyDate { get; set; }
        public int? BiltyPending { get; set; }
        public int? SalesmanID { get; set; }
        public string CompositKey { get; set; }
        public string RefCompositKey { get; set; }
        public int? SerialNo { get; set; }
        public decimal? ItemID { get; set; }
        public int? UomId { get; set; }
        public int? Uom2Id { get; set; }
        public decimal? SecondQty { get; set; }
        public decimal? RemainingQty { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Rate { get; set; }
        public string RateOn { get; set; }
        public decimal? FirstUomRate { get; set; }
        public decimal? SecondUomRate { get; set; }
        public decimal? InchRate { get; set; }
        public decimal? NetRate { get; set; }
        public decimal? GrossValue { get; set; }
        public int? Tax1ID { get; set; }
        public string Tax1Name { get; set; }
        public decimal? Tax1Rate { get; set; }
        public decimal? Tax1Value { get; set; }
        public int? Tax2ID { get; set; }
        public string Tax2Name { get; set; }
        public decimal? Tax2Rate { get; set; }
        public decimal? Tax2Value { get; set; }
        public decimal? NetValue { get; set; }
        public string ItemNote { get; set; }
        public decimal? InQty { get; set; }
        public decimal? OutQty { get; set; }
        public decimal? InSecondQty { get; set; }
        public decimal? OutSecondQty { get; set; }
        public decimal? AvgRate { get; set; }
        public decimal? CostRate { get; set; }
        public int? WarehouseID { get; set; }
        public int ExtraOrNot { get; set; }
        public string DetailName { get; set; }
        public string SubDetailName { get; set; }
        public string ReceivedDetailName { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public string WarehouseName { get; set; }
        public string Reason { get; set; }
        public string DateType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? ByDate { get; set; }
        public List<clsAccountDetail> DetailList = new List<clsAccountDetail>();
        public List<clsInvoice> InvoicesList = new List<clsInvoice>();
        public List<clsInvoice> InvoicesList2 = new List<clsInvoice>();
        public List<clsInvoice> InvoicesList3 = new List<clsInvoice>();
        public List<clsInvoice> OverheadsList = new List<clsInvoice>();
        public List<clsInvoice> WarehousesList = new List<clsInvoice>();
        public clsItem ItemList = new clsItem();
        public string UnitName { get; set; }
        public string ItemCode { get; set; }
        public string QtyInString { get; set; }
        public string SecondQtyInString { get; set; }
        public int? CategoryID { get; set; }
        public int? BrandID { get; set; }
        public int? SubCategoryID { get; set; }
        public string SubCategoryName { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }

        public string PickupLocation { get; set; }
        public string DropOffLocation { get; set; }

        public decimal? PreviousStock1 { get; set; }
        public decimal? Stock1 { get; set; }
        public decimal? AverageValue1 { get; set; }
        public decimal? PerKgOverhead { get; set; }
        public string Barcode { get; set; }
        public decimal? Difference1 { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? OverDue { get; set; }
        public int LogId { get; set; }
        public string BalanceAmountString { get; set; }
        public string CreditLimitString { get; set; }
        public string OverDueString { get; set; }
        public int? InvoiceType { get; set; }
        public string CashCustomerName { get; set; }
        public int? ShowBalance { get; set; }
        public int? CityId { get; set; }
        public string Address { get; set; }
        public int DateFilter { get; set; }
        public string UrduName { get; set; }
        public string Phone { get; set; }
        public int FooterId { get; set; }
        public string CityName { get; set; }
        public string Cell1 { get; set; }
        public string InvoiceTypeName { get; set; }
        public string HiddenComputerNo { get; set; }
        public string SalesmanName { get; set; }

        public string PaymentStatus { get; set; }
        public int? ServiceProviderID { get; set; }
        public int? CabTypeID { get; set; }
        public decimal? SupplierDetailCode { get; set; }
        public decimal? DriverDetailCode { get; set; }

        public decimal? NetSale { get; set; }
        public decimal? NetCost { get; set; }
        public decimal? Breakage { get; set; }
        public decimal? Coupen { get; set; }
        public decimal? Other { get; set; }

        public List<clsVoucher> CreditNoteList = new List<clsVoucher>();
    }
}