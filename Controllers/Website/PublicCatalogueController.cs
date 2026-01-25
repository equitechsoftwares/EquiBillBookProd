using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2013.WebExtension;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Wordprocessing;
using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Data.Entity;

namespace EquiBillBook.Controllers.Website
{
    public class PublicCatalogueController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: /c/{slug}
        [HttpGet]
        [ActionName("View")]
        public async Task<ActionResult> ViewCatalogue(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("Index", "Home");
            }

            serializer.MaxJsonLength = 2147483644;

            // Fetch catalogue details
            CatalogueController catalogueController = new CatalogueController();
            var catalogueResult = await catalogueController.PublicCatalogue(slug);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);

            if (oClsResponse.Status == 0 || oClsResponse.Data == null)
            {
                return RedirectToAction("CatalogueNotFound");
            }          

            dynamic catalogue = oClsResponse.Data.Catalogue;
            ViewBag.Catalogue = catalogue;

            MenuController menuController = new MenuController();
            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = ToLong(GetDynamicValue(catalogue, "CompanyId")), CompanyId = ToLong(GetDynamicValue(catalogue, "CompanyId")) };
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "catalogue").FirstOrDefault();
            if(ViewBag.MenuPermission.IsView == false)
            {
                return RedirectToAction("CatalogueNotFound");
            }

            int defaultSortOrder = ToInt(GetDynamicValue(catalogue, "DefaultSortOrder"), 1);
            if (defaultSortOrder <= 0)
            {
                defaultSortOrder = 1;
            }
            ViewBag.DefaultSortOrder = defaultSortOrder;

            // SEO Meta Tags
            ViewBag.PageTitle = catalogue.MetaTitle ?? catalogue.CatalogueName;
            ViewBag.MetaDescription = catalogue.MetaDescription;
            ViewBag.CanonicalUrl = Request.Url.Scheme + "://" + Request.Url.Host + "/c/" + slug;

            // Check if password protected
            if (catalogue.IsPasswordProtected == true)
            {
                // Check session for password verification
                if (Session["CatalogueAuth_" + slug] == null ||
                    Session["CatalogueAuth_" + slug].ToString() != "verified")
                {
                    return RedirectToAction("PasswordProtected", new { slug });
                }
            }

            CategoryController categoryController = new CategoryController();
            ClsCategoryVm categoryObj = new ClsCategoryVm { CompanyId = ToLong(GetDynamicValue(catalogue, "CompanyId")) };
            var categoryResult = await categoryController.PublicCategorys(categoryObj);
            ClsResponse oClsResponse82 = await oCommonController.ExtractResponseFromActionResult(categoryResult);
            ViewBag.Categories = oClsResponse82.Data.Categories;

            BrandController brandController = new BrandController();
            ClsBrandVm brandObj = new ClsBrandVm { CompanyId = ToLong(GetDynamicValue(catalogue, "CompanyId")) };
            var brandResult = await brandController.PublicBrands(brandObj);
            ClsResponse oClsResponseBrands = await oCommonController.ExtractResponseFromActionResult(brandResult);
            ViewBag.Brands = oClsResponseBrands.Data.Brands;

            // Fetch Places of Supply (States) for India
            try
            {
                StateController stateController = new StateController();
                ClsStateVm stateObj = new ClsStateVm { CompanyId = ToLong(GetDynamicValue(catalogue, "CompanyId")) };
                var stateResult = await stateController.ActiveStates(stateObj);
                ClsResponse oClsResponseStates = await oCommonController.ExtractResponseFromActionResult(stateResult);
                if (oClsResponseStates.Status == 1 && oClsResponseStates.Data != null)
                {
                    ViewBag.PlacesOfSupply = oClsResponseStates.Data.States;
                }
            }
            catch
            {
                // ignore if states cannot be fetched
            }

            int itemsPerPage = 0;
            int.TryParse(Convert.ToString(catalogue.ItemsPerPage), out itemsPerPage);

            long branchId = ResolveBranchId(catalogue);

            long? placeOfSupplyId = null;
            try
            {
                var posCookie = Request.Cookies["publicCatalogueSelectedStateId"];
                if (posCookie != null)
                {
                    long parsedPos;
                    if (long.TryParse(Convert.ToString(posCookie.Value), out parsedPos) && parsedPos > 0)
                    {
                        placeOfSupplyId = parsedPos;
                    }
                }
            }
            catch
            {
                // ignore cookie parsing errors
            }

            ItemController itemController = new ItemController();
            ClsItemBranchMapVm itemObj = new ClsItemBranchMapVm
            {
                CompanyId = ToLong(GetDynamicValue(catalogue, "CompanyId")),
                PageIndex = 1,
                PageSize = itemsPerPage > 0 ? itemsPerPage : 0,
                SortOrder = defaultSortOrder,
                BranchId = branchId,
                PlaceOfSupplyId = placeOfSupplyId.HasValue ? placeOfSupplyId.Value : 0
            };
            var itemResult = await itemController.PublicItems(itemObj);
            ClsResponse oClsResponseItems = await oCommonController.ExtractResponseFromActionResult(itemResult);
            IEnumerable initialItems = null;
            int initialPageIndex = 1;
            int initialPageSize = 0;
            int initialTotalCount = 0;

            if (oClsResponseItems.Status == 1 && oClsResponseItems.Data != null)
            {
                ViewBag.ItemDetails = oClsResponseItems.Data.ItemDetails;
                ViewBag.TotalItems = oClsResponseItems.Data.TotalCount;
                initialItems = oClsResponseItems.Data.ItemDetails as IEnumerable ??
                                oClsResponseItems.Data.Items as IEnumerable;

                initialPageIndex = ToInt(GetDynamicValue(oClsResponseItems.Data, "PageIndex"), 1);
                initialPageSize = ToInt(GetDynamicValue(oClsResponseItems.Data, "PageSize"), 0);
                initialTotalCount = ToInt(GetDynamicValue(oClsResponseItems.Data, "TotalCount"), 0);

                ViewBag.MinPrice = oClsResponseItems.Data.MinPrice;
                ViewBag.MaxPrice = oClsResponseItems.Data.MaxPrice;
            }
            else
            {
                ViewBag.ItemDetails = new List<object>();
                ViewBag.TotalItems = 0;
                initialItems = new List<object>();
            }

            ViewBag.ProductGridModel = BuildProductGridModel(catalogue, initialItems, initialPageIndex, initialPageSize, initialTotalCount);

            if (catalogue != null)
            {
                try
                {
                    int catalogueTypeValue = Convert.ToInt32(catalogue.CatalogueType);
                    if (catalogueTypeValue == 3 && ViewBag.Categories != null)
                    {
                        var selectedCategoryIds = new List<long>();
                        var catalogueCategoryIds = catalogue.CategoryIds as IEnumerable;
                        if (catalogueCategoryIds != null)
                        {
                            foreach (var id in catalogueCategoryIds)
                            {
                                try
                                {
                                    var parsed = Convert.ToInt64(id);
                                    if (parsed > 0 && !selectedCategoryIds.Contains(parsed))
                                    {
                                        selectedCategoryIds.Add(parsed);
                                    }
                                }
                                catch
                                {
                                    // ignore parse errors
                                }
                            }
                        }

                        if (selectedCategoryIds.Count > 0)
                        {
                            var categoryList = new List<object>();
                            var sourceCategories = ViewBag.Categories as IEnumerable;
                            if (sourceCategories != null)
                            {
                                foreach (var category in sourceCategories)
                                {
                                    try
                                    {
                                        var categoryIdObject = category.GetType().GetProperty("CategoryId")?.GetValue(category, null);
                                        var categoryId = Convert.ToInt64(categoryIdObject);
                                        if (selectedCategoryIds.Contains(categoryId))
                                        {
                                            categoryList.Add(category);
                                        }
                                    }
                                    catch
                                    {
                                        // ignore invalid category entries
                                    }
                                }
                            }
                            ViewBag.Categories = categoryList;
                        }
                    }

                    if (catalogueTypeValue == 4 && ViewBag.Brands != null)
                    {
                        var selectedBrandIds = new List<long>();
                        var catalogueBrandIds = catalogue.BrandIds as IEnumerable;
                        if (catalogueBrandIds != null)
                        {
                            foreach (var id in catalogueBrandIds)
                            {
                                try
                                {
                                    var parsed = Convert.ToInt64(id);
                                    if (parsed > 0 && !selectedBrandIds.Contains(parsed))
                                    {
                                        selectedBrandIds.Add(parsed);
                                    }
                                }
                                catch
                                {
                                    // ignore parse errors
                                }
                            }
                        }

                        if (selectedBrandIds.Count > 0)
                        {
                            var brandList = new List<object>();
                            var sourceBrands = ViewBag.Brands as IEnumerable;
                            if (sourceBrands != null)
                            {
                                foreach (var brand in sourceBrands)
                                {
                                    try
                                    {
                                        var brandIdObject = brand.GetType().GetProperty("BrandId")?.GetValue(brand, null);
                                        var brandId = Convert.ToInt64(brandIdObject);
                                        if (selectedBrandIds.Contains(brandId))
                                        {
                                            brandList.Add(brand);
                                        }
                                    }
                                    catch
                                    {
                                        // ignore invalid brand entries
                                    }
                                }
                            }
                            ViewBag.Brands = brandList;
                        }
                    }

                }
                catch
                {
                    // ignore filtering failures
                }
            }

            return View();
        }

        public async Task<ActionResult> PasswordProtected(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("CatalogueNotFound");
            }

            serializer.MaxJsonLength = 2147483644;

            CatalogueController catalogueController = new CatalogueController();
            var catalogueResult = await catalogueController.PublicCatalogue(slug);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);

            if (oClsResponse.Status == 0 || oClsResponse.Data == null)
            {
                return RedirectToAction("CatalogueNotFound");
            }

            dynamic catalogue = oClsResponse.Data.Catalogue;
            ViewBag.Catalogue = catalogue;
            ViewBag.PageTitle = catalogue.MetaTitle ?? catalogue.CatalogueName;
            ViewBag.MetaDescription = catalogue.MetaDescription;
            ViewBag.CanonicalUrl = Request.Url.Scheme + "://" + Request.Url.Host + "/c/" + slug;

            return View();
        }

        public Task<ActionResult> CatalogueNotFound()
        {
            return Task.FromResult<ActionResult>(View());
        }

        [HttpPost]
        public async Task<ActionResult> VerifyPassword(string slug, string password)
        {
            var obj = new ClsCataloguePasswordVm
            {
                UniqueSlug = slug,
                Password = password
            };

            CatalogueController catalogueController = new CatalogueController();
            var verifyResult = await catalogueController.VerifyCataloguePassword(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(verifyResult);

            if (oClsResponse.Status == 1)
            {
                Session["CatalogueAuth_" + slug] = "verified";
            }

            return Json(oClsResponse);
        }

        [HttpPost]
        public async Task<ActionResult> RenderProducts(PublicCatalogueFilterRequest filter)
        {
            if (filter == null || string.IsNullOrWhiteSpace(filter.Slug))
            {
                return new HttpStatusCodeResult(400, "Slug is required");
            }

            serializer.MaxJsonLength = 2147483644;

            CatalogueController catalogueController = new CatalogueController();
            var catalogueResponseResult = await catalogueController.PublicCatalogue(filter.Slug);
            ClsResponse catalogueResult = await oCommonController.ExtractResponseFromActionResult(catalogueResponseResult);
            if (catalogueResult.Status == 0 || catalogueResult.Data == null)
            {
                return new HttpStatusCodeResult(404, "Catalogue not found");
            }

            dynamic catalogue = catalogueResult.Data.Catalogue;

            int defaultSortOrder = ToInt(GetDynamicValue(catalogue, "DefaultSortOrder"), 1);
            if (defaultSortOrder <= 0)
            {
                defaultSortOrder = 1;
            }
            if (filter.SortOrder <= 0)
            {
                filter.SortOrder = defaultSortOrder;
            }

            long branchId = ResolveBranchId(catalogue);

            var itemRequest = new ClsCatalogueItemVm
            {
                UniqueSlug = filter.Slug,
                Search = filter.Search,
                PageIndex = filter.PageIndex > 0 ? filter.PageIndex : 1,
                PageSize = filter.PageSize,
                SortOrder = filter.SortOrder,
                CategoryId = filter.CategoryId,
                SubCategoryId = filter.SubCategoryId,
                SubSubCategoryId = filter.SubSubCategoryId,
                BrandIds = filter.BrandIds ?? new List<long>(),
                MinPrice = filter.MinPrice,
                MaxPrice = filter.MaxPrice,
                IncludeOutOfStock = filter.IncludeOutOfStock,
                CompanyId = catalogue.CompanyId,
                BranchId = branchId,
                PlaceOfSupplyId = filter.PlaceOfSupplyId
            };

            if (itemRequest.PageSize <= 0)
            {
                int defaultPageSize;
                if (int.TryParse(Convert.ToString(GetDynamicValue(catalogue, "ItemsPerPage")), out defaultPageSize) && defaultPageSize > 0)
                {
                    itemRequest.PageSize = defaultPageSize;
                }
            }

            if (itemRequest.BrandIds == null)
            {
                itemRequest.BrandIds = new List<long>();
            }

            ItemController itemController = new ItemController();
            ClsItemBranchMapVm itemBranchMapObj = new ClsItemBranchMapVm
            {
                CompanyId = ToLong(GetDynamicValue(catalogue, "CompanyId")),
                PageIndex = itemRequest.PageIndex,
                PageSize = itemRequest.PageSize,
                SortOrder = itemRequest.SortOrder,
                BranchId = itemRequest.BranchId,
                PlaceOfSupplyId = itemRequest.PlaceOfSupplyId.HasValue ? itemRequest.PlaceOfSupplyId.Value : 0,
                Search = itemRequest.Search,
                CategoryId = itemRequest.CategoryId.HasValue ? itemRequest.CategoryId.Value : 0,
                SubCategoryId = itemRequest.SubCategoryId.HasValue ? itemRequest.SubCategoryId.Value : 0,
                SubSubCategoryId = itemRequest.SubSubCategoryId.HasValue ? itemRequest.SubSubCategoryId.Value : 0,
                BrandIds = itemRequest.BrandIds,
                MinPrice = itemRequest.MinPrice,
                MaxPrice = itemRequest.MaxPrice,
                IncludeOutOfStock = itemRequest.IncludeOutOfStock
            };
            var itemsResponseResult = await itemController.PublicItems(itemBranchMapObj);
            ClsResponse itemsResponse = await oCommonController.ExtractResponseFromActionResult(itemsResponseResult);
            IEnumerable items = null;
            int responsePageIndex = itemRequest.PageIndex;
            int responsePageSize = itemRequest.PageSize;
            int responseTotalCount = 0;

            if (itemsResponse.Status == 1 && itemsResponse.Data != null)
            {
                items = itemsResponse.Data.Items as IEnumerable ??
                        itemsResponse.Data.ItemDetails as IEnumerable;
                responsePageIndex = ToInt(GetDynamicValue(itemsResponse.Data, "PageIndex"), responsePageIndex);
                responsePageSize = ToInt(GetDynamicValue(itemsResponse.Data, "PageSize"), responsePageSize);
                responseTotalCount = ToInt(GetDynamicValue(itemsResponse.Data, "TotalCount"), responseTotalCount);
            }

            var model = BuildProductGridModel(catalogue, items, responsePageIndex, responsePageSize, responseTotalCount);

            return PartialView("~/Views/PublicCatalogue/_ProductGrid.cshtml", model);
        }

        [HttpPost]
        public async Task<ActionResult> SearchSuggestions(PublicCatalogueSearchSuggestionsRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Slug))
            {
                return Json(new { suggestions = new object[0] });
            }

            var term = (request.Term ?? string.Empty).Trim();
            if (term.Length < 2)
            {
                return Json(new { suggestions = new object[0] });
            }

            serializer.MaxJsonLength = 2147483644;

            CatalogueController catalogueController = new CatalogueController();
            var catalogueResponseResult = await catalogueController.PublicCatalogue(request.Slug);
            ClsResponse catalogueResult = await oCommonController.ExtractResponseFromActionResult(catalogueResponseResult);
            if (catalogueResult.Status == 0 || catalogueResult.Data == null || catalogueResult.Data.Catalogue == null)
            {
                return Json(new { suggestions = new object[0] });
            }

            dynamic catalogue = catalogueResult.Data.Catalogue;
            long branchId = ResolveBranchId(catalogue);

            int limit = request.Limit > 0 ? request.Limit : 8;
            if (limit > 15)
            {
                limit = 15;
            }

            int defaultSortOrder = ToInt(GetDynamicValue(catalogue, "DefaultSortOrder"), 1);
            if (defaultSortOrder <= 0)
            {
                defaultSortOrder = 1;
            }

            bool includeOutOfStock = request.IncludeOutOfStock ?? true;
            var brandIds = request.BrandIds ?? new List<long>();
            if (brandIds.Count > 0)
            {
                brandIds = brandIds
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();
            }
            decimal? minPrice = request.MinPrice;
            decimal? maxPrice = request.MaxPrice;
            if (minPrice.HasValue && minPrice.Value < 0)
            {
                minPrice = 0;
            }
            if (maxPrice.HasValue && maxPrice.Value < 0)
            {
                maxPrice = 0;
            }

            ItemController itemController = new ItemController();
            ClsItemBranchMapVm itemBranchMapObj = new ClsItemBranchMapVm
            {
                CompanyId = ToLong(GetDynamicValue(catalogue, "CompanyId")),
                PageIndex = 1,
                PageSize = limit,
                SortOrder = defaultSortOrder,
                BranchId = branchId,
                PlaceOfSupplyId = (request.PlaceOfSupplyId.HasValue && request.PlaceOfSupplyId.Value > 0) ? request.PlaceOfSupplyId.Value : 0,
                Search = term,
                IncludeOutOfStock = includeOutOfStock,
                BrandIds = brandIds,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };
            var itemsResponseResult = await itemController.PublicItems(itemBranchMapObj);
            ClsResponse itemsResponse = await oCommonController.ExtractResponseFromActionResult(itemsResponseResult);
            var suggestions = new List<object>();

            if (itemsResponse.Status == 1 && itemsResponse.Data != null)
            {
                IEnumerable items = itemsResponse.Data.Items as IEnumerable ??
                                    itemsResponse.Data.ItemDetails as IEnumerable;
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var mapped = MapProduct(item);
                        if (mapped == null)
                        {
                            continue;
                        }

                        suggestions.Add(new
                        {
                            itemId = mapped.ItemId,
                            itemDetailsId = mapped.ItemDetailsId,
                            label = mapped.ItemName,
                            sku = mapped.SKU,
                            price = mapped.SalesIncTax,
                            image = mapped.ProductImage
                        });
                    }
                }
            }

            return Json(new { suggestions = suggestions });
        }

        [HttpPost]
        public async Task<ActionResult> ExistingCustomerByMobile(PublicCustomerLookupRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.MobileNo))
            {
                return new HttpStatusCodeResult(400, "Slug and MobileNo are required");
            }

            serializer.MaxJsonLength = 2147483644;

            // Resolve catalogue to obtain CompanyId
            CatalogueController catalogueController = new CatalogueController();
            var catalogueResponseResult = await catalogueController.PublicCatalogue(request.Slug);
            ClsResponse catalogueResult = await oCommonController.ExtractResponseFromActionResult(catalogueResponseResult);
            if (catalogueResult.Status == 0 || catalogueResult.Data == null || catalogueResult.Data.Catalogue == null)
            {
                return new HttpStatusCodeResult(404, "Catalogue not found");
            }

            dynamic catalogue = catalogueResult.Data.Catalogue;

            // Prepare request for User/UserByMobileNo
            UserController userController = new UserController();
            var httpRequest = new System.Net.Http.HttpRequestMessage();
            var config = new System.Web.Http.HttpConfiguration();
            userController.ControllerContext = new System.Web.Http.Controllers.HttpControllerContext(
                config,
                new System.Web.Http.Routing.HttpRouteData(new System.Web.Http.Routing.HttpRoute()),
                httpRequest);
            
            var vm = new ClsUserVm
            {
                CompanyId = Convert.ToInt64(catalogue.CompanyId),
                UserType = "customer",
                MobileNo = request.MobileNo
            };
            var userResult = await userController.UserByMobileNo(vm);
            ClsResponse userResponse = await oCommonController.ExtractResponseFromActionResult(userResult);

            // Pass-through the API response as JSON
            return Json(userResponse);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitQuotation(PublicQuotationSubmitRequest request)
        {
            serializer.MaxJsonLength = 2147483644;

            if (request == null)
            {
                return Json(new { Status = 0, Message = "Invalid request." });
            }

            if (string.IsNullOrWhiteSpace(request.Slug))
            {
                return Json(new { Status = 0, Message = "Catalogue is required." });
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return Json(new
                {
                    Status = 2,
                    Message = "Your cart is empty.",
                    Errors = new[] { new ClsError { Message = "Your cart is empty." } }
                });
            }

            if (request.Customer == null)
            {
                return Json(new
                {
                    Status = 2,
                    Message = "Customer details are required.",
                    Errors = new[] { new ClsError { Message = "Customer details are required." } }
                });
            }

            CatalogueController catalogueController = new CatalogueController();
            var catalogueResult = await catalogueController.PublicCatalogue(request.Slug);
            var catalogueResponse = await oCommonController.ExtractResponseFromActionResult(catalogueResult);
            if (catalogueResponse == null || catalogueResponse.Status != 1 || catalogueResponse.Data == null || catalogueResponse.Data.Catalogue == null)
            {
                return Json(new { Status = 0, Message = "Catalogue not found." });
            }

            dynamic catalogue = catalogueResponse.Data.Catalogue;

            long companyId = ToLong(GetDynamicValue(catalogue, "CompanyId"));
            if (companyId <= 0)
            {
                return Json(new { Status = 0, Message = "Invalid catalogue configuration." });
            }

            long branchId = ResolveBranchId(catalogue);
            if (branchId <= 0)
            {
                return Json(new { Status = 0, Message = "Branch information is missing for this catalogue." });
            }

            long sellingPriceGroupId = ToLong(GetDynamicValue(catalogue, "SellingPriceGroupId"));
            long addedBy = ToLong(GetDynamicValue(catalogue, "AddedBy"));
            if (addedBy <= 0)
            {
                addedBy = companyId;
            }

            int configuredMode = ToInt(GetDynamicValue(catalogue, "AddToCartQuotationMode"), 0);
            int mode = request.Mode > 0 ? request.Mode : configuredMode;
            if (mode <= 0)
            {
                mode = 1;
            }

            var normalizeResult = NormalizeQuotationItems(request.Items);
            if (!normalizeResult.Success)
            {
                return Json(new
                {
                    Status = normalizeResult.Status,
                    Message = normalizeResult.Message,
                    Errors = normalizeResult.Errors
                });
            }

            var normalizedItems = normalizeResult.Items;
            if ((mode == 1 || mode == 3) && normalizedItems.Any(item => item.UnitCost <= 0))
            {
                return Json(new
                {
                    Status = 2,
                    Message = "Item prices are required to create a sales quotation in the system.",
                    Errors = new[] { new ClsError { Message = "Update item prices before creating a quotation in the system." } }
                });
            }

            var customer = request.Customer;
            long requestedPlaceOfSupplyId = request.PlaceOfSupplyId > 0 ? request.PlaceOfSupplyId : 0;
            if (requestedPlaceOfSupplyId > 0 && customer != null)
            {
                if (customer.Billing == null)
                {
                    customer.Billing = new PublicQuotationAddress();
                }

                if (customer.Billing != null && customer.Billing.StateId <= 0)
                {
                    customer.Billing.StateId = requestedPlaceOfSupplyId;
                }
            }

            string primaryMobileDigits = NormalizeDigits(!string.IsNullOrWhiteSpace(customer.Mobile) ? customer.Mobile : customer.MobileRaw);
            if (string.IsNullOrEmpty(primaryMobileDigits))
            {
                return Json(new
                {
                    Status = 2,
                    Message = "A valid customer mobile number is required.",
                    Errors = new[] { new ClsError { Message = "Enter a valid customer mobile number." } }
                });
            }

            var branchMeta = await GetBranchMetaAsync(branchId, catalogue);
            if (branchMeta == null)
            {
                return Json(new { Status = 0, Message = "Unable to resolve branch details. Please contact the business owner." });
            }

            long customerId = 0;
            UserController userController = new UserController();
            var httpRequest = new System.Net.Http.HttpRequestMessage();
            var config = new System.Web.Http.HttpConfiguration();
            userController.ControllerContext = new System.Web.Http.Controllers.HttpControllerContext(
                config,
                new System.Web.Http.Routing.HttpRouteData(new System.Web.Http.Routing.HttpRoute()),
                httpRequest);
            
            ClsUserVm lookupVm = new ClsUserVm
            {
                CompanyId = companyId,
                UserType = "customer",
                MobileNo = primaryMobileDigits
            };
            try
            {
                var lookupResult = await userController.UserByMobileNo(lookupVm);
                var lookupResponse = await oCommonController.ExtractResponseFromActionResult(lookupResult);
                if (lookupResponse != null && lookupResponse.Status == 1 && lookupResponse.Data != null && lookupResponse.Data.User != null)
                {
                    customerId = lookupResponse.Data.User.UserId;
                }
            }
            catch
            {
                // ignore lookup parse errors
            }

            if ((mode == 1 || mode == 3) && customerId <= 0)
            {
                var createCustomerResult = await CreateCustomerAsync(customer, primaryMobileDigits, companyId, branchId, addedBy, branchMeta, requestedPlaceOfSupplyId);
                if (!createCustomerResult.Success)
                {
                    return Json(new
                    {
                        Status = createCustomerResult.Status,
                        Message = createCustomerResult.Message,
                        Errors = createCustomerResult.Errors
                    });
                }

                customerId = createCustomerResult.CustomerId;
            }

            string invoiceNo = null;
            long salesQuotationId = 0;
            string whatsappUrl = null;

            if (mode == 1 || mode == 3)
            {
                var quotationResult = await CreateSalesQuotationAsync(normalizedItems, companyId, branchId, sellingPriceGroupId, addedBy, customerId, customer, branchMeta, mode, requestedPlaceOfSupplyId);
                if (!quotationResult.Success)
                {
                    return Json(new
                    {
                        Status = quotationResult.Status,
                        Message = quotationResult.Message,
                        Errors = quotationResult.Errors
                    });
                }

                invoiceNo = quotationResult.InvoiceNo;
                salesQuotationId = quotationResult.SalesQuotationId;
                whatsappUrl = quotationResult.WhatsappUrl;
            }
            else
            {
                whatsappUrl = BuildWhatsappUrl(normalizedItems, customer, branchMeta.WhatsappDigits, null);
            }

            var responseData = new
            {
                InvoiceNo = invoiceNo,
                SalesQuotationId = salesQuotationId,
                WhatsappUrl = whatsappUrl
            };

            return Json(new
            {
                Status = 1,
                Message = "Quotation submitted successfully.",
                Data = responseData
            });
        }

        private long ResolveBranchId(dynamic catalogue)
        {
            long branchId = 0;
            if (catalogue == null)
            {
                return branchId;
            }
            try
            {
                var branchIdProperty = catalogue.GetType().GetProperty("BranchId");
                if (branchIdProperty != null)
                {
                    var branchIdValue = branchIdProperty.GetValue(catalogue, null);
                    if (branchIdValue != null)
                    {
                        long.TryParse(Convert.ToString(branchIdValue), out branchId);
                    }
                }

                if (branchId == 0 && catalogue.Branch != null)
                {
                    var branchObj = catalogue.Branch;
                    var branchObjBranchIdProperty = branchObj.GetType().GetProperty("BranchId");
                    if (branchObjBranchIdProperty != null)
                    {
                        var value = branchObjBranchIdProperty.GetValue(branchObj, null);
                        if (value != null)
                        {
                            long.TryParse(Convert.ToString(value), out branchId);
                        }
                    }
                }
            }
            catch
            {
                branchId = 0;
            }

            return branchId;
        }

        private async Task<BranchMeta> GetBranchMetaAsync(long branchId, dynamic catalogue)
        {
            string dialingCode = Convert.ToString(GetDynamicValue(GetDynamicValue(catalogue, "Branch"), "DialingCode"));
            string whatsappNo = Convert.ToString(GetDynamicValue(GetDynamicValue(catalogue, "Branch"), "WhatsappNo"));
            string whatsappDigits = NormalizeDigits((dialingCode ?? string.Empty) + (whatsappNo ?? string.Empty));

            using (var db = new ConnectionContext())
            {
                var branch = await db.DbClsBranch.Where(b => b.BranchId == branchId)
                    .Select(b => new { b.BranchId, b.StateId, b.CountryId })
                    .FirstOrDefaultAsync();

                if (branch == null)
                {
                    return null;
                }

                return new BranchMeta
                {
                    BranchId = branch.BranchId,
                    StateId = branch.StateId,
                    CountryId = branch.CountryId,
                    WhatsappDigits = whatsappDigits
                };
            }
        }

        private string GetClientIpAddress()
        {
            try
            {
                var context = System.Web.HttpContext.Current;
                if (context != null && context.Request != null)
                {
                    return context.Request.UserHostAddress;
                }

                return Request?.RequestContext?.HttpContext?.Request?.UserHostAddress;
            }
            catch
            {
                return null;
            }
        }

        private static object GetDynamicValue(object source, string propertyName)
        {
            if (source == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            // Try dictionary access first (for JSON deserialized objects)
            var dictionary = source as IDictionary<string, object>;
            if (dictionary != null)
            {
                object value;
                if (dictionary.TryGetValue(propertyName, out value))
                {
                    return value;
                }
            }

            // Try property access via reflection
            try
            {
                var type = source.GetType();
                var property = type.GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (property != null)
                {
                    return property.GetValue(source, null);
                }
            }
            catch
            {
                // Ignore reflection errors
            }

            return null;
        }

        private static bool ToBoolean(object value, bool defaultValue = false)
        {
            if (value == null)
            {
                return defaultValue;
            }

            if (value is bool flag)
            {
                return flag;
            }

            var stringValue = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return defaultValue;
            }

            bool parsedBool;
            if (bool.TryParse(stringValue, out parsedBool))
            {
                return parsedBool;
            }

            int parsedInt;
            if (int.TryParse(stringValue, out parsedInt))
            {
                return parsedInt != 0;
            }

            return defaultValue;
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null)
            {
                return 0m;
            }

            if (value is decimal dec)
            {
                return dec;
            }

            if (value is double dbl)
            {
                return Convert.ToDecimal(dbl);
            }

            if (value is float flt)
            {
                return Convert.ToDecimal(flt);
            }

            decimal parsedDecimal;
            return decimal.TryParse(Convert.ToString(value), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedDecimal)
                ? parsedDecimal
                : 0m;
        }

        private static decimal RoundCurrency(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static long ToLong(object value, long defaultValue = 0)
        {
            if (value == null)
            {
                return defaultValue;
            }

            if (value is long lng)
            {
                return lng;
            }

            if (value is int integer)
            {
                return integer;
            }

            long parsed;
            return long.TryParse(Convert.ToString(value), out parsed) ? parsed : defaultValue;
        }

        private static string NormalizeDigits(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return Regex.Replace(value, "[^0-9]", string.Empty);
        }

        private NormalizeItemsResult NormalizeQuotationItems(IEnumerable<PublicQuotationItem> items)
        {
            var result = new NormalizeItemsResult();
            if (items == null)
            {
                result.Success = false;
                result.Status = 2;
                result.Message = "Your cart is empty.";
                result.Errors.Add(new ClsError { Message = "Your cart is empty." });
                return result;
            }

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                long itemId = item.ItemId;
                long itemDetailsId = item.ItemDetailsId;
                string name = string.IsNullOrWhiteSpace(item.Name)
                    ? (!string.IsNullOrWhiteSpace(item.Code) ? item.Code : (!string.IsNullOrWhiteSpace(item.Sku) ? item.Sku : "Item"))
                    : item.Name.Trim();

                decimal quantity = item.Quantity.HasValue ? Convert.ToDecimal(item.Quantity.Value) : 0m;
                if (quantity <= 0)
                {
                    quantity = 1m;
                }

                decimal price = item.Price.HasValue ? Convert.ToDecimal(item.Price.Value) : 0m;
                decimal unitCost = price;
                decimal amount = unitCost * quantity;

                result.Items.Add(new NormalizedQuotationItem
                {
                    ItemId = itemId,
                    ItemDetailsId = itemDetailsId,
                    Code = item.Code,
                    Sku = item.Sku,
                    Name = name,
                    Quantity = quantity,
                    UnitCost = unitCost,
                    PriceIncTax = price,
                    Amount = amount,
                    DisplayPrice = price
                });
            }

            if (result.Items.Count == 0)
            {
                result.Success = false;
                result.Status = 2;
                result.Message = "Your cart is empty.";
                result.Errors.Add(new ClsError { Message = "Your cart is empty." });
                return result;
            }

            result.Success = true;
            result.Status = 1;
            return result;
        }

        private async Task<Dictionary<long, ItemDetailSnapshot>> FetchItemDetailSnapshotsAsync(IEnumerable<NormalizedQuotationItem> items, long companyId, long branchId, long sellingPriceGroupId, long placeOfSupplyId)
        {
            var snapshots = new Dictionary<long, ItemDetailSnapshot>();
            if (items == null)
            {
                return snapshots;
            }

            var itemDetailIds = items
                .Where(i => i != null && i.ItemDetailsId > 0)
                .Select(i => i.ItemDetailsId)
                .Distinct()
                .ToList();

            if (itemDetailIds.Count == 0)
            {
                return snapshots;
            }

            foreach (var itemDetailsId in itemDetailIds)
            {
                try
                {
                    var matchingItem = items.FirstOrDefault(i => i != null && i.ItemDetailsId == itemDetailsId);
                    ItemController itemController = new ItemController();
                    ClsItemVm itemVm = new ClsItemVm
                    {
                        CompanyId = companyId,
                        BranchId = branchId,
                        ItemDetailsId = itemDetailsId,
                        ItemCode = matchingItem?.Code ?? string.Empty,
                        PageIndex = 1,
                        PageSize = 1,
                        SellingPriceGroupId = sellingPriceGroupId,
                        PlaceOfSupplyId = placeOfSupplyId
                    };
                    var searchResult = await itemController.SearchItems(itemVm);
                    var response = await oCommonController.ExtractResponseFromActionResult(searchResult);
                    if (response == null)
                    {
                        continue;
                    }
                    if (response == null || response.Status != 1 || response.Data == null)
                    {
                        continue;
                    }

                    var details = response.Data.ItemDetails as IEnumerable ?? response.Data.Items as IEnumerable;
                    if (details == null)
                    {
                        continue;
                    }

                    foreach (var detail in details)
                    {
                        long detailId = ToLong(GetDynamicValue(detail, "ItemDetailsId"));
                        if (detailId <= 0 || detailId != itemDetailsId || snapshots.ContainsKey(detailId))
                        {
                            continue;
                        }

                        snapshots[detailId] = new ItemDetailSnapshot
                        {
                            ItemDetail = detail,
                            ItemId = ToLong(GetDynamicValue(detail, "ItemId")),
                            SalesExcTax = ToDecimal(GetDynamicValue(detail, "SalesExcTax")),
                            SalesIncTax = ToDecimal(GetDynamicValue(detail, "SalesIncTax")),
                            TaxId = ToLong(GetDynamicValue(detail, "TaxId")),
                            PriceAddedFor = ToInt(GetDynamicValue(detail, "PriceAddedFor"), 1),
                            SKU = Convert.ToString(GetDynamicValue(detail, "SKU")),
                            ItemName = Convert.ToString(GetDynamicValue(detail, "ItemName"))
                        };
                    }
                }
                catch
                {
                    // ignore lookup failures for individual items
                }
            }

            return snapshots;
        }

        private List<ClsAddressVm> BuildAddressList(PublicQuotationCustomer customer, BranchMeta branchMeta)
        {
            var addresses = new List<ClsAddressVm>();
            if (customer == null)
            {
                return addresses;
            }

            var billing = CreateAddressVm(customer.Billing, "Billing", customer.Name, branchMeta);
            if (billing != null)
            {
                addresses.Add(billing);
            }

            if (customer.IsShippingDifferent && customer.Shipping != null)
            {
                var shipping = CreateAddressVm(customer.Shipping, "Shipping", customer.Name, branchMeta);
                if (shipping != null)
                {
                    addresses.Add(shipping);
                }
            }

            return addresses;
        }

        private ClsAddressVm CreateAddressVm(PublicQuotationAddress address, string type, string fallbackName, BranchMeta branchMeta)
        {
            if (address == null && string.IsNullOrWhiteSpace(fallbackName))
            {
                return null;
            }

            address = address ?? new PublicQuotationAddress();
            string name = string.IsNullOrWhiteSpace(address.Name) ? fallbackName : address.Name.Trim();
            string email = string.IsNullOrWhiteSpace(address.Email) ? null : address.Email.Trim();
            string addrLine = string.IsNullOrWhiteSpace(address.Address) ? null : address.Address.Trim();
            string landmark = string.IsNullOrWhiteSpace(address.Landmark) ? null : address.Landmark.Trim();
            string mobile = NormalizeDigits(address.Mobile);
            string altMobile = NormalizeDigits(address.AltMobile);

            long countryId = address.CountryId > 0 ? address.CountryId : branchMeta.CountryId;
            long stateId = address.StateId > 0 ? address.StateId : branchMeta.StateId;
            long cityId = address.CityId > 0 ? address.CityId : 0;
            long zipcode = 0;
            var pinDigits = NormalizeDigits(address.Pincode);
            if (!string.IsNullOrWhiteSpace(pinDigits))
            {
                long.TryParse(pinDigits, out zipcode);
            }

            return new ClsAddressVm
            {
                Type = type,
                Name = string.IsNullOrWhiteSpace(name) ? fallbackName : name,
                EmailId = email,
                Address = addrLine,
                Landmark = landmark,
                MobileNo = mobile,
                MobileNo2 = altMobile,
                CountryId = countryId,
                StateId = stateId,
                CityId = cityId,
                Zipcode = zipcode,
                IsActive = true,
                IsDeleted = false
            };
        }

        private async Task<CreateCustomerResult> CreateCustomerAsync(PublicQuotationCustomer customer, string primaryMobileDigits, long companyId, long branchId, long addedBy, BranchMeta branchMeta, long placeOfSupplyIdOverride = 0)
        {
            var result = new CreateCustomerResult { Status = 0 };
            try
            {
                var billing = customer?.Billing;
                if (billing == null)
                {
                    billing = new PublicQuotationAddress();
                    if (customer != null)
                    {
                        customer.Billing = billing;
                    }
                }

                if (placeOfSupplyIdOverride > 0 && billing.StateId <= 0)
                {
                    billing.StateId = placeOfSupplyIdOverride;
                }

                var addresses = BuildAddressList(customer, branchMeta);
                long branchStateId = branchMeta?.StateId ?? 0;
                long placeOfSupplyId = placeOfSupplyIdOverride > 0
                    ? placeOfSupplyIdOverride
                    : (billing.StateId > 0 ? billing.StateId : branchStateId);

                if (branchMeta != null && branchMeta.CountryId != 2)
                {
                    placeOfSupplyId = 0;
                }

                var userVm = new ClsUserVm
                {
                    CompanyId = companyId,
                    AddedBy = addedBy,
                    UserType = "customer",
                    Name = (customer?.Name ?? string.Empty).Trim(),
                    MobileNo = primaryMobileDigits,
                    AltMobileNo = NormalizeDigits(billing.AltMobile),
                    EmailId = string.IsNullOrWhiteSpace(customer?.Email) ? null : customer.Email.Trim(),
                    IsActive = true,
                    IsDeleted = false,
                    JoiningDate = DateTime.UtcNow,
                    BranchId = branchId,
                    IsShippingAddressDifferent = customer?.IsShippingDifferent ?? false,
                    GstTreatment = "Consumer",
                    PlaceOfSupplyId = placeOfSupplyId,
                    TaxPreference = "Taxable",
                    Addresses = addresses,
                    IpAddress = GetClientIpAddress(),
                    Browser = Request?.Browser?.Browser ?? "Public Catalogue",
                    Platform = Request?.Browser?.Platform ?? "web",
                    Domain = Request?.Url?.Host
                };
                if (userVm.Addresses == null)
                {
                    userVm.Addresses = new List<ClsAddressVm>();
                }

                var controller = new EquiBillBook.Controllers.WebApi.UserController();
                var request = new HttpRequestMessage();
                var config = new System.Web.Http.HttpConfiguration();
                controller.ControllerContext = new System.Web.Http.Controllers.HttpControllerContext(
                    config,
                    new System.Web.Http.Routing.HttpRouteData(new System.Web.Http.Routing.HttpRoute()),
                    request);

                var actionResult = await controller.InsertUser(userVm);
                var responseMessage = await actionResult.ExecuteAsync(CancellationToken.None);
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var response = serializer.Deserialize<ClsResponse>(responseContent);

                if (response == null)
                {
                    result.Message = "Unable to create customer.";
                    return result;
                }

                if (response.Status != 1)
                {
                    result.Status = response.Status;
                    result.Message = string.IsNullOrWhiteSpace(response.Message) ? "Unable to create customer." : response.Message;
                    if (response.Errors != null && response.Errors.Count > 0)
                    {
                        result.Errors = response.Errors;
                    }
                    return result;
                }

                if (response.Data == null || response.Data.User == null || response.Data.User.UserId <= 0)
                {
                    result.Status = 0;
                    result.Message = "Customer creation response was incomplete.";
                    return result;
                }

                result.Success = true;
                result.Status = 1;
                result.CustomerId = response.Data.User.UserId;
                result.Message = string.IsNullOrWhiteSpace(response.Message) ? "Customer created successfully." : response.Message;
                return result;
            }
            catch (Exception ex)
            {
                result.Status = 0;
                result.Message = "Unable to create customer.";
                result.Errors.Add(new ClsError { Message = ex.Message });
                return result;
            }
        }

        private async Task<QuotationOperationResult> CreateSalesQuotationAsync(List<NormalizedQuotationItem> items, long companyId, long branchId, long sellingPriceGroupId, long addedBy, long customerId, PublicQuotationCustomer customer, BranchMeta branchMeta, int mode, long placeOfSupplyIdOverride = 0)
        {
            ConnectionContext oConnectionContext = new ConnectionContext();
            var CurrentDate = oCommonController.CurrentDate(companyId);
            var result = new QuotationOperationResult { Status = 0 };
            try
            {
                var quotationVm = new ClsSalesQuotationVm
                {
                    CompanyId = companyId,
                    BranchId = branchId,
                    CustomerId = customerId,
                    SalesQuotationDate = CurrentDate,
                    Status = "Sent",
                    AddedBy = addedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = addedBy,
                    ModifiedOn = CurrentDate,
                    IsActive = true,
                    IsDeleted = false,
                    SellingPriceGroupId = sellingPriceGroupId,
                    InvoiceNo = string.Empty,
                    IsShippingAddressDifferent = customer?.IsShippingDifferent ?? false,
                    CustomerName = customer?.Name,
                    CustomerMobileNo = NormalizeDigits(!string.IsNullOrWhiteSpace(customer?.Mobile) ? customer.Mobile : customer?.MobileRaw),
                    CustomerEmailId = string.IsNullOrWhiteSpace(customer?.Email) ? null : customer.Email.Trim(),
                    PaymentType = string.Empty,
                    PayTerm = 0,
                    PayTermNo = 0,
                    Notes = string.Empty,
                    ShippingAddress = FormatAddress(customer?.IsShippingDifferent == true && customer?.Shipping != null ? customer.Shipping : customer?.Billing),
                    ShippingDetails = FormatAddress(customer?.Shipping ?? customer?.Billing),
                    ShippingStatus = string.Empty,
                    DeliveredTo = null,
                    AttachDocument = null,
                    ShippingDocument = null,
                    IpAddress = GetClientIpAddress(),
                    Browser = Request?.Browser?.Browser ?? "Public Catalogue",
                    Platform = Request?.Browser?.Platform ?? "web",
                    Domain = Request?.Url?.Host,
                    CashRegisterId = 0,
                    SmsSettingsId = 0,
                    EmailSettingsId = 0,
                    WhatsappSettingsId = 0,
                    OnlinePaymentSettingsId = 0,
                    ExchangeRate = 1,
                    Discount = 0,
                    DiscountType = "fixed",
                    TotalDiscount = 0,
                    TotalPaying = 0,
                    Balance = 0,
                    ChangeReturn = 0,
                    TaxAmount = 0,
                    TotalTaxAmount = 0,
                    SpecialDiscount = 0,
                    RoundOff = 0,
                    NetAmountReverseCharge = 0,
                    RoundOffReverseCharge = 0,
                    GrandTotalReverseCharge = 0,
                    PayTaxForExport = 0,
                    TaxCollectedFromCustomer = 0,
                    TaxExemptionId = 0,
                    CustomerAddress = FormatAddress(customer?.Billing),
                    CustomerMobile = NormalizeDigits(customer?.Billing != null ? customer.Billing.Mobile : customer?.Mobile),
                    GstTreatment = "Consumer",
                    IsBusinessRegistered = 0,
                    SalesQuotationDetails = new List<ClsSalesQuotationDetailsVm>()
                };

                long placeOfSupplyId = 0;
                if (placeOfSupplyIdOverride > 0)
                {
                    placeOfSupplyId = placeOfSupplyIdOverride;
                }
                else if (branchMeta != null)
                {
                    placeOfSupplyId = customer?.Billing != null && customer.Billing.StateId > 0
                        ? customer.Billing.StateId
                        : branchMeta.StateId;
                }
                if (branchMeta != null && branchMeta.CountryId != 2)
                {
                    placeOfSupplyId = 0;
                }
                quotationVm.PlaceOfSupplyId = placeOfSupplyId;

                //var itemDetailSnapshots = await FetchItemDetailSnapshotsAsync(items, companyId, branchId, sellingPriceGroupId, quotationVm.PlaceOfSupplyId);

                ItemController itemController = new ItemController();
                foreach (var item in items)
                {
                    ClsItemVm itemVm = new ClsItemVm
                    {
                        CompanyId = companyId,
                        BranchId = branchId,
                        ItemCode = item.Code,
                        SellingPriceGroupId = sellingPriceGroupId,
                        PlaceOfSupplyId = placeOfSupplyId,
                        Type = "Sales",
                        CustomerId = customerId
                    };
                    var searchResult = await itemController.SearchItems(itemVm);
                    var _response = await oCommonController.ExtractResponseFromActionResult(searchResult);
                    if (_response == null || _response.Status != 1 || _response.Data == null)
                    {
                        continue;
                    }

                    var details = _response.Data.ItemDetails;
                    if (details == null)
                    {
                        continue;
                    }

                    foreach (var detail in details)
                    {
                        decimal _AmountExcTax = detail.SalesExcTax * item.Quantity;
                        decimal _AmountIncTax = detail.SalesIncTax * item.Quantity;
                        decimal _Quantity = item.Quantity;
                        long _ItemDetailsId = detail.ItemDetailsId;
                        long _ItemId = detail.ItemId;
                        decimal _PriceExcTax = detail.PurchaseExcTax;
                        decimal _PriceIncTax = detail.PurchaseIncTax;
                        string _Tax = detail.Tax;
                        long _TaxId = detail.TaxId;
                        decimal _UnitCost = detail.SalesExcTax;
                        decimal _SalesIncTax = detail.SalesIncTax;
                        decimal _TaxAmount = (detail.TaxPercent / 100) * detail.SalesExcTax;
                        decimal _TotalTaxAmount = _AmountIncTax - _AmountExcTax;

                        quotationVm.SalesQuotationDetails.Add(new ClsSalesQuotationDetailsVm
                        {
                            Quantity= _Quantity,
                            AmountExcTax= _AmountExcTax,
                            AmountIncTax= _AmountIncTax,
                            ItemDetailsId= _ItemDetailsId,
                            ItemId= _ItemId,
                            PriceExcTax= _PriceExcTax,
                            PriceIncTax= _PriceIncTax,
                            Tax= _Tax,
                            TaxId= _TaxId,
                            Discount= 0,
                            UnitCost= _UnitCost,
                            IsActive= true,
                            IsDeleted= false,
                            LotNo= "",
                            PriceAddedFor= 4,
                            SalesIncTax= _SalesIncTax,
                            FreeQuantity=0,
                            TaxAmount= _TaxAmount,
                            TotalTaxAmount = _TotalTaxAmount,
                            DiscountType= "fixed",
                            OtherInfo="",
                            LotId=0,
                            LotType="",
                            WarrantyId=0,
                            UnitAddedFor=1,
                            ExtraDiscount=0
                        });
                    }
                }

                quotationVm.TotalQuantity = items.Sum(i => i.Quantity);
                quotationVm.Subtotal = items.Sum(i => i.Amount);
                quotationVm.TaxableAmount = quotationVm.Subtotal;
                quotationVm.GrandTotal = quotationVm.Subtotal;
                quotationVm.NetAmount = quotationVm.Subtotal;
                quotationVm.Balance = quotationVm.Subtotal;

                WebApi.SalesQuotationController salesQuotationController = new WebApi.SalesQuotationController();
                var actionResult = await salesQuotationController.InsertSalesQuotation(quotationVm);
                var response = await oCommonController.ExtractResponseFromActionResult(actionResult);

                if (response == null)
                {
                    result.Message = "Unable to create sales quotation.";
                    return result;
                }

                if (response.Status != 1)
                {
                    result.Status = response.Status;
                    result.Message = string.IsNullOrWhiteSpace(response.Message) ? "Unable to create sales quotation." : response.Message;
                    if (response.Errors != null && response.Errors.Count > 0)
                    {
                        result.Errors = response.Errors;
                    }
                    return result;
                }

                var quotationData = response.Data != null ? response.Data.SalesQuotation : null;
                result.Success = true;
                result.Status = 1;
                result.InvoiceNo = quotationVm.InvoiceNo;
                result.SalesQuotationId = quotationData != null ? quotationData.SalesQuotationId : 0;
                result.InvoiceId = quotationData != null ? quotationData.InvoiceId : null;
                result.Message = string.IsNullOrWhiteSpace(response.Message) ? "Sales quotation created successfully." : response.Message;

                if (mode == 3)
                {
                    result.WhatsappUrl = BuildWhatsappUrl(items, customer, branchMeta?.WhatsappDigits, result.InvoiceNo);
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Status = 0;
                result.Message = "Unable to create sales quotation.";
                result.Errors.Add(new ClsError { Message = ex.Message });
                return result;
            }
        }

        private string BuildWhatsappUrl(IEnumerable<NormalizedQuotationItem> items, PublicQuotationCustomer customer, string whatsappDigits, string invoiceNo)
        {
            if (string.IsNullOrWhiteSpace(whatsappDigits) || items == null || customer == null)
            {
                return null;
            }

            var itemList = items.ToList();
            if (itemList.Count == 0)
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.AppendLine(!string.IsNullOrWhiteSpace(invoiceNo) ? "Quotation #" + invoiceNo : "Quotation request");
            sb.AppendLine("----------------");
            decimal total = 0m;
            int index = 1;
            foreach (var item in itemList)
            {
                sb.Append(index).Append(". ").Append(string.IsNullOrWhiteSpace(item.Name) ? "Item" : item.Name);
                sb.Append(" — Qty ").Append(item.Quantity.ToString("0.##"));
                if (item.DisplayPrice > 0)
                {
                    sb.Append(" @ ₹").Append(item.DisplayPrice.ToString("0.##"));
                    total += item.DisplayPrice * item.Quantity;
                }
                sb.AppendLine();
                index++;
            }

            if (itemList.Any(i => i.DisplayPrice > 0))
            {
                sb.AppendLine("Total: ₹" + total.ToString("0.##"));
            }

            sb.AppendLine();
            sb.AppendLine("Customer Details");
            if (!string.IsNullOrWhiteSpace(customer.Name))
            {
                sb.AppendLine("Name: " + customer.Name.Trim());
            }

            var mobileDigits = NormalizeDigits(!string.IsNullOrWhiteSpace(customer.Mobile) ? customer.Mobile : customer.MobileRaw);
            if (!string.IsNullOrWhiteSpace(mobileDigits))
            {
                sb.AppendLine("Mobile: " + mobileDigits);
            }

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                sb.AppendLine("Email: " + customer.Email.Trim());
            }

            var billingAddress = FormatAddress(customer.Billing);
            if (!string.IsNullOrWhiteSpace(billingAddress))
            {
                sb.AppendLine();
                sb.AppendLine("Billing Address:");
                sb.AppendLine(billingAddress);
            }

            if (customer.IsShippingDifferent && customer.Shipping != null)
            {
                var shippingAddress = FormatAddress(customer.Shipping);
                if (!string.IsNullOrWhiteSpace(shippingAddress))
                {
                    sb.AppendLine();
                    sb.AppendLine("Shipping Address:");
                    sb.AppendLine(shippingAddress);
                }
            }

            var message = sb.ToString().Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            return "https://wa.me/" + whatsappDigits + "?text=" + HttpUtility.UrlEncode(message);
        }

        private static string FormatAddress(PublicQuotationAddress address)
        {
            if (address == null)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(address.Address))
            {
                parts.Add(address.Address.Trim());
            }
            if (!string.IsNullOrWhiteSpace(address.Landmark))
            {
                parts.Add(address.Landmark.Trim());
            }

            var locality = new List<string>();
            if (!string.IsNullOrWhiteSpace(address.City))
            {
                locality.Add(address.City.Trim());
            }
            if (!string.IsNullOrWhiteSpace(address.State))
            {
                locality.Add(address.State.Trim());
            }
            if (!string.IsNullOrWhiteSpace(address.Country))
            {
                locality.Add(address.Country.Trim());
            }
            if (locality.Count > 0)
            {
                parts.Add(string.Join(", ", locality));
            }
            if (!string.IsNullOrWhiteSpace(address.Pincode))
            {
                parts.Add("PIN: " + address.Pincode.Trim());
            }

            return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private class BranchMeta
        {
            public long BranchId { get; set; }
            public long StateId { get; set; }
            public long CountryId { get; set; }
            public string WhatsappDigits { get; set; }
        }

        private class NormalizedQuotationItem
        {
            public long ItemId { get; set; }
            public long ItemDetailsId { get; set; }
            public string Code { get; set; }
            public string Sku { get; set; }
            public string Name { get; set; }
            public decimal Quantity { get; set; }
            public decimal UnitCost { get; set; }
            public decimal PriceIncTax { get; set; }
            public decimal Amount { get; set; }
            public decimal DisplayPrice { get; set; }
        }

        private class ItemDetailSnapshot
        {
            public dynamic ItemDetail { get; set; }
            public long ItemId { get; set; }
            public decimal SalesExcTax { get; set; }
            public decimal SalesIncTax { get; set; }
            public long TaxId { get; set; }
            public int PriceAddedFor { get; set; }
            public string SKU { get; set; }
            public string ItemName { get; set; }
        }

        private class NormalizeItemsResult
        {
            public bool Success { get; set; }
            public int Status { get; set; } = 1;
            public string Message { get; set; }
            public List<ClsError> Errors { get; set; } = new List<ClsError>();
            public List<NormalizedQuotationItem> Items { get; set; } = new List<NormalizedQuotationItem>();
        }

        private class CreateCustomerResult
        {
            public bool Success { get; set; }
            public int Status { get; set; }
            public string Message { get; set; }
            public List<ClsError> Errors { get; set; } = new List<ClsError>();
            public long CustomerId { get; set; }
        }

        private class QuotationOperationResult
        {
            public bool Success { get; set; }
            public int Status { get; set; }
            public string Message { get; set; }
            public List<ClsError> Errors { get; set; } = new List<ClsError>();
            public long SalesQuotationId { get; set; }
            public string InvoiceNo { get; set; }
            public string InvoiceId { get; set; }
            public string WhatsappUrl { get; set; }
        }

        private static string ConvertAttributeMappingsToJson(List<ClsItemDetailsVariationMapVm> attributeMappings)
        {
            if (attributeMappings == null)
            {
                // System.Diagnostics.Debug.WriteLine("[Attributes] AttributeMappings is null");
                return "{}";
            }

            if (attributeMappings.Count == 0)
            {
                // System.Diagnostics.Debug.WriteLine("[Attributes] AttributeMappings is empty");
                return "{}";
            }

            // Group by VariationName (attribute name) and collect VariationDetailsName (attribute values)
            var attributesDict = new Dictionary<string, List<string>>();
            
            foreach (var mapping in attributeMappings)
            {
                if (mapping == null || string.IsNullOrWhiteSpace(mapping.VariationName))
                {
                    continue;
                }

                var attributeName = mapping.VariationName.Trim();
                var attributeValue = mapping.VariationDetailsName ?? string.Empty;

                if (!attributesDict.ContainsKey(attributeName))
                {
                    attributesDict[attributeName] = new List<string>();
                }

                // Add value if not already present (avoid duplicates)
                if (!string.IsNullOrWhiteSpace(attributeValue) && !attributesDict[attributeName].Contains(attributeValue))
                {
                    attributesDict[attributeName].Add(attributeValue.Trim());
                }
            }

            if (attributesDict.Count == 0)
            {
                // System.Diagnostics.Debug.WriteLine("[Attributes] No valid attributes found after processing");
                return "{}";
            }

            // Convert to JSON format: { "Color": ["Red", "Blue"], "Size": ["Small", "Large"] }
            var serializer = new JavaScriptSerializer();
            var result = serializer.Serialize(attributesDict);
            // System.Diagnostics.Debug.WriteLine($"[Attributes] Converted to JSON: {result}");
            return result;
        }

        private static PublicCatalogueProductVm MapProduct(object item)
        {
            if (item == null)
            {
                return null;
            }

            // Check if item is ClsItemDetailsVm for direct property access
            var itemDetailsVm = item as ClsItemDetailsVm;
            string productType;
            bool isVariable;

            if (itemDetailsVm != null)
            {
                productType = itemDetailsVm.ProductType ?? "Single";
                isVariable = productType.ToLower() == "variable";

                var vm = new PublicCatalogueProductVm
                {
                    ItemId = itemDetailsVm.ItemId,
                    ItemDetailsId = itemDetailsVm.ItemDetailsId,
                    ItemName = itemDetailsVm.ItemName ?? string.Empty,
                    Description = itemDetailsVm.Description ?? string.Empty,
                    SKU = itemDetailsVm.SKU,
                    ProductCode = itemDetailsVm.ItemCode,
                    ProductImage = itemDetailsVm.ProductImage,
                    SalesIncTax = itemDetailsVm.SalesIncTax,
                    SalesExcTax = itemDetailsVm.SalesExcTax,
                    DefaultMrp = itemDetailsVm.DefaultMrp,
                    DiscountPercent = itemDetailsVm.DiscountPercent,
                    Quantity = itemDetailsVm.Quantity,
                    AttributesJson = ConvertAttributeMappingsToJson(itemDetailsVm.AttributeMappings),
                    VariationName = itemDetailsVm.VariationName ?? string.Empty,
                    ProductType = productType,
                    Variants = new List<PublicCatalogueProductVariantVm>()
                };

                // Handle variants if variable product
                if (isVariable && itemDetailsVm.Variants != null && itemDetailsVm.Variants.Any())
                {
                    foreach (var variant in itemDetailsVm.Variants)
                    {
                        if (variant == null) continue;
                        
                        var variantVm = new PublicCatalogueProductVariantVm
                        {
                            ItemDetailsId = variant.ItemDetailsId,
                            VariationName = variant.VariationName ?? string.Empty,
                            ProductImage = variant.ProductImage ?? vm.ProductImage,
                            SalesIncTax = variant.SalesIncTax,
                            DefaultMrp = variant.DefaultMrp,
                            Quantity = variant.Quantity,
                            SKU = variant.SKU ?? string.Empty,
                            AttributesJson = ConvertAttributeMappingsToJson(variant.AttributeMappings)
                        };
                        vm.Variants.Add(variantVm);
                    }

                    // Set price range and use first variant as default
                    if (vm.Variants.Any())
                    {
                        vm.MinPrice = vm.Variants.Min(v => v.SalesIncTax);
                        vm.MaxPrice = vm.Variants.Max(v => v.SalesIncTax);
                        
                    // Use first variant as default display
                    var firstVariant = vm.Variants.First();
                    vm.ItemDetailsId = firstVariant.ItemDetailsId;
                    vm.ProductImage = firstVariant.ProductImage;
                    vm.SalesIncTax = firstVariant.SalesIncTax;
                    vm.DefaultMrp = firstVariant.DefaultMrp;
                    vm.Quantity = firstVariant.Quantity;
                    vm.VariationName = firstVariant.VariationName;
                    vm.SKU = firstVariant.SKU;
                    vm.AttributesJson = firstVariant.AttributesJson;
                    }
                }

                if (vm.SalesIncTax <= 0m)
                {
                    vm.SalesIncTax = vm.SalesExcTax;
                }

                if (vm.Quantity <= 0m)
                {
                    var stock = ToDecimal(GetDynamicValue(item, "CurrentStock"));
                    if (stock > 0m)
                    {
                        vm.Quantity = stock;
                    }
                }

                if (string.IsNullOrWhiteSpace(vm.SKU))
                {
                    vm.SKU = vm.ProductCode;
                }

                if (string.IsNullOrWhiteSpace(vm.AttributesJson))
                {
                    vm.AttributesJson = "{}";
                }

                return vm;
            }

            // Fallback to dynamic access for non-ClsItemDetailsVm objects
            productType = Convert.ToString(GetDynamicValue(item, "ProductType")) ?? "Single";
            isVariable = productType.ToLower() == "variable";

            var vmFallback = new PublicCatalogueProductVm
            {
                ItemId = ToLong(GetDynamicValue(item, "ItemId")),
                ItemDetailsId = ToLong(GetDynamicValue(item, "ItemDetailsId")),
                ItemName = Convert.ToString(GetDynamicValue(item, "ItemName")) ?? string.Empty,
                Description = Convert.ToString(GetDynamicValue(item, "Description")) ?? string.Empty,
                SKU = Convert.ToString(GetDynamicValue(item, "SKU")),
                ProductCode = Convert.ToString(GetDynamicValue(item, "ItemCode")),
                ProductImage = Convert.ToString(GetDynamicValue(item, "ProductImage")),
                SalesIncTax = ToDecimal(GetDynamicValue(item, "SalesIncTax")),
                SalesExcTax = ToDecimal(GetDynamicValue(item, "SalesExcTax")),
                DefaultMrp = ToDecimal(GetDynamicValue(item, "DefaultMrp")),
                DiscountPercent = ToDecimal(GetDynamicValue(item, "DiscountPercent")),
                Quantity = ToDecimal(GetDynamicValue(item, "Quantity")),
                AttributesJson = Convert.ToString(GetDynamicValue(item, "AttributesJson")),
                VariationName = Convert.ToString(GetDynamicValue(item, "VariationName")) ?? string.Empty,
                ProductType = productType,
                Variants = new List<PublicCatalogueProductVariantVm>()
            };

            // Try to get AttributeMappings if AttributesJson is empty
            if (string.IsNullOrWhiteSpace(vmFallback.AttributesJson) || vmFallback.AttributesJson == "{}")
            {
                var attributeMappings = GetDynamicValue(item, "AttributeMappings");
                if (attributeMappings is List<ClsItemDetailsVariationMapVm> mappings)
                {
                    vmFallback.AttributesJson = ConvertAttributeMappingsToJson(mappings);
                }
            }

            // Handle variants if variable product (fallback path)
            if (isVariable)
            {
                // Fallback: Try dynamic access for backward compatibility
                var variantsData = GetDynamicValue(item, "Variants") ?? GetDynamicValue(item, "variants");
                if (variantsData != null)
                {
                    // Handle different deserialized formats (array, list, enumerable)
                    IEnumerable variantsList = null;
                    
                    if (variantsData is IEnumerable enumerable && !(variantsData is string))
                    {
                        variantsList = enumerable;
                    }
                    
                    if (variantsList != null)
                    {
                        foreach (var variant in variantsList)
                        {
                            if (variant == null) continue;
                            
                            var variantAttrsJson = Convert.ToString(GetDynamicValue(variant, "AttributesJson")) ?? "{}";
                            
                            // Try to get AttributeMappings if AttributesJson is empty
                            if (string.IsNullOrWhiteSpace(variantAttrsJson) || variantAttrsJson == "{}")
                            {
                                var variantAttributeMappings = GetDynamicValue(variant, "AttributeMappings");
                                if (variantAttributeMappings is List<ClsItemDetailsVariationMapVm> variantMappings)
                                {
                                    variantAttrsJson = ConvertAttributeMappingsToJson(variantMappings);
                                }
                            }
                            
                            var variantVm = new PublicCatalogueProductVariantVm
                            {
                                ItemDetailsId = ToLong(GetDynamicValue(variant, "ItemDetailsId")),
                                VariationName = Convert.ToString(GetDynamicValue(variant, "VariationName")) ?? string.Empty,
                                ProductImage = Convert.ToString(GetDynamicValue(variant, "ProductImage")) ?? vmFallback.ProductImage,
                                SalesIncTax = ToDecimal(GetDynamicValue(variant, "SalesIncTax")),
                                DefaultMrp = ToDecimal(GetDynamicValue(variant, "DefaultMrp")),
                                Quantity = ToDecimal(GetDynamicValue(variant, "Quantity")),
                                SKU = Convert.ToString(GetDynamicValue(variant, "SKU")),
                                AttributesJson = variantAttrsJson
                            };
                            vmFallback.Variants.Add(variantVm);
                        }
                    }
                }

                // Set price range and use first variant as default
                if (vmFallback.Variants.Any())
                {
                    vmFallback.MinPrice = vmFallback.Variants.Min(v => v.SalesIncTax);
                    vmFallback.MaxPrice = vmFallback.Variants.Max(v => v.SalesIncTax);
                    
                    // Use first variant as default display
                    var firstVariant = vmFallback.Variants.First();
                    vmFallback.ItemDetailsId = firstVariant.ItemDetailsId;
                    vmFallback.ProductImage = firstVariant.ProductImage;
                    vmFallback.SalesIncTax = firstVariant.SalesIncTax;
                    vmFallback.DefaultMrp = firstVariant.DefaultMrp;
                    vmFallback.Quantity = firstVariant.Quantity;
                    vmFallback.VariationName = firstVariant.VariationName;
                    vmFallback.SKU = firstVariant.SKU;
                    vmFallback.AttributesJson = firstVariant.AttributesJson;
                }
            }

            if (vmFallback.SalesIncTax <= 0m)
            {
                var fallbackPrice = ToDecimal(GetDynamicValue(item, "Price"));
                if (fallbackPrice > 0m)
                {
                    vmFallback.SalesIncTax = fallbackPrice;
                }
            }

            if (vmFallback.SalesIncTax <= 0m)
            {
                vmFallback.SalesIncTax = vmFallback.SalesExcTax;
            }

            if (vmFallback.Quantity <= 0m)
            {
                var stock = ToDecimal(GetDynamicValue(item, "CurrentStock"));
                if (stock > 0m)
                {
                    vmFallback.Quantity = stock;
                }
            }

            if (string.IsNullOrWhiteSpace(vmFallback.SKU))
            {
                vmFallback.SKU = vmFallback.ProductCode;
            }

            if (string.IsNullOrWhiteSpace(vmFallback.AttributesJson))
            {
                vmFallback.AttributesJson = "{}";
            }

            return vmFallback;
        }

        private static int ToInt(object value, int defaultValue = 0)
        {
            if (value == null)
            {
                return defaultValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                if (longValue > int.MaxValue)
                {
                    return int.MaxValue;
                }
                if (longValue < int.MinValue)
                {
                    return int.MinValue;
                }
                return (int)longValue;
            }

            if (value is short shortValue)
            {
                return shortValue;
            }

            int parsedInt;
            return int.TryParse(Convert.ToString(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedInt)
                ? parsedInt
                : defaultValue;
        }

        private PublicCatalogueProductGridViewModel BuildProductGridModel(dynamic catalogue, IEnumerable items, int pageIndex = 1, int pageSize = 0, int totalCount = 0)
        {
            var model = new PublicCatalogueProductGridViewModel
            {
                ShowPrices = ToBoolean(GetDynamicValue(catalogue, "ShowPrices")),
                ShowMRP = ToBoolean(GetDynamicValue(catalogue, "ShowMRP")),
                ShowDiscount = ToBoolean(GetDynamicValue(catalogue, "ShowDiscount")),
                ShowStock = ToBoolean(GetDynamicValue(catalogue, "ShowStock")),
                ShowOutOfStock = ToBoolean(GetDynamicValue(catalogue, "ShowOutOfStock")),
                ShowProductCode = ToBoolean(GetDynamicValue(catalogue, "ShowProductCode")),
                ShowImages = ToBoolean(GetDynamicValue(catalogue, "ShowImages"), true),
                PageIndex = pageIndex > 0 ? pageIndex : 1,
                PageSize = pageSize > 0 ? pageSize : 0,
                TotalCount = totalCount > 0 ? totalCount : 0
            };

            if (items != null)
            {
                foreach (var item in items)
                {
                    var product = MapProduct(item);
                    if (product != null)
                    {
                        model.Products.Add(product);
                    }
                }
            }

            if (model.PageSize > 0 && model.TotalCount > 0)
            {
                var consumed = model.PageIndex * model.PageSize;
                model.HasMore = consumed < model.TotalCount;
            }
            else
            {
                model.HasMore = model.Products.Count > 0 && model.Products.Count == model.PageSize && model.PageSize > 0;
            }

            model.NextPageIndex = model.HasMore ? model.PageIndex + 1 : model.PageIndex;

            return model;
        }

        private static PublicCatalogueCategoryVm MapCategoryFromDto(PublicCatalogueCategoryDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            var categoryName = (dto.Category ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return null;
            }

            var subCategories = dto.SubCategorys != null
                ? dto.SubCategorys
                    .Select(MapSubCategoryFromDto)
                    .Where(subCategory => subCategory != null)
                    .ToList()
                : new List<PublicCatalogueSubCategoryVm>();

            return new PublicCatalogueCategoryVm
            {
                CategoryId = dto.CategoryId,
                CategoryName = categoryName,
                SubCategories = subCategories ?? new List<PublicCatalogueSubCategoryVm>()
            };
        }

        private static PublicCatalogueSubCategoryVm MapSubCategoryFromDto(PublicCatalogueSubCategoryDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            var subCategoryName = (dto.SubCategory ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(subCategoryName))
            {
                return null;
            }

            var subSubCategories = dto.SubSubCategorys != null
                ? dto.SubSubCategorys
                    .Select(MapSubSubCategoryFromDto)
                    .Where(subSubCategory => subSubCategory != null)
                    .ToList()
                : new List<PublicCatalogueSubSubCategoryVm>();

            return new PublicCatalogueSubCategoryVm
            {
                SubCategoryId = dto.SubCategoryId,
                SubCategoryName = subCategoryName,
                SubSubCategories = subSubCategories ?? new List<PublicCatalogueSubSubCategoryVm>()
            };
        }

        private static PublicCatalogueSubSubCategoryVm MapSubSubCategoryFromDto(PublicCatalogueSubSubCategoryDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            var name = (dto.SubSubCategory ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return new PublicCatalogueSubSubCategoryVm
            {
                SubSubCategoryId = dto.SubSubCategoryId,
                SubSubCategoryName = name
            };
        }

    }

    internal class PublicCatalogueCategoryDto
    {
        public long? CategoryId { get; set; }
        public string Category { get; set; }
        public List<PublicCatalogueSubCategoryDto> SubCategorys { get; set; }
    }

    internal class PublicCatalogueSubCategoryDto
    {
        public long? SubCategoryId { get; set; }
        public string SubCategory { get; set; }
        public List<PublicCatalogueSubSubCategoryDto> SubSubCategorys { get; set; }
    }

    internal class PublicCatalogueSubSubCategoryDto
    {
        public long? SubSubCategoryId { get; set; }
        public string SubSubCategory { get; set; }
    }

    public class PublicCustomerLookupRequest
    {
        public string Slug { get; set; }
        public string MobileNo { get; set; }
    }

    public class PublicQuotationSubmitRequest
    {
        public string Slug { get; set; }
        public int Mode { get; set; }
        public long PlaceOfSupplyId { get; set; }
        public List<PublicQuotationItem> Items { get; set; }
        public PublicQuotationCustomer Customer { get; set; }
    }

    public class PublicQuotationItem
    {
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public string Code { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Mrp { get; set; }
    }

    public class PublicQuotationCustomer
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string MobileRaw { get; set; }
        public string Email { get; set; }
        public bool IsShippingDifferent { get; set; }
        public PublicQuotationAddress Billing { get; set; }
        public PublicQuotationAddress Shipping { get; set; }
    }

    public class PublicQuotationAddress
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string AltMobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Landmark { get; set; }
        public string Pincode { get; set; }
        public long CountryId { get; set; }
        public string Country { get; set; }
        public long StateId { get; set; }
        public string State { get; set; }
        public long CityId { get; set; }
        public string City { get; set; }
    }
}

