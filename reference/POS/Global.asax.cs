using POS.Models.DAL.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace POS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            try
            {
                AreaRegistration.RegisterAllAreas();
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);
                string businessName = System.Configuration.ConfigurationManager.ConnectionStrings["BusinessName"].ConnectionString;
                string businessAddress = System.Configuration.ConfigurationManager.ConnectionStrings["BusinessAddress"].ConnectionString;
                Application["BusinessName"] = businessName;
                Application["BusinessAddress"] = businessAddress;
                Application["Concat"] = " _ ";
            }
            catch (Exception)
            {

            }
            
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
                    newUser.UserId = serializeModel.UserId;
                    newUser.Name = serializeModel.Name;
                    newUser.roles = serializeModel.roles;
                    newUser.Main = serializeModel.Main;
                    newUser.Save = serializeModel.Save;
                    newUser.Change = serializeModel.Change;
                    newUser.Delete = serializeModel.Delete;
                    newUser.FiscalYearId = serializeModel.FiscalYearId;
                    newUser.WarehouseId = serializeModel.WarehouseId;
                    HttpContext.Current.User = newUser;
                }
                catch (Exception)
                {

                }
            }

        }
    }
}
