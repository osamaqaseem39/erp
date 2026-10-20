using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace POS.Controllers
{
    public class DocumentsController : BaseController
    {
        PosMBDb db = new PosMBDb();
        // GET: /Documents/
        public ActionResult AttachDocument(string type, string id)
        {
            clsDocument document = new clsDocument();
            document.TypeId = id;
            document.DocumentType = type;
            return PartialView(document);
        }
        public ActionResult DocumentsList(string type, string typeId)
        {
            List<clsDocument> documents = new List<clsDocument>();
            documents = (from d in db.Form_Document
                         where d.TypeId == typeId && d.DocumentType == type
                         select new clsDocument
                         {
                             DocumentId = d.DocumentId,
                             Remarks = d.Remarks,
                             Date = d.Date,
                             DocumentExtension = d.DocumentExtension,
                             DocumentType = d.DocumentType,
                             TypeId = typeId
                         }).ToList();
            var data = documents.ToList();
            return new JsonResult { Data = new { data = data } };
        }
        public ActionResult AttachDocumentsForPost(clsDocument doc)
        {
            bool status = true;
            int documentId = 0;

            if (doc.DocumentType == "Cab")
            {
                documentId = db.Form_Document.Where(x => x.TypeId == doc.TypeId.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId).FirstOrDefault() ;
            }

            if (doc.DocumentType == "Driver")
            {
                documentId = db.Form_Document.Where(x => x.TypeId == doc.TypeId.ToString() && x.DocumentType == "Driver" && x.Remarks == "Driver Image").Select(x => x.DocumentId).FirstOrDefault();
            }


            if (doc.Date == null)
            {
                DateTime date = new DateTime(2050, 12, 31);
                doc.Date = date;
            }
            if (doc.Remarks == null)
            {
                doc.Remarks = "";
            }


            if (documentId > 0)
            {
                var document = db.Form_Document.Where(x => x.DocumentId == documentId).FirstOrDefault();
                document.Remarks = doc.Remarks;
                document.Date = doc.Date;
                document.DocumentExtension = doc.DocumentExtension; 
                db.SaveChanges();
            }
            else
            {
                //if (doc.DocumentType == "Employee" && doc.Remarks == "Profile Image")
                //{

                //}
                Form_Document document = new Form_Document();
                document.Remarks = doc.Remarks;
                document.Date = doc.Date;
                document.TypeId = doc.TypeId;
                document.DocumentExtension = doc.DocumentExtension;
                document.DocumentType = doc.DocumentType;
                db.Form_Document.Add(document);
                db.SaveChanges();
                documentId = document.DocumentId;
                //Create Main Documents Folder
                var path = Server.MapPath("~/FormsDocuments");
                bool exists = Directory.Exists(path);//check folder exsit or not
                if (!exists)
                {
                    Directory.CreateDirectory(path);
                }
                //Create Main Type Documents Folder
                var path2 = Server.MapPath("~/FormsDocuments/"+doc.DocumentType);
                bool exists2 = Directory.Exists(path2);//check folder exsit or not
                if (!exists2)
                {
                    Directory.CreateDirectory(path2);
                }
            }
            return new JsonResult { Data = new { status = status, documentId = documentId, documentType = doc.DocumentType , typeId = doc.TypeId } };
        }
        public JsonResult UploadDocument(string id)
        {
            bool status = false;
            int documentId = Convert.ToInt32(id.Split('_')[0]);
            string documentType = id.Split('_')[1];
            string typeId = id.Split('_')[2];
            string extension = id.Split('_')[3];
            if (extension != null && extension != "")
            {
                HttpPostedFileBase document = Request.Files[0] as HttpPostedFileBase;
                if (document != null && document.ContentLength > 0)
                {
                    try
                    {
                        // string fileName = System.IO.Path.GetFileName(doc.Document.FileName);
                        string fileName = documentId + "." + extension;
                        //Set the Image File Path.
                        string filePath = "~/FormsDocuments/" + documentType + "/" + fileName;

                        //Save the Image File in Folder.
                        document.SaveAs(Server.MapPath(filePath));
                        status = true;
                    }
                    catch
                    {
                        status = false;
                    }
                }
            }
            else
            {
                status = true;
            }
            return Json(new
            {
                status = status,
            });
        }
        public ActionResult DeleteDocument(int id)
        {
            string message = "";
            bool status = true;
            var document = db.Form_Document.Where(x => x.DocumentId == id).FirstOrDefault();
            var documentType = document.DocumentType;
            string extension = document.DocumentExtension;
            string file_name = id + "." + extension;
            string path = Server.MapPath("~/FormsDocuments/" + documentType + "/" + file_name);
            FileInfo file = new FileInfo(path);
            if (file.Exists)//check file exsit or not
            {
                try
                {
                    file.Delete();
                    status = true;
                }
                catch
                {
                    status = false;
                }
            }
            if (status)
            {
                db.Form_Document.Remove(document);
                db.SaveChanges();
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
	}
}