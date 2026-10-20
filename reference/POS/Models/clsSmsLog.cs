using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsSmsLog
    {
        public int SmsLogId { get; set; }
        public DateTime? SaveDateTime { get; set; }
        public decimal? DetailCode { get; set; }
        public string DetailName { get; set; }
        public int? SmsUserId { get; set; }
        public string CellNo { get; set; }
        public string SmsMessage { get; set; }
        public string ComputerNo { get; set; }
        public string Status { get; set; }
        public DateTime? SendDateTime { get; set; }
    }
}