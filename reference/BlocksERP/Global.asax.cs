using BlocksERP.Models.DAL.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace BlocksERP
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ModelDb db = new ModelDb();
            string companyName = db.tblCompanies.Select(x => x.CompanyName).FirstOrDefault();
            Application["BusinessName"] = companyName;
            Application["CompanyLogo"] = db.tblDocumentOthers.Where(x => x.DocumentType == "Company").Select(x => "/FormsDocuments/Company/" + x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "/Content/dummy2.png";
            Application["UserImage"] = "/content/images/person.png";

            //Application["BusinessAddress"] = businessAddress;

            //string businessName = System.Configuration.ConfigurationManager.ConnectionStrings["BusinessName"].ConnectionString;
            //string businessAddress = System.Configuration.ConfigurationManager.ConnectionStrings["BusinessAddress"].ConnectionString;
            //Application["BusinessName"] = businessName;
            //Application["BusinessAddress"] = businessAddress;
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            //HttpCookie authCookie = FormsAuthentication.GetAuthCookie("UserId", true);
            if (authCookie != null)
            {
                try
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    CustomPrincipalSerializeModel serializeModel = JsonConvert.DeserializeObject<CustomPrincipalSerializeModel>(authTicket.UserData);
                    CustomPrincipal newUser = new CustomPrincipal(authTicket.Name);
                    newUser.UserID = serializeModel.UserID;
                    newUser.Name = serializeModel.Name;
                    newUser.FiscalYearID = serializeModel.FiscalYearID;
                    newUser.IsSalesman = serializeModel.IsSalesman;
                    newUser.SalesmanID = serializeModel.SalesmanID;

                    newUser.DistributorID = serializeModel.DistributorID;
                    newUser.DistributorName = serializeModel.DistributorName;


                    HttpContext.Current.User = newUser;

                    ModelDb db = new ModelDb();
                    string ImagePath = db.tblDocumentOthers.Where(x => x.TypeId == serializeModel.UserID.ToString() && x.DocumentType == "User").Select(x => "/FormsDocuments/User/" + x.DocumentId + "." + x.DocumentExtension).FirstOrDefault() ?? "";
                    System.Web.HttpContext.Current.Application["UserImage"] = ImagePath;
                }
                catch (Exception)
                {

                }
            }

        }
    }
}
