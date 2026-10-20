using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsItem
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string UrduName { get; set; }
        public string Barcode { get; set; }
        public int? UomId { get; set; }
        public int? CategoryID { get; set; }
        public int? BrandID { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? DetailCode { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? MinSalePrice { get; set; }
        public decimal? MinLevel { get; set; }
        public decimal? MaxLevel { get; set; }
        public decimal? MinOrder { get; set; }
        public string Status { get; set; }
        public int FiscalYearID { get; set; }
        public int WarehouseID { get; set; }
        public decimal? Stock { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Value { get; set; }
        public decimal? CurrentStock { get; set; }
        public int BusinessID { get; set; }
        public decimal? LastAvgStock { get; set; }
        public decimal? LastAvgRate { get; set; }
        public decimal? LastAvgValue { get; set; }
        public decimal? RunAvgRate { get; set; }
        public decimal? RunAvgValue { get; set; }
        public decimal? RunStock { get; set; }
        public string Description { get; set; }
        public string UnitName { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public decimal? PriceOrInch { get; set; }
        public decimal? Stock1 { get; set; }
        public decimal? InQty { get; set; }
        public decimal? OutQty { get; set; }
        public string WarehouseName { get; set; }
        public List<clsItem> ItemsList = new List<clsItem>();
        public List<clsItem> WarehousesList = new List<clsItem>();
        public decimal? SaleRate { get; set; }
        public decimal? PurchaseRate { get; set; }
        public decimal? AverageRate { get; set; }
        public double? AverageRate2 { get; set; }
        public DateTime? SaleDate { get; set; }
        public int? SubCategoryID { get; set; }
        public string SubCategoryName { get; set; }

        public decimal? LastDisc { get; set; }
        public string LastDiscType { get; set; }

        public string ValueString { get; set; }
        public decimal? ValueNumber { get; set; }
    }

    public class clsDocumentType
    {
        public int DocumentTypeID { get; set; }
        public string Description { get; set; }
        public string NeedExpiry { get; set; }
        public string Status { get; set; }
    }

    public class clsItemList
    {
        public int id { get; set; }
        public string text { get; set; }
        public string description { get; set; }
    }
    public class clsBrandList
    {
        public int id { get; set; }
        public string text { get; set; }
    }
    public class clsCategoryList
    {
        public int id { get; set; }
        public string text { get; set; }
    }
    public class clsCodeList
    {
        public string id { get; set; }
        public string text { get; set; }
    }
    public class clsCashCustomerList
    {
        public string id { get; set; }
        public string text { get; set; }
    }
    public class clsAccountList
    {
        public decimal id { get; set; }
        public string text { get; set; }
    }
    public class Select2PagedItemResult
    {
        public int Total { get; set; }

        public List<clsItemList> Results { get; set; }
    }
    public class Select2PagedBrandResult
    {
        public int Total { get; set; }

        public List<clsBrandList> Results { get; set; }
    }
    public class Select2PagedCategoryResult
    {
        public int Total { get; set; }

        public List<clsCategoryList> Results { get; set; }
    }
    public class Select2PagedCodeResult
    {
        public int Total { get; set; }

        public List<clsCodeList> Results { get; set; }
    }
    public class Select2PagedCashCustomerResult
    {
        public int Total { get; set; }

        public List<clsCashCustomerList> Results { get; set; }
    }
    public class Select2PagedAccountResult
    {
        public int Total { get; set; }

        public List<clsAccountList> Results { get; set; }
    }
}