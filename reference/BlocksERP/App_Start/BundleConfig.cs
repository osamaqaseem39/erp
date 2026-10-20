#region Using

using System.Web.Optimization;

#endregion

namespace BlocksERP
{
    public static class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            //bundles.Add(new StyleBundle("~/content/smartadmin").IncludeDirectory("~/content/css", "*.min.css"));
            //bundles.Add(new StyleBundle("~/content/smartadmin").IncludeDirectory("~/content/css", "*.css"));
            bundles.Add(new StyleBundle("~/content/smartadmin").Include(
                "~/Content/vendors.bundle.css",
                "~/Content/app.bundle.css",
                "~/Content/miscellaneous/reactions/reactions.css",
                "~/Content/miscellaneous/fullcalendar/fullcalendar.bundle.css",
                "~/Content/miscellaneous/jqvmap/jqvmap.bundle.css",
                "~/Content/datagrid/datatables/datatables.bundle.css",
                "~/Content/notifications/toastr/toastr.css",
                "~/Content/theme-demo.css",
                "~/Content/formplugins/select2/select2.bundle.css",
                "~/Content/confirm.css",
                "~/Content/fa-solid.css"
               ));
            bundles.Add(new ScriptBundle("~/script/smartadmin").Include(
                "~/Scripts/vendors.bundle.js",
                "~/Scripts/app.bundle.js",
                "~/Scripts/dependency/moment/moment.js",
                "~/Scripts/miscellaneous/fullcalendar/fullcalendar.bundle.js",
                "~/Scripts/statistics/sparkline/sparkline.bundle.js",
                "~/Scripts/statistics/easypiechart/easypiechart.bundle.js",
                "~/Scripts/statistics/flot/flot.bundle.js",
                "~/Scripts/miscellaneous/jqvmap/jqvmap.bundle.js",
                "~/Scripts/datagrid/datatables/datatables.bundle.js",
                //"~/Scripts/jquery.validate.min.js"
                //"~/Scripts/jquery.validate.unobtrusive.js",
                "~/Scripts/notifications/toastr/toastr.js",
                "~/Scripts/formplugins/select2/select2.bundle.js",
                "~/Scripts/confirm.js"
               ));
            bundles.Add(new ScriptBundle("~/script/jqueryvalidator").Include(
               "~/Scripts/jquery.validate.min.js",
               "~/Scripts/jquery.validate.unobtrusive.js"
              ));
            BundleTable.EnableOptimizations = true;
        }
    }
}