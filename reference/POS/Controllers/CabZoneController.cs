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
    public class CabZoneController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult CabZoneIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "ItemBrand"
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
        public ActionResult insertupdateCabZone(clsCabZone CabForm , string ModelList)
        {
            string message = "";
            bool status = false;
            string insertupdatestatus = "";
            
            List<clsCabZone> modeldetail = JsonConvert.DeserializeObject<List<clsCabZone>>(ModelList);
            DataTable dtM = new DataTable();
            dtM.Columns.Add("Id");
            dtM.Columns.Add("AreaCode");

            for (int i = 0; i < modeldetail.Count; i++)
            {
                dtM.Rows.Add(new object[] { i + 1, modeldetail[i].AreaCode });
            }


            string returnId = "";
            if (CabForm.CabZoneID > 0)
            {
                insertupdatestatus = "Update";
            }
            else
            {
                insertupdatestatus = "Save";
                CabForm.CabZoneID = 0;
            }

            returnId = AddEditCabZone(CabForm.CabZoneID, CabForm.Description, CabForm.Remarks, CabForm.Status, dtM, insertupdatestatus);
            if (returnId != "0")
            {
                ModelState.Clear();
                status = true;
                message = "Record Save Successfully";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }


            return new JsonResult { Data = new { status = status, message = message } };
        }

        private string AddEditCabZone(int CabZoneID, string Description, string Remarks, string Status, DataTable dtModel, string insertUpdateStatus)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateZone", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ZoneID", SqlDbType.VarChar, 25).Value = CabZoneID;
                        cmd.Parameters.Add("@Description", SqlDbType.VarChar, 100).Value = Description;
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Value = Remarks ;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 100).Value = Status;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                        cmd.Parameters.Add("@dtInvoiceArea", SqlDbType.Structured).Value = dtModel;
                        cmd.Parameters.Add("@InsertUpdateStatus", SqlDbType.NVarChar).Value = insertUpdateStatus;
                        cmd.Parameters.Add("@CheckReturn", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
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


        public ActionResult GetCabZoneById(int id)
        {
            clsCabZone CabZone = new clsCabZone();
            CabZone = (from f in db.Cab_Zone
                        where f.ZoneID == id
                        select new clsCabZone
                        {
                            CabZoneID = f.ZoneID,
                            Description = f.Description,
                            Remarks=f.Remarks,
                            Status = f.Status
                        }).FirstOrDefault();
            var data = CabZone;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCabZoneFareDetailById(int id)
        {
            List<clsCabZone> fareDetail = new List<clsCabZone>();
            fareDetail = (from f in db.Cab_Zone_Area
                          where f.ZoneID == id
                             select new clsCabZone
                             {
                                 SerialNo = f.SerialNo,
                                 AreaCode = f.AreaCode ?? "",
                             }).OrderBy(x => x.SerialNo).ToList();
            var data = fareDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CabZoneList()
        {
            return PartialView();
        }
        public ActionResult GetAllCabZone()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var description = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCabZone> CabZone = (from f in db.Cab_Zone
                                         select new clsCabZone
                                         {
                                             CabZoneID = f.ZoneID,
                                             Description = f.Description,
                                             Remarks = f.Remarks,
                                             Status = f.Status
                                         }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(description))
                {
                    CabZone = CabZone.Where(c => c.Description.ToLower().Contains(description.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    CabZone = CabZone.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        CabZone = CabZone.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = CabZone.Count();
                var data = CabZone.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteCabZone(int id)
        {
            string message = "";
            bool status = false;

            DataTable dtM = new DataTable();
            dtM.Columns.Add("Id");
            dtM.Columns.Add("AreaCode");
            dtM.Rows.Add(new object[] { 0, "" });

            string returnId = "";
            string insertupdatestatus = "Delete";

            returnId = AddEditCabZone(id, "", "","", dtM, insertupdatestatus);
            if (Convert.ToDecimal(returnId) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Record Save Successfully";
            }
            else if (Convert.ToInt32(returnId) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this Zone first.";
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