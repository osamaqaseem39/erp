using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsSms
    {
        public int? SmsId { get; set; }
        public string Type { get; set; }
        public string FormReportName { get; set; }
        public string Status { get; set; }
        public string ToCustomer { get; set; }
        public string ToUser { get; set; }
        public int? SerialNo { get; set; }
        public int? SmsUserId { get; set; }
        public string SmsUserName { get; set; }
        public int SmsDetailId { get; set; }
        public string FieldTitle { get; set; }
        public string FieldDbName { get; set; }
        public int? SequenceNo { get; set; }
        public string NewLine { get; set; }
    }
}