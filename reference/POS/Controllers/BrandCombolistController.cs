using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POS.Controllers
{
    public class BrandCombolistController : Controller
    {
        PosMBDb db = new PosMBDb();

        IQueryable<clsBrandList> AllItemsList;


        public JsonResult GetOptionList(string searchTerm, int pageSize, int pageNumber)
        {
            AllItemsList = AllItemsListDetail();
            var select2pagedResult = new Select2PagedBrandResult();
            var totalResults = 0;
            select2pagedResult.Results = GetPagedListOptions(searchTerm, pageSize, pageNumber, out totalResults);
            select2pagedResult.Total = totalResults;

            var result = select2pagedResult;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public IQueryable<clsBrandList> AllItemsListDetail()
        {
            //string cacheKey = "Select2Options";
            ////check cache 
            //if (System.Web.HttpContext.Current.Cache[cacheKey] != null)
            //{
            //    return (IQueryable<clsItemList>)System.Web.HttpContext.Current.Cache[cacheKey];
            //}

            List<clsBrandList> item = new List<clsBrandList>();
            item = (from c in db.Item_Brand
                    where c.Status == "Active"
                    select new clsBrandList
                    {
                        id = c.BrandID,
                        text = c.Description
                    }).ToList();

            var result = item.AsQueryable();

            //cache results
            //System.Web.HttpContext.Current.Cache[cacheKey] = result;

            return result;
        }


        List<clsBrandList> GetPagedListOptions(string searchTerm, int pageSize, int pageNumber, out int totalSearchRecords)
        {
            var allSearchedResults = GetAllSearchResults(searchTerm);
            totalSearchRecords = allSearchedResults.Count;
            return allSearchedResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        List<clsBrandList> GetAllSearchResults(string searchTerm)
        {
            //AllItemsList = AllItemsListDetail();
            var resultList = new List<clsBrandList>();

            if (!string.IsNullOrEmpty(searchTerm))
                resultList = AllItemsList.Where(n => n.text.ToLower().Contains(searchTerm.ToLower())).ToList();
            else
                resultList = AllItemsList.ToList();
            return resultList;
        }
	}
}