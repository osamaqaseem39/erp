using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsDocument
    {
        public int DocumentId { get; set; }
        public int DeleteRoleId { get; set; }
        public string TypeId { get; set; }
        public string Remarks { get; set; }
        public DateTime? Date { get; set; }
        public string DocumentExtension { get; set; }
        public string DocumentType { get; set; }
        public ObservableCollection<ImagesSaveBase64> imagesBase64 { get; set; }
        public int? EmployeeId { get; set; }


        public int? userId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string DocumentPath { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

    }

    public class ImagesSaveBase64
    {
        //public ImageSource images { get; set; }
        public string imgConvertBase64 { get; set; }
    }
}