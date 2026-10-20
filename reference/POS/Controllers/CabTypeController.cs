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
    public class CabTypeController : BaseController
    {
        PosMBDb db = new PosMBDb();
        public ActionResult CabTypeIndex()
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
        public ActionResult insertupdatecabtype(clsCabType CabForm , string FareList, string ModelList)
        {
            string message = "";
            bool status = false;
            string insertupdatestatus = "";
            
            List<clsCabType> faredetail = JsonConvert.DeserializeObject<List<clsCabType>>(FareList);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("UptoKm");
            dt.Columns.Add("Fare");

            for (int i = 0; i < faredetail.Count; i++)
            {
                dt.Rows.Add(new object[] { i + 1, faredetail[i].UptoKm, faredetail[i].FarePrice });
            }

            List<clsCabType> modeldetail = JsonConvert.DeserializeObject<List<clsCabType>>(ModelList);
            DataTable dtM = new DataTable();
            dtM.Columns.Add("Id");
            dtM.Columns.Add("Model");

            for (int i = 0; i < modeldetail.Count; i++)
            {
                dtM.Rows.Add(new object[] { i + 1, modeldetail[i].CarModel });
            }


            string returnId = "";
            if (CabForm.CabTypeID > 0)
            {
                insertupdatestatus = "Update";
            }
            else
            {
                insertupdatestatus = "Save";
                CabForm.CabTypeID = 0;
            }

            returnId = AddEditCabType(CabForm.CabTypeID, CabForm.Description, CabForm.SeatCapacity, CabForm.Remarks, CabForm.Status, dt, dtM, insertupdatestatus, CabForm.PricePers);
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


            return new JsonResult { Data = new { status = status, message = message, cabtypeid = returnId } };
        }

        private string AddEditCabType(int CabTypeID, string Description, int? SeatCapacity, string Remarks, string Status, DataTable dtFare, DataTable dtModel, string insertUpdateStatus, decimal? PricePers)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCabType", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@CabTypeID", SqlDbType.VarChar, 25).Value = CabTypeID;
                        cmd.Parameters.Add("@Description", SqlDbType.VarChar, 100).Value = Description;
                        cmd.Parameters.Add("@SeatCapacity", SqlDbType.Int).Value = SeatCapacity ;
                        cmd.Parameters.Add("@PricePers", SqlDbType.Decimal).Value = PricePers;
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 1000).Value = Remarks ;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 100).Value = Status;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                        cmd.Parameters.Add("@dtInvoiceFare", SqlDbType.Structured).Value = dtFare;
                        cmd.Parameters.Add("@dtInvoiceModel", SqlDbType.Structured).Value = dtModel;
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


        public ActionResult GetCabTypeById(int id)
        {
            clsCabType cabtype = new clsCabType();
            cabtype = (from f in db.Cab_CabType
                        where f.CabTypeID == id
                        select new clsCabType
                        {
                            CabTypeID = f.CabTypeID,
                            Description = f.Description,
                            SeatCapacity=f.SeatCapacity ?? 0,
                            PricePers = f.PricePers ?? 0,
                            Remarks=f.Remarks,
                            Status = f.Status,
                            ImageName = db.Form_Document.Where(x => x.TypeId == f.CabTypeID.ToString() && x.DocumentType == "Cab" && x.Remarks == "Cab Image").Select(x => x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "Nothing",
                        }).FirstOrDefault();
            var data = cabtype;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCabTypeFareDetailById(int id)
        {
            List<clsCabType> fareDetail = new List<clsCabType>();
            fareDetail = (from f in db.Cab_CabType_Fare 
                             where f.CabTypeID == id
                             select new clsCabType
                             {
                                 SerialNo = f.SerialNo,
                                 UptoKm = f.UptoKm ?? 0,
                                 FarePrice=f.FarePrice ?? 0
                             }).OrderBy(x => x.UptoKm).ToList();
            var data = fareDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCabTypeModelDetailById(int id)
        {
            List<clsCabType> fareDetail = new List<clsCabType>();
            fareDetail = (from f in db.Cab_CabType_Model 
                          where f.CabTypeID == id
                          select new clsCabType
                          {
                              SerialNo = f.SerialNo,
                              CarModel = f.CarModel  ?? "",
                          }).OrderBy(x => x.SerialNo).ToList();
            var data = fareDetail.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CabTypeList()
        {
            return PartialView();
        }
        public ActionResult GetAllCabType()
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
                List<clsCabType> cabtype = (from f in db.Cab_CabType
                                         select new clsCabType
                                         {
                                             CabTypeID = f.CabTypeID,
                                             Description = f.Description,
                                             SeatCapacity = f.SeatCapacity ?? 0,
                                             PricePers = f.PricePers ?? 0,
                                             Remarks = f.Remarks,
                                             Status = f.Status
                                         }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(description))
                {
                    cabtype = cabtype.Where(c => c.Description.ToLower().Contains(description.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cabtype = cabtype.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        cabtype = cabtype.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = cabtype.Count();
                var data = cabtype.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteCabType(int id)
        {
            string message = "";
            bool status = false;

            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("UptoKm");
            dt.Columns.Add("Fare");
            dt.Rows.Add(new object[] { 0, 0, 0 });

            DataTable dtM = new DataTable();
            dtM.Columns.Add("Id");
            dtM.Columns.Add("Model");
            dtM.Rows.Add(new object[] { 0, "" });

            string returnId = "";
            string insertupdatestatus = "Delete";

            returnId = AddEditCabType(id, "", 0, "","", dt, dtM, insertupdatestatus,0);
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
                message = "Error! Please delete all data against this Cab Type first.";
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