using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsChartofAccount
    {
        public string FirstLevelAccountCode { get; set; }
        public string SecondLevelAccountCode { get; set; }
        public int AccountCode { get; set; }
        public string DisplayCode { get; set; }
        public string Description { get; set; }
        public string IsNeWOrEdit { get; set; }
        public string Code { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public int? ParentLevelCode { get; set; }
        public string ParentLevelDescription { get; set; }
        public int? AccountLevel { get; set; }
        public string Remarks { get; set; }
        public string SectionName { get; set; }
      
      
        public int? Status { get; set; }
        public string StatusString { get; set; }
        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
      
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }

        public decimal? PreviousBalance { get; set; }
        public List<clsChequeBook> ChequeList = new List<clsChequeBook>();

        public List<clsCashbank> LedgerList = new List<clsCashbank>();
        

        public List<clsCashbank> VoucherDetailLine = new List<clsCashbank>();
        public List<clsAccountDetail> DetailList = new List<clsAccountDetail>();
        public List<clsCashbank> VouchersList = new List<clsCashbank>();
        public List<clsInvoice> InvoicesList = new List<clsInvoice>();
        public List<clsChartofAccount> ThirdLevelList = new List<clsChartofAccount>();


        public List<clsCashbank> CashBankList = new List<clsCashbank>();
        public List<clsCashbank> CashBankGroupList = new List<clsCashbank>();

        public List<clsCashbank> FirstLevelListV = new List<clsCashbank>();
        public List<clsCashbank> SecondLevelListV = new List<clsCashbank>();
        public List<clsCashbank> ThirdLevelListV = new List<clsCashbank>();

    }
}