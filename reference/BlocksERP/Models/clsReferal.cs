using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BlocksERP.Models;
using static log4net.Appender.RollingFileAppender;

namespace BlocksERP.Models
{
    public class clsReferal
    {

        public int UserID { get; set; }
        public int ReferalID { get; set; }

        public int? ContactRelationID { get; set; }
        public string ContactRelationName { get; set; }
        public string RefferalName { get; set; }
        public string RefferalPhoneNumber { get; set; }
        public string RefferalSIN { get; set; }
        public string RefferalAddress { get; set; }

        public DateTime? CreatedAt { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByString { get; set; }


    }
}
