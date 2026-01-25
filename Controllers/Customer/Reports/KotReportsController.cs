using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Customer.Reports
{
    [AuthorizationPrivilegeFilter]
    public class KotReportsController : Controller
    {
        CommonController oCommonController = new CommonController();

        // GET: KotReports/KotSummary
        public async Task<ActionResult> KotSummary(string returnUrl = null)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // POST: KotReports/KotSummaryFetch
        [HttpPost]
        public async Task<ActionResult> KotSummaryFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialKotSummary");
        }

        // GET: KotReports/KitchenPerformance
        public async Task<ActionResult> KitchenPerformance()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/KitchenPerformanceFetch
        [HttpPost]
        public async Task<ActionResult> KitchenPerformanceFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialKitchenPerformance");
        }

        // GET: KotReports/ItemWiseKot
        public async Task<ActionResult> ItemWiseKot()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/ItemWiseKotFetch
        [HttpPost]
        public async Task<ActionResult> ItemWiseKotFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialItemWiseKot");
        }

        // GET: KotReports/StandaloneKotReport
        public async Task<ActionResult> StandaloneKotReport()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
                obj.WithSales = false; // Standalone KOTs (without sales)
            }
            obj.UserType = "customer";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/StandaloneKotReportFetch
        [HttpPost]
        public async Task<ActionResult> StandaloneKotReportFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.WithSales = false; // Standalone KOTs
            }
            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialStandaloneKotReport");
        }

        // GET: KotReports/LinkedKotReport
        public async Task<ActionResult> LinkedKotReport()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
                obj.WithSales = true; // Linked KOTs (with sales)
            }
            obj.UserType = "customer";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/LinkedKotReportFetch
        [HttpPost]
        public async Task<ActionResult> LinkedKotReportFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.WithSales = true; // Linked KOTs
            }
            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialLinkedKotReport");
        }

        // GET: KotReports/StationWisePerformance
        public async Task<ActionResult> StationWisePerformance()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            var kitchenStationController = new WebApi.KitchenStationController();
            ClsKitchenStationVm oClsKitchenStationVm = new ClsKitchenStationVm
            {
                CompanyId = obj.CompanyId,
                BranchId = obj.BranchId,
                AddedBy = obj.AddedBy
            };
            var stationsResult = await kitchenStationController.GetStations(oClsKitchenStationVm);
            var oClsResponseStations = await oCommonController.ExtractResponseFromActionResult(stationsResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.Stations = oClsResponseStations.Data?.Stations ?? new System.Collections.Generic.List<ClsKitchenStationVm>();
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/StationWisePerformanceFetch
        [HttpPost]
        public async Task<ActionResult> StationWisePerformanceFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.KitchenStationId = obj.KitchenStationId;

            return PartialView("PartialStationWisePerformance");
        }

        // GET: KotReports/TableWiseRevenue
        public async Task<ActionResult> TableWiseRevenue(string returnUrl = null)
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // POST: KotReports/TableWiseRevenueFetch
        [HttpPost]
        public async Task<ActionResult> TableWiseRevenueFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialTableWiseRevenue");
        }

        // GET: KotReports/FloorWisePerformance
        public async Task<ActionResult> FloorWisePerformance()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            var restaurantFloorController = new WebApi.RestaurantFloorController();
            ClsRestaurantFloorVm oClsRestaurantFloorVm = new ClsRestaurantFloorVm
            {
                CompanyId = obj.CompanyId,
                BranchId = obj.BranchId,
                AddedBy = obj.AddedBy
            };
            var floorsResult = await restaurantFloorController.GetFloors(oClsRestaurantFloorVm);
            ClsResponse oClsResponseFloors = await oCommonController.ExtractResponseFromActionResult(floorsResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.Floors = oClsResponseFloors.Data?.Floors ?? new System.Collections.Generic.List<ClsRestaurantFloorVm>();
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/FloorWisePerformanceFetch
        [HttpPost]
        public async Task<ActionResult> FloorWisePerformanceFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialFloorWisePerformance");
        }

        // GET: KotReports/PeakHoursAnalysis
        public async Task<ActionResult> PeakHoursAnalysis()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/PeakHoursAnalysisFetch
        [HttpPost]
        public async Task<ActionResult> PeakHoursAnalysisFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }
            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialPeakHoursAnalysis");
        }

        // GET: KotReports/KotCancellationReport
        public async Task<ActionResult> KotCancellationReport()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.OrderStatus = "Cancelled";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/KotCancellationReportFetch
        [HttpPost]
        public async Task<ActionResult> KotCancellationReportFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.OrderStatus = "Cancelled";
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialKotCancellationReport");
        }

        // GET: KotReports/AveragePreparationTime
        public async Task<ActionResult> AveragePreparationTime()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/AveragePreparationTimeFetch
        [HttpPost]
        public async Task<ActionResult> AveragePreparationTimeFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialAveragePreparationTime");
        }

        // GET: KotReports/TableTurnover
        public async Task<ActionResult> TableTurnover()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize > 0 ? oClsResponse.Data.PageSize : 10;
            ViewBag.FromDate = DateTime.Now.Date.AddDays(-30);
            ViewBag.ToDate = DateTime.Now.Date;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/TableTurnoverFetch
        [HttpPost]
        public async Task<ActionResult> TableTurnoverFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKots(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.Kots = oClsResponse.Data.Kots ?? new System.Collections.Generic.List<ClsKotMasterVm>();
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)(oClsResponse.Data.TotalCount > 0 ? oClsResponse.Data.TotalCount : 0) / (obj.PageSize > 0 ? obj.PageSize : 10));
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize > 0 ? obj.PageSize : 10;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialTableTurnover");
        }

        // GET: KotReports/CategoryWiseKotPerformance
        public async Task<ActionResult> CategoryWiseKotPerformance()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.FromDate = DateTime.Now.Date.AddDays(-30);
            obj.ToDate = DateTime.Now.Date;

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetCategoryWiseKotPerformance(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            var categoryData = oClsResponse.Data as dynamic;
            ViewBag.CategoryPerformance = categoryData?.CategoryPerformance;
            ViewBag.TotalCount = categoryData?.TotalCount != null ? (long)categoryData.TotalCount : 0;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/CategoryWiseKotPerformanceFetch
        [HttpPost]
        public async Task<ActionResult> CategoryWiseKotPerformanceFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetCategoryWiseKotPerformance(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var categoryData = oClsResponse.Data as dynamic;
            ViewBag.CategoryPerformance = categoryData?.CategoryPerformance;
            ViewBag.TotalCount = categoryData?.TotalCount != null ? (long)categoryData.TotalCount : 0;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialCategoryWiseKotPerformance");
        }

        // GET: KotReports/HourlyRevenueReport
        public async Task<ActionResult> HourlyRevenueReport()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.FromDate = DateTime.Now.Date.AddDays(-30);
            obj.ToDate = DateTime.Now.Date;

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetHourlyRevenueReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            var hourlyData = oClsResponse.Data as dynamic;
            ViewBag.HourlyRevenue = hourlyData?.HourlyRevenue;
            ViewBag.TotalCount = hourlyData?.TotalCount != null ? (long)hourlyData.TotalCount : 0;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/HourlyRevenueReportFetch
        [HttpPost]
        public async Task<ActionResult> HourlyRevenueReportFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetHourlyRevenueReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var hourlyData = oClsResponse.Data as dynamic;
            ViewBag.HourlyRevenue = hourlyData?.HourlyRevenue;
            ViewBag.TotalCount = hourlyData?.TotalCount != null ? (long)hourlyData.TotalCount : 0;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialHourlyRevenueReport");
        }

        // GET: KotReports/KotStatusTransitionReport
        public async Task<ActionResult> KotStatusTransitionReport()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.FromDate = DateTime.Now.Date.AddDays(-30);
            obj.ToDate = DateTime.Now.Date;

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKotStatusTransitionReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.StatusTransitions = oClsResponse.Data?.StatusTransitions;
            ViewBag.TotalCount = oClsResponse.Data?.TotalCount ?? 0;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/KotStatusTransitionReportFetch
        [HttpPost]
        public async Task<ActionResult> KotStatusTransitionReportFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetKotStatusTransitionReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.StatusTransitions = oClsResponse.Data?.StatusTransitions;
            ViewBag.TotalCount = oClsResponse.Data?.TotalCount ?? 0;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialKotStatusTransitionReport");
        }

        // GET: KotReports/StaffPerformanceReport
        public async Task<ActionResult> StaffPerformanceReport()
        {
            ClsKotMasterVm obj = new ClsKotMasterVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
                obj.PageIndex = 1;
            }
            obj.UserType = "customer";
            obj.FromDate = DateTime.Now.Date.AddDays(-30);
            obj.ToDate = DateTime.Now.Date;

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetStaffPerformanceReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            var menuController = new WebApi.MenuController();
            ClsMenuVm oClsMenuVm = new ClsMenuVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var menuResult = await menuController.ControlsPermission(oClsMenuVm);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var branchController = new WebApi.BranchController();
            ClsBranchVm oClsBranchVm = new ClsBranchVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var branchResult = await branchController.ActiveBranchs(oClsBranchVm);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.StaffPerformance = oClsResponse.Data?.StaffPerformance;
            ViewBag.TotalCount = oClsResponse.Data?.TotalCount ?? 0;
            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions?.Where(a => a.Menu.ToLower() == "kot reports").FirstOrDefault();

            return View();
        }

        // POST: KotReports/StaffPerformanceReportFetch
        [HttpPost]
        public async Task<ActionResult> StaffPerformanceReportFetch(ClsKotMasterVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.BranchId = Convert.ToInt64(Request.Cookies["data"]["BranchId"]);
            }

            var kotController = new WebApi.KotController();
            var kotResult = await kotController.GetStaffPerformanceReport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(kotResult);

            ViewBag.StaffPerformance = oClsResponse.Data?.StaffPerformance;
            ViewBag.TotalCount = oClsResponse.Data?.TotalCount ?? 0;
            ViewBag.FromDate = obj.FromDate;
            ViewBag.ToDate = obj.ToDate;

            return PartialView("PartialStaffPerformanceReport");
        }
    }
}

