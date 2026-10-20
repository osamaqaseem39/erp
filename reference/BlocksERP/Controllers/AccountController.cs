using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlocksERP.Models.DAL.Security;
using BlocksERP.Models;
using System.Web.Security;
using Newtonsoft.Json;
using System.Web.Configuration;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Net.NetworkInformation;
using BlocksERP.Controllers;

namespace BlocksERP.Controllers
{
    public class AccountController : BaseController
    {        ModelDb db = new ModelDb();
        // GET: Account
        [AllowAnonymous]
        public ActionResult Login()
        {

            return View();
        }


        [AllowAnonymous]
        public ActionResult LoginPartial()
        {
            return PartialView();
        }

        [HttpPost]


        public ActionResult Login(clsUser login)
        {
            string hashPassword = clsPasswordEncrypt.GetHash(login.Password);
            bool status = false;
            int UserID = 0;
            var vuser = (from u in db.tblUsers
                        where u.Email.ToLower() == login.Username.ToLower().Trim()  && u.Password == hashPassword
                        && u.Status.ToLower() == "active" && u.type.ToLower() == "admin"
                         select u).FirstOrDefault();

            if (vuser != null)
            {
                status = true;
                string[] roles = new string[1];
                roles[0] = "User";
                CustomPrincipalSerializeModel serializeModel = new CustomPrincipalSerializeModel();
                serializeModel.UserID = vuser.UserId;
                serializeModel.Name = vuser.Email;
                serializeModel.IsSalesman = 0;
                serializeModel.roles = roles;
                serializeModel.FiscalYearID = 1;
                serializeModel.DefaultWarehouseID=0;
                serializeModel.DefaultWarehouseName="";

                //serializeModel.DistributorID = 6059;
                serializeModel.DistributorID = 0;
                serializeModel.DistributorName = db.tblAccountDetails.Where(x=>x.DetailCode == serializeModel.DistributorID).Select(x=>x.Name).FirstOrDefault() ??"";
                
                int tempempid = 0;
                int tempsalesmanid=0;

                serializeModel.SalesmanID = tempsalesmanid;


                string userData = JsonConvert.SerializeObject(serializeModel);
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                         1,
                         vuser.Email,
                         DateTime.Now,
                         DateTime.Now.AddMinutes(100),
                         false,
                         userData);

                string encTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                Response.Cookies.Add(faCookie);
                status = true;
                UserID = vuser.UserId;

                string ImagePath = db.tblDocumentOthers.Where(x => x.TypeId == UserID.ToString() && x.DocumentType == "User").Select(x => "/FormsDocuments/User/" + x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "";
                System.Web.HttpContext.Current.Application["UserImage"] = ImagePath;
            }
            else
            {
                status = false;
            }
            return new JsonResult { Data = new { status = status, UserID = UserID } };
           
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
            return RedirectToAction("login", "account", new { area = "" });
        }

        public ActionResult GetUserPicture(int id)
        {
            string imagePath = ""; // db.tblDocuments.Where(x => x.TypeID == User.UserID.ToString() && x.DocumentType == "User").Select(x => "/FormsDocuments/User/" + x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "/Content/dummy2.png";
            string completePath = Server.MapPath(imagePath);
            byte[] image = null;
            try
            {
                image = System.IO.File.ReadAllBytes(completePath);
            }
            catch (Exception ex)
            {

            }
            if (image != null)
            {
                return File(image, "image/jpg");
            }
            else
            {
                return null;
            }
        }

        public ActionResult GetCompanyName()
        {
            string companyName = db.tblCompanies.Select(x => x.CompanyName).FirstOrDefault();
            return Content(companyName);
        }
        public ActionResult GetCompanyLogo()
        {
            string imagePath = db.tblDocumentOthers.Where(x => x.DocumentType == "Company").Select(x => "/FormsDocuments/Company/" + x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "/Content/dummy2.png";
            string completePath = Server.MapPath(imagePath);
            try
            {
                byte[] image = System.IO.File.ReadAllBytes(completePath);
                if (image != null)
                {
                    return File(image, "image/jpg");
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
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
                bool check = db.tblUsers.Where(x => x.UserId == User.UserID).Any(x => x.Password == hashPassword);
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
                var getUser = db.tblUsers.Where(x => x.UserId == User.UserID).FirstOrDefault();
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