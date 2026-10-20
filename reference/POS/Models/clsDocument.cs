using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsDocument
    {
        public int DocumentId { get; set; }
        public string TypeId { get; set; }
        public string Remarks { get; set; }
        public DateTime? Date { get; set; }
        public string DocumentExtension { get; set; }
        public string DocumentType { get; set; }
    }
}