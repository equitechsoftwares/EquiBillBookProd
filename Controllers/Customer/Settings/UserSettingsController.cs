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

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class UserSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        // GET: UserSettings
        #region user
        public async Task<ActionResult> Users(long? BranchId)
        {
            ClsUserVm obj = new ClsUserVm();
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
                obj.UserType = "user";
                //obj.Title = "Users";
                ViewBag.AddedBy = obj.AddedBy;
                ViewBag.CompanyId = obj.CompanyId;
                if (BranchId != null)
                {
                    obj.BranchId = Convert.ToInt64(BranchId);
                    ViewBag.BranchId = obj.BranchId;
                }
            }
            WebApi.UserController userController = new WebApi.UserController();
            var allUsersResult = await userController.AllUsers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allUsersResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            ViewBag.Users = oClsResponse.Data.Users;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            return View();
        }
        public async Task<ActionResult> UserFetch(ClsUserVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "user";
                //obj.Title = "Users";
                ViewBag.AddedBy = Convert.ToInt64(arr[2]);
            }
            WebApi.UserController userController = new WebApi.UserController();
            var allUsersResult = await userController.AllUsers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allUsersResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "users").FirstOrDefault();

            return PartialView("PartialUser");
        }
        public async Task<ActionResult> UserEdit(long UserId)
        {
            ClsUserVm obj = new ClsUserVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = UserId;
            }
            WebApi.UserController userController = new WebApi.UserController();
            ClsUser userObj = new ClsUser { UserId = obj.UserId, CompanyId = obj.CompanyId };
            var userResult = await userController.User(userObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.AltStates = oClsResponse.Data.AltStates;
            ViewBag.AltCitys = oClsResponse.Data.AltCitys;
            ViewBag.Roles = oClsResponse.Data.Roles;
            ViewBag.Religions = oClsResponse.Data.Religions;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;

            ViewBag.UserRolePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "user role").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> UserAdd()
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

            WebApi.RoleController roleController = new WebApi.RoleController();
            ClsRoleVm roleObj = new ClsRoleVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeRolesResult = await roleController.ActiveRoles(roleObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(activeRolesResult);

            WebApi.ReligionController religionController = new WebApi.ReligionController();
            ClsReligionVm religionObj = new ClsReligionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeReligionsResult = await religionController.ActiveReligions(religionObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(activeReligionsResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Countrys = oClsResponse.Data.Countrys;
            //ViewBag.AltCountrys = oClsResponse.Data.Countrys;
            ViewBag.Roles = oClsResponse1.Data.Roles;
            ViewBag.Religions = oClsResponse2.Data.Religions;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;

            ViewBag.UserRolePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "user role").FirstOrDefault();
            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();
            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UserInsert(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var insertUserResult = await userController.InsertUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertUserResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UserUpdate(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var updateUserResult = await userController.UpdateUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateUserResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UserActiveInactive(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var userActiveInactiveResult = await userController.UserActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userActiveInactiveResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UserDelete(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var userDeleteResult = await userController.UserDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ActiveStates(ClsStateVm obj)
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
            var activeStatesResult = await stateController.ActiveStates(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeStatesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.StatePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "state").FirstOrDefault();

            if (obj.Type == "" || obj.Type == null)
            {
                ViewBag.States = oClsResponse.Data.States;
                return PartialView("PartialStatesDropdown");
            }
            else
            {
                ViewBag.AltStates = oClsResponse.Data.States;
                return PartialView("PartialAltStatesDropdown");
            }
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
            var activeCitysResult = await cityController.ActiveCitys(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeCitysResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.CityPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "city").FirstOrDefault();

            if (obj.Type == "" || obj.Type == null)
            {
                ViewBag.Citys = oClsResponse.Data.Citys;
                return PartialView("PartialCitysDropdown");
            }
            else
            {
                ViewBag.AltCitys = oClsResponse.Data.Citys;
                return PartialView("PartialAltCitysDropdown");
            }
        }
        public async Task<ActionResult> UserAutocomplete(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var userAutocompleteResult = await userController.UserAutocomplete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userAutocompleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> AddExisting(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var addExistingResult = await userController.AddExisting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(addExistingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> UserView(long UserId)
        {
            ClsUserVm obj = new ClsUserVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = UserId;
            }
            WebApi.UserController userController = new WebApi.UserController();
            ClsUser userObj = new ClsUser { UserId = obj.UserId, CompanyId = obj.CompanyId };
            var userResult = await userController.User(userObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            ViewBag.User = oClsResponse.Data.User;

            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            return PartialView("PartialUserView");
        }
        public async Task<ActionResult> AllActiveUsers(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var allActiveUsersResult = await userController.AllActiveUsers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allActiveUsersResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> FetchCompanyCurrency(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var fetchCompanyCurrencyResult = await userController.FetchCompanyCurrency(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(fetchCompanyCurrencyResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UpdateUserPassword(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var updateUserPasswordResult = await userController.UpdateUserPassword(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateUserPasswordResult);
            return Json(oClsResponse);
        }
        #endregion

        #region userrole
        public async Task<ActionResult> UserRole(ClsRoleVm obj)
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
            }
            WebApi.RoleController roleController = new WebApi.RoleController();
            var allRolesResult = await roleController.AllRoles(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allRolesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Roles = oClsResponse.Data.Roles;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "user role").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> UserRoleFetch(ClsRoleVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "User Role";
            }
            WebApi.RoleController roleController = new WebApi.RoleController();
            var allRolesResult = await roleController.AllRoles(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allRolesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.Roles = oClsResponse.Data.Roles;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "user role").FirstOrDefault();

            return PartialView("PartialUserRole");
        }
        public async Task<ActionResult> UserRoleEdit(long RoleId)
        {
            ClsRoleVm obj = new ClsRoleVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.RoleId = RoleId;
            }
            WebApi.RoleController roleController = new WebApi.RoleController();
            ClsRole roleObj = new ClsRole { RoleId = obj.RoleId, CompanyId = obj.CompanyId };
            var roleResult = await roleController.Role(roleObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(roleResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allMenuResult = await menuController.AllMenus(menuObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(allMenuResult);


            ViewBag.Role = oClsResponse.Data.Role;
            ViewBag.Menus = oClsResponse1.Data.Menus;
            return View();
        }
        public async Task<ActionResult> UserRoleAdd(ClsRoleVm obj)
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
            }
            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allMenuResult = await menuController.AllMenus(menuObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allMenuResult);

            ViewBag.Menus = oClsResponse.Data.Menus;

            return View();
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UserRoleInsert(ClsRoleVm obj)
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
            WebApi.RoleController roleController = new WebApi.RoleController();
            var insertRoleResult = await roleController.InsertRole(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertRoleResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UserRoleUpdate(ClsRoleVm obj)
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
            WebApi.RoleController roleController = new WebApi.RoleController();
            var updateRoleResult = await roleController.UpdateRole(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateRoleResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> RoleActiveInactive(ClsRoleVm obj)
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
            WebApi.RoleController roleController = new WebApi.RoleController();
            var roleActiveInactiveResult = await roleController.RoleActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(roleActiveInactiveResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UserRoleDelete(ClsRoleVm obj)
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
            WebApi.RoleController roleController = new WebApi.RoleController();
            var roleDeleteResult = await roleController.RoleDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(roleDeleteResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> UserRoleView(long RoleId)
        {
            ClsRoleVm obj = new ClsRoleVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.RoleId = RoleId;
            }
            WebApi.RoleController roleController = new WebApi.RoleController();
            ClsRole roleObj = new ClsRole { RoleId = obj.RoleId, CompanyId = obj.CompanyId };
            var roleResult = await roleController.Role(roleObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(roleResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var allMenuResult = await menuController.AllMenus(menuObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(allMenuResult);


            ViewBag.Role = oClsResponse.Data.Role;
            ViewBag.Menus = oClsResponse1.Data.Menus;
            return PartialView("PartialUserRoleView");
        }
        #endregion

        #region permission
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region salesagent
        public async Task<ActionResult> SalesAgent(ClsUserVm obj)
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
                obj.UserType = "sales";
                obj.Title = "Sales Agent";
            }
            WebApi.UserController userController = new WebApi.UserController();
            var allUsersResult = await userController.AllUsers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allUsersResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);


            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermission;
            return View();
        }
        public async Task<ActionResult> SalesAgentFetch(ClsUserVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "sales";
                obj.Title = "Sales Agent";
            }
            WebApi.UserController userController = new WebApi.UserController();
            var allUsersResult = await userController.AllUsers(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allUsersResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.Users = oClsResponse.Data.Users;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermission;

            return PartialView("PartialSalesAgent");
        }
        public async Task<ActionResult> SalesAgentEdit(long UserId)
        {
            ClsUserVm obj = new ClsUserVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = UserId;
            }
            WebApi.UserController userController = new WebApi.UserController();
            ClsUser userObj = new ClsUser { UserId = obj.UserId, CompanyId = obj.CompanyId };
            var userResult = await userController.User(userObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.AltStates = oClsResponse.Data.AltStates;
            ViewBag.AltCitys = oClsResponse.Data.AltCitys;
            ViewBag.Religions = oClsResponse.Data.Religions;
            return View();
        }
        public async Task<ActionResult> SalesAgentAdd()
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

            WebApi.ReligionController religionController = new WebApi.ReligionController();
            ClsReligionVm religionObj = new ClsReligionVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeReligionsResult = await religionController.ActiveReligions(religionObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(activeReligionsResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);


            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.Religions = oClsResponse2.Data.Religions;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            return View();
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> SalesAgentInsert(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var insertUserResult = await userController.InsertUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertUserResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> SalesAgentUpdate(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var updateUserResult = await userController.UpdateUser(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateUserResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> SalesAgentActiveInactive(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var userActiveInactiveResult = await userController.UserActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userActiveInactiveResult);
            return Json(oClsResponse);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> SalesAgentDelete(ClsUserVm obj)
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
            WebApi.UserController userController = new WebApi.UserController();
            var userDeleteResult = await userController.UserDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userDeleteResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ExistingSalesAgent(string MobileNo)
        {
            ClsUserVm obj = new ClsUserVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.MobileNo = MobileNo;
                obj.UserType = "sales";
            }
            WebApi.UserController userController = new WebApi.UserController();
            var userByMobileNoResult = await userController.UserByMobileNo(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(userByMobileNoResult);

            WebApi.BranchController branchController = new WebApi.BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeBranchsResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(activeBranchsResult);

            ViewBag.User = oClsResponse.Data.User;
            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.States = oClsResponse.Data.States;
            ViewBag.Citys = oClsResponse.Data.Citys;
            ViewBag.AltStates = oClsResponse.Data.AltStates;
            ViewBag.AltCitys = oClsResponse.Data.AltCitys;
            ViewBag.Religions = oClsResponse.Data.Religions;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            return View();
        }

        #endregion

    }
}