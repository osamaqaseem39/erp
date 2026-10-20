using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;

namespace POS.Controllers
{
    public class StoreController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Store In
        public ActionResult StoreInIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "StoreIn"
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
                    string fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    string toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    ViewBag.FromDate = fromDate;
                    ViewBag.ToDate = toDate;
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        [HttpPost]
        public ActionResult InsertUpdateStoreIn(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsInvoice> invoice = JsonConvert.DeserializeObject<List<clsInvoice>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");

            string invoiceNo = invoice[0].ComputerNo;
            if (invoice[0].IsNewOrEdit == "Edit")
            {
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID ,""});
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Store In", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, invoice[0].Reason, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Store In Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID ,""});
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Store In", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, invoice[0].Reason, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Store In Successfully Saved";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, invoiceNo = invoiceNo } };
        }
        private string AddEditInvoice(string invoiceNo, string type, DateTime? recordDate, decimal? detailCode,
                                    string referenceNo, string remarks, decimal? grossTotal, string discountType,
                                    decimal? discount, decimal? netDiscount, decimal? overHead, decimal? freight,
                                    decimal? netAmount, int? received, decimal? receivedDetailCode, decimal? receivedAmount, string receivedRemarks,
                                    decimal? balanceAmount, string status, int businessId, int? userId, int? lastUserId, DateTime? saveDate, DateTime? lastDate,
                                    decimal? previousBalance, int? creditDays, int fiscalYear, int extraOrNot, string transporter, string biltyNo, string packing,
                                    DateTime? biltyDate, int? biltyPending, int? salemanId, string reason, DataTable dt, string insertUpdateStatus)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateInvoice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ComputerNo", SqlDbType.VarChar, 25).Value = invoiceNo;
                        cmd.Parameters.Add("@Type", SqlDbType.VarChar, 25).Value = type;
                        cmd.Parameters.Add("@RecordDate", SqlDbType.Date).Value = recordDate;
                        cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = detailCode;
                        cmd.Parameters.Add("@RefrenceNo", SqlDbType.VarChar, 30).Value = referenceNo;
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 255).Value = remarks;
                        cmd.Parameters.Add("@GrossTotal", SqlDbType.Decimal).Value = grossTotal;
                        cmd.Parameters.Add("@DiscountType", SqlDbType.VarChar, 5).Value = discountType;
                        cmd.Parameters.Add("@Discount", SqlDbType.Decimal).Value = discount;
                        cmd.Parameters.Add("@NetDiscount", SqlDbType.Decimal).Value = netDiscount;
                        cmd.Parameters.Add("@Overhead", SqlDbType.Decimal).Value = overHead;
                        cmd.Parameters.Add("@Freight", SqlDbType.Decimal).Value = freight;
                        cmd.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = netAmount;
                        cmd.Parameters.Add("@Received", SqlDbType.Int).Value = received;
                        cmd.Parameters.Add("@ReceivedDetailCode", SqlDbType.Decimal).Value = receivedDetailCode;
                        cmd.Parameters.Add("@ReceivedAmount", SqlDbType.Decimal).Value = receivedAmount;
                        cmd.Parameters.Add("@ReceivedRemarks", SqlDbType.VarChar, 255).Value = receivedRemarks;
                        cmd.Parameters.Add("@BalanceAmount", SqlDbType.Decimal).Value = balanceAmount;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 30).Value = status;
                        cmd.Parameters.Add("@BusinessID", SqlDbType.Int).Value = businessId;
                        cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                        cmd.Parameters.Add("@LastUserID", SqlDbType.VarChar, 30).Value = lastUserId.ToString();
                        cmd.Parameters.Add("@SaveDateTime", SqlDbType.DateTime).Value = saveDate;
                        cmd.Parameters.Add("@LastEditDateTime", SqlDbType.DateTime).Value = lastDate;
                        cmd.Parameters.Add("@PreviousBalance", SqlDbType.Decimal).Value = previousBalance;
                        cmd.Parameters.Add("@CreditDays", SqlDbType.Int).Value = creditDays;
                        cmd.Parameters.Add("@FiscalYearID", SqlDbType.Int).Value = fiscalYear;
                        cmd.Parameters.Add("@ExtraOrNot", SqlDbType.Int).Value = extraOrNot;
                        cmd.Parameters.Add("@Transporter", SqlDbType.VarChar, 255).Value = transporter;
                        cmd.Parameters.Add("@BiltyNo", SqlDbType.VarChar, 50).Value = biltyNo;
                        cmd.Parameters.Add("@BiltyPacking", SqlDbType.VarChar, 50).Value = packing;
                        cmd.Parameters.Add("@BiltyDate", SqlDbType.Date).Value = biltyDate;
                        cmd.Parameters.Add("@BiltyPending", SqlDbType.Int).Value = biltyPending;
                        cmd.Parameters.Add("@SalesmanID", SqlDbType.Int).Value = salemanId;
                        cmd.Parameters.Add("@Reason", SqlDbType.VarChar, 200).Value = reason;
                        cmd.Parameters.Add("@InvoiceType", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@CashCustomerName", SqlDbType.NVarChar).Value = "";
                        cmd.Parameters.Add("@ShowBalance", SqlDbType.Int).Value = 1;
                        cmd.Parameters.Add("@NoDueDateAlert", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@dtInvoiceDetail", SqlDbType.Structured).Value = dt;
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.NVarChar).Value = insertUpdateStatus;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 25).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        returnId = cmd.Parameters["@CheckReturn"].Value.ToString();
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
        public ActionResult GetStoreInById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           RecordDate = f.RecordDate,
                           Reason = f.Reason,
                           RefrenceNo = f.RefrenceNo,
                           Remarks = f.Remarks,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           //ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           //Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           //BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           //BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           //BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           //BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           //SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetStoreInDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                // ItemNote = f.ItemNote,
                                 LineDiscountType=f.DiscountType ,
                                 LineDiscount =f.Discount, 
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue,
                                 SerialNo = f.SerialNo
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult StoreInList()
        {
            return PartialView();
        }
        public ActionResult GetAllStoreIn()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var invoiceNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var reason = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var referenceNo = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Store In"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                Reason = f.Reason,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                NetAmount = f.NetAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                FiscalYearID = f.FiscalYearID,
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(invoiceNo))
                {
                    invoice = invoice.Where(c => c.ComputerNo.ToLower().Contains(invoiceNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(reason))
                {
                    invoice = invoice.Where(c => c.Reason.ToLower().Contains(reason.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(referenceNo))
                {
                    invoice = invoice.Where(c => c.RefrenceNo.ToLower().Contains(referenceNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    invoice = invoice.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = invoice.Count();
                var data = invoice.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteStore(string id)
        {
            string message = "";
            bool status = false;
            List<clsInvoice> invoice = new List<clsInvoice>();
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0,"" });
            string returnId = AddEditInvoice(id, "", DateTime.Now, 0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", dt, "Delete");
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Invoice Successfully Deleted";
            }
            else if (returnId == "-9999")
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this invoice first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        
        public List<clsItem> getItems()
        {
            List<clsItem> item = new List<clsItem>();
            item = (from c in db.Item_Information
                    where c.Status == "Active"
                    select new clsItem
                    {
                        ItemID = c.ItemID,
                        ItemName = c.ItemName
                    }).ToList();
            return item;
        }
        public List<clsWarehouse> getWarehouses()
        {
            List<clsWarehouse> warehouse = new List<clsWarehouse>();
            warehouse = (from c in db.Admin_Setup_Warehouse
                         where c.Status == "Active"
                         select new clsWarehouse
                         {
                             WarehouseID = c.WarehouseID,
                             WarehouseName = c.WarehouseName
                         }).ToList();
            return warehouse;
        }
        public ActionResult getItemInformation(int itemId)
        {
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("spGet_ItemInformation", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
            cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = 0;
            cmd.Parameters.Add("@CashName", SqlDbType.VarChar, 200).Value = "";
            SqlDataReader r = cmd.ExecuteReader();
            clsItem item = new clsItem();
            while (r.Read())
            {
                item.ItemID = Convert.ToInt32(r[0].ToString());
                item.ItemName = r[1].ToString();
                item.UrduName = r[2].ToString();
                item.CostPrice = Convert.ToDecimal(r[3].ToString());
                item.SalePrice = Convert.ToDecimal(r[4].ToString());
                item.UnitName = r[5].ToString();
                item.CategoryName = r[6].ToString();
                item.BrandName = r[7].ToString();
            }
            r.Close();
            con.Close();
            SqlConnection.ClearPool(con);
            var data = item;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getItemStock(string invoiceNo,int itemId, int warehouseId, string code)
        {
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            db.sp_GetItemStock(invoiceNo,itemId, code, warehouseId, 0, User.FiscalYearId, CheckReturn);
            decimal stock = Convert.ToDecimal(CheckReturn.Value);
            return Json(new { stock = stock }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getMaxCodeForStoreIn()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "IS-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Store In").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Store In").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
                maxCode = maxCode.Split('-')[1];
                maxCode = maxCode.Substring(4, 4);
                int codeInIntegar = Convert.ToInt32(maxCode);
                codeInIntegar += 1;
                if (codeInIntegar.ToString().Length == 1)
                {
                    maxCode = invoiceCode + "000" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 2)
                {
                    maxCode = invoiceCode + "00" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 3)
                {
                    maxCode = invoiceCode + "0" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 4)
                {
                    maxCode = invoiceCode + codeInIntegar.ToString();
                }
            }
            else
            {
                maxCode = invoiceCode + "0001";
            }

            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Store In Report
        public ActionResult StoreInReport(string id)
        {
            if (User != null)
            {
                List<clsInvoice> invoice = new List<clsInvoice>();
                invoice = (from f in db.Invoice_Main
                           join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                           where f.ComputerNo == id
                           select new clsInvoice
                           {
                               ComputerNo = f.ComputerNo,
                               SerialNo = fd.SerialNo,
                               RecordDate = f.RecordDate,
                               RefrenceNo = f.RefrenceNo,
                               CreditDays = f.CreditDays,
                               DiscountType = f.DiscountType ?? "",
                               Discount = f.Discount,
                               NetDiscount = f.NetDiscount,
                               Overhead = f.Overhead,
                               Reason = f.Reason ?? "",
                               Freight = f.Freight,
                               Remarks = f.Remarks,
                               Received = f.Received,
                               ReceivedDetailCode = f.ReceivedDetailCode,
                               ReceivedAmount = f.ReceivedAmount,
                               ReceivedRemarks = f.ReceivedRemarks,
                               BalanceAmount = f.BalanceAmount ?? 0,
                               PreviousBalance = f.PreviousBalance ?? 0,
                               GrossTotal = f.GrossTotal,
                               NetAmount = f.NetAmount,
                               Status = f.Status,
                               DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                               Address = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Address).FirstOrDefault() ?? "",
                               Phone = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Phone1).FirstOrDefault() ?? "",
                               InvoiceType = f.InvoiceType,
                               CashCustomerName = f.CashCustomerName ?? "",
                               ShowBalance = f.ShowBalance,
                               ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                               ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                               Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                               BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                               BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                               BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                               //BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                               //SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,

                               //////////--------------Line Data-----------------/////////
                               ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                               //Barcode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.UrduName).FirstOrDefault() ?? "",
                               //BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                               WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                               Rate = fd.Rate,
                               Qty = fd.Qty,
                               ItemNote = fd.ItemNote,
                               LineDiscountType = fd.DiscountType ?? string.Empty,
                               LineDiscount = fd.Discount ?? 0,
                               Tax1Value = fd.Tax1Value,
                               Tax2Value = fd.Tax2Value,
                               LineNetRate = fd.NetRate,
                               LineGrossAmount = fd.GrossValue,
                               LineNetAmount = fd.NetValue
                           }).OrderBy(x => x.SerialNo).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        #region Store Out
        public ActionResult StoreOutIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "StoreOut"
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
                    string fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    string toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    ViewBag.FromDate = fromDate;
                    ViewBag.ToDate = toDate;
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        [HttpPost]
        public ActionResult InsertUpdateStoreOut(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsInvoice> invoice = JsonConvert.DeserializeObject<List<clsInvoice>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");

            string invoiceNo = invoice[0].ComputerNo;
            if (invoice[0].IsNewOrEdit == "Edit")
            {
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID,"" });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Store Out", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, invoice[0].Reason, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Store Out Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID ,""});
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Store Out", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, invoice[0].Reason, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Store Out Successfully Saved";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, invoiceNo = invoiceNo } };
        }

        public ActionResult GetStoreOutById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           RecordDate = f.RecordDate,
                           Reason = f.Reason,
                           RefrenceNo = f.RefrenceNo,
                           Remarks = f.Remarks,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           //ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           //Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           //BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           //BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           //BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           //BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           //SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetStoreOutDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                 // ItemNote = f.ItemNote,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue,
                                 SerialNo = f.SerialNo
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult StoreOutList()
        {
            return PartialView();
        }
        public ActionResult GetAllStoreOut()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var invoiceNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var reason = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var referenceNo = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Store Out"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                Reason = f.Reason,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                NetAmount = f.NetAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                FiscalYearID = f.FiscalYearID,
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(invoiceNo))
                {
                    invoice = invoice.Where(c => c.ComputerNo.ToLower().Contains(invoiceNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(reason))
                {
                    invoice = invoice.Where(c => c.Reason.ToLower().Contains(reason.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(referenceNo))
                {
                    invoice = invoice.Where(c => c.RefrenceNo.ToLower().Contains(referenceNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    invoice = invoice.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = invoice.Count();
                var data = invoice.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult getMaxCodeForStoreOut()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "OS-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Store Out").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Store Out").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
                maxCode = maxCode.Split('-')[1];
                maxCode = maxCode.Substring(4, 4);
                int codeInIntegar = Convert.ToInt32(maxCode);
                codeInIntegar += 1;
                if (codeInIntegar.ToString().Length == 1)
                {
                    maxCode = invoiceCode + "000" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 2)
                {
                    maxCode = invoiceCode + "00" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 3)
                {
                    maxCode = invoiceCode + "0" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 4)
                {
                    maxCode = invoiceCode + codeInIntegar.ToString();
                }
            }
            else
            {
                maxCode = invoiceCode + "0001";
            }

            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Store Out Report
        public ActionResult StoreOutReport(string id)
        {
            if (User != null)
            {
                List<clsInvoice> invoice = new List<clsInvoice>();
                invoice = (from f in db.Invoice_Main
                           join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                           where f.ComputerNo == id
                           select new clsInvoice
                           {
                               ComputerNo = f.ComputerNo,
                               SerialNo = fd.SerialNo,
                               RecordDate = f.RecordDate,
                               RefrenceNo = f.RefrenceNo,
                               CreditDays = f.CreditDays,
                               DiscountType = f.DiscountType ?? "",
                               Discount = f.Discount,
                               NetDiscount = f.NetDiscount,
                               Overhead = f.Overhead,
                               Reason = f.Reason ?? "",
                               Freight = f.Freight,
                               Remarks = f.Remarks,
                               Received = f.Received,
                               ReceivedDetailCode = f.ReceivedDetailCode,
                               ReceivedAmount = f.ReceivedAmount,
                               ReceivedRemarks = f.ReceivedRemarks,
                               BalanceAmount = f.BalanceAmount ?? 0,
                               PreviousBalance = f.PreviousBalance ?? 0,
                               GrossTotal = f.GrossTotal,
                               NetAmount = f.NetAmount,
                               Status = f.Status,
                               DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                               Address = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Address).FirstOrDefault() ?? "",
                               Phone = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Phone1).FirstOrDefault() ?? "",
                               InvoiceType = f.InvoiceType,
                               CashCustomerName = f.CashCustomerName ?? "",
                               ShowBalance = f.ShowBalance,
                               ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                               ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                               Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                               BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                               BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                               BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                               //BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                               //SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,

                               //////////--------------Line Data-----------------/////////
                               ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                               //Barcode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.UrduName).FirstOrDefault() ?? "",
                               //BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                               WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                               Rate = fd.Rate,
                               Qty = fd.Qty,
                               ItemNote = fd.ItemNote,
                               LineDiscountType = fd.DiscountType ?? string.Empty,
                               LineDiscount = fd.Discount ?? 0,
                               Tax1Value = fd.Tax1Value,
                               Tax2Value = fd.Tax2Value,
                               LineNetRate = fd.NetRate,
                               LineGrossAmount = fd.GrossValue,
                               LineNetAmount = fd.NetValue
                           }).OrderBy(x => x.SerialNo).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        #region Store to Store
        public ActionResult StoreToStoreIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "StoreToStore"
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
                    string fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    string toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault()).ToString("yyyy-MM-dd");
                    ViewBag.FromDate = fromDate;
                    ViewBag.ToDate = toDate;
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        [HttpPost]
        public ActionResult InsertUpdateStoreToStore(string data)
        {
            string message = "";
            bool status = false;
            string returnId = "0";
            List<clsInvoice> invoice = JsonConvert.DeserializeObject<List<clsInvoice>>(data);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("Qty");
            dt.Columns.Add("Rate");
            dt.Columns.Add("DiscountType");
            dt.Columns.Add("Discount");
            dt.Columns.Add("NetRate");
            dt.Columns.Add("GrossValue");
            dt.Columns.Add("Tax1ID");
            dt.Columns.Add("Tax1Rate");
            dt.Columns.Add("Tax1Value");
            dt.Columns.Add("Tax2ID");
            dt.Columns.Add("Tax2Rate");
            dt.Columns.Add("Tax2Value");
            dt.Columns.Add("NetValue");
            dt.Columns.Add("ItemNote");
            dt.Columns.Add("AvgRate");
            dt.Columns.Add("WarehouseID");
            dt.Columns.Add("RefCompositKey");

            string invoiceNo = invoice[0].ComputerNo;
            if (invoice[0].IsNewOrEdit == "Edit")
            {
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID,"" });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Store To Store", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, invoice[0].Reason, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Store Successfully Updated";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, "" });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Store To Store", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, invoice[0].Reason, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Store Successfully Saved";
                }
                else
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, invoiceNo = invoiceNo } };
        }

        public ActionResult GetStoreToStoreById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           RecordDate = f.RecordDate,
                           Reason = f.Reason,
                           RefrenceNo = f.RefrenceNo,
                           Remarks = f.Remarks,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           //ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           //Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           //BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           //BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           //BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           //BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           //SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetStoreToStoreDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id && f.SerialNo < 10000
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                 // ItemNote = f.ItemNote,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue,
                                 SerialNo = f.SerialNo
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult StoreToStoreList()
        {
            return PartialView();
        }
        public ActionResult GetAllStoreToStore()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var invoiceNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var toWarehouse = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var referenceNo = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Store To Store"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                DetailName = db.Admin_Setup_Warehouse.Where(x=>x.WarehouseID == f.DetailCode).Select(x=>x.WarehouseName).FirstOrDefault(),
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                NetAmount = f.NetAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                FiscalYearID = f.FiscalYearID,
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(invoiceNo))
                {
                    invoice = invoice.Where(c => c.ComputerNo.ToLower().Contains(invoiceNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(toWarehouse))
                {
                    invoice = invoice.Where(c => c.DetailName.ToLower().Contains(toWarehouse.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(referenceNo))
                {
                    invoice = invoice.Where(c => c.RefrenceNo.ToLower().Contains(referenceNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    invoice = invoice.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        invoice = invoice.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = invoice.Count();
                var data = invoice.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult getMaxCodeForStoreToStore()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "SS-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Store To Store").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Store To Store").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
                maxCode = maxCode.Split('-')[1];
                maxCode = maxCode.Substring(4, 4);
                int codeInIntegar = Convert.ToInt32(maxCode);
                codeInIntegar += 1;
                if (codeInIntegar.ToString().Length == 1)
                {
                    maxCode = invoiceCode + "000" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 2)
                {
                    maxCode = invoiceCode + "00" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 3)
                {
                    maxCode = invoiceCode + "0" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 4)
                {
                    maxCode = invoiceCode + codeInIntegar.ToString();
                }
            }
            else
            {
                maxCode = invoiceCode + "0001";
            }

            return Json(maxCode, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Store to Store Report
        public ActionResult StoreToStoreReport(string id)
        {
            if (User != null)
            {
                List<clsInvoice> invoice = new List<clsInvoice>();
                invoice = (from f in db.Invoice_Main
                           join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                           where f.ComputerNo == id && fd.SerialNo < 10000
                           select new clsInvoice
                           {
                               ComputerNo = f.ComputerNo,
                               SerialNo = fd.SerialNo,
                               RecordDate = f.RecordDate,
                               RefrenceNo = f.RefrenceNo,
                               CreditDays = f.CreditDays,
                               DiscountType = f.DiscountType ?? "",
                               Discount = f.Discount,
                               NetDiscount = f.NetDiscount,
                               Overhead = f.Overhead,
                               Freight = f.Freight,
                               Remarks = f.Remarks,
                               Received = f.Received,
                               ReceivedDetailCode = f.ReceivedDetailCode,
                               ReceivedAmount = f.ReceivedAmount,
                               ReceivedRemarks = f.ReceivedRemarks,
                               BalanceAmount = f.BalanceAmount ?? 0,
                               PreviousBalance = f.PreviousBalance ?? 0,
                               GrossTotal = f.GrossTotal,
                               NetAmount = f.NetAmount,
                               Reason = f.Reason ?? "",
                               Status = f.Status,
                               DetailName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.DetailCode).Select(x => x.WarehouseName).FirstOrDefault(),
                               Address = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Address).FirstOrDefault() ?? "",
                               Phone = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Phone1).FirstOrDefault() ?? "",
                               InvoiceType = f.InvoiceType,
                               CashCustomerName = f.CashCustomerName ?? "",
                               ShowBalance = f.ShowBalance,
                               ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                               ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                               Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                               BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                               BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                               BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                               //BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                               //SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,

                               //////////--------------Line Data-----------------/////////
                               ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                               //Barcode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.UrduName).FirstOrDefault() ?? "",
                               //BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                               WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                               Rate = fd.Rate,
                               Qty = fd.Qty,
                               ItemNote = fd.ItemNote,
                               LineDiscountType = fd.DiscountType ?? string.Empty,
                               LineDiscount = fd.Discount ?? 0,
                               Tax1Value = fd.Tax1Value,
                               Tax2Value = fd.Tax2Value,
                               LineNetRate = fd.NetRate,
                               LineGrossAmount = fd.GrossValue,
                               LineNetAmount = fd.NetValue
                           }).OrderBy(x => x.SerialNo).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
	}
}