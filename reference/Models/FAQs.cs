using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class FAQs
    {
        public int ContentID { get; set; }
        public string Question { get; set; }
        public string Description { get; set; }
        public int SerialNo { get; set; }
        public string Status { get; set; }
    }

    public class Blogs
    {
        public int ContentID { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string BlogUser { get; set; }
        public DateTime? BlogDate { get; set; }
        public int SerialNo { get; set; }
        public string Status { get; set; }
        public string PageName { get; set; }
    }


    public class clsContentPage
    {
        public int ContentID { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string ContentGroup { get; set; }
        public int SerialNo { get; set; }
        public string Status { get; set; }
        public string DisplayName { get; set; }
        public List<clsContentPage> grouplist { get; set; }
        public List<clsContentPage> sublist { get; set; }
    }
}