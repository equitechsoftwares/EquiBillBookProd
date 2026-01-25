using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Admin
{
    [AdminAuthorizationPrivilegeFilter]
    public class AdminCouponController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        ConnectionContext oConnectionContext = new ConnectionContext();

        public async Task<ActionResult> Index(ClsCouponVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.PageIndex = 1;
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CouponController couponController = new WebApi.CouponController();
            var result = await couponController.AllCoupons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Coupons = oClsResponse.Data.Coupons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            return View();
        }
        
        public async Task<ActionResult> CouponFetch(ClsCouponVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            
            if (obj.PageIndex <= 0)
            {
                obj.PageIndex = 1;
            }
            
            if (obj.PageSize <= 0)
            {
                obj.PageSize = 10;
            }
            
            serializer.MaxJsonLength = 2147483644;

            WebApi.CouponController couponController = new WebApi.CouponController();
            var result = await couponController.AllCoupons(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);

            ViewBag.Coupons = oClsResponse.Data.Coupons;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            return PartialView("PartialAdminCoupon");
        }

        public async Task<ActionResult> Add()
        {
            // Get all term lengths for the form
            ClsTermLengthVm termLengthObj = new ClsTermLengthVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                termLengthObj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.TermLengthController termLengthController = new WebApi.TermLengthController();
            var termLengthResult = await termLengthController.ActiveTermLengths(termLengthObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(termLengthResult);
            ViewBag.TermLengths = oClsResponse.Data.TermLengths;

            // Get all addons for the form (not filtered by company)
            var allAddons = oConnectionContext.DbClsPlanAddons
                .Where(a => a.IsDeleted == false && a.IsActive == true)
                .OrderBy(a => a.OrderNo)
                .ThenBy(a => a.Title)
                .Select(a => new ClsPlanAddonsVm
                {
                    PlanAddonsId = a.PlanAddonsId,
                    Title = a.Title,
                    Type = a.Type,
                    Description = a.Description
                }).ToList();
            ViewBag.AllAddons = allAddons;

            return View();
        }

        public async Task<ActionResult> Edit(long CouponId)
        {
            ClsCouponVm obj = new ClsCouponVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            obj.CouponId = CouponId;
            serializer.MaxJsonLength = 2147483644;

            WebApi.CouponController couponController = new WebApi.CouponController();
            var couponResult = await couponController.Coupon(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(couponResult);
            ViewBag.Coupon = oClsResponse.Data.Coupon;
            ViewBag.TermLengths = oClsResponse.Data.TermLengths;

            // Get all term lengths for the form
            ClsTermLengthVm termLengthObj = new ClsTermLengthVm();
            termLengthObj.CompanyId = obj.CompanyId;
            
            WebApi.TermLengthController termLengthController = new WebApi.TermLengthController();
            var termLengthResult = await termLengthController.ActiveTermLengths(termLengthObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(termLengthResult);
            ViewBag.AllTermLengths = oClsResponse2.Data.TermLengths;

            // Get addons for this coupon
            ViewBag.Addons = oClsResponse.Data.Addons ?? new List<ClsCouponAddonVm>();

            // Get all addons for the form (not filtered by company)
            var allAddons = oConnectionContext.DbClsPlanAddons
                .Where(a => a.IsDeleted == false && a.IsActive == true)
                .OrderBy(a => a.OrderNo)
                .ThenBy(a => a.Title)
                .Select(a => new ClsPlanAddonsVm
                {
                    PlanAddonsId = a.PlanAddonsId,
                    Title = a.Title,
                    Type = a.Type,
                    Description = a.Description
                }).ToList();
            ViewBag.AllAddons = allAddons;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Insert(ClsCouponVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
            }
            obj.IsActive = true;
            serializer.MaxJsonLength = 2147483644;

            WebApi.CouponController couponController = new WebApi.CouponController();
            var result = await couponController.InsertCoupon(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Update(ClsCouponVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.ModifiedBy = Convert.ToInt64(arr[2]);
                if (obj.CompanyId == 0)
                {
                    obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                }
                if (obj.AddedBy == 0)
                {
                    obj.AddedBy = Convert.ToInt64(arr[2]);
                }
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CouponController couponController = new WebApi.CouponController();
            var result = await couponController.UpdateCoupon(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(ClsCouponVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.ModifiedBy = Convert.ToInt64(arr[2]);
                if (obj.CompanyId == 0)
                {
                    obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                }
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CouponController couponController = new WebApi.CouponController();
            var result = await couponController.DeleteCoupon(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> GetCoupon(ClsCouponVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                if (obj.CompanyId == 0)
                {
                    obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                }
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CouponController couponController = new WebApi.CouponController();
            var result = await couponController.Coupon(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> ActiveInactive(ClsCouponVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["adata"] != null)
            {
                arr[0] = Request.Cookies["adata"]["UserType"];
                arr[1] = Request.Cookies["adata"]["Token"];
                arr[2] = Request.Cookies["adata"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["adata"]["CompanyId"]);
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            serializer.MaxJsonLength = 2147483644;

            WebApi.CouponController couponController = new WebApi.CouponController();
            var result = await couponController.CouponActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(result);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }
    }
}