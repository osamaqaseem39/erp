using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using POS.Models.DAL.Security;
using POS.Models;
using System.Web.Security;
using Newtonsoft.Json;
using System.Web.Configuration;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace POS.Controllers
{
    public class AccountController : BaseController
    {
        PosMBDb db = new PosMBDb();
        // GET: Account
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(clsUser login)
        {
            string hashPassword = clsPasswordEncrypt.GetHash(login.Password);
            bool status = false;
            int userId = 0;
            var user = (from u in db.Admin_Setup_User
                        where u.Username.ToLower() == login.Username.ToLower().Trim() && u.Password == hashPassword && u.Status == "Active"
                        select u).FirstOrDefault();
            if (user != null)
            {
                status = true;
                userId = user.UserID;
                SetFiscalYear(4, userId);
               
            }
            else
            {
                status = false;
            }
            return new JsonResult { Data = new { status = status, userId = userId } };
        }
        [AllowAnonymous]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            // clear authentication cookie
            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            authCookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Set(authCookie);

            // clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
            SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
            HttpCookie cookie2 = new HttpCookie(sessionStateSection.CookieName, "");
            cookie2.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Set(cookie2);

            return RedirectToAction("Index", "account");
        }
        public ActionResult FiscalYear(int id)
        {
            ViewBag.UserId = id;
            ViewBag.FiscalYear = new SelectList(getFiscalYears(), "FiscalYearID", "Description");
            return PartialView();
        }
        public List<clsFiscalYear> getFiscalYears()
        {
            List<clsFiscalYear> firstLevel = new List<clsFiscalYear>();
            firstLevel = (from c in db.Admin_Setup_FiscalYear
                          where c.Status == "Active"
                          select new clsFiscalYear
                          {
                              FiscalYearID = c.FiscalYearID,
                              Description = c.Description
                          }).ToList();
            return firstLevel;
        }
        public ActionResult SetFiscalYear(int fiscalYearId, int userId)
        {
            string[] roles = new string[1];
            int? warehouseId = db.Admin_Setup_Business.Where(x => x.BusinessID == 0).Select(x => x.DefaultWarehouseID).FirstOrDefault();
            string username = db.Admin_Setup_User.Where(x => x.UserID == userId).Select(x => x.Username).FirstOrDefault();
            roles[0] = "User";
            CustomPrincipalSerializeModel serializeModel = new CustomPrincipalSerializeModel();
            serializeModel.UserId = userId;
            serializeModel.Name = username;
            serializeModel.roles = roles;
            serializeModel.FiscalYearId = fiscalYearId;
            serializeModel.WarehouseId = warehouseId;
            string userData = JsonConvert.SerializeObject(serializeModel);
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                     1,
                     username,
                     DateTime.Now,
                     DateTime.Now.AddMinutes(30),
                     false,
                     userData);

            string encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            Response.Cookies.Add(faCookie);
            //Session["FiscalYearId"] = id;
            string nothing = "";
            return Json(nothing, JsonRequestBehavior.AllowGet);
        }
        #region Change Password
        public ActionResult CheckOldPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CheckOldPassword(string password)
        {
            bool result = false;
            if (password != null)
            {
                string hashPassword = clsPasswordEncrypt.GetHash(password);
                bool check = db.Admin_Setup_User.Where(x => x.UserID == User.UserId).Any(x => x.Password == hashPassword);
                if (check)
                {
                    result = true;
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                result = false;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(string password)
        {
            bool result = false;
            if (password != null)
            {
                string hashPassword = clsPasswordEncrypt.GetHash(password);
                var getUser = db.Admin_Setup_User.Where(x => x.UserID == User.UserId).FirstOrDefault();
                getUser.Password = hashPassword;
                db.SaveChanges();
                result = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                result = false;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}