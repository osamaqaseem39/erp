using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BlocksERP.Models;
using static log4net.Appender.RollingFileAppender;

namespace BlocksERP.Models
{
    public class clsWork
    {

        public int UserID { get; set; }
        public int WorkInfoID { get; set; }
        public int? IncomeSourceID { get; set; }
        public string IncomeSourceName { get; set; }
        public int? EmploymentStatusID { get; set; }
        public string EmploymentStatusName { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationPhoneNumber { get; set; }
        public string OrganizationAddress { get; set; }
        public DateTime? DOJ { get; set; }
        public decimal? MonthlyIncome { get; set; }
        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }


    }
}
