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
using System.IO;
using System.Net;

namespace POS.Controllers
{
    public class SaleController : BaseController
    {
        PosMBDb db = new PosMBDb();

        #region sub customer pick
        IQueryable<clsACDetailSub> AllACSubDetailList;


        public JsonResult GetOptionACSubDetailList(string searchTerm, int pageSize, int pageNumber, decimal? detailCode = 0)
        {
            AllACSubDetailList = AllItemsListDetail(detailCode);
            var select2pagedResult = new Select2PagedSubDetailResult();
            var totalResults = 0;
            select2pagedResult.Results = GetPagedListOptions(searchTerm, pageSize, pageNumber, out totalResults);
            select2pagedResult.Total = totalResults;

            var result = select2pagedResult;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public IQueryable<clsACDetailSub> AllItemsListDetail(decimal? detailCode)
        {
            List<clsACDetailSub> item = new List<clsACDetailSub>();
            item = (from c in db.AC_Detail_Sub 
                    where c.DetailCode == detailCode && c.Status=="Active"
                    select new clsACDetailSub
                    {
                        id = c.SubDetailCode,
                        text = c.Name
                    }).Distinct().ToList();

            var result = item.AsQueryable();
            return result;
        }


        List<clsACDetailSub> GetPagedListOptions(string searchTerm, int pageSize, int pageNumber, out int totalSearchRecords)
        {
            var allSearchedResults = GetAllSearchResults(searchTerm);
            totalSearchRecords = allSearchedResults.Count;
            return allSearchedResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        List<clsACDetailSub> GetAllSearchResults(string searchTerm)
        {
            var resultList = new List<clsACDetailSub>();

            if (!string.IsNullOrEmpty(searchTerm))
                resultList = AllACSubDetailList.Where(n => n.text.ToLower().StartsWith(searchTerm.ToLower())).ToList();
            else
                resultList = AllACSubDetailList.ToList();
            return resultList;
        }
        #endregion

        #region Misc
        private string AddEditInvoice(string invoiceNo, string type, DateTime? recordDate, decimal? detailCode,decimal? SubDetailCode,
                                   string referenceNo, string remarks, decimal? grossTotal, string discountType,
                                   decimal? discount, decimal? netDiscount, decimal? overHead, decimal? freight,
                                   decimal? netAmount, int? received, decimal? receivedDetailCode, decimal? receivedAmount, string receivedRemarks,
                                   decimal? balanceAmount, string status, int businessId, int? userId, int? lastUserId, DateTime? saveDate, DateTime? lastDate,
                                   decimal? previousBalance, int? creditDays, int fiscalYear, int extraOrNot, string transporter, string biltyNo, string packing,
                                   DateTime? biltyDate, int? biltyPending, int? salemanId, string reason, int? invoiceType, string cashCustomerName, int? showBalance, DataTable dt, string insertUpdateStatus)
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
                        cmd.Parameters.Add("@SubDetailCode", SqlDbType.Decimal).Value = SubDetailCode;
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
                        cmd.Parameters.Add("@InvoiceType", SqlDbType.Int).Value = invoiceType;
                        cmd.Parameters.Add("@CashCustomerName", SqlDbType.NVarChar).Value = cashCustomerName;
                        cmd.Parameters.Add("@ShowBalance", SqlDbType.Int).Value = showBalance;
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



        public ActionResult GetAllStoreNoteForInvoice(string type)
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var orderNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var referenceNo = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_StoreNoteForInvoice", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Type", SqlDbType.VarChar).Value = type;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);

                r.Close();
                con.Close();
                DataTableReader dt = tdt.CreateDataReader();
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        ComputerNo = dt["ComputerNo"].ToString(),
                        RecordDate = Convert.ToDateTime(dt["RecordDate"]),
                        RefrenceNo = dt["RefrenceNo"].ToString(),
                        Remarks = dt["Remarks"].ToString(),
                        NetAmount = Convert.ToDecimal(dt["NetAmount"]),
                        DetailName = dt["DetailName"].ToString(),
                        SubDetailName = dt["SubDetailName"].ToString()
                    });
                }
                SqlConnection.ClearPool(con);
                //SEARCHING...
                if (!string.IsNullOrEmpty(orderNo))
                {
                    invoice = invoice.Where(c => c.ComputerNo.ToLower().Contains(orderNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(referenceNo))
                {
                    invoice = invoice.Where(c => c.RefrenceNo.ToLower().Contains(referenceNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(detailName))
                {
                    invoice = invoice.Where(c => c.DetailName.ToLower().Contains(detailName.ToLower())).ToList();
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
        public ActionResult GetAllStoreNoteForInvoiceDetail(string id)
        {
            if (User != null)
            {
                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("spGet_StoreNoteForInvoiceDetail", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar).Value = id;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);
                r.Close();
                con.Close();
                DataTableReader dt = tdt.CreateDataReader();
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        DetailCode = Convert.ToDecimal(dt["DetailCode"]),
                        CompositKey = dt["CompositKey"].ToString(),
                        DetailName = dt["DetailName"].ToString(),

                        SubDetailCode = Convert.ToDecimal(dt["SubDetailCode"]),
                        SubDetailName = dt["SubDetailName"].ToString(),

                        RecordDate = Convert.ToDateTime(dt["RecordDate"]),
                        RefrenceNo = dt["RefrenceNo"].ToString(),
                        ComputerNo = id,
                        InvoiceType = Convert.ToInt32(dt["InvoiceType"]),
                        ItemID = Convert.ToInt32(dt["ItemID"]),
                        ItemName = dt["ItemName"].ToString(),
                        ItemDescription = dt["ItemDescription"].ToString(),
                        WarehouseID = Convert.ToInt32(dt["WarehouseID"]),
                        WarehouseName = dt["WarehouseName"].ToString(),
                        Rate = Convert.ToDecimal(dt["Rate"]),
                        Qty = Convert.ToDecimal(dt["Qty"]),
                        LineDiscountType = dt["DiscountType"].ToString(),
                        LineDiscount = Convert.ToDecimal(dt["Discount"]),
                        Tax1ID = Convert.ToInt32(dt["Tax1ID"]),
                        Tax1Name = dt["TaxName"].ToString(),
                        Tax1Rate = Convert.ToDecimal(dt["Tax1Rate"]),
                        Tax1Value = Convert.ToDecimal(dt["Tax1Value"]),
                        LineNetRate = Convert.ToDecimal(dt["NetRate"]),
                        LineGrossAmount = Convert.ToDecimal(dt["GrossValue"]),
                        LineNetAmount = Convert.ToDecimal(dt["NetValue"]),
                    });
                }
                SqlConnection.ClearPool(con);
                //SEARCHING...
                var data = invoice.ToList();
                return Json(new { data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult SetSessionsForSaleList(bool? customerCheckbox, decimal? customerId, bool? dateCheckbox, DateTime? fromDate,
                                        DateTime? toDate, bool? cashCustomerCheckbox, string cashCustomerName,
                                        bool? cashCreditTypeCheckbox, string cashCreditType, bool? computerNoCheckbox, string computerNo)
        {
            bool status = true;
            if (customerCheckbox == true && customerId != null)
            {
                Session["CustomerId"] = customerId;
            }
            else
            {
                Session["CustomerId"] = null;
            }

            if (dateCheckbox == true && fromDate != null && toDate != null)
            {
                Session["FromDate"] = fromDate;
                Session["ToDate"] = toDate;
            }
            else
            {
                Session["FromDate"] = null;
                Session["ToDate"] = null;
            }
            if (cashCustomerCheckbox == true && !(string.IsNullOrEmpty(cashCustomerName)))
            {
                Session["CashCustomerName"] = cashCustomerName;
            }
            else
            {
                Session["CashCustomerName"] = null;
            }
            //////////////////
            if (cashCreditTypeCheckbox == true && !(string.IsNullOrEmpty(cashCreditType)))
            {
                Session["CashCreditType"] = cashCreditType;
            }
            else
            {
                Session["CashCreditType"] = null;
            }
            if (computerNoCheckbox == true && !(string.IsNullOrEmpty(computerNo)))
            {
                Session["ComputerNo"] = computerNo;
            }
            else
            {
                Session["ComputerNo"] = null;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }



        public ActionResult ItemStockListWarehouseBase(int itemId)
        {
            List<clsItem> list = new List<clsItem>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("spGet_ItemStockWarehouseBase", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsItem()
                {
                    WarehouseName = dt[0].ToString(),
                    Stock = Convert.ToDecimal(dt["Stock"]),
                });
            }
            return PartialView(list);
        }
        public List<clsAccountDetail> getDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && (c.Type == "Customer" || c.Type == "Customer + Supplier")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).OrderBy(x => x.Name).ToList();
            return detail;
        }
        public List<clsAccountDetail> getReceivedDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      join ac in db.AC_DefaultGL on c.AccountCode equals ac.AccountCode
                      where c.Status == "Active" && (ac.Type == "Cash" || ac.Type == "Bank")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name
                      }).OrderBy(x => x.Name).ToList();
            return detail;
        }
        public List<clsVoucher> getTaxes()
        {
            List<clsVoucher> tax = new List<clsVoucher>();
            tax = (from c in db.Admin_Setup_Tax
                   where c.Status == "Active"
                   select new clsVoucher
                   {
                       TaxID = c.TaxID,
                       TaxName = c.TaxName
                   }).ToList();
            return tax;
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
        public List<clsFooter> getFooters()
        {
            List<clsFooter> footer = new List<clsFooter>();
            footer = (from c in db.Footers
                      select new clsFooter
                      {
                          FooterId = c.FooterId,
                          FooterName = c.FooterName,
                          Status = c.Status ?? "In-Active"
                      }).OrderBy(x => x.Status).ToList();
            return footer;
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
        public List<clsSalesman> getSalesman()
        {
            List<clsSalesman> salesman = new List<clsSalesman>();
            salesman = (from c in db.Admin_Setup_Salesman
                        where c.Status == "Active"
                        select new clsSalesman
                        {
                            SalesmanID = c.SalesmanID,
                            FullName = c.FullName
                        }).ToList();
            return salesman;
            //return Json(new { data = data, data2 = data2 }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getCreditDaysAndPreviousBalance(string voucherNo, decimal detailCode, string code)
        {
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn2 = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn2", typeof(decimal));
            db.sp_GetCreditDaysAndPreviousBalance(voucherNo, detailCode, DateTime.Now, code, 0, User.FiscalYearId, CheckReturn, CheckReturn2);
            decimal creditDays = Convert.ToDecimal(CheckReturn.Value);
            decimal previousBalance = Convert.ToDecimal(CheckReturn2.Value);
            return Json(new { creditDays = creditDays, previousBalance = previousBalance }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getItemInformation(int itemId, decimal? detailCode, string cashCustomerName)
        {
            if (detailCode == null)
            {
                detailCode = 0;
            }
            if (cashCustomerName == null)
            {
                cashCustomerName = "";
            }
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("spGet_ItemInformation", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
            cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = detailCode;
            cmd.Parameters.Add("@CashName", SqlDbType.VarChar, 200).Value = cashCustomerName;
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
                item.SaleRate = Convert.ToDecimal(r[8].ToString());
                item.PurchaseRate = Convert.ToDecimal(r[9].ToString());
                item.AverageRate = Convert.ToDecimal(r[10].ToString());
                item.SaleDate = Convert.ToDateTime(r[11]);
                item.SubCategoryName = r[12].ToString();
                item.MinSalePrice = Convert.ToDecimal(r[13].ToString());
                item.LastDiscType = r["LastDiscType"].ToString();
                item.LastDisc = Convert.ToDecimal(r["LastDisc"].ToString());                

            }

            var data = item;
            r.Close();
            cmd = new SqlCommand();
            cmd = new SqlCommand("sp_GetItemSaleRate", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
            cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = detailCode;
            r = cmd.ExecuteReader();
            decimal saleRate = 0;
            while (r.Read())
            {
                saleRate = Convert.ToDecimal(r[0]);
            }
            r.Close();
            con.Close();
            SqlConnection.ClearPool(con);
            //System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            //db.sp_GetItemSaleRate(itemId, detailCode, CheckReturn);
            //decimal saleRate = Convert.ToDecimal(CheckReturn.Value);
            return Json(new { data = data, saleRate = saleRate }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getItemStock(string invoiceNo, int itemId, int warehouseId, string code)
        {
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            db.sp_GetItemStock(invoiceNo, itemId, code, warehouseId, 0, User.FiscalYearId, CheckReturn);
            decimal stock = Convert.ToDecimal(CheckReturn.Value);
            return Json(new { stock = stock }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SelectItem()
        {
            return PartialView();
        }
        public ActionResult GetAllItemsList()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var itemName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var urduName = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var category = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var brand = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
               
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsItem> items = (from f in db.Item_Information
                                       where f.Status == "Active"
                                       select new clsItem
                                       {
                                           ItemID = f.ItemID,
                                           ItemName = f.ItemName,
                                           UrduName = f.UrduName ?? "",
                                           Barcode = f.Barcode,
                                           UnitName = db.Item_UOM.Where(x => x.UomId == f.UomId).Select(x => x.Description).FirstOrDefault(),
                                           //CategoryID = f.CategoryID,
                                           CategoryName = db.Item_Category.Where(x => x.CategoryID == f.CategoryID).Select(x => x.Description).FirstOrDefault(),
                                           //BrandID = f.BrandID,
                                           BrandName = db.Item_Brand.Where(x => x.BrandID == f.BrandID).Select(x => x.Description).FirstOrDefault(),
                                       }).OrderBy(x => x.ItemName).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(itemName))
                {
                    items = items.Where(c => c.ItemName.ToLower().Trim().Contains(itemName.ToLower().Trim())).ToList();
                }
                else if (!string.IsNullOrEmpty(urduName))
                {
                    items = items.Where(c => c.UrduName.Contains(urduName)).ToList();
                }
                else if (!string.IsNullOrEmpty(category))
                {
                    items = items.Where(c => c.CategoryName.ToLower().Contains(category.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(brand))
                {
                    items = items.Where(c => c.BrandName.ToLower().Contains(brand.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        items = items.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = items.Count();
                var data = items.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion

        #region Delivery Challan
        public ActionResult DeliveryChallanIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "DeliveryChallan"
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
                    ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                    ViewBag.ReceivedDetailCode = new SelectList(getReceivedDetailCodes(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult getMaxCodeForDeliveryChallan()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "DC-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Delivery Challan").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Delivery Challan").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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


        [HttpPost]
        public ActionResult InsertUpdateDeliveryChallan(string data)
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
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, "" });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Delivery Challan", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, 0, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Record Successfully Updated";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
                    }
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
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Delivery Challan", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, 0, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Record Successfully Saved";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
                    }
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



        public ActionResult DeleteDeliveryChallan(string id)
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
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0 ,""});
            string returnId = AddEditInvoice(id, "Delivery Challan", DateTime.Now, 0,0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Delete");
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Record Successfully Deleted";
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


        public ActionResult DeliveryChallanList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }

        public ActionResult GetAllDeliveryChallan()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                decimal? customerId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string cashCustomerName = "";
                string cashCreditType = "";
                string computerNo = "";
                int invoiceType = 0;
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                }
                if (Session["CashCustomerName"] != null)
                {
                    cashCustomerName = Session["CashCustomerName"].ToString();
                }
                if (Session["CashCreditType"] != null)
                {
                    cashCreditType = Session["CashCreditType"].ToString();
                    if (cashCreditType == "Credit")
                    {
                        invoiceType = 1;
                    }
                    else
                    {
                        invoiceType = 0;
                    }
                }
                if (Session["ComputerNo"] != null)
                {
                    computerNo = Session["ComputerNo"].ToString();
                }
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Delivery Challan"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                SubDetailCode = f.SubDetailCode,
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                DiscountType = f.DiscountType,
                                                Discount = f.Discount,
                                                NetDiscount = f.NetDiscount,
                                                Overhead = f.Overhead,
                                                Freight = f.Freight,
                                                NetAmount = f.NetAmount,
                                                Received = f.Received,
                                                ReceivedDetailCode = f.ReceivedDetailCode,
                                                ReceivedAmount = f.ReceivedAmount,
                                                ReceivedRemarks = f.ReceivedRemarks,
                                                BalanceAmount = f.BalanceAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                PreviousBalance = f.PreviousBalance,
                                                CreditDays = f.CreditDays,
                                                FiscalYearID = f.FiscalYearID,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                                InvoiceTypeName = (f.InvoiceType == 1) ? "Credit" : "Cash",
                                                DetailName = (f.InvoiceType == 1) ? db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() : f.CashCustomerName,
                                                SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (customerId != 0)
                {
                    invoice = invoice.Where(x => x.DetailCode == customerId).ToList();
                }
                if (!(string.IsNullOrEmpty(cashCustomerName)))
                {
                    invoice = invoice.Where(x => x.CashCustomerName.ToLower().Contains(cashCustomerName.ToLower())).ToList();
                }
                if (!(string.IsNullOrEmpty(cashCreditType)))
                {
                    invoice = invoice.Where(x => x.InvoiceType == invoiceType).ToList();
                }
                if (!(string.IsNullOrEmpty(computerNo)))
                {
                    invoice = invoice.Where(x => x.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                if (fromDate != null && toDate != null)
                {
                    invoice = invoice.Where(x => x.RecordDate >= fromDate && x.RecordDate <= toDate).ToList();
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




        public ActionResult GetDeliveryChallanById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           SubDetailCode = f.SubDetailCode,
                           RecordDate = f.RecordDate,
                           RefrenceNo = f.RefrenceNo,
                           CreditDays = f.CreditDays,
                           DiscountType = f.DiscountType,
                           Discount = f.Discount,
                           NetDiscount = f.NetDiscount,
                           Overhead = f.Overhead,
                           Freight = f.Freight,
                           Remarks = f.Remarks,
                           Received = f.Received,
                           ReceivedDetailCode = f.ReceivedDetailCode,
                           ReceivedAmount = f.ReceivedAmount,
                           ReceivedRemarks = f.ReceivedRemarks,
                           BalanceAmount = f.BalanceAmount,
                           PreviousBalance = f.PreviousBalance,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           CashCustomerName = f.CashCustomerName,
                           InvoiceType = f.InvoiceType,
                           ShowBalance = f.ShowBalance,
                           ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                           SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDeliveryChallanDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 SerialNo = f.SerialNo,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                 ItemNote = f.ItemNote,
                                 LineDiscountType = f.DiscountType ?? string.Empty,
                                 LineDiscount = f.Discount ?? 0,
                                 Tax1ID = f.Tax1ID,
                                 Tax1Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax1ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax1Rate = f.Tax1Rate,
                                 Tax1Value = f.Tax1Value,
                                 Tax2ID = f.Tax2ID,
                                 Tax2Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax2ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax2Rate = f.Tax2Rate,
                                 Tax2Value = f.Tax2Value,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Delivery Challan Report
        public ActionResult DeliveryChallanReport(string id)
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
                           }).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion

        #region Sale Invoice
        public ActionResult SaleInvoiceIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SaleInvoice"
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
                    ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                    ViewBag.ReceivedDetailCode = new SelectList(getReceivedDetailCodes(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public decimal getItemInformationMinRate(int itemId)
        {

            decimal vminrate = 0;
            
            decimal detailCode = 0;
            string cashCustomerName = "";

            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("spGet_ItemInformation", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
            cmd.Parameters.Add("@DetailCode", SqlDbType.Decimal).Value = detailCode;
            cmd.Parameters.Add("@CashName", SqlDbType.VarChar, 200).Value = cashCustomerName;
            SqlDataReader r = cmd.ExecuteReader();
            clsItem item = new clsItem();
            while (r.Read())
            {
                vminrate = Convert.ToDecimal(r[13].ToString());
            }

            var data = item;
            r.Close();
            con.Close();
            SqlConnection.ClearPool(con);

            return vminrate;
        }


        [HttpPost]
        public ActionResult InsertUpdateSaleInvoice(string data)
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

            int vminratecheck = 0;
            for (int i = 0; i < invoice.Count; i++)
            {
                decimal vrate = invoice[i].Rate ?? 0;
                if (getItemInformationMinRate(Convert.ToInt32(invoice[i].ItemID)) > vrate)
                {
                    vminratecheck = vminratecheck + 1;
                    
                }
            }

            if (vminratecheck > 0)
            {
                ModelState.Clear();
                status = false;
                message = "You cannot Sale Below Min Rate";
            }
            else
            {
                if (invoice[0].IsNewOrEdit == "Edit")
                {
                    for (int i = 0; i < invoice.Count; i++)
                    {
                        dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, invoice[i].RefCompositKey });
                    }
                    returnId = AddEditInvoice(invoice[0].ComputerNo, "Sale", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, dt, "Update");
                    if (returnId == invoiceNo)
                    {
                        ModelState.Clear();
                        status = true;
                        message = "Invoice Successfully Updated";
                        string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                        if (sendSms == "Y")
                        {
                            SmsController sms = new SmsController();
                            sms.SendSms(invoiceNo);
                        }
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
                        dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, invoice[i].RefCompositKey });
                    }
                    returnId = AddEditInvoice(invoice[0].ComputerNo, "Sale", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, dt, "Save");
                    invoiceNo = returnId;
                    if (returnId != "0")
                    {
                        ModelState.Clear();
                        status = true;
                        message = "Invoice Successfully Saved";
                        string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                        if (sendSms == "Y")
                        {
                            SmsController sms = new SmsController();
                            sms.SendSms(invoiceNo);
                        }
                    }
                    else
                    {
                        ModelState.Clear();
                        status = false;
                        message = "Error! Please try again.";
                    }
                }
            }
            return new JsonResult { Data = new { status = status, message = message, invoiceNo = invoiceNo } };
        }


        public ActionResult GetSaleInvoiceById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           SubDetailCode = f.SubDetailCode,
                           SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                           RecordDate = f.RecordDate,
                           RefrenceNo = f.RefrenceNo,
                           CreditDays = f.CreditDays,
                           DiscountType = f.DiscountType,
                           Discount = f.Discount,
                           NetDiscount = f.NetDiscount,
                           Overhead = f.Overhead,
                           Freight = f.Freight,
                           Remarks = f.Remarks,
                           Received = f.Received,
                           ReceivedDetailCode = f.ReceivedDetailCode,
                           ReceivedAmount = f.ReceivedAmount,
                           ReceivedRemarks = f.ReceivedRemarks,
                           BalanceAmount = f.BalanceAmount,
                           PreviousBalance = f.PreviousBalance,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           CashCustomerName = f.CashCustomerName,
                           InvoiceType = f.InvoiceType,
                           ShowBalance = f.ShowBalance,
                           DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                           ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSaleInvoiceDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 SerialNo = f.SerialNo,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                 ItemNote = f.ItemNote,
                                 LineDiscountType = f.DiscountType ?? string.Empty,
                                 LineDiscount = f.Discount ?? 0,
                                 Tax1ID = f.Tax1ID,
                                 Tax1Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax1ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax1Rate = f.Tax1Rate,
                                 Tax1Value = f.Tax1Value,
                                 Tax2ID = f.Tax2ID,
                                 Tax2Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax2ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax2Rate = f.Tax2Rate,
                                 Tax2Value = f.Tax2Value,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue,
                                 CompositKey = f.RefCompositKey ?? "0",
                                 HiddenComputerNo = db.Invoice_Detail.Where(x => x.CompositKey == f.RefCompositKey).Select(x => x.ComputerNo).FirstOrDefault() ?? "0"
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaleInvoiceList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllSaleInvoices()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                decimal? customerId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string computerNo = "";
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                }
                if (Session["ComputerNo"] != null)
                {
                    computerNo = Session["ComputerNo"].ToString();
                }
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Sale"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                SubDetailCode = f.SubDetailCode,
                                                SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                DiscountType = f.DiscountType,
                                                Discount = f.Discount,
                                                NetDiscount = f.NetDiscount,
                                                Overhead = f.Overhead,
                                                Freight = f.Freight,
                                                NetAmount = f.NetAmount,
                                                Received = f.Received,
                                                ReceivedDetailCode = f.ReceivedDetailCode,
                                                ReceivedAmount = f.ReceivedAmount,
                                                ReceivedRemarks = f.ReceivedRemarks,
                                                BalanceAmount = f.BalanceAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                PreviousBalance = f.PreviousBalance,
                                                CreditDays = f.CreditDays,
                                                FiscalYearID = f.FiscalYearID,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                                InvoiceTypeName = (f.InvoiceType == 1) ? "Credit" : "Cash",
                                                DetailName = (f.InvoiceType == 1) ? db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() : f.CashCustomerName,
                                                ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (customerId != 0)
                {
                    invoice = invoice.Where(x => x.DetailCode == customerId).ToList();
                }
                if (!(string.IsNullOrEmpty(computerNo)))
                {
                    invoice = invoice.Where(x => x.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                if (fromDate != null && toDate != null)
                {
                    invoice = invoice.Where(x => x.RecordDate >= fromDate && x.RecordDate <= toDate).ToList();
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
        public ActionResult DeleteSaleInvoice(string id)
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
            string returnId = AddEditInvoice(id, "Sale", DateTime.Now, 0,0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Delete");
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

        public ActionResult getMaxCodeForSaleInvoice()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "SI-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Sale").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Sale").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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
















        public ActionResult SaleInvoiceHoldList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllSaleHoldInvoices()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                decimal? customerId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string cashCustomerName = "";
                string cashCreditType = "";
                string computerNo = "";
                int invoiceType = 0;
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                }
                if (Session["CashCustomerName"] != null)
                {
                    cashCustomerName = Session["CashCustomerName"].ToString();
                }
                if (Session["CashCreditType"] != null)
                {
                    cashCreditType = Session["CashCreditType"].ToString();
                    if (cashCreditType == "Credit")
                    {
                        invoiceType = 1;
                    }
                    else
                    {
                        invoiceType = 0;
                    }
                }
                if (Session["ComputerNo"] != null)
                {
                    computerNo = Session["ComputerNo"].ToString();
                }
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Sale" && f.Status == "Hold"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                DiscountType = f.DiscountType,
                                                Discount = f.Discount,
                                                NetDiscount = f.NetDiscount,
                                                Overhead = f.Overhead,
                                                Freight = f.Freight,
                                                NetAmount = f.NetAmount,
                                                Received = f.Received,
                                                ReceivedDetailCode = f.ReceivedDetailCode,
                                                ReceivedAmount = f.ReceivedAmount,
                                                ReceivedRemarks = f.ReceivedRemarks,
                                                BalanceAmount = f.BalanceAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                PreviousBalance = f.PreviousBalance,
                                                CreditDays = f.CreditDays,
                                                FiscalYearID = f.FiscalYearID,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                                DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                                ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (customerId != 0)
                {
                    invoice = invoice.Where(x => x.DetailCode == customerId).ToList();
                }
                if (!(string.IsNullOrEmpty(cashCustomerName)))
                {
                    invoice = invoice.Where(x => x.CashCustomerName.ToLower().Contains(cashCustomerName.ToLower())).ToList();
                }
                if (!(string.IsNullOrEmpty(cashCreditType)))
                {
                    invoice = invoice.Where(x => x.InvoiceType == invoiceType).ToList();
                }
                if (!(string.IsNullOrEmpty(computerNo)))
                {
                    invoice = invoice.Where(x => x.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                if (fromDate != null && toDate != null)
                {
                    invoice = invoice.Where(x => x.RecordDate >= fromDate && x.RecordDate <= toDate).ToList();
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
        #endregion
        #region Sale Invoice Report
        public ActionResult SaleInvoiceReport(string id)
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
                               SerialNo =fd.SerialNo,
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
                               Status = f.Status,
                               SubDetailCode = f.SubDetailCode,
                               SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
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

        #region Sale Estimate
        public ActionResult SaleEstimateIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SaleEstimate"
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
                    ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                    ViewBag.ReceivedDetailCode = new SelectList(getReceivedDetailCodes(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        [HttpPost]
        public ActionResult InsertUpdateSaleEstimate(string data)
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

            int vminratecheck = 0;
            for (int i = 0; i < invoice.Count; i++)
            {
                decimal vrate = invoice[i].Rate ?? 0;
                if (getItemInformationMinRate(Convert.ToInt32(invoice[i].ItemID)) > vrate)
                {
                    vminratecheck = vminratecheck + 1;

                }
            }

            if (vminratecheck > 0)
            {
                ModelState.Clear();
                status = false;
                message = "You cannot Sale Below Min Rate";
            }
            else
            {
                if (invoice[0].IsNewOrEdit == "Edit")
                {
                    for (int i = 0; i < invoice.Count; i++)
                    {
                        dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, invoice[i].RefCompositKey });
                    }
                    returnId = AddEditInvoice(invoice[0].ComputerNo, "Sale Estimate", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, dt, "Update");
                    if (returnId == invoiceNo)
                    {
                        ModelState.Clear();
                        status = true;
                        message = "Invoice Successfully Updated";
                        string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                        if (sendSms == "Y")
                        {
                            SmsController sms = new SmsController();
                            sms.SendSms(invoiceNo);
                        }
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
                        dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, invoice[i].RefCompositKey });
                    }
                    returnId = AddEditInvoice(invoice[0].ComputerNo, "Sale Estimate", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, dt, "Save");
                    invoiceNo = returnId;
                    if (returnId != "0")
                    {
                        ModelState.Clear();
                        status = true;
                        message = "Invoice Successfully Saved";
                        string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                        if (sendSms == "Y")
                        {
                            SmsController sms = new SmsController();
                            sms.SendSms(invoiceNo);
                        }
                    }
                    else
                    {
                        ModelState.Clear();
                        status = false;
                        message = "Error! Please try again.";
                    }
                }
            }
            return new JsonResult { Data = new { status = status, message = message, invoiceNo = invoiceNo } };
        }


        public ActionResult GetSaleEstimateById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           SubDetailCode = f.SubDetailCode,
                           SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                           RecordDate = f.RecordDate,
                           RefrenceNo = f.RefrenceNo,
                           CreditDays = f.CreditDays,
                           DiscountType = f.DiscountType,
                           Discount = f.Discount,
                           NetDiscount = f.NetDiscount,
                           Overhead = f.Overhead,
                           Freight = f.Freight,
                           Remarks = f.Remarks,
                           Received = f.Received,
                           ReceivedDetailCode = f.ReceivedDetailCode,
                           ReceivedAmount = f.ReceivedAmount,
                           ReceivedRemarks = f.ReceivedRemarks,
                           BalanceAmount = f.BalanceAmount,
                           PreviousBalance = f.PreviousBalance,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           CashCustomerName = f.CashCustomerName,
                           InvoiceType = f.InvoiceType,
                           ShowBalance = f.ShowBalance,
                           DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                           ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSaleEstimateDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 SerialNo = f.SerialNo,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                 ItemNote = f.ItemNote,
                                 LineDiscountType = f.DiscountType ?? string.Empty,
                                 LineDiscount = f.Discount ?? 0,
                                 Tax1ID = f.Tax1ID,
                                 Tax1Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax1ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax1Rate = f.Tax1Rate,
                                 Tax1Value = f.Tax1Value,
                                 Tax2ID = f.Tax2ID,
                                 Tax2Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax2ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax2Rate = f.Tax2Rate,
                                 Tax2Value = f.Tax2Value,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue,
                                 CompositKey = f.RefCompositKey ?? "0",
                                 HiddenComputerNo = db.Invoice_Detail.Where(x => x.CompositKey == f.RefCompositKey).Select(x => x.ComputerNo).FirstOrDefault() ?? "0"
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaleEstimateList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllSaleEstimates()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                decimal? customerId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string computerNo = "";
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                }
                if (Session["ComputerNo"] != null)
                {
                    computerNo = Session["ComputerNo"].ToString();
                }
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Sale Estimate"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                SubDetailCode = f.SubDetailCode,
                                                SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                DiscountType = f.DiscountType,
                                                Discount = f.Discount,
                                                NetDiscount = f.NetDiscount,
                                                Overhead = f.Overhead,
                                                Freight = f.Freight,
                                                NetAmount = f.NetAmount,
                                                Received = f.Received,
                                                ReceivedDetailCode = f.ReceivedDetailCode,
                                                ReceivedAmount = f.ReceivedAmount,
                                                ReceivedRemarks = f.ReceivedRemarks,
                                                BalanceAmount = f.BalanceAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                PreviousBalance = f.PreviousBalance,
                                                CreditDays = f.CreditDays,
                                                FiscalYearID = f.FiscalYearID,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                                InvoiceTypeName = (f.InvoiceType == 1) ? "Credit" : "Cash",
                                                DetailName = (f.InvoiceType == 1) ? db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() : f.CashCustomerName,
                                                ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (customerId != 0)
                {
                    invoice = invoice.Where(x => x.DetailCode == customerId).ToList();
                }
                if (!(string.IsNullOrEmpty(computerNo)))
                {
                    invoice = invoice.Where(x => x.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                if (fromDate != null && toDate != null)
                {
                    invoice = invoice.Where(x => x.RecordDate >= fromDate && x.RecordDate <= toDate).ToList();
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
        public ActionResult DeleteSaleEstimate(string id)
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

            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0, "" });
            string returnId = AddEditInvoice(id, "Sale Estimate", DateTime.Now, 0, 0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Delete");
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

        public ActionResult getMaxCodeForSaleEstimate()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "SE-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Sale Estimate").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Sale Estimate").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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
        #region Sale Estimate Report
        public ActionResult SaleEstimateReport(string id)
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
                               SubDetailCode = f.SubDetailCode,
                               SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
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
        
        #region Sale Return
        public ActionResult SaleReturnIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "SaleReturn"
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
                    ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                    ViewBag.ReceivedDetailCode = new SelectList(getReceivedDetailCodes(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }

        public ActionResult getMaxCodeForSaleReturn()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "SR-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Sale Return").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Sale Return").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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


        [HttpPost]
        public ActionResult InsertUpdateSaleReturn(string data)
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
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, "" });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Sale Return", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, 0, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Record Successfully Updated";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
                    }
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
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Sale Return", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, 0, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Record Successfully Saved";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
                    }
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



        public ActionResult DeleteSaleReturn(string id)
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
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0 ,""});
            string returnId = AddEditInvoice(id, "Sale Return", DateTime.Now, 0,0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Delete");
            if (returnId == "2")
            {
                ModelState.Clear();
                status = true;
                message = "Record Successfully Deleted";
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


        public ActionResult SaleReturnList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }

        public ActionResult GetAllSaleReturn()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                decimal? customerId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string cashCustomerName = "";
                string cashCreditType = "";
                string computerNo = "";
                int invoiceType = 0;
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                }
                if (Session["CashCustomerName"] != null)
                {
                    cashCustomerName = Session["CashCustomerName"].ToString();
                }
                if (Session["CashCreditType"] != null)
                {
                    cashCreditType = Session["CashCreditType"].ToString();
                    if (cashCreditType == "Credit")
                    {
                        invoiceType = 1;
                    }
                    else
                    {
                        invoiceType = 0;
                    }
                }
                if (Session["ComputerNo"] != null)
                {
                    computerNo = Session["ComputerNo"].ToString();
                }
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Sale Return"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                SubDetailCode = f.SubDetailCode,
                                                SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                DiscountType = f.DiscountType,
                                                Discount = f.Discount,
                                                NetDiscount = f.NetDiscount,
                                                Overhead = f.Overhead,
                                                Freight = f.Freight,
                                                NetAmount = f.NetAmount,
                                                Received = f.Received,
                                                ReceivedDetailCode = f.ReceivedDetailCode,
                                                ReceivedAmount = f.ReceivedAmount,
                                                ReceivedRemarks = f.ReceivedRemarks,
                                                BalanceAmount = f.BalanceAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                PreviousBalance = f.PreviousBalance,
                                                CreditDays = f.CreditDays,
                                                FiscalYearID = f.FiscalYearID,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                                InvoiceTypeName = (f.InvoiceType == 1) ? "Credit" : "Cash",
                                                DetailName = (f.InvoiceType == 1) ? db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() : f.CashCustomerName,
                                                ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (customerId != 0)
                {
                    invoice = invoice.Where(x => x.DetailCode == customerId).ToList();
                }
                if (!(string.IsNullOrEmpty(cashCustomerName)))
                {
                    invoice = invoice.Where(x => x.CashCustomerName.ToLower().Contains(cashCustomerName.ToLower())).ToList();
                }
                if (!(string.IsNullOrEmpty(cashCreditType)))
                {
                    invoice = invoice.Where(x => x.InvoiceType == invoiceType).ToList();
                }
                if (!(string.IsNullOrEmpty(computerNo)))
                {
                    invoice = invoice.Where(x => x.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                if (fromDate != null && toDate != null)
                {
                    invoice = invoice.Where(x => x.RecordDate >= fromDate && x.RecordDate <= toDate).ToList();
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




        public ActionResult GetSaleReturnById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           SubDetailCode = f.SubDetailCode,
                           SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                           RecordDate = f.RecordDate,
                           RefrenceNo = f.RefrenceNo,
                           CreditDays = f.CreditDays,
                           DiscountType = f.DiscountType,
                           Discount = f.Discount,
                           NetDiscount = f.NetDiscount,
                           Overhead = f.Overhead,
                           Freight = f.Freight,
                           Remarks = f.Remarks,
                           Received = f.Received,
                           ReceivedDetailCode = f.ReceivedDetailCode,
                           ReceivedAmount = f.ReceivedAmount,
                           ReceivedRemarks = f.ReceivedRemarks,
                           BalanceAmount = f.BalanceAmount,
                           PreviousBalance = f.PreviousBalance,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           CashCustomerName = f.CashCustomerName,
                           InvoiceType = f.InvoiceType,
                           ShowBalance = f.ShowBalance,
                           ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSaleReturnDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 SerialNo = f.SerialNo,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                 ItemNote = f.ItemNote,
                                 LineDiscountType = f.DiscountType ?? string.Empty,
                                 LineDiscount = f.Discount ?? 0,
                                 Tax1ID = f.Tax1ID,
                                 Tax1Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax1ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax1Rate = f.Tax1Rate,
                                 Tax1Value = f.Tax1Value,
                                 Tax2ID = f.Tax2ID,
                                 Tax2Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax2ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax2Rate = f.Tax2Rate,
                                 Tax2Value = f.Tax2Value,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Sale Return Report
        public ActionResult SaleReturnReport(string id)
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

        #region Credit Note
        public ActionResult CreditNoteIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CreditNote"
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
                    ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
                    ViewBag.ReceivedDetailCode = new SelectList(getReceivedDetailCodes(), "DetailCode", "Name");
                    ViewBag.Tax = new SelectList(getTaxes(), "TaxID", "TaxName");
                    ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
                    ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
                    ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        [HttpPost]
        public ActionResult InsertUpdateCreditNote(string data)
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
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, invoice[i].RefCompositKey });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Credit Note", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Updated";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
                    }
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
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, invoice[i].Qty, invoice[i].Rate, invoice[i].LineDiscountType, invoice[i].LineDiscount, invoice[i].LineNetRate, invoice[i].LineGrossAmount, invoice[i].Tax1ID, invoice[i].Tax1Rate, invoice[i].Tax1Value, invoice[i].Tax2ID, invoice[i].Tax2Rate, invoice[i].Tax2Value, invoice[i].LineNetAmount, invoice[i].ItemNote, 0, invoice[i].WarehouseID, invoice[i].RefCompositKey });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Credit Note", invoice[0].RecordDate, invoice[0].DetailCode, invoice[0].SubDetailCode, invoice[0].RefrenceNo, invoice[0].Remarks, invoice[0].GrossTotal, invoice[0].DiscountType, invoice[0].Discount, invoice[0].NetDiscount, invoice[0].Overhead, invoice[0].Freight, invoice[0].NetAmount, invoice[0].Received, invoice[0].ReceivedDetailCode, invoice[0].ReceivedAmount, invoice[0].ReceivedRemarks, invoice[0].BalanceAmount, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, invoice[0].PreviousBalance, invoice[0].CreditDays, User.FiscalYearId, invoice[0].ExtraOrNot, invoice[0].Transporter, invoice[0].BiltyNo, invoice[0].BiltyPacking, invoice[0].BiltyDate, invoice[0].BiltyPending, invoice[0].SalesmanID, "", invoice[0].InvoiceType, invoice[0].CashCustomerName, invoice[0].ShowBalance, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Saved";
                    string sendSms = db.SmsSettings.Select(x => x.SendSms).FirstOrDefault() ?? "N";
                    if (sendSms == "Y")
                    {
                        SmsController sms = new SmsController();
                        sms.SendSms(invoiceNo);
                    }
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


        public ActionResult GetCreditNoteById(string id)
        {
            clsInvoice invoice = new clsInvoice();
            invoice = (from f in db.Invoice_Main
                       where f.ComputerNo == id
                       select new clsInvoice
                       {
                           ComputerNo = f.ComputerNo,
                           DetailCode = f.DetailCode,
                           SubDetailCode = f.SubDetailCode,
                           SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                           RecordDate = f.RecordDate,
                           RefrenceNo = f.RefrenceNo,
                           CreditDays = f.CreditDays,
                           DiscountType = f.DiscountType,
                           Discount = f.Discount,
                           NetDiscount = f.NetDiscount,
                           Overhead = f.Overhead,
                           Freight = f.Freight,
                           Remarks = f.Remarks,
                           Received = f.Received,
                           ReceivedDetailCode = f.ReceivedDetailCode,
                           ReceivedAmount = f.ReceivedAmount,
                           ReceivedRemarks = f.ReceivedRemarks,
                           BalanceAmount = f.BalanceAmount,
                           PreviousBalance = f.PreviousBalance,
                           GrossTotal = f.GrossTotal,
                           NetAmount = f.NetAmount,
                           Status = f.Status,
                           CashCustomerName = f.CashCustomerName,
                           InvoiceType = f.InvoiceType,
                           ShowBalance = f.ShowBalance,
                           DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                           ExtraOrNot = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Count(),
                           Transporter = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.Transporter).FirstOrDefault() ?? string.Empty,
                           BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                           BiltyPacking = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPacking).FirstOrDefault() ?? string.Empty,
                           BiltyDate = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyDate).FirstOrDefault(),
                           BiltyPending = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyPending).FirstOrDefault(),
                           SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                       }).FirstOrDefault();
            var data = invoice;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCreditNoteDetailById(string id)
        {
            List<clsInvoice> invoiceDetail = new List<clsInvoice>();
            invoiceDetail = (from f in db.Invoice_Detail
                             where f.ComputerNo == id
                             select new clsInvoice
                             {
                                 ComputerNo = f.ComputerNo,
                                 ItemID = f.ItemID,
                                 SerialNo = f.SerialNo,
                                 ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                 ItemDescription = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                 WarehouseID = f.WarehouseID,
                                 WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == f.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                 Rate = f.Rate,
                                 Qty = f.Qty,
                                 ItemNote = f.ItemNote,
                                 LineDiscountType = f.DiscountType ?? string.Empty,
                                 LineDiscount = f.Discount ?? 0,
                                 Tax1ID = f.Tax1ID,
                                 Tax1Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax1ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax1Rate = f.Tax1Rate,
                                 Tax1Value = f.Tax1Value,
                                 Tax2ID = f.Tax2ID,
                                 Tax2Name = db.Admin_Setup_Tax.Where(x => x.TaxID == f.Tax2ID).Select(x => x.TaxName).FirstOrDefault() ?? string.Empty,
                                 Tax2Rate = f.Tax2Rate,
                                 Tax2Value = f.Tax2Value,
                                 LineNetRate = f.NetRate,
                                 LineGrossAmount = f.GrossValue,
                                 LineNetAmount = f.NetValue,
                                 CompositKey = f.RefCompositKey ?? "0",
                                 HiddenComputerNo = db.Invoice_Detail.Where(x => x.CompositKey == f.RefCompositKey).Select(x => x.ComputerNo).FirstOrDefault() ?? "0"
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = invoiceDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreditNoteList()
        {
            ViewBag.DetailCode = new SelectList(getDetailCodes(), "DetailCode", "Name");
            return PartialView();
        }
        public ActionResult GetAllCreditNotes()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                decimal? customerId = 0;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                string computerNo = "";
                if (Session["CustomerId"] != null)
                {
                    customerId = Convert.ToDecimal(Session["CustomerId"]);
                }
                if (Session["FromDate"] != null)
                {
                    fromDate = Convert.ToDateTime(Session["FromDate"]);
                    toDate = Convert.ToDateTime(Session["ToDate"]);
                }
                if (Session["ComputerNo"] != null)
                {
                    computerNo = Session["ComputerNo"].ToString();
                }
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Credit Note"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                Type = f.Type,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                SubDetailCode = f.SubDetailCode,
                                                SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                RefrenceNo = f.RefrenceNo,
                                                Remarks = f.Remarks,
                                                GrossTotal = f.GrossTotal,
                                                DiscountType = f.DiscountType,
                                                Discount = f.Discount,
                                                NetDiscount = f.NetDiscount,
                                                Overhead = f.Overhead,
                                                Freight = f.Freight,
                                                NetAmount = f.NetAmount,
                                                Received = f.Received,
                                                ReceivedDetailCode = f.ReceivedDetailCode,
                                                ReceivedAmount = f.ReceivedAmount,
                                                ReceivedRemarks = f.ReceivedRemarks,
                                                BalanceAmount = f.BalanceAmount,
                                                BusinessID = f.BusinessID,
                                                UserID = f.UserID,
                                                LastUserID = f.LastUserID,
                                                SaveDateTime = f.SaveDateTime,
                                                LastEditDateTime = f.LastEditDateTime,
                                                PreviousBalance = f.PreviousBalance,
                                                CreditDays = f.CreditDays,
                                                FiscalYearID = f.FiscalYearID,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                                InvoiceTypeName = (f.InvoiceType == 1) ? "Credit" : "Cash",
                                                DetailName = (f.InvoiceType == 1) ? db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() : f.CashCustomerName,
                                                ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault(),
                                                Status = f.Status
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (customerId != 0)
                {
                    invoice = invoice.Where(x => x.DetailCode == customerId).ToList();
                }
                if (!(string.IsNullOrEmpty(computerNo)))
                {
                    invoice = invoice.Where(x => x.ComputerNo.ToLower().Contains(computerNo.ToLower())).ToList();
                }
                if (fromDate != null && toDate != null)
                {
                    invoice = invoice.Where(x => x.RecordDate >= fromDate && x.RecordDate <= toDate).ToList();
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
        public ActionResult DeleteCreditNote(string id)
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
            dt.Rows.Add(new object[] { 0, 0, 0, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0 ,""});
            string returnId = AddEditInvoice(id, "Credit Note", DateTime.Now, 0,0, "", "", 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Delete");
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

        public ActionResult getMaxCodeForCreditNote()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "CN-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Credit Note").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Credit Note").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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
        #region Credit Note Report
        public ActionResult CreditNoteReport(string id)
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
                               SubDetailCode = f.SubDetailCode,
                               SubDetailName = db.AC_Detail_Sub.Where(x => x.SubDetailCode == f.SubDetailCode).Select(x => x.Name).FirstOrDefault(),
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
        
        #region Reports
        #region Sale Schedule
        public List<clsItem> getSubCategories()
        {
            List<clsItem> subcategory = new List<clsItem>();
            subcategory = (from c in db.Item_SubCategory
                           where c.Status == "Active"
                           select new clsItem
                           {
                               SubCategoryID = c.SubCategoryID,
                               SubCategoryName = c.Description
                           }).ToList();
            return subcategory;
        }
        public ActionResult SaleSchedule()
        {
            ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.City = new SelectList(getCities(), "CityId", "CityName");

            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.SubCategory = new SelectList(getSubCategories(), "SubCategoryID", "SubCategoryName");

            return View();
        }
        [HttpPost]
        public ActionResult PrintSaleScheduleReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                string reportType = Request.Form["ReportType"].ToString();
                String[] items = Request.Form.GetValues("ItemID");
                //int itemID = Convert.ToInt32(Request.Form["ItemID"]);
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);

                int categoryid = Convert.ToInt32(Request.Form["CategoryID"]);
                int subcategoryid = Convert.ToInt32(Request.Form["SubCategoryID"]);
                

                String[] City = Request.Form.GetValues("CityId");

                List<clsInvoice> invoiceList = new List<clsInvoice>();

                if (cust != null)
                {
                    for (int i = 0; i < cust.Length; i++)
                    {
                        decimal customerId = Convert.ToDecimal(cust[i]);
                        List<clsInvoice> list = GetInvoicesListForSaleSchedule(customerId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }
                else
                {
                    List<clsAccountDetail> customers = getCustomers();
                    for (int i = 0; i < customers.Count; i++)
                    {
                        decimal customerId = Convert.ToDecimal(customers[i].DetailCode);
                        List<clsInvoice> list = GetInvoicesListForSaleSchedule(customerId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }

                List<clsInvoice> finalList = new List<clsInvoice>(); 
                if (City != null)
                {
                    int j = 0;
                    for (int i = 0; i < City.Length; i++)
                    {
                        int cityId = Convert.ToInt32(City[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = invoiceList.Where(x => x.CityId == cityId).ToList();
                        finalList.AddRange(list);
                    }
                }
                else
                {
                    finalList = invoiceList.ToList();
                }


                if (categoryid != null && categoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.CategoryID == categoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                if (subcategoryid != null && subcategoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.SubCategoryID == subcategoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                


                finalList = finalList.OrderBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ThenBy(x => x.SerialNo).ToList();
                return View(finalList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetInvoicesListForSaleSchedule(decimal customerId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, String[] items, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> invoiceList = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Sale Invoice + Sale Return")
            {
                type1 = "Sale";
                type2 = "Sale Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (items == null && warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            RecordDate = f.RecordDate,
                            RefrenceNo = f.RefrenceNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                            ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),

                            CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,

                            //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                            SerialNo = fd.SerialNo,
                            Qty = fd.Qty ?? 0,
                            UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            Rate = fd.Rate ?? 0,
                            GrossValue = fd.GrossValue ?? 0,
                            NetValue = fd.NetValue ?? 0,
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).ToList();
                invoiceList.AddRange(list);
            }
            else if (items != null && warehouseId == 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == customerId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && (f.Type == type1 || f.Type == type2)
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                                CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                SerialNo = fd.SerialNo,
                                Qty = fd.Qty ?? 0,
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                Rate = fd.Rate ?? 0,
                                GrossValue = fd.GrossValue ?? 0,
                                NetValue = fd.NetValue ?? 0,
                                SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).ToList();
                    invoiceList.AddRange(list);
                }
            }
            else if (items == null && warehouseId != 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && fd.WarehouseID == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            RecordDate = f.RecordDate,
                            RefrenceNo = f.RefrenceNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                            ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                            SerialNo = fd.SerialNo,
                            Qty = fd.Qty ?? 0,
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            Rate = fd.Rate ?? 0,
                            GrossValue = fd.GrossValue ?? 0,
                            NetValue = fd.NetValue ?? 0,
                            SalesmanID = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).ToList();
                invoiceList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == customerId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && fd.WarehouseID == warehouseId
                            && (f.Type == type1 || f.Type == type2)
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                                CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                SerialNo = fd.SerialNo,
                                Qty = fd.Qty ?? 0,
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                Rate = fd.Rate ?? 0,
                                GrossValue = fd.GrossValue ?? 0,
                                NetValue = fd.NetValue ?? 0,
                                SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).ToList();
                    invoiceList.AddRange(list);
                }
            }
            if (salesmanId != 0)
            {
                invoiceList = invoiceList.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return invoiceList;
        }
        public List<clsAccountDetail> getCustomers()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && (c.Type == "Customer" || c.Type == "Customer + Supplier")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).OrderBy(x=>x.Name).ToList();
            return detail;
        }
        public List<clsAccountDetail> getCustomersAndSuppliers()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active" && (c.Type == "Customer" || c.Type == "Supplier" || c.Type == "Customer + Supplier")
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).OrderBy(x => x.Name).ToList();
            return detail;
        }
        public List<clsCity> getCities()
        {
            List<clsCity> city = new List<clsCity>();
            city = (from c in db.Cities
                    select new clsCity
                    {
                        CityId = c.CityId,
                        CityName = c.CityName
                    }).ToList();
            return city;
        }
        #endregion
        #region Sale Invoice Summary
        public ActionResult SaleInvoiceSummary()
        {
            ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintSaleInvoiceSummaryReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                string reportType = Request.Form["ReportType"].ToString();

                String[] City = Request.Form.GetValues("CityId");


                List<clsInvoice> invoiceList = new List<clsInvoice>();


                if (cust != null)
                {
                    for (int i = 0; i < cust.Length; i++)
                    {
                        decimal customerId = Convert.ToDecimal(cust[i]);
                        List<clsInvoice> list = GetInvoicesListForSaleInvoiceSummary(customerId, fromDate, toDate, referenceNo, computerNo, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }
                else
                {
                    List<clsAccountDetail> customers = getCustomers();
                    for (int i = 0; i < customers.Count; i++)
                    {
                        decimal customerId = Convert.ToDecimal(customers[i].DetailCode);
                        List<clsInvoice> list = GetInvoicesListForSaleInvoiceSummary(customerId, fromDate, toDate, referenceNo, computerNo, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }


                List<clsInvoice> finalList = new List<clsInvoice>();
                if (City != null)
                {
                    int j = 0;
                    for (int i = 0; i < City.Length; i++)
                    {
                        int cityId = Convert.ToInt32(City[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = invoiceList.Where(x => x.CityId == cityId).ToList();
                        finalList.AddRange(list);
                    }
                }
                else
                {
                    finalList = invoiceList.ToList();
                }


                finalList = finalList.OrderBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ToList();
                return View(finalList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetInvoicesListForSaleInvoiceSummary(decimal customerId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Sale Invoice + Sale Return")
            {
                type1 = "Sale";
                type2 = "Sale Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            RecordDate = f.RecordDate,
                            RefrenceNo = f.RefrenceNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                            Remarks = f.Remarks,
                            BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                            SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            NetAmount = f.NetAmount ?? 0,
                            ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                            ReceivedAmount = f.ReceivedAmount ?? 0,
                            BalanceAmount = f.BalanceAmount ?? 0
                        }).ToList();
            }
            else
            {
                list = (from f in db.Invoice_Main
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && db.Invoice_Detail.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.WarehouseID).FirstOrDefault() == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            RecordDate = f.RecordDate,
                            RefrenceNo = f.RefrenceNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                            Remarks = f.Remarks,
                            BiltyNo = db.Invoice_Extra.Where(x => x.ComputerNo == f.ComputerNo).Select(x => x.BiltyNo).FirstOrDefault() ?? string.Empty,
                            SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            NetAmount = f.NetAmount ?? 0,
                            ReceivedDetailName = db.AC_Detail.Where(x => x.DetailCode == f.ReceivedDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                            ReceivedAmount = f.ReceivedAmount ?? 0,
                            BalanceAmount = f.BalanceAmount ?? 0
                        }).ToList();
            }
            if (salesmanId != 0)
            {
                list = list.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return list;
        }
        #endregion
        #region Sale Summary Customer Item
        public ActionResult SaleSummaryCustomerItem()
        {
            ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.City = new SelectList(getCities(), "CityId", "CityName");

            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.SubCategory = new SelectList(getSubCategories(), "SubCategoryID", "SubCategoryName");

            return View();
        }
        [HttpPost]
        public ActionResult PrintSaleSummaryCustomerItemReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                String[] items = Request.Form.GetValues("ItemID");
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                String[] City = Request.Form.GetValues("CityId");


                int categoryid = Convert.ToInt32(Request.Form["CategoryID"]);
                int subcategoryid = Convert.ToInt32(Request.Form["SubCategoryID"]);
                
                string reportType = Request.Form["ReportType"].ToString();
                List<clsInvoice> invoiceList = new List<clsInvoice>();
                clsInvoice invoice = new clsInvoice();
                clsAccountDetail customer;
                if (cust != null)
                {
                    for (int i = 0; i < cust.Length; i++)
                    {
                        customer = new clsAccountDetail();
                        decimal customerId = Convert.ToDecimal(cust[i]);
                        customer.DetailCode = customerId;
                        customer.Name = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault();
                        customer.CityName = (from c in db.AC_Detail_Extra join city in db.Cities on c.CityId equals city.CityId where c.DetailCode == customerId select city.CityName).FirstOrDefault();
                        invoice.DetailList.Add(customer);
                        List<clsInvoice> list = GetInvoicesListForSaleSummaryCustomerItem(customerId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);


                    }
                }
                else
                {
                    List<clsAccountDetail> customers = getCustomers();
                    for (int i = 0; i < customers.Count; i++)
                    {
                        customer = new clsAccountDetail();
                        decimal customerId = Convert.ToDecimal(customers[i].DetailCode);
                        customer.DetailCode = customerId;
                        customer.Name = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault() + ", " + (from c in db.AC_Detail_Extra join city in db.Cities on c.CityId equals city.CityId where c.DetailCode == customerId select city.CityName).FirstOrDefault();
                        customer.CityName = (from c in db.AC_Detail_Extra join city in db.Cities on c.CityId equals city.CityId where c.DetailCode == customerId select city.CityName).FirstOrDefault();
                        invoice.DetailList.Add(customer);
                        List<clsInvoice> list = GetInvoicesListForSaleSummaryCustomerItem(customerId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }


                List<clsInvoice> finalList = new List<clsInvoice>();
                if (City != null)
                {
                    int j = 0;
                    for (int i = 0; i < City.Length; i++)
                    {
                        int cityId = Convert.ToInt32(City[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = invoiceList.Where(x => x.CityId == cityId).ToList();
                        finalList.AddRange(list);
                    }
                }

                else
                {
                    finalList = invoiceList.ToList();
                }


                if (categoryid != null && categoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.CategoryID == categoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                if (subcategoryid != null && subcategoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.SubCategoryID == subcategoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                

                invoice.InvoicesList = finalList.ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetInvoicesListForSaleSummaryCustomerItem(decimal customerId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, String[] items, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> invoiceList = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Sale Invoice + Sale Return")
            {
                type1 = "Sale";
                type2 = "Sale Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (items == null && warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = customerId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,

                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else if (items != null && warehouseId == 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == customerId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = customerId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                                CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            else if (items == null && warehouseId != 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && fd.WarehouseID == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = customerId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == customerId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && fd.WarehouseID == warehouseId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = customerId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                                CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            if (salesmanId != 0)
            {
                invoiceList = invoiceList.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return invoiceList;
        }
        #endregion
        #region Sale Summary Item Customer
        public ActionResult SaleSummaryItemCustomer()
        {
            ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.SubCategory = new SelectList(getSubCategories(), "SubCategoryID", "SubCategoryName");

            return View();
        }
        [HttpPost]
        public ActionResult PrintSaleSummaryItemCustomerReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                String[] items = Request.Form.GetValues("ItemID");
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                string reportType = Request.Form["ReportType"].ToString();
                String[] City = Request.Form.GetValues("CityId");

                int categoryid = Convert.ToInt32(Request.Form["CategoryID"]);
                int subcategoryid = Convert.ToInt32(Request.Form["SubCategoryID"]);

                List<clsInvoice> invoiceList = new List<clsInvoice>();
                clsInvoice invoice = new clsInvoice();
                if (cust != null)
                {
                    for (int i = 0; i < cust.Length; i++)
                    {
                        decimal customerId = Convert.ToDecimal(cust[i]);
                        List<clsInvoice> list = GetInvoicesListForSaleSummaryItemCustomer(customerId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }
                else
                {
                    List<clsAccountDetail> customers = getCustomers();
                    for (int i = 0; i < customers.Count; i++)
                    {
                        decimal customerId = Convert.ToDecimal(customers[i].DetailCode);
                        List<clsInvoice> list = GetInvoicesListForSaleSummaryItemCustomer(customerId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }


                List<clsInvoice> finalList = new List<clsInvoice>();
                if (City != null)
                {
                    int j = 0;
                    for (int i = 0; i < City.Length; i++)
                    {
                        int cityId = Convert.ToInt32(City[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = invoiceList.Where(x => x.CityId == cityId).ToList();
                        finalList.AddRange(list);
                    }
                }
                else
                {
                    finalList = invoiceList.ToList();
                }

                if (categoryid != null && categoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.CategoryID == categoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                if (subcategoryid != null && subcategoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.SubCategoryID == subcategoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                

                invoice.InvoicesList = finalList.ToList();
                invoice.InvoicesList2 = finalList.GroupBy(x => x.ItemID).Select(g => g.First()).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetInvoicesListForSaleSummaryItemCustomer(decimal customerId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, String[] items, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> invoiceList = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Sale Invoice + Sale Return")
            {
                type1 = "Sale";
                type2 = "Sale Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (items == null && warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = customerId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else if (items != null && warehouseId == 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == customerId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = customerId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                                CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            else if (items == null && warehouseId != 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && fd.WarehouseID == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = customerId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == customerId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && fd.WarehouseID == warehouseId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = customerId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                                CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                                CityName = (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == customerId select ct.CityName).FirstOrDefault() ?? "",
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            if (salesmanId != 0)
            {
                invoiceList = invoiceList.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return invoiceList;
        }
        #endregion
        #region Sale Summary Item
        public ActionResult SaleSummaryItem()
        {
            ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.SubCategory = new SelectList(getSubCategories(), "SubCategoryID", "SubCategoryName");

            return View();
        }
        [HttpPost]
        public ActionResult PrintSaleSummaryItemReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                String[] items = Request.Form.GetValues("ItemID");
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                String[] City = Request.Form.GetValues("CityId");

                int categoryid = Convert.ToInt32(Request.Form["CategoryID"]);
                int subcategoryid = Convert.ToInt32(Request.Form["SubCategoryID"]);

                string sortType = Request.Form["SortType"].ToString();
                string reportType = Request.Form["ReportType"].ToString();
                List<clsInvoice> invoiceList = new List<clsInvoice>();
                clsInvoice invoice = new clsInvoice();
                if (cust != null)
                {
                    for (int i = 0; i < cust.Length; i++)
                    {
                        decimal customerId = Convert.ToDecimal(cust[i]);
                        List<clsInvoice> list = GetInvoicesListForSaleSummaryItem(customerId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }
                else
                {
                    List<clsAccountDetail> customers = getCustomers();
                    for (int i = 0; i < customers.Count; i++)
                    {
                        decimal customerId = Convert.ToDecimal(customers[i].DetailCode);
                        List<clsInvoice> list = GetInvoicesListForSaleSummaryItem(customerId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }
                }


                List<clsInvoice> finalList = new List<clsInvoice>();
                if (City != null)
                {
                    int j = 0;
                    for (int i = 0; i < City.Length; i++)
                    {
                        int cityId = Convert.ToInt32(City[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = invoiceList.Where(x => x.CityId == cityId).ToList();
                        finalList.AddRange(list);
                    }
                }
                else
                {
                    finalList = invoiceList.ToList();
                }


                if (categoryid != null && categoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.CategoryID == categoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                if (subcategoryid != null && subcategoryid != 0)
                {
                    List<clsInvoice> list = new List<clsInvoice>();
                    list = finalList.Where(x => x.SubCategoryID == subcategoryid).ToList();
                    finalList.Clear();
                    finalList.AddRange(list);
                }
                
                invoice.InvoicesList = finalList.ToList();
                invoice.InvoicesList2 = finalList.GroupBy(x => x.ItemID).Select(g => 
                       new clsInvoice 
                       {
                          ItemID = g.Key,
                          ItemName = g.Max(x => x.ItemName),
                          UrduName = g.Max(x => x.UrduName),
                          UnitName = g.Max(x => x.UnitName),
                          Qty = g.Sum(x => x.Qty)
                       }).ToList();
                
                if (sortType != null)
                {
                    if (sortType == "Sold Qty Ascending")
                    {
                        invoice.InvoicesList2 = invoice.InvoicesList2.OrderBy(x => x.Qty).ThenBy(x => x.ItemName).ToList();
                    }
                    else
                    {
                        invoice.InvoicesList2 = invoice.InvoicesList2.OrderByDescending(x => x.Qty).ThenBy(x => x.ItemName).ToList();
                    }
                }
                else
                {
                    invoice.InvoicesList2 = invoice.InvoicesList2.OrderByDescending(x => x.Qty).ThenBy(x => x.ItemName).ToList();
                }
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetInvoicesListForSaleSummaryItem(decimal customerId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, String[] items, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> invoiceList = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Sale Invoice + Sale Return")
            {
                type1 = "Sale";
                type2 = "Sale Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (items == null && warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = customerId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else if (items != null && warehouseId == 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == customerId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = customerId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                                CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            else if (items == null && warehouseId != 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && fd.WarehouseID == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        group fd by fd.ItemID into g
                        select new clsInvoice
                        {
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailCode = customerId,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            ItemID = g.FirstOrDefault().ItemID,
                            ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                            Qty = g.Sum(a => a.Qty),
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            GrossValue = g.Sum(a => a.GrossValue),
                            NetValue = g.Sum(a => a.NetValue),
                            SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).OrderBy(x => x.ItemName).ToList();
                invoiceList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == customerId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && fd.WarehouseID == warehouseId
                            && (f.Type == type1 || f.Type == type2)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailCode = customerId,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                                CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                SubCategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.SubCategoryID).FirstOrDefault() ?? 0,
                                Qty = g.Sum(a => a.Qty),
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                GrossValue = g.Sum(a => a.GrossValue),
                                NetValue = g.Sum(a => a.NetValue),
                                SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).OrderBy(x => x.ItemName).ToList();
                    invoiceList.AddRange(list);
                }
            }
            if (salesmanId != 0)
            {
                invoiceList = invoiceList.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return invoiceList;
        }
        #endregion
        #region Customer Supplier Detail
        public ActionResult CustomerSupplierDetail()
        {
            ViewBag.Customers = new SelectList(getCustomersAndSuppliers(), "DetailCode", "Name");
            ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintCustomerSupplierDetailReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                String[] City = Request.Form.GetValues("CityId");
                string reportType = Request.Form["ReportType"].ToString();
                clsInvoice invoice = new clsInvoice();
                List<clsInvoice> invoiceList = GetCustomerSupplierDetail(cust, City, reportType);
                invoiceList = invoiceList.OrderBy(x => x.DetailName).ToList();
                invoice.InvoicesList = invoiceList;
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetCustomerSupplierDetail(String[] cust, String[] City, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> list2 = new List<clsInvoice>();
            List<clsInvoice> list3 = new List<clsInvoice>();
            List<clsInvoice> list4 = new List<clsInvoice>();
            list2 = (from d in db.AC_Detail
                    join e in db.AC_Detail_Extra on d.DetailCode equals e.DetailCode
                    where d.Type == "Customer" || d.Type == "Supplier" || d.Type == "Customer + Supplier"
                    select new clsInvoice
                    {
                        DetailCode = d.DetailCode,
                        DetailName = d.Name,
                        CityId = e.CityId ?? 0,
                        CityName = (from c in db.AC_Detail_Extra join cc in db.Cities on c.CityId equals cc.CityId where c.DetailCode == d.DetailCode select cc.CityName).FirstOrDefault() ?? "",
                        Cell1 = e.Cell1 ?? "",
                        Address = e.Address  ?? "",
                        Type = d.Type
                    }).ToList();
            if (reportType != "All")
            {
                list2 = list2.Where(x => x.Type == reportType).ToList();
            }
            list3.AddRange(list2);
            list4.AddRange(list2);
            if (cust != null)
            {
                int j = 0;
                for (int i = 0; i < cust.Length; i++)
                {
                    decimal customerId = Convert.ToDecimal(cust[i]);
                    //if (list2.Any(x => x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsInvoice>();
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list2.Where(x => x.DetailCode == customerId).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    j++;
                    //}
                }
            }
            if (City != null)
            {
                int j = 0;
                for (int i = 0; i < City.Length; i++)
                {
                    int cityId = Convert.ToInt32(City[i]);
                    //if (list2.Any(x => x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list3.Where(x => x.CityId == cityId).ToList();
                    list4.AddRange(list);
                    j++;
                    //}
                }
            }
            if (reportType != "All")
            {
                list4 = list4.Where(x => x.Type == reportType).ToList();
            }
            return list4;
        }
        #endregion
        #endregion
        #region Ledger
        public ActionResult Ledger()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "Ledger"
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
                    DateTime fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
                    clsInvoice calculate = new clsInvoice();
                    calculate.ByDate = fromDate;
                    ViewBag.DetailCode = new SelectList(getAllDetailCodes(), "DetailCode", "Name");
                    return View(calculate);
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsAccountDetail> getAllDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active"
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                      }).OrderBy(x=>x.Name).ToList();
            return detail;
        }
        #endregion
        #region Product Lookup
        public ActionResult ProductLookup()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "ProductLookup"
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
        public ActionResult SetSessions(bool? itemCheckbox, int? itemId, bool? categoryCheckbox, int? categoryId,
                                        bool? brandCheckbox, int? brandId, bool? codeCheckbox, string code)
        {
            bool status = true;

            if (itemCheckbox == true && itemId != null)
            {
                Session["ItemId"] = itemId;
            }
            else
            {
                Session["ItemId"] = null;
            }

            //////////////////
            if (categoryCheckbox == true && categoryId != null)
            {
                Session["Category"] = categoryId;
            }
            else
            {
                Session["Category"] = null;
            }
            ////////////////
            if (brandCheckbox == true && brandId != null)
            {
                Session["Brand"] = brandId;
            }
            else
            {
                Session["Brand"] = null;
            }
            /////////////////////
            if (codeCheckbox == true && !(string.IsNullOrEmpty(code)))
            {
                Session["Code"] = code;
            }
            else
            {
                Session["Code"] = null;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllItems()
        {
            if (User != null)
            {
                int? itemId = 0;
                int? categoryId = 0;
                int? brandId = 0;
                string code = "";
                if (Session["ItemId"] != null)
                {
                    itemId = Convert.ToInt32(Session["ItemId"]);
                }
                if (Session["Category"] != null)
                {
                    categoryId = Convert.ToInt32(Session["Category"]);
                }
                if (Session["Brand"] != null)
                {
                    brandId = Convert.ToInt32(Session["Brand"]);
                }
                if (Session["Code"] != null)
                {
                    code = Session["Code"].ToString();
                }
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsItem> items = new List<clsItem>();
                //if (itemId == 0)
                //{
                //    items = (from i in db.Item_Information
                //             select new clsItem
                //             {
                //                 ItemID = i.ItemID,
                //                 ItemName = i.ItemName,
                //                 Stock = i.Stock,
                //                 CostPrice = i.CostPrice,
                //                 SalePrice = i.SalePrice,
                //                 MinLevel = i.MinPurchasePrice,
                //                 MaxLevel = i.MaxPurchasePrice,
                //                 PurchaseRate = i.PurchaseRate,
                //                 AverageRate2 = Math.Round(i.AverageRate , 2),
                //                 CategoryName = i.CategoryName,
                //                 BrandName = i.BrandName,
                //                 UrduName = i.UrduName,
                //                 CategoryID = i.CategoryID,
                //                 BrandID = i.BrandID
                //             }).ToList();
                //}
                //else
                //{
                //    items = (from i in db.Item_Information
                //             where i.ItemID == itemId
                //             select new clsItem
                //             {
                //                 ItemID = i.ItemID,
                //                 ItemName = i.ItemName,
                //                 Stock = i.Stock,
                //                 CostPrice = i.CostPrice,
                //                 SalePrice = i.SalePrice,
                //                 MinLevel = i.MinPurchasePrice,
                //                 MaxLevel = i.MaxPurchasePrice,
                //                 PurchaseRate = i.PurchaseRate,
                //                 AverageRate2 = Math.Round(i.AverageRate , 2),
                //                 CategoryName = i.CategoryName,
                //                 BrandName = i.BrandName,
                //                 UrduName = i.UrduName,
                //                 CategoryID = i.CategoryID,
                //                 BrandID = i.BrandID
                //             }).ToList();
                //}
                if (categoryId != 0)
                {
                    items = items.Where(x => x.CategoryID==categoryId).ToList();
                }
                if (brandId != 0)
                {
                    items = items.Where(x => x.BrandID == brandId).ToList();
                }
                if (!(string.IsNullOrEmpty(code)))
                {
                    items = items.Where(x => x.UrduName.ToLower().Contains(code.ToLower())).ToList();
                }
                items = items.OrderBy(x => x.ItemName).ToList();
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        items = items.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = items.Count();
                var data = items.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult ItemStockListWarehouseBaseProductLookup(int id)
        {
            List<clsItem> list = new List<clsItem>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("spGet_ItemStockWarehouseBase", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = id;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsItem()
                {
                    WarehouseName = dt[0].ToString(),
                    Stock = Convert.ToDecimal(dt["Stock"]),
                });
            }
            var data = list.ToList();
            return Json(new { data = data },
                JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetSessionForItemId(int? itemId)
        {
            bool status = true;

            if (itemId != null)
            {
                Session["ItemIdSingle"] = itemId;
            }
            else
            {
                Session["ItemIdSingle"] = null;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetItemLastPurchaseRate()
        {
            if (User != null)
            {
                int? itemId = 0;
                if (Session["ItemIdSingle"] != null)
                {
                    itemId = Convert.ToInt32(Session["ItemIdSingle"]);
                }
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                                            where fd.ItemID == itemId && fd.Type == "Purchase"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                                ItemID = fd.ItemID,
                                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault() ?? "",
                                                Qty = fd.Qty,
                                                Rate = fd.NetRate,
                                                GrossValue = fd.GrossValue,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                            }).ToList();
                invoice = invoice.OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
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
        public ActionResult GetItemLastSaleRate()
        {
            if (User != null)
            {
                int? itemId = 0;
                if (Session["ItemIdSingle"] != null)
                {
                    itemId = Convert.ToInt32(Session["ItemIdSingle"]);
                }
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                                            where fd.ItemID == itemId && fd.Type == "Sale"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                RecordDate = f.RecordDate,
                                                DetailCode = f.DetailCode,
                                                DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() ?? "",
                                                ItemID = fd.ItemID,
                                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault() ?? "",
                                                Qty = fd.Qty,
                                                Rate = fd.NetRate,
                                                GrossValue = fd.GrossValue,
                                                CashCustomerName = f.CashCustomerName,
                                                InvoiceType = f.InvoiceType,
                                            }).ToList();
                invoice = invoice.OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
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
        #endregion
        #region Manual Product Price
        public ActionResult ManualProductPrice()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "ManualProductPrice"
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
        public ActionResult SetSessionsForManualProductPrice(bool? brandCheckbox, int? brandId, bool? categoryCheckbox, int? categoryId, bool? itemCheckbox, int? itemId)
        {
            bool status = true;

            if (itemCheckbox == true && itemId != null)
            {
                Session["ItemID"] = itemId;
            }
            else
            {
                Session["ItemID"] = null;
            }

            ////////////////
            if (brandCheckbox == true && brandId != null)
            {
                Session["Brand"] = brandId;
            }
            else
            {
                Session["Brand"] = null;
            }
            /////////////////////
            if (categoryCheckbox == true && categoryId != null)
            {
                Session["Category"] = categoryId;
            }
            else
            {
                Session["Category"] = null;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllItemsForManualProductPrice()
        {
            if (User != null)
            {
                int? brandId = 0;
                int? categoryId = 0;
                int? itemId = 0;
                if (Session["Brand"] != null)
                {
                    brandId = Convert.ToInt32(Session["Brand"]);
                }
                if (Session["Category"] != null)
                {
                    categoryId = Convert.ToInt32(Session["Category"]);
                }

                if (Session["ItemID"] != null)
                {
                    itemId = Convert.ToInt32(Session["ItemID"]);
                }
                

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var detailName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsItem> items = (from f in db.Item_Information
                                       select new clsItem
                                       {
                                           ItemID = f.ItemID,
                                           ItemName = f.ItemName,
                                           UrduName = f.UrduName,
                                           Barcode = f.Barcode,
                                           CategoryID = f.CategoryID,
                                           CategoryName = db.Item_Category.Where(x => x.CategoryID == f.CategoryID).Select(x => x.Description).FirstOrDefault() ?? "",
                                           BrandID = f.BrandID,
                                           BrandName = db.Item_Brand.Where(x => x.BrandID == f.BrandID).Select(x => x.Description).FirstOrDefault() ?? "",
                                           CostPrice = f.CostPrice ?? 0,
                                           SalePrice = f.SalePrice ?? 0,
                                       }).OrderBy(x => x.UrduName).ThenBy(x => x.ItemID).ToList();


                if (itemId != 0)
                {
                    items = items.Where(x => x.ItemID == itemId).ToList();
                }
                if (brandId != 0)
                {
                    items = items.Where(x => x.BrandID == brandId).ToList();
                }
                if (categoryId != 0)
                {
                    items = items.Where(x => x.CategoryID == categoryId).ToList();
                }
                items = items.OrderBy(x => x.ItemName).ToList();
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        items = items.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = items.Count();
                var data = items.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult UpdateItemManualPrice(string data)
        {
            string message = "";
            bool status = false;
            int returnId = 0;
            List<clsItem> items = JsonConvert.DeserializeObject<List<clsItem>>(data);
            if(items.Count == 0)
            {
                status = false;
                message = "There is no record to be saved.";
                return new JsonResult { Data = new { status = status, message = message } };
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("CostPrice");
            dt.Columns.Add("SalePrice");
            for (int i = 0; i < items.Count; i++)
            {
                dt.Rows.Add(new object[] { i + 1, items[i].ItemID, items[i].CostPrice, items[i].SalePrice });
            }
            returnId = AddEdit(dt);
            if (returnId > 0)
            {
                ModelState.Clear();
                status = true;
                message = "Record Successfully Updated";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        private int AddEdit(DataTable dt)
        {
            int returnId = 0;
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spUpdateItemManualPrice", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@dtPrice", SqlDbType.Structured).Value = dt;
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
        [HttpPost]
        public ActionResult InsertUpdateItemsPrices(string data)
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
            if (invoice[0].ComputerNo != "")
            {
                
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, 0, invoice[i].Rate, "", 0, invoice[i].LineNetRate, 0, 0, 0, 0, 0, 0,0,0, "", 0, 0 ,""});
                }
                 returnId = AddEditInvoice(invoice[0].ComputerNo, "Manual Price", DateTime.Now, 0,0,"", invoice[0].Remarks,0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0,0, User.FiscalYearId, 0,"", "", "", DateTime.Now, 0, 0, "", 0,"", 0, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Updated";
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
                string maxCode = getMaxCodeForPriceList();
                invoiceNo = maxCode;
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, 0, invoice[i].Rate, "", 0, invoice[i].LineNetRate, 0, 0, 0, 0, 0, 0,0,0, "", 0, 0 ,""});
                }
                returnId = AddEditInvoice(maxCode, "Manual Price", DateTime.Now, 0,0, "", invoice[0].Remarks, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Saved";
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
        public string getMaxCodeForPriceList()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "MP-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Manual Price").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Manual Price").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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

            return maxCode;
        }
         public ActionResult ManualPricesList()
        {
            return PartialView();
        }
         public ActionResult GetAllManualPricesLists()
         {
             if (User != null)
             {
                 var draw = Request.Form.GetValues("draw").FirstOrDefault();
                 var start = Request.Form.GetValues("start").FirstOrDefault();
                 var length = Request.Form.GetValues("length").FirstOrDefault();
                 var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                         + "][name]").FirstOrDefault();
                 var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                 var remarks = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                 int pageSize = length != null ? Convert.ToInt32(length) : 0;
                 int skip = start != null ? Convert.ToInt16(start) : 0;
                 int recordsTotal = 0;
                 List<clsInvoice> invoice = (from f in db.Invoice_Main
                                             join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                             where fis.FiscalYearID == User.FiscalYearId && f.Type == "Manual Price"
                                             select new clsInvoice
                                             {
                                                 ComputerNo = f.ComputerNo,
                                                 RecordDate = f.RecordDate,
                                                 Remarks = f.Remarks,
                                             }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                 //SEARCHING...
                 if (!string.IsNullOrEmpty(remarks))
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
         public ActionResult GetPriceById(string id)
         {
             clsInvoice invoice = new clsInvoice();
             invoice = (from f in db.Invoice_Main
                        where f.ComputerNo == id
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            Remarks = f.Remarks,
                        }).FirstOrDefault();
             var data = invoice;
             return Json(new { data = data }, JsonRequestBehavior.AllowGet);
         }
         public ActionResult GetPriceDetailById(string id)
         {
             List<clsInvoice> invoiceDetail = new List<clsInvoice>();
             invoiceDetail = (from f in db.Invoice_Detail
                              where f.ComputerNo == id
                              select new clsInvoice
                              {
                                  ItemID = f.ItemID,
                                  SerialNo=f.SerialNo ,
                                  ItemName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                  UrduName = db.Item_Information.Where(x => x.ItemID == f.ItemID).Select(x => x.UrduName).FirstOrDefault(),
                                  Rate = f.Rate,
                                  LineNetRate = f.NetRate,
                              }).OrderBy(x=>x.SerialNo).ToList();
             var data = invoiceDetail.ToList();
             return Json(new { data = data }, JsonRequestBehavior.AllowGet);
         }
        #endregion
        #region Manual Price Report
        public ActionResult PriceListReport(string id)
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
                               RecordDate = f.RecordDate,
                               RefrenceNo = f.RefrenceNo,
                               CreditDays = f.CreditDays,
                               DiscountType = f.DiscountType,
                               Discount = f.Discount,
                               NetDiscount = f.NetDiscount,
                               Overhead = f.Overhead,
                               Freight = f.Freight,
                               Remarks = f.Remarks,
                               Received = f.Received,
                               ReceivedDetailCode = f.ReceivedDetailCode,
                               ReceivedAmount = f.ReceivedAmount,
                               ReceivedRemarks = f.ReceivedRemarks,
                               BalanceAmount = f.BalanceAmount,
                               PreviousBalance = f.PreviousBalance,
                               GrossTotal = f.GrossTotal,
                               NetAmount = f.NetAmount,
                               Status = f.Status,
                               DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                               Address = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Address).FirstOrDefault() ?? "",
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
                               //ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                               Barcode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.UrduName).FirstOrDefault() ?? "",
                               BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                               WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                               Rate = fd.Rate,
                               Qty = fd.Qty,
                               ItemNote = fd.ItemNote,
                               LineDiscountType = f.DiscountType ?? string.Empty,
                               LineDiscount = f.Discount ?? 0,
                               Tax1Value = fd.Tax1Value,
                               Tax2Value = fd.Tax2Value,
                               LineNetRate = fd.NetRate,
                               LineGrossAmount = fd.GrossValue,
                               LineNetAmount = fd.NetValue
                           }).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion


        #region Manual Product Price Sale
        public ActionResult ManualProductPriceSale()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "ManualSalePrice"
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
        [HttpPost]
        public ActionResult UpdateItemManualPriceSale(string data)
        {
            string message = "";
            bool status = false;
            int returnId = 0;
            List<clsItem> items = JsonConvert.DeserializeObject<List<clsItem>>(data);
            if (items.Count == 0)
            {
                status = false;
                message = "There is no record to be saved.";
                return new JsonResult { Data = new { status = status, message = message } };
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ItemID");
            dt.Columns.Add("CostPrice");
            dt.Columns.Add("SalePrice");
            for (int i = 0; i < items.Count; i++)
            {
                dt.Rows.Add(new object[] { i + 1, items[i].ItemID, 0, items[i].SalePrice });
            }
            returnId = AddEditSale(dt);
            if (returnId > 0)
            {
                ModelState.Clear();
                status = true;
                message = "Record Successfully Updated";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        private int AddEditSale(DataTable dt)
        {
            int returnId = 0;
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spUpdateItemManualPriceSale", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@dtPrice", SqlDbType.Structured).Value = dt;
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
        
        public ActionResult ManualPricesSaleList()
        {
            return PartialView();
        }
        public ActionResult GetAllManualPricesSaleLists()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsInvoice> invoice = (from f in db.Invoice_Main
                                            join fis in db.Admin_Setup_FiscalYear on f.FiscalYearID equals fis.FiscalYearID
                                            where fis.FiscalYearID == User.FiscalYearId && f.Type == "Manual Price Sale"
                                            select new clsInvoice
                                            {
                                                ComputerNo = f.ComputerNo,
                                                RecordDate = f.RecordDate,
                                                Remarks = f.Remarks,
                                            }).OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.ComputerNo).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(remarks))
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


        [HttpPost]
        public ActionResult InsertUpdateItemsPricesSale(string data)
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
            if (invoice[0].ComputerNo != "")
            {

                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, 0, invoice[i].Rate, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0, "" });
                }
                returnId = AddEditInvoice(invoice[0].ComputerNo, "Manual Price Sale", DateTime.Now, 0, 0, "", invoice[0].Remarks, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Update");
                if (returnId == invoiceNo)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Updated";
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
                string maxCode = getMaxCodeForPriceSaleList();
                invoiceNo = maxCode;
                for (int i = 0; i < invoice.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, invoice[i].ItemID, 0, invoice[i].Rate, "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, 0, "" });
                }
                returnId = AddEditInvoice(maxCode, "Manual Price Sale", DateTime.Now, 0, 0, "", invoice[0].Remarks, 0, "", 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "Unpost", 0, User.UserId, User.UserId, DateTime.Now, DateTime.Now, 0, 0, User.FiscalYearId, 0, "", "", "", DateTime.Now, 0, 0, "", 0, "", 0, dt, "Save");
                invoiceNo = returnId;
                if (returnId != "0")
                {
                    ModelState.Clear();
                    status = true;
                    message = "Invoice Successfully Saved";
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
        public string getMaxCodeForPriceSaleList()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string invoiceCode = "MS-" + currentYear + currentMonth;
            int count = db.Invoice_Main.Where(x => x.Type == "Manual Price Sale").Count();
            if (count > 0)
            {
                maxCode = db.Invoice_Main.Where(x => x.Type == "Manual Price Sale").OrderByDescending(x => x.ComputerNo).Select(x => x.ComputerNo).FirstOrDefault();
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

            return maxCode;
        }
        
        #endregion


        #region Manual Price Sale Report
        public ActionResult PriceListSaleReport(string id)
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
                               RecordDate = f.RecordDate,
                               RefrenceNo = f.RefrenceNo,
                               CreditDays = f.CreditDays,
                               DiscountType = f.DiscountType,
                               Discount = f.Discount,
                               NetDiscount = f.NetDiscount,
                               Overhead = f.Overhead,
                               Freight = f.Freight,
                               Remarks = f.Remarks,
                               Received = f.Received,
                               ReceivedDetailCode = f.ReceivedDetailCode,
                               ReceivedAmount = f.ReceivedAmount,
                               ReceivedRemarks = f.ReceivedRemarks,
                               BalanceAmount = f.BalanceAmount,
                               PreviousBalance = f.PreviousBalance,
                               GrossTotal = f.GrossTotal,
                               NetAmount = f.NetAmount,
                               Status = f.Status,
                               DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                               Address = db.AC_Detail_Extra.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Address).FirstOrDefault() ?? "",
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
                               //ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                               Barcode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.UrduName).FirstOrDefault() ?? "",
                               BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                               WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                               Rate = fd.Rate,
                               Qty = fd.Qty,
                               ItemNote = fd.ItemNote,
                               LineDiscountType = f.DiscountType ?? string.Empty,
                               LineDiscount = f.Discount ?? 0,
                               Tax1Value = fd.Tax1Value,
                               Tax2Value = fd.Tax2Value,
                               LineNetRate = fd.NetRate,
                               LineGrossAmount = fd.GrossValue,
                               LineNetAmount = fd.NetValue
                           }).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion

        #region Pls Summary Report(Profit Loss Statement)
        public ActionResult PlsSummaryReport()
        {
            ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
            return View();
        }

        [HttpPost]
        public ActionResult PrintPlsSummaryReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);
                String[] City = Request.Form.GetValues("CityId");

                string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
                SqlConnection con = new SqlConnection(connection);
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("PLS_Sale_Summary", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate;
                cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate;
                SqlDataReader r = cmd.ExecuteReader();
                DataTable tdt = new DataTable();
                tdt.Load(r);

                r.Close();
                con.Close();
                DataTableReader dt = tdt.CreateDataReader();
                List<clsInvoice> invoice = new List<clsInvoice>();
                while (dt.Read())
                {
                    invoice.Add(new clsInvoice()
                    {
                        DetailCode = Convert.ToDecimal(dt["DetailCode"]),
                        DetailName = dt["Name"].ToString(),
                        CityId = Convert.ToInt32(dt["CityId"]),
                        CityName = dt["CityName"].ToString(),
                        SalesmanID = Convert.ToInt32(dt["SalesmanID"]),
                        SalesmanName = dt["SalesmanName"].ToString(),

                        NetSale= Convert.ToDecimal(dt["NetSale"]),
                        NetCost = Convert.ToDecimal(dt["NetCost"]),
                        Breakage = Convert.ToDecimal(dt["Breakage"]),
                        Coupen = Convert.ToDecimal(dt["Coupen"]),
                        Discount = Convert.ToDecimal(dt["Discount"]),
                        Other = Convert.ToDecimal(dt["Other"]),
                        ReceivedAmount = Convert.ToDecimal(dt["Received"]),
                        FromDate=fromDate ,
                        ToDate=toDate ,
                    });
                }
                SqlConnection.ClearPool(con);
                
                List<clsInvoice> invoiceList = new List<clsInvoice>();
                List<clsInvoice> finalList = new List<clsInvoice>();

                if (cust != null)
                {
                    for (int i = 0; i < cust.Length; i++)
                    {
                        decimal customerId = Convert.ToDecimal(cust[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        invoiceList = invoice.ToList();
                        list = invoiceList.Where(x => x.DetailCode == customerId).ToList();
                        finalList.AddRange(list);
                    }
                }
                else
                {
                    invoiceList = invoice.ToList();
                    if (City != null)
                    {
                        int j = 0;
                        for (int i = 0; i < City.Length; i++)
                        {
                            int cityId = Convert.ToInt32(City[i]);
                            List<clsInvoice> list = new List<clsInvoice>();
                            invoiceList = invoice.ToList();
                            list = invoiceList.Where(x => x.CityId == cityId).ToList();
                            finalList.AddRange(list);
                        }
                    }
                    else
                    {
                        finalList = invoiceList.ToList();
                    }

                    if (salesmanID != null && salesmanID != 0)
                    {
                        finalList.Clear();
                        List<clsInvoice> list = new List<clsInvoice>();
                        invoiceList = finalList.ToList();
                        list = invoiceList.Where(x => x.SalesmanID == salesmanID).ToList();
                        finalList.AddRange(list);
                    }
                }

                finalList = finalList.OrderBy(x => x.DetailName).ThenBy(x => x.DetailCode).ThenBy(x => x.SerialNo).ToList();

                return View(finalList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion

        
        #region Pls Report(Profit Loss Statement)
        public List<clsItem> getCategories()
        {
            List<clsItem> category = new List<clsItem>();
            category = (from c in db.Item_Category
                        where c.Status == "Active"
                        select new clsItem
                        {
                            CategoryID = c.CategoryID,
                            CategoryName = c.Description
                        }).ToList();
            return category;
        }
        public ActionResult PlsReport()
        {
            ViewBag.Customers = new SelectList(getCustomers(), "DetailCode", "Name");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            ViewBag.Salesman = new SelectList(getSalesman(), "SalesmanID", "FullName");
            ViewBag.City = new SelectList(getCities(), "CityId", "CityName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintPlsReport()
        {
            if (User != null)
            {
                String[] cust = Request.Form.GetValues("DetailCode");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                string computerNo = Request.Form["ComputerNo"];
                string referenceNo = Request.Form["ReferenceNo"];
                string reportType = Request.Form["ReportType"].ToString();
                String[] items = Request.Form.GetValues("ItemID");
                //int itemID = Convert.ToInt32(Request.Form["ItemID"]);
                int warehouseID = Convert.ToInt32(Request.Form["WarehouseID"]);
                int salesmanID = Convert.ToInt32(Request.Form["SalesmanID"]);

                String[] City = Request.Form.GetValues("CityId");
                String[] categories = Request.Form.GetValues("CategoryID");

                List<clsInvoice> invoiceList = new List<clsInvoice>();

                List<clsVoucher> voucherDetail = new List<clsVoucher>();
                voucherDetail= GetCreditBreakage();
                voucherDetail=voucherDetail.Where(x=> x.VoucherDate >= fromDate && x.VoucherDate<= toDate).ToList();
                List<clsVoucher> voucherDetailSub = new List<clsVoucher>();
                List<clsVoucher> voucherDetailFilter = new List<clsVoucher>();
                List<clsVoucher> voucherDetailTemp = new List<clsVoucher>();

                if (cust != null)
                {
                    for (int i = 0; i < cust.Length; i++)
                    {
                        decimal customerId = Convert.ToDecimal(cust[i]);
                        List<clsInvoice> list = GetInvoicesListForPls(customerId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);

                        voucherDetailFilter=voucherDetail.Where(x=> x.DetailCode == customerId).ToList();
                        voucherDetailSub.AddRange(voucherDetailFilter);
                    }
                }
                else
                {
                    List<clsAccountDetail> customers = getCustomers();
                    for (int i = 0; i < customers.Count; i++)
                    {
                        decimal customerId = Convert.ToDecimal(customers[i].DetailCode);
                        List<clsInvoice> list = GetInvoicesListForPls(customerId, fromDate, toDate, referenceNo, computerNo, items, warehouseID, salesmanID, reportType);
                        invoiceList.AddRange(list);
                    }

                    voucherDetailSub.AddRange(voucherDetail);
                }



                List<clsInvoice> finalList = new List<clsInvoice>();
                if (City != null)
                {

                    voucherDetailTemp=voucherDetailSub.ToList();
                    voucherDetailSub.Clear();

                    int j = 0;
                    for (int i = 0; i < City.Length; i++)
                    {
                        int cityId = Convert.ToInt32(City[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = invoiceList.Where(x => x.CityId == cityId).ToList();
                        finalList.AddRange(list);

                        voucherDetailFilter=voucherDetailTemp.Where(x=> x.CityId == cityId).ToList();
                        voucherDetailSub.AddRange(voucherDetailFilter);
                    }
                }
                else
                {
                    finalList = invoiceList.ToList();
                }


                List<clsInvoice> finalListcateg = new List<clsInvoice>();
                if (categories != null)
                {
                    int j = 0;
                    for (int i = 0; i < categories.Length; i++)
                    {
                        int categid = Convert.ToInt32(categories[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = finalList.Where(x => x.CategoryID == categid).ToList();
                        finalListcateg.AddRange(list);
                    }
                }
                else
                {
                    finalListcateg = finalList;
                }

                finalListcateg = finalListcateg.OrderBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ThenBy(x => x.SerialNo).ToList();
                if (finalListcateg.Count > 0)
                {
                    finalListcateg[0].CreditNoteList = voucherDetailSub.ToList();
                }
                                
                return View(finalListcateg);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public List<clsVoucher> GetCreditBreakage()
        {
            List<clsVoucher> voucherDetail = new List<clsVoucher>();
            voucherDetail = (from f in db.AC_CashBank_Main
                             where f.VoucherType == "Credit Breakage" 
                             select new clsVoucher
                             {
                                 VoucherNo = f.VoucherNo,
                                 DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault() + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == f.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                                 CityName = (from s in db.Cities join sd in db.AC_Detail_Extra on s.CityId equals sd.CityId where sd.DetailCode == f.DetailCode select s.CityName).FirstOrDefault() ?? "",
                                 CityId = (from e in db.AC_Detail_Extra where e.DetailCode == f.DetailCode select e.CityId).FirstOrDefault() ?? 0,
                                 VoucherDate=f.VoucherDate, 
                                 DetailCode = f.DetailCode,
                                 Remarks = f.Remarks ?? string.Empty,
                                 Credit = f.Credit,
                             }).OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ToList();
            var data = voucherDetail.ToList();
            return data;
        }

        public List<clsInvoice> GetInvoicesListForPls(decimal customerId, DateTime fromDate, DateTime toDate, string referenceNo, string computerNo, String[] items, int warehouseId, int salesmanId, string reportType)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> invoiceList = new List<clsInvoice>();
            string type1 = "";
            string type2 = "";
            if (reportType == "Sale Invoice + Sale Return")
            {
                type1 = "Sale";
                type2 = "Sale Return";
            }
            else
            {
                type1 = reportType;
                type2 = reportType;
            }
            if (items == null && warehouseId == 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && (f.Type == type1 || f.Type == type2)
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            RecordDate = f.RecordDate,
                            RefrenceNo = f.RefrenceNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityName = (from s in db.Cities join sd in db.AC_Detail_Extra on s.CityId equals sd.CityId where sd.DetailCode == customerId select s.CityName).FirstOrDefault() ?? "",
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                            SerialNo = fd.SerialNo,
                            Qty = fd.Qty ?? 0,
                            UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            Rate = fd.Rate ?? 0,
                            AvgRate = fd.AvgRate ?? 0,
                            GrossValue = fd.GrossValue ?? 0,
                            NetValue = fd.NetValue ?? 0,
                            SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).ToList();
                invoiceList.AddRange(list);
            }
            else if (items != null && warehouseId == 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == customerId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && (f.Type == type1 || f.Type == type2)
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                                CityName = (from s in db.Cities join sd in db.AC_Detail_Extra on s.CityId equals sd.CityId where sd.DetailCode == customerId select s.CityName).FirstOrDefault() ?? "",
                                CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                                CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                SerialNo = fd.SerialNo,
                                Qty = fd.Qty ?? 0,
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                Rate = fd.Rate ?? 0,
                                AvgRate = fd.AvgRate ?? 0,
                                GrossValue = fd.GrossValue ?? 0,
                                NetValue = fd.NetValue ?? 0,
                                SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).ToList();
                    invoiceList.AddRange(list);
                }
            }
            else if (items == null && warehouseId != 0)
            {
                list = (from f in db.Invoice_Main
                        join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                        where
                        f.DetailCode == customerId
                        && f.RecordDate >= fromDate
                        && f.RecordDate <= toDate
                        && f.RefrenceNo.Contains(referenceNo)
                        && f.ComputerNo.Contains(computerNo)
                        && fd.WarehouseID == warehouseId
                        && (f.Type == type1 || f.Type == type2)
                        select new clsInvoice
                        {
                            ComputerNo = f.ComputerNo,
                            RecordDate = f.RecordDate,
                            RefrenceNo = f.RefrenceNo,
                            FromDate = fromDate,
                            ToDate = toDate,
                            DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                            CityName = (from s in db.Cities join sd in db.AC_Detail_Extra on s.CityId equals sd.CityId where sd.DetailCode == customerId select s.CityName).FirstOrDefault() ?? "",
                            CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                            CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                            ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                            //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                            SerialNo = fd.SerialNo,
                            Qty = fd.Qty ?? 0,
                            UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                            Rate = fd.Rate ?? 0,
                            AvgRate = fd.AvgRate ?? 0,
                            GrossValue = fd.GrossValue ?? 0,
                            NetValue = fd.NetValue ?? 0,
                            SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                        }).ToList();
                invoiceList.AddRange(list);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    decimal itemId = Convert.ToDecimal(items[i]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where
                            f.DetailCode == customerId
                            && f.RecordDate >= fromDate
                            && f.RecordDate <= toDate
                            && f.RefrenceNo.Contains(referenceNo)
                            && f.ComputerNo.Contains(computerNo)
                            && fd.ItemID == itemId
                            && fd.WarehouseID == warehouseId
                            && (f.Type == type1 || f.Type == type2)
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == customerId).Select(x => x.Name).FirstOrDefault(),
                                CityName = (from s in db.Cities join sd in db.AC_Detail_Extra on s.CityId equals sd.CityId where sd.DetailCode == customerId select s.CityName).FirstOrDefault() ?? "",
                                CityId = (from e in db.AC_Detail_Extra where e.DetailCode == customerId select e.CityId).FirstOrDefault() ?? 0,
                                CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault() ?? 0,
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                //WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault(),
                                SerialNo = fd.SerialNo,
                                Qty = fd.Qty ?? 0,
                                UnitName = (from k in db.Item_Information join u in db.Item_UOM on k.UomId equals u.UomId where k.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? string.Empty,
                                Rate = fd.Rate ?? 0,
                                AvgRate = fd.AvgRate ?? 0,
                                GrossValue = fd.GrossValue ?? 0,
                                NetValue = fd.NetValue ?? 0,
                                SalesmanID = db.AC_Detail_Extra.Where(x => x.DetailCode == customerId).Select(x => x.SalesmanID).FirstOrDefault() ?? 0,
                            }).ToList();
                    invoiceList.AddRange(list);
                }
            }
            if (salesmanId != 0)
            {
                invoiceList = invoiceList.Where(x => x.SalesmanID == salesmanId).ToList();
            }
            return invoiceList;
        }
        #endregion
    }
}