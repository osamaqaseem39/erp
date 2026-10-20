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
    public class CabPriceListController : BaseController
    {
        PosMBDb db = new PosMBDb();

        public ActionResult GetAllCabType()
        {
            List<clsCabType> cabtype = (from f in db.Cab_CabType
                                        where f.Status.ToLower() == "active"
                                        select new clsCabType
                                        {
                                            CabTypeID = f.CabTypeID,
                                            Description = f.Description,
                                            SeatCapacity = f.SeatCapacity ?? 0,
                                            PricePers = f.PricePers ?? 0,
                                            Remarks = f.Remarks,
                                            Status = f.Status,
                                        }).ToList();

            cabtype = cabtype.OrderBy(x => x.SeatCapacity).ThenBy(x => x.PricePers).ThenBy(x => x.CabTypeID).ToList();
            var data = cabtype.ToList();
            return Json(data, JsonRequestBehavior.AllowGet);          
        }


        public ActionResult CabPriceUpdate()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CabOffer"
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
        public ActionResult UpdateCabPrice(string ToPostCode, decimal? vPers, string FareList)
        {
            string message = "";
            bool status = false;
            string returnId = "";

            List<clsCabType> faredetail = JsonConvert.DeserializeObject<List<clsCabType>>(FareList);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("Model");

            if (!String.IsNullOrEmpty(FareList))
            {
                for (int i = 0; i < faredetail.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, faredetail[i].CarModel});
                }
            }
            else
            {
                dt.Rows.Add(new object[] { 0, ""});
            }


            returnId = UpdateCabPrice(ToPostCode, vPers, dt);
            if (returnId != "0")
            {
                ModelState.Clear();
                status = true;
                message = "Record Updated Successfully";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }

            return new JsonResult { Data = new { status = status, message = message } };
        }


        private string UpdateCabPrice(string ToPostCode, decimal? vPers, DataTable dtModel)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCabPriceUpdate", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@ToPostCode", SqlDbType.VarChar, 100).Value = ToPostCode;
                        cmd.Parameters.Add("@vPers", SqlDbType.Decimal).Value = vPers;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                        cmd.Parameters.Add("@dtInvoiceModel", SqlDbType.Structured).Value = dtModel;
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



        public ActionResult CabPriceListIndex()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CabOffer"
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
        public ActionResult InsertUpdateCabPriceList(clsCabOfferMain CabOffer, string FareList)
        {
            string message = "";
            bool status = false;
            string insertupdatestatus = "";
            string returnId = "";

            List<clsCabType> faredetail = JsonConvert.DeserializeObject<List<clsCabType>>(FareList);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("CabTypeID");
            dt.Columns.Add("Price");

            if (!String.IsNullOrEmpty(FareList))
            {
                for (int i = 0; i < faredetail.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, faredetail[i].CabTypeID, faredetail[i].FarePrice });
                }
            }
            else
            {
                dt.Rows.Add(new object[] { 0, 0, 0 });
            }


            if (CabOffer.OfferID > 0)
            {
                insertupdatestatus = "Update";
            }
            else
            {
                insertupdatestatus = "Save";
                CabOffer.Status = "Active";
                CabOffer.OfferID = 0;
            }

            returnId = AddEditCabPriceList(CabOffer.OfferID, CabOffer.FromPostCode, CabOffer.ToPostCode, CabOffer.BasePrice, CabOffer.Remarks, CabOffer.Status, dt,insertupdatestatus);
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

        private string AddEditCabPriceList(int PriceID, string FromPostCode, string ToPostCode, decimal? BasePrice, string Remarks, string Status, DataTable dtFare, string insertUpdateStatus)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCabPriceList", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@PriceID", SqlDbType.VarChar, 25).Value = PriceID;
                        cmd.Parameters.Add("@FromPostCode", SqlDbType.VarChar, 50).Value = FromPostCode;
                        cmd.Parameters.Add("@ToPostCode", SqlDbType.VarChar,50).Value = ToPostCode;
                        cmd.Parameters.Add("@BasePrice", SqlDbType.Decimal).Value = BasePrice;
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 255).Value = Remarks;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 100).Value = Status;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                        cmd.Parameters.Add("@dtInvoiceFare", SqlDbType.Structured).Value = dtFare;
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

        public ActionResult GetCabPriceListById(int id)
        {
            clsCabOfferMain brand = new clsCabOfferMain();
            brand = (from f in db.PriceList_Main
                     where f.PriceID == id
                     select new clsCabOfferMain
                        {
                            OfferID = f.PriceID,
                            FromPostCode = f.FromPostCode,
                            ToPostCode = f.ToPostCode,
                            Remarks = f.Remarks,
                            Status = f.Status,
                            BasePrice = f.BasePrice ?? 0,
                        }).FirstOrDefault();
            var data = brand;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetCabTypeFareDetailById(int id)
        {
            List<clsCabType> fareDetail = new List<clsCabType>();
            fareDetail = (from f in db.PriceList_Detail
                          join c in db.Cab_CabType on f.CabTypeID equals c.CabTypeID
                          where f.PriceID == id
                          select new clsCabType
                          {
                              SerialNo = f.DetailID,
                              CabTypeID = f.CabTypeID ?? 0,
                              Description =c.Description ?? "",
                              PricePers= c.PricePers ?? 0,
                              FarePrice = f.Price ?? 0,
                              SeatCapacity=c.SeatCapacity ?? 0
                          }).OrderBy(x => x.SeatCapacity).ThenBy(x => x.PricePers).ThenBy(x => x.CabTypeID).ToList();
            var data = fareDetail.ToList();
            return Json(data, JsonRequestBehavior.AllowGet);          
        }

        public ActionResult CabPriceListList()
        {
            return PartialView();
        }
        public ActionResult GetAllCabPriceList()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var brandName = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCabOfferMain> brand = (from f in db.PriceList_Main
                                               select new clsCabOfferMain
                                         {
                                             OfferID = f.PriceID,
                                             FromPostCode = f.FromPostCode,
                                             ToPostCode = f.ToPostCode,
                                             Remarks = f.Remarks,
                                             Status = f.Status,
                                             BasePrice=f.BasePrice ?? 0,
                                         }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(brandName))
                {
                    brand = brand.Where(c => c.FromPostCode.ToLower().Contains(brandName.ToLower()) || c.ToPostCode.ToLower().Contains(brandName.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    brand = brand.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        brand = brand.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = brand.Count();
                var data = brand.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult DeleteCabPriceList(int id)
        {
            string message = "";
            bool status = false;
            string returnId = "";

            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("CabTypeID");
            dt.Columns.Add("Price");
            dt.Rows.Add(new object[] { 0, 0, 0 });

            returnId = AddEditCabPriceList(id, "", "", 0, "", "", dt, "Delete");
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
                message = "Error! Please delete all data against this record first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }

            return new JsonResult { Data = new { status = status, message = message } };
        }



        #region Cab Price Update New
        public List<clsCabType> GetCabType()
        {
            List<clsCabType> detail = new List<clsCabType>();
            detail = (from f in db.Cab_CabType
                      where f.Status.ToLower() == "active"
                      select new clsCabType
                      {
                          CabTypeID = f.CabTypeID,
                          Description = f.Description,
                          SeatCapacity = f.SeatCapacity ?? 0,
                          PricePers = f.PricePers ?? 0,
                          Remarks = f.Remarks,
                          Status = f.Status,
                      }).ToList();
            detail = detail.OrderBy(x => x.Description).ThenBy(x => x.CabTypeID).ToList();
            return detail;
        }

        public ActionResult CabPriceUpdateNew()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "CabOffer"
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
                    ViewBag.CabType = new SelectList(GetCabType(), "CabTypeID", "Description");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }


        public ActionResult GetAllPriceDetail(string vpostcode,int? cabtypeid)
        {
            List<clsCabOfferMain> brand= new List<clsCabOfferMain>();
            if (cabtypeid != 0)
            {
                brand = (from f in db.PriceList_Detail 
                         where f.CabTypeID == cabtypeid
                         && f.FromPostCode.ToLower().StartsWith(vpostcode.ToLower())
                         select new clsCabOfferMain
                         {
                             OfferID = f.PriceID ?? 0,
                             FromPostCode = f.FromPostCode,
                             ToPostCode = f.ToPostCode,
                             Remarks = "",
                             Status = f.Status,
                             BasePrice = f.Price ?? 0,
                         }).ToList();
            }
            else
            {
                brand = (from f in db.PriceList_Main
                         where f.FromPostCode.ToLower().StartsWith(vpostcode.ToLower())
                         select new clsCabOfferMain
                         {
                             OfferID = f.PriceID,
                             FromPostCode = f.FromPostCode,
                             ToPostCode = f.ToPostCode,
                             Remarks = f.Remarks,
                             Status = f.Status,
                             BasePrice = f.BasePrice ?? 0,
                         }).ToList();
            }
                    
            //SEARCHING...
            //if (!string.IsNullOrEmpty(vpostcode))
            //{
            //    brand = brand.Where(c => c.FromPostCode.ToLower().Contains(vpostcode.ToLower()) || c.ToPostCode.ToLower().Contains(vpostcode.ToLower())).ToList();
            //}

            brand = brand.OrderBy(x => x.FromPostCode).ThenBy(x => x.ToPostCode).ThenBy(x => x.OfferID).ToList();
            var data = brand.ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult UpdateCabPriceNew(string FareList)
        {
            string message = "";
            bool status = false;
            string returnId = "";

            List<clsCabOfferMain> faredetail = JsonConvert.DeserializeObject<List<clsCabOfferMain>>(FareList);
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("FromPostCode");
            dt.Columns.Add("ToPostCode");
            dt.Columns.Add("BasePrice");
            dt.Columns.Add("CabTypeID");

            if (!String.IsNullOrEmpty(FareList))
            {
                for (int i = 0; i < faredetail.Count; i++)
                {
                    dt.Rows.Add(new object[] { i + 1, faredetail[i].FromPostCode, faredetail[i].ToPostCode, faredetail[i].BasePrice, faredetail[i].CabTypeID });
                }
            }
            else
            {
                dt.Rows.Add(new object[] { 0, "","",0,0 });
            }


            returnId = UpdateCabPriceNew(dt);
            if (returnId != "0")
            {
                ModelState.Clear();
                status = true;
                message = "Record Updated Successfully";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }

            return new JsonResult { Data = new { status = status, message = message } };
        }


        private string UpdateCabPriceNew(DataTable dtModel)
        {
            string returnId = "";
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["PosADO"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connection))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("spInsertUpdateCabPriceUpdateNew", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.UserId;
                        cmd.Parameters.Add("@dtInvoiceModel", SqlDbType.Structured).Value = dtModel;
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
        #endregion
    }
}