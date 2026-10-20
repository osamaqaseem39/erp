using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POS.Controllers
{
    public class CodeCombolistController : Controller
    {
        PosMBDb db = new PosMBDb();

        IQueryable<clsCodeList> AllItemsList;


        public JsonResult GetOptionList(string searchTerm, int pageSize, int pageNumber)
        {
            AllItemsList = AllItemsListDetail();
            var select2pagedResult = new Select2PagedCodeResult();
            var totalResults = 0;
            select2pagedResult.Results = GetPagedListOptions(searchTerm, pageSize, pageNumber, out totalResults);
            select2pagedResult.Total = totalResults;

            var result = select2pagedResult;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public IQueryable<clsCodeList> AllItemsListDetail()
        {
            //string cacheKey = "Select2Options";
            ////check cache 
            //if (System.Web.HttpContext.Current.Cache[cacheKey] != null)
            //{
            //    return (IQueryable<clsItemList>)System.Web.HttpContext.Current.Cache[cacheKey];
            //}

            List<clsCodeList> item = new List<clsCodeList>();
            item = (from c in db.Item_Information
                    where c.Status == "Active"
                    select new clsCodeList
                    {
                        id = c.UrduName ?? "",
                        text = c.UrduName ?? ""
                    }).Distinct().ToList();

            var result = item.AsQueryable();

            //cache results
            //System.Web.HttpContext.Current.Cache[cacheKey] = result;

            return result;
        }


        List<clsCodeList> GetPagedListOptions(string searchTerm, int pageSize, int pageNumber, out int totalSearchRecords)
        {
            var allSearchedResults = GetAllSearchResults(searchTerm);
            totalSearchRecords = allSearchedResults.Count;
            return allSearchedResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        List<clsCodeList> GetAllSearchResults(string searchTerm)
        {
            //AllItemsList = AllItemsListDetail();
            var resultList = new List<clsCodeList>();

            if (!string.IsNullOrEmpty(searchTerm))
                resultList = AllItemsList.Where(n => n.text.ToLower().Contains(searchTerm.ToLower())).ToList();
            else
                resultList = AllItemsList.ToList();
            return resultList;
        }
	}
}