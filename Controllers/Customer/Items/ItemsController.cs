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
using System.Text;
using System.IO;
using ClosedXML.Excel;

namespace EquiBillBook.Controllers
{
    [AuthorizationPrivilegeFilter]
    public class ItemsController : Controller
    {
        // GET: Items

        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        #region items
        public async Task<ActionResult> Items(long? BranchId)
        {
            ClsItemVm obj = new ClsItemVm();
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
                //obj.Title = "Items";
                if (BranchId != null)
                {
                    obj.BranchId = Convert.ToInt64(BranchId);
                    ViewBag.BranchId = obj.BranchId;
                }
            }
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.AllItems(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            UnitController unitController = new UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var unitResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(unitResult);

            BrandController brandController = new BrandController();
            ClsBrandVm brandObj = new ClsBrandVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var brandResult = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(brandResult);

            CategoryController categoryController = new CategoryController();
            ClsCategoryVm categoryObj = new ClsCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var categoryResult = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse23 = await oCommonController.ExtractResponseFromActionResult(categoryResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Items = oClsResponse.Data.Items;
            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.Units = oClsResponse21.Data.Units;
            ViewBag.Brands = oClsResponse22.Data.Brands;
            ViewBag.Categories = oClsResponse23.Data.Categories;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.DuplicateItemPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "duplicate item").FirstOrDefault();
            ViewBag.OpeningStockPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "add/edit opening stock").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.StockDetailsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock details report").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;
            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            ViewBag.SubSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;
            return View();
        }
        public async Task<ActionResult> ItemsFetch(ClsItemVm obj)
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
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.AllItems(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            UnitController unitController = new UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var unitResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(unitResult);

            BrandController brandController = new BrandController();
            ClsBrandVm brandObj = new ClsBrandVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var brandResult = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(brandResult);

            CategoryController categoryController = new CategoryController();
            ClsCategoryVm categoryObj = new ClsCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var categoryResult = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse23 = await oCommonController.ExtractResponseFromActionResult(categoryResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ViewBag.Items = oClsResponse.Data.Items;
            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;
            ViewBag.ActiveCount = oClsResponse.Data.ActiveCount;
            ViewBag.InactiveCount = oClsResponse.Data.InactiveCount;

            ViewBag.Units = oClsResponse21.Data.Units;
            ViewBag.Brands = oClsResponse22.Data.Brands;
            ViewBag.Categories = oClsResponse23.Data.Categories;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;

            ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            ViewBag.CurrentPageIndex = obj.PageIndex;
            ViewBag.PageSize = oClsResponse.Data.PageSize;
            ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "items").FirstOrDefault();
            ViewBag.DuplicateItemPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "duplicate item").FirstOrDefault();
            ViewBag.OpeningStockPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "add/edit opening stock").FirstOrDefault();
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            ViewBag.StockDetailsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock details report").FirstOrDefault();

            return PartialView("PartialItem");
        }
        public async Task<ActionResult> ItemsView(long ItemId)
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ItemId = ItemId;
            }
            //obj.Type = "item";
            obj.TaxExemptionType = "item";

            ClsItem itemObj = new ClsItem { ItemId = obj.ItemId, CompanyId = obj.CompanyId };
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.Item(itemObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            if (oClsResponse.Data.Item.ItemType == "Product")
            {
                obj.ItemCodeType = "hsn";
            }
            else
            {
                obj.ItemCodeType = "sac";
            }

            ItemCodeController itemCodeController = new ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { ItemCodeType = obj.ItemCodeType, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            TaxExemptionController taxExemptionController = new TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { TaxExemptionType = obj.TaxExemptionType, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            SaltController saltController = new SaltController();
            ClsSaltVm saltObj = new ClsSaltVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saltResult = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(saltResult);

            ViewBag.Item = oClsResponse.Data.Item;
            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.SecondaryUnits = oClsResponse.Data.SecondaryUnits;
            ViewBag.TertiaryUnits = oClsResponse.Data.TertiaryUnits;
            ViewBag.QuaternaryUnits = oClsResponse.Data.QuaternaryUnits;
            ViewBag.Brands = oClsResponse.Data.Brands;
            ViewBag.Categories = oClsResponse.Data.Categories;
            ViewBag.SubCategories = oClsResponse.Data.SubCategories;
            ViewBag.SubSubCategories = oClsResponse.Data.SubSubCategories;
            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.Variations = oClsResponse.Data.Variations;
            ViewBag.Warrantys = oClsResponse.Data.Warrantys;
            ViewBag.ItemCodes = oClsResponse.Data.ItemCodes;
            ViewBag.ItemSetting = oClsResponse6.Data.ItemSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.Salts = oClsResponse63.Data.Salts;

            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            ViewBag.TertiaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();
            ViewBag.QuaternaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "quaternary units").FirstOrDefault();
            ViewBag.BrandPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "brand").FirstOrDefault();
            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();
            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            ViewBag.SubSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();
            ViewBag.WarrantiesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "warranties").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.VariationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "variation").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return PartialView("PartialItemView");
        }
        public async Task<ActionResult> ItemsAdd()
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
            //obj.Type = "item";
            obj.ItemCodeType = "hsn";
            obj.TaxExemptionType = "item";

            UnitController unitController = new UnitController();
            ClsUnitVm unitObj = new ClsUnitVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var unitResult = await unitController.ActiveUnits(unitObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(unitResult);

            BrandController brandController = new BrandController();
            ClsBrandVm brandObj = new ClsBrandVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var brandResult = await brandController.ActiveBrands(brandObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(brandResult);

            CategoryController categoryController = new CategoryController();
            ClsCategoryVm categoryObj = new ClsCategoryVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var categoryResult = await categoryController.ActiveCategorys(categoryObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(categoryResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            WarrantyController warrantyController = new WarrantyController();
            ClsWarrantyVm warrantyObj = new ClsWarrantyVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var warrantyResult = await warrantyController.ActiveWarrantys(warrantyObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(warrantyResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            ItemCodeController itemCodeController = new ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { ItemCodeType = obj.ItemCodeType, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            TaxExemptionController taxExemptionController = new TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { TaxExemptionType = obj.TaxExemptionType, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            SaltController saltController = new SaltController();
            ClsSaltVm saltObj = new ClsSaltVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saltResult = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(saltResult);

            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.Brands = oClsResponse1.Data.Brands;
            ViewBag.Categories = oClsResponse2.Data.Categories;
            ViewBag.Branchs = oClsResponse3.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.Warrantys = oClsResponse5.Data.Warrantys;
            ViewBag.ItemSetting = oClsResponse6.Data.ItemSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.Salts = oClsResponse63.Data.Salts;

            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            ViewBag.TertiaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();
            ViewBag.QuaternaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "quaternary units").FirstOrDefault();
            ViewBag.BrandPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "brand").FirstOrDefault();
            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();
            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            ViewBag.SubSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();
            ViewBag.WarrantiesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "warranties").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.VariationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "variation").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();
            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
            ViewBag.OpeningStockPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "add/edit opening stock").FirstOrDefault();
            return View();
        }
        public async Task<ActionResult> ItemsEdit(long ItemId)
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ItemId = ItemId;
            }
            //obj.Type = "item";
            obj.TaxExemptionType = "item";

            ClsItem itemObj = new ClsItem { ItemId = obj.ItemId, CompanyId = obj.CompanyId };
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.Item(itemObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            if(oClsResponse.Data.Item.ItemType == "Product")
            {
                obj.ItemCodeType = "hsn";
            }
            else
            {
                obj.ItemCodeType = "sac";
            }

            ItemCodeController itemCodeController = new ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { ItemCodeType = obj.ItemCodeType, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            TaxExemptionController taxExemptionController = new TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { TaxExemptionType = obj.TaxExemptionType, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            SaltController saltController = new SaltController();
            ClsSaltVm saltObj = new ClsSaltVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saltResult = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(saltResult);

            ViewBag.Item = oClsResponse.Data.Item;
            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.SecondaryUnits = oClsResponse.Data.SecondaryUnits;
            ViewBag.TertiaryUnits = oClsResponse.Data.TertiaryUnits;
            ViewBag.QuaternaryUnits = oClsResponse.Data.QuaternaryUnits;
            ViewBag.Brands = oClsResponse.Data.Brands;
            ViewBag.Categories = oClsResponse.Data.Categories;
            ViewBag.SubCategories = oClsResponse.Data.SubCategories;
            ViewBag.SubSubCategories = oClsResponse.Data.SubSubCategories;
            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.Variations = oClsResponse.Data.Variations;
            ViewBag.Warrantys = oClsResponse.Data.Warrantys;
            ViewBag.ItemCodes = oClsResponse.Data.ItemCodes;
            ViewBag.ItemSetting = oClsResponse6.Data.ItemSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.Salts = oClsResponse63.Data.Salts;

            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            ViewBag.TertiaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();
            ViewBag.QuaternaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "quaternary units").FirstOrDefault();
            ViewBag.BrandPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "brand").FirstOrDefault();
            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();
            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            ViewBag.SubSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();
            ViewBag.WarrantiesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "warranties").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.VariationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "variation").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return View();
        }

        public async Task<ActionResult> LoadModals(long ItemId)
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ItemId = ItemId;
            }
            //obj.Type = "item";
            obj.TaxExemptionType = "item";

            ClsItem itemObj = new ClsItem { ItemId = obj.ItemId, CompanyId = obj.CompanyId };
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.Item(itemObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            if (oClsResponse.Data.Item.ItemType == "Product")
            {
                obj.ItemCodeType = "hsn";
            }
            else
            {
                obj.ItemCodeType = "sac";
            }

            ItemCodeController itemCodeController = new ItemCodeController();
            ClsItemCodeVm itemCodeObj = new ClsItemCodeVm { ItemCodeType = obj.ItemCodeType, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var itemCodeResult = await itemCodeController.ActiveItemCodes(itemCodeObj);
            ClsResponse oClsResponse52 = await oCommonController.ExtractResponseFromActionResult(itemCodeResult);

            TaxExemptionController taxExemptionController = new TaxExemptionController();
            ClsTaxExemptionVm taxExemptionObj = new ClsTaxExemptionVm { TaxExemptionType = obj.TaxExemptionType, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var taxExemptionResult = await taxExemptionController.ActiveTaxExemptions(taxExemptionObj);
            ClsResponse oClsResponse53 = await oCommonController.ExtractResponseFromActionResult(taxExemptionResult);

            SaltController saltController = new SaltController();
            ClsSaltVm saltObj = new ClsSaltVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var saltResult = await saltController.ActiveSalts(saltObj);
            ClsResponse oClsResponse63 = await oCommonController.ExtractResponseFromActionResult(saltResult);

            ViewBag.Item = oClsResponse.Data.Item;
            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.SecondaryUnits = oClsResponse.Data.SecondaryUnits;
            ViewBag.TertiaryUnits = oClsResponse.Data.TertiaryUnits;
            ViewBag.QuaternaryUnits = oClsResponse.Data.QuaternaryUnits;
            ViewBag.Brands = oClsResponse.Data.Brands;
            ViewBag.Categories = oClsResponse.Data.Categories;
            ViewBag.SubCategories = oClsResponse.Data.SubCategories;
            ViewBag.SubSubCategories = oClsResponse.Data.SubSubCategories;
            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.Variations = oClsResponse.Data.Variations;
            ViewBag.Warrantys = oClsResponse.Data.Warrantys;
            ViewBag.ItemCodes = oClsResponse.Data.ItemCodes;
            ViewBag.ItemSetting = oClsResponse6.Data.ItemSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.ItemCodes = oClsResponse52.Data.ItemCodes;
            ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;
            ViewBag.Salts = oClsResponse63.Data.Salts;

            ViewBag.ItemCodePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "item code").FirstOrDefault();
            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            ViewBag.TertiaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();
            ViewBag.QuaternaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "quaternary units").FirstOrDefault();
            ViewBag.BrandPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "brand").FirstOrDefault();
            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();
            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            ViewBag.SubSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();
            ViewBag.WarrantiesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "warranties").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.VariationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "variation").FirstOrDefault();
            ViewBag.TaxExemptionPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax exemptions").FirstOrDefault();
            ViewBag.SaltPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "salt").FirstOrDefault();

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;

            return PartialView("Components/_Modals");
        }
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> ItemsInsert(ClsItemVm obj)
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
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.InsertItem(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemsUpdate(ClsItemVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.UpdateItem(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemsActiveInactive(ClsItemVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.ItemActiveInactive(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemsDelete(ClsItemVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.ItemDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemAutocomplete(ClsItemVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.ItemAutocomplete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> SearchItems(ClsItemVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.SearchItems(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> SearchItemsWithoutBranch(ClsItemVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.SearchItemsWithoutBranch(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemDetailsDelete(ClsItemDetailsVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.itemDetailsDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemsDuplicate(long ItemId)
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.ItemId = ItemId;
            }
            ClsItem itemObj = new ClsItem { ItemId = obj.ItemId, CompanyId = obj.CompanyId };
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.Item(itemObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse36 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxResult = await taxController.ActiveTaxs(taxObj);
            ClsResponse oClsResponse38 = await oCommonController.ExtractResponseFromActionResult(taxResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            ViewBag.Item = oClsResponse.Data.Item;
            ViewBag.Units = oClsResponse.Data.Units;
            ViewBag.SecondaryUnits = oClsResponse.Data.SecondaryUnits;
            ViewBag.TertiaryUnits = oClsResponse.Data.TertiaryUnits;
            ViewBag.QuaternaryUnits = oClsResponse.Data.QuaternaryUnits;
            ViewBag.Brands = oClsResponse.Data.Brands;
            ViewBag.Categories = oClsResponse.Data.Categories;
            ViewBag.SubCategories = oClsResponse.Data.SubCategories;
            ViewBag.SubSubCategories = oClsResponse.Data.SubSubCategories;
            ViewBag.Branchs = oClsResponse.Data.Branchs;
            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.Variations = oClsResponse.Data.Variations;
            ViewBag.Warrantys = oClsResponse.Data.Warrantys;

            ViewBag.ItemSetting = oClsResponse6.Data.ItemSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;

            ViewBag.TaxsForGroup = oClsResponse38.Data.Taxs;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;

            ViewBag.UnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "units").FirstOrDefault();
            ViewBag.SecondaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "secondary units").FirstOrDefault();
            ViewBag.TertiaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tertiary units").FirstOrDefault();
            ViewBag.QuaternaryUnitsPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "quaternary units").FirstOrDefault();
            ViewBag.BrandPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "brand").FirstOrDefault();
            ViewBag.CategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "categories").FirstOrDefault();
            ViewBag.SubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub categories").FirstOrDefault();
            ViewBag.SubSubCategoriesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sub sub categories").FirstOrDefault();
            ViewBag.WarrantiesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "warranties").FirstOrDefault();
            ViewBag.TaxListPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax list").FirstOrDefault();
            ViewBag.TaxGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax group").FirstOrDefault();
            ViewBag.VariationPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "variation").FirstOrDefault();
            ViewBag.IsAccountsAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;
            ViewBag.IsPurchaseAddon = oClsResponse36.Data.PlanAddons.Where(a => a.Type == "Purchase").Count() == 0 ? false : true;


            return View();
        }
        public async Task<ActionResult> ItemImport()
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }
            //obj.Type = "Item";
            obj.TaxExemptionType = "item";

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            TaxController taxController = new TaxController();
            ClsTaxVm taxObj = new ClsTaxVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxAllResult = await taxController.ActiveAllTaxs(taxObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(taxAllResult);

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserId = obj.UserId, Type = "Item" };
            var otherSoftwareImportResult = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            AccountController accountController = new AccountController();
            ClsAccount accountObj = new ClsAccount { CompanyId = obj.CompanyId };
            var accountResult = await accountController.ActiveAccountsDropdown(accountObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(accountResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var planAddonsResult = await menuController.PlanAddons(menuObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            //var res53 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TaxExemption/ActiveTaxExemptions", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse53 = serializer.Deserialize<ClsResponse>(res53);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.Taxs = oClsResponse4.Data.Taxs;
            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;
            ViewBag.ItemSetting = oClsResponse6.Data.ItemSetting;
            ViewBag.AccountSubTypes = oClsResponse11.Data.AccountSubTypes;
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            //ViewBag.TaxExemptions = oClsResponse53.Data.TaxExemptions;

            ViewBag.IsAccountsAddon = oClsResponse21.Data.PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            ViewBag.ShowBranchColumn = ViewBag.Branchs != null && ViewBag.Branchs.Count > 1;

            return View();
        }
        public async Task<ActionResult> ImportItem(ClsItemVm obj)
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
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.ImportItem(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OtherSoftwareImportFetch(ClsItemVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }

            obj.PageSize = 10000000;
            obj.Type = "Item";

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserId = obj.UserId, Type = obj.Type, PageSize = obj.PageSize };
            var otherSoftwareImportResult = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);

            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;

            return PartialView("PartialOtherSoftwareImport");
        }
        public async Task<ActionResult> OtherSoftwareImportInsert(ClsOtherSoftwareImportVm obj)
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
            obj.Type = "Item";

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            var otherSoftwareImportResult = await otherSoftwareImportController.InsertOtherSoftwareImport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OtherSoftwareImportDelete(ClsOtherSoftwareImportVm obj)
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

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            var otherSoftwareImportResult = await otherSoftwareImportController.OtherSoftwareImportDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OpeningStockImport()
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Opening Stock";

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserId = obj.UserId, Type = obj.Type };
            var otherSoftwareImportResult = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            ViewBag.Branchs = oClsResponse1.Data.Branchs;
            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;
            ViewBag.ItemSetting = oClsResponse8.Data.ItemSetting;

            // Calculate column visibility flags
            ViewBag.ShowBranchColumn = ViewBag.Branchs != null && ViewBag.Branchs.Count > 1;

            return View();
        }

        public async Task<ActionResult> DownloadOpeningStockSample()
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Opening Stock";

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            // Fetch Item Settings to decide columns dynamically
            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsItemSettings = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            // Check if user has more than 1 branch
            bool showBranchColumn = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 1;
            // Item settings flags
            bool enableSecondaryUnit = oClsItemSettings.Data.ItemSetting.EnableSecondaryUnit == true;
            bool enableLotNo = oClsItemSettings.Data.ItemSetting.EnableLotNo == true;
            int expiryType = oClsItemSettings.Data.ItemSetting.ExpiryType;
            bool enableMrp = oClsItemSettings.Data.ItemSetting.EnableMrp == true;

            // Generate CSV content
            string csvContent = GenerateOpeningStockCsvContent(
                showBranchColumn,
                oClsResponse1.Data.Branchs,
                enableSecondaryUnit,
                enableLotNo,
                expiryType,
                enableMrp
            );

            // Return CSV file
            byte[] csvBytes = Encoding.UTF8.GetBytes(csvContent);
            return File(csvBytes, "text/csv", "OpeningStock_Sample.csv");
        }

        private string GenerateOpeningStockCsvContent(bool showBranchColumn, dynamic branches, bool enableSecondaryUnit, bool enableLotNo, int expiryType, bool enableMrp)
        {
            StringBuilder csv = new StringBuilder();

            // Add headers
            List<string> headers = new List<string>();
            headers.Add("SKU");
            if (showBranchColumn)
            {
                headers.Add("BranchName");
            }
            if (enableSecondaryUnit)
            {
                headers.Add("UnitType");
            }
            headers.Add("Quantity");
            headers.Add("UnitCost");
            if (enableLotNo)
            {
                headers.Add("LotNo");
            }
            if (enableLotNo && expiryType == 2)
            {
                headers.Add("ManufacturingDate");
            }
            if (enableLotNo)
            {
                headers.Add("ExpiryDate");
            }
            headers.Add("Date");
            headers.Add("Notes");
            headers.Add("SalesExcTax");
            if (enableMrp)
            {
                headers.Add("Mrp");
            }

            csv.AppendLine(string.Join(",", headers));

            // Add sample data rows
            List<string> sampleRow1 = new List<string>();
            sampleRow1.Add("SKU001");
            if (showBranchColumn)
            {
                sampleRow1.Add(branches != null && branches.Count > 0 ? branches[0].Branch : "");
            }
            if (enableSecondaryUnit)
            {
                sampleRow1.Add("Primary Unit");
            }
            sampleRow1.Add("100");
            sampleRow1.Add("50.00");
            if (enableLotNo)
            {
                sampleRow1.Add("LOT001");
            }
            if (enableLotNo && expiryType == 2)
            {
                sampleRow1.Add("2020-01-01");
            }
            if (enableLotNo)
            {
                sampleRow1.Add("2025-01-01");
            }
            sampleRow1.Add("2020-04-21");
            sampleRow1.Add("Sample opening stock");
            sampleRow1.Add("75.00");
            if (enableMrp)
            {
                sampleRow1.Add("100.00");
            }

            List<string> sampleRow2 = new List<string>();
            sampleRow2.Add("SKU002");
            if (showBranchColumn)
            {
                sampleRow2.Add("");
            }
            if (enableSecondaryUnit)
            {
                sampleRow2.Add("Secondary Unit");
            }
            sampleRow2.Add("50");
            sampleRow2.Add("25.00");
            if (enableLotNo)
            {
                sampleRow2.Add(""); // LotNo
            }
            if (enableLotNo && expiryType == 2)
            {
                sampleRow2.Add(""); // ManufacturingDate
            }
            if (enableLotNo)
            {
                sampleRow2.Add(""); // ExpiryDate
            }
            sampleRow2.Add("2020-04-21");
            sampleRow2.Add("Another sample");
            sampleRow2.Add(""); // SalesExcTax
            if (enableMrp)
            {
                sampleRow2.Add(""); // Mrp
            }

            csv.AppendLine(string.Join(",", sampleRow1));
            csv.AppendLine(string.Join(",", sampleRow2));

            return csv.ToString();
        }

        public async Task<ActionResult> DownloadOpeningStockSampleXlsx()
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Opening Stock";

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            // Fetch Item Settings to decide columns dynamically
            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsItemSettings = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            // Check if user has more than 1 branch
            bool showBranchColumn = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 1;
            // Item settings flags
            bool enableSecondaryUnit = oClsItemSettings.Data.ItemSetting.EnableSecondaryUnit == true;
            bool enableLotNo = oClsItemSettings.Data.ItemSetting.EnableLotNo == true;
            int expiryType = oClsItemSettings.Data.ItemSetting.ExpiryType;
            bool enableMrp = oClsItemSettings.Data.ItemSetting.EnableMrp == true;

            // Generate XLSX content
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Opening Stock Import Sample");

                // Add headers
                int col = 1;
                worksheet.Cell(1, col).Value = "SKU";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                
                if (showBranchColumn)
                {
                    worksheet.Cell(1, col).Value = "BranchName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enableSecondaryUnit)
                {
                    worksheet.Cell(1, col).Value = "UnitType";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                worksheet.Cell(1, col).Value = "Quantity";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "UnitCost";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                if (enableLotNo)
                {
                    worksheet.Cell(1, col).Value = "LotNo";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enableLotNo && expiryType == 2)
                {
                    worksheet.Cell(1, col).Value = "ManufacturingDate";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enableLotNo)
                {
                    worksheet.Cell(1, col).Value = "ExpiryDate";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                worksheet.Cell(1, col).Value = "Date";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "Notes";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "SalesExcTax";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                if (enableMrp)
                {
                    worksheet.Cell(1, col).Value = "Mrp";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }

                // Add sample data rows
                int row = 2;
                
                // Sample row 1
                col = 1;
                worksheet.Cell(row, col).Value = "SKU001";
                col++;
                if (showBranchColumn)
                {
                    worksheet.Cell(row, col).Value = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 0 ? oClsResponse1.Data.Branchs[0].Branch : "";
                    col++;
                }
                if (enableSecondaryUnit)
                {
                    worksheet.Cell(row, col).Value = "Primary Unit"; col++;
                }
                worksheet.Cell(row, col).Value = "100"; col++;
                worksheet.Cell(row, col).Value = "50.00"; col++;
                if (enableLotNo)
                {
                    worksheet.Cell(row, col).Value = "LOT001"; col++;
                }
                if (enableLotNo && expiryType == 2)
                {
                    worksheet.Cell(row, col).Value = "2020-01-01"; col++;
                }
                if (enableLotNo)
                {
                    worksheet.Cell(row, col).Value = "2025-01-01"; col++;
                }
                worksheet.Cell(row, col).Value = "2020-04-21"; col++;
                worksheet.Cell(row, col).Value = "Sample opening stock"; col++;
                worksheet.Cell(row, col).Value = "75.00"; col++;
                if (enableMrp)
                {
                    worksheet.Cell(row, col).Value = "100.00"; col++;
                }

                // Sample row 2
                row++;
                col = 1;
                worksheet.Cell(row, col).Value = "SKU002";
                col++;
                if (showBranchColumn)
                {
                    worksheet.Cell(row, col).Value = "";
                    col++;
                }
                if (enableSecondaryUnit)
                {
                    worksheet.Cell(row, col).Value = "Secondary Unit"; col++;
                }
                worksheet.Cell(row, col).Value = "50"; col++;
                worksheet.Cell(row, col).Value = "25.00"; col++;
                if (enableLotNo)
                {
                    worksheet.Cell(row, col).Value = ""; col++;// LotNo
                }
                if (enableLotNo && expiryType == 2)
                {
                    worksheet.Cell(row, col).Value = ""; col++;// ManufacturingDate
                }
                if (enableLotNo)
                {
                    worksheet.Cell(row, col).Value = ""; col++;// ExpiryDate
                }
                worksheet.Cell(row, col).Value = "2020-04-21"; col++;
                worksheet.Cell(row, col).Value = "Another sample"; col++;
                worksheet.Cell(row, col).Value = ""; col++;// SalesExcTax
                if (enableMrp)
                {
                    worksheet.Cell(row, col).Value = ""; col++;// Mrp
                }

                // Format Date columns to yyyy-mm-dd format
                // Recompute date column indices based on enabled columns
                int runningCol = 1; // SKU
                if (showBranchColumn) runningCol++;
                if (enableSecondaryUnit) runningCol++;
                runningCol++; // Quantity
                runningCol++; // UnitCost
                int? manufacturingDateColumnIndex = null;
                int? expiryDateColumnIndex = null;
                if (enableLotNo)
                {
                    runningCol++; // LotNo
                    if (expiryType == 2)
                    {
                        manufacturingDateColumnIndex = runningCol + 0;
                        runningCol++;
                    }
                    expiryDateColumnIndex = runningCol + 0;
                    runningCol++;
                }
                int dateColumnIndex = runningCol + 0; // Date
                
                if (manufacturingDateColumnIndex.HasValue)
                {
                    worksheet.Column(manufacturingDateColumnIndex.Value).Style.NumberFormat.Format = "yyyy-mm-dd";
                }
                if (expiryDateColumnIndex.HasValue)
                {
                    worksheet.Column(expiryDateColumnIndex.Value).Style.NumberFormat.Format = "yyyy-mm-dd";
                }
                worksheet.Column(dateColumnIndex).Style.NumberFormat.Format = "yyyy-mm-dd";

                // Auto-fit columns
                worksheet.ColumnsUsed().AdjustToContents();

                // Return XLSX file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OpeningStock_Sample.xlsx");
                }
            }
        }

        public async Task<ActionResult> DownloadItemSample()
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Item";

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            // Check if user has more than 1 branch
            bool showBranchColumn = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 1;
            bool isIndia = oClsResponse2.Data.BusinessSetting.CountryId == 2;
            
            // Item settings flags
            bool enableSecondaryUnit = oClsResponse3.Data.ItemSetting.EnableSecondaryUnit == true;
            bool enableTertiaryUnit = oClsResponse3.Data.ItemSetting.EnableTertiaryUnit == true;
            bool enableQuaternaryUnit = oClsResponse3.Data.ItemSetting.EnableQuaternaryUnit == true;
            bool enableBrands = oClsResponse3.Data.ItemSetting.EnableBrands == true;
            bool enableSubCategory = oClsResponse3.Data.ItemSetting.EnableSubCategory == true;
            bool enableSubSubCategory = oClsResponse3.Data.ItemSetting.EnableSubSubCategory == true;
            bool enableWarranty = oClsResponse3.Data.ItemSetting.EnableWarranty == true;
            bool enableSalt = oClsResponse3.Data.ItemSetting.EnableSalt == true;
            bool enableProductDescription = oClsResponse3.Data.ItemSetting.EnableProductDescription == true;
            bool enableRacks = oClsResponse3.Data.ItemSetting.EnableRacks == true;
            bool enableRow = oClsResponse3.Data.ItemSetting.EnableRow == true;
            bool enablePosition = oClsResponse3.Data.ItemSetting.EnablePosition == true;
            bool enableProductVariation = oClsResponse3.Data.ItemSetting.EnableProductVariation == true;

            // Generate CSV content
            string csvContent = GenerateItemCsvContent(showBranchColumn, isIndia, enableSecondaryUnit, enableTertiaryUnit, 
                enableQuaternaryUnit, enableBrands, enableSubCategory, enableSubSubCategory, enableWarranty, 
                enableSalt, enableProductDescription, enableRacks, enableRow, enablePosition, enableProductVariation,
                oClsResponse1.Data.Branchs);

            // Return CSV file
            byte[] csvBytes = Encoding.UTF8.GetBytes(csvContent);
            return File(csvBytes, "text/csv", "Item_Sample.csv");
        }

        private string GenerateItemCsvContent(bool showBranchColumn, bool isIndia, bool enableSecondaryUnit, bool enableTertiaryUnit,
            bool enableQuaternaryUnit, bool enableBrands, bool enableSubCategory, bool enableSubSubCategory, bool enableWarranty,
            bool enableSalt, bool enableProductDescription, bool enableRacks, bool enableRow, bool enablePosition, bool enableProductVariation,
            dynamic branches)
        {
            StringBuilder csv = new StringBuilder();

            // Add headers
            List<string> headers = new List<string>();
            headers.Add("ItemType");
            headers.Add("SkuCode");
            headers.Add("HsnSacCode");
            headers.Add("ItemName");
            headers.Add("UnitName");
            
            if (enableSecondaryUnit)
            {
                headers.Add("SecondaryUnitName");
                headers.Add("UToSValue");
            }
            if (enableTertiaryUnit)
            {
                headers.Add("TertiaryUnitName");
                headers.Add("SToTValue");
            }
            if (enableQuaternaryUnit)
            {
                headers.Add("QuaternaryUnitName");
                headers.Add("TToQValue");
            }
            if (enableBrands)
            {
                headers.Add("BrandName");
            }
            headers.Add("CategoryName");
            if (enableSubCategory)
            {
                headers.Add("SubCategoryName");
            }
            if (enableSubSubCategory)
            {
                headers.Add("SubSubCategoryName");
            }
            headers.Add("IsManageStock");
            headers.Add("AlertQuantity");
            headers.Add("ExpiryPeriod");
            headers.Add("ExpiryPeriodType");
            if (enableWarranty)
            {
                headers.Add("WarrantyName");
                headers.Add("WarrantyDuration");
                headers.Add("WarrantyDurationType");
            }
            headers.Add("EnableImei");
            if (enableSalt)
            {
                headers.Add("SaltName");
            }
            if (enableProductDescription)
            {
                headers.Add("Description");
            }
            if (showBranchColumn)
            {
                headers.Add("BranchNames");
            }
            if (enableRacks)
            {
                headers.Add("Rack");
            }
            if (enableRow)
            {
                headers.Add("Row");
            }
            if (enablePosition)
            {
                headers.Add("Position");
            }
            headers.Add("TaxPreference");
            headers.Add("TaxExemptionReason");
            headers.Add("IntraStateTaxName");
            headers.Add("InterStateTaxName");
            if (enableProductVariation)
            {
                headers.Add("ProductType");
                headers.Add("VariationGroupName");
                headers.Add("VariationName");
                headers.Add("VariationValues");
                headers.Add("VariationSKUs");
            }
            headers.Add("PurchasePrice");
            headers.Add("SellingPrice");
            headers.Add("MRP");

            csv.AppendLine(string.Join(",", headers));

            // Add sample data rows
            List<string> sampleRow1 = new List<string>();
            sampleRow1.Add("Product");
            sampleRow1.Add("SKU001");
            sampleRow1.Add("12345678");
            sampleRow1.Add("Sample Product 1");
            sampleRow1.Add("Piece");
            
            if (enableSecondaryUnit)
            {
                sampleRow1.Add("Box");
                sampleRow1.Add("10");
            }
            if (enableTertiaryUnit)
            {
                sampleRow1.Add("Carton");
                sampleRow1.Add("5");
            }
            if (enableQuaternaryUnit)
            {
                sampleRow1.Add("Pallet");
                sampleRow1.Add("2");
            }
            if (enableBrands)
            {
                sampleRow1.Add("Sample Brand");
            }
            sampleRow1.Add("Sample Category");
            if (enableSubCategory)
            {
                sampleRow1.Add("Sample Sub Category");
            }
            if (enableSubSubCategory)
            {
                sampleRow1.Add("Sample Sub Sub Category");
            }
            sampleRow1.Add("True");
            sampleRow1.Add("10");
            sampleRow1.Add("365");
            sampleRow1.Add("Days");
            if (enableWarranty)
            {
                sampleRow1.Add("Sample Warranty");
                sampleRow1.Add("12");
                sampleRow1.Add("Months");
            }
            sampleRow1.Add("False");
            if (enableSalt)
            {
                sampleRow1.Add("Sample Salt");
            }
            if (enableProductDescription)
            {
                sampleRow1.Add("Sample product description");
            }
            if (showBranchColumn)
            {
                sampleRow1.Add(branches != null && branches.Count > 0 ? branches[0].Branch : "");
            }
            if (enableRacks)
            {
                sampleRow1.Add("RACK1");
            }
            if (enableRow)
            {
                sampleRow1.Add("ROW1");
            }
            if (enablePosition)
            {
                sampleRow1.Add("POS1");
            }
            sampleRow1.Add("Taxable");
            sampleRow1.Add("");
            sampleRow1.Add("CGST + SGST");
            sampleRow1.Add("IGST");
            if (enableProductVariation)
            {
                sampleRow1.Add("Single");
                sampleRow1.Add("");
                sampleRow1.Add("");
                sampleRow1.Add("");
                sampleRow1.Add("");
            }
            sampleRow1.Add("100.00");
            sampleRow1.Add("150.00");
            sampleRow1.Add("200.00");

            List<string> sampleRow2 = new List<string>();
            sampleRow2.Add("Service");
            sampleRow2.Add("SKU002");
            sampleRow2.Add("87654321");
            sampleRow2.Add("Sample Service 1");
            sampleRow2.Add("");
            
            if (enableSecondaryUnit)
            {
                sampleRow2.Add("");
                sampleRow2.Add("");
            }
            if (enableTertiaryUnit)
            {
                sampleRow2.Add("");
                sampleRow2.Add("");
            }
            if (enableQuaternaryUnit)
            {
                sampleRow2.Add("");
                sampleRow2.Add("");
            }
            if (enableBrands)
            {
                sampleRow2.Add("");
            }
            sampleRow2.Add("Service Category");
            if (enableSubCategory)
            {
                sampleRow2.Add("");
            }
            if (enableSubSubCategory)
            {
                sampleRow2.Add("");
            }
            sampleRow2.Add("False");
            sampleRow2.Add("");
            sampleRow2.Add("");
            sampleRow2.Add("");
            if (enableWarranty)
            {
                sampleRow2.Add("");
                sampleRow2.Add("");
                sampleRow2.Add("");
            }
            sampleRow2.Add("False");
            if (enableSalt)
            {
                sampleRow2.Add("");
            }
            if (enableProductDescription)
            {
                sampleRow2.Add("Sample service description");
            }
            if (showBranchColumn)
            {
                sampleRow2.Add("");
            }
            if (enableRacks)
            {
                sampleRow2.Add("");
            }
            if (enableRow)
            {
                sampleRow2.Add("");
            }
            if (enablePosition)
            {
                sampleRow2.Add("");
            }
            sampleRow2.Add("Non-Taxable");
            sampleRow2.Add("Service");
            sampleRow2.Add("");
            sampleRow2.Add("");
            if (enableProductVariation)
            {
                sampleRow2.Add("Single");
                sampleRow2.Add("");
                sampleRow2.Add("");
                sampleRow2.Add("");
                sampleRow2.Add("");
            }
            sampleRow2.Add("50.00");
            sampleRow2.Add("75.00");
            sampleRow2.Add("");

            csv.AppendLine(string.Join(",", sampleRow1));
            csv.AppendLine(string.Join(",", sampleRow2));

            return csv.ToString();
        }

        public async Task<ActionResult> DownloadItemSampleXlsx()
        {
            ClsItemVm obj = new ClsItemVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }
            obj.Type = "Item";

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            ItemSettingsController itemSettingsController = new ItemSettingsController();
            ClsItemSettingsVm itemSettingsObj = new ClsItemSettingsVm { CompanyId = obj.CompanyId };
            var itemSettingsResult = await itemSettingsController.ItemSetting(itemSettingsObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(itemSettingsResult);

            // Check if user has more than 1 branch
            bool showBranchColumn = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 1;
            bool isIndia = oClsResponse2.Data.BusinessSetting.CountryId == 2;
            
            // Item settings flags
            bool enableSecondaryUnit = oClsResponse3.Data.ItemSetting.EnableSecondaryUnit == true;
            bool enableTertiaryUnit = oClsResponse3.Data.ItemSetting.EnableTertiaryUnit == true;
            bool enableQuaternaryUnit = oClsResponse3.Data.ItemSetting.EnableQuaternaryUnit == true;
            bool enableBrands = oClsResponse3.Data.ItemSetting.EnableBrands == true;
            bool enableSubCategory = oClsResponse3.Data.ItemSetting.EnableSubCategory == true;
            bool enableSubSubCategory = oClsResponse3.Data.ItemSetting.EnableSubSubCategory == true;
            bool enableWarranty = oClsResponse3.Data.ItemSetting.EnableWarranty == true;
            bool enableSalt = oClsResponse3.Data.ItemSetting.EnableSalt == true;
            bool enableProductDescription = oClsResponse3.Data.ItemSetting.EnableProductDescription == true;
            bool enableRacks = oClsResponse3.Data.ItemSetting.EnableRacks == true;
            bool enableRow = oClsResponse3.Data.ItemSetting.EnableRow == true;
            bool enablePosition = oClsResponse3.Data.ItemSetting.EnablePosition == true;
            bool enableProductVariation = oClsResponse3.Data.ItemSetting.EnableProductVariation == true;

            // Generate XLSX content
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Item Import Sample");

                // Add headers
                int col = 1;
                worksheet.Cell(1, col).Value = "ItemType";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "SkuCode";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "HsnSacCode";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "ItemName";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "UnitName";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                
                if (enableSecondaryUnit)
                {
                    worksheet.Cell(1, col).Value = "SecondaryUnitName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                    worksheet.Cell(1, col).Value = "UToSValue";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enableTertiaryUnit)
                {
                    worksheet.Cell(1, col).Value = "TertiaryUnitName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                    worksheet.Cell(1, col).Value = "SToTValue";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enableQuaternaryUnit)
                {
                    worksheet.Cell(1, col).Value = "QuaternaryUnitName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                    worksheet.Cell(1, col).Value = "TToQValue";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enableBrands)
                {
                    worksheet.Cell(1, col).Value = "BrandName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                worksheet.Cell(1, col).Value = "CategoryName";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                if (enableSubCategory)
                {
                    worksheet.Cell(1, col).Value = "SubCategoryName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enableSubSubCategory)
                {
                    worksheet.Cell(1, col).Value = "SubSubCategoryName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                worksheet.Cell(1, col).Value = "IsManageStock";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "AlertQuantity";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "ExpiryPeriod";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "ExpiryPeriodType";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                if (enableWarranty)
                {
                    worksheet.Cell(1, col).Value = "WarrantyName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                    worksheet.Cell(1, col).Value = "WarrantyDuration";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                    worksheet.Cell(1, col).Value = "WarrantyDurationType";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                worksheet.Cell(1, col).Value = "EnableImei";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                if (enableSalt)
                {
                    worksheet.Cell(1, col).Value = "SaltName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enableProductDescription)
                {
                    worksheet.Cell(1, col).Value = "Description";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (showBranchColumn)
                {
                    worksheet.Cell(1, col).Value = "BranchNames";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enableRacks)
                {
                    worksheet.Cell(1, col).Value = "Rack";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enableRow)
                {
                    worksheet.Cell(1, col).Value = "Row";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                if (enablePosition)
                {
                    worksheet.Cell(1, col).Value = "Position";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                worksheet.Cell(1, col).Value = "TaxPreference";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "TaxExemptionReason";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "IntraStateTaxName";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "InterStateTaxName";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                if (enableProductVariation)
                {
                    worksheet.Cell(1, col).Value = "ProductType";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                    worksheet.Cell(1, col).Value = "VariationGroupName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                    worksheet.Cell(1, col).Value = "VariationName";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                    worksheet.Cell(1, col).Value = "VariationValues";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                    worksheet.Cell(1, col).Value = "VariationSKUs";
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    col++;
                }
                worksheet.Cell(1, col).Value = "PurchasePrice";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "SellingPrice";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;
                worksheet.Cell(1, col).Value = "MRP";
                worksheet.Cell(1, col).Style.Font.Bold = true;
                col++;

                // Add sample data rows
                int row = 2;
                
                // Sample row 1
                col = 1;
                worksheet.Cell(row, col).Value = "Product"; col++;
                worksheet.Cell(row, col).Value = "SKU001"; col++;
                worksheet.Cell(row, col).Value = "12345678"; col++;
                worksheet.Cell(row, col).Value = "Sample Product 1"; col++;
                worksheet.Cell(row, col).Value = "Piece"; col++;
                
                if (enableSecondaryUnit)
                {
                    worksheet.Cell(row, col).Value = "Box"; col++;
                    worksheet.Cell(row, col).Value = "10"; col++;
                }
                if (enableTertiaryUnit)
                {
                    worksheet.Cell(row, col).Value = "Carton"; col++;
                    worksheet.Cell(row, col).Value = "5"; col++;
                }
                if (enableQuaternaryUnit)
                {
                    worksheet.Cell(row, col).Value = "Pallet"; col++;
                    worksheet.Cell(row, col).Value = "2"; col++;
                }
                if (enableBrands)
                {
                    worksheet.Cell(row, col).Value = "Sample Brand"; col++;
                }
                worksheet.Cell(row, col).Value = "Sample Category"; col++;
                if (enableSubCategory)
                {
                    worksheet.Cell(row, col).Value = "Sample Sub Category"; col++;
                }
                if (enableSubSubCategory)
                {
                    worksheet.Cell(row, col).Value = "Sample Sub Sub Category"; col++;
                }
                worksheet.Cell(row, col).Value = "True"; col++;
                worksheet.Cell(row, col).Value = "10"; col++;
                worksheet.Cell(row, col).Value = "365"; col++;
                worksheet.Cell(row, col).Value = "Days"; col++;
                if (enableWarranty)
                {
                    worksheet.Cell(row, col).Value = "Sample Warranty"; col++;
                    worksheet.Cell(row, col).Value = "12"; col++;
                    worksheet.Cell(row, col).Value = "Months"; col++;
                }
                worksheet.Cell(row, col).Value = "False"; col++;
                if (enableSalt)
                {
                    worksheet.Cell(row, col).Value = "Sample Salt"; col++;
                }
                if (enableProductDescription)
                {
                    worksheet.Cell(row, col).Value = "Sample product description"; col++;
                }
                if (showBranchColumn)
                {
                    worksheet.Cell(row, col).Value = oClsResponse1.Data.Branchs != null && oClsResponse1.Data.Branchs.Count > 0 ? oClsResponse1.Data.Branchs[0].Branch : ""; col++;
                }
                if (enableRacks)
                {
                    worksheet.Cell(row, col).Value = "RACK1"; col++;
                }
                if (enableRow)
                {
                    worksheet.Cell(row, col).Value = "ROW1"; col++;
                }
                if (enablePosition)
                {
                    worksheet.Cell(row, col).Value = "POS1"; col++;
                }
                worksheet.Cell(row, col).Value = "Taxable"; col++;
                worksheet.Cell(row, col).Value = ""; col++;
                worksheet.Cell(row, col).Value = "CGST + SGST"; col++;
                worksheet.Cell(row, col).Value = "IGST"; col++;
                if (enableProductVariation)
                {
                    worksheet.Cell(row, col).Value = "Single"; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                worksheet.Cell(row, col).Value = "100.00"; col++;
                worksheet.Cell(row, col).Value = "150.00"; col++;
                worksheet.Cell(row, col).Value = "200.00"; col++;

                // Sample row 2
                row++;
                col = 1;
                worksheet.Cell(row, col).Value = "Service"; col++;
                worksheet.Cell(row, col).Value = "SKU002"; col++;
                worksheet.Cell(row, col).Value = "87654321"; col++;
                worksheet.Cell(row, col).Value = "Sample Service 1"; col++;
                worksheet.Cell(row, col).Value = ""; col++;
                
                if (enableSecondaryUnit)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                if (enableTertiaryUnit)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                if (enableQuaternaryUnit)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                if (enableBrands)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                worksheet.Cell(row, col).Value = "Service Category"; col++;
                if (enableSubCategory)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                if (enableSubSubCategory)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                worksheet.Cell(row, col).Value = "False"; col++;
                worksheet.Cell(row, col).Value = ""; col++;
                worksheet.Cell(row, col).Value = ""; col++;
                worksheet.Cell(row, col).Value = ""; col++;
                if (enableWarranty)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                worksheet.Cell(row, col).Value = "False"; col++;
                if (enableSalt)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                if (enableProductDescription)
                {
                    worksheet.Cell(row, col).Value = "Sample service description"; col++;
                }
                if (showBranchColumn)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                if (enableRacks)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                if (enableRow)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                if (enablePosition)
                {
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                worksheet.Cell(row, col).Value = "Non-Taxable"; col++;
                worksheet.Cell(row, col).Value = "Service"; col++;
                worksheet.Cell(row, col).Value = ""; col++;
                worksheet.Cell(row, col).Value = ""; col++;
                if (enableProductVariation)
                {
                    worksheet.Cell(row, col).Value = "Single"; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                    worksheet.Cell(row, col).Value = ""; col++;
                }
                worksheet.Cell(row, col).Value = "50.00"; col++;
                worksheet.Cell(row, col).Value = "75.00"; col++;
                worksheet.Cell(row, col).Value = ""; col++;

                // Auto-fit columns
                worksheet.ColumnsUsed().AdjustToContents();

                // Return XLSX file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Item_Sample.xlsx");
                }
            }
        }

        public async Task<ActionResult> ImportOpeningStock(ClsOpeningStockVm obj)
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
            OpeningStockController openingStockController = new OpeningStockController();
            var openingStockResult = await openingStockController.ImportOpeningStock(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(openingStockResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OtherSoftwareImportFetch_OpeningStock(ClsItemVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserId = obj.CompanyId;
            }

            obj.PageSize = 10000000;
            obj.Type = "Opening Stock";

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            ClsOtherSoftwareImportVm otherSoftwareImportObj = new ClsOtherSoftwareImportVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserId = obj.UserId, Type = obj.Type, PageSize = obj.PageSize };
            var otherSoftwareImportResult = await otherSoftwareImportController.AllOtherSoftwareImports(otherSoftwareImportObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);

            ViewBag.OtherSoftwareImports = oClsResponse5.Data.OtherSoftwareImports;

            return PartialView("PartialOtherSoftwareImport_OpeningStock");
        }
        public async Task<ActionResult> OtherSoftwareImportInsert_OpeningStock(ClsOtherSoftwareImportVm obj)
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
            obj.Type = "Opening Stock";

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            var otherSoftwareImportResult = await otherSoftwareImportController.InsertOtherSoftwareImport(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OtherSoftwareImportDelete_OpeningStock(ClsOtherSoftwareImportVm obj)
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

            OtherSoftwareImportController otherSoftwareImportController = new OtherSoftwareImportController();
            var otherSoftwareImportResult = await otherSoftwareImportController.OtherSoftwareImportDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(otherSoftwareImportResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemCountByBatch(ClsItemVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.ItemCountByBatch(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }
        public async Task<ActionResult> AvailableBranches(ClsItemVm obj)
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

            ClsItem itemObj = new ClsItem { ItemId = obj.ItemId, CompanyId = obj.CompanyId };
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.AvailableBranches(itemObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);

            //ViewBag.Branchs = oClsResponse.Data.Branchs;
            //ViewBag.OpeningStockDetails = oClsResponse.Data.OpeningStockDetails;
            return Json(oClsResponse);
        }
        public async Task<ActionResult> OpeningStock(ClsOpeningStockVm obj)
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
            ClsOpeningStock openingStockObj = new ClsOpeningStock { ItemId = obj.ItemId, ItemDetailsId = obj.ItemDetailsId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId };
            OpeningStockController openingStockController = new OpeningStockController();
            var openingStockResult = await openingStockController.OpeningStock(openingStockObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(openingStockResult);

            return Json(oClsResponse);
        }
        public async Task<ActionResult> ItemDetailsForOpeningStock(ClsOpeningStockVm obj)
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

            ClsItemVm itemVmObj = new ClsItemVm { ItemId = obj.ItemId,ItemDetailsId = obj.ItemDetailsId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy, BranchId = obj.BranchId };
            OpeningStockController openingStockController = new OpeningStockController();
            var openingStockResult = await openingStockController.ItemDetailsForOpeningStock(itemVmObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(openingStockResult);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> OpeningStockInsert(ClsItemVm obj)
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

            OpeningStockController openingStockController = new OpeningStockController();
            //if(obj.OpeningStockId == 0)
            //{
            //    var openingStockResult = await openingStockController.InsertOpeningStock(obj);
            //}
            //else
            //{
            var openingStockResult = await openingStockController.UpdateOpeningStock(obj);
            //}
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(openingStockResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> OpeningStockDelete(ClsOpeningStockVm obj)
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

            OpeningStockController openingStockController = new OpeningStockController();
            var openingStockResult = await openingStockController.OpeningStockDelete(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(openingStockResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> AvailableSellingPriceGroups(ClsItemVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.AvailableSellingPriceGroups(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.ItemDetails = oClsResponse.Data.ItemDetails;
            ViewBag.SellingPriceGroupPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "selling price group").FirstOrDefault();
            return PartialView("PartialSellingPriceGroups");
        }

        public async Task<ActionResult> UpdateAvailableSellingPriceGroups(ClsItemVm obj)
        {
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
            }
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.UpdateAvailableSellingPriceGroups(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> CalculateExpiryDate(ClsOpeningStock obj)
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

            ClsOpeningStock openingStockObj = new ClsOpeningStock { ItemId = obj.ItemId, CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.CalculateExpiryDate(openingStockObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ItemMultipleUnits(ClsItemVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.ItemMultipleUnits(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }

        public async Task<ActionResult> ActiveItems(ClsItemVm obj)
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

            WebApi.ItemController itemController = new WebApi.ItemController();
            var itemResult = await itemController.ActiveItems(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(itemResult);
            return Json(oClsResponse);
        }     

        #endregion

        #region print label
        public async Task<ActionResult> PrintLabel()
        {
            ClsPurchaseVm obj = new ClsPurchaseVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                obj.UserType = "supplier";
            }
            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, UserType = obj.UserType };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Branchs = oClsResponse.Data.Branchs;
            return View();
        }

        public async Task<ActionResult> PrintLabelInsert(ClsPrintLabelVm obj)
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
            PrintLabelController printLabelController = new PrintLabelController();
            var printLabelResult = await printLabelController.InsertPrintLabel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(printLabelResult);
            return Json(oClsResponse);
        }

        [AllowAnonymous]
        public async Task<ActionResult> PrintLabelPreview(long PrintLabelId)
        {
            ClsPrintLabelVm obj = new ClsPrintLabelVm();
            obj.PrintLabelId = PrintLabelId;

            PrintLabelController printLabelController = new PrintLabelController();
            var printLabelResult = await printLabelController.PrintLabel(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(printLabelResult);

            ViewBag.PrintLabel = oClsResponse.Data.PrintLabel;
            ViewBag.CurrencySymbol = oClsResponse.Data.User.CurrencySymbol;
            return View();
        }
        #endregion

    }
}