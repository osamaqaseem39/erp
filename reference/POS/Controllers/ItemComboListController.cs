using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POS.Controllers
{
    public class ItemComboListController : BaseController
    {
        PosMBDb db = new PosMBDb();
        string concatString = System.Web.HttpContext.Current.Application["Concat"].ToString();

        IQueryable<clsItemList> AllItemsList;


        public JsonResult GetOptionList(string searchTerm, int pageSize, int pageNumber)
        {
            AllItemsList = AllItemsListDetail();
            var select2pagedResult = new Select2PagedItemResult();
            var totalResults = 0;
            select2pagedResult.Results = GetPagedListOptions(searchTerm, pageSize, pageNumber, out totalResults);
            select2pagedResult.Total = totalResults;

            var result = select2pagedResult;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public IQueryable<clsItemList> AllItemsListDetail()
        {
            //string cacheKey = "Select2Options";
            ////check cache 
            //if (System.Web.HttpContext.Current.Cache[cacheKey] != null)
            //{
            //    return (IQueryable<clsItemList>)System.Web.HttpContext.Current.Cache[cacheKey];
            //}

            List<clsItemList> item = new List<clsItemList>();
            item = (from c in db.Item_Information
                    where c.Status == "Active"
                    select new clsItemList
                    {
                        id = c.ItemID,
                        text = c.UrduName != null ?  c.UrduName : c.ItemName,
                        description = c.ItemName ?? ""
                    }).ToList();


            //text = c.UrduName != null ? "<div class='row'>" +
            //                "<div class='col-md-3'>" + c.UrduName + "</div>" +
            //                "<div class='col-md-9'>" +c.ItemName + "</div>" +
            //            "</div>" : c.ItemName

            //text = c.UrduName != null ? c.UrduName + concatString + c.ItemName : c.ItemName,

            var result = item.AsQueryable();

            //cache results
            //System.Web.HttpContext.Current.Cache[cacheKey] = result;

            return result;
        }


        List<clsItemList> GetPagedListOptions(string searchTerm, int pageSize, int pageNumber, out int totalSearchRecords)
        {
            var allSearchedResults = GetAllSearchResults(searchTerm);
            totalSearchRecords = allSearchedResults.Count;
            return allSearchedResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        List<clsItemList> GetAllSearchResults(string searchTerm)
        {
            //AllItemsList = AllItemsListDetail();
            var resultList = new List<clsItemList>();

            if (!string.IsNullOrEmpty(searchTerm))
                resultList = AllItemsList.Where(n => n.text.ToLower().StartsWith(searchTerm.ToLower())).ToList();
            else
                resultList = AllItemsList.ToList();
            return resultList;
        }
        

    }
}
