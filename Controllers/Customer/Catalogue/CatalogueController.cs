using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Customer.Catalogue
{
    [AuthorizationPrivilegeFilter]
    public class CatalogueController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<ActionResult> Index(ClsCatalogueVm obj)
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

            WebApi.CatalogueController catalogueController = new WebApi.CatalogueController();
            var catalogueResult = await catalogueController.AllCatalogues(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.Catalogues = oClsResponse.Data.Catalogues;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "catalogue").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> CatalogueFetch(ClsCatalogueVm obj)
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

            WebApi.CatalogueController catalogueController = new WebApi.CatalogueController();
            var catalogueResult = await catalogueController.AllCatalogues(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.Catalogues = oClsResponse.Data.Catalogues;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = obj.PageSize;
            ViewBag.PageIndex = obj.PageIndex;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "catalogue").FirstOrDefault();

            return PartialView("PartialCatalogue");
        }

        public async Task<ActionResult> Add()
        {
            ClsSalesVm obj = new ClsSalesVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            obj.CountryId = 2;

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, CountryId = obj.CountryId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            SellingPriceGroupController sellingPriceGroupController = new SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            var branchList = oClsResponse.Data.Branchs ?? new List<ClsBranchVm>();
            ViewBag.Branchs = branchList;
            long selectedBranchId = branchList.Any() ? branchList.First().BranchId : 0;
            ViewBag.SelectedBranchId = selectedBranchId;

            CategoryController categoryController = new CategoryController();
            ClsCategoryVm categoryRequest = new ClsCategoryVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var categoryResult = await categoryController.ActiveCategorys(categoryRequest);
            ClsResponse oClsResponseCategory = await oCommonController.ExtractResponseFromActionResult(categoryResult);

            BrandController brandController = new BrandController();
            ClsBrandVm brandRequest = new ClsBrandVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var brandResult = await brandController.ActiveBrands(brandRequest);
            ClsResponse oClsResponseBrand = await oCommonController.ExtractResponseFromActionResult(brandResult);

            ItemController itemController = new ItemController();
            ClsItemVm itemRequest = new ClsItemVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                BranchId = selectedBranchId
            };
            var itemsResult = await itemController.ActiveItems(itemRequest);
            ClsResponse oClsResponseItems = await oCommonController.ExtractResponseFromActionResult(itemsResult);

            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;
            ViewBag.Categories = oClsResponseCategory.Data.Categories ?? new List<ClsCategoryVm>();
            ViewBag.Brands = oClsResponseBrand.Data.Brands ?? new List<ClsBrandVm>();
            ViewBag.Items = oClsResponseItems.Data.Items ?? new List<ClsItemVm>();

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ItemsByBranch(long branchId)
        {
            string[] arr = { "", "", "" };
            long companyId = 0;
            long addedBy = 0;
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                long.TryParse(arr[2], out addedBy);
                long.TryParse(Request.Cookies["data"]["CompanyId"], out companyId);
            }

            ClsItemVm itemRequest = new ClsItemVm
            {
                CompanyId = companyId,
                AddedBy = addedBy,
                BranchId = branchId
            };

            ItemController itemController = new ItemController();
            var itemsResult = await itemController.ActiveItems(itemRequest);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemsResult);
            var items = oClsResponse.Data != null && oClsResponse.Data.Items != null ? oClsResponse.Data.Items : new List<ClsItemVm>();

            var normalizedItems = items.Select(item => new
            {
                ItemId = item.ItemId,
                ItemDetailsId = item.ItemDetailsId,
                ItemName = item.ItemName,
                VariationName = item.VariationName,
                SKU = item.SKU
            }).ToList();

            return Json(new
            {
                oClsResponse.Status,
                oClsResponse.Message,
                Items = normalizedItems
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Edit(long CatalogueId)
        {
            ClsCatalogueVm obj = new ClsCatalogueVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            ClsCatalogue objCat = new ClsCatalogue();
            objCat.CatalogueId = CatalogueId;
            objCat.CompanyId = obj.CompanyId;

            WebApi.CatalogueController catalogueController = new WebApi.CatalogueController();
            var catalogueResult = await catalogueController.Catalogue(objCat);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);

            ClsSalesVm objSales = new ClsSalesVm();
            objSales.CountryId = 2;
            objSales.AddedBy = obj.AddedBy;
            objSales.CompanyId = obj.CompanyId;

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = objSales.AddedBy, CompanyId = objSales.CompanyId, CountryId = objSales.CountryId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            SellingPriceGroupController sellingPriceGroupController = new SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = objSales.AddedBy, CompanyId = objSales.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = objSales.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ViewBag.Catalogue = oClsResponse.Data.Catalogue;
            long selectedBranchId = 0;
            var catalogueData = oClsResponse.Data.Catalogue;
            if (catalogueData != null)
            {
                var branchIdProperty = catalogueData.GetType().GetProperty("BranchId");
                if (branchIdProperty != null)
                {
                    var branchIdValue = branchIdProperty.GetValue(catalogueData, null);
                    if (branchIdValue != null)
                    {
                        long.TryParse(Convert.ToString(branchIdValue), out selectedBranchId);
                    }
                }

            }
            var branchList = oClsResponse2.Data.Branchs ?? new List<ClsBranchVm>();
            if (selectedBranchId == 0 && branchList.Any())
            {
                selectedBranchId = branchList.First().BranchId;
            }
            ViewBag.SelectedBranchId = selectedBranchId;
            ViewBag.Branchs = branchList;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;
            ViewBag.ItemSetting = oClsResponse14.Data.ItemSetting;

            CategoryController categoryController = new CategoryController();
            ClsCategoryVm categoryRequest = new ClsCategoryVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var categoryResult = await categoryController.ActiveCategorys(categoryRequest);
            ClsResponse oClsResponseCategory = await oCommonController.ExtractResponseFromActionResult(categoryResult);

            BrandController brandController = new BrandController();
            ClsBrandVm brandRequest = new ClsBrandVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy
            };
            var brandResult = await brandController.ActiveBrands(brandRequest);
            ClsResponse oClsResponseBrand = await oCommonController.ExtractResponseFromActionResult(brandResult);

            ItemController itemController = new ItemController();
            ClsItemVm itemRequest = new ClsItemVm
            {
                CompanyId = obj.CompanyId,
                AddedBy = obj.AddedBy,
                BranchId = selectedBranchId
            };
            var itemsResult = await itemController.ActiveItems(itemRequest);
            ClsResponse oClsResponseItems = await oCommonController.ExtractResponseFromActionResult(itemsResult);

            ViewBag.Categories = oClsResponseCategory.Data.Categories ?? new List<ClsCategoryVm>();
            ViewBag.Brands = oClsResponseBrand.Data.Brands ?? new List<ClsBrandVm>();
            ViewBag.Items = oClsResponseItems.Data.Items ?? new List<ClsItemVm>();

            var selectedItemPairs = new List<string>();
            var selectedCategoryIds = new List<long>();
            var selectedBrandIds = new List<long>();

            if (catalogueData != null)
            {
                var itemIds = new List<long>();
                var itemDetailIds = new List<long>();

                if (catalogueData.ItemIds != null)
                {
                    foreach (var value in (IEnumerable)catalogueData.ItemIds)
                    {
                        if (value != null)
                        {
                            itemIds.Add(Convert.ToInt64(value));
                        }
                    }
                }

                if (catalogueData.ItemDetailIds != null)
                {
                    foreach (var value in (IEnumerable)catalogueData.ItemDetailIds)
                    {
                        if (value != null)
                        {
                            itemDetailIds.Add(Convert.ToInt64(value));
                        }
                    }
                }

                for (int idx = 0; idx < Math.Min(itemIds.Count, itemDetailIds.Count); idx++)
                {
                    selectedItemPairs.Add(itemIds[idx] + "|" + itemDetailIds[idx]);
                }

                if (catalogueData.CategoryIds != null)
                {
                    foreach (var value in (IEnumerable)catalogueData.CategoryIds)
                    {
                        if (value != null)
                        {
                            selectedCategoryIds.Add(Convert.ToInt64(value));
                        }
                    }
                }

                if (catalogueData.BrandIds != null)
                {
                    foreach (var value in (IEnumerable)catalogueData.BrandIds)
                    {
                        if (value != null)
                        {
                            selectedBrandIds.Add(Convert.ToInt64(value));
                        }
                    }
                }
            }

            ViewBag.SelectedCatalogueItemPairs = selectedItemPairs;
            ViewBag.SelectedCategoryIds = selectedCategoryIds;
            ViewBag.SelectedBrandIds = selectedBrandIds;
            return View();
        }

        public async Task<ActionResult> CatalogueInsert(ClsCatalogueVm obj)
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
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }
            WebApi.CatalogueController catalogueController = new WebApi.CatalogueController();
            var catalogueResult = await catalogueController.InsertCatalogue(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> CatalogueUpdate(ClsCatalogueVm obj)
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
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CatalogueController catalogueController = new WebApi.CatalogueController();
            var catalogueResult = await catalogueController.UpdateCatalogue(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> CatalogueActiveInactive(ClsCatalogueVm obj)
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
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CatalogueController catalogueController = new WebApi.CatalogueController();
            var catalogueResult = await catalogueController.CatalogueActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> CatalogueDelete(ClsCatalogueVm obj)
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
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
            }

            WebApi.CatalogueController catalogueController = new WebApi.CatalogueController();
            var catalogueResult = await catalogueController.CatalogueDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> CatalogueView(long CatalogueId)
        {
            ClsCatalogueVm obj = new ClsCatalogueVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            }
            ClsCatalogue objCat = new ClsCatalogue();
            objCat.CatalogueId = CatalogueId;
            objCat.CompanyId = obj.CompanyId;

            WebApi.CatalogueController catalogueController = new WebApi.CatalogueController();
            var catalogueResult = await catalogueController.Catalogue(objCat);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);

            ClsSalesVm objSales = new ClsSalesVm();
            objSales.CountryId = 2;
            objSales.AddedBy = obj.AddedBy;
            objSales.CompanyId = obj.CompanyId;

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = objSales.AddedBy, CompanyId = objSales.CompanyId, CountryId = objSales.CountryId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            SellingPriceGroupController sellingPriceGroupController = new SellingPriceGroupController();
            ClsSellingPriceGroupVm sellingPriceGroupObj = new ClsSellingPriceGroupVm { AddedBy = objSales.AddedBy, CompanyId = objSales.CompanyId };
            var sellingPriceGroupResult = await sellingPriceGroupController.ActiveSellingPriceGroups(sellingPriceGroupObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(sellingPriceGroupResult);

            ViewBag.Catalogue = oClsResponse.Data.Catalogue;
            long selectedBranchId = 0;
            var catalogueData = oClsResponse.Data.Catalogue;
            if (catalogueData != null)
            {
                var branchIdProperty = catalogueData.GetType().GetProperty("BranchId");
                if (branchIdProperty != null)
                {
                    var branchIdValue = branchIdProperty.GetValue(catalogueData, null);
                    if (branchIdValue != null)
                    {
                        long.TryParse(Convert.ToString(branchIdValue), out selectedBranchId);
                    }
                }

            }
            ViewBag.SelectedBranchId = selectedBranchId;
            ViewBag.Branchs = oClsResponse2.Data.Branchs;
            ViewBag.SellingPriceGroups = oClsResponse9.Data.SellingPriceGroups;

            return PartialView("PartialCatalogueView");
        }
    }
}