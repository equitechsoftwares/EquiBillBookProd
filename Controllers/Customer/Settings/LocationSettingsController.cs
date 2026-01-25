using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class LocationSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: LocationSettings

        #region Country

        public async Task<ActionResult> Country(ClsCountryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                obj.Title = "Country";
            }
            WebApi.CountryController countryController = new WebApi.CountryController();
            var allCountrysResult = await countryController.AllCountrys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allCountrysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "country").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> CountryFetch(ClsCountryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.Title = "Country";
            }
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Country/AllCountrys", arr[0], arr[1], arr[2]);

            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);
            var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);


            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "country").FirstOrDefault();

            return PartialView("PartialCountry");
        }
        public async Task<ActionResult> CountryEdit(long CountryId)
        {
            ClsCountryVm obj = new ClsCountryVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.CountryId = CountryId;
            }
            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountry countryObj = new ClsCountry { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var countryResult = await countryController.Country(countryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(countryResult);

            ViewBag.Country = oClsResponse.Data.Country;
            return View();
        }
        public ActionResult CountryAdd()
        {
            return View();
        }
        public async Task<ActionResult> CountryInsert(ClsCountryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CountryController countryController = new WebApi.CountryController();
            var insertCountryResult = await countryController.InsertCountry(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertCountryResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CountryUpdate(ClsCountryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CountryController countryController = new WebApi.CountryController();
            var updateCountryResult = await countryController.UpdateCountry(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateCountryResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CountryActiveInactive(ClsCountryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CountryController countryController = new WebApi.CountryController();
            var countryActiveInactiveResult = await countryController.CountryActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(countryActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CountryDelete(ClsCountryVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CountryController countryController = new WebApi.CountryController();
            var countryDeleteResult = await countryController.CountryDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(countryDeleteResult);
            return Json(oClsResponse);
        }

        #endregion

        #region State

        public async Task<ActionResult> State(ClsStateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "State";
            }
            WebApi.StateController stateController = new WebApi.StateController();
            var allStatesResult = await stateController.AllStates(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allStatesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.States = oClsResponse.Data.States;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> StateFetch(ClsStateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "State";
            }
            WebApi.StateController stateController = new WebApi.StateController();
            var allStatesResult = await stateController.AllStates(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allStatesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.States = oClsResponse.Data.States;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();

            return PartialView("Partialstate");
        }
        public async Task<ActionResult> StateEdit(long StateId)
        {
            ClsStateVm obj = new ClsStateVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.StateId = StateId;
            }
            WebApi.StateController stateController = new WebApi.StateController();
            ClsState stateObj = new ClsState { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, StateId = obj.StateId };
            var stateResult = await stateController.State(stateObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stateResult);

            ViewBag.State = oClsResponse.Data.State;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            return View();
        }
        public async Task<ActionResult> StateAdd()
        {
            ClsStateVm obj = new ClsStateVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCountrysResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeCountrysResult);

            ViewBag.Countrys = oClsResponse.Data.Countrys;
            return View();

        }
        public async Task<ActionResult> StateInsert(ClsStateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StateController stateController = new WebApi.StateController();
            var insertStateResult = await stateController.InsertState(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertStateResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StateUpdate(ClsStateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StateController stateController = new WebApi.StateController();
            var updateStateResult = await stateController.UpdateState(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateStateResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StateActiveInactive(ClsStateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StateController stateController = new WebApi.StateController();
            var stateActiveInactiveResult = await stateController.StateActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stateActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> StateDelete(ClsStateVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.StateController stateController = new WebApi.StateController();
            var stateDeleteResult = await stateController.StateDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(stateDeleteResult);
            return Json(oClsResponse);
        }

        #endregion

        #region City

        public async Task<ActionResult> City(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "City";
            }
            WebApi.CityController cityController = new WebApi.CityController();
            var allCitysResult = await cityController.AllCitys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allCitysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> CityFetch(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "City";
            }
            WebApi.CityController cityController = new WebApi.CityController();
            var allCitysResult = await cityController.AllCitys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allCitysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

            return PartialView("PartialCity");
        }
        public async Task<ActionResult> CityEdit(long CityId)
        {
            ClsCityVm obj = new ClsCityVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.CityId = CityId;
            }
            WebApi.CityController cityController = new WebApi.CityController();
            ClsCity cityObj = new ClsCity { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CityId = obj.CityId };
            var cityResult = await cityController.City(cityObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(cityResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.City = oClsResponse.Data.City;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> CityAdd()
        {
            ClsStateVm obj = new ClsStateVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);

            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Country/ActiveCountrys", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            var res35 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "Menu/ControlsPermission", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse35 = serializer.Deserialize<ClsResponse>(res35);

            ViewBag.Countrys = oClsResponse.Data.Countrys;

            //var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "State/ActiveStates", arr[0], arr[1], arr[2]);

            //ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            //ViewBag.States = oClsResponse.Data.States;

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> CityInsert(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CityController cityController = new WebApi.CityController();
            var insertCityResult = await cityController.InsertCity(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertCityResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CityUpdate(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CityController cityController = new WebApi.CityController();
            var updateCityResult = await cityController.UpdateCity(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateCityResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CityActiveInactive(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CityController cityController = new WebApi.CityController();
            var cityActiveInactiveResult = await cityController.CityActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(cityActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> CityDelete(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CityController cityController = new WebApi.CityController();
            var cityDeleteResult = await cityController.CityDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(cityDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveStates(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var activeStatesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeStatesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.States = oClsResponse.Data.States;
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();

            return PartialView("PartialStatesDropdown");
        }

        public async Task<ActionResult> ActiveStatesForCityModal(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "State/ActiveStates", arr[0], arr[1], arr[2]);

            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            return Json(oClsResponse);
        }

        #endregion

        #region Branch

        public async Task<ActionResult> Branch(ClsBranchVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.PageIndex = 1;
                //obj.PageSize = 10;
                //obj.Title = "Branch";
            }
            WebApi.BranchController branchController = new WebApi.BranchController();
            var allBranchsResult = await branchController.AllBranchs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allBranchsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "branch").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> BranchFetch(ClsBranchVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Branch";
            }
            WebApi.BranchController branchController = new WebApi.BranchController();
            var allBranchsResult = await branchController.AllBranchs(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allBranchsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "branch").FirstOrDefault();

            return PartialView("PartialBranch");
        }
        public async Task<ActionResult> BranchEdit(long BranchId)
        {
            ClsBranchVm obj = new ClsBranchVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = BranchId;
            }
            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var branchResult = await branchController.Branch(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            obj.BranchId = 0;

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAllTaxsResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeAllTaxsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(activeAccountsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            ClsPrefixMasterVm prefixMasterObj = new ClsPrefixMasterVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allPrefixMastersResult = await prefixController.AllPrefixMasters(prefixMasterObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(allPrefixMastersResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingResult);

            WebApi.PurchaseSettingsController purchaseSettingsController = new WebApi.PurchaseSettingsController();
            ClsPurchaseSettingsVm purchaseSettingsObj = new ClsPurchaseSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var purchaseSettingResult = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(purchaseSettingResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            var activeTaxsResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeStatesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(activeStatesResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBusinessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(activeBusinessRegistrationNamesResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeTaxSettingsResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(activeTaxSettingsResult);

            ClsPrefixVm prefixObj = new ClsPrefixVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activePrefixsResult = await prefixController.ActivePrefixs(prefixObj);
            ClsResponse oClsResponse57 = await oCommonController.ExtractResponseFromActionResult(activePrefixsResult);

            ViewBag.Branch = oClsResponse.Data.Branch;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.Currencys = oClsResponse.Data.Currencys;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.Accounts = oClsResponse5.Data.Accounts;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PrefixMasters = oClsResponse11.Data.PrefixMasters;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.States = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;
            ViewBag.Prefixs = oClsResponse57.Data.Prefixs;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.PosPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "pos").FirstOrDefault();
            //ViewBag.IsAccounts = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower()== "accounts").FirstOrDefault() ;
            //ViewBag.PlanAddons = oClsResponse21.Data.PlanAddons;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.TaxSettingPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax settings").FirstOrDefault();
            ViewBag.PrefixPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "prefix").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return View();
        }
        public async Task<ActionResult> BranchAdd()
        {
            ClsBranchVm obj = new ClsBranchVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.CountryController countryController = new WebApi.CountryController();
            ClsCountryVm countryObj = new ClsCountryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeCountrysResult = await countryController.ActiveCountrys(countryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeCountrysResult);

            WebApi.PaymentTypeController paymentTypeController = new WebApi.PaymentTypeController();
            ClsPaymentTypeVm paymentTypeObj = new ClsPaymentTypeVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PaymentType = "all" };
            var activePaymentTypesResult = await paymentTypeController.ActivePaymentTypes(paymentTypeObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(activePaymentTypesResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(activeAccountsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            ClsPrefixMasterVm prefixMasterObj = new ClsPrefixMasterVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allPrefixMastersResult = await prefixController.AllPrefixMasters(prefixMasterObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(allPrefixMastersResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingResult);

            WebApi.PurchaseSettingsController purchaseSettingsController = new WebApi.PurchaseSettingsController();
            ClsPurchaseSettingsVm purchaseSettingsObj = new ClsPurchaseSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var purchaseSettingResult = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(purchaseSettingResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeStatesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(activeStatesResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBusinessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(activeBusinessRegistrationNamesResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeTaxSettingsResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(activeTaxSettingsResult);

            ClsPrefixVm prefixObj = new ClsPrefixVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activePrefixsResult = await prefixController.ActivePrefixs(prefixObj);
            ClsResponse oClsResponse57 = await oCommonController.ExtractResponseFromActionResult(activePrefixsResult);

            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.PaymentTypes = oClsResponse2.Data.PaymentTypes;
            //ViewBag.Users = oClsResponse3.Data.Users;
            //ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.Accounts = oClsResponse5.Data.Accounts;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PrefixMasters = oClsResponse11.Data.PrefixMasters;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            //ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            //ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.States = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;
            ViewBag.Prefixs = oClsResponse57.Data.Prefixs;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.PosPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "pos").FirstOrDefault();
            //ViewBag.IsAccounts = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault() ;

            //ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            //ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.TaxSettingPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax settings").FirstOrDefault();
            ViewBag.PrefixPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "prefix").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            //ViewBag.PlanAddons = oClsResponse21.Data.PlanAddons;
            return View();
        }
        public async Task<ActionResult> BranchInsert(ClsBranchVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.BranchController branchController = new WebApi.BranchController();
            var insertBranchResult = await branchController.InsertBranch(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertBranchResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> BranchUpdate(ClsBranchVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.BranchController branchController = new WebApi.BranchController();
            var updateBranchResult = await branchController.UpdateBranch(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateBranchResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> BranchActiveInactive(ClsBranchVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web"; obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.BranchController branchController = new WebApi.BranchController();
            var branchActiveInactiveResult = await branchController.BranchActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> BranchDelete(ClsBranchVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.BranchController branchController = new WebApi.BranchController();
            var branchDeleteResult = await branchController.BranchDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveCitys(ClsCityVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.CityController cityController = new WebApi.CityController();
            ClsCityVm cityObj = new ClsCityVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, StateId = obj.StateId };
            var activeCitysResult = await cityController.ActiveCitys(cityObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeCitysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

            return PartialView("PartialCitysDropdown");
        }
        public async Task<ActionResult> BranchView(long BranchId)
        {
            ClsBranchVm obj = new ClsBranchVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = BranchId;
            }
            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, BranchId = obj.BranchId };
            var branchResult = await branchController.Branch(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            obj.BranchId = 0;

            WebApi.TaxController taxController = new WebApi.TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAllTaxsResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(activeAllTaxsResult);

            WebApi.AccountController accountController = new WebApi.AccountController();
            ClsAccount accountObj = new ClsAccount { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeAccountsResult = await accountController.ActiveAccounts(accountObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(activeAccountsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.PrefixController prefixController = new WebApi.PrefixController();
            ClsPrefixMasterVm prefixMasterObj = new ClsPrefixMasterVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allPrefixMastersResult = await prefixController.AllPrefixMasters(prefixMasterObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(allPrefixMastersResult);

            WebApi.SaleSettingsController saleSettingsController = new WebApi.SaleSettingsController();
            ClsSaleSettingsVm saleSettingsObj = new ClsSaleSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saleSettingResult = await saleSettingsController.SaleSetting(saleSettingsObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(saleSettingResult);

            WebApi.PurchaseSettingsController purchaseSettingsController = new WebApi.PurchaseSettingsController();
            ClsPurchaseSettingsVm purchaseSettingsObj = new ClsPurchaseSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var purchaseSettingResult = await purchaseSettingsController.PurchaseSetting(purchaseSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(purchaseSettingResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var activeAccountsDropdownResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse37 = await oCommonController.ExtractResponseFromActionResult(activeAccountsDropdownResult);

            var activeTaxsResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(activeTaxsResult);

            WebApi.TaxTypeController taxTypeController = new WebApi.TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var activeTaxTypesResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(activeTaxTypesResult);

            WebApi.StateController stateController = new WebApi.StateController();
            ClsStateVm stateObj = new ClsStateVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeStatesResult = await stateController.ActiveStates(stateObj);
            ClsResponse oClsResponse54 = await oCommonController.ExtractResponseFromActionResult(activeStatesResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBusinessRegistrationNamesResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(activeBusinessRegistrationNamesResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeTaxSettingsResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(activeTaxSettingsResult);

            ClsPrefixVm prefixObj = new ClsPrefixVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activePrefixsResult = await prefixController.ActivePrefixs(prefixObj);
            ClsResponse oClsResponse57 = await oCommonController.ExtractResponseFromActionResult(activePrefixsResult);

            ViewBag.Branch = oClsResponse.Data.Branch;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.Currencys = oClsResponse.Data.Currencys;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.Accounts = oClsResponse5.Data.Accounts;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;
            ViewBag.SaleSetting = oClsResponse12.Data.SaleSetting;
            ViewBag.PrefixMasters = oClsResponse11.Data.PrefixMasters;
            ViewBag.PurchaseSetting = oClsResponse14.Data.PurchaseSetting;
            ViewBag.AccountSubTypes = oClsResponse37.Data.AccountSubTypes;
            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.States = oClsResponse54.Data.States;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;
            ViewBag.Prefixs = oClsResponse57.Data.Prefixs;

            ViewBag.AccountsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "accounts").FirstOrDefault();
            ViewBag.PosPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "pos").FirstOrDefault();
            //ViewBag.IsAccounts = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower()== "accounts").FirstOrDefault() ;
            //ViewBag.PlanAddons = oClsResponse21.Data.PlanAddons;

            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();
            ViewBag.PaymentMethodsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "payment methods").FirstOrDefault();
            ViewBag.TaxSettingPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax settings").FirstOrDefault();
            ViewBag.PrefixPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "prefix").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return PartialView("PartialBranchView");
        }

        public async Task<ActionResult> BranchCount()
        {
            ClsBranchVm obj = new ClsBranchVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.BranchController branchController = new WebApi.BranchController();
            var branchCountResult = await branchController.BranchCount(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchCountResult);
            return Json(oClsResponse);
        }


        #endregion


    }
}