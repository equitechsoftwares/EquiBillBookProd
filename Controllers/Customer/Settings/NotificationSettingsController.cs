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

namespace EquiBillBook.Controllers.Customer.Settings
{
    [AuthorizationPrivilegeFilter]
    public class NotificationSettingsController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        // GET: NotificationSettings
        public async Task<ActionResult> NotificationTemplates()
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
                obj.UserId = Convert.ToInt64(arr[2]);
            }
            WebApi.NotificationModulesController notificationModulesController = new WebApi.NotificationModulesController();
            ClsNotificationModulesVm notificationModulesObj = new ClsNotificationModulesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var activeNotificationModulesResult = await notificationModulesController.ActiveNotificationModules(notificationModulesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(activeNotificationModulesResult);

            ViewBag.NotificationModules = oClsResponse.Data.NotificationModules;
            return View();
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> NotificationTemplateUpdate(ClsNotificationModulesSettingsVm obj)
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
            WebApi.NotificationTemplatesController notificationTemplatesController = new WebApi.NotificationTemplatesController();
            var updateNotificationTemplateResult = await notificationTemplatesController.UpdateNotificationTemplate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateNotificationTemplateResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> FetchNotificationModule(ClsNotificationModulesVm obj)
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
            WebApi.NotificationModulesController notificationModulesController = new WebApi.NotificationModulesController();
            var fetchNotificationModuleResult = await notificationModulesController.FetchNotificationModule(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(fetchNotificationModuleResult);

            ViewBag.NotificationModulesDetail = oClsResponse.Data.NotificationModulesDetail;

            return PartialView("PartialNotificationModule");
        }

        public async Task<ActionResult> SendNotifications(ClsNotificationModulesSettingsVm obj)
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
            WebApi.NotificationModulesController notificationModulesController = new WebApi.NotificationModulesController();
            var sendNotificationsResult = await notificationModulesController.SendNotifications(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(sendNotificationsResult);
            return Json(oClsResponse);
        }


        #region Reminders

        public async Task<ActionResult> Reminders()
        {
            ClsReminderModulesSettingsVm obj = new ClsReminderModulesSettingsVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            WebApi.ReminderModulesController reminderModulesController = new WebApi.ReminderModulesController();
            ClsReminderModulesVm reminderModulesObj = new ClsReminderModulesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var reminderModulesResult = await reminderModulesController.ReminderModules(reminderModulesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(reminderModulesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SalesReminderModule = oClsResponse.Data.ReminderModules.Where(a => a.Name == "Sales Invoice").FirstOrDefault();
            ViewBag.PurchaseReminderModule = oClsResponse.Data.ReminderModules.Where(a => a.Name == "Purchase Bill").FirstOrDefault();

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "reminders").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> RemindersFetch(ClsReminderModulesSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Items";
            }
            WebApi.ReminderModulesController reminderModulesController = new WebApi.ReminderModulesController();
            ClsReminderModulesVm reminderModulesObj = new ClsReminderModulesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var reminderModulesResult = await reminderModulesController.ReminderModules(reminderModulesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(reminderModulesResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SalesReminderModule = oClsResponse.Data.ReminderModules.Where(a => a.Name == "Sales Invoice").FirstOrDefault();
            ViewBag.PurchaseReminderModule = oClsResponse.Data.ReminderModules.Where(a => a.Name == "Purchase Bill").FirstOrDefault();

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "reminders").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();

            return PartialView("PartialReminders");
        }

        public async Task<ActionResult> ReminderEdit(ClsReminderModulesSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "customer";
            }
            WebApi.ReminderModulesController reminderModulesController = new WebApi.ReminderModulesController();
            var reminderModulesSettingResult = await reminderModulesController.ReminderModulesSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(reminderModulesSettingResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var allActiveUsersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(allActiveUsersResult);

            ViewBag.ReminderModulesSetting = oClsResponse.Data.ReminderModulesSetting;
            ViewBag.ReminderModule = oClsResponse.Data.ReminderModule;
            ViewBag.Users = oClsResponse6.Data.Users;
            return View();
        }
        public async Task<ActionResult> ReminderAdd(ClsReminderModulesSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "customer";
            }
            WebApi.ReminderModulesController reminderModulesController = new WebApi.ReminderModulesController();
            ClsReminderModulesVm reminderModulesObj = new ClsReminderModulesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var fetchAvailableTagsResult = await reminderModulesController.FetchAvailableTags(reminderModulesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(fetchAvailableTagsResult);

            WebApi.UserController userController = new WebApi.UserController();
            ClsUserVm userObj = new ClsUserVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var allActiveUsersResult = await userController.AllActiveUsers(userObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(allActiveUsersResult);

            ViewBag.ReminderModule = oClsResponse.Data.ReminderModule;
            ViewBag.Users = oClsResponse6.Data.Users;
            return View();
        }
        public async Task<ActionResult> ReminderInsert(ClsReminderModulesSettingsVm obj)
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
            WebApi.ReminderModulesController reminderModulesController = new WebApi.ReminderModulesController();
            var reminderTemplateInsertResult = await reminderModulesController.ReminderTemplateInsert(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(reminderTemplateInsertResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ReminderUpdate(ClsReminderModulesSettingsVm obj)
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

            WebApi.ReminderModulesController reminderModulesController = new WebApi.ReminderModulesController();
            var reminderTemplateUpdateResult = await reminderModulesController.ReminderTemplateUpdate(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(reminderTemplateUpdateResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ReminderActiveInactive(ClsReminderModulesSettingsVm obj)
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

            WebApi.ReminderModulesController reminderModulesController = new WebApi.ReminderModulesController();
            var reminderActiveInactiveResult = await reminderModulesController.ReminderActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(reminderActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ReminderDelete(ClsReminderModulesSettingsVm obj)
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

            WebApi.ReminderModulesController reminderModulesController = new WebApi.ReminderModulesController();
            var reminderDeleteResult = await reminderModulesController.ReminderDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(reminderDeleteResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> FetchReminderModule(ClsReminderModulesSettingsVm obj)
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

            WebApi.ReminderModulesController reminderModulesController = new WebApi.ReminderModulesController();
            var fetchReminderModuleResult = await reminderModulesController.FetchReminderModule(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(fetchReminderModuleResult);

            ViewBag.ReminderModulesSetting = oClsResponse.Data.ReminderModulesSetting;
            ViewBag.ReminderModule = oClsResponse.Data.ReminderModule;

            return PartialView("PartialReminderModule");
        }

        public async Task<ActionResult> SendReminders(ClsReminderModulesSettingsVm obj)
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
            WebApi.ReminderModulesController reminderModulesController = new WebApi.ReminderModulesController();
            var sendRemindersResult = await reminderModulesController.SendReminders(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(sendRemindersResult);
            return Json(oClsResponse);
        }

        #endregion

        #region Email Settings
        public async Task<ActionResult> EmailSettings(ClsEmailSettingsVm obj)
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
            }
            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var allEmailSettingsResult = await emailSettingsController.AllEmailSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allEmailSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.EmailSettings = oClsResponse.Data.EmailSettings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "email settings").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> EmailSettingsFetch(ClsEmailSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Categories";
            }
            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var allEmailSettingsResult = await emailSettingsController.AllEmailSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allEmailSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.EmailSettings = oClsResponse.Data.EmailSettings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "email settings").FirstOrDefault();

            return PartialView("PartialEmailSettings");
        }
        public async Task<ActionResult> EmailSettingEdit(ClsEmailSettingsVm obj)
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
            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var emailSettingResult = await emailSettingsController.EmailSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(emailSettingResult);

            ViewBag.EmailSetting= oClsResponse.Data.EmailSetting;
            return View();
        }
        public async Task<ActionResult> EmailSettingAdd(ClsEmailSettingsVm obj)
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
            }
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "EmailSettings/AllEmailSettings", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            return View();
        }
        public async Task<ActionResult> EmailSettingInsert(ClsEmailSettingsVm obj)
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
            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var insertEmailSettingResult = await emailSettingsController.InsertEmailSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertEmailSettingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> EmailSettingUpdate(ClsEmailSettingsVm obj)
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

            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var updateEmailSettingResult = await emailSettingsController.UpdateEmailSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateEmailSettingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> EmailSettingActiveInactive(ClsEmailSettingsVm obj)
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

            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var emailSettingActiveInactiveResult = await emailSettingsController.EmailSettingActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(emailSettingActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> EmailSettingDelete(ClsEmailSettingsVm obj)
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

            WebApi.EmailSettingsController emailSettingsController = new WebApi.EmailSettingsController();
            var emailSettingDeleteResult = await emailSettingsController.EmailSettingDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(emailSettingDeleteResult);
            return Json(oClsResponse);
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> SendTestEmail(ClsEmailSettingsVm obj)
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

            WebApi.NotificationModulesController notificationModulesController = new WebApi.NotificationModulesController();
            var sendTestEmailResult = await notificationModulesController.SendTestEmail(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(sendTestEmailResult);
            return Json(oClsResponse);
        }
        #endregion

        #region Sms Settings
        public async Task<ActionResult> SmsSettings(ClsSmsSettingsVm obj)
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
                //obj.Title = "Categories";
            }
            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var allSmsSettingsResult = await smsSettingsController.AllSmsSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSmsSettingsResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(businessSettingResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SmsSettings = oClsResponse.Data.SmsSettings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.BusinessSetting = oClsResponse6.Data.BusinessSetting;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sms settings").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> SmsSettingsFetch(ClsSmsSettingsVm obj)
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
            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var allSmsSettingsResult = await smsSettingsController.AllSmsSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allSmsSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.SmsSettings = oClsResponse.Data.SmsSettings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sms settings").FirstOrDefault();

            return PartialView("PartialSmsSettings");
        }
        public async Task<ActionResult> SmsSettingEdit(ClsSmsSettingsVm obj)
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
            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var smsSettingResult = await smsSettingsController.SmsSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(smsSettingResult);

            ViewBag.SmsSetting= oClsResponse.Data.SmsSetting;
            return View();
        }
        public async Task<ActionResult> SmsSettingAdd(ClsSmsSettingsVm obj)
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
                //obj.Title = "Categories";
            }
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);

            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "SmsSettings/AllSmsSettings", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            return View();
        }
        public async Task<ActionResult> SmsSettingInsert(ClsSmsSettingsVm obj)
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
            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var insertSmsSettingResult = await smsSettingsController.InsertSmsSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertSmsSettingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SmsSettingUpdate(ClsSmsSettingsVm obj)
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

            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var updateSmsSettingResult = await smsSettingsController.UpdateSmsSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateSmsSettingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SmsSettingActiveInactive(ClsSmsSettingsVm obj)
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

            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var smsSettingActiveInactiveResult = await smsSettingsController.SmsSettingActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(smsSettingActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> SmsSettingDelete(ClsSmsSettingsVm obj)
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

            WebApi.SmsSettingsController smsSettingsController = new WebApi.SmsSettingsController();
            var smsSettingDeleteResult = await smsSettingsController.SmsSettingDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(smsSettingDeleteResult);
            return Json(oClsResponse);
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> sendTestSms(ClsSmsSettingsVm obj)
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

            WebApi.NotificationModulesController notificationModulesController = new WebApi.NotificationModulesController();
            var sendTestSmsResult = await notificationModulesController.SendTestSms(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(sendTestSmsResult);
            return Json(oClsResponse);
        }
        #endregion

        #region Whatsapp Settings
        public async Task<ActionResult> WhatsappSettings(ClsWhatsappSettingsVm obj)
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
                //obj.Title = "Categories";
            }
            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var allWhatsappSettingsResult = await whatsappSettingsController.AllWhatsappSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allWhatsappSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.WhatsappSettings = oClsResponse.Data.WhatsappSettings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "whatsapp settings").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> WhatsappSettingsFetch(ClsWhatsappSettingsVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //obj.Title = "Categories";
            }
            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var allWhatsappSettingsResult = await whatsappSettingsController.AllWhatsappSettings(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(allWhatsappSettingsResult);

            WebApi.MenuController menuController = new WebApi.MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var controlsPermissionResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(controlsPermissionResult);

            ViewBag.WhatsappSettings = oClsResponse.Data.WhatsappSettings;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "whatsapp settings").FirstOrDefault();

            return PartialView("PartialWhatsappSettings");
        }
        public async Task<ActionResult> WhatsappSettingEdit(ClsWhatsappSettingsVm obj)
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
            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var whatsappSettingResult = await whatsappSettingsController.WhatsappSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(whatsappSettingResult);

            ViewBag.WhatsappSetting = oClsResponse.Data.WhatsappSetting;
            return View();
        }
        public async Task<ActionResult> WhatsappSettingAdd(ClsWhatsappSettingsVm obj)
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
                //obj.Title = "Categories";
            }
            serializer.MaxJsonLength = 2147483644;
            string _json = serializer.Serialize(obj);
            var res = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "WhatsappSettings/AllWhatsappSettings", arr[0], arr[1], arr[2]);
            ClsResponse oClsResponse = serializer.Deserialize<ClsResponse>(res);

            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            return View();
        }
        public async Task<ActionResult> WhatsappSettingInsert(ClsWhatsappSettingsVm obj)
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
            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var insertWhatsappSettingResult = await whatsappSettingsController.InsertWhatsappSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(insertWhatsappSettingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> WhatsappSettingUpdate(ClsWhatsappSettingsVm obj)
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

            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var updateWhatsappSettingResult = await whatsappSettingsController.UpdateWhatsappSetting(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(updateWhatsappSettingResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> WhatsappSettingActiveInactive(ClsWhatsappSettingsVm obj)
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

            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var whatsappSettingActiveInactiveResult = await whatsappSettingsController.WhatsappSettingActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(whatsappSettingActiveInactiveResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> WhatsappSettingDelete(ClsWhatsappSettingsVm obj)
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

            WebApi.WhatsappSettingsController whatsappSettingsController = new WebApi.WhatsappSettingsController();
            var whatsappSettingDeleteResult = await whatsappSettingsController.WhatsappSettingDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(whatsappSettingDeleteResult);
            return Json(oClsResponse);
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> SendTestWhatsapp(ClsWhatsappSettingsVm obj)
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

            WebApi.NotificationModulesController notificationModulesController = new WebApi.NotificationModulesController();
            var sendTestWhatsappResult = await notificationModulesController.SendTestWhatsapp(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(sendTestWhatsappResult);
            return Json(oClsResponse);
        }
        #endregion

    }
}