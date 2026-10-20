using BlocksERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlocksERP.Controllers
{
    public class CombolistController : BaseController
    {
        ModelDb db = new ModelDb();

        IQueryable<clsList> AllItemsList;
        IQueryable<clsStringList> AllItemsStringList;
        
        List<clsList> GetPagedListOptions(string searchTerm, int pageSize, int pageNumber, out int totalSearchRecords)
        {
            var allSearchedResults = GetAllSearchResults(searchTerm);
            totalSearchRecords = allSearchedResults.Count;
            return allSearchedResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }
        List<clsList> GetAllSearchResults(string searchTerm)
        {
            var resultList = new List<clsList>();

            if (!string.IsNullOrEmpty(searchTerm))
                resultList = AllItemsList.Where(n => n.text.ToLower().Contains(searchTerm.ToLower())).ToList();
            else
                resultList = AllItemsList.ToList();
            return resultList;
        }



        #region country
        public JsonResult GetCountryList(string searchTerm, int pageSize, int pageNumber)
        {
            AllItemsList = AllCountryListListDetail();
            var select2pagedResult = new Select2PagedResult();
            var totalResults = 0;
            select2pagedResult.Results = GetPagedListOptions(searchTerm, pageSize, pageNumber, out totalResults);
            select2pagedResult.Total = totalResults;

            var result = select2pagedResult;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public IQueryable<clsList> AllCountryListListDetail()
        {


            List<clsList> item = new List<clsList>();
            item = (from c in db.tblCountries
                    where c.Status == 1
                    orderby c.CountryId
                    select new clsList
                    {
                        id = c.CountryId,
                        text = c.CountryName
                    }).ToList();

            var result = item.AsQueryable();



            return result;
        }

        #endregion country
        #region City
        public JsonResult GetCityList(string searchTerm, int pageSize, int pageNumber)
        {
            AllItemsList = AllCityListDetail();
            var select2pagedResult = new Select2PagedResult();
            var totalResults = 0;
            select2pagedResult.Results = GetPagedListOptions(searchTerm, pageSize, pageNumber, out totalResults);
            select2pagedResult.Total = totalResults;

            var result = select2pagedResult;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public IQueryable<clsList> AllCityListDetail()
        {


            List<clsList> item = new List<clsList>();
            item = (from c in db.tblCities
                    where c.Status == 1

                    orderby c.CityId
                    select new clsList
                    {
                        id = c.CityId,
                        text = c.DisplayName ?? c.CityName
                    }).ToList();

            var result = item.AsQueryable();



            return result;
        }

        #endregion
        #region Bank
        public JsonResult GetBankList(string searchTerm, int pageSize, int pageNumber)
        {
            AllItemsList = AllBankListDetail();
            var select2pagedResult = new Select2PagedResult();
            var totalResults = 0;
            select2pagedResult.Results = GetPagedListOptions(searchTerm, pageSize, pageNumber, out totalResults);
            select2pagedResult.Total = totalResults;

            var result = select2pagedResult;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public IQueryable<clsList> AllBankListDetail()
        {


            List<clsList> item = new List<clsList>();
            item = (from c in db.tblBanks 
                    where c.Status.ToLower() == "active"
                    orderby c.BankName
                    select new clsList
                    {
                        id = c.BankId,
                        text = c.BankName
                    }).ToList();

            var result = item.AsQueryable();



            return result;
        }

        #endregion country



        List<clsStringList> GetPagedListOptionsString(string searchTerm, int pageSize, int pageNumber, out int totalSearchRecords)
        {
            var allSearchedResults = GetAllSearchResultsString(searchTerm);
            totalSearchRecords = allSearchedResults.Count;
            return allSearchedResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }
        List<clsStringList> GetAllSearchResultsString(string searchTerm)
        {
            var resultList = new List<clsStringList>();

            if (!string.IsNullOrEmpty(searchTerm))
                resultList = AllItemsStringList.Where(n => n.text.ToLower().Contains(searchTerm.ToLower())).ToList();
            else
                resultList = AllItemsStringList.ToList();
            return resultList;
        }
        
    }
}