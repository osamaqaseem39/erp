using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POS.Controllers
{
    public class AccountComboListController : BaseController
    {
        PosMBDb db = new PosMBDb();

        IQueryable<clsAccountList> AllItemsList;


        public JsonResult GetOptionList(string searchTerm, int pageSize, int pageNumber)
        {
            AllItemsList = AllItemsListDetail();
            var select2pagedResult = new Select2PagedAccountResult();
            var totalResults = 0;
            select2pagedResult.Results = GetPagedListOptions(searchTerm, pageSize, pageNumber, out totalResults);
            select2pagedResult.Total = totalResults;

            var result = select2pagedResult;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public IQueryable<clsAccountList> AllItemsListDetail()
        {
            //string cacheKey = "Select2Options";
            ////check cache 
            //if (System.Web.HttpContext.Current.Cache[cacheKey] != null)
            //{
            //    return (IQueryable<clsItemList>)System.Web.HttpContext.Current.Cache[cacheKey];
            //}

            List<clsAccountList> item = new List<clsAccountList>();
            item = (from c in db.AC_Detail
                    join ef in db.AC_Detail_Extra on c.DetailCode equals ef.DetailCode
                    where c.Status == "Active" && (c.Type == "Customer" || c.Type == "Supplier" || c.Type == "Customer + Supplier")
                    && ef.Cell1 != null && ef.Cell1 != ""
                    select new clsAccountList
                    {
                        id = c.DetailCode,
                        text = c.Name + ", " + (from e in db.AC_Detail_Extra join ct in db.Cities on e.CityId equals ct.CityId where e.DetailCode == c.DetailCode select ct.CityName).FirstOrDefault() ?? "",
                    }).ToList();

            var result = item.AsQueryable();

            //cache results
            //System.Web.HttpContext.Current.Cache[cacheKey] = result;

            return result;
        }


        List<clsAccountList> GetPagedListOptions(string searchTerm, int pageSize, int pageNumber, out int totalSearchRecords)
        {
            var allSearchedResults = GetAllSearchResults(searchTerm);
            totalSearchRecords = allSearchedResults.Count;
            return allSearchedResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        List<clsAccountList> GetAllSearchResults(string searchTerm)
        {
            //AllItemsList = AllItemsListDetail();
            var resultList = new List<clsAccountList>();

            if (!string.IsNullOrEmpty(searchTerm))
                resultList = AllItemsList.Where(n => n.text.ToLower().StartsWith(searchTerm.ToLower())).ToList();
            else
                resultList = AllItemsList.ToList();
            return resultList;
        }


    }
}
