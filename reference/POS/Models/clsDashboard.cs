using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsDashboard
    {
        public int? ShowCashBankBalance { get; set; }
        public int? ShowTopSupplierBalance { get; set; }
        public int? ShowTopCustomerBalance { get; set; }
        public int? ShowAllCustomersBalance { get; set; }
        public int? ShowSalePieChart { get; set; }
        public int? ShowReminder { get; set; }
        public int? ShowSaleInvoiceCreditDays { get; set; }
        public int? ShowLastYearCurrentMonthSale { get; set; }
        public int? ShowCurrentMonthSale { get; set; }
        public int? ShowLastYearCurrentMonthReceived { get; set; }
        public int? ShowCurrentMonthReceived { get; set; }
    }
}