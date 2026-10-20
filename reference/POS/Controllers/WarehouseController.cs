using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data.Entity;

namespace POS.Controllers
{
    public class WarehouseController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult WarehouseIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "Warehouse"
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
        public ActionResult InsertUpdateWarehouse(clsWarehouse warehouse)
        {
            string message = "";
            bool status = false;
            if (warehouse.WarehouseID > 0)
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateWarehouse(warehouse.WarehouseID, 0, warehouse.WarehouseName, warehouse.WarehouseAbb, warehouse.Address, warehouse.Phone1, warehouse.Phone2, warehouse.Cell1, warehouse.Cell2, warehouse.Email, warehouse.WarehouseIncharge, warehouse.Status, User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Warehouse Successfully Updated";
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
                db.spInsertUpdateWarehouse(warehouse.WarehouseID, 0, warehouse.WarehouseName, warehouse.WarehouseAbb, warehouse.Address, warehouse.Phone1, warehouse.Phone2, warehouse.Cell1, warehouse.Cell2, warehouse.Email, warehouse.WarehouseIncharge, "Active", User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Warehouse Successfully Saved";
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
        public ActionResult GetWarehouseById(int id)
        {
            clsWarehouse warehouse = new clsWarehouse();
            warehouse = (from f in db.Admin_Setup_Warehouse
                     where f.WarehouseID == id
                     select new clsWarehouse
                     {
                         WarehouseID = f.WarehouseID,
                         BusinessID = f.BusinessID,
                         WarehouseName = f.WarehouseName,
                         WarehouseAbb = f.WarehouseAbb,
                         Address = f.Address,
                         Phone1 = f.Phone1,
                         Phone2 = f.Phone2,
                         Cell1 = f.Cell1,
                         Cell2 = f.Cell2,
                         Email = f.Email,
                         WarehouseIncharge = f.WarehouseIncharge,
                         Status = f.Status
                     }).FirstOrDefault();
            var data = warehouse;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WarehousesList()
        {
            return PartialView();
        }
        public ActionResult GetAllWarehouses()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var warehouseName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var warehouseAbb = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var address = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var email = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var incharge = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsWarehouse> warehouse = (from f in db.Admin_Setup_Warehouse
                                            select new clsWarehouse
                                       {
                                           WarehouseID = f.WarehouseID,
                                           BusinessID = f.BusinessID,
                                           WarehouseName = f.WarehouseName,
                                           WarehouseAbb = f.WarehouseAbb,
                                           Address = f.Address,
                                           Phone1 = f.Phone1,
                                           Phone2 = f.Phone2,
                                           Cell1 = f.Cell1,
                                           Cell2 = f.Cell2,
                                           Email = f.Email,
                                           WarehouseIncharge = f.WarehouseIncharge,
                                           Status = f.Status
                                       }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(warehouseName))
                {
                    warehouse = warehouse.Where(c => c.WarehouseName.ToLower().Contains(warehouseName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(warehouseAbb))
                {
                    warehouse = warehouse.Where(c => c.WarehouseAbb.ToLower().Contains(warehouseAbb.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(address))
                {
                    warehouse = warehouse.Where(c => c.Address.ToLower().Contains(address.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(email))
                {
                    warehouse = warehouse.Where(c => c.Email.ToLower().Contains(email.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(incharge))
                {
                    warehouse = warehouse.Where(c => c.WarehouseIncharge.ToLower().Contains(incharge.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    warehouse = warehouse.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        warehouse = warehouse.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = warehouse.Count();
                var data = warehouse.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteWarehouse(int id)
        {
            string message = "";
            bool status = false;
            clsWarehouse warehouse = new clsWarehouse();
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateWarehouse(id, 0, "", "", "", "", "", "", "", "", "", "Active", User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Warehouse Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this warehouse first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
	}
}