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
using System.Net;

namespace POS.Controllers
{
    public class SmsController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult SmsIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "Sms"
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
                    ViewBag.SmsUser = new SelectList(getSmsUsers(), "SmsUserId", "SmsUserName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        [HttpPost]
        public ActionResult InsertUpdateSms(clsSms sms, string userData, string fieldData)
        {
            string message = "";
            bool status = false;
            List<clsSms> userList = JsonConvert.DeserializeObject<List<clsSms>>(userData);
            List<clsSms> fieldList = JsonConvert.DeserializeObject<List<clsSms>>(fieldData);
            DataTable dtUser = new DataTable();
            dtUser.Columns.Add("Id");
            dtUser.Columns.Add("SmsUserId");

            DataTable dtFieldData = new DataTable();
            dtFieldData.Columns.Add("Id");
            dtFieldData.Columns.Add("FieldTitle");
            dtFieldData.Columns.Add("FieldDbName");
            dtFieldData.Columns.Add("SequenceNo");
            dtFieldData.Columns.Add("NewLine");
            dtFieldData.Columns.Add("ToCustomer");
            dtFieldData.Columns.Add("ToUser");

            int returnId = 0;
            if (sms.SmsId > 0)
            {
                for (int i = 0; i < userList.Count; i++)
                {
                    dtUser.Rows.Add(new object[] { i + 1, userList[i].SmsUserId });
                }
                for (int i = 0; i < fieldList.Count; i++)
                {
                    dtFieldData.Rows.Add(new object[] { i + 1, fieldList[i].FieldTitle, fieldList[i].FieldDbName, fieldList[i].SequenceNo, fieldList[i].NewLine, fieldList[i].ToCustomer, fieldList[i].ToUser });
                }
                if (userList.Count == 0)
                {
                    dtUser.Rows.Add(new object[] { 0, 0, 0, 0 });
                }
                if (fieldList.Count == 0)
                {
                    dtFieldData.Rows.Add(new object[] { 0, 0, 0, 0 });
                }
                returnId = AddEditSms(sms, dtUser, dtFieldData, "Update");
                if (returnId == 2)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Sms Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        private int AddEditSms(clsSms sms, DataTable dtSize, DataTable dtFashion, string insertUpdateStatus)
        {
            int returnId = 0;
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateSmsMain", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@SmsId", SqlDbType.Int).Value = sms.SmsId;
                        cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = sms.Status;
                        cmd.Parameters.Add("@ToCustomer", SqlDbType.NVarChar).Value = sms.ToCustomer;
                        cmd.Parameters.Add("@ToUser", SqlDbType.NVarChar).Value = sms.ToUser;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                        cmd.Parameters.Add("@dtSmsDetailUser", SqlDbType.Structured).Value = dtSize;
                        cmd.Parameters.Add("@dtSmsDetailField", SqlDbType.Structured).Value = dtFashion;
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
                { 
                }
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
        public ActionResult GetSmsUserById(int id)
        {
            List<clsSms> user = new List<clsSms>();
            user = (from f in db.Sms_Detail_User
                    join s in db.Sms_User on f.SmsUserId equals s.SmsUserId
                    where f.SmsId == id
                    select new clsSms
                             {
                                 SmsUserId = f.SmsUserId,
                                 SmsUserName = s.Name,
                                 SerialNo = f.SerialNo
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = user.ToList();
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
        public void SendSms(string invoiceNo)
        {
            List<clsSmsLog> sms = new List<clsSmsLog>();
            sms = (from s in db.Sms_Log
                   where s.ComputerNo == invoiceNo && s.Status == "Active"
                   select new clsSmsLog
                   {
                       SmsLogId = s.SmsLogId,
                       CellNo = s.CellNo ?? "",
                       SmsMessage = s.SmsMessage ?? ""
                   }).ToList();
            var smsSetting = db.SmsSettings.FirstOrDefault();
            string username = "";
            string password = "";
            string masking = "";
            if (smsSetting != null)
            {
                username = smsSetting.Username;
                password = smsSetting.Password;
                masking = smsSetting.Masking;
            }
            foreach (clsSmsLog s in sms)
            {
                string to = "";
                if (!(string.IsNullOrEmpty(s.CellNo)))
                {
                    if (s.CellNo.ToString().Contains("-"))
                    {
                        string num = s.CellNo.Split('-')[0].ToString() + s.CellNo.Split('-')[1].ToString();
                        to = "92" + num.Remove(0, 1).ToString();
                    }
                    else
                    {
                        to = "92" + s.CellNo.Remove(0, 1).ToString();
                    }
                }
                string urlString = string.Format("http://weblogin.premiumsms.pk/sendsms_url.html?Username={0}&Password={1}&From={2}&To={3}&Message={4}", username, password, masking, to, s.SmsMessage);
                Uri URI = new Uri(urlString);
                WebClient wc = new WebClient();
                string res = wc.DownloadString(URI);
                if (res.ToLower() == "message sent successfully")
                {
                    var smsLog = db.Sms_Log.Where(x => x.SmsLogId == s.SmsLogId).FirstOrDefault();
                    smsLog.Status = "Sent";
                    db.SaveChanges();
                }
            }
        }
        public ActionResult SendSmsFromHistory(int smsLogId)
        {
            List<clsSmsLog> sms = new List<clsSmsLog>();
            List<clsSmsLog> list = new List<clsSmsLog>();
            bool status = false;
            string message = "There is no message to be send.";
            if (smsLogId != 0)
            {
                list = (from s in db.Sms_Log
                        where s.SmsLogId == smsLogId
                        select new clsSmsLog
                        {
                            SmsLogId = s.SmsLogId,
                            CellNo = s.CellNo ?? "",
                            SmsMessage = s.SmsMessage ?? ""
                        }).ToList();
            }
            else
            {
                list = (from s in db.Sms_Log
                        where s.Status == "Active"
                        select new clsSmsLog
                        {
                            SmsLogId = s.SmsLogId,
                            CellNo = s.CellNo ?? "",
                            SmsMessage = s.SmsMessage ?? ""
                        }).ToList();
            }
            sms.AddRange(list);
            var smsSetting = db.SmsSettings.FirstOrDefault();
            string username = "";
            string password = "";
            string masking = "";
            if (smsSetting != null)
            {
                username = smsSetting.Username;
                password = smsSetting.Password;
                masking = smsSetting.Masking;
            }
            foreach (clsSmsLog s in sms)
            {
                try
                {
                    string to = "";
                    if (!(string.IsNullOrEmpty(s.CellNo)))
                    {
                        if (s.CellNo.ToString().Contains("-"))
                        {
                            string num = s.CellNo.Split('-')[0].ToString() + s.CellNo.Split('-')[1].ToString();
                            to = "92" + num.Remove(0, 1).ToString();
                        }
                        else
                        {
                            to = "92" + s.CellNo.Remove(0, 1).ToString();
                        }
                    }
                    string urlString = string.Format("http://weblogin.premiumsms.pk/sendsms_url.html?Username={0}&Password={1}&From={2}&To={3}&Message={4}", username, password, masking, to, s.SmsMessage);
                    Uri URI = new Uri(urlString);
                    WebClient wc = new WebClient();
                    string res = wc.DownloadString(URI);
                    if (res.ToLower() == "message sent successfully")
                    {
                        var smsLog = db.Sms_Log.Where(x => x.SmsLogId == s.SmsLogId).FirstOrDefault();
                        smsLog.Status = "Sent";
                        db.SaveChanges();
                        status = true;
                    }
                    else
                    {
                        status = false;
                        message = res;
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message.ToString();
                    status = false;
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        #region Ledger Sms
        public ActionResult LedgerSmsIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "LedgerSms"
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
                    ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetLedgerSmsReport(decimal detailCode, DateTime? fromDate, DateTime? toDate)
        {
            if (User != null)
            {
                List<clsVoucher> voucherList = new List<clsVoucher>();
                clsVoucher voucher = new clsVoucher();
                clsAccountDetail customer;
                decimal customerId = Convert.ToDecimal(detailCode);
                customer = new clsAccountDetail();
                customer = (from c in db.AC_Detail_Extra
                            join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                            where c.DetailCode == customerId
                            select new clsAccountDetail
                            {
                                DetailCode = customerId,
                                Name = cd.Name,
                                Address = c.Address ?? string.Empty,
                                Phone1 = c.Phone1 ?? string.Empty,
                            }).FirstOrDefault();
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn2 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn2", typeof(decimal));
                db.sp_GetCreditDaysAndPreviousBalance("", customerId, fromDate, "", 0, User.FiscalYearId, CheckReturn, CheckReturn2);
                if (CheckReturn2.Value.ToString() != "")
                {
                    customer.PreviousBalance = Convert.ToDecimal(CheckReturn2.Value);
                }
                else
                {
                    customer.PreviousBalance = 0;
                }
                voucher.DetailList.Add(customer);
                List<clsVoucher> list = GetLedgerSmsDetail(customerId, fromDate, toDate);
                list = list.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ToList();
                voucher.VouchersList = list;
                return PartialView(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetLedgerSmsDetail(decimal customerId, DateTime? fromDate, DateTime? toDate)
        {
            List<clsVoucher> list = new List<clsVoucher>();
            list = (from f in db.AC_Voucher_Main
                    join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                    where
                    fd.DetailCode == customerId
                    && f.VoucherDate >= fromDate
                    && f.VoucherDate <= toDate
                    select new clsVoucher
                    {
                        VoucherNo = f.VoucherNo,
                        VoucherDate = f.VoucherDate,
                        VoucherType = f.VoucherType,
                        DetailCode = fd.DetailCode,
                        RefrenceNo = f.RefrenceNo,
                        SerialNo = fd.SerialNo,
                        FromDate = fromDate,
                        ToDate = toDate,
                        Remarks = fd.Remarks,
                        Debit = fd.Debit,
                        Credit = fd.Credit,
                        ChequeNo = fd.ChequeNo ?? "",
                        ChequeComputerNo = fd.ChequeComputerNo ?? 0,
                        //LineChequeNo = db.AC_ChequeInfo.Where(x => x.ChequeInfoId == fd.ChequeComputerNo).Select(x => x.ChequeNo).FirstOrDefault() ?? ""
                    }).ToList();
            return list;
        }
        public ActionResult SendLedgerSmsPost(decimal detailCode, DateTime? fromDate, DateTime? toDate, int toParty, int toManage)
        {
            if (User != null)
            {
                bool status = false;
                string message2 = "";
                clsAccountDetail customer = new clsAccountDetail();
                customer = (from c in db.AC_Detail_Extra
                            join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                            where c.DetailCode == detailCode
                            select new clsAccountDetail
                            {
                                DetailCode = c.DetailCode,
                                Name = cd.Name,
                                Cell1 = c.Cell1 ?? string.Empty,
                            }).FirstOrDefault();
                string cell = db.AC_Detail_Extra.Where(x => x.DetailCode == detailCode).Select(x => x.Cell1).FirstOrDefault() ?? "";
                string message = "";
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn2 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn2", typeof(decimal));
                db.sp_GetCreditDaysAndPreviousBalance("", detailCode, fromDate, "", 0, User.FiscalYearId, CheckReturn, CheckReturn2);
                decimal prevBalance = 0;
                if (CheckReturn2.Value.ToString() != "")
                {
                    prevBalance = Math.Round(Convert.ToDecimal(CheckReturn2.Value), 0);
                }
                message = customer.Name + "\n" + "Previous Balance: " + prevBalance.ToString();
                string invoices = GetLedgerSmsMessage(detailCode, fromDate, toDate, prevBalance);
                message += invoices;

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
                if (toParty == 1)
                {
                    dtUser.Rows.Clear();
                    dtUser.Rows.Add(new object[] { 1, 0, DateTime.Now, detailCode, 0, customer.Cell1, message, "", "Active", DateTime.Now });
                    returnId = AddEditSmsLog(dtUser, "Save");
                    if (returnId > 0)
                    {
                        ModelState.Clear();
                        status = true;
                        message2 = "Sms Log Successfully Updated";
                        SmsController sms2 = new SmsController();
                        sms2.SendSmsFromHistory(returnId);
                    }
                    else
                    {
                        ModelState.Clear();
                        status = false;
                        message2 = "Error! Please try again.";
                    }
                }
                if (toManage == 1)
                {
                    List<clsSmsUser> user = new List<clsSmsUser>();
                    user = (from u in db.Sms_User
                            where u.Status == "Active"
                            select new clsSmsUser
                            {
                                SmsUserId = u.SmsUserId,
                                Name = u.Name ?? "",
                                CellNo = u.CellNo ?? ""
                            }).ToList();
                    foreach (clsSmsUser u in user)
                    {
                        dtUser.Rows.Clear();
                        dtUser.Rows.Add(new object[] { 1, 0, DateTime.Now, 0, 0, u.CellNo, message, "", "Active", DateTime.Now });
                        returnId = AddEditSmsLog(dtUser, "Save");
                        if (returnId > 0)
                        {
                            ModelState.Clear();
                            status = true;
                            message2 = "Sms Log Successfully Updated";
                            SmsController sms2 = new SmsController();
                            sms2.SendSmsFromHistory(returnId);
                        }
                        else
                        {
                            ModelState.Clear();
                            status = false;
                            message2 = "Error! Please try again.";
                        }
                    }
                }
                return new JsonResult { Data = new { status = status, message = message2 } };
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public string GetLedgerSmsMessage(decimal customerId, DateTime? fromDate, DateTime? toDate, decimal prevBalance)
        {
            string message = "";
            List<clsVoucher> list = new List<clsVoucher>();
            list = (from f in db.AC_Voucher_Main
                    join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                    where
                    fd.DetailCode == customerId
                    && f.VoucherDate >= fromDate
                    && f.VoucherDate <= toDate
                    select new clsVoucher
                    {
                        VoucherNo = f.VoucherNo,
                        VoucherDate = f.VoucherDate,
                        Debit = fd.Debit,
                        Credit = fd.Credit,
                    }).ToList();
            int j = 0;
            decimal closingBalance = 0;
            decimal amount = 0;
            foreach (clsVoucher c in list)
            {
                amount = Convert.ToDecimal(c.Debit) - Convert.ToDecimal(c.Credit);
                message += "\n" + Convert.ToDateTime(c.VoucherDate).ToString("dd/MM/yyyy") + " , " + c.VoucherNo.ToString() + " , " + Math.Round(amount, 0);
                if (j == 0)
                {
                    closingBalance = prevBalance + amount;
                }
                else
                {
                    closingBalance = closingBalance + amount;
                }
                j++;
            }
            message += "\n" + "Closing Balance: " + Math.Round(closingBalance, 0);
            return message;
        }
        public ActionResult SendOutstandingSmsPost(decimal detailCode, DateTime? fromDate, DateTime? toDate, int toParty, int toManage)
        {
            if (User != null)
            {
                bool status = false;
                string message2 = "";
                clsAccountDetail customer = new clsAccountDetail();
                customer = (from c in db.AC_Detail_Extra
                            join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                            where c.DetailCode == detailCode
                            select new clsAccountDetail
                            {
                                DetailCode = c.DetailCode,
                                Name = cd.Name,
                                Cell1 = c.Cell1 ?? string.Empty,
                            }).FirstOrDefault();
                string cell = db.AC_Detail_Extra.Where(x => x.DetailCode == detailCode).Select(x => x.Cell1).FirstOrDefault() ?? "";
                string message = "";
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn2 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn2", typeof(decimal));
                db.sp_GetCreditDaysAndPreviousBalance("", detailCode, fromDate, "", 0, User.FiscalYearId, CheckReturn, CheckReturn2);
                decimal prevBalance = 0;
                if (CheckReturn2.Value.ToString() != "")
                {
                    prevBalance = Math.Round(Convert.ToDecimal(CheckReturn2.Value), 0);
                }
                message = customer.Name + "\n" + "Previous Balance: " + prevBalance.ToString();
                string invoices = GetOutstandingSmsMessage(detailCode, fromDate, toDate, prevBalance);
                message += invoices;

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
                if (toParty == 1)
                {
                    dtUser.Rows.Clear();
                    dtUser.Rows.Add(new object[] { 1, 0, DateTime.Now, detailCode, 0, customer.Cell1, message, "", "Active", DateTime.Now });
                    returnId = AddEditSmsLog(dtUser, "Save");
                    if (returnId > 0)
                    {
                        ModelState.Clear();
                        status = true;
                        message2 = "Sms Log Successfully Updated";
                        SmsController sms2 = new SmsController();
                        sms2.SendSmsFromHistory(returnId);
                    }
                    else
                    {
                        ModelState.Clear();
                        status = false;
                        message2 = "Error! Please try again.";
                    }
                }
                if (toManage == 1)
                {
                    List<clsSmsUser> user = new List<clsSmsUser>();
                    user = (from u in db.Sms_User
                            where u.Status == "Active"
                            select new clsSmsUser
                            {
                                SmsUserId = u.SmsUserId,
                                Name = u.Name ?? "",
                                CellNo = u.CellNo ?? ""
                            }).ToList();
                    foreach (clsSmsUser u in user)
                    {
                        dtUser.Rows.Clear();
                        dtUser.Rows.Add(new object[] { 1, 0, DateTime.Now, 0, 0, u.CellNo, message, "", "Active", DateTime.Now });
                        returnId = AddEditSmsLog(dtUser, "Save");
                        if (returnId > 0)
                        {
                            ModelState.Clear();
                            status = true;
                            message2 = "Sms Log Successfully Updated";
                            SmsController sms2 = new SmsController();
                            sms2.SendSmsFromHistory(returnId);
                        }
                        else
                        {
                            ModelState.Clear();
                            status = false;
                            message2 = "Error! Please try again.";
                        }
                    }
                }

                return new JsonResult { Data = new { status = status, message = message2 } };
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public string GetOutstandingSmsMessage(decimal customerId, DateTime? fromDate, DateTime? toDate, decimal prevBalance)
        {
            string message = "";
            List<clsVoucher> list = new List<clsVoucher>();
            list = (from f in db.AC_Voucher_Main
                    join fd in db.AC_Voucher_Detail on f.VoucherNo equals fd.VoucherNo
                    where
                    fd.DetailCode == customerId
                    && f.VoucherDate >= fromDate
                    && f.VoucherDate <= toDate
                    select new clsVoucher
                    {
                        VoucherNo = f.VoucherNo,
                        VoucherDate = f.VoucherDate,
                        Debit = fd.Debit,
                        Credit = fd.Credit,
                    }).ToList();
            int j = 0;
            decimal closingBalance = 0;
            decimal amount = 0;
            foreach (clsVoucher c in list)
            {
                amount = Convert.ToDecimal(c.Debit) - Convert.ToDecimal(c.Credit);
                message += "\n" + Convert.ToDateTime(c.VoucherDate).ToString("dd/MM/yyyy") + " , " + c.VoucherNo.ToString() + " , " + Math.Round(amount, 0);
                if (j == 0)
                {
                    closingBalance = prevBalance + amount;
                }
                else
                {
                    closingBalance = closingBalance + amount;
                }
                j++;
            }
            message = "\n" + "Closing Balance: " + Math.Round(closingBalance, 0);
            return message;
        }
        private int AddEditSmsLog(DataTable dtLog, string insertUpdateStatus)
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
                        cmd.Parameters.Add("@CheckReturn2", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        returnId = Convert.ToInt32(cmd.Parameters["@CheckReturn2"].Value);
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
        public List<clsAccountDetail> getCustomers()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && c.Type != "Detail"
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name
                      }).ToList();
            return detail;
        }
        #endregion
        #region Sale Aging Sms
        public ActionResult SaleAgingSmsIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SaleAgingSms"
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
                    ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetSaleAgingSmsReport(decimal detailCode, DateTime? toDate)
        {
            if (User != null)
            {
                List<clsVoucher> voucherList = new List<clsVoucher>();
                clsVoucher voucher = new clsVoucher();
                clsAccountDetail customer;
                decimal customerId = Convert.ToDecimal(detailCode);
                customer = new clsAccountDetail();
                customer = (from c in db.AC_Detail_Extra
                            join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                            where c.DetailCode == customerId
                            select new clsAccountDetail
                            {
                                DetailCode = customerId,
                                Name = cd.Name,
                                Address = c.Address ?? string.Empty,
                                Phone1 = c.Phone1 ?? string.Empty,
                            }).FirstOrDefault();
                voucher.DetailList.Add(customer);
                List<clsVoucher> list = GetSaleAgingSmsDetail(customerId, toDate);
                list = list.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ToList();
                voucher.VouchersList = list;
                return PartialView(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetSaleAgingSmsDetail(decimal customerId, DateTime? toDate)
        {
            List<clsVoucher> list = new List<clsVoucher>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_AgingSale", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vCustCode", SqlDbType.Decimal).Value = customerId;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = toDate;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsVoucher()
                {
                    DetailCode = customerId,
                    DetailName = dt[1].ToString(),
                    Address = dt[2].ToString(),
                    Phone1 = dt[3].ToString(),
                    CreditDays = Convert.ToInt32(dt[4]),
                    DueDays = Convert.ToInt32(dt[5]),
                    VoucherNo = dt[6].ToString(),
                    VoucherDate = Convert.ToDateTime(dt[7]),
                    NetAmount = Convert.ToDecimal(dt[8]),
                    ReceivedAmount = Convert.ToDecimal(dt[9]),
                    BalanceAmount = Convert.ToDecimal(dt[10]),
                    AdjustAmount = Convert.ToDecimal(dt[11]),
                    PendingAmount = Convert.ToDecimal(dt[12]),
                    ToDate = toDate,
                });
            }
            list = list.Where(x => x.PendingAmount != 0).ToList();
            return list;
        }
        public ActionResult SendSaleAgingSmsPost(decimal detailCode, DateTime? toDate, int toParty, int toManage)
        {
            if (User != null)
            {
                bool status = false;
                string message2 = "";
                clsAccountDetail customer = new clsAccountDetail();
                customer = (from c in db.AC_Detail_Extra
                            join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                            where c.DetailCode == detailCode
                            select new clsAccountDetail
                            {
                                DetailCode = c.DetailCode,
                                Name = cd.Name,
                                Cell1 = c.Cell1 ?? string.Empty,
                            }).FirstOrDefault();
                string cell = db.AC_Detail_Extra.Where(x => x.DetailCode == detailCode).Select(x => x.Cell1).FirstOrDefault() ?? "";
                string message = "";

                message = customer.Name;
                string invoices = GetSaleAgingSmsMessage(detailCode, toDate);
                message += invoices;

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
                if (toParty == 1)
                {
                    dtUser.Rows.Clear();
                    dtUser.Rows.Add(new object[] { 1, 0, DateTime.Now, detailCode, 0, customer.Cell1, message, "", "Active", DateTime.Now });
                    returnId = AddEditSmsLog(dtUser, "Save");
                    if (returnId > 0)
                    {
                        ModelState.Clear();
                        status = true;
                        message2 = "Sms Log Successfully Updated";
                        SmsController sms2 = new SmsController();
                        sms2.SendSmsFromHistory(returnId);
                    }
                    else
                    {
                        ModelState.Clear();
                        status = false;
                        message2 = "Error! Please try again.";
                    }
                }
                if (toManage == 1)
                {
                    List<clsSmsUser> user = new List<clsSmsUser>();
                    user = (from u in db.Sms_User
                            where u.Status == "Active"
                            select new clsSmsUser
                            {
                                SmsUserId = u.SmsUserId,
                                Name = u.Name ?? "",
                                CellNo = u.CellNo ?? ""
                            }).ToList();
                    foreach (clsSmsUser u in user)
                    {
                        dtUser.Rows.Clear();
                        dtUser.Rows.Add(new object[] { 1, 0, DateTime.Now, 0, 0, u.CellNo, message, "", "Active", DateTime.Now });
                        returnId = AddEditSmsLog(dtUser, "Save");
                        if (returnId > 0)
                        {
                            ModelState.Clear();
                            status = true;
                            message2 = "Sms Log Successfully Updated";
                            SmsController sms2 = new SmsController();
                            sms2.SendSmsFromHistory(returnId);
                        }
                        else
                        {
                            ModelState.Clear();
                            status = false;
                            message2 = "Error! Please try again.";
                        }
                    }
                }

                return new JsonResult { Data = new { status = status, message = message2 } };
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public string GetSaleAgingSmsMessage(decimal customerId, DateTime? toDate)
        {
            string message = "";
            List<clsVoucher> list = new List<clsVoucher>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_AgingSale", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vCustCode", SqlDbType.Decimal).Value = customerId;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = toDate;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsVoucher()
                {
                    DetailCode = Convert.ToDecimal(dt[0]),
                    DetailName = dt[1].ToString(),
                    Address = dt[2].ToString(),
                    Phone1 = dt[3].ToString(),
                    CreditDays = Convert.ToInt32(dt[4]),
                    DueDays = Convert.ToInt32(dt[5]),
                    VoucherNo = dt[6].ToString(),
                    VoucherDate = Convert.ToDateTime(dt[7]),
                    NetAmount = Convert.ToDecimal(dt[8]),
                    ReceivedAmount = Convert.ToDecimal(dt[9]),
                    BalanceAmount = Convert.ToDecimal(dt[10]),
                    AdjustAmount = Convert.ToDecimal(dt[11]),
                    PendingAmount = Convert.ToDecimal(dt[12]),
                    Bellow30 = Convert.ToDecimal(dt[13]),
                    TO60 = Convert.ToDecimal(dt[14]),
                    TO90 = Convert.ToDecimal(dt[15]),
                    Above90 = Convert.ToDecimal(dt[16]),
                    SalesmanID = Convert.ToInt32(dt[17]),
                    ToDate = toDate,
                });
            }
            list = list.Where(x => x.PendingAmount != 0).ToList();
            int j = 0;
            decimal closingBalance = 0;
            decimal amount = 0;
            foreach (clsVoucher c in list)
            {
                amount = Convert.ToDecimal(c.PendingAmount);
                message += "\n" + c.DueDays.ToString() + " days, " + Math.Round(amount, 0);
                closingBalance += amount;
            }
            message += "\n" + "Total Balance: " + Math.Round(closingBalance, 0);
            return message;
        }
        #endregion
        #region Sale Summary Sms
        public ActionResult SaleSummarySmsIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SaleSummarySms"
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
                    ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetSaleSummarySmsReport(decimal detailCode, DateTime? fromDate, DateTime? toDate)
        {
            if (User != null)
            {
                List<clsVoucher> voucherList = new List<clsVoucher>();
                clsVoucher voucher = new clsVoucher();
                clsAccountDetail customer;
                decimal customerId = Convert.ToDecimal(detailCode);
                customer = new clsAccountDetail();
                //customer = (from c in db.AC_Detail_Extra
                //            join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                //            where c.DetailCode == customerId
                //            select new clsAccountDetail
                //            {
                //                DetailCode = customerId,
                //                Name = cd.Name,
                //                Address = c.Address ?? string.Empty,
                //                Phone1 = c.Phone1 ?? string.Empty,
                //            }).FirstOrDefault();
                voucher.DetailList.Add(customer);
                List<clsVoucher> list = GetSaleSummarySmsDetail(customerId, fromDate, toDate);
                list = list.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ToList();
                voucher.VouchersList = list;
                return PartialView(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetSaleSummarySmsDetail(decimal customerId, DateTime? fromDate, DateTime? toDate)
        {
            List<clsVoucher> list = new List<clsVoucher>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_SummaryForSms", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate;
            cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate;
            cmd.Parameters.Add("@CustCode", SqlDbType.Decimal).Value = customerId;
            cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = "Sale";
            cmd.Parameters.Add("@FiscalYearId", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsVoucher()
                {
                    ByDate = Convert.ToDateTime(dt[0]),
                    CashSale = Convert.ToDecimal(dt[1]),
                    CreditSale = Convert.ToDecimal(dt[2]),
                    TotalSale = Convert.ToDecimal(dt[3]),
                    CashSaleReturn = Convert.ToDecimal(dt[4]),
                    CreditSaleReturn = Convert.ToDecimal(dt[5]),
                    TotalSaleReturn = Convert.ToDecimal(dt[6]),
                    NetSale = Convert.ToDecimal(dt[7]),
                    Counts = Convert.ToInt32(dt[8]),
                });
            }
            return list;
        }
        public ActionResult SendSaleSummarySmsPost(decimal detailCode, DateTime? fromDate, DateTime? toDate)
        {
            if (User != null)
            {
                bool status = false;
                string message2 = "";
                clsAccountDetail customer = new clsAccountDetail();
                //customer = (from c in db.AC_Detail_Extra
                //            join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                //            where c.DetailCode == detailCode
                //            select new clsAccountDetail
                //            {
                //                DetailCode = c.DetailCode,
                //                Name = cd.Name,
                //                Cell1 = c.Cell1 ?? string.Empty,
                //            }).FirstOrDefault();
                //string cell = db.AC_Detail_Extra.Where(x => x.DetailCode == detailCode).Select(x => x.Cell1).FirstOrDefault() ?? "";
                string message = "";
                string invoices = GetSaleSummarySmsMessage(detailCode, fromDate, toDate);
                message += invoices;

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
                List<clsSmsUser> user = new List<clsSmsUser>();
                user = (from u in db.Sms_User
                        where u.Status == "Active"
                        select new clsSmsUser
                        {
                            SmsUserId = u.SmsUserId,
                            Name = u.Name ?? "",
                            CellNo = u.CellNo ?? ""
                        }).ToList();
                foreach (clsSmsUser u in user)
                {
                    dtUser.Rows.Clear();
                    dtUser.Rows.Add(new object[] { 1, 0, DateTime.Now, 0, 0, u.CellNo, message, "", "Active", DateTime.Now });
                    returnId = AddEditSmsLog(dtUser, "Save");
                    if (returnId > 0)
                    {
                        ModelState.Clear();
                        status = true;
                        message2 = "Sms Log Successfully Updated";
                        SmsController sms2 = new SmsController();
                        sms2.SendSmsFromHistory(returnId);
                    }
                    else
                    {
                        ModelState.Clear();
                        status = false;
                        message2 = "Error! Please try again.";
                    }
                }
                return new JsonResult { Data = new { status = status, message = message2 } };
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public string GetSaleSummarySmsMessage(decimal customerId, DateTime? fromDate, DateTime? toDate)
        {
            string message = "";
            List<clsVoucher> list = new List<clsVoucher>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_SummaryForSms", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate;
            cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate;
            cmd.Parameters.Add("@CustCode", SqlDbType.Decimal).Value = customerId;
            cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = "Sale";
            cmd.Parameters.Add("@FiscalYearId", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsVoucher()
                {
                    ByDate = Convert.ToDateTime(dt[0]),
                    CashSale = Convert.ToDecimal(dt[1]),
                    CreditSale = Convert.ToDecimal(dt[2]),
                    TotalSale = Convert.ToDecimal(dt[3]),
                    CashSaleReturn = Convert.ToDecimal(dt[4]),
                    CreditSaleReturn = Convert.ToDecimal(dt[5]),
                    TotalSaleReturn = Convert.ToDecimal(dt[6]),
                    NetSale = Convert.ToDecimal(dt[7]),
                    Counts = Convert.ToInt32(dt[8]),
                });
            }
            int j = 0;
            decimal closingBalance = 0;
            decimal amount = 0;
            foreach (clsVoucher c in list)
            {
                message += "Date:" + Convert.ToDateTime(c.ByDate).ToString("dd/MM/yyyy") + ","
                            + "Cash Sale:" + Math.Round(Convert.ToDecimal(c.CashSale), 0) + ","
                            + "Credit Sale:" + Math.Round(Convert.ToDecimal(c.CreditSale), 0) + ","
                            + "Total Sale:" + Math.Round(Convert.ToDecimal(c.TotalSale), 0) + ","
                            + "Cash Sale Return:" + Math.Round(Convert.ToDecimal(c.CashSaleReturn), 0) + ","
                            + "Credit Sale Return:" + Math.Round(Convert.ToDecimal(c.CreditSaleReturn), 0) + ","
                            + "Total Sale Return:" + Math.Round(Convert.ToDecimal(c.TotalSaleReturn), 0) + ","
                            + "Net Sale:" + Math.Round(Convert.ToDecimal(c.NetSale), 0)
                            + "\n";
                //closingBalance = closingBalance + amount;
                j++;
            }
            //message += "\n" + "Closing Balance: " + Math.Round(closingBalance, 0);
            return message;
        }

        #endregion
        #region Purchase Summary Sms
        public ActionResult PurchaseSummarySmsIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SaleSummarySms"
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
                    ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetPurchaseSummarySmsReport(decimal detailCode, DateTime? fromDate, DateTime? toDate)
        {
            if (User != null)
            {
                List<clsVoucher> voucherList = new List<clsVoucher>();
                clsVoucher voucher = new clsVoucher();
                clsAccountDetail customer;
                decimal customerId = Convert.ToDecimal(detailCode);
                customer = new clsAccountDetail();
                //customer = (from c in db.AC_Detail_Extra
                //            join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                //            where c.DetailCode == customerId
                //            select new clsAccountDetail
                //            {
                //                DetailCode = customerId,
                //                Name = cd.Name,
                //                Address = c.Address ?? string.Empty,
                //                Phone1 = c.Phone1 ?? string.Empty,
                //            }).FirstOrDefault();
                voucher.DetailList.Add(customer);
                List<clsVoucher> list = GetPurchaseSummarySmsDetail(customerId, fromDate, toDate);
                list = list.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ToList();
                voucher.VouchersList = list;
                return PartialView(voucher);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsVoucher> GetPurchaseSummarySmsDetail(decimal customerId, DateTime? fromDate, DateTime? toDate)
        {
            List<clsVoucher> list = new List<clsVoucher>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_SummaryForSms", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate;
            cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate;
            cmd.Parameters.Add("@CustCode", SqlDbType.Decimal).Value = customerId;
            cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = "Purchase";
            cmd.Parameters.Add("@FiscalYearId", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsVoucher()
                {
                    ByDate = Convert.ToDateTime(dt[0]),
                    CashSale = Convert.ToDecimal(dt[1]),
                    CreditSale = Convert.ToDecimal(dt[2]),
                    TotalSale = Convert.ToDecimal(dt[3]),
                    CashSaleReturn = Convert.ToDecimal(dt[4]),
                    CreditSaleReturn = Convert.ToDecimal(dt[5]),
                    TotalSaleReturn = Convert.ToDecimal(dt[6]),
                    NetSale = Convert.ToDecimal(dt[7]),
                    Counts = Convert.ToInt32(dt[8]),
                });
            }
            return list;
        }
        public ActionResult SendPurchaseSummarySmsPost(decimal detailCode, DateTime? fromDate, DateTime? toDate)
        {
            if (User != null)
            {
                bool status = false;
                string message2 = "";
                clsAccountDetail customer = new clsAccountDetail();
                //customer = (from c in db.AC_Detail_Extra
                //            join cd in db.AC_Detail on c.DetailCode equals cd.DetailCode
                //            where c.DetailCode == detailCode
                //            select new clsAccountDetail
                //            {
                //                DetailCode = c.DetailCode,
                //                Name = cd.Name,
                //                Cell1 = c.Cell1 ?? string.Empty,
                //            }).FirstOrDefault();
                //string cell = db.AC_Detail_Extra.Where(x => x.DetailCode == detailCode).Select(x => x.Cell1).FirstOrDefault() ?? "";
                string message = "";
                string invoices = GetPurchaseSummarySmsMessage(detailCode, fromDate, toDate);
                message += invoices;

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
                List<clsSmsUser> user = new List<clsSmsUser>();
                user = (from u in db.Sms_User
                        where u.Status == "Active"
                        select new clsSmsUser
                        {
                            SmsUserId = u.SmsUserId,
                            Name = u.Name ?? "",
                            CellNo = u.CellNo ?? ""
                        }).ToList();
                foreach (clsSmsUser u in user)
                {
                    dtUser.Rows.Clear();
                    dtUser.Rows.Add(new object[] { 1, 0, DateTime.Now, 0, 0, u.CellNo, message, "", "Active", DateTime.Now });
                    returnId = AddEditSmsLog(dtUser, "Save");
                    if (returnId > 0)
                    {
                        ModelState.Clear();
                        status = true;
                        message2 = "Sms Log Successfully Updated";
                        SmsController sms2 = new SmsController();
                        sms2.SendSmsFromHistory(returnId);
                    }
                    else
                    {
                        ModelState.Clear();
                        status = false;
                        message2 = "Error! Please try again.";
                    }
                }
                return new JsonResult { Data = new { status = status, message = message2 } };
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public string GetPurchaseSummarySmsMessage(decimal customerId, DateTime? fromDate, DateTime? toDate)
        {
            string message = "";
            List<clsVoucher> list = new List<clsVoucher>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_SummaryForSms", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate;
            cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate;
            cmd.Parameters.Add("@CustCode", SqlDbType.Decimal).Value = customerId;
            cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = "Purchase";
            cmd.Parameters.Add("@FiscalYearId", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsVoucher()
                {
                    ByDate = Convert.ToDateTime(dt[0]),
                    CashSale = Convert.ToDecimal(dt[1]),
                    CreditSale = Convert.ToDecimal(dt[2]),
                    TotalSale = Convert.ToDecimal(dt[3]),
                    CashSaleReturn = Convert.ToDecimal(dt[4]),
                    CreditSaleReturn = Convert.ToDecimal(dt[5]),
                    TotalSaleReturn = Convert.ToDecimal(dt[6]),
                    NetSale = Convert.ToDecimal(dt[7]),
                    Counts = Convert.ToInt32(dt[8]),
                });
            }
            int j = 0;
            decimal closingBalance = 0;
            decimal amount = 0;
            foreach (clsVoucher c in list)
            {
                message += "Date:" + Convert.ToDateTime(c.ByDate).ToString("dd/MM/yyyy") + ","
                            + "Cash Purchase:" + Math.Round(Convert.ToDecimal(c.CashSale), 0) + ","
                            + "Credit Purchase:" + Math.Round(Convert.ToDecimal(c.CreditSale), 0) + ","
                            + "Total Purchase:" + Math.Round(Convert.ToDecimal(c.TotalSale), 0) + ","
                            + "Cash Purchase Return:" + Math.Round(Convert.ToDecimal(c.CashSaleReturn), 0) + ","
                            + "Credit Purchase Return:" + Math.Round(Convert.ToDecimal(c.CreditSaleReturn), 0) + ","
                            + "Total Purchase Return:" + Math.Round(Convert.ToDecimal(c.TotalSaleReturn), 0) + ","
                            + "Net Purchase:" + Math.Round(Convert.ToDecimal(c.NetSale), 0)
                            + "\n";
                //closingBalance = closingBalance + amount;
                j++;
            }
            //message += "\n" + "Closing Balance: " + Math.Round(closingBalance, 0);
            return message;
        }

        #endregion
    }
}