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

namespace POS.Controllers
{
    public class ItemController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Item
        public ActionResult ItemIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "Item"
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
                    //UomId UnitName CategoryID CategoryName BrandID BrandName
                    ViewBag.Unit = new SelectList(getUnitOfMeasures(), "UomId", "UnitName");
                    ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
                    ViewBag.SubCategory = new SelectList(getSubCategories(), "SubCategoryID", "SubCategoryName");
                    ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateItem(clsItem item)
        {
            string message = "";
            bool status = false;
            if(item.BrandID == null)
            {
                item.BrandID = 0;
            }
            if (item.ItemID > 0)
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateItem(item.ItemID, item.ItemName, item.UrduName, item.Barcode, item.UomId, item.CategoryID, item.SubCategoryID, item.BrandID, item.CostPrice, item.SalePrice,item.MinSalePrice, item.MinLevel, item.MaxLevel, item.MinOrder, item.Status, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Item Successfully Updated";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            else
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateItem(item.ItemID, item.ItemName, item.UrduName, item.Barcode, item.UomId, item.CategoryID,item.SubCategoryID, item.BrandID, item.CostPrice, item.SalePrice,item.MinSalePrice, item.MinLevel, item.MaxLevel, item.MinOrder, "Active", User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Item Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult GetItemById(int id)
        {
            clsItem item = new clsItem();
            item = (from f in db.Item_Information
                    where f.ItemID == id
                    select new clsItem
                         {
                             ItemID = f.ItemID,
                             ItemName = f.ItemName,
                             UrduName = f.UrduName,
                             Barcode = f.Barcode,
                             UomId = f.UomId,
                             CategoryID = f.CategoryID,
                             SubCategoryID = f.SubCategoryID,
                             BrandID = f.BrandID,
                             CostPrice = f.CostPrice,
                             SalePrice = f.SalePrice,
                             MinSalePrice = f.MinSalePrice,
                             MinLevel = f.MinLevel,
                             MaxLevel = f.MaxLevel,
                             MinOrder = f.MinOrder,
                             Status = f.Status
                         }).FirstOrDefault();
            var data = item;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public List<clsItem> getUnitOfMeasures()
        {
            List<clsItem> unit = new List<clsItem>();
            unit = (from c in db.Item_UOM
                    where c.Status == "Active"
                    select new clsItem
                          {
                              UomId = c.UomId,
                              UnitName = c.Description
                          }).ToList();
            return unit;
        }
        
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
        public List<clsItem> getBrands()
        {
            List<clsItem> brand = new List<clsItem>();
            brand = (from c in db.Item_Brand
                    where c.Status == "Active"
                    select new clsItem
                    {
                        BrandID = c.BrandID,
                        BrandName = c.Description
                    }).ToList();
            return brand;
        }
        public ActionResult ItemsList()
        {
            return PartialView();
        }
        public ActionResult GetAllItems()
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
                var unit = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var category = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var brand = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
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
                                           UomId = f.UomId,
                                           UnitName = db.Item_UOM.Where(x => x.UomId == f.UomId).Select(x => x.Description).FirstOrDefault(),
                                           CategoryID = f.CategoryID,
                                           CategoryName = db.Item_Category.Where(x => x.CategoryID == f.CategoryID).Select(x => x.Description).FirstOrDefault(),
                                           SubCategoryID = f.SubCategoryID,
                                           SubCategoryName = db.Item_SubCategory.Where(x => x.SubCategoryID == f.SubCategoryID).Select(x => x.Description).FirstOrDefault(),
                                           BrandID = f.BrandID,
                                           BrandName = db.Item_Brand.Where(x => x.BrandID == f.BrandID).Select(x => x.Description).FirstOrDefault(),
                                           CostPrice = f.CostPrice,
                                           SalePrice = f.SalePrice,
                                           MinSalePrice = f.MinSalePrice,
                                           MinLevel = f.MinLevel,
                                           MaxLevel = f.MaxLevel,
                                           MinOrder = f.MinOrder,
                                           Status = f.Status
                                       }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(itemName))
                {
                    items = items.Where(c => c.ItemName.ToLower().Contains(itemName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(urduName))
                {
                    items = items.Where(c => c.UrduName.Contains(urduName)).ToList();
                }
                else if (!string.IsNullOrEmpty(unit))
                {
                    items = items.Where(c => c.UnitName.ToLower().Contains(unit.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(category))
                {
                    items = items.Where(c => c.CategoryName.ToLower().Contains(category.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(brand))
                {
                    items = items.Where(c => c.BrandName.ToLower().Contains(brand.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    items = items.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
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
        public ActionResult DeleteItem(int id)
        {
            string message = "";
            bool status = false;
            clsItem item = new clsItem();
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateItem(id, "", "", "", 0, 0,0, 0, 0, 0, 0, 0, 0, 0, "Active", User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Item Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this item first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        #endregion
        #region Reports
        #region Item List Report
        public ActionResult ItemListReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintItemsList()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");

                List<clsItem> itemsList = itemsList = GetItemsForItemsListReport(items, categories, brands);
                itemsList = itemsList.OrderBy(x => x.ItemName).ToList();
                return View(itemsList);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsItem> GetItemsForItemsListReport(String[] items, String[] categories, String[] brands)
        {
            List<clsItem> list = new List<clsItem>();
            List<clsItem> list2 = new List<clsItem>();
            List<clsItem> list3 = new List<clsItem>();
            List<clsItem> list4 = new List<clsItem>();
            if (items == null)
            {
                list = (from f in db.Item_Information
                        select new clsItem
                        {
                            ItemID = f.ItemID,
                            ItemName = f.ItemName,
                            UrduName = f.UrduName,
                            Barcode = f.Barcode,
                            UomId = f.UomId,
                            UnitName = db.Item_UOM.Where(x => x.UomId == f.UomId).Select(x => x.Description).FirstOrDefault(),
                            CategoryID = f.CategoryID,
                            CategoryName = db.Item_Category.Where(x => x.CategoryID == f.CategoryID).Select(x => x.Description).FirstOrDefault(),
                            SubCategoryID = f.SubCategoryID,
                            SubCategoryName = db.Item_SubCategory.Where(x => x.SubCategoryID == f.SubCategoryID).Select(x => x.Description).FirstOrDefault(),
                            BrandID = f.BrandID,
                            BrandName = db.Item_Brand.Where(x => x.BrandID == f.BrandID).Select(x => x.Description).FirstOrDefault(),
                            CostPrice = f.CostPrice,
                            SalePrice = f.SalePrice,
                            MinLevel = f.MinLevel,
                            MaxLevel = f.MaxLevel,
                            MinOrder = f.MinOrder,
                            Status = f.Status,
                        }).ToList();
                list2.AddRange(list);
                list3.AddRange(list);
                list4.AddRange(list);
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    int itemId = Convert.ToInt32(items[i]);
                    list = (from f in db.Item_Information
                            where f.ItemID == itemId
                            select new clsItem
                            {
                                ItemID = f.ItemID,
                                ItemName = f.ItemName,
                                UrduName = f.UrduName,
                                Barcode = f.Barcode,
                                UomId = f.UomId,
                                UnitName = db.Item_UOM.Where(x => x.UomId == f.UomId).Select(x => x.Description).FirstOrDefault(),
                                CategoryID = f.CategoryID,
                                CategoryName = db.Item_Category.Where(x => x.CategoryID == f.CategoryID).Select(x => x.Description).FirstOrDefault(),
                                SubCategoryID = f.SubCategoryID,
                                SubCategoryName = db.Item_SubCategory.Where(x => x.SubCategoryID == f.SubCategoryID).Select(x => x.Description).FirstOrDefault(),
                                BrandID = f.BrandID,
                                BrandName = db.Item_Brand.Where(x => x.BrandID == f.BrandID).Select(x => x.Description).FirstOrDefault(),
                                CostPrice = f.CostPrice,
                                SalePrice = f.SalePrice,
                                MinLevel = f.MinLevel,
                                MaxLevel = f.MaxLevel,
                                MinOrder = f.MinOrder,
                                Status = f.Status,
                            }).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                }
            }
            if (categories != null)
            {
                int j = 0;
                for (int i = 0; i < categories.Length; i++)
                {
                    int categoryId = Convert.ToInt32(categories[i]);
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsItem>();
                        list4 = new List<clsItem>();
                    }
                    list = new List<clsItem>();
                    list = list2.Where(x => x.CategoryID == categoryId).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    j++;
                    //}
                }
            }
            if (brands != null)
            {
                int j = 0;
                for (int i = 0; i < brands.Length; i++)
                {
                    int brandId = Convert.ToInt32(brands[i]);
                    //if (list3.Any(x => x.BrandID == brandId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsItem>();
                    }
                    list = new List<clsItem>();
                    list = list3.Where(x => x.BrandID == brandId).ToList();
                    list4.AddRange(list);
                    j++;
                    //}
                }
            }
            return list4;
        }
        #endregion
        #region Item Overall Stock
        public ActionResult ItemOverallStockReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintItemsOverallStock()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");
                var checkbox = Request.Form.GetValues("UntilDateCheckbox");
                string Negcheckbox = Request.Form["NeagtiveStockCheckbox"];

                DateTime? untilDate = null;
                if (checkbox != null)
                {
                    untilDate = Convert.ToDateTime(Request.Form["UntilDate"]);
                }
                List<clsItem> list = new List<clsItem>();
                List<clsItem> list2 = new List<clsItem>();
                List<clsItem> list3 = new List<clsItem>();
                List<clsItem> list4 = new List<clsItem>();

                if (Negcheckbox == null)
                {
                    Negcheckbox = "";
                }

                if (items != null)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        int itemId = Convert.ToInt32(items[i]);
                        list = GetItemsOverallStock(itemId, categories, brands, untilDate,Negcheckbox );
                        list2.AddRange(list);
                        list3.AddRange(list);
                        list4.AddRange(list);
                    }
                }
                else
                {
                    list = GetItemsOverallStock(0, categories, brands, untilDate,Negcheckbox);
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                    //List<clsItem> Items = getItems();
                    //for (int i = 0; i < Items.Count; i++)
                    //{
                    //    int itemId = Convert.ToInt32(Items[i].ItemID);
                    //    list = GetItemsOverallStock(itemId, categories, brands, untilDate);
                    //    list2.AddRange(list);
                    //    list3.AddRange(list);
                    //    list4.AddRange(list);
                    //}
                }

                



                if (categories != null)
                {
                    int j = 0;
                    for (int i = 0; i < categories.Length; i++)
                    {
                        int categoryId = Convert.ToInt32(categories[i]);
                        //if (list2.Any(x => x.CategoryID == categoryId))
                        //{
                        if (j == 0)
                        {
                            list3 = new List<clsItem>();
                            list4 = new List<clsItem>();
                        }
                        list = new List<clsItem>();
                        list = list2.Where(x => x.CategoryID == categoryId).ToList();
                        list3.AddRange(list);
                        list4.AddRange(list);
                        j++;
                        //}
                    }
                }
                if (brands != null)
                {
                    int j = 0;
                    for (int i = 0; i < brands.Length; i++)
                    {
                        int brandId = Convert.ToInt32(brands[i]);
                        //if (list3.Any(x => x.BrandID == brandId))
                        //{
                        if (j == 0)
                        {
                            list4 = new List<clsItem>();
                        }
                        list = new List<clsItem>();
                        list = list3.Where(x => x.BrandID == brandId).ToList();
                        list4.AddRange(list);
                        j++;
                        //}
                    }
                }
                list4 = list4.OrderBy(x => x.ItemName).ToList();
                return View(list4);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsItem> GetItemsOverallStock(int itemId, String[] categories, String[] brands, DateTime? untilDate, string NegativeStock)
        {
            List<clsItem> list = new List<clsItem>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            if (untilDate == null)
            {
                untilDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
            }
            if (itemId == 0)
            {
                cmd.CommandText = "select  v.ItemID,v.ItemName,v.UrduName,v.CategoryID,v.BrandID ,v.BrandName,v.CategoryName,v.UOM1Name,v.Stock, isnull(Sum(d.InQty),0) as 'InQty',isnull(Sum(d.OutQty),0) as 'OutQty' from View_Item_Opening_Stock v  left outer join Invoice_Detail d on v.ItemID = D.ItemID AND v.FiscalYearID = D.FiscalYearID  and d.RecordDate < '" + untilDate + "'  group by v.ItemID ,v.ItemName,v.UrduName,v.BrandName,v.CategoryName,v.UOM1Name,v.Stock,v.CategoryID,v.BrandID  HAVING v.Stock+(isnull(Sum(d.InQty),0)-isnull(Sum(d.OutQty),0)) <> 0";
            }
            else
            {
                cmd.CommandText = "select  v.ItemID,v.ItemName,v.UrduName,v.CategoryID,v.BrandID ,v.BrandName,v.CategoryName,v.UOM1Name,v.Stock, isnull(Sum(d.InQty),0) as 'InQty',isnull(Sum(d.OutQty),0) as 'OutQty' from View_Item_Opening_Stock v  left outer join Invoice_Detail d on v.ItemID = D.ItemID AND v.FiscalYearID = D.FiscalYearID  and d.RecordDate < '" + untilDate + "' where v.ItemID = " + itemId + "   group by v.ItemID ,v.ItemName,v.UrduName,v.BrandName,v.CategoryName,v.UOM1Name,v.Stock,v.CategoryID,v.BrandID  HAVING v.Stock+(isnull(Sum(d.InQty),0)-isnull(Sum(d.OutQty),0)) <> 0";
            }
            cmd.Connection = con;
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
                    ItemID = Convert.ToInt32(dt["ItemID"]),
                    ItemName = dt["ItemName"].ToString(),
                    Barcode = dt["UrduName"].ToString(),
                    BrandName = dt["BrandName"].ToString(),
                    CategoryName = dt["CategoryName"].ToString(),
                    UnitName = dt["UOM1Name"].ToString(),
                    Stock1 = Convert.ToDecimal(dt["Stock"]),
                    InQty = Convert.ToDecimal(dt["InQty"]),
                    OutQty = Convert.ToDecimal(dt["OutQty"]),
                    BrandID = Convert.ToInt32(dt["BrandID"]),
                    CategoryID = Convert.ToInt32(dt["CategoryID"]),
                    RunStock = Convert.ToDecimal(dt["Stock"]) + Convert.ToDecimal(dt["InQty"]) - Convert.ToDecimal(dt["OutQty"]),
                    SaleDate = untilDate
                });
            }


            if (NegativeStock == "on" || NegativeStock == "1")
            {
                list = list.Where(x => x.RunStock < 0).ToList(); 
            }

            return list;
        }
        #endregion
        #region Item Stock Warehouse Base
        public ActionResult ItemStockWarehouseBaseReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintItemsStockWarehouseBase()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");
                String[] warehouses = Request.Form.GetValues("WarehouseID");
                var checkbox = Request.Form.GetValues("UntilDateCheckbox");
                DateTime? untilDate = null;
                if (checkbox != null)
                {
                    untilDate = Convert.ToDateTime(Request.Form["UntilDate"]);
                }
                clsItem item = new clsItem();
                List<clsItem> list = new List<clsItem>();
                List<clsItem> list2 = new List<clsItem>();
                List<clsItem> list3 = new List<clsItem>();
                List<clsItem> list4 = new List<clsItem>();
                List<clsItem> list5 = new List<clsItem>();
                if (items != null)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        int itemId = Convert.ToInt32(items[i]);
                        list = GetItemsStockWarehouseBase(itemId , 0 , untilDate);
                        list2.AddRange(list);
                        list3.AddRange(list);
                        list4.AddRange(list);
                        list5.AddRange(list);
                    }
                }
                else
                {
                    for (int i = 0; i < warehouses.Length; i++)
                    {
                        int warehouseId = Convert.ToInt32(warehouses[i]);
                        list = GetItemsStockWarehouseBase(0 , warehouseId , untilDate);
                        list2.AddRange(list);
                        list3.AddRange(list);
                        list4.AddRange(list);
                        list5.AddRange(list);
                    }
                    //List<clsItem> Items = getItems();
                    //for (int i = 0; i < Items.Count; i++)
                    //{
                    //    int itemId = Convert.ToInt32(Items[i].ItemID);
                    //    list = GetItemsStockWarehouseBase(itemId, untilDate);
                    //    list2.AddRange(list);
                    //    list3.AddRange(list);
                    //    list4.AddRange(list);
                    //    list5.AddRange(list);
                    //}
                }
                if (categories != null)
                {
                    int j = 0;
                    for (int i = 0; i < categories.Length; i++)
                    {
                        int categoryId = Convert.ToInt32(categories[i]);
                        //if (list2.Any(x => x.CategoryID == categoryId))
                        //{
                        if (j == 0)
                        {
                            list3 = new List<clsItem>();
                            list4 = new List<clsItem>();
                            list5 = new List<clsItem>();
                        }
                        list = new List<clsItem>();
                        list = list2.Where(x => x.CategoryID == categoryId).ToList();
                        list3.AddRange(list);
                        list4.AddRange(list);
                        list5.AddRange(list);
                        j++;
                        //}
                    }
                }
                if (brands != null)
                {
                    int j = 0;
                    for (int i = 0; i < brands.Length; i++)
                    {
                        int brandId = Convert.ToInt32(brands[i]);
                        //if (list3.Any(x => x.BrandID == brandId))
                        //{
                        if (j == 0)
                        {
                            list4 = new List<clsItem>();
                            list5 = new List<clsItem>();
                        }
                        list = new List<clsItem>();
                        list = list3.Where(x => x.BrandID == brandId).ToList();
                        list4.AddRange(list);
                        list5.AddRange(list);
                        j++;
                        //}
                    }
                }
                list5 = list5.OrderBy(x => x.ItemName).ToList();
                item.ItemsList = list5;
                item.WarehousesList = list5.GroupBy(x => x.WarehouseID).Select(g => g.First()).ToList();
                item.WarehousesList = item.WarehousesList.OrderBy(x => x.WarehouseName).ToList();
                return View(item);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsItem> GetItemsStockWarehouseBase(int itemId, int warehouseId, DateTime? untilDate)
        {
            List<clsItem> list = new List<clsItem>();
            if (untilDate == null)
            {
                untilDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
            }
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_ItemStockWarehouse", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = untilDate;
            cmd.Parameters.Add("@vFiscalYear", SqlDbType.Int).Value = User.FiscalYearId;
            cmd.Parameters.Add("@vWarehouseID", SqlDbType.Int).Value = warehouseId;
            cmd.Parameters.Add("@vItemID", SqlDbType.Int).Value = itemId;
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
                    ItemID = Convert.ToInt32(dt[0]),
                    ItemName = dt[1].ToString(),
                    Barcode = dt[2].ToString(),
                    CategoryID = Convert.ToInt32(dt[3]),
                    BrandID = Convert.ToInt32(dt[4]),
                    BrandName = dt[5].ToString(),
                    CategoryName = dt[6].ToString(),
                    UnitName = dt[7].ToString(),
                    Stock1 = Convert.ToDecimal(dt[8]),
                    WarehouseID = Convert.ToInt32(dt[9]),
                    WarehouseName = dt[10].ToString(),
                    SaleDate = untilDate
                });
            }
            return list;
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
        #endregion
        #region Items Stock Register Overall
        public ActionResult ItemStockRegisterOverallReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintItemStockRegisterOverall()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                String[] code = Request.Form.GetValues("UrduName");
                var checkbox = Request.Form.GetValues("DateCheckbox");
                if (checkbox == null || fromDate == null || toDate == null)
                {
                    fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
                    toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                clsInvoice invoice = new clsInvoice();
                List<clsInvoice> itemsList = itemsList = GetItemsStockRegisterOverall(items,code, categories, brands, fromDate, toDate);
                itemsList = itemsList.OrderBy(x => x.ItemName).ToList();
                invoice.InvoicesList = itemsList.OrderBy(x => x.ItemName).ThenBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ToList(); 
                invoice.InvoicesList2 = itemsList.GroupBy(x => x.ItemID).Select(g => g.First()).ToList();
                invoice.InvoicesList2 = invoice.InvoicesList2.OrderBy(x => x.ItemName).ThenBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetItemsStockRegisterOverall(String[] items,String[] code, String[] categories, String[] brands, DateTime fromDate, DateTime toDate)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> list2 = new List<clsInvoice>();
            List<clsInvoice> list3 = new List<clsInvoice>();
            List<clsInvoice> list4 = new List<clsInvoice>();
            if (items == null)
            {
                for (int j = 0; j < code.Length; j++)
                {
                    int code1 = Convert.ToInt32(code[j].ToString());
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            join iti in db.Item_Information on fd.ItemID equals iti.ItemID
                            where f.RecordDate >= fromDate && f.RecordDate <= toDate && iti.ItemID == code1 
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                DetailCode = f.DetailCode,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                ItemID = fd.ItemID,
                                ItemCode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.Barcode).FirstOrDefault(),
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault(),
                                BrandID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.BrandID).FirstOrDefault(),
                                CategoryName = (from i in db.Item_Information join c in db.Item_Category on i.CategoryID equals c.CategoryID where i.ItemID == fd.ItemID select c.Description).FirstOrDefault() ?? "",
                                BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                                UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? "",
                                Qty = fd.Qty,
                                InQty = fd.InQty,
                                OutQty = fd.OutQty,
                                PreviousStock1 = db.Item_Opening.Where(x => x.ItemID == fd.ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Stock) + db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate).Sum(x => x.InQty) - db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate).Sum(x => x.OutQty),
                                WarehouseID = fd.WarehouseID,
                            }).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                }
            }
            else
            {
                for (int j = 0; j < items.Length; j++)
                {
                    int itemId = Convert.ToInt32(items[j]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where f.RecordDate >= fromDate && f.RecordDate <= toDate && fd.ItemID == itemId
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                DetailCode = f.DetailCode,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                ItemID = fd.ItemID,
                                ItemCode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.Barcode).FirstOrDefault(),
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault(),
                                BrandID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.BrandID).FirstOrDefault(),
                                CategoryName = (from i in db.Item_Information join c in db.Item_Category on i.CategoryID equals c.CategoryID where i.ItemID == fd.ItemID select c.Description).FirstOrDefault() ?? "",
                                BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                                UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? "",
                                Qty = fd.Qty,
                                InQty = fd.InQty,
                                OutQty = fd.OutQty,
                                PreviousStock1 = db.Item_Opening.Where(x => x.ItemID == fd.ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Stock) + db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate).Sum(x => x.InQty) - db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate).Sum(x => x.OutQty),
                                WarehouseID = fd.WarehouseID,
                            }).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                }
            }
            if (categories != null)
            {
                int j = 0;
                for (int i = 0; i < categories.Length; i++)
                {
                    int categoryId = Convert.ToInt32(categories[i]);
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsInvoice>();
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list2.Where(x => x.CategoryID == categoryId).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    j++;
                    //}
                }
            }
            if (brands != null)
            {
                int j = 0;
                for (int i = 0; i < brands.Length; i++)
                {
                    int brandId = Convert.ToInt32(brands[i]);
                    //if (list3.Any(x => x.BrandID == brandId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list3.Where(x => x.BrandID == brandId).ToList();
                    list4.AddRange(list);
                    j++;
                    //}
                }
            }
            return list4;
        }
        #endregion
        #region Items Stock Register Warehouse Base
        public ActionResult ItemStockRegisterWarehouseBaseReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            ViewBag.Warehouse = new SelectList(getWarehouses(), "WarehouseID", "WarehouseName");
            return View();
        }
        //[HttpPost]
        //public ActionResult PrintItemStockRegisterWarehouseBase()
        //{
        //    var doc1 = new Document();
        //    //use a variable to let my code fit across the page...

        //    string path = Server.MapPath("PDF");

        //    PdfWriter.GetInstance(doc1, new FileStream(path + "/Doc1.pdf", FileMode.Create));
        //    doc1.Open();

        //    doc1.Add(new Paragraph("My first PDF"));

        //    doc1.Close();
        //    return RedirectToAction("Index", "account");
        //}
        [HttpPost]
        public ActionResult PrintItemStockRegisterWarehouseBase()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");
                String[] warehouses = Request.Form.GetValues("WarehouseID");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                var checkbox = Request.Form.GetValues("DateCheckbox");
                if (checkbox == null || fromDate == null || toDate == null)
                {
                    fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
                    toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                clsInvoice invoice = new clsInvoice();
                List<clsInvoice> itemsList = itemsList = GetItemsStockRegisterWarehouseBase(items, categories, brands, warehouses, fromDate, toDate);
                itemsList = itemsList.OrderBy(x => x.ItemName).ToList();
                invoice.InvoicesList = itemsList.OrderBy(x => x.ItemName).ThenBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ToList(); 
                invoice.InvoicesList2 = itemsList.GroupBy(x => x.ItemID).Select(g => g.First()).ToList();
                invoice.InvoicesList2 = invoice.InvoicesList2.OrderBy(x => x.ItemName).ThenBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ToList();
                invoice.WarehousesList = itemsList.GroupBy(x => x.WarehouseID).Select(g => g.First()).ToList();
                invoice.WarehousesList = invoice.WarehousesList.OrderBy(x => x.WarehouseName).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetItemsStockRegisterWarehouseBase(String[] items, String[] categories, String[] brands, String[] warehouses, DateTime fromDate, DateTime toDate)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> list2 = new List<clsInvoice>();
            List<clsInvoice> list3 = new List<clsInvoice>();
            List<clsInvoice> list4 = new List<clsInvoice>();
            List<clsInvoice> list5 = new List<clsInvoice>();
            if (items == null)
            {
                for (int j = 0; j < warehouses.Length; j++)
                {
                    int warehouseId = Convert.ToInt32(warehouses[j]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where f.RecordDate >= fromDate && f.RecordDate <= toDate && fd.WarehouseID == warehouseId
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                DetailCode = f.DetailCode,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                ItemID = fd.ItemID,
                                ItemCode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.Barcode).FirstOrDefault(),
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault(),
                                BrandID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.BrandID).FirstOrDefault(),
                                CategoryName = (from i in db.Item_Information join c in db.Item_Category on i.CategoryID equals c.CategoryID where i.ItemID == fd.ItemID select c.Description).FirstOrDefault() ?? "",
                                BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                                UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? "",
                                Qty = fd.Qty,
                                InQty = fd.InQty,
                                OutQty = fd.OutQty,
                                PreviousStock1 = db.Item_Opening.Where(x => x.ItemID == fd.ItemID && x.FiscalYearID == User.FiscalYearId && x.WarehouseID == fd.WarehouseID).Sum(x => x.Stock) + db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate && x.WarehouseID == fd.WarehouseID).Sum(x => x.InQty) - db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate && x.WarehouseID == fd.WarehouseID).Sum(x => x.OutQty),
                                WarehouseID = fd.WarehouseID,
                                WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault() ?? string.Empty,
                            }).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                }
            }
            else
            {
                for (int j = 0; j < items.Length; j++)
                {
                    int itemId = Convert.ToInt32(items[j]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where f.RecordDate >= fromDate && f.RecordDate <= toDate && fd.ItemID == itemId
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                DetailCode = f.DetailCode,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                ItemID = fd.ItemID,
                                ItemCode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.Barcode).FirstOrDefault(),
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault(),
                                BrandID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.BrandID).FirstOrDefault(),
                                CategoryName = (from i in db.Item_Information join c in db.Item_Category on i.CategoryID equals c.CategoryID where i.ItemID == fd.ItemID select c.Description).FirstOrDefault() ?? "",
                                BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                                UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? "",
                                Qty = fd.Qty,
                                InQty = fd.InQty,
                                OutQty = fd.OutQty,
                                PreviousStock1 = db.Item_Opening.Where(x => x.ItemID == fd.ItemID && x.FiscalYearID == User.FiscalYearId && x.WarehouseID == fd.WarehouseID).Sum(x => x.Stock) + db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate && x.WarehouseID == fd.WarehouseID).Sum(x => x.InQty) - db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate && x.WarehouseID == fd.WarehouseID).Sum(x => x.OutQty),
                                WarehouseID = fd.WarehouseID,
                                WarehouseName = db.Admin_Setup_Warehouse.Where(x => x.WarehouseID == fd.WarehouseID).Select(x => x.WarehouseName).FirstOrDefault() ?? string.Empty,
                            }).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                }
                if (warehouses != null)
                {
                    int j = 0;
                    for (int i = 0; i < warehouses.Length; i++)
                    {
                        int warehouseId = Convert.ToInt32(warehouses[i]);
                        //if (list3.Any(x => x.BrandID == brandId))
                        //{
                        if (j == 0)
                        {
                            list5 = new List<clsInvoice>();
                        }
                        list = new List<clsInvoice>();
                        list = list4.Where(x => x.WarehouseID == warehouseId).ToList();
                        list5.AddRange(list);
                        j++;
                        //}
                    }
                }
            }
            if (categories != null)
            {
                int j = 0;
                for (int i = 0; i < categories.Length; i++)
                {
                    int categoryId = Convert.ToInt32(categories[i]);
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsInvoice>();
                        list4 = new List<clsInvoice>();
                        list5 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list2.Where(x => x.CategoryID == categoryId).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }
            if (brands != null)
            {
                int j = 0;
                for (int i = 0; i < brands.Length; i++)
                {
                    int brandId = Convert.ToInt32(brands[i]);
                    //if (list3.Any(x => x.BrandID == brandId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                        list5 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list3.Where(x => x.BrandID == brandId).ToList();
                    list4.AddRange(list);
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }
            return list5;
        }
        #endregion
        #region Items Stock Status
        public ActionResult ItemStockStatusReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintItemStockStatus()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                String[] code = Request.Form.GetValues("UrduName");
                var checkbox = Request.Form.GetValues("DateCheckbox");
                if (checkbox == null || fromDate == null || toDate == null)
                {
                    fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
                    toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                clsInvoice invoice = new clsInvoice();
                List<clsInvoice> itemsList = itemsList = GetItemsStockStatus(items, code, categories, brands, fromDate, toDate);
                itemsList = itemsList.OrderBy(x => x.ItemName).ToList();
                invoice.InvoicesList = itemsList;
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetItemsStockStatus(String[] items, String[] code, String[] categories, String[] brands, DateTime fromDate, DateTime toDate)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> list2 = new List<clsInvoice>();
            List<clsInvoice> list3 = new List<clsInvoice>();
            List<clsInvoice> list4 = new List<clsInvoice>();
            List<clsInvoice> list5 = new List<clsInvoice>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_ItemStockStatus", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vFromDate", SqlDbType.Date).Value = fromDate;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = toDate;
            cmd.Parameters.Add("@vFiscalYear", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsInvoice()
                {
                    ItemID = Convert.ToInt32(dt[0]),
                    ItemName = dt[1].ToString(),
                    UrduName = dt[2].ToString(),
                    UnitName = dt[8].ToString(),
                    Stock1 = Convert.ToDecimal(dt[9]),
                    InQty = Convert.ToDecimal(dt[10]),
                    OutQty = Convert.ToDecimal(dt[11]),
                    CategoryID = Convert.ToInt32(dt[4]),
                    BrandID = Convert.ToInt32(dt[5]),
                    FromDate = fromDate,
                    ToDate = toDate
                });
            }
            if (items == null)
            {
                list2.AddRange(list);
                list3.AddRange(list);
                list4.AddRange(list);
                list5.AddRange(list);
            }
            else
            {
                for (int j = 0; j < items.Length; j++)
                {
                    int itemId = Convert.ToInt32(items[j]);
                    list = list.Where(x => x.ItemID == itemId).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                }
            }
            if (code != null)
            {
                int j = 0;
                for (int i = 0; i < code.Length; i++)
                {
                    int code1 = Convert.ToInt32(code[j].ToString());
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsInvoice>();
                        list4 = new List<clsInvoice>();
                        list5 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list2.Where(x => x.ItemID == code1).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }
            if (categories != null)
            {
                int j = 0;
                for (int i = 0; i < categories.Length; i++)
                {
                    int categoryId = Convert.ToInt32(categories[i]);
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                        list5 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list3.Where(x => x.CategoryID == categoryId).ToList();
                    list4.AddRange(list);
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }
            if (brands != null)
            {
                int j = 0;
                for (int i = 0; i < brands.Length; i++)
                {
                    int brandId = Convert.ToInt32(brands[i]);
                    //if (list3.Any(x => x.BrandID == brandId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list4.Where(x => x.BrandID == brandId).ToList();
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }
            return list5;
        }
        #endregion
        #region Items Stock With Average Rate
        public ActionResult ItemStockWithAverageRateReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintItemStockWithAverageRate()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");
                String[] code = Request.Form.GetValues("UrduName");
                var checkbox = Request.Form.GetValues("UntilDateCheckbox");
                DateTime? untilDate = null;
                if (checkbox != null)
                {
                    untilDate = Convert.ToDateTime(Request.Form["UntilDate"]);
                }
                else
                {
                    untilDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                clsInvoice invoice = new clsInvoice();
                List<clsInvoice> itemsList = itemsList = GetItemsStockWithAverageRate(items, code, categories, brands, untilDate);
                itemsList = itemsList.OrderBy(x => x.ItemName).ToList();
                invoice.InvoicesList = itemsList;
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetItemsStockWithAverageRate(String[] items, String[] code, String[] categories, String[] brands, DateTime? toDate)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> list2 = new List<clsInvoice>();
            List<clsInvoice> list3 = new List<clsInvoice>();
            List<clsInvoice> list4 = new List<clsInvoice>();

            if (items == null && code == null)
            {
                List<clsItem> itemslist = getItems();
                for (int j = 0; j < itemslist.Count; j++)
                {
                    int vitemid = itemslist[j].ItemID;
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            join it in db.Item_Information on fd.ItemID equals it.ItemID
                            where f.RecordDate <= toDate && it.ItemID.Equals(vitemid)
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                ToDate = toDate,
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemCode = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.Barcode).FirstOrDefault(),
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault(),
                                BrandID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.BrandID).FirstOrDefault(),
                                CategoryName = (from i in db.Item_Information join c in db.Item_Category on i.CategoryID equals c.CategoryID where i.ItemID == g.FirstOrDefault().ItemID select c.Description).FirstOrDefault() ?? "",
                                BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == g.FirstOrDefault().ItemID select b.Description).FirstOrDefault() ?? "",
                                UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? "",
                                PreviousStock1 = db.Item_Opening.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Stock) + db.Invoice_Detail.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.RecordDate <= toDate).Sum(x => x.InQty) - db.Invoice_Detail.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.RecordDate <= toDate).Sum(x => x.OutQty),
                                AverageValue1 = db.Item_Opening.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Value) + db.Invoice_Detail.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.RecordDate <= toDate).Sum(x => (x.AvgRate * x.InQty) - (x.AvgRate * x.OutQty))
                            }).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                }
            }
            else if (items == null && code != null)
            {
                for (int j = 0; j < code.Length; j++)
                {
                    int code1 = Convert.ToInt32(code[j].ToString());
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            join it in db.Item_Information on fd.ItemID equals it.ItemID
                            where f.RecordDate <= toDate && it.ItemID == code1
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                ToDate = toDate,
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemCode = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.Barcode).FirstOrDefault(),
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault(),
                                BrandID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.BrandID).FirstOrDefault(),
                                CategoryName = (from i in db.Item_Information join c in db.Item_Category on i.CategoryID equals c.CategoryID where i.ItemID == g.FirstOrDefault().ItemID select c.Description).FirstOrDefault() ?? "",
                                BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == g.FirstOrDefault().ItemID select b.Description).FirstOrDefault() ?? "",
                                UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? "",
                                PreviousStock1 = db.Item_Opening.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Stock) + db.Invoice_Detail.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.RecordDate <= toDate).Sum(x => x.InQty) - db.Invoice_Detail.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.RecordDate <= toDate).Sum(x => x.OutQty),
                                AverageValue1 = db.Item_Opening.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Value) + db.Invoice_Detail.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.RecordDate <= toDate).Sum(x => (x.AvgRate * x.InQty) - (x.AvgRate * x.OutQty))
                            }).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                }
            }
            else
            {
                for (int j = 0; j < items.Length; j++)
                {
                    int itemId = Convert.ToInt32(items[j]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where f.RecordDate <= toDate && fd.ItemID == itemId
                            group fd by fd.ItemID into g
                            select new clsInvoice
                            {
                                ToDate = toDate,
                                ItemID = g.FirstOrDefault().ItemID,
                                ItemCode = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.Barcode).FirstOrDefault(),
                                ItemName = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.CategoryID).FirstOrDefault(),
                                BrandID = db.Item_Information.Where(x => x.ItemID == g.FirstOrDefault().ItemID).Select(x => x.BrandID).FirstOrDefault(),
                                CategoryName = (from i in db.Item_Information join c in db.Item_Category on i.CategoryID equals c.CategoryID where i.ItemID == g.FirstOrDefault().ItemID select c.Description).FirstOrDefault() ?? "",
                                BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == g.FirstOrDefault().ItemID select b.Description).FirstOrDefault() ?? "",
                                UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == g.FirstOrDefault().ItemID select u.Description).FirstOrDefault() ?? "",
                                PreviousStock1 = db.Item_Opening.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Stock) + db.Invoice_Detail.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.RecordDate <= toDate).Sum(x => x.InQty) - db.Invoice_Detail.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.RecordDate <= toDate).Sum(x => x.OutQty),
                                AverageValue1 = db.Item_Opening.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Value) + db.Invoice_Detail.Where(x => x.ItemID == g.FirstOrDefault().ItemID && x.RecordDate <= toDate).Sum(x => (x.AvgRate * x.InQty) - (x.AvgRate * x.OutQty))
                            }).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                }
            }
            if (categories != null)
            {
                int j = 0;
                for (int i = 0; i < categories.Length; i++)
                {
                    int categoryId = Convert.ToInt32(categories[i]);
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsInvoice>();
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list2.Where(x => x.CategoryID == categoryId).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    j++;
                    //}
                }
            }
            if (brands != null)
            {
                int j = 0;
                for (int i = 0; i < brands.Length; i++)
                {
                    int brandId = Convert.ToInt32(brands[i]);
                    //if (list3.Any(x => x.BrandID == brandId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list3.Where(x => x.BrandID == brandId).ToList();
                    list4.AddRange(list);
                    j++;
                    //}
                }
            }
            return list4;
        }
        #endregion
        #region Items Stock Average Rate Register
        public ActionResult ItemStockAverageRateRegisterReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintItemStockAverageRateRegister()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");
                String[] code = Request.Form.GetValues("UrduName");
                var checkbox = Request.Form.GetValues("UntilDateCheckbox");
                DateTime? fromDate = null;
                DateTime? untilDate = null;
                if (checkbox != null)
                {

                    fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                    untilDate = Convert.ToDateTime(Request.Form["UntilDate"]);
                }
                else
                {
                    fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
                    untilDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                clsInvoice invoice = new clsInvoice();
                List<clsInvoice> itemsList = itemsList = GetItemsStockAverateRateRegister(items, code, categories, brands, fromDate, untilDate);
                itemsList = itemsList.OrderBy(x => x.ItemName).ToList();
                invoice.InvoicesList = itemsList.OrderBy(x => x.ItemName).ThenBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ToList(); 
                invoice.InvoicesList2 = itemsList.GroupBy(x => x.ItemID).Select(g => g.First()).ToList();
                invoice.InvoicesList2 = invoice.InvoicesList2.OrderBy(x => x.ItemName).ThenBy(x => x.RecordDate).ThenBy(x => x.ComputerNo).ToList();
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetItemsStockAverateRateRegister(String[] items, String[] code, String[] categories, String[] brands, DateTime? fromDate, DateTime? toDate)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> list2 = new List<clsInvoice>();
            List<clsInvoice> list3 = new List<clsInvoice>();
            List<clsInvoice> list4 = new List<clsInvoice>();
            if (items == null)
            {
                for (int j = 0; j < code.Length; j++)
                {
                    int code1 = Convert.ToInt32(code[j].ToString());
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            join it in db.Item_Information on fd.ItemID equals it.ItemID
                            where f.RecordDate>= fromDate &&  f.RecordDate <= toDate && it.ItemID == code1 
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                DetailCode = f.DetailCode,
                                FromDate = fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                ItemID = fd.ItemID,
                                ItemCode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.Barcode).FirstOrDefault(),
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault(),
                                BrandID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.BrandID).FirstOrDefault(),
                                CategoryName = (from i in db.Item_Information join c in db.Item_Category on i.CategoryID equals c.CategoryID where i.ItemID == fd.ItemID select c.Description).FirstOrDefault() ?? "",
                                BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                                UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? "",
                                InQty = fd.InQty,
                                OutQty = fd.OutQty,
                                AvgRate = fd.AvgRate,
                                PreviousStock1 = db.Item_Opening.Where(x => x.ItemID == fd.ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Stock) + db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate).Sum(x => x.InQty) - db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate).Sum(x => x.OutQty),
                                // PreviousStock2 = db.Item_Opening.Where(x => x.ItemID == fd.ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Stock2) + db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate >= fromDate && x.RecordDate <= toDate).Sum(x => x.InSecondQty) - db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate >= fromDate && x.RecordDate <= toDate).Sum(x => x.OutSecondQty),
                                AverageValue1 = db.Item_Opening.Where(x => x.ItemID == fd.ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Value) + db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate).Sum(x => (x.AvgRate * x.InQty) - (x.AvgRate * x.OutQty)),
                            }).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                }
            }
            else
            {
                for (int j = 0; j < items.Length; j++)
                {
                    int itemId = Convert.ToInt32(items[j]);
                    list = (from f in db.Invoice_Main
                            join fd in db.Invoice_Detail on f.ComputerNo equals fd.ComputerNo
                            where f.RecordDate >= fromDate &&  f.RecordDate <= toDate && fd.ItemID == itemId
                            select new clsInvoice
                            {
                                ComputerNo = f.ComputerNo,
                                RecordDate = f.RecordDate,
                                RefrenceNo = f.RefrenceNo,
                                DetailCode = f.DetailCode,
                                FromDate =fromDate,
                                ToDate = toDate,
                                DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                ItemID = fd.ItemID,
                                ItemCode = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.Barcode).FirstOrDefault(),
                                ItemName = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.ItemName).FirstOrDefault(),
                                CategoryID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.CategoryID).FirstOrDefault(),
                                BrandID = db.Item_Information.Where(x => x.ItemID == fd.ItemID).Select(x => x.BrandID).FirstOrDefault(),
                                CategoryName = (from i in db.Item_Information join c in db.Item_Category on i.CategoryID equals c.CategoryID where i.ItemID == fd.ItemID select c.Description).FirstOrDefault() ?? "",
                                BrandName = (from i in db.Item_Information join b in db.Item_Brand on i.BrandID equals b.BrandID where i.ItemID == fd.ItemID select b.Description).FirstOrDefault() ?? "",
                                UnitName = (from i in db.Item_Information join u in db.Item_UOM on i.UomId equals u.UomId where i.ItemID == fd.ItemID select u.Description).FirstOrDefault() ?? "",
                                InQty = fd.InQty,
                                OutQty = fd.OutQty,
                                AvgRate = fd.AvgRate,
                                PreviousStock1 = db.Item_Opening.Where(x => x.ItemID == fd.ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Stock) + db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate).Sum(x => x.InQty) - db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate).Sum(x => x.OutQty),
                                // PreviousStock2 = db.Item_Opening.Where(x => x.ItemID == fd.ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Stock2) + db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate >= fromDate && x.RecordDate <= toDate).Sum(x => x.InSecondQty) - db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate >= fromDate && x.RecordDate <= toDate).Sum(x => x.OutSecondQty),
                                AverageValue1 = db.Item_Opening.Where(x => x.ItemID == fd.ItemID && x.FiscalYearID == User.FiscalYearId).Sum(x => x.Value) + db.Invoice_Detail.Where(x => x.ItemID == fd.ItemID && x.RecordDate < fromDate).Sum(x => (x.AvgRate * x.InQty) - (x.AvgRate * x.OutQty)),
                            }).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                }
            }
            if (categories != null)
            {
                int j = 0;
                for (int i = 0; i < categories.Length; i++)
                {
                    int categoryId = Convert.ToInt32(categories[i]);
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsInvoice>();
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list2.Where(x => x.CategoryID == categoryId).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    j++;
                    //}
                }
            }
            if (brands != null)
            {
                int j = 0;
                for (int i = 0; i < brands.Length; i++)
                {
                    int brandId = Convert.ToInt32(brands[i]);
                    //if (list3.Any(x => x.BrandID == brandId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list3.Where(x => x.BrandID == brandId).ToList();
                    list4.AddRange(list);
                    j++;
                    //}
                }
            }
            return list4;
        }
        #endregion
        #region Items Stock With Last Purchase Rate
        public ActionResult ItemStockLastPurchaseRateReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintItemStockWithLastPurchaseRate()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");
                String[] code = Request.Form.GetValues("UrduName");
                var checkbox = Request.Form.GetValues("UntilDateCheckbox");
                DateTime? untilDate = null;
                if (checkbox != null)
                {
                    untilDate = Convert.ToDateTime(Request.Form["UntilDate"]);
                }
                else
                {
                    untilDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                clsInvoice invoice = new clsInvoice();
                List<clsInvoice> itemsList = itemsList = GetItemsStockWithLastPurchaseRate(items,code, categories, brands, untilDate);
                itemsList = itemsList.OrderBy(x => x.ItemName).ToList();
                invoice.InvoicesList = itemsList;
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetItemsStockWithLastPurchaseRate(String[] items,String[] code, String[] categories, String[] brands, DateTime? toDate)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> list2 = new List<clsInvoice>();
            List<clsInvoice> list3 = new List<clsInvoice>();
            List<clsInvoice> list4 = new List<clsInvoice>();
            List<clsInvoice> list5 = new List<clsInvoice>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_ItemStockLastPurchase", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vToDate", SqlDbType.Date).Value = toDate;
            cmd.Parameters.Add("@vFiscalYear", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsInvoice()
                {
                    ItemID = Convert.ToInt32(dt[0]),
                    ItemName = dt[1].ToString(),
                    UrduName = dt[2].ToString(),
                    UnitName = dt[8].ToString(),
                    Stock1 = Convert.ToDecimal(dt[9]),
                    AvgRate = Convert.ToDecimal(dt[10]),
                    AverageValue1 = Convert.ToDecimal(dt[11]),
                    CategoryID = Convert.ToInt32(dt[4]),
                    BrandID = Convert.ToInt32(dt[5]),
                    BrandName = dt[6].ToString(),
                    CategoryName = dt[7].ToString(),
                    ToDate = toDate
                });
            }
            if (items == null)
            {
                list2.AddRange(list);
                list3.AddRange(list);
                list4.AddRange(list);
                list5.AddRange(list);
            }
            else
            {
                for (int j = 0; j < items.Length; j++)
                {
                    int itemId = Convert.ToInt32(items[j]);
                    list = list.Where(x => x.ItemID == itemId).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                }
            }
            if (code != null)
            {
                int j = 0;
                for (int i = 0; i < code.Length; i++)
                {
                    int code1 = Convert.ToInt32(code[j].ToString());
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsInvoice>();
                        list4 = new List<clsInvoice>();
                        list5 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list2.Where(x => x.ItemID == code1).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }
            if (categories != null)
            {
                int j = 0;
                for (int i = 0; i < categories.Length; i++)
                {
                    int categoryId = Convert.ToInt32(categories[i]);
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                        list5 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list3.Where(x => x.CategoryID == categoryId).ToList();
                    list4.AddRange(list);
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }
            if (brands != null)
            {
                int j = 0;
                for (int i = 0; i < brands.Length; i++)
                {
                    int brandId = Convert.ToInt32(brands[i]);
                    //if (list3.Any(x => x.BrandID == brandId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list4.Where(x => x.BrandID == brandId).ToList();
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }



            //list5 = list5.Where(x => x.Stock1 != 0).ToList();
            return list5;
        }
        #endregion

        
        
        #region Items Not Sold
        public ActionResult ItemNotSoldReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintItemNotSoldReport()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                String[] code = Request.Form.GetValues("UrduName");
                var checkbox = Request.Form.GetValues("DateCheckbox");
                if (checkbox == null || fromDate == null || toDate == null)
                {
                    fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
                    toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                clsInvoice invoice = new clsInvoice();
                List<clsInvoice> itemsList = itemsList = GetItemsNotSold(items, code, categories, brands, fromDate, toDate);

                itemsList = itemsList.Where(x => x.PreviousStock1 >0).ToList();
                itemsList = itemsList.Where(x => x.FromDate <= toDate).ToList();
                itemsList = itemsList.OrderBy(x => x.ItemName).ToList();
                invoice.InvoicesList = itemsList;
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetItemsNotSold(String[] items, String[] code, String[] categories, String[] brands, DateTime fromDate, DateTime toDate)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            List<clsInvoice> list2 = new List<clsInvoice>();
            List<clsInvoice> list3 = new List<clsInvoice>();
            List<clsInvoice> list4 = new List<clsInvoice>();
            List<clsInvoice> list5 = new List<clsInvoice>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_ItemStockNotSold", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@vFiscalYear", SqlDbType.Int).Value = User.FiscalYearId;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsInvoice()
                {
                    ItemID = Convert.ToInt32(dt[0]),
                    ItemName = dt[1].ToString(),
                    UrduName = dt[2].ToString(),
                    UnitName = dt[8].ToString(),
                    Stock1 = Convert.ToDecimal(dt[9]),
                    InQty = Convert.ToDecimal(dt[10]),
                    OutQty = Convert.ToDecimal(dt[11]),
                    PreviousStock1 = Convert.ToDecimal(dt["ClosingStock"]),
                    CategoryID = Convert.ToInt32(dt[4]),
                    BrandID = Convert.ToInt32(dt[5]),
                    FromDate = Convert.ToDateTime(dt["LastSoldDate"]),
                    ToDate = toDate
                });
            }
            if (items == null)
            {
                list2.AddRange(list);
                list3.AddRange(list);
                list4.AddRange(list);
                list5.AddRange(list);
            }
            else
            {
                for (int j = 0; j < items.Length; j++)
                {
                    int itemId = Convert.ToInt32(items[j]);
                    list = list.Where(x => x.ItemID == itemId).ToList();
                    list2.AddRange(list);
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                }
            }
            if (code != null)
            {
                int j = 0;
                for (int i = 0; i < code.Length; i++)
                {
                    int code1 = Convert.ToInt32(code[j].ToString());
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list3 = new List<clsInvoice>();
                        list4 = new List<clsInvoice>();
                        list5 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list2.Where(x => x.ItemID == code1).ToList();
                    list3.AddRange(list);
                    list4.AddRange(list);
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }
            if (categories != null)
            {
                int j = 0;
                for (int i = 0; i < categories.Length; i++)
                {
                    int categoryId = Convert.ToInt32(categories[i]);
                    //if (list2.Any(x=>x.CategoryID == categoryId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                        list5 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list3.Where(x => x.CategoryID == categoryId).ToList();
                    list4.AddRange(list);
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }
            if (brands != null)
            {
                int j = 0;
                for (int i = 0; i < brands.Length; i++)
                {
                    int brandId = Convert.ToInt32(brands[i]);
                    //if (list3.Any(x => x.BrandID == brandId))
                    //{
                    if (j == 0)
                    {
                        list4 = new List<clsInvoice>();
                    }
                    list = new List<clsInvoice>();
                    list = list4.Where(x => x.BrandID == brandId).ToList();
                    list5.AddRange(list);
                    j++;
                    //}
                }
            }
            return list5;
        }
        #endregion

        #region Items PLS Report
        public ActionResult ItemPLSReport()
        {
            //ViewBag.Item = new SelectList(getItems(), "ItemID", "ItemName");
            ViewBag.Category = new SelectList(getCategories(), "CategoryID", "CategoryName");
            ViewBag.Brand = new SelectList(getBrands(), "BrandID", "BrandName");
            ViewBag.SubCategory = new SelectList(getSubCategories(), "SubCategoryID", "SubCategoryName");
            return View();
        }
        [HttpPost]
        public ActionResult PrintItemPLSReport()
        {
            if (User != null)
            {
                String[] items = Request.Form.GetValues("ItemID");
                String[] categories = Request.Form.GetValues("CategoryID");
                String[] brands = Request.Form.GetValues("BrandID");
                String[] subcateg = Request.Form.GetValues("SubCategoryID");
                DateTime fromDate = Convert.ToDateTime(Request.Form["FromDate"]);
                DateTime toDate = Convert.ToDateTime(Request.Form["ToDate"]);
                String[] code = Request.Form.GetValues("UrduName");

                string plsfromstr =Request.Form["PLSPersFrom"];
                string plstostr = Request.Form["PLSPersTo"];

                if (plsfromstr == null || plsfromstr == "") { plsfromstr = "0"; }
                if (plstostr == null || plstostr == "") { plstostr = "0"; }

                decimal plsfrom = Convert.ToDecimal(plsfromstr);
                decimal plsto = Convert.ToDecimal(plstostr);

                
                var checkbox = Request.Form.GetValues("DateCheckbox");
                if (checkbox == null || fromDate == null || toDate == null)
                {
                    fromDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.FromDate).FirstOrDefault());
                    toDate = Convert.ToDateTime(db.Admin_Setup_FiscalYear.Where(x => x.FiscalYearID == User.FiscalYearId).Select(x => x.ToDate).FirstOrDefault());
                }
                clsInvoice invoice = new clsInvoice();
                List<clsInvoice> itemsList =  GetItemsPLS(fromDate, toDate);


                List<clsInvoice> tempList = new List<clsInvoice>();
                List<clsInvoice> finalList = new List<clsInvoice>();
                if (categories != null)
                {
                    tempList = itemsList;
                    int j = 0;
                    for (int i = 0; i < categories.Length; i++)
                    {
                        int subid = Convert.ToInt32(categories[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = tempList.Where(x => x.CategoryID == subid).ToList();
                        finalList.AddRange(list);
                    }
                }
                else
                {
                    finalList = itemsList.ToList();
                }

                if (subcateg != null)
                {
                    tempList = finalList.ToList();
                    finalList.Clear();
                    int j = 0;
                    for (int i = 0; i < subcateg.Length; i++)
                    {
                        int subid = Convert.ToInt32(subcateg[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = tempList.Where(x => x.SubCategoryID == subid).ToList();
                        finalList.AddRange(list);
                    }
                }

                if (brands != null)
                {
                    tempList = finalList.ToList();
                    finalList.Clear();
                    int j = 0;
                    for (int i = 0; i < brands.Length; i++)
                    {
                        int subid = Convert.ToInt32(brands[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = tempList.Where(x => x.BrandID == subid).ToList();
                        finalList.AddRange(list);
                    }
                }

                if (items != null)
                {
                    tempList = finalList.ToList();
                    finalList.Clear();
                    int j = 0;
                    for (int i = 0; i < items.Length; i++)
                    {
                        int subid = Convert.ToInt32(items[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = tempList.Where(x => x.ItemID == subid).ToList();
                        finalList.AddRange(list);
                    }
                }


                if (code != null)
                {
                    tempList = finalList.ToList();
                    finalList.Clear();
                    int j = 0;
                    for (int i = 0; i < code.Length; i++)
                    {
                        int subid = Convert.ToInt32(code[i]);
                        List<clsInvoice> list = new List<clsInvoice>();
                        list = tempList.Where(x => x.ItemID == subid).ToList();
                        finalList.AddRange(list);
                    }
                }

                if (plsfrom != 0 || plsto != 0)
                {
                    finalList = finalList.Where(x => x.Discount >=plsfrom && x.Discount<=plsto).ToList();
                }

                finalList = finalList.OrderBy(x => x.Discount).ThenBy(x => x.ItemName).ToList();
                invoice.InvoicesList = finalList;
                return View(invoice);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public List<clsInvoice> GetItemsPLS(DateTime fromDate, DateTime toDate)
        {
            List<clsInvoice> list = new List<clsInvoice>();
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_ItemWisePLS", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate;
            cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate;
            SqlDataReader r = cmd.ExecuteReader();
            DataTable tdt = new DataTable();
            tdt.Load(r);
            r.Close();
            DataTableReader dt = tdt.CreateDataReader();
            con.Close();
            SqlConnection.ClearPool(con);
            while (dt.Read())
            {
                list.Add(new clsInvoice()
                {
                    ItemID = Convert.ToInt32(dt["ItemID"]),
                    ItemName = dt["ItemName"].ToString(),
                    UrduName = dt["UrduName"].ToString(),
                    UnitName = dt["UOMName"].ToString(),

                    CategoryID = Convert.ToInt32(dt["CategoryID"]),
                    CategoryName = dt["CategoryName"].ToString(),
                    BrandID = Convert.ToInt32(dt["BrandID"]),
                    BrandName = dt["BrandName"].ToString(),
                    SubCategoryID = Convert.ToInt32(dt["SubCategoryID"]),
                    SubCategoryName = dt["SubCategoryName"].ToString(),

                    Qty = Convert.ToDecimal(dt["NetQty"]),
                    NetSale = Convert.ToDecimal(dt["NetSale"]),
                    NetCost = Convert.ToDecimal(dt["NetCost"]),

                    NetValue = Convert.ToDecimal(dt["PLS"]),
                    Discount = Convert.ToDecimal(dt["PLSPers"]),
                    FromDate = fromDate,
                    ToDate = toDate
                });
            }
            return list;

        }
        #endregion

        #endregion
    }
}