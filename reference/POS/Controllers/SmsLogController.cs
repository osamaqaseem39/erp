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

namespace POS.Controllers
{
    public class SmsLogController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult SmsLogIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SmsLog"
                          select new clsUserRight
                          {
                              showMain = r.showMain,
                              showChange = r.showChange,
                              showSave = r.showSave,
                              showDelete = r.showDelete
                          }).FirstOrDefault();
                if (rights == null)
                {
                    User.Main = 0;
                    User.Change = 0;
                    User.Save = 0;
                    User.Delete = 0;
                }
                else
                {
                    User.Main = Convert.ToInt32(rights.showMain);
                    User.Change = Convert.ToInt32(rights.showChange);
                    User.Save = Convert.ToInt32(rights.showSave);
                    User.Delete = Convert.ToInt32(rights.showDelete);
                }
                if (User.Main == 0)
                {
                    return RedirectToAction("index", "home");
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetAllCustomersAndSuppliers()
        {
            List<clsAccountDetail> account = new List<clsAccountDetail>();
            account = (from c in db.AC_Detail
                       join e in db.AC_Detail_Extra on c.DetailCode equals e.DetailCode
                       where c.Status == "Active" && (c.Type == "Customer" || c.Type == "Supplier" || c.Type == "Customer + Supplier")
                       && e.Cell1 != null && e.Cell1 != ""
                       select new clsAccountDetail
                    {
                       DetailCode = c.DetailCode,
                       Name = c.Name ?? "",
                       Cell1 = e.Cell1 ?? ""
                    }).OrderBy(x => x.Name).ToList();
            var data = account.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSellNo(decimal id)
        {
            string cellNo = db.AC_Detail_Extra.Where(x => x.DetailCode == id).Select(x => x.Cell1).FirstOrDefault() ?? "";
            var data = cellNo;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult InsertUpdateSmsLog(clsSmsLog sms, string userData)
        {
            string message = "";
            bool status = false;
            List<clsSmsLog> userList = JsonConvert.DeserializeObject<List<clsSmsLog>>(userData);
            DataTable dtUser = new DataTable();
            dtUser.Columns.Add("Id");
            dtUser.Columns.Add("SmsLogId");
            dtUser.Columns.Add("SaveDateTime");
            dtUser.Columns.Add("DetailCode");
            dtUser.Columns.Add("SmsUserId");
            dtUser.Columns.Add("CellNo");
            dtUser.Columns.Add("SmsMessage");
            dtUser.Columns.Add("ComputerNo");
            dtUser.Columns.Add("Status");
            dtUser.Columns.Add("SendDateTime");
            int returnId = 0;
                for (int i = 0; i < userList.Count; i++)
                {
                    dtUser.Rows.Add(new object[] { i + 1, 0, DateTime.Now, userList[i].DetailCode, 0, userList[i].CellNo, sms.SmsMessage,"","Active",DateTime.Now });
                }
                if (userList.Count == 0)
                {
                    dtUser.Rows.Add(new object[] { 0, 0, 0, 0 });
                }
                returnId = AddEditSmsLog(sms, dtUser, "Save");
                if (returnId == 2)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Sms Log Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        private int AddEditSmsLog(clsSmsLog sms, DataTable dtLog,  string insertUpdateStatus)
        {
            int returnId = 0;
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateSmsLog", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                        cmd.Parameters.Add("@dtSmsLog", SqlDbType.Structured).Value = dtLog;
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.NVarChar).Value = insertUpdateStatus;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        returnId = Convert.ToInt32(cmd.Parameters["@CheckReturn"].Value);
                        cmd.Dispose();
                    }
                    con.Close();
                    con.Dispose();
                }
                catch (Exception)
                { }
            }
            return returnId;
        }
        public ActionResult GetSmsById(int id)
        {
            clsSms sms = new clsSms();
            sms = (from f in db.Sms_Main
                   where f.SmsId == id
                   select new clsSms
                   {
                       SmsId = f.SmsId,
                       Type = f.Type ?? "",
                       FormReportName = f.FormReportName ?? "",
                       Status = f.Status ?? "",
                       ToCustomer = f.ToCustomer ?? "N",
                       ToUser = f.ToUser ?? "N"
                   }).FirstOrDefault();
            var data = sms;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetSmsFieldById(int id)
        {
            List<clsSms> user = new List<clsSms>();
            user = (from f in db.Sms_Detail_Field
                    where f.SmsId == id
                    select new clsSms
                    {
                        SmsDetailId = f.SmsDetailId,
                        SmsId = f.SmsId,
                        SerialNo = f.SerialNo,
                        FieldTitle = f.FieldTitle ?? "",
                        FieldDbName = f.FieldDbName ?? "",
                        SequenceNo = f.SequenceNo ?? 0,
                        NewLine = f.NewLine ?? "N",
                        ToCustomer = f.ToCustomer ?? "N",
                        ToUser = f.ToUser ?? "N",
                    }).OrderBy(x => x.SerialNo).ToList();
            var data = user.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SmsList()
        {
            return PartialView();
        }
        public ActionResult GetAllSms()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var type = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsSms> sms = (from f in db.Sms_Main
                                    select new clsSms
                                    {
                                        SmsId = f.SmsId,
                                        Type = f.Type ?? "",
                                        FormReportName = f.FormReportName ?? "",
                                        Status = f.Status ?? "",
                                        ToCustomer = f.ToCustomer ?? "N",
                                        ToUser = f.ToUser ?? "N",
                                    }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(type))
                {
                    sms = sms.Where(c => c.Type.ToLower().Contains(type.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        sms = sms.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = sms.Count();
                var data = sms.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteSms(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateSmsUser(id, "", "", "Active", User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Sms User Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this sms user first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public List<clsSms> getSmsUsers()
        {
            List<clsSms> sms = new List<clsSms>();
            sms = (from c in db.Sms_User
                   where c.Status == "Active"
                   select new clsSms
                   {
                       SmsUserId = c.SmsUserId,
                       SmsUserName = c.Name
                   }).ToList();
            return sms;
        }
    }
}