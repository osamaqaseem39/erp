using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsSmsSetting
    {
        public int SmsSettingId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SendSms { get; set; }
        public string Masking { get; set; }
    }
}