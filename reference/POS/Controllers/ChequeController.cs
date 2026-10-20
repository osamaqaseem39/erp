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
    public class ChequeController : BaseController
    {
        PosMBDb db = new PosMBDb();
        #region Cheque Book
        public ActionResult ChequeBook()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "ChequeBook"
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
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateChequeBook(clsCheque formData)
        {
            string message = "";
            bool status = false;
            string firstCharacterFrom = formData.FromSerialNo.Substring(0, 1);
            string firstCharacterTo = formData.ToSerialNo.Substring(0, 1);
            if (firstCharacterFrom == "0")
            {
                formData.ContainFromZero = "0";
            }
            else
            {
                formData.ContainFromZero = "1";
            }
            if (firstCharacterTo == "0")
            {
                formData.ContainToZero = "0";
            }
            else
            {
                formData.ContainToZero = "1";
            }
            if (formData.ChequeBookCompNo > 0)
            {
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateChequeBook(formData.ChequeBookCompNo, formData.ChequeBookNo, formData.FromSerialNo, formData.ToSerialNo, formData.ContainFromZero, formData.ContainToZero, formData.IssuedDate, formData.DetailCode, formData.Status,User.UserId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Cheque Book Successfully Updated";
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
                db.spInsertUpdateChequeBook(formData.ChequeBookCompNo, formData.ChequeBookNo, formData.FromSerialNo, formData.ToSerialNo, formData.ContainFromZero, formData.ContainToZero, formData.IssuedDate, formData.DetailCode, formData.Status, User.UserId, "Save", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Cheque Book Successfully Saved";
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
        public ActionResult ChequeBooksList()
        {
            return PartialView();
        }
        public ActionResult GetAllChequeBooks()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var chequeNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var detail = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCheque> cheque = (from f in db.AC_ChequeBook
                                          where f.Status == "Pending"
                                          select new clsCheque
                                          {
                                              ChequeBookCompNo = f.ChequeBookCompNo,
                                              ChequeBookNo = f.ChequeBookNo,
                                              FromSerialNo = f.FromSerialNo,
                                              ToSerialNo = f.ToSerialNo,
                                              IssuedDate = f.IssuedDate,
                                              DetailCode = f.DetailCode,
                                              DetailName = db.AC_Detail.Where(x => x.DetailCode == f.DetailCode).Select(x => x.Name).FirstOrDefault(),
                                              Status = f.Status
                                          }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(chequeNo))
                {
                    cheque = cheque.Where(c => c.ChequeBookNo.Contains(chequeNo)).ToList();
                }
                else if (!string.IsNullOrEmpty(detail))
                {
                    cheque = cheque.Where(c => c.DetailName.ToLower().Contains(detail.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        cheque = cheque.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = cheque.Count();
                var data = cheque.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetChequeBookDetailById(int id)
        {
            clsCheque cheque = new clsCheque();
            cheque = (from f in db.AC_ChequeBook
                      where f.ChequeBookCompNo == id
                      select new clsCheque
                      {
                          ChequeBookCompNo = f.ChequeBookCompNo,
                          ChequeBookNo = f.ChequeBookNo,
                          FromSerialNo = f.FromSerialNo,
                          ToSerialNo = f.ToSerialNo,
                          IssuedDate = f.IssuedDate,
                          DetailCode = f.DetailCode,
                          Status = f.Status
                      }).FirstOrDefault();
            var data = cheque;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteChequeBook(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateChequeBook(id, "", "", "", "", "", DateTime.Now, 0, "", User.UserId, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Cheque Book Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this cheque book first.";
            }
            else
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public List<clsAccountDetail> getDetailCodes()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      join ac in db.AC_DefaultGL on c.AccountCode equals ac.AccountCode
                      where c.Status == "Active" && ac.Type == "Bank"
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name
                      }).ToList();
            return detail;
        }
        #endregion
        #region Cheque Payment
        public ActionResult ChequePayment()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "ChequePayment"
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
                    ViewBag.AllDetailCode = new SelectList(getAllDetailCodes(), "DetailCode", "Name");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateChequePayment(clsCheque formData)
        {
            string message = "";
            bool status = false;
            int chequeInfoId = 0;
            if (formData.ChequeBookDId != null && formData.ChequeBookDId != 0)
            {
                formData.ChequeNo = db.AC_ChequeBookDetail.Where(x => x.ChequeBookDId == formData.ChequeBookDId).Select(x => x.ChequeNo).FirstOrDefault();
            }
            if (formData.ChequeInfoId > 0)
            {
                chequeInfoId = formData.ChequeInfoId;
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateChequeInfo(formData.ChequeInfoId, formData.ChequeNo, "Paid", formData.ChequeDate, formData.DueDate, formData.DetailName, formData.ChequeType, formData.Amount, formData.CDetailCode, formData.DDetailCode, formData.Status, formData.Remarks, formData.DepositRemarks, formData.ClearRemarks, formData.ClearDate, formData.ReturnRemarks, formData.ReceivedBy, formData.PaidBy, formData.DepositReceivedBy, formData.RefVoucherNo, User.UserId, User.UserId.ToString(), DateTime.Now, DateTime.Now, 0, User.FiscalYearId, formData.ChequeBookDId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Cheque Payment Successfully Updated";
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
                db.spInsertUpdateChequeInfo(formData.ChequeInfoId, formData.ChequeNo, "Paid", formData.ChequeDate, formData.DueDate, formData.DetailName, formData.ChequeType, formData.Amount, formData.CDetailCode, formData.DDetailCode, formData.Status, formData.Remarks, formData.DepositRemarks, formData.ClearRemarks, formData.ClearDate, formData.ReturnRemarks, formData.ReceivedBy, formData.PaidBy, formData.DepositReceivedBy, formData.RefVoucherNo, User.UserId, User.UserId.ToString(), DateTime.Now, DateTime.Now, 0, User.FiscalYearId, formData.ChequeBookDId, "Save", CheckReturn);
                chequeInfoId = Convert.ToInt32(CheckReturn.Value);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Cheque Payment Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, chequeInfoId = chequeInfoId } };
        }
        public ActionResult ChequePaymentsList()
        {
            return PartialView();
        }
        public ActionResult GetAllChequePayments()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var chequeNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var bank = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var paidTo = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var paidBy = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var receivedBy = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCheque> cheque = (from f in db.AC_ChequeInfo
                                          where f.Status == "Unpost" || f.Status == "Deposit" || f.Status == "Issued" && f.Type == "Paid"
                                          select new clsCheque
                                          {
                                              ChequeInfoId = f.ChequeInfoId,
                                              ChequeNo = f.ChequeNo,
                                              Type = f.Type,
                                              ChequeDate = f.ChequeDate,
                                              DueDate = f.DueDate,
                                              Bank = f.Bank,
                                              ChequeType = f.ChequeType,
                                              Amount = f.Amount,
                                              DDetailCode = f.DDetailCode,
                                              CDetailCode = f.CDetailCode,
                                              PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.DDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                                              Status = f.Status,
                                              Remarks = f.Remarks,
                                              PaidBy = f.PaidBy,
                                              ReceivedBy = f.ReceivedBy,
                                              FiscalYearID = f.FiscalYearID
                                          }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(chequeNo))
                {
                    cheque = cheque.Where(c => c.ChequeNo.ToLower().Contains(chequeNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(bank))
                {
                    cheque = cheque.Where(c => c.Bank.ToLower().Contains(bank.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidTo))
                {
                    cheque = cheque.Where(c => c.PaidTo.ToLower().Contains(paidTo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    cheque = cheque.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidBy))
                {
                    cheque = cheque.Where(c => c.PaidBy.ToLower().Contains(paidBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(receivedBy))
                {
                    cheque = cheque.Where(c => c.ReceivedBy.ToLower().Contains(receivedBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cheque = cheque.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        cheque = cheque.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = cheque.Count();
                var data = cheque.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetChequePaymentDetailById(int id)
        {
            clsCheque cheque = new clsCheque();
            cheque = (from f in db.AC_ChequeInfo
                      where f.ChequeInfoId == id
                      select new clsCheque
                      {
                          ChequeInfoId = f.ChequeInfoId,
                          ChequeBookDId = f.ChequeBookDId,
                          ChequeNo = f.ChequeNo,
                          Type = f.Type,
                          ChequeDate = f.ChequeDate,
                          DueDate = f.DueDate,
                          Bank = f.Bank,
                          ChequeType = f.ChequeType,
                          Amount = f.Amount,
                          DDetailCode = f.DDetailCode,
                          CDetailCode = f.CDetailCode,
                          Status = f.Status,
                          Remarks = f.Remarks,
                          PaidBy = f.PaidBy,
                          ReceivedBy = f.ReceivedBy,
                          FiscalYearID = f.FiscalYearID
                      }).FirstOrDefault();
            var data = cheque;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
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
                      }).ToList();
            return detail;
        }
        public ActionResult getChequeBookDetailByDetailCode(decimal id)
        {
            List<clsCheque> chequeBook = new List<clsCheque>();
            chequeBook = (from c in db.AC_ChequeBook
                          join cd in db.AC_ChequeBookDetail on c.ChequeBookCompNo equals cd.ChequeBookCompNo
                          where c.DetailCode == id && cd.Status == "Unpost"
                          select new clsCheque
                          {
                              ChequeBookDId = cd.ChequeBookDId,
                              ChequeNo = cd.ChequeNo
                          }).ToList();
            var data = chequeBook.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getChequeBookDetailByDetailCodeForUpdate(decimal id)
        {
            List<clsCheque> chequeBook = new List<clsCheque>();
            chequeBook = (from c in db.AC_ChequeBook
                          join cd in db.AC_ChequeBookDetail on c.ChequeBookCompNo equals cd.ChequeBookCompNo
                          where c.DetailCode == id
                          select new clsCheque
                          {
                              ChequeBookDId = cd.ChequeBookDId,
                              ChequeNo = cd.ChequeNo
                          }).ToList();
            var data = chequeBook.ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteChequePayment(int id)
        {
            string message = "";
            bool status = false;
            string chequeNo = db.AC_ChequeInfo.Where(x => x.ChequeInfoId == id).Select(x => x.ChequeNo).FirstOrDefault();
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateChequeInfo(id, chequeNo, "Paid", DateTime.Now, DateTime.Now, "", "", 0, 0, 0, "", "", "", "", DateTime.Now, "", "", "", "", "", User.UserId, User.UserId.ToString(), DateTime.Now, DateTime.Now, 0, User.FiscalYearId, 0, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Cheque Payment Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this cheque payment first.";
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
        #region Cheque Payment Report
        public ActionResult ChequePaymentReport(int id)
        {
            if (User != null)
            {
               clsCheque cheque = (from f in db.AC_ChequeInfo
                                          where f.ChequeInfoId == id
                                          select new clsCheque
                                          {
                                              ChequeInfoId = f.ChequeInfoId,
                                              ChequeNo = f.ChequeNo ?? string.Empty,
                                              Type = f.Type ?? string.Empty,
                                              ChequeDate = f.ChequeDate,
                                              DueDate = f.DueDate,
                                              Bank = f.Bank ?? string.Empty,
                                              ChequeType = f.ChequeType ?? string.Empty,
                                              Amount = f.Amount ?? 0,
                                              PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.DDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                                              Status = f.Status ?? string.Empty,
                                              Remarks = f.Remarks ?? string.Empty,
                                              PaidBy = f.PaidBy ?? string.Empty,
                                              ReceivedBy = f.ReceivedBy ?? string.Empty,
                                              FiscalYearID = f.FiscalYearID
                                          }).FirstOrDefault();
                return View(cheque);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        #region Cheque Paymnent Clearance
        public ActionResult ChequePaymentClearance()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "ChequeClearance"
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
        public ActionResult GetAllChequePaymentsForClearance()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var chequeNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var bank = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var paidTo = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var paidBy = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var receivedBy = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
                var type = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCheque> cheque = (from f in db.AC_ChequeInfo
                                          where f.Status == "Deposit" || f.Status == "Issued" || f.Status == "Forward"
                                          select new clsCheque
                                          {
                                              ChequeInfoId = f.ChequeInfoId,
                                              ChequeNo = f.ChequeNo,
                                              Type = f.Type,
                                              ChequeDate = f.ChequeDate,
                                              DueDate = f.DueDate,
                                              Bank = f.Bank,
                                              ChequeType = f.ChequeType,
                                              Amount = f.Amount,
                                              DDetailCode = f.DDetailCode,
                                              CDetailCode = f.CDetailCode,
                                              PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.DDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                                              Status = f.Status,
                                              Remarks = f.Remarks,
                                              PaidBy = f.PaidBy,
                                              ReceivedBy = f.ReceivedBy,
                                              FiscalYearID = f.FiscalYearID
                                          }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(chequeNo))
                {
                    cheque = cheque.Where(c => c.ChequeNo.ToLower().Contains(chequeNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(bank))
                {
                    cheque = cheque.Where(c => c.Bank.ToLower().Contains(bank.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidTo))
                {
                    cheque = cheque.Where(c => c.PaidTo.ToLower().Contains(paidTo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    cheque = cheque.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidBy))
                {
                    cheque = cheque.Where(c => c.PaidBy.ToLower().Contains(paidBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(receivedBy))
                {
                    cheque = cheque.Where(c => c.ReceivedBy.ToLower().Contains(receivedBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cheque = cheque.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cheque = cheque.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(type))
                {
                    cheque = cheque.Where(c => c.Type.ToLower().Contains(type.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        cheque = cheque.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = cheque.Count();
                var data = cheque.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult ChequePaymentClearanceListForEdit()
        {
            return PartialView();
        }
        public ActionResult GetAllChequePaymentClearanceListForEdit()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var chequeNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var bank = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var paidTo = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var paidBy = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var receivedBy = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
                var type = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCheque> cheque = (from f in db.AC_ChequeInfo
                                          where f.Status == "Cancel" || f.Status == "Return" || f.Status == "Clear"
                                          select new clsCheque
                                          {
                                              ChequeInfoId = f.ChequeInfoId,
                                              ChequeNo = f.ChequeNo,
                                              Type = f.Type,
                                              ChequeDate = f.ChequeDate,
                                              DueDate = f.DueDate,
                                              Bank = f.Bank,
                                              ChequeType = f.ChequeType,
                                              Amount = f.Amount,
                                              DDetailCode = f.DDetailCode,
                                              CDetailCode = f.CDetailCode,
                                              PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.DDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                                              Status = f.Status,
                                              Remarks = f.Remarks,
                                              PaidBy = f.PaidBy,
                                              ReceivedBy = f.ReceivedBy,
                                              FiscalYearID = f.FiscalYearID,
                                              ClearRemarks = f.ClearRemarks,
                                              ClearDate = f.ClearDate
                                          }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(chequeNo))
                {
                    cheque = cheque.Where(c => c.ChequeNo.ToLower().Contains(chequeNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(bank))
                {
                    cheque = cheque.Where(c => c.Bank.ToLower().Contains(bank.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidTo))
                {
                    cheque = cheque.Where(c => c.PaidTo.ToLower().Contains(paidTo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    cheque = cheque.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidBy))
                {
                    cheque = cheque.Where(c => c.PaidBy.ToLower().Contains(paidBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(receivedBy))
                {
                    cheque = cheque.Where(c => c.ReceivedBy.ToLower().Contains(receivedBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cheque = cheque.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cheque = cheque.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(type))
                {
                    cheque = cheque.Where(c => c.Type.ToLower().Contains(type.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        cheque = cheque.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = cheque.Count();
                var data = cheque.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetChequePaymentDetailByIdForClearance(int id)
        {
            clsCheque cheque = new clsCheque();
            cheque = (from f in db.AC_ChequeInfo
                      where f.ChequeInfoId == id
                      select new clsCheque
                      {
                          ChequeInfoId = f.ChequeInfoId,
                          ChequeBookDId = f.ChequeBookDId,
                          ChequeNo = f.ChequeNo,
                          Type = f.Type,
                          ChequeDate = f.ChequeDate,
                          DueDate = f.DueDate,
                          Bank = f.Bank,
                          PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.DDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                          ChequeType = f.ChequeType,
                          Amount = f.Amount,
                          DDetailCode = f.DDetailCode,
                          CDetailCode = f.CDetailCode,
                          Status = f.Status,
                          Remarks = f.Remarks,
                          PaidBy = f.PaidBy,
                          ReceivedBy = f.ReceivedBy,
                          FiscalYearID = f.FiscalYearID
                      }).FirstOrDefault();
            var data = cheque;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetChequePaymentDetailByIdForEdit(int id)
        {
            clsCheque cheque = new clsCheque();
            cheque = (from f in db.AC_ChequeInfo
                      where f.ChequeInfoId == id
                      select new clsCheque
                      {
                          ChequeInfoId = f.ChequeInfoId,
                          ChequeBookDId = f.ChequeBookDId,
                          ChequeNo = f.ChequeNo,
                          Type = f.Type,
                          ChequeDate = f.ChequeDate,
                          DueDate = f.DueDate,
                          Bank = f.Bank,
                          PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.DDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                          ChequeType = f.ChequeType,
                          Amount = f.Amount,
                          DDetailCode = f.DDetailCode,
                          CDetailCode = f.CDetailCode,
                          Status = f.Status,
                          Remarks = f.Remarks,
                          PaidBy = f.PaidBy,
                          ReceivedBy = f.ReceivedBy,
                          FiscalYearID = f.FiscalYearID,
                          ClearDate = f.ClearDate,
                          ClearRemarks = f.ClearRemarks
                      }).FirstOrDefault();
            var data = cheque;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult InsertUpdateChequePaymentClearance(clsCheque formData)
        {
            string message = "";
            bool status = false;
            //if (formData.ChequeInfoId > 0)
            //{
                string maxCheuqueCode="";
                if (formData.Status == "Clear")
                {
                    maxCheuqueCode = getMaxChequeCode();
                }
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateChequeClearance(formData.ChequeInfoId, formData.Status,  formData.ClearRemarks, formData.ClearDate,User.UserId, User.UserId.ToString(), DateTime.Now,@maxCheuqueCode,0,User.FiscalYearId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Cheque Payment Cleared Successfully Updated";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            //}
            //else
            //{
            //    System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            //    db.spInsertUpdateChequePayment(formData.ChequeInfoId, formData.ChequeNo, "Issue", formData.ChequeDate, formData.DueDate, formData.DetailName, formData.ChequeType, formData.Amount, formData.CDetailCode, formData.DDetailCode, formData.Status, formData.Remarks, formData.DepositRemarks, formData.ClearRemarks, formData.ClearDate, formData.ReturnRemarks, formData.ReceivedBy, formData.PaidBy, formData.DepositReceivedBy, formData.RefVoucherNo, User.UserId, User.UserId.ToString(), DateTime.Now, DateTime.Now, 0, User.FiscalYearId, formData.ChequeBookDId, "Save", CheckReturn);
            //    if (Convert.ToDecimal(CheckReturn.Value) > 0)
            //    {
            //        ModelState.Clear();
            //        status = true;
            //        message = "Cheque Payment Successfully Saved";
            //    }
            //    else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            //    {
            //        ModelState.Clear();
            //        status = false;
            //        message = "Error! Please try again.";
            //    }
            //}
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public string getMaxChequeCode()
        {
            string maxCode = "";
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            if (currentMonth.Length == 1)
            {
                currentMonth = "0" + currentMonth;
            }
            currentYear = currentYear.Substring(2, 2);
            string voucherNo = "CV-" + currentYear + currentMonth;
            int count = db.AC_CashBank_Main.Where(x => x.VoucherType == "Cheque Voucher").Count();
            if (count > 0)
            {
                maxCode = db.AC_CashBank_Main.Where(x => x.VoucherType == "Cheque Voucher").OrderByDescending(x => x.VoucherNo).Select(x => x.VoucherNo).FirstOrDefault();

                maxCode = maxCode.Split('-')[1];
                maxCode = maxCode.Substring(4, 4);
                int codeInIntegar = Convert.ToInt32(maxCode);
                codeInIntegar += 1;
                if (codeInIntegar.ToString().Length == 1)
                {
                    maxCode = voucherNo + "000" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 2)
                {
                    maxCode = voucherNo + "00" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 3)
                {
                    maxCode = voucherNo + "0" + codeInIntegar.ToString();
                }
                else if (codeInIntegar.ToString().Length == 4)
                {
                    maxCode = voucherNo + codeInIntegar.ToString();
                }
            }
            else
            {
                maxCode = voucherNo + "0001";
            }

            return maxCode;
        }
        public ActionResult UnpostChequeFromClearance(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateChequeClearance(id, "Unpost", "", null, User.UserId, User.UserId.ToString(), DateTime.Now,"",0,0, "Update", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 1)
            {
                ModelState.Clear();
                status = true;
                message = "Cheque Successfully Unposted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this cheque first.";
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
        #region Cheque Received
        public ActionResult ChequeReceived()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "ChequeReceived"
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
                    ViewBag.AllDetailCode = new SelectList(getAllDetailCodes(), "DetailCode", "Name");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        [HttpPost]
        public ActionResult InsertUpdateChequeReceived(clsCheque formData)
        {
            string message = "";
            bool status = false;
            int chequeInfoId = 0;
            if (formData.ChequeInfoId > 0)
            {
                chequeInfoId = formData.ChequeInfoId;
                System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
                db.spInsertUpdateChequeInfo(formData.ChequeInfoId, formData.ChequeNo, "Received", formData.ChequeDate, formData.DueDate, formData.DetailName, formData.ChequeType, formData.Amount, formData.CDetailCode, formData.DDetailCode, "Unpost", formData.Remarks, formData.DepositRemarks, formData.ClearRemarks, formData.ClearDate, formData.ReturnRemarks, formData.ReceivedBy, formData.PaidBy, formData.DepositReceivedBy, formData.RefVoucherNo, User.UserId, User.UserId.ToString(), DateTime.Now, DateTime.Now, 0, User.FiscalYearId, formData.ChequeBookDId, "Update", CheckReturn);
                if (Convert.ToDecimal(CheckReturn.Value) == 1)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Cheque Receive Successfully Updated";
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
                db.spInsertUpdateChequeInfo(formData.ChequeInfoId, formData.ChequeNo, "Received", formData.ChequeDate, formData.DueDate, formData.DetailName, formData.ChequeType, formData.Amount, formData.CDetailCode, formData.DDetailCode, "Unpost", formData.Remarks, formData.DepositRemarks, formData.ClearRemarks, formData.ClearDate, formData.ReturnRemarks, formData.ReceivedBy, formData.PaidBy, formData.DepositReceivedBy, formData.RefVoucherNo, User.UserId, User.UserId.ToString(), DateTime.Now, DateTime.Now, 0, User.FiscalYearId, formData.ChequeBookDId, "Save", CheckReturn);
                chequeInfoId = Convert.ToInt32(CheckReturn.Value);
                if (Convert.ToDecimal(CheckReturn.Value) > 0)
                {
                    ModelState.Clear();
                    status = true;
                    message = "Cheque Receive Successfully Saved";
                }
                else if (Convert.ToInt32(CheckReturn.Value) == -9999)
                {
                    ModelState.Clear();
                    status = false;
                    message = "Error! Please try again.";
                }
            }
            return new JsonResult { Data = new { status = status, message = message, chequeInfoId = chequeInfoId } };
        }
        public ActionResult ChequeReceivedList()
        {
            return PartialView();
        }
        public ActionResult GetAllChequeReceived()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var chequeNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var bank = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var paidTo = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var paidBy = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var receivedBy = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCheque> cheque = (from f in db.AC_ChequeInfo
                                          where f.Status == "Unpost" && f.Type == "Received"
                                          select new clsCheque
                                          {
                                              ChequeInfoId = f.ChequeInfoId,
                                              ChequeNo = f.ChequeNo,
                                              Type = f.Type,
                                              ChequeDate = f.ChequeDate,
                                              DueDate = f.DueDate,
                                              Bank = f.Bank,
                                              ChequeType = f.ChequeType,
                                              Amount = f.Amount,
                                              DDetailCode = f.DDetailCode,
                                              CDetailCode = f.CDetailCode,
                                              PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.CDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                                              Status = f.Status,
                                              Remarks = f.Remarks,
                                              PaidBy = f.PaidBy,
                                              ReceivedBy = f.ReceivedBy,
                                              FiscalYearID = f.FiscalYearID
                                          }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(chequeNo))
                {
                    cheque = cheque.Where(c => c.ChequeNo.ToLower().Contains(chequeNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(bank))
                {
                    cheque = cheque.Where(c => c.Bank.ToLower().Contains(bank.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidTo))
                {
                    cheque = cheque.Where(c => c.PaidTo.ToLower().Contains(paidTo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    cheque = cheque.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidBy))
                {
                    cheque = cheque.Where(c => c.PaidBy.ToLower().Contains(paidBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(receivedBy))
                {
                    cheque = cheque.Where(c => c.ReceivedBy.ToLower().Contains(receivedBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cheque = cheque.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        cheque = cheque.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = cheque.Count();
                var data = cheque.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetChequeReceivedDetailById(int id)
        {
            clsCheque cheque = new clsCheque();
            cheque = (from f in db.AC_ChequeInfo
                      where f.ChequeInfoId == id
                      select new clsCheque
                      {
                          ChequeInfoId = f.ChequeInfoId,
                          ChequeBookDId = f.ChequeBookDId,
                          ChequeNo = f.ChequeNo,
                          Type = f.Type,
                          ChequeDate = f.ChequeDate,
                          DueDate = f.DueDate,
                          Bank = f.Bank,
                          ChequeType = f.ChequeType,
                          Amount = f.Amount,
                          DDetailCode = f.DDetailCode,
                          CDetailCode = f.CDetailCode,
                          Status = f.Status,
                          Remarks = f.Remarks,
                          PaidBy = f.PaidBy,
                          ReceivedBy = f.ReceivedBy,
                          FiscalYearID = f.FiscalYearID
                      }).FirstOrDefault();
            var data = cheque;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteChequeReceived(int id)
        {
            string message = "";
            bool status = false;
            string chequeNo = db.AC_ChequeInfo.Where(x => x.ChequeInfoId == id).Select(x => x.ChequeNo).FirstOrDefault();
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateChequeInfo(id, chequeNo, "Received", DateTime.Now, DateTime.Now, "", "", 0, 0, 0, "", "", "", "", DateTime.Now, "", "", "", "", "", User.UserId, User.UserId.ToString(), DateTime.Now, DateTime.Now, 0, User.FiscalYearId, 0, "Delete", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 2)
            {
                ModelState.Clear();
                status = true;
                message = "Cheque Receive Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this cheque receive first.";
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
        #region Cheque Received Report
        public ActionResult ChequeReceiveReport(int id)
        {
            if (User != null)
            {
                clsCheque cheque = (from f in db.AC_ChequeInfo
                                    where f.ChequeInfoId == id
                                    select new clsCheque
                                    {
                                        ChequeInfoId = f.ChequeInfoId,
                                        ChequeNo = f.ChequeNo ?? string.Empty,
                                        Type = f.Type ?? string.Empty,
                                        ChequeDate = f.ChequeDate,
                                        DueDate = f.DueDate,
                                        Bank = f.Bank ?? string.Empty,
                                        ChequeType = f.ChequeType ?? string.Empty,
                                        Amount = f.Amount ?? 0,
                                        PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.DDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                                        Status = f.Status ?? string.Empty,
                                        Remarks = f.Remarks ?? string.Empty,
                                        PaidBy = f.PaidBy ?? string.Empty,
                                        ReceivedBy = f.ReceivedBy ?? string.Empty,
                                        FiscalYearID = f.FiscalYearID
                                    }).FirstOrDefault();
                return View(cheque);
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        #region Cheque Deposit
        public ActionResult ChequeDeposit()
        {
            if (User != null)
            {
                clsUserRight rights = new clsUserRight();
                rights = (from r in db.Admin_Setup_UserRights
                          where r.UserID == User.UserId && r.MenuName == "ChequeDeposit"
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
                    ViewBag.AllDetailCode = new SelectList(getAllDetailCodes(), "DetailCode", "Name");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetAllChequeReceivedForDeposit()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var chequeNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var bank = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var paidTo = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var paidBy = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var receivedBy = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
                var type = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCheque> cheque = (from f in db.AC_ChequeInfo
                                          where f.Status == "Unpost" && f.Type == "Received"
                                          select new clsCheque
                                          {
                                              ChequeInfoId = f.ChequeInfoId,
                                              ChequeNo = f.ChequeNo,
                                              Type = f.Type,
                                              ChequeDate = f.ChequeDate,
                                              DueDate = f.DueDate,
                                              Bank = f.Bank,
                                              ChequeType = f.ChequeType,
                                              Amount = f.Amount,
                                              DDetailCode = f.DDetailCode,
                                              CDetailCode = f.CDetailCode,
                                              PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.CDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                                              Status = f.Status,
                                              Remarks = f.Remarks,
                                              PaidBy = f.PaidBy,
                                              ReceivedBy = f.ReceivedBy,
                                              FiscalYearID = f.FiscalYearID
                                          }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(chequeNo))
                {
                    cheque = cheque.Where(c => c.ChequeNo.ToLower().Contains(chequeNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(bank))
                {
                    cheque = cheque.Where(c => c.Bank.ToLower().Contains(bank.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidTo))
                {
                    cheque = cheque.Where(c => c.PaidTo.ToLower().Contains(paidTo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    cheque = cheque.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidBy))
                {
                    cheque = cheque.Where(c => c.PaidBy.ToLower().Contains(paidBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(receivedBy))
                {
                    cheque = cheque.Where(c => c.ReceivedBy.ToLower().Contains(receivedBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cheque = cheque.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cheque = cheque.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(type))
                {
                    cheque = cheque.Where(c => c.Type.ToLower().Contains(type.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        cheque = cheque.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = cheque.Count();
                var data = cheque.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetChequeDepositDetailByIdForDeposit(int id)
        {
            clsCheque cheque = new clsCheque();
            cheque = (from f in db.AC_ChequeInfo
                      where f.ChequeInfoId == id
                      select new clsCheque
                      {
                          ChequeInfoId = f.ChequeInfoId,
                          ChequeBookDId = f.ChequeBookDId,
                          ChequeNo = f.ChequeNo,
                          Type = f.Type,
                          ChequeDate = f.ChequeDate,
                          DueDate = f.DueDate,
                          Bank = f.Bank,
                          PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.CDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                          ChequeType = f.ChequeType,
                          Amount = f.Amount,
                          DDetailCode = f.DDetailCode,
                          CDetailCode = f.CDetailCode,
                          Status = f.Status,
                          Remarks = f.Remarks,
                          PaidBy = f.PaidBy,
                          ReceivedBy = f.ReceivedBy,
                          FiscalYearID = f.FiscalYearID
                      }).FirstOrDefault();
            var data = cheque;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult InsertUpdateChequeDeposit(clsCheque formData)
        {
            string message = "";
            bool status = false;
            //if (formData.ChequeInfoId > 0)
            //{
            string maxCheuqueCode = "";
            maxCheuqueCode = getMaxChequeCode();
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            db.spInsertUpdateChequeDeposit(formData.ChequeInfoId, formData.DDetailCode, formData.Status, formData.DepositRemarks, formData.DepositDate, User.UserId.ToString(), DateTime.Now, maxCheuqueCode, "Update", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 1)
            {
                ModelState.Clear();
                status = true;
                message = "Cheque Deposit Successfully Updated";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please try again.";
            }
            //}
            //else
            //{
            //    System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(decimal));
            //    db.spInsertUpdateChequePayment(formData.ChequeInfoId, formData.ChequeNo, "Issue", formData.ChequeDate, formData.DueDate, formData.DetailName, formData.ChequeType, formData.Amount, formData.CDetailCode, formData.DDetailCode, formData.Status, formData.Remarks, formData.DepositRemarks, formData.ClearRemarks, formData.ClearDate, formData.ReturnRemarks, formData.ReceivedBy, formData.PaidBy, formData.DepositReceivedBy, formData.RefVoucherNo, User.UserId, User.UserId.ToString(), DateTime.Now, DateTime.Now, 0, User.FiscalYearId, formData.ChequeBookDId, "Save", CheckReturn);
            //    if (Convert.ToDecimal(CheckReturn.Value) > 0)
            //    {
            //        ModelState.Clear();
            //        status = true;
            //        message = "Cheque Payment Successfully Saved";
            //    }
            //    else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            //    {
            //        ModelState.Clear();
            //        status = false;
            //        message = "Error! Please try again.";
            //    }
            //}
            return new JsonResult { Data = new { status = status, message = message } };
        }
        public ActionResult ChequeDepositListForEdit()
        {
            return PartialView();
        }
        public ActionResult GetAllChequeDepositListForEdit()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var chequeNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var bank = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var paidTo = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();
                var paidBy = Request.Form.GetValues("columns[4][search][value]").FirstOrDefault();
                var receivedBy = Request.Form.GetValues("columns[5][search][value]").FirstOrDefault();
                var status = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
                var creditAccount = Request.Form.GetValues("columns[7][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                List<clsCheque> cheque = (from f in db.AC_ChequeInfo
                                          where f.Status == "Deposit" || f.Status == "Forward" || f.Status == "Return" && f.Type == "Received"
                                          select new clsCheque
                                          {
                                              ChequeInfoId = f.ChequeInfoId,
                                              ChequeNo = f.ChequeNo,
                                              Type = f.Type,
                                              ChequeDate = f.ChequeDate,
                                              DueDate = f.DueDate,
                                              Bank = f.Bank,
                                              ChequeType = f.ChequeType,
                                              Amount = f.Amount,
                                              DDetailCode = f.DDetailCode,
                                              CDetailCode = f.CDetailCode,
                                              DetailName = db.AC_Detail.Where(x => x.DetailCode == f.CDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                                              PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.DDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                                              Status = f.Status,
                                              Remarks = f.Remarks,
                                              PaidBy = f.PaidBy,
                                              ReceivedBy = f.ReceivedBy,
                                              FiscalYearID = f.FiscalYearID,
                                              DepositRemarks = f.DepositRemarks,
                                              DepositDate = f.DepositDate
                                          }).ToList();
                //SEARCHING...
                if (!string.IsNullOrEmpty(chequeNo))
                {
                    cheque = cheque.Where(c => c.ChequeNo.ToLower().Contains(chequeNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(bank))
                {
                    cheque = cheque.Where(c => c.Bank.ToLower().Contains(bank.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidTo))
                {
                    cheque = cheque.Where(c => c.PaidTo.ToLower().Contains(paidTo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    cheque = cheque.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(paidBy))
                {
                    cheque = cheque.Where(c => c.PaidBy.ToLower().Contains(paidBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(receivedBy))
                {
                    cheque = cheque.Where(c => c.ReceivedBy.ToLower().Contains(receivedBy.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cheque = cheque.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    cheque = cheque.Where(c => c.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(creditAccount))
                {
                    cheque = cheque.Where(c => c.DetailName.ToLower().Contains(creditAccount.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        cheque = cheque.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = cheque.Count();
                var data = cheque.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetChequeDepositDetailByIdForEdit(int id)
        {
            clsCheque cheque = new clsCheque();
            cheque = (from f in db.AC_ChequeInfo
                      where f.ChequeInfoId == id
                      select new clsCheque
                      {
                          ChequeInfoId = f.ChequeInfoId,
                          ChequeBookDId = f.ChequeBookDId,
                          ChequeNo = f.ChequeNo,
                          Type = f.Type,
                          ChequeDate = f.ChequeDate,
                          DueDate = f.DueDate,
                          Bank = f.Bank,
                          PaidTo = db.AC_Detail.Where(x => x.DetailCode == f.DDetailCode).Select(x => x.Name).FirstOrDefault() ?? string.Empty,
                          ChequeType = f.ChequeType,
                          Amount = f.Amount,
                          DDetailCode = f.DDetailCode,
                          CDetailCode = f.CDetailCode,
                          Status = f.Status,
                          Remarks = f.Remarks,
                          PaidBy = f.PaidBy,
                          ReceivedBy = f.ReceivedBy,
                          FiscalYearID = f.FiscalYearID,
                          DepositDate = f.DepositDate,
                          DepositRemarks = f.DepositRemarks
                      }).FirstOrDefault();
            var data = cheque;
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UnpostDepositedCheque(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateChequeDeposit(id,0, "Unpost", "", null, User.UserId.ToString(), DateTime.Now, "","Update", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 1)
            {
                ModelState.Clear();
                status = true;
                message = "City Successfully Deleted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this city first.";
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
        #region Cheque Due Date Alert
        public ActionResult ChequeDueDatesAlert()
        {
            if (User != null)
            {
                    return View();
            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        public ActionResult GetAllChequeDueDateAlerts()
        {
            if (User != null)
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                        + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var chequeNo = Request.Form.GetValues("columns[0][search][value]").FirstOrDefault();
                var bank = Request.Form.GetValues("columns[1][search][value]").FirstOrDefault();
                var remarks = Request.Form.GetValues("columns[2][search][value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt16(start) : 0;
                int recordsTotal = 0;
                DateTime todayDate = DateTime.Now.Date;
                DateTime nextTwoDaysDate = todayDate.AddDays(2);
                List<clsCheque> cheque = (from f in db.AC_ChequeInfo
                                          where 
                                          f.DueDate <= nextTwoDaysDate 
                                          && (f.Status == "Post" || f.Status == "Unpost" || f.Status == "Issued")
                                          select new clsCheque
                                          {
                                              ChequeNo = f.ChequeNo ?? string.Empty,
                                              Type = f.Type,
                                              ChequeDate = f.ChequeDate,
                                              DueDate = f.DueDate,
                                              Bank = f.Bank ?? string.Empty,
                                              ChequeType = f.ChequeType,
                                              Amount = f.Amount,
                                              Status = f.Status,
                                              Remarks = f.Remarks ?? string.Empty,
                                              PaidBy = f.PaidBy,
                                              ReceivedBy = f.ReceivedBy,
                                          }).ToList();
                foreach (clsCheque c in cheque)
                {
                    DateTime dueDatee = Convert.ToDateTime(c.DueDate);
                    if (DateTime.Compare(dueDatee.Date, todayDate.Date) < 0)
                    {
                        double remainingdayss = DateTime.Now.Date.Subtract(dueDatee.Date).Days;
                        if (remainingdayss == 1)
                        {
                            c.StatusCountDays = "1";
                        }
                        else
                        {
                            c.StatusCountDays = "2-" + remainingdayss;
                        }
                    }
                    if (DateTime.Compare(dueDatee.Date, todayDate.Date) == 0)
                    {
                        c.StatusCountDays = "3";
                    }
                    if (DateTime.Compare(dueDatee.Date, todayDate.Date) > 0)
                    {
                        double remainingdayss = dueDatee.Date.Subtract(DateTime.Now.Date).Days;
                        if (remainingdayss == 1)
                        {
                            c.StatusCountDays = "4";
                        }
                        else
                        {
                            c.StatusCountDays = "5-" + remainingdayss;
                        }
                    }
                }
                //SEARCHING...
                if (!string.IsNullOrEmpty(chequeNo))
                {
                    cheque = cheque.Where(c => c.ChequeNo.ToLower().Contains(chequeNo.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(bank))
                {
                    cheque = cheque.Where(c => c.Bank.ToLower().Contains(bank.ToLower())).ToList();
                }
                else if (!string.IsNullOrEmpty(remarks))
                {
                    cheque = cheque.Where(c => c.Remarks.ToLower().Contains(remarks.ToLower())).ToList();
                }
                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        cheque = cheque.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                    }
                }
                recordsTotal = cheque.Count();
                var data = cheque.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data },
                    JsonRequestBehavior.AllowGet);

            }
            else
            {
                return RedirectToAction("Index", "account");
            }
        }
        #endregion
        #region Cheque Summary Report
        public ActionResult ChequeSummary()
        {
            ViewBag.DetailCode = new SelectList(getAllDetailCodesForReport(), "DetailCode", "Name");
            ViewBag.Type = new SelectList(getType(), "Value", "Text");
            ViewBag.Status = new SelectList(getStatus(), "Value", "Text");
            return View();
        }
        public List<clsAccountDetail> getAllDetailCodesForReport()
        {
            List<clsAccountDetail> detail = new List<clsAccountDetail>();
            detail = (from c in db.AC_Detail
                      where c.Status == "Active"
                      select new clsAccountDetail
                      {
                          DetailCode = c.DetailCode,
                          Name = c.Name
                      }).ToList();
            return detail;
        }
        public List<SelectListItem> getType()
        {
            List<SelectListItem> nationality = new List<SelectListItem>();
            nationality.Add(new SelectListItem { Text = "Paid", Value = "Paid" });
            nationality.Add(new SelectListItem { Text = "Received", Value = "Received" });
            return nationality;
        }
        public List<SelectListItem> getStatus()
        {
            List<SelectListItem> nationality = new List<SelectListItem>();
            nationality.Add(new SelectListItem { Text = "Unpost", Value = "Unpost" });
            nationality.Add(new SelectListItem { Text = "Deposit", Value = "Deposit" });
            nationality.Add(new SelectListItem { Text = "Issued", Value = "Issued" });
            nationality.Add(new SelectListItem { Text = "Cancel", Value = "Cancel" });
            nationality.Add(new SelectListItem { Text = "Return", Value = "Return" });
            nationality.Add(new SelectListItem { Text = "Clear", Value = "Clear" });
            nationality.Add(new SelectListItem { Text = "Forward", Value = "Forward" });
            return nationality;
        }
        #endregion
        public ActionResult UnpostCheque(int id)
        {
            string message = "";
            bool status = false;
            System.Data.Entity.Core.Objects.ObjectParameter CheckReturn = new System.Data.Entity.Core.Objects.ObjectParameter("CheckReturn", typeof(int));
            db.spInsertUpdateChequeClearance(id, "Unpost", "", null, User.UserId, User.UserId.ToString(), DateTime.Now, "", 0, 0, "Update", CheckReturn);
            if (Convert.ToDecimal(CheckReturn.Value) == 1)
            {
                ModelState.Clear();
                status = true;
                message = "Cheque Successfully Unposted";
            }
            else if (Convert.ToInt32(CheckReturn.Value) == -9999)
            {
                ModelState.Clear();
                status = false;
                message = "Error! Please delete all data against this cheque first.";
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