using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsReminder
    {
        public int LogID { get; set; }
        public DateTime? LogDateTime { get; set; }
        public int? UserID { get; set; }
        public decimal? DetailCode { get; set; }
        public DateTime? DueDate { get; set; }
        public string Remarks { get; set; }
        public int? DueDateAlert { get; set; }
        public string Status { get; set; }
        public string FollowUpRemarks { get; set; }
        public int? FollowUpUserId { get; set; }
        public DateTime? FollowUpDateTime { get; set; }
    }
}