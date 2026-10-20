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
using BlocksERP.Models;

namespace BlocksERP.Controllers
{
    public class DocumentsController : BaseController
    {
        ModelDb db = new ModelDb();
        // GET: /Documents/
        public ActionResult AttachDocumentHRM(string type, string id)
        {
            clsDocument document = new clsDocument();
            document.TypeId = id;
            document.DocumentType = type;
            return PartialView(document);
        }

        public ActionResult ShowDocumentHRM(string type, string id)
        {
            clsDocument document = new clsDocument();
            document.TypeId = id;
            document.DocumentType = type;
            return PartialView(document);
        }

        public ActionResult AttachDocument(string type, string id)
        {
            clsDocument document = new clsDocument();
            document.TypeId = id;
            document.DocumentType = type;
            return PartialView(document);
        }
        public ActionResult AttachDocumentEmployee(string type, string id, string EmployeeId)
        {
            int vempid = 0;
            if (!String.IsNullOrEmpty(EmployeeId)) { vempid = Convert.ToInt32(EmployeeId); }

            clsDocument document = new clsDocument();
            document.TypeId = id;
            document.DocumentType = type;
            document.EmployeeId = vempid;

            return PartialView(document);
        }
        public ActionResult AttachDocumentEmployeeViewOnly(string type, string id, string EmployeeId)
        {
            int vempid = 0;
            if (!String.IsNullOrEmpty(EmployeeId)) { vempid = Convert.ToInt32(EmployeeId); }

            clsDocument document = new clsDocument();
            document.TypeId = id;
            document.DocumentType = type;
            document.EmployeeId = vempid;
            
            return PartialView(document);
        }


        //public ActionResult DocumentsListEmployee(string type, string typeId)
        //{
        //    List<clsDocument> documents = new List<clsDocument>();
        //    if (type.ToLower() == ("EmployeeDocument").ToLower())
        //    {
        //        int vempid = Convert.ToInt32(typeId);

        //        documents = (from d in db.tblDocuments
        //                     where d.EmployeeId == vempid
        //                       && (d.DocumentType.ToLower() == ("EmployeeDocument").ToLower() || d.DocumentType.ToLower() == ("EmployeeDocTable").ToLower())
        //                     select new clsDocument
        //                     {
        //                         DocumentId = d.DocumentId,
        //                         Date = d.Date,
        //                         DocumentExtension = d.DocumentExtension,
        //                         DocumentType = d.DocumentType,
        //                         Remarks = d.Remarks ?? "",
        //                         TypeId = typeId
        //                     }).ToList();
        //    }
        //    else
        //    {
        //        documents = (from d in db.tblDocuments
        //                     where d.TypeId == typeId && d.DocumentType == type
        //                     select new clsDocument
        //                     {
        //                         DocumentId = d.DocumentId,
        //                         Date = d.Date,
        //                         DocumentExtension = d.DocumentExtension,
        //                         DocumentType = d.DocumentType,
        //                         Remarks = d.Remarks ?? "",
        //                         TypeId = typeId
        //                     }).ToList();
        //    }

        //    var data = documents.ToList();
        //    return new JsonResult { Data = new { data = data } };
        //}

        //public ActionResult DocumentsList(string type, string typeId)
        //{
        //    List<clsDocument> documents = new List<clsDocument>();
        //     documents = (from d in db.tblDocuments
        //                     where d.TypeId == typeId && d.DocumentType == type
        //                     select new clsDocument
        //                     {
        //                         DocumentId = d.DocumentId,
        //                         Date = d.Date,
        //                         DocumentExtension = d.DocumentExtension,
        //                         DocumentType = d.DocumentType,
        //                         Remarks = d.Remarks ?? "",
        //                         TypeId = typeId
        //                     }).ToList();

        //    var data = documents.ToList();
        //    return new JsonResult { Data = new { data = data } };
        //}

        public ActionResult AttachDocumentsForPost(clsDocument doc)
        {
            bool status = true;
            int documentId = 0;
            doc.Date = DateTime.Now;
            if (string.IsNullOrEmpty(doc.Remarks))
            {
                doc.Remarks = "";
            }
            if (!(string.IsNullOrEmpty(doc.DocumentExtension)))
            {
                if (doc.DocumentType == "Item" || doc.DocumentType == "Supplier" || doc.DocumentType == "User" || doc.DocumentType == "Company" || doc.DocumentType == "Business" || doc.DocumentType == "PurchaseOrder" || doc.DocumentType == "HRMEmployee")
                {
                    var itemDocument = db.tblDocumentOthers.Where(x => x.TypeId == doc.TypeId && x.DocumentType == doc.DocumentType).FirstOrDefault();
                    if (itemDocument != null)
                    {
                        try
                        {
                            int documentsId = itemDocument.DocumentId;
                            string documentExtension = itemDocument.DocumentExtension;
                            db.tblDocumentOthers.Remove(itemDocument);
                            db.SaveChanges();
                            string fileName = documentsId + "." + documentExtension;
                            string path1 = Server.MapPath("~/FormsDocuments/" + doc.DocumentType + "/" + fileName);
                            FileInfo file = new FileInfo(path1);
                            if (file.Exists)//check file exsit or not
                            {
                                file.Delete();
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                if (doc.DocumentId > 0)
                {
                    try
                    {
                        var dcc = db.tblDocumentOthers.Where(x => x.DocumentId == doc.DocumentId).FirstOrDefault();
                        dcc.DocumentExtension = doc.DocumentExtension;
                        dcc.Remarks = doc.Remarks;
                        db.SaveChanges();
                        documentId = doc.DocumentId;
                    }
                    catch
                    {
                        documentId = 0;
                    }
                }
                else
                {
                    tblDocumentOther document = new tblDocumentOther();
                    document.Date = doc.Date;
                    document.TypeId = doc.TypeId;
                    document.DocumentExtension = doc.DocumentExtension;
                    document.DocumentType = doc.DocumentType;
                    document.Remarks = doc.Remarks;
                    document.UserId = User.UserID;
                    db.tblDocumentOthers.Add(document);
                    db.SaveChanges();
                    documentId = document.DocumentId;
                }

            }
            //Create Main Documents Folder
            var path = Server.MapPath("~/FormsDocuments");
            bool exists = Directory.Exists(path);//check folder exsit or not
            if (!exists)
            {
                Directory.CreateDirectory(path);
            }
            //Create Main Type Documents Folder
            var path2 = Server.MapPath("~/FormsDocuments/" + doc.DocumentType);
            bool exists2 = Directory.Exists(path2);//check folder exsit or not
            if (!exists2)
            {
                Directory.CreateDirectory(path2);
            }
            return new JsonResult { Data = new { status = status, documentId = documentId, documentType = doc.DocumentType, typeId = doc.TypeId } };
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
                    //Delete Previous File for edit document
                    string file_name = documentId + "." + extension;
                    string path = Server.MapPath("~/FormsDocuments/" + documentType + "/" + file_name);
                    FileInfo file = new FileInfo(path);
                    if (file.Exists)//check file exsit or not
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch
                        {
                        }
                    }

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
            var document = db.tblDocumentOthers.Where(x => x.DocumentId == id).FirstOrDefault();
            if (document != null)
            {
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
                    db.tblDocumentOthers.Remove(document);
                    db.SaveChanges();
                }
            }
            else
            {
                status = true;
            }

            return new JsonResult { Data = new { status = status, message = message } };
        }
    }
}