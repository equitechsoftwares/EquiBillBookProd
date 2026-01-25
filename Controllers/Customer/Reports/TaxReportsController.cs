using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Customer.Reports
{
    [AuthorizationPrivilegeFilter]
    public class TaxReportsController : Controller
    {
        // GET: TaxReports
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        #region Gstr1
        public async Task<ActionResult> Gstr1()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            var b2cLargeResult = await taxController.B2CLarge(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(b2cLargeResult);

            var b2cSmallResult = await taxController.B2CSmall(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(b2cSmallResult);

            var exportsResult = await taxController.Exports(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(exportsResult);

            var creditDebitRegisteredResult = await taxController.CreditDebitRegistered(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(creditDebitRegisteredResult);

            var creditDebitUnRegisteredResult = await taxController.CreditDebitUnRegistered(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(creditDebitUnRegisteredResult);

            var advancesReceivedResult = await taxController.AdvancesReceived(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(advancesReceivedResult);

            var adjustmentOfAdvancesResult = await taxController.AdjustmentOfAdvances(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(adjustmentOfAdvancesResult);

            var hsnWiseSummaryResult = await taxController.HsnWiseSummary(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(hsnWiseSummaryResult);

            var nilRatedResult = await taxController.NilRated(salesObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(nilRatedResult);

            var summaryOfDocumentsResult = await taxController.SummaryOfDocuments(salesObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(summaryOfDocumentsResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            TaxSettingController taxSettingController = new TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            // B2B
            ViewBag.B2BTotalCount = oClsResponse.Data.TotalCount;
            ViewBag.B2BTotalInvoiceValue = oClsResponse.Data.TotalInvoiceValue;
            ViewBag.B2BTotalTaxableValue = oClsResponse.Data.TotalTaxableValue;
            ViewBag.B2BTotalCGST = oClsResponse.Data.TotalCgstValue;
            ViewBag.B2BTotalSGST = oClsResponse.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.B2BTotalIGST = oClsResponse.Data.TotalIgstValue;
            ViewBag.B2BTotalCESS = oClsResponse.Data.TotalCessValue;
            // B2B

            // B2CLarge
            ViewBag.B2CLargeTotalCount = oClsResponse1.Data.TotalCount;
            ViewBag.B2CLargeTotalInvoiceValue = oClsResponse1.Data.TotalInvoiceValue;
            ViewBag.B2CLargeTotalTaxableValue = oClsResponse1.Data.TotalTaxableValue;
            ViewBag.B2CLargeTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.B2CLargeTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse1.Data.TotalUtgstValue;
            ViewBag.B2CLargeTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.B2CLargeTotalCESS = oClsResponse1.Data.TotalCessValue;
            // B2CLarge

            // B2CSmall
            ViewBag.B2CSmallTotalCount = oClsResponse2.Data.TotalCount;
            ViewBag.B2CSmallTotalInvoiceValue = oClsResponse2.Data.TotalInvoiceValue;
            ViewBag.B2CSmallTotalTaxableValue = oClsResponse2.Data.TotalTaxableValue;
            ViewBag.B2CSmallTotalCGST = oClsResponse2.Data.TotalCgstValue;
            ViewBag.B2CSmallTotalSGST = oClsResponse2.Data.TotalSgstValue + oClsResponse2.Data.TotalUtgstValue;
            ViewBag.B2CSmallTotalIGST = oClsResponse2.Data.TotalIgstValue;
            ViewBag.B2CSmallTotalCESS = oClsResponse2.Data.TotalCessValue;
            // B2CSmall

            // Exports
            ViewBag.ExportsTotalCount = oClsResponse3.Data.TotalCount;
            ViewBag.ExportsTotalInvoiceValue = oClsResponse3.Data.TotalInvoiceValue;
            ViewBag.ExportsTotalTaxableValue = oClsResponse3.Data.TotalTaxableValue;
            ViewBag.ExportsTotalCGST = oClsResponse3.Data.TotalCgstValue;
            ViewBag.ExportsTotalSGST = oClsResponse3.Data.TotalSgstValue + oClsResponse3.Data.TotalUtgstValue;
            ViewBag.ExportsTotalIGST = oClsResponse3.Data.TotalIgstValue;
            ViewBag.ExportsTotalCESS = oClsResponse3.Data.TotalCessValue;
            // Exports

            // CreditDebitRegistered
            ViewBag.CreditDebitRegisteredTotalCount = oClsResponse4.Data.TotalCount;
            ViewBag.CreditDebitRegisteredTotalInvoiceValue = oClsResponse4.Data.TotalInvoiceValue;
            ViewBag.CreditDebitRegisteredTotalTaxableValue = oClsResponse4.Data.TotalTaxableValue;
            ViewBag.CreditDebitRegisteredTotalCGST = oClsResponse4.Data.TotalCgstValue;
            ViewBag.CreditDebitRegisteredTotalSGST = oClsResponse4.Data.TotalSgstValue + oClsResponse4.Data.TotalUtgstValue;
            ViewBag.CreditDebitRegisteredTotalIGST = oClsResponse4.Data.TotalIgstValue;
            ViewBag.CreditDebitRegisteredTotalCESS = oClsResponse4.Data.TotalCessValue;
            // CreditDebitRegistered

            // CreditDebitUnRegistered
            ViewBag.CreditDebitUnRegisteredTotalCount = oClsResponse5.Data.TotalCount;
            ViewBag.CreditDebitUnRegisteredTotalInvoiceValue = oClsResponse5.Data.TotalInvoiceValue;
            ViewBag.CreditDebitUnRegisteredTotalTaxableValue = oClsResponse5.Data.TotalTaxableValue;
            ViewBag.CreditDebitUnRegisteredTotalCGST = oClsResponse5.Data.TotalCgstValue;
            ViewBag.CreditDebitUnRegisteredTotalSGST = oClsResponse5.Data.TotalSgstValue + oClsResponse5.Data.TotalUtgstValue;
            ViewBag.CreditDebitUnRegisteredTotalIGST = oClsResponse5.Data.TotalIgstValue;
            ViewBag.CreditDebitUnRegisteredTotalCESS = oClsResponse5.Data.TotalCessValue;
            // CreditDebitUnRegistered

            // AdvancesReceived
            ViewBag.AdvancesReceivedTotalCount = oClsResponse6.Data.TotalCount;
            ViewBag.AdvancesReceivedTotalInvoiceValue = oClsResponse6.Data.TotalInvoiceValue;
            ViewBag.AdvancesReceivedTotalTaxableValue = oClsResponse6.Data.TotalTaxableValue;
            ViewBag.AdvancesReceivedTotalCGST = oClsResponse6.Data.TotalCgstValue;
            ViewBag.AdvancesReceivedTotalSGST = oClsResponse6.Data.TotalSgstValue + oClsResponse6.Data.TotalUtgstValue;
            ViewBag.AdvancesReceivedTotalIGST = oClsResponse6.Data.TotalIgstValue;
            ViewBag.AdvancesReceivedTotalCESS = oClsResponse6.Data.TotalCessValue;
            // AdvancesReceived

            // AdjustmentOfAdvances
            ViewBag.AdjustmentOfAdvancesTotalCount = oClsResponse7.Data.TotalCount;
            ViewBag.AdjustmentOfAdvancesTotalInvoiceValue = oClsResponse7.Data.TotalInvoiceValue;
            ViewBag.AdjustmentOfAdvancesTotalTaxableValue = oClsResponse7.Data.TotalTaxableValue;
            ViewBag.AdjustmentOfAdvancesTotalCGST = oClsResponse7.Data.TotalCgstValue;
            ViewBag.AdjustmentOfAdvancesTotalSGST = oClsResponse7.Data.TotalSgstValue + oClsResponse7.Data.TotalUtgstValue;
            ViewBag.AdjustmentOfAdvancesTotalIGST = oClsResponse7.Data.TotalIgstValue;
            ViewBag.AdjustmentOfAdvancesTotalCESS = oClsResponse7.Data.TotalCessValue;
            // AdjustmentOfAdvances

            // HsnWiseSummary
            ViewBag.HsnWiseSummaryTotalCount = oClsResponse8.Data.TotalCount;
            ViewBag.HsnWiseSummaryTotalInvoiceValue = oClsResponse8.Data.TotalInvoiceValue;
            ViewBag.HsnWiseSummaryTotalTaxableValue = oClsResponse8.Data.TotalTaxableValue;
            ViewBag.HsnWiseSummaryTotalCGST = oClsResponse8.Data.TotalCgstValue;
            ViewBag.HsnWiseSummaryTotalSGST = oClsResponse8.Data.TotalSgstValue + oClsResponse8.Data.TotalUtgstValue;
            ViewBag.HsnWiseSummaryTotalIGST = oClsResponse8.Data.TotalIgstValue;
            ViewBag.HsnWiseSummaryTotalCESS = oClsResponse8.Data.TotalCessValue;
            // HsnWiseSummary

            // NilRated
            ViewBag.NilRatedTotalCount = oClsResponse9.Data.TotalCount;
            ViewBag.NilRatedTotalNilAmount = oClsResponse9.Data.TotalNilAmount;
            ViewBag.NilRatedTotalExemptedAmount = oClsResponse9.Data.TotalExemptedAmount;
            ViewBag.NilRatedTotalNonGstAmount = oClsResponse9.Data.TotalNonGstAmount;
            // NilRated

            // Summary Of Documents
            ViewBag.SummaryOfDocumentsTotalCount = oClsResponse10.Data.TaxDocuments.Select(a=>a.TotalNumber).Sum();
            ViewBag.SummaryOfDocumentsTotalCancelled = oClsResponse10.Data.TaxDocuments.Select(a => a.TotalCancelled).Sum();
            // NilRated

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }

        public async Task<ActionResult> Gstr1Fetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            var b2cLargeResult = await taxController.B2CLarge(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(b2cLargeResult);

            var b2cSmallResult = await taxController.B2CSmall(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(b2cSmallResult);

            var exportsResult = await taxController.Exports(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(exportsResult);

            var creditDebitRegisteredResult = await taxController.CreditDebitRegistered(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(creditDebitRegisteredResult);

            var creditDebitUnRegisteredResult = await taxController.CreditDebitUnRegistered(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(creditDebitUnRegisteredResult);

            var advancesReceivedResult = await taxController.AdvancesReceived(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(advancesReceivedResult);

            var adjustmentOfAdvancesResult = await taxController.AdjustmentOfAdvances(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(adjustmentOfAdvancesResult);

            var hsnWiseSummaryResult = await taxController.HsnWiseSummary(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(hsnWiseSummaryResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            TaxSettingController taxSettingController = new TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr1");
        }
        public async Task<ActionResult> Gstr1ExcelViewFetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            var b2cLargeResult = await taxController.B2CLarge(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(b2cLargeResult);

            var b2cSmallResult = await taxController.B2CSmall(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(b2cSmallResult);

            var exportsResult = await taxController.Exports(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(exportsResult);

            var creditDebitRegisteredResult = await taxController.CreditDebitRegistered(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(creditDebitRegisteredResult);

            var creditDebitUnRegisteredResult = await taxController.CreditDebitUnRegistered(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(creditDebitUnRegisteredResult);

            var advancesReceivedResult = await taxController.AdvancesReceived(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(advancesReceivedResult);

            var adjustmentOfAdvancesResult = await taxController.AdjustmentOfAdvances(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(adjustmentOfAdvancesResult);

            var hsnWiseSummaryResult = await taxController.HsnWiseSummary(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(hsnWiseSummaryResult);

            var nilRatedResult = await taxController.NilRated(salesObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(nilRatedResult);

            var summaryOfDocumentsResult = await taxController.SummaryOfDocuments(salesObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(summaryOfDocumentsResult);

            //var res51 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TaxType/ActiveTaxTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse51 = serializer.Deserialize<ClsResponse>(res51);

            ViewBag.SalesB2B = oClsResponse.Data.Sales;
            ViewBag.SalesB2CL = oClsResponse1.Data.Sales;
            ViewBag.SalesB2CS = oClsResponse2.Data.Sales;
            ViewBag.SalesEXP = oClsResponse3.Data.Sales;
            ViewBag.SalesCDNR = oClsResponse4.Data.Sales;
            ViewBag.SalesCDNUR = oClsResponse5.Data.Sales;
            ViewBag.CustomerPaymentsAT = oClsResponse6.Data.CustomerPayments;
            ViewBag.CustomerPaymentsATADJ = oClsResponse7.Data.CustomerPayments;
            ViewBag.SalesDetailsHSN = oClsResponse8.Data.SalesDetails;
            ViewBag.SalesDetailsEXEMP = oClsResponse9.Data.SalesDetails;
            ViewBag.TaxDocumentsDOCS = oClsResponse10.Data.TaxDocuments;
            //ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;

            return PartialView("PartialGstr1ExcelView");
        }
        public async Task<ActionResult> Gstr1Json(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateGstr1Json(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }

        public async Task<ActionResult> Gstr1B2B(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> B2BJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateB2BJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();
           
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title+".json");
        }
        public async Task<ActionResult> Gstr1B2CLarge(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2cLargeResult = await taxController.B2CLarge(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2cLargeResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> B2CLargeJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateB2CLargeJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }
        public async Task<ActionResult> Gstr1B2CSmall(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2cSmallResult = await taxController.B2CSmall(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2cSmallResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> B2CSmallJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateB2CSmallJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }
        public async Task<ActionResult> Gstr1Exports(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var exportsResult = await taxController.Exports(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(exportsResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> ExportsJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateExportsJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }
        public async Task<ActionResult> Gstr1CDRegistered(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var creditDebitRegisteredResult = await taxController.CreditDebitRegistered(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(creditDebitRegisteredResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> CDRegisteredJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateCreditDebitRegisteredJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }
        public async Task<ActionResult> Gstr1CDUnRegistered(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var creditDebitUnRegisteredResult = await taxController.CreditDebitUnRegistered(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(creditDebitUnRegisteredResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Sales = oClsResponse.Data.Sales;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> CDUnRegisteredJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateCreditDebitUnRegisteredJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }
        public async Task<ActionResult> Gstr1AdvancesReceived(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var advancesReceivedResult = await taxController.AdvancesReceived(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(advancesReceivedResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.CustomerPayments = oClsResponse.Data.CustomerPayments;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> AdvancesReceivedJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateAdvancesReceivedJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }
        public async Task<ActionResult> Gstr1AdjustmentOfAdvances(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var adjustmentOfAdvancesResult = await taxController.AdjustmentOfAdvances(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(adjustmentOfAdvancesResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.CustomerPayments = oClsResponse.Data.CustomerPayments;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> AdjustmentOfAdvancesJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateAdjustmentOfAdvancesJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }
        public async Task<ActionResult> Gstr1HsnWiseSummary(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var hsnWiseSummaryResult = await taxController.HsnWiseSummary(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(hsnWiseSummaryResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> HsnWiseSummaryJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateHsnWiseSummaryJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }
        public async Task<ActionResult> Gstr1NilRated(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var nilRatedResult = await taxController.NilRated(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(nilRatedResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.SalesDetails = oClsResponse.Data.SalesDetails;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> NilRatedJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateNilRatedJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }
        public async Task<ActionResult> Gstr1SummaryOfDocuments(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var summaryOfDocumentsResult = await taxController.SummaryOfDocuments(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(summaryOfDocumentsResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.TaxDocuments = oClsResponse.Data.TaxDocuments;
            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();

            return View();
        }
        public async Task<ActionResult> SummaryOfDocumentsJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateSummaryOfDocumentsJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", obj.Title + ".json");
        }
        #endregion

        public async Task<ActionResult> Gstr2()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            var b2cLargeResult = await taxController.B2CLarge(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(b2cLargeResult);

            var b2cSmallResult = await taxController.B2CSmall(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(b2cSmallResult);

            var exportsResult = await taxController.Exports(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(exportsResult);

            var creditDebitRegisteredResult = await taxController.CreditDebitRegistered(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(creditDebitRegisteredResult);

            var creditDebitUnRegisteredResult = await taxController.CreditDebitUnRegistered(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(creditDebitUnRegisteredResult);

            var advancesReceivedResult = await taxController.AdvancesReceived(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(advancesReceivedResult);

            var adjustmentOfAdvancesResult = await taxController.AdjustmentOfAdvances(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(adjustmentOfAdvancesResult);

            var hsnWiseSummaryResult = await taxController.HsnWiseSummary(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(hsnWiseSummaryResult);

            var nilRatedResult = await taxController.NilRated(salesObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(nilRatedResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            TaxSettingController taxSettingController = new TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            // B2B
            ViewBag.B2BTotalCount = oClsResponse.Data.TotalCount;
            ViewBag.B2BTotalInvoiceValue = oClsResponse.Data.TotalInvoiceValue;
            ViewBag.B2BTotalTaxableValue = oClsResponse.Data.TotalTaxableValue;
            ViewBag.B2BTotalCGST = oClsResponse.Data.TotalCgstValue;
            ViewBag.B2BTotalSGST = oClsResponse.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.B2BTotalIGST = oClsResponse.Data.TotalIgstValue;
            ViewBag.B2BTotalCESS = oClsResponse.Data.TotalCessValue;
            // B2B

            // B2CLarge
            ViewBag.B2CLargeTotalCount = oClsResponse1.Data.TotalCount;
            ViewBag.B2CLargeTotalInvoiceValue = oClsResponse1.Data.TotalInvoiceValue;
            ViewBag.B2CLargeTotalTaxableValue = oClsResponse1.Data.TotalTaxableValue;
            ViewBag.B2CLargeTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.B2CLargeTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse1.Data.TotalUtgstValue;
            ViewBag.B2CLargeTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.B2CLargeTotalCESS = oClsResponse1.Data.TotalCessValue;
            // B2CLarge

            // B2CSmall
            ViewBag.B2CSmallTotalCount = oClsResponse2.Data.TotalCount;
            ViewBag.B2CSmallTotalInvoiceValue = oClsResponse2.Data.TotalInvoiceValue;
            ViewBag.B2CSmallTotalTaxableValue = oClsResponse2.Data.TotalTaxableValue;
            ViewBag.B2CSmallTotalCGST = oClsResponse2.Data.TotalCgstValue;
            ViewBag.B2CSmallTotalSGST = oClsResponse2.Data.TotalSgstValue + oClsResponse2.Data.TotalUtgstValue;
            ViewBag.B2CSmallTotalIGST = oClsResponse2.Data.TotalIgstValue;
            ViewBag.B2CSmallTotalCESS = oClsResponse2.Data.TotalCessValue;
            // B2CSmall

            // Exports
            ViewBag.ExportsTotalCount = oClsResponse3.Data.TotalCount;
            ViewBag.ExportsTotalInvoiceValue = oClsResponse3.Data.TotalInvoiceValue;
            ViewBag.ExportsTotalTaxableValue = oClsResponse3.Data.TotalTaxableValue;
            ViewBag.ExportsTotalCGST = oClsResponse3.Data.TotalCgstValue;
            ViewBag.ExportsTotalSGST = oClsResponse3.Data.TotalSgstValue + oClsResponse3.Data.TotalUtgstValue;
            ViewBag.ExportsTotalIGST = oClsResponse3.Data.TotalIgstValue;
            ViewBag.ExportsTotalCESS = oClsResponse3.Data.TotalCessValue;
            // Exports

            // CreditDebitRegistered
            ViewBag.CreditDebitRegisteredTotalCount = oClsResponse4.Data.TotalCount;
            ViewBag.CreditDebitRegisteredTotalInvoiceValue = oClsResponse4.Data.TotalInvoiceValue;
            ViewBag.CreditDebitRegisteredTotalTaxableValue = oClsResponse4.Data.TotalTaxableValue;
            ViewBag.CreditDebitRegisteredTotalCGST = oClsResponse4.Data.TotalCgstValue;
            ViewBag.CreditDebitRegisteredTotalSGST = oClsResponse4.Data.TotalSgstValue + oClsResponse4.Data.TotalUtgstValue;
            ViewBag.CreditDebitRegisteredTotalIGST = oClsResponse4.Data.TotalIgstValue;
            ViewBag.CreditDebitRegisteredTotalCESS = oClsResponse4.Data.TotalCessValue;
            // CreditDebitRegistered

            // CreditDebitUnRegistered
            ViewBag.CreditDebitUnRegisteredTotalCount = oClsResponse5.Data.TotalCount;
            ViewBag.CreditDebitUnRegisteredTotalInvoiceValue = oClsResponse5.Data.TotalInvoiceValue;
            ViewBag.CreditDebitUnRegisteredTotalTaxableValue = oClsResponse5.Data.TotalTaxableValue;
            ViewBag.CreditDebitUnRegisteredTotalCGST = oClsResponse5.Data.TotalCgstValue;
            ViewBag.CreditDebitUnRegisteredTotalSGST = oClsResponse5.Data.TotalSgstValue + oClsResponse5.Data.TotalUtgstValue;
            ViewBag.CreditDebitUnRegisteredTotalIGST = oClsResponse5.Data.TotalIgstValue;
            ViewBag.CreditDebitUnRegisteredTotalCESS = oClsResponse5.Data.TotalCessValue;
            // CreditDebitUnRegistered

            // AdvancesReceived
            ViewBag.AdvancesReceivedTotalCount = oClsResponse6.Data.TotalCount;
            ViewBag.AdvancesReceivedTotalInvoiceValue = oClsResponse6.Data.TotalInvoiceValue;
            ViewBag.AdvancesReceivedTotalTaxableValue = oClsResponse6.Data.TotalTaxableValue;
            ViewBag.AdvancesReceivedTotalCGST = oClsResponse6.Data.TotalCgstValue;
            ViewBag.AdvancesReceivedTotalSGST = oClsResponse6.Data.TotalSgstValue + oClsResponse6.Data.TotalUtgstValue;
            ViewBag.AdvancesReceivedTotalIGST = oClsResponse6.Data.TotalIgstValue;
            ViewBag.AdvancesReceivedTotalCESS = oClsResponse6.Data.TotalCessValue;
            // AdvancesReceived

            // AdjustmentOfAdvances
            ViewBag.AdjustmentOfAdvancesTotalCount = oClsResponse7.Data.TotalCount;
            ViewBag.AdjustmentOfAdvancesTotalInvoiceValue = oClsResponse7.Data.TotalInvoiceValue;
            ViewBag.AdjustmentOfAdvancesTotalTaxableValue = oClsResponse7.Data.TotalTaxableValue;
            ViewBag.AdjustmentOfAdvancesTotalCGST = oClsResponse7.Data.TotalCgstValue;
            ViewBag.AdjustmentOfAdvancesTotalSGST = oClsResponse7.Data.TotalSgstValue + oClsResponse7.Data.TotalUtgstValue;
            ViewBag.AdjustmentOfAdvancesTotalIGST = oClsResponse7.Data.TotalIgstValue;
            ViewBag.AdjustmentOfAdvancesTotalCESS = oClsResponse7.Data.TotalCessValue;
            // AdjustmentOfAdvances

            // HsnWiseSummary
            ViewBag.HsnWiseSummaryTotalCount = oClsResponse8.Data.TotalCount;
            ViewBag.HsnWiseSummaryTotalInvoiceValue = oClsResponse8.Data.TotalInvoiceValue;
            ViewBag.HsnWiseSummaryTotalTaxableValue = oClsResponse8.Data.TotalTaxableValue;
            ViewBag.HsnWiseSummaryTotalCGST = oClsResponse8.Data.TotalCgstValue;
            ViewBag.HsnWiseSummaryTotalSGST = oClsResponse8.Data.TotalSgstValue + oClsResponse8.Data.TotalUtgstValue;
            ViewBag.HsnWiseSummaryTotalIGST = oClsResponse8.Data.TotalIgstValue;
            ViewBag.HsnWiseSummaryTotalCESS = oClsResponse8.Data.TotalCessValue;
            // HsnWiseSummary

            // NilRated
            ViewBag.NilRatedTotalCount = oClsResponse9.Data.TotalCount;
            ViewBag.NilRatedTotalNilAmount = oClsResponse9.Data.TotalNilAmount;
            ViewBag.NilRatedTotalExemptedAmount = oClsResponse9.Data.TotalExemptedAmount;
            ViewBag.NilRatedTotalNonGstAmount = oClsResponse9.Data.TotalNonGstAmount;
            // NilRated

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }

        public async Task<ActionResult> Gstr2Fetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            var b2cLargeResult = await taxController.B2CLarge(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(b2cLargeResult);

            var b2cSmallResult = await taxController.B2CSmall(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(b2cSmallResult);

            var exportsResult = await taxController.Exports(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(exportsResult);

            var creditDebitRegisteredResult = await taxController.CreditDebitRegistered(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(creditDebitRegisteredResult);

            var creditDebitUnRegisteredResult = await taxController.CreditDebitUnRegistered(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(creditDebitUnRegisteredResult);

            var advancesReceivedResult = await taxController.AdvancesReceived(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(advancesReceivedResult);

            var adjustmentOfAdvancesResult = await taxController.AdjustmentOfAdvances(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(adjustmentOfAdvancesResult);

            var hsnWiseSummaryResult = await taxController.HsnWiseSummary(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(hsnWiseSummaryResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            TaxSettingController taxSettingController = new TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr1");
        }

        public async Task<ActionResult> Gstr2AReconcilation()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            var b2cLargeResult = await taxController.B2CLarge(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(b2cLargeResult);

            var b2cSmallResult = await taxController.B2CSmall(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(b2cSmallResult);

            var exportsResult = await taxController.Exports(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(exportsResult);

            var creditDebitRegisteredResult = await taxController.CreditDebitRegistered(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(creditDebitRegisteredResult);

            var creditDebitUnRegisteredResult = await taxController.CreditDebitUnRegistered(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(creditDebitUnRegisteredResult);

            var advancesReceivedResult = await taxController.AdvancesReceived(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(advancesReceivedResult);

            var adjustmentOfAdvancesResult = await taxController.AdjustmentOfAdvances(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(adjustmentOfAdvancesResult);

            var hsnWiseSummaryResult = await taxController.HsnWiseSummary(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(hsnWiseSummaryResult);

            var nilRatedResult = await taxController.NilRated(salesObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(nilRatedResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            TaxSettingController taxSettingController = new TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            // B2B
            ViewBag.B2BTotalCount = oClsResponse.Data.TotalCount;
            ViewBag.B2BTotalInvoiceValue = oClsResponse.Data.TotalInvoiceValue;
            ViewBag.B2BTotalTaxableValue = oClsResponse.Data.TotalTaxableValue;
            ViewBag.B2BTotalCGST = oClsResponse.Data.TotalCgstValue;
            ViewBag.B2BTotalSGST = oClsResponse.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.B2BTotalIGST = oClsResponse.Data.TotalIgstValue;
            ViewBag.B2BTotalCESS = oClsResponse.Data.TotalCessValue;
            // B2B

            // B2CLarge
            ViewBag.B2CLargeTotalCount = oClsResponse1.Data.TotalCount;
            ViewBag.B2CLargeTotalInvoiceValue = oClsResponse1.Data.TotalInvoiceValue;
            ViewBag.B2CLargeTotalTaxableValue = oClsResponse1.Data.TotalTaxableValue;
            ViewBag.B2CLargeTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.B2CLargeTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse1.Data.TotalUtgstValue;
            ViewBag.B2CLargeTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.B2CLargeTotalCESS = oClsResponse1.Data.TotalCessValue;
            // B2CLarge

            // B2CSmall
            ViewBag.B2CSmallTotalCount = oClsResponse2.Data.TotalCount;
            ViewBag.B2CSmallTotalInvoiceValue = oClsResponse2.Data.TotalInvoiceValue;
            ViewBag.B2CSmallTotalTaxableValue = oClsResponse2.Data.TotalTaxableValue;
            ViewBag.B2CSmallTotalCGST = oClsResponse2.Data.TotalCgstValue;
            ViewBag.B2CSmallTotalSGST = oClsResponse2.Data.TotalSgstValue + oClsResponse2.Data.TotalUtgstValue;
            ViewBag.B2CSmallTotalIGST = oClsResponse2.Data.TotalIgstValue;
            ViewBag.B2CSmallTotalCESS = oClsResponse2.Data.TotalCessValue;
            // B2CSmall

            // Exports
            ViewBag.ExportsTotalCount = oClsResponse3.Data.TotalCount;
            ViewBag.ExportsTotalInvoiceValue = oClsResponse3.Data.TotalInvoiceValue;
            ViewBag.ExportsTotalTaxableValue = oClsResponse3.Data.TotalTaxableValue;
            ViewBag.ExportsTotalCGST = oClsResponse3.Data.TotalCgstValue;
            ViewBag.ExportsTotalSGST = oClsResponse3.Data.TotalSgstValue + oClsResponse3.Data.TotalUtgstValue;
            ViewBag.ExportsTotalIGST = oClsResponse3.Data.TotalIgstValue;
            ViewBag.ExportsTotalCESS = oClsResponse3.Data.TotalCessValue;
            // Exports

            // CreditDebitRegistered
            ViewBag.CreditDebitRegisteredTotalCount = oClsResponse4.Data.TotalCount;
            ViewBag.CreditDebitRegisteredTotalInvoiceValue = oClsResponse4.Data.TotalInvoiceValue;
            ViewBag.CreditDebitRegisteredTotalTaxableValue = oClsResponse4.Data.TotalTaxableValue;
            ViewBag.CreditDebitRegisteredTotalCGST = oClsResponse4.Data.TotalCgstValue;
            ViewBag.CreditDebitRegisteredTotalSGST = oClsResponse4.Data.TotalSgstValue + oClsResponse4.Data.TotalUtgstValue;
            ViewBag.CreditDebitRegisteredTotalIGST = oClsResponse4.Data.TotalIgstValue;
            ViewBag.CreditDebitRegisteredTotalCESS = oClsResponse4.Data.TotalCessValue;
            // CreditDebitRegistered

            // CreditDebitUnRegistered
            ViewBag.CreditDebitUnRegisteredTotalCount = oClsResponse5.Data.TotalCount;
            ViewBag.CreditDebitUnRegisteredTotalInvoiceValue = oClsResponse5.Data.TotalInvoiceValue;
            ViewBag.CreditDebitUnRegisteredTotalTaxableValue = oClsResponse5.Data.TotalTaxableValue;
            ViewBag.CreditDebitUnRegisteredTotalCGST = oClsResponse5.Data.TotalCgstValue;
            ViewBag.CreditDebitUnRegisteredTotalSGST = oClsResponse5.Data.TotalSgstValue + oClsResponse5.Data.TotalUtgstValue;
            ViewBag.CreditDebitUnRegisteredTotalIGST = oClsResponse5.Data.TotalIgstValue;
            ViewBag.CreditDebitUnRegisteredTotalCESS = oClsResponse5.Data.TotalCessValue;
            // CreditDebitUnRegistered

            // AdvancesReceived
            ViewBag.AdvancesReceivedTotalCount = oClsResponse6.Data.TotalCount;
            ViewBag.AdvancesReceivedTotalInvoiceValue = oClsResponse6.Data.TotalInvoiceValue;
            ViewBag.AdvancesReceivedTotalTaxableValue = oClsResponse6.Data.TotalTaxableValue;
            ViewBag.AdvancesReceivedTotalCGST = oClsResponse6.Data.TotalCgstValue;
            ViewBag.AdvancesReceivedTotalSGST = oClsResponse6.Data.TotalSgstValue + oClsResponse6.Data.TotalUtgstValue;
            ViewBag.AdvancesReceivedTotalIGST = oClsResponse6.Data.TotalIgstValue;
            ViewBag.AdvancesReceivedTotalCESS = oClsResponse6.Data.TotalCessValue;
            // AdvancesReceived

            // AdjustmentOfAdvances
            ViewBag.AdjustmentOfAdvancesTotalCount = oClsResponse7.Data.TotalCount;
            ViewBag.AdjustmentOfAdvancesTotalInvoiceValue = oClsResponse7.Data.TotalInvoiceValue;
            ViewBag.AdjustmentOfAdvancesTotalTaxableValue = oClsResponse7.Data.TotalTaxableValue;
            ViewBag.AdjustmentOfAdvancesTotalCGST = oClsResponse7.Data.TotalCgstValue;
            ViewBag.AdjustmentOfAdvancesTotalSGST = oClsResponse7.Data.TotalSgstValue + oClsResponse7.Data.TotalUtgstValue;
            ViewBag.AdjustmentOfAdvancesTotalIGST = oClsResponse7.Data.TotalIgstValue;
            ViewBag.AdjustmentOfAdvancesTotalCESS = oClsResponse7.Data.TotalCessValue;
            // AdjustmentOfAdvances

            // HsnWiseSummary
            ViewBag.HsnWiseSummaryTotalCount = oClsResponse8.Data.TotalCount;
            ViewBag.HsnWiseSummaryTotalInvoiceValue = oClsResponse8.Data.TotalInvoiceValue;
            ViewBag.HsnWiseSummaryTotalTaxableValue = oClsResponse8.Data.TotalTaxableValue;
            ViewBag.HsnWiseSummaryTotalCGST = oClsResponse8.Data.TotalCgstValue;
            ViewBag.HsnWiseSummaryTotalSGST = oClsResponse8.Data.TotalSgstValue + oClsResponse8.Data.TotalUtgstValue;
            ViewBag.HsnWiseSummaryTotalIGST = oClsResponse8.Data.TotalIgstValue;
            ViewBag.HsnWiseSummaryTotalCESS = oClsResponse8.Data.TotalCessValue;
            // HsnWiseSummary

            // NilRated
            ViewBag.NilRatedTotalCount = oClsResponse9.Data.TotalCount;
            ViewBag.NilRatedTotalNilAmount = oClsResponse9.Data.TotalNilAmount;
            ViewBag.NilRatedTotalExemptedAmount = oClsResponse9.Data.TotalExemptedAmount;
            ViewBag.NilRatedTotalNonGstAmount = oClsResponse9.Data.TotalNonGstAmount;
            // NilRated

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }

        public async Task<ActionResult> Gstr2AReconcilationFetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            var b2cLargeResult = await taxController.B2CLarge(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(b2cLargeResult);

            var b2cSmallResult = await taxController.B2CSmall(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(b2cSmallResult);

            var exportsResult = await taxController.Exports(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(exportsResult);

            var creditDebitRegisteredResult = await taxController.CreditDebitRegistered(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(creditDebitRegisteredResult);

            var creditDebitUnRegisteredResult = await taxController.CreditDebitUnRegistered(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(creditDebitUnRegisteredResult);

            var advancesReceivedResult = await taxController.AdvancesReceived(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(advancesReceivedResult);

            var adjustmentOfAdvancesResult = await taxController.AdjustmentOfAdvances(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(adjustmentOfAdvancesResult);

            var hsnWiseSummaryResult = await taxController.HsnWiseSummary(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(hsnWiseSummaryResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BusinessSettingsController businessSettingsController = new BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            BusinessRegistrationNameController businessRegistrationNameController = new BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            TaxSettingController taxSettingController = new TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr2AReconcilation");
        }

        public async Task<ActionResult> Gstr3B()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var outwardTaxableResult = await taxController.OutwardTaxable(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(outwardTaxableResult);

            var outwardTaxableZeroRatedResult = await taxController.OutwardTaxableZeroRated(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(outwardTaxableZeroRatedResult);

            var otherOutwardNilRatedResult = await taxController.OtherOutwardNilRated(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(otherOutwardNilRatedResult);

            var outwardNonGstResult = await taxController.OutwardNonGst(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(outwardNonGstResult);

            var interStateUnregisteredResult = await taxController.InterStateUnregistered(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(interStateUnregisteredResult);

            var interStateCompositionResult = await taxController.InterStateComposition(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(interStateCompositionResult);

            var importOfGoodsResult = await taxController.ImportOfGoods(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(importOfGoodsResult);

            var importOfServicesResult = await taxController.ImportOfServices(salesObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(importOfServicesResult);

            var allOtherItcResult = await taxController.AllOtherItc(salesObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(allOtherItcResult);

            var inwardInterCompositionResult = await taxController.InwardInterComposition(salesObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(inwardInterCompositionResult);

            var inwardIntraCompositionResult = await taxController.InwardIntraComposition(salesObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(inwardIntraCompositionResult);

            var inwardInterNonGstResult = await taxController.InwardInterNonGst(salesObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(inwardInterNonGstResult);

            var inwardIntraNonGstResult = await taxController.InwardIntraNonGst(salesObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(inwardIntraNonGstResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse1.Data.FromDate;
            ViewBag.ToDate = oClsResponse1.Data.ToDate;

            // Outward taxable supplies (other than zero rated, nil rated and exempted)
            ViewBag.OutwardTaxableTotalTaxableValue = oClsResponse1.Data.TotalTaxableValue;
            ViewBag.OutwardTaxableTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.OutwardTaxableTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse1.Data.TotalUtgstValue;
            ViewBag.OutwardTaxableTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.OutwardTaxableTotalCESS = oClsResponse1.Data.TotalCessValue;
            // Outward taxable supplies (other than zero rated, nil rated and exempted)

            //  Outward taxable supplies (zero rated)
            ViewBag.OutwardTaxableZeroRatedotalTaxableValue = oClsResponse2.Data.TotalTaxableValue;
            ViewBag.OutwardTaxableZeroRatedTotalCGST = oClsResponse2.Data.TotalCgstValue;
            ViewBag.OutwardTaxableZeroRatedTotalSGST = oClsResponse2.Data.TotalSgstValue + oClsResponse2.Data.TotalUtgstValue;
            ViewBag.OutwardTaxableZeroRatedTotalIGST = oClsResponse2.Data.TotalIgstValue;
            ViewBag.OutwardTaxableZeroRatedTotalCESS = oClsResponse2.Data.TotalCessValue;
            //  Outward taxable supplies (zero rated)

            //  Other outward supplies (Nil rated, exempted)
            ViewBag.OtherOutwardNilRatedotalTaxableValue = oClsResponse3.Data.TotalTaxableValue;
            ViewBag.OtherOutwardNilRatedTotalCGST = oClsResponse3.Data.TotalCgstValue;
            ViewBag.OtherOutwardNilRatedTotalSGST = oClsResponse3.Data.TotalSgstValue + oClsResponse3.Data.TotalUtgstValue;
            ViewBag.OtherOutwardNilRatedTotalIGST = oClsResponse3.Data.TotalIgstValue;
            ViewBag.OtherOutwardNilRatedTotalCESS = oClsResponse3.Data.TotalCessValue;
            //  Other outward supplies (Nil rated, exempted)

            //  Inward supplies (liable to reverse charge)
            //ViewBag.InwardReverseChargeTotalTaxableValue = oClsResponse4.Data.TotalTaxableValue;
            //ViewBag.InwardReverseChargeTotalCGST = oClsResponse4.Data.TotalCgstValue;
            //ViewBag.InwardReverseChargeTotalSGST = oClsResponse4.Data.TotalSgstValue + oClsResponse4.Data.TotalUtgstValue;
            //ViewBag.InwardReverseChargeTotalIGST = oClsResponse4.Data.TotalIgstValue;
            //ViewBag.InwardReverseChargeTotalCESS = oClsResponse4.Data.TotalCessValue;

            ViewBag.InwardReverseChargeTotalTaxableValue = 0;
            ViewBag.InwardReverseChargeTotalCGST = 0;
            ViewBag.InwardReverseChargeTotalSGST = 0;
            ViewBag.InwardReverseChargeTotalIGST = 0;
            ViewBag.InwardReverseChargeTotalCESS = 0;

            //  Inward supplies (liable to reverse charge)

            //  Non-GST outward supplies
            ViewBag.OutwardNonGstTotalTaxableValue = oClsResponse5.Data.TotalTaxableValue;
            ViewBag.OutwardNonGstTotalCGST = oClsResponse5.Data.TotalCgstValue;
            ViewBag.OutwardNonGstTotalSGST = oClsResponse5.Data.TotalSgstValue + oClsResponse5.Data.TotalUtgstValue;
            ViewBag.OutwardNonGstTotalIGST = oClsResponse5.Data.TotalIgstValue;
            ViewBag.OutwardNonGstTotalCESS = oClsResponse5.Data.TotalCessValue;
            //  Non-GST outward supplies

            //  Supplies made to Unregistered Persons
            ViewBag.InterStateUnregistered = oClsResponse6.Data.Sales.GroupBy(s => new { s.PlaceOfSupply })
            .Select(g => new ClsSalesVm
            {
                PlaceOfSupply = g.Key.PlaceOfSupply,
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TotalIgstValue = g.Select(b => b.TotalIgstValue).Sum(),
            }).ToList();
            //  Supplies made to Unregistered Persons

            //  Supplies made to Composition Taxable Persons
            ViewBag.InterStateComposition = oClsResponse7.Data.Sales.GroupBy(s => new { s.PlaceOfSupply })
            .Select(g => new ClsSalesVm
            {
                PlaceOfSupply = g.Key.PlaceOfSupply,
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TotalIgstValue = g.Select(b => b.TotalIgstValue).Sum(),
            }).ToList();
            //  Supplies made to Composition Taxable Persons

            //  Import of Goods
            ViewBag.ImportOfGoodsTotalCGST = oClsResponse8.Data.TotalCgstValue;
            ViewBag.ImportOfGoodsTotalSGST = oClsResponse8.Data.TotalSgstValue + oClsResponse8.Data.TotalUtgstValue;
            ViewBag.ImportOfGoodsTotalIGST = oClsResponse8.Data.TotalIgstValue;
            ViewBag.ImportOfGoodsTotalCESS = oClsResponse8.Data.TotalCessValue;
            //  Import of Goods

            //  Import of Services
            ViewBag.ImportOfServicesTotalCGST = oClsResponse9.Data.TotalCgstValue;
            ViewBag.ImportOfServicesTotalSGST = oClsResponse9.Data.TotalSgstValue + oClsResponse9.Data.TotalUtgstValue;
            ViewBag.ImportOfServicesTotalIGST = oClsResponse9.Data.TotalIgstValue;
            ViewBag.ImportOfServicesTotalCESS = oClsResponse9.Data.TotalCessValue;
            //  Import of Services

            //  Inward supplies liable to reverse charge
            //ViewBag.InwardLiableToReverseChargeTotalCGST = oClsResponse10.Data.TotalCgstValue;
            //ViewBag.InwardLiableToReverseChargeTotalSGST = oClsResponse10.Data.TotalSgstValue + oClsResponse10.Data.TotalUtgstValue;
            //ViewBag.InwardLiableToReverseChargeTotalIGST = oClsResponse10.Data.TotalIgstValue;
            //ViewBag.InwardLiableToReverseChargeTotalCESS = oClsResponse10.Data.TotalCessValue;

            ViewBag.InwardLiableToReverseChargeTotalCGST = 0;
            ViewBag.InwardLiableToReverseChargeTotalSGST = 0;
            ViewBag.InwardLiableToReverseChargeTotalIGST = 0;
            ViewBag.InwardLiableToReverseChargeTotalCESS = 0;

            //  Inward supplies liable to reverse charge

            // All other ITC
            ViewBag.AllOtherItcTotalCGST = oClsResponse11.Data.TotalCgstValue;
            ViewBag.AllOtherItcTotalSGST = oClsResponse11.Data.TotalSgstValue + oClsResponse11.Data.TotalUtgstValue;
            ViewBag.AllOtherItcTotalIGST = oClsResponse11.Data.TotalIgstValue;
            ViewBag.AllOtherItcTotalCESS = oClsResponse11.Data.TotalCessValue;
            // All other ITC

            // Values of exempt, nil-rated and non-GST inward supplies
            ViewBag.InwardInterCompositionTotalTaxableValue = oClsResponse12.Data.TotalTaxableValue;
            ViewBag.InwardIntraCompositionTotalTaxableValue = oClsResponse13.Data.TotalTaxableValue;
            ViewBag.InwardInterNonGstTotalTaxableValue = oClsResponse14.Data.TotalTaxableValue;
            ViewBag.InwardIntraNonGstTotalTaxableValue = oClsResponse15.Data.TotalTaxableValue;
            // Values of exempt, nil-rated and non-GST inward supplies

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr3b").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            //ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }

        public async Task<ActionResult> Gstr3BFetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var outwardTaxableResult = await taxController.OutwardTaxable(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(outwardTaxableResult);

            var outwardTaxableZeroRatedResult = await taxController.OutwardTaxableZeroRated(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(outwardTaxableZeroRatedResult);

            var otherOutwardNilRatedResult = await taxController.OtherOutwardNilRated(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(otherOutwardNilRatedResult);

            var inwardReverseChargeResult = await taxController.InwardReverseCharge(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(inwardReverseChargeResult);

            var outwardNonGstResult = await taxController.OutwardNonGst(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(outwardNonGstResult);

            var interStateUnregisteredResult = await taxController.InterStateUnregistered(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(interStateUnregisteredResult);

            var interStateCompositionResult = await taxController.InterStateComposition(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(interStateCompositionResult);

            var importOfGoodsResult = await taxController.ImportOfGoods(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(importOfGoodsResult);

            var importOfServicesResult = await taxController.ImportOfServices(salesObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(importOfServicesResult);

            var inwardLiableToReverseChargeResult = await taxController.InwardLiableToReverseCharge(salesObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(inwardLiableToReverseChargeResult);

            var allOtherItcResult = await taxController.AllOtherItc(salesObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(allOtherItcResult);

            var inwardInterCompositionResult = await taxController.InwardInterComposition(salesObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(inwardInterCompositionResult);

            var inwardIntraCompositionResult = await taxController.InwardIntraComposition(salesObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(inwardIntraCompositionResult);

            var inwardInterNonGstResult = await taxController.InwardInterNonGst(salesObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(inwardInterNonGstResult);

            var inwardIntraNonGstResult = await taxController.InwardIntraNonGst(salesObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(inwardIntraNonGstResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse1.Data.FromDate;
            ViewBag.ToDate = oClsResponse1.Data.ToDate;

            // Outward taxable supplies (other than zero rated, nil rated and exempted)
            ViewBag.OutwardTaxableTotalTaxableValue = oClsResponse1.Data.TotalTaxableValue;
            ViewBag.OutwardTaxableTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.OutwardTaxableTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse1.Data.TotalUtgstValue;
            ViewBag.OutwardTaxableTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.OutwardTaxableTotalCESS = oClsResponse1.Data.TotalCessValue;
            // Outward taxable supplies (other than zero rated, nil rated and exempted)

            //  Outward taxable supplies (zero rated)
            ViewBag.OutwardTaxableZeroRatedotalTaxableValue = oClsResponse2.Data.TotalTaxableValue;
            ViewBag.OutwardTaxableZeroRatedTotalCGST = oClsResponse2.Data.TotalCgstValue;
            ViewBag.OutwardTaxableZeroRatedTotalSGST = oClsResponse2.Data.TotalSgstValue + oClsResponse2.Data.TotalUtgstValue;
            ViewBag.OutwardTaxableZeroRatedTotalIGST = oClsResponse2.Data.TotalIgstValue;
            ViewBag.OutwardTaxableZeroRatedTotalCESS = oClsResponse2.Data.TotalCessValue;
            //  Outward taxable supplies (zero rated)

            //  Other outward supplies (Nil rated, exempted)
            ViewBag.OtherOutwardNilRatedotalTaxableValue = oClsResponse3.Data.TotalTaxableValue;
            ViewBag.OtherOutwardNilRatedTotalCGST = oClsResponse3.Data.TotalCgstValue;
            ViewBag.OtherOutwardNilRatedTotalSGST = oClsResponse3.Data.TotalSgstValue + oClsResponse3.Data.TotalUtgstValue;
            ViewBag.OtherOutwardNilRatedTotalIGST = oClsResponse3.Data.TotalIgstValue;
            ViewBag.OtherOutwardNilRatedTotalCESS = oClsResponse3.Data.TotalCessValue;
            //  Other outward supplies (Nil rated, exempted)

            //  Inward supplies (liable to reverse charge)
            ViewBag.InwardReverseChargeTotalTaxableValue = oClsResponse4.Data.TotalTaxableValue;
            ViewBag.InwardReverseChargeTotalCGST = oClsResponse4.Data.TotalCgstValue;
            ViewBag.InwardReverseChargeTotalSGST = oClsResponse4.Data.TotalSgstValue + oClsResponse4.Data.TotalUtgstValue;
            ViewBag.InwardReverseChargeTotalIGST = oClsResponse4.Data.TotalIgstValue;
            ViewBag.InwardReverseChargeTotalCESS = oClsResponse4.Data.TotalCessValue;
            //  Inward supplies (liable to reverse charge)

            //  Non-GST outward supplies
            ViewBag.OutwardNonGstTotalTaxableValue = oClsResponse5.Data.TotalTaxableValue;
            ViewBag.OutwardNonGstTotalCGST = oClsResponse5.Data.TotalCgstValue;
            ViewBag.OutwardNonGstTotalSGST = oClsResponse5.Data.TotalSgstValue + oClsResponse5.Data.TotalUtgstValue;
            ViewBag.OutwardNonGstTotalIGST = oClsResponse5.Data.TotalIgstValue;
            ViewBag.OutwardNonGstTotalCESS = oClsResponse5.Data.TotalCessValue;
            //  Non-GST outward supplies

            //  Supplies made to Unregistered Persons
            ViewBag.InterStateUnregistered = oClsResponse6.Data.Sales.GroupBy(s => new { s.PlaceOfSupply })
            .Select(g => new
            {
                PlaceOfSupply = g.Key.PlaceOfSupply,
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TotalIgstValue = g.Select(b => b.TotalIgstValue).Sum(),
            }).ToList();
            //  Supplies made to Unregistered Persons

            //  Supplies made to Composition Taxable Persons
            ViewBag.InterStateComposition = oClsResponse7.Data.Sales.GroupBy(s => new { s.PlaceOfSupply })
            .Select(g => new
            {
                PlaceOfSupply = g.Key.PlaceOfSupply,
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TotalIgstValue = g.Select(b => b.TotalIgstValue).Sum(),
            }).ToList();
            //  Supplies made to Composition Taxable Persons

            //  Import of Goods
            ViewBag.ImportOfGoodsTotalCGST = oClsResponse8.Data.TotalCgstValue;
            ViewBag.ImportOfGoodsTotalSGST = oClsResponse8.Data.TotalSgstValue + oClsResponse8.Data.TotalUtgstValue;
            ViewBag.ImportOfGoodsTotalIGST = oClsResponse8.Data.TotalIgstValue;
            ViewBag.ImportOfGoodsTotalCESS = oClsResponse8.Data.TotalCessValue;
            //  Import of Goods

            //  Import of Services
            ViewBag.ImportOfServicesTotalCGST = oClsResponse9.Data.TotalCgstValue;
            ViewBag.ImportOfServicesTotalSGST = oClsResponse9.Data.TotalSgstValue + oClsResponse9.Data.TotalUtgstValue;
            ViewBag.ImportOfServicesTotalIGST = oClsResponse9.Data.TotalIgstValue;
            ViewBag.ImportOfServicesTotalCESS = oClsResponse9.Data.TotalCessValue;
            //  Import of Services

            //  Inward supplies liable to reverse charge
            ViewBag.InwardLiableToReverseChargeTotalCGST = oClsResponse10.Data.TotalCgstValue;
            ViewBag.InwardLiableToReverseChargeTotalSGST = oClsResponse10.Data.TotalSgstValue + oClsResponse10.Data.TotalUtgstValue;
            ViewBag.InwardLiableToReverseChargeTotalIGST = oClsResponse10.Data.TotalIgstValue;
            ViewBag.InwardLiableToReverseChargeTotalCESS = oClsResponse10.Data.TotalCessValue;
            //  Inward supplies liable to reverse charge

            // All other ITC
            ViewBag.AllOtherItcTotalCGST = oClsResponse11.Data.TotalCgstValue;
            ViewBag.AllOtherItcTotalSGST = oClsResponse11.Data.TotalSgstValue + oClsResponse11.Data.TotalUtgstValue;
            ViewBag.AllOtherItcTotalIGST = oClsResponse11.Data.TotalIgstValue;
            ViewBag.AllOtherItcTotalCESS = oClsResponse11.Data.TotalCessValue;
            // All other ITC

            // Values of exempt, nil-rated and non-GST inward supplies
            ViewBag.InwardInterCompositionTotalTaxableValue = oClsResponse12.Data.TotalTaxableValue;
            ViewBag.InwardIntraCompositionTotalTaxableValue = oClsResponse13.Data.TotalTaxableValue;
            ViewBag.InwardInterNonGstTotalTaxableValue = oClsResponse14.Data.TotalTaxableValue;
            ViewBag.InwardIntraNonGstTotalTaxableValue = oClsResponse15.Data.TotalTaxableValue;
            // Values of exempt, nil-rated and non-GST inward supplies

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr3b").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            //ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr3B");
        }

        public async Task<ActionResult> Gstr3BDetails(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            IHttpActionResult taxResult = null;

            if(obj.Title == "Outward taxable supplies (other than zero rated, nil rated and exempted)")
            {
                taxResult = await taxController.OutwardTaxable(salesObj);
            }
            else if(obj.Title == "Outward taxable supplies (zero rated)")
            {
                taxResult = await taxController.OutwardTaxableZeroRated(salesObj);
            }
            else if(obj.Title == "Other outward supplies (Nil rated, exempted)")
            {
                taxResult = await taxController.OtherOutwardNilRated(salesObj);
            }
            else if (obj.Title == "Inward supplies (liable to reverse charge)")
            {
                taxResult = await taxController.InwardReverseCharge(salesObj);
            }
            else if (obj.Title == "Non-GST outward supplies")
            {
                taxResult = await taxController.OutwardNonGst(salesObj);
            }
            else if (obj.Title == "Supplies made to Unregistered Persons")
            {
                taxResult = await taxController.InterStateUnregistered(salesObj);
            }
            else if (obj.Title == "Supplies made to Composition Taxable Persons")
            {
                taxResult = await taxController.InterStateComposition(salesObj);
            }
            else if (obj.Title == "Import of Goods")
            {
                taxResult = await taxController.ImportOfGoods(salesObj);
            }
            else if (obj.Title == "Import of Services")
            {
                taxResult = await taxController.ImportOfServices(salesObj);
            }
            //else if (obj.Title == "Inward supplies liable to reverse charge ( other than 1 & 2 above)")
            else if (obj.Title == "Inward supplies liable to reverse charge ( other than 1 ")
            {
                taxResult = await taxController.InwardLiableToReverseCharge(salesObj);
                obj.Title = "Inward supplies liable to reverse charge ( other than 1 & 2 above)";
            }
            else if (obj.Title == "All other ITC")
            {
                taxResult = await taxController.AllOtherItc(salesObj);
            }
            else if (obj.Title == "Composition Scheme, Exempted, Nil Rated (Inter-State Supplies)")
            {
                taxResult = await taxController.InwardInterComposition(salesObj);
            }
            else if (obj.Title == "Composition Scheme, Exempted, Nil Rated (Intra-State Supplies)")
            {
                taxResult = await taxController.InwardIntraComposition(salesObj);
            }
            else if (obj.Title == "Non-GST supply (Inter-State Supplies)")
            {
                taxResult = await taxController.InwardInterNonGst(salesObj);
            }
            else if (obj.Title == "Non-GST supply (Intra-State Supplies)")
            {
                taxResult = await taxController.InwardIntraNonGst(salesObj);
            }

            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            //var res39 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "BusinessSettings/BusinessSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse39 = serializer.Deserialize<ClsResponse>(res39);

            //var res55 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "BusinessRegistrationName/ActiveBusinessRegistrationNames", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse55 = serializer.Deserialize<ClsResponse>(res55);

            //var res56 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TaxSetting/ActiveTaxSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse56 = serializer.Deserialize<ClsResponse>(res56);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Sales = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr3b").FirstOrDefault();
            //ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            //ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            //ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            ViewBag.PageTitle = obj.Title;

            return View();
        }

        public async Task<ActionResult> Gstr3BJson(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateGstr3BJson(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            // Convert string response to byte array
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", "GSTR3B.json");
        }

        public async Task<ActionResult> GstrCmp08()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, TaxSettingId = obj.TaxSettingId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var outwardIncExemptResult = await taxController.OutwardIncExempt(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(outwardIncExemptResult);

            var inwardRevChargeIncImpOfServicesResult = await taxController.InwardRevChargeIncImpOfServices(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(inwardRevChargeIncImpOfServicesResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            // Outward supplies (including exempt supplies)
            ViewBag.OutwardTotalInvoiceValue = oClsResponse.Data.TotalInvoiceValue;
            ViewBag.OutwardTotalCGST = oClsResponse.Data.TotalCgstValue;
            ViewBag.OutwardTotalSGST = oClsResponse.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.OutwardTotalIGST = oClsResponse.Data.TotalIgstValue;
            ViewBag.OutwardTotalCESS = oClsResponse.Data.TotalCessValue;
            // Outward supplies (including exempt supplies)

            // Inward supplies attracting reverse charge including import of services
            ViewBag.InwardTotalInvoiceValue = oClsResponse1.Data.TotalInvoiceValue;
            ViewBag.InwardTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.InwardTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse1.Data.TotalUtgstValue;
            ViewBag.InwardTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.InwardTotalCESS = oClsResponse1.Data.TotalCessValue;
            // Inward supplies attracting reverse charge including import of services

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr cmp-08").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;
            ViewBag.TaxSettingId = obj.TaxSettingId;

            return View();
        }

        public async Task<ActionResult> GstrCmp08Fetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, TaxSettingId = obj.TaxSettingId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var outwardIncExemptResult = await taxController.OutwardIncExempt(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(outwardIncExemptResult);

            var inwardRevChargeIncImpOfServicesResult = await taxController.InwardRevChargeIncImpOfServices(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(inwardRevChargeIncImpOfServicesResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            // Outward supplies (including exempt supplies)
            ViewBag.OutwardTotalInvoiceValue = oClsResponse.Data.TotalInvoiceValue;
            ViewBag.OutwardTotalCGST = oClsResponse.Data.TotalCgstValue;
            ViewBag.OutwardTotalSGST = oClsResponse.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.OutwardTotalIGST = oClsResponse.Data.TotalIgstValue;
            ViewBag.OutwardTotalCESS = oClsResponse.Data.TotalCessValue;
            // Outward supplies (including exempt supplies)

            // Inward supplies attracting reverse charge including import of services
            ViewBag.InwardTotalInvoiceValue = oClsResponse1.Data.TotalInvoiceValue;
            ViewBag.InwardTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.InwardTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse1.Data.TotalUtgstValue;
            ViewBag.InwardTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.InwardTotalCESS = oClsResponse1.Data.TotalCessValue;
            // Inward supplies attracting reverse charge including import of services

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr cmp-08").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;
            ViewBag.TaxSettingId = obj.TaxSettingId;

            return PartialView("PartialGstrCmp08");
        }

        public async Task<ActionResult> GstrCmp08Details(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            IHttpActionResult taxResult = null;

            if(obj.Title == "Outward supplies (including exempt supplies)")
            {
                taxResult = await taxController.OutwardIncExempt(salesObj);
            }
            else
            {
                taxResult = await taxController.InwardRevChargeIncImpOfServices(salesObj);
            }

            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            //var res39 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "BusinessSettings/BusinessSetting", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse39 = serializer.Deserialize<ClsResponse>(res39);

            //var res51 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TaxType/ActiveTaxTypes", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse51 = serializer.Deserialize<ClsResponse>(res51);

            //var res55 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "BusinessRegistrationName/ActiveBusinessRegistrationNames", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse55 = serializer.Deserialize<ClsResponse>(res55);

            //var res56 = await oCommonController.PostMethod(_json, oCommonController.baseUrl + "TaxSetting/ActiveTaxSettings", arr[0], arr[1], arr[2]);
            //ClsResponse oClsResponse56 = serializer.Deserialize<ClsResponse>(res56);

            ViewBag.Sales = oClsResponse.Data.Sales;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr cmp-08").FirstOrDefault();
            //ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            //ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            //ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            //ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            ViewBag.PageTitle = obj.Title;
            return View();
        }

        public async Task<ActionResult> Gstr4()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var gstr4Sec4AResult = await taxController.Gstr4Sec4A(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4AResult);

            var gstr4Sec4BResult = await taxController.Gstr4Sec4B(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4BResult);

            var gstr4Sec4CResult = await taxController.Gstr4Sec4C(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4CResult);

            var gstr4Sec4DResult = await taxController.Gstr4Sec4D(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4DResult);

            var outwardIncExemptResult = await taxController.OutwardIncExempt(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(outwardIncExemptResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            // Gstr4 Sec4A
            ViewBag.Sec4ATotalTaxableValue = oClsResponse.Data.TotalTaxableValue;
            ViewBag.Sec4ATotalCGST = oClsResponse.Data.TotalCgstValue;
            ViewBag.Sec4ATotalSGST = oClsResponse.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec4ATotalIGST = oClsResponse.Data.TotalIgstValue;
            ViewBag.Sec4ATotalCESS = oClsResponse.Data.TotalCessValue;
            // Gstr4 Sec4A

            // Gstr4 Sec4B
            ViewBag.Sec4BTotalTaxableValue = oClsResponse1.Data.TotalTaxableValue;
            ViewBag.Sec4BTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.Sec4BTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec4BTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.Sec4BTotalCESS = oClsResponse1.Data.TotalCessValue;
            // Gstr4 Sec4B

            // Gstr4 Sec4C
            ViewBag.Sec4CTotalTaxableValue = oClsResponse2.Data.TotalTaxableValue;
            ViewBag.Sec4CTotalCGST = oClsResponse2.Data.TotalCgstValue;
            ViewBag.Sec4CTotalSGST = oClsResponse2.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec4CTotalIGST = oClsResponse2.Data.TotalIgstValue;
            ViewBag.Sec4CTotalCESS = oClsResponse2.Data.TotalCessValue;
            // Gstr4 Sec4C

            // Gstr4 Sec4D
            ViewBag.Sec4DTotalTaxableValue = oClsResponse3.Data.TotalTaxableValue;
            ViewBag.Sec4DTotalCGST = oClsResponse3.Data.TotalCgstValue;
            ViewBag.Sec4DTotalSGST = oClsResponse3.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec4DTotalIGST = oClsResponse3.Data.TotalIgstValue;
            ViewBag.Sec4DTotalCESS = oClsResponse3.Data.TotalCessValue;
            // Gstr4 Sec4D

            // Gstr4 Sec6
            ViewBag.Sec6TotalTaxableValue = oClsResponse4.Data.TotalInvoiceValue;
            ViewBag.Sec6TotalCGST = oClsResponse4.Data.TotalCgstValue;
            ViewBag.Sec6TotalSGST = oClsResponse4.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec6TotalIGST = oClsResponse4.Data.TotalIgstValue;
            ViewBag.Sec6TotalCESS = oClsResponse4.Data.TotalCessValue;
            // Gstr4 Sec6

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }

        public async Task<ActionResult> Gstr4Fetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var gstr4Sec4AResult = await taxController.Gstr4Sec4A(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4AResult);

            var gstr4Sec4BResult = await taxController.Gstr4Sec4B(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4BResult);

            var gstr4Sec4CResult = await taxController.Gstr4Sec4C(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4CResult);

            var gstr4Sec4DResult = await taxController.Gstr4Sec4D(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4DResult);

            var outwardIncExemptResult = await taxController.OutwardIncExempt(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(outwardIncExemptResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            // Gstr4 Sec4A
            ViewBag.Sec4ATotalTaxableValue = oClsResponse.Data.TotalTaxableValue;
            ViewBag.Sec4ATotalCGST = oClsResponse.Data.TotalCgstValue;
            ViewBag.Sec4ATotalSGST = oClsResponse.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec4ATotalIGST = oClsResponse.Data.TotalIgstValue;
            ViewBag.Sec4ATotalCESS = oClsResponse.Data.TotalCessValue;
            // Gstr4 Sec4A

            // Gstr4 Sec4B
            ViewBag.Sec4BTotalTaxableValue = oClsResponse1.Data.TotalTaxableValue;
            ViewBag.Sec4BTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.Sec4BTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec4BTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.Sec4BTotalCESS = oClsResponse1.Data.TotalCessValue;
            // Gstr4 Sec4B

            // Gstr4 Sec4C
            ViewBag.Sec4CTotalTaxableValue = oClsResponse2.Data.TotalTaxableValue;
            ViewBag.Sec4CTotalCGST = oClsResponse2.Data.TotalCgstValue;
            ViewBag.Sec4CTotalSGST = oClsResponse2.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec4CTotalIGST = oClsResponse2.Data.TotalIgstValue;
            ViewBag.Sec4CTotalCESS = oClsResponse2.Data.TotalCessValue;
            // Gstr4 Sec4C

            // Gstr4 Sec4D
            ViewBag.Sec4DTotalTaxableValue = oClsResponse3.Data.TotalTaxableValue;
            ViewBag.Sec4DTotalCGST = oClsResponse3.Data.TotalCgstValue;
            ViewBag.Sec4DTotalSGST = oClsResponse3.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec4DTotalIGST = oClsResponse3.Data.TotalIgstValue;
            ViewBag.Sec4DTotalCESS = oClsResponse3.Data.TotalCessValue;
            // Gstr4 Sec4D

            // Gstr4 Sec6
            ViewBag.Sec6TotalTaxableValue = oClsResponse4.Data.TotalInvoiceValue;
            ViewBag.Sec6TotalCGST = oClsResponse4.Data.TotalCgstValue;
            ViewBag.Sec6TotalSGST = oClsResponse4.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec6TotalIGST = oClsResponse4.Data.TotalIgstValue;
            ViewBag.Sec6TotalCESS = oClsResponse4.Data.TotalCessValue;
            // Gstr4 Sec6

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr4");
        }
        
        public async Task<ActionResult> Gstr4Details(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            IHttpActionResult taxResult = null;

            if (obj.Title == "4A - Inward supplies received from a registered supplier(other than supplies attracting reverse charge)")
            {
                taxResult = await taxController.Gstr4Sec4ADetails(salesObj);
            }
            else if (obj.Title == "4B - Inward supplies received from a registered supplier (attracting reverse charge)")
            {
                taxResult = await taxController.Gstr4Sec4BDetails(salesObj);
            }
            else if (obj.Title == "4C - Inward supplies received from an unregistered supplier")
            {
                taxResult = await taxController.Gstr4Sec4CDetails(salesObj);
            }
            else if (obj.Title == "4D - Import of services")
            {
                taxResult = await taxController.Gstr4Sec4DDetails(salesObj);
            }

            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Sales = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr3b").FirstOrDefault();

            ViewBag.PageTitle = obj.Title;

            return View();
        }

        public async Task<ActionResult> Gstr4InvoiceDetails(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Title = obj.Title };

            WebApi.TaxController taxController = new WebApi.TaxController();
            IHttpActionResult taxResult = null;

            if (obj.Title == "4A - Inward supplies received from a registered supplier(other than supplies attracting reverse charge)")
            {
                taxResult = await taxController.Gstr4Sec4A(salesObj);
            }
            else if (obj.Title == "4B - Inward supplies received from a registered supplier (attracting reverse charge)")
            {
                taxResult = await taxController.Gstr4Sec4B(salesObj);
            }
            else if (obj.Title == "4C - Inward supplies received from an unregistered supplier")
            {
                taxResult = await taxController.Gstr4Sec4C(salesObj);
            }
            else if (obj.Title == "4D - Import of services")
            {
                taxResult = await taxController.Gstr4Sec4D(salesObj);
            }
            else if (obj.Title == "6 - Tax rate wise details of outward supplies/inward supplies attracting reverse charge during the year (Net of advances, credit and debit notes and any other adjustment due to amendments etc).")
            {
                taxResult = await taxController.OutwardIncExempt(salesObj);
            }

            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Sales = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr3b").FirstOrDefault();

            ViewBag.PageTitle = obj.Title;

            return View();
        }

        public async Task<ActionResult> Gstr4ExcelViewFetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var gstr4Sec4ADetailsResult = await taxController.Gstr4Sec4ADetails(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4ADetailsResult);

            var gstr4Sec4BDetailsResult = await taxController.Gstr4Sec4BDetails(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4BDetailsResult);

            var gstr4Sec4CDetailsResult = await taxController.Gstr4Sec4CDetails(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4CDetailsResult);

            var gstr4Sec4DDetailsResult = await taxController.Gstr4Sec4DDetails(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(gstr4Sec4DDetailsResult);

            var outwardIncExemptResult = await taxController.OutwardIncExempt(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(outwardIncExemptResult);

            ViewBag.Sales4A = oClsResponse.Data.Sales;
            ViewBag.Sales4B = oClsResponse1.Data.Sales;
            ViewBag.Sales4C = oClsResponse2.Data.Sales;
            ViewBag.Sales4D = oClsResponse3.Data.Sales;

            // Gstr4 Sec6
            ViewBag.Sec6TaxPercent = oClsResponse4.Data.TaxPercent;
            ViewBag.Sec6TotalTaxableValue = oClsResponse4.Data.TotalInvoiceValue;
            ViewBag.Sec6TotalCGST = oClsResponse4.Data.TotalCgstValue;
            ViewBag.Sec6TotalSGST = oClsResponse4.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Sec6TotalIGST = oClsResponse4.Data.TotalIgstValue;
            ViewBag.Sec6TotalCESS = oClsResponse4.Data.TotalCessValue;
            // Gstr4 Sec6

            return PartialView("PartialGstr4ExcelView");
        }

        public async Task<ActionResult> Gstr4Json(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var jsonResult = await taxController.GenerateGstr4Json(salesObj);
            var responseContent = await jsonResult.ExecuteAsync(System.Threading.CancellationToken.None);
            string res = await responseContent.Content.ReadAsStringAsync();

            // Convert string response to byte array
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(res);

            // Return file as downloadable JSON
            return File(fileBytes, "application/json", "GSTR4.json");
        }

        public async Task<ActionResult> Gstr9()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var gstr9_4AResult = await taxController.Gstr9_4A(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(gstr9_4AResult);

            var gstr9_4BResult = await taxController.Gstr9_4B(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(gstr9_4BResult);

            var gstr9_4CResult = await taxController.Gstr9_4C(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(gstr9_4CResult);

            var gstr9_4DResult = await taxController.Gstr9_4D(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(gstr9_4DResult);

            var gstr9_4FResult = await taxController.Gstr9_4F(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(gstr9_4FResult);

            var gstr9_4GResult = await taxController.Gstr9_4G(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(gstr9_4GResult);

            var gstr9_4IResult = await taxController.Gstr9_4I(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(gstr9_4IResult);

            var gstr9_4JResult = await taxController.Gstr9_4J(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(gstr9_4JResult);

            var gstr9_5AResult = await taxController.Gstr9_5A(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(gstr9_5AResult);

            var gstr9_5BResult = await taxController.Gstr9_5B(salesObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(gstr9_5BResult);

            var gstr9_5CResult = await taxController.Gstr9_5C(salesObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(gstr9_5CResult);

            var gstr9_5DResult = await taxController.Gstr9_5D(salesObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(gstr9_5DResult);

            var gstr9_5EResult = await taxController.Gstr9_5E(salesObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(gstr9_5EResult);

            var gstr9_5FResult = await taxController.Gstr9_5F(salesObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(gstr9_5FResult);

            var gstr9_5HResult = await taxController.Gstr9_5H(salesObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(gstr9_5HResult);

            var gstr9_5IResult = await taxController.Gstr9_5I(salesObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(gstr9_5IResult);

            var gstr9_6BInputsResult = await taxController.Gstr9_6BInputs(salesObj);
            ClsResponse oClsResponse16 = await oCommonController.ExtractResponseFromActionResult(gstr9_6BInputsResult);

            var gstr9_6BCapitalGoodsResult = await taxController.Gstr9_6BCapitalGoods(salesObj);
            ClsResponse oClsResponse17 = await oCommonController.ExtractResponseFromActionResult(gstr9_6BCapitalGoodsResult);

            var gstr9_6BInputServicesResult = await taxController.Gstr9_6BInputServices(salesObj);
            ClsResponse oClsResponse18 = await oCommonController.ExtractResponseFromActionResult(gstr9_6BInputServicesResult);

            var gstr9_6CInputsResult = await taxController.Gstr9_6CInputs(salesObj);
            ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(gstr9_6CInputsResult);

            var gstr9_6CCapitalGoodsResult = await taxController.Gstr9_6CCapitalGoods(salesObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(gstr9_6CCapitalGoodsResult);

            var gstr9_6CInputServicesResult = await taxController.Gstr9_6CInputServices(salesObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(gstr9_6CInputServicesResult);

            var gstr9_6DInputsResult = await taxController.Gstr9_6DInputs(salesObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(gstr9_6DInputsResult);

            var gstr9_6DCapitalGoodsResult = await taxController.Gstr9_6DCapitalGoods(salesObj);
            ClsResponse oClsResponse23 = await oCommonController.ExtractResponseFromActionResult(gstr9_6DCapitalGoodsResult);

            var gstr9_6DInputServicesResult = await taxController.Gstr9_6DInputServices(salesObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(gstr9_6DInputServicesResult);

            var gstr9_6EInputsResult = await taxController.Gstr9_6EInputs(salesObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(gstr9_6EInputsResult);

            var gstr9_6ECapitalGoodsResult = await taxController.Gstr9_6ECapitalGoods(salesObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(gstr9_6ECapitalGoodsResult);

            var gstr9_6FResult = await taxController.Gstr9_6F(salesObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(gstr9_6FResult);

            var gstr9_10Result = await taxController.Gstr9_10(salesObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(gstr9_10Result);

            var gstr9_11Result = await taxController.Gstr9_11(salesObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(gstr9_11Result);

            var gstr9_16AResult = await taxController.Gstr9_16A(salesObj);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(gstr9_16AResult);

            var gstr9_17Result = await taxController.Gstr9_17(salesObj);
            ClsResponse oClsResponse31 = await oCommonController.ExtractResponseFromActionResult(gstr9_17Result);

            var gstr9_18Result = await taxController.Gstr9_18(salesObj);
            ClsResponse oClsResponse32 = await oCommonController.ExtractResponseFromActionResult(gstr9_18Result);

            var gstr9_14Result = await taxController.Gstr9_14(salesObj);
            ClsResponse oClsResponse33 = await oCommonController.ExtractResponseFromActionResult(gstr9_14Result);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSetting taxSettingObj = new ClsTaxSetting { CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.TaxSetting(taxSettingObj);
            ClsResponse oClsResponse34 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController2 = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingVmObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingActiveResult = await taxSettingController2.ActiveTaxSettings(taxSettingVmObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingActiveResult);

            // Gstr9_4A
            ViewBag.Gstr9_4ATotalTaxableValue = oClsResponse.Data.TotalTaxableValue;
            ViewBag.Gstr9_4ATotalCGST = oClsResponse.Data.TotalCgstValue;
            ViewBag.Gstr9_4ATotalSGST = oClsResponse.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Gstr9_4ATotalIGST = oClsResponse.Data.TotalIgstValue;
            ViewBag.Gstr9_4ATotalCESS = oClsResponse.Data.TotalCessValue;
            // Gstr9_4A

            // Gstr9_4B
            ViewBag.Gstr9_4BTotalTaxableValue = oClsResponse1.Data.TotalTaxableValue;
            ViewBag.Gstr9_4BTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.Gstr9_4BTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse1.Data.TotalUtgstValue;
            ViewBag.Gstr9_4BTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.Gstr9_4BTotalCESS = oClsResponse1.Data.TotalCessValue;
            // Gstr9_4B

            // Gstr9_4C
            ViewBag.Gstr9_4CTotalTaxableValue = oClsResponse2.Data.TotalTaxableValue;
            ViewBag.Gstr9_4CTotalCGST = oClsResponse2.Data.TotalCgstValue;
            ViewBag.Gstr9_4CTotalSGST = oClsResponse2.Data.TotalSgstValue + oClsResponse2.Data.TotalUtgstValue;
            ViewBag.Gstr9_4CTotalIGST = oClsResponse2.Data.TotalIgstValue;
            ViewBag.Gstr9_4CTotalCESS = oClsResponse2.Data.TotalCessValue;
            // Gstr9_4C

            // Gstr9_4D
            ViewBag.Gstr9_4DTotalTaxableValue = oClsResponse3.Data.TotalTaxableValue;
            ViewBag.Gstr9_4DTotalCGST = oClsResponse3.Data.TotalCgstValue;
            ViewBag.Gstr9_4DTotalSGST = oClsResponse3.Data.TotalSgstValue + oClsResponse3.Data.TotalUtgstValue;
            ViewBag.Gstr9_4DTotalIGST = oClsResponse3.Data.TotalIgstValue;
            ViewBag.Gstr9_4DTotalCESS = oClsResponse3.Data.TotalCessValue;
            // Gstr9_4D

            // Gstr9_4F
            ViewBag.Gstr9_4FTotalTaxableValue = oClsResponse4.Data.TotalTaxableValue;
            ViewBag.Gstr9_4FTotalCGST = oClsResponse4.Data.TotalCgstValue;
            ViewBag.Gstr9_4FTotalSGST = oClsResponse4.Data.TotalSgstValue + oClsResponse4.Data.TotalUtgstValue;
            ViewBag.Gstr9_4FTotalIGST = oClsResponse4.Data.TotalIgstValue;
            ViewBag.Gstr9_4FTotalCESS = oClsResponse4.Data.TotalCessValue;
            // Gstr9_4F

            // Gstr9_4G
            ViewBag.Gstr9_4GTotalTaxableValue = oClsResponse5.Data.TotalTaxableValue;
            ViewBag.Gstr9_4GTotalCGST = oClsResponse5.Data.TotalCgstValue;
            ViewBag.Gstr9_4GTotalSGST = oClsResponse5.Data.TotalSgstValue + oClsResponse5.Data.TotalUtgstValue;
            ViewBag.Gstr9_4GTotalIGST = oClsResponse5.Data.TotalIgstValue;
            ViewBag.Gstr9_4GTotalCESS = oClsResponse5.Data.TotalCessValue;
            // Gstr9_4G

            // Gstr9_4I
            ViewBag.Gstr9_4ITotalTaxableValue = oClsResponse6.Data.TotalTaxableValue;
            ViewBag.Gstr9_4ITotalCGST = oClsResponse6.Data.TotalCgstValue;
            ViewBag.Gstr9_4ITotalSGST = oClsResponse6.Data.TotalSgstValue + oClsResponse6.Data.TotalUtgstValue;
            ViewBag.Gstr9_4ITotalIGST = oClsResponse6.Data.TotalIgstValue;
            ViewBag.Gstr9_4ITotalCESS = oClsResponse6.Data.TotalCessValue;
            // Gstr9_4I

            // Gstr9_4J
            ViewBag.Gstr9_4JTotalTaxableValue = oClsResponse7.Data.TotalTaxableValue;
            ViewBag.Gstr9_4JTotalCGST = oClsResponse7.Data.TotalCgstValue;
            ViewBag.Gstr9_4JTotalSGST = oClsResponse7.Data.TotalSgstValue + oClsResponse7.Data.TotalUtgstValue;
            ViewBag.Gstr9_4JTotalIGST = oClsResponse7.Data.TotalIgstValue;
            ViewBag.Gstr9_4JTotalCESS = oClsResponse7.Data.TotalCessValue;
            // Gstr9_4J

            // Gstr9_5A
            ViewBag.Gstr9_5ATotalTaxableValue = oClsResponse8.Data.TotalTaxableValue;
            ViewBag.Gstr9_5ATotalCGST = oClsResponse8.Data.TotalCgstValue;
            ViewBag.Gstr9_5ATotalSGST = oClsResponse8.Data.TotalSgstValue + oClsResponse8.Data.TotalUtgstValue;
            ViewBag.Gstr9_5ATotalIGST = oClsResponse8.Data.TotalIgstValue;
            ViewBag.Gstr9_5ATotalCESS = oClsResponse8.Data.TotalCessValue;
            // Gstr9_5A

            // Gstr9_5B
            ViewBag.Gstr9_5BTotalTaxableValue = oClsResponse9.Data.TotalTaxableValue;
            ViewBag.Gstr9_5BTotalCGST = oClsResponse9.Data.TotalCgstValue;
            ViewBag.Gstr9_5BTotalSGST = oClsResponse9.Data.TotalSgstValue + oClsResponse9.Data.TotalUtgstValue;
            ViewBag.Gstr9_5BTotalIGST = oClsResponse9.Data.TotalIgstValue;
            ViewBag.Gstr9_5BTotalCESS = oClsResponse9.Data.TotalCessValue;
            // Gstr9_5B

            // Gstr9_5C
            ViewBag.Gstr9_5CTotalTaxableValue = oClsResponse10.Data.TotalTaxableValue;
            ViewBag.Gstr9_5CTotalCGST = oClsResponse10.Data.TotalCgstValue;
            ViewBag.Gstr9_5CTotalSGST = oClsResponse10.Data.TotalSgstValue + oClsResponse10.Data.TotalUtgstValue;
            ViewBag.Gstr9_5CTotalIGST = oClsResponse10.Data.TotalIgstValue;
            ViewBag.Gstr9_5CTotalCESS = oClsResponse10.Data.TotalCessValue;
            // Gstr9_5C

            // Gstr9_5D
            ViewBag.Gstr9_5DTotalTaxableValue = oClsResponse11.Data.TotalTaxableValue;
            ViewBag.Gstr9_5DTotalCGST = oClsResponse11.Data.TotalCgstValue;
            ViewBag.Gstr9_5DTotalSGST = oClsResponse11.Data.TotalSgstValue + oClsResponse11.Data.TotalUtgstValue;
            ViewBag.Gstr9_5DTotalIGST = oClsResponse11.Data.TotalIgstValue;
            ViewBag.Gstr9_5DTotalCESS = oClsResponse11.Data.TotalCessValue;
            // Gstr9_5D

            // GstrGstr9_5E
            ViewBag.Gstr9_5ETotalTaxableValue = oClsResponse12.Data.TotalTaxableValue;
            ViewBag.Gstr9_5ETotalCGST = oClsResponse12.Data.TotalCgstValue;
            ViewBag.Gstr9_5ETotalSGST = oClsResponse12.Data.TotalSgstValue + oClsResponse12.Data.TotalUtgstValue;
            ViewBag.Gstr9_5ETotalIGST = oClsResponse12.Data.TotalIgstValue;
            ViewBag.Gstr9_5ETotalCESS = oClsResponse12.Data.TotalCessValue;
            // Gstr9_5E

            // Gstr9_5F
            ViewBag.Gstr9_5FTotalTaxableValue = oClsResponse13.Data.TotalTaxableValue;
            ViewBag.Gstr9_5FTotalCGST = oClsResponse13.Data.TotalCgstValue;
            ViewBag.Gstr9_5FTotalSGST = oClsResponse13.Data.TotalSgstValue + oClsResponse13.Data.TotalUtgstValue;
            ViewBag.Gstr9_5FTotalIGST = oClsResponse13.Data.TotalIgstValue;
            ViewBag.Gstr9_5FTotalCESS = oClsResponse13.Data.TotalCessValue;
            // Gstr9_5F

            // Gstr9_5H
            ViewBag.Gstr9_5HTotalTaxableValue = oClsResponse14.Data.TotalTaxableValue;
            ViewBag.Gstr9_5HTotalCGST = oClsResponse14.Data.TotalCgstValue;
            ViewBag.Gstr9_5HTotalSGST = oClsResponse14.Data.TotalSgstValue + oClsResponse14.Data.TotalUtgstValue;
            ViewBag.Gstr9_5HTotalIGST = oClsResponse14.Data.TotalIgstValue;
            ViewBag.Gstr9_5HTotalCESS = oClsResponse14.Data.TotalCessValue;
            // Gstr9_5H

            // Gstr9_5I
            ViewBag.Gstr9_5ITotalTaxableValue = oClsResponse15.Data.TotalTaxableValue;
            ViewBag.Gstr9_5ITotalCGST = oClsResponse15.Data.TotalCgstValue;
            ViewBag.Gstr9_5ITotalSGST = oClsResponse15.Data.TotalSgstValue + oClsResponse15.Data.TotalUtgstValue;
            ViewBag.Gstr9_5ITotalIGST = oClsResponse15.Data.TotalIgstValue;
            ViewBag.Gstr9_5ITotalCESS = oClsResponse15.Data.TotalCessValue;
            // Gstr9_5I

            // Gstr9_6BInputs
            ViewBag.Gstr9_6BInputsTotalCGST = oClsResponse16.Data.TotalCgstValue;
            ViewBag.Gstr9_6BInputsTotalSGST = oClsResponse16.Data.TotalSgstValue + oClsResponse16.Data.TotalUtgstValue;
            ViewBag.Gstr9_6BInputsTotalIGST = oClsResponse16.Data.TotalIgstValue;
            ViewBag.Gstr9_6BInputsTotalCESS = oClsResponse16.Data.TotalCessValue;
            // Gstr9_6BInputs

            // Gstr9_6BCapitalGoods
            ViewBag.Gstr9_6BCapitalGoodsTotalCGST = oClsResponse17.Data.TotalCgstValue;
            ViewBag.Gstr9_6BCapitalGoodsTotalSGST = oClsResponse17.Data.TotalSgstValue + oClsResponse17.Data.TotalUtgstValue;
            ViewBag.Gstr9_6BCapitalGoodsTotalIGST = oClsResponse17.Data.TotalIgstValue;
            ViewBag.Gstr9_6BCapitalGoodsTotalCESS = oClsResponse17.Data.TotalCessValue;
            // Gstr9_6BCapitalGoods

            // Gstr9_6BInputServices
            ViewBag.Gstr9_6BInputServicesTotalCGST = oClsResponse18.Data.TotalCgstValue;
            ViewBag.Gstr9_6BInputServicesTotalSGST = oClsResponse18.Data.TotalSgstValue + oClsResponse18.Data.TotalUtgstValue;
            ViewBag.Gstr9_6BInputServicesTotalIGST = oClsResponse18.Data.TotalIgstValue;
            ViewBag.Gstr9_6BInputServicesTotalCESS = oClsResponse18.Data.TotalCessValue;
            // Gstr9_6BInputServices

            // Gstr9_6CInputs
            ViewBag.Gstr9_6CInputsTotalCGST = oClsResponse19.Data.TotalCgstValue;
            ViewBag.Gstr9_6CInputsTotalSGST = oClsResponse19.Data.TotalSgstValue + oClsResponse19.Data.TotalUtgstValue;
            ViewBag.Gstr9_6CInputsTotalIGST = oClsResponse19.Data.TotalIgstValue;
            ViewBag.Gstr9_6CInputsTotalCESS = oClsResponse19.Data.TotalCessValue;
            // Gstr9_6CInputs

            // Gstr9_6CCapitalGoods
            ViewBag.Gstr9_6CCapitalGoodsTotalCGST = oClsResponse20.Data.TotalCgstValue;
            ViewBag.Gstr9_6CCapitalGoodsTotalSGST = oClsResponse20.Data.TotalSgstValue + oClsResponse20.Data.TotalUtgstValue;
            ViewBag.Gstr9_6CCapitalGoodsTotalIGST = oClsResponse20.Data.TotalIgstValue;
            ViewBag.Gstr9_6CCapitalGoodsTotalCESS = oClsResponse20.Data.TotalCessValue;
            // Gstr9_6CCapitalGoods

            // Gstr9_6CInputServices
            ViewBag.Gstr9_6CInputServicesTotalCGST = oClsResponse21.Data.TotalCgstValue;
            ViewBag.Gstr9_6CInputServicesTotalSGST = oClsResponse21.Data.TotalSgstValue + oClsResponse21.Data.TotalUtgstValue;
            ViewBag.Gstr9_6CInputServicesTotalIGST = oClsResponse21.Data.TotalIgstValue;
            ViewBag.Gstr9_6CInputServicesTotalCESS = oClsResponse21.Data.TotalCessValue;
            // Gstr9_6CInputServices

            // Gstr9_6DInputs
            ViewBag.Gstr9_6DInputsTotalCGST = oClsResponse22.Data.TotalCgstValue;
            ViewBag.Gstr9_6DInputsTotalSGST = oClsResponse22.Data.TotalSgstValue + oClsResponse22.Data.TotalUtgstValue;
            ViewBag.Gstr9_6DInputsTotalIGST = oClsResponse22.Data.TotalIgstValue;
            ViewBag.Gstr9_6DInputsTotalCESS = oClsResponse22.Data.TotalCessValue;
            // Gstr9_6DInputs

            // Gstr9_6DCapitalGoods
            ViewBag.Gstr9_6DCapitalGoodsTotalCGST = oClsResponse23.Data.TotalCgstValue;
            ViewBag.Gstr9_6DCapitalGoodsTotalSGST = oClsResponse23.Data.TotalSgstValue + oClsResponse23.Data.TotalUtgstValue;
            ViewBag.Gstr9_6DCapitalGoodsTotalIGST = oClsResponse23.Data.TotalIgstValue;
            ViewBag.Gstr9_6DCapitalGoodsTotalCESS = oClsResponse23.Data.TotalCessValue;
            // Gstr9_6DCapitalGoods

            // Gstr9_6DInputServices
            ViewBag.Gstr9_6DInputServicesTotalCGST = oClsResponse24.Data.TotalCgstValue;
            ViewBag.Gstr9_6DInputServicesTotalSGST = oClsResponse24.Data.TotalSgstValue + oClsResponse24.Data.TotalUtgstValue;
            ViewBag.Gstr9_6DInputServicesTotalIGST = oClsResponse24.Data.TotalIgstValue;
            ViewBag.Gstr9_6DInputServicesTotalCESS = oClsResponse24.Data.TotalCessValue;
            // Gstr9_6DInputServices

            // Gstr9_6EInputs
            ViewBag.Gstr9_6EInputsTotalIGST = oClsResponse25.Data.TotalIgstValue;
            ViewBag.Gstr9_6EInputsTotalCESS = oClsResponse25.Data.TotalCessValue;
            // Gstr9_6EInputs

            // Gstr9_6ECapitalGoods
            ViewBag.Gstr9_6ECapitalGoodsTotalIGST = oClsResponse26.Data.TotalIgstValue;
            ViewBag.Gstr9_6ECapitalGoodsTotalCESS = oClsResponse26.Data.TotalCessValue;
            // Gstr9_6ECapitalGoods

            // Gstr9_6F
            ViewBag.Gstr9_6FTotalIGST = oClsResponse27.Data.TotalIgstValue;
            ViewBag.Gstr9_6FTotalCESS = oClsResponse27.Data.TotalCessValue;
            // Gstr9_6F

            // Gstr9_10
            ViewBag.Gstr9_10TotalTaxableValue = oClsResponse28.Data.TotalTaxableValue;
            ViewBag.Gstr9_10TotalCGST = oClsResponse28.Data.TotalCgstValue;
            ViewBag.Gstr9_10TotalSGST = oClsResponse28.Data.TotalSgstValue + oClsResponse28.Data.TotalUtgstValue;
            ViewBag.Gstr9_10TotalIGST = oClsResponse28.Data.TotalIgstValue;
            ViewBag.Gstr9_10TotalCESS = oClsResponse28.Data.TotalCessValue;
            // Gstr9_10

            // Gstr9_11
            ViewBag.Gstr9_11TotalTaxableValue = oClsResponse29.Data.TotalTaxableValue;
            ViewBag.Gstr9_11TotalCGST = oClsResponse29.Data.TotalCgstValue;
            ViewBag.Gstr9_11TotalSGST = oClsResponse29.Data.TotalSgstValue + oClsResponse29.Data.TotalUtgstValue;
            ViewBag.Gstr9_11TotalIGST = oClsResponse29.Data.TotalIgstValue;
            ViewBag.Gstr9_11TotalCESS = oClsResponse29.Data.TotalCessValue;
            // Gstr9_11

            // Gstr9_16A
            ViewBag.Gstr9_16ATotalTaxableValue = oClsResponse30.Data.TotalTaxableValue;
            // Gstr9_16A

            // Gstr9_17
            ViewBag.Gstr9_17 = oClsResponse31.Data.Sales;
            // Gstr9_17

            // Gstr9_18
            ViewBag.Gstr9_18= oClsResponse32.Data.Sales;
            // Gstr9_18

            // Gstr9_14
            ViewBag.Gstr9_14TotalCGST = oClsResponse33.Data.TotalCgstValue;
            ViewBag.Gstr9_14TotalSGST = oClsResponse33.Data.TotalSgstValue + oClsResponse33.Data.TotalUtgstValue;
            ViewBag.Gstr9_14TotalIGST = oClsResponse33.Data.TotalIgstValue;
            ViewBag.Gstr9_14TotalCESS = oClsResponse3.Data.TotalCessValue;
            // Gstr9_14

            ViewBag.TaxSetting = oClsResponse34.Data.TaxSetting;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }

        public async Task<ActionResult> Gstr9Fetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var gstr9_4AResult = await taxController.Gstr9_4A(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(gstr9_4AResult);

            var gstr9_4BResult = await taxController.Gstr9_4B(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(gstr9_4BResult);

            var gstr9_4CResult = await taxController.Gstr9_4C(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(gstr9_4CResult);

            var gstr9_4DResult = await taxController.Gstr9_4D(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(gstr9_4DResult);

            var gstr9_4FResult = await taxController.Gstr9_4F(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(gstr9_4FResult);

            var gstr9_4GResult = await taxController.Gstr9_4G(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(gstr9_4GResult);

            var gstr9_4IResult = await taxController.Gstr9_4I(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(gstr9_4IResult);

            var gstr9_4JResult = await taxController.Gstr9_4J(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(gstr9_4JResult);

            var gstr9_5AResult = await taxController.Gstr9_5A(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(gstr9_5AResult);

            var gstr9_5BResult = await taxController.Gstr9_5B(salesObj);
            ClsResponse oClsResponse9 = await oCommonController.ExtractResponseFromActionResult(gstr9_5BResult);

            var gstr9_5CResult = await taxController.Gstr9_5C(salesObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(gstr9_5CResult);

            var gstr9_5DResult = await taxController.Gstr9_5D(salesObj);
            ClsResponse oClsResponse11 = await oCommonController.ExtractResponseFromActionResult(gstr9_5DResult);

            var gstr9_5EResult = await taxController.Gstr9_5E(salesObj);
            ClsResponse oClsResponse12 = await oCommonController.ExtractResponseFromActionResult(gstr9_5EResult);

            var gstr9_5FResult = await taxController.Gstr9_5F(salesObj);
            ClsResponse oClsResponse13 = await oCommonController.ExtractResponseFromActionResult(gstr9_5FResult);

            var gstr9_5HResult = await taxController.Gstr9_5H(salesObj);
            ClsResponse oClsResponse14 = await oCommonController.ExtractResponseFromActionResult(gstr9_5HResult);

            var gstr9_5IResult = await taxController.Gstr9_5I(salesObj);
            ClsResponse oClsResponse15 = await oCommonController.ExtractResponseFromActionResult(gstr9_5IResult);

            var gstr9_6BInputsResult = await taxController.Gstr9_6BInputs(salesObj);
            ClsResponse oClsResponse16 = await oCommonController.ExtractResponseFromActionResult(gstr9_6BInputsResult);

            var gstr9_6BCapitalGoodsResult = await taxController.Gstr9_6BCapitalGoods(salesObj);
            ClsResponse oClsResponse17 = await oCommonController.ExtractResponseFromActionResult(gstr9_6BCapitalGoodsResult);

            var gstr9_6BInputServicesResult = await taxController.Gstr9_6BInputServices(salesObj);
            ClsResponse oClsResponse18 = await oCommonController.ExtractResponseFromActionResult(gstr9_6BInputServicesResult);

            var gstr9_6CInputsResult = await taxController.Gstr9_6CInputs(salesObj);
            ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(gstr9_6CInputsResult);

            var gstr9_6CCapitalGoodsResult = await taxController.Gstr9_6CCapitalGoods(salesObj);
            ClsResponse oClsResponse20 = await oCommonController.ExtractResponseFromActionResult(gstr9_6CCapitalGoodsResult);

            var gstr9_6CInputServicesResult = await taxController.Gstr9_6CInputServices(salesObj);
            ClsResponse oClsResponse21 = await oCommonController.ExtractResponseFromActionResult(gstr9_6CInputServicesResult);

            var gstr9_6DInputsResult = await taxController.Gstr9_6DInputs(salesObj);
            ClsResponse oClsResponse22 = await oCommonController.ExtractResponseFromActionResult(gstr9_6DInputsResult);

            var gstr9_6DCapitalGoodsResult = await taxController.Gstr9_6DCapitalGoods(salesObj);
            ClsResponse oClsResponse23 = await oCommonController.ExtractResponseFromActionResult(gstr9_6DCapitalGoodsResult);

            var gstr9_6DInputServicesResult = await taxController.Gstr9_6DInputServices(salesObj);
            ClsResponse oClsResponse24 = await oCommonController.ExtractResponseFromActionResult(gstr9_6DInputServicesResult);

            var gstr9_6EInputsResult = await taxController.Gstr9_6EInputs(salesObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(gstr9_6EInputsResult);

            var gstr9_6ECapitalGoodsResult = await taxController.Gstr9_6ECapitalGoods(salesObj);
            ClsResponse oClsResponse26 = await oCommonController.ExtractResponseFromActionResult(gstr9_6ECapitalGoodsResult);

            var gstr9_6FResult = await taxController.Gstr9_6F(salesObj);
            ClsResponse oClsResponse27 = await oCommonController.ExtractResponseFromActionResult(gstr9_6FResult);

            var gstr9_10Result = await taxController.Gstr9_10(salesObj);
            ClsResponse oClsResponse28 = await oCommonController.ExtractResponseFromActionResult(gstr9_10Result);

            var gstr9_11Result = await taxController.Gstr9_11(salesObj);
            ClsResponse oClsResponse29 = await oCommonController.ExtractResponseFromActionResult(gstr9_11Result);

            var gstr9_16AResult = await taxController.Gstr9_16A(salesObj);
            ClsResponse oClsResponse30 = await oCommonController.ExtractResponseFromActionResult(gstr9_16AResult);

            var gstr9_17Result = await taxController.Gstr9_17(salesObj);
            ClsResponse oClsResponse31 = await oCommonController.ExtractResponseFromActionResult(gstr9_17Result);

            var gstr9_18Result = await taxController.Gstr9_18(salesObj);
            ClsResponse oClsResponse32 = await oCommonController.ExtractResponseFromActionResult(gstr9_18Result);

            var gstr9_14Result = await taxController.Gstr9_14(salesObj);
            ClsResponse oClsResponse33 = await oCommonController.ExtractResponseFromActionResult(gstr9_14Result);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSetting taxSettingObj = new ClsTaxSetting { CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.TaxSetting(taxSettingObj);
            ClsResponse oClsResponse34 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController2 = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingVmObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingActiveResult = await taxSettingController2.ActiveTaxSettings(taxSettingVmObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingActiveResult);

            // Gstr9_4A
            ViewBag.Gstr9_4ATotalTaxableValue = oClsResponse.Data.TotalTaxableValue;
            ViewBag.Gstr9_4ATotalCGST = oClsResponse.Data.TotalCgstValue;
            ViewBag.Gstr9_4ATotalSGST = oClsResponse.Data.TotalSgstValue + oClsResponse.Data.TotalUtgstValue;
            ViewBag.Gstr9_4ATotalIGST = oClsResponse.Data.TotalIgstValue;
            ViewBag.Gstr9_4ATotalCESS = oClsResponse.Data.TotalCessValue;
            // Gstr9_4A

            // Gstr9_4B
            ViewBag.Gstr9_4BTotalTaxableValue = oClsResponse1.Data.TotalTaxableValue;
            ViewBag.Gstr9_4BTotalCGST = oClsResponse1.Data.TotalCgstValue;
            ViewBag.Gstr9_4BTotalSGST = oClsResponse1.Data.TotalSgstValue + oClsResponse1.Data.TotalUtgstValue;
            ViewBag.Gstr9_4BTotalIGST = oClsResponse1.Data.TotalIgstValue;
            ViewBag.Gstr9_4BTotalCESS = oClsResponse1.Data.TotalCessValue;
            // Gstr9_4B

            // Gstr9_4C
            ViewBag.Gstr9_4CTotalTaxableValue = oClsResponse2.Data.TotalTaxableValue;
            ViewBag.Gstr9_4CTotalCGST = oClsResponse2.Data.TotalCgstValue;
            ViewBag.Gstr9_4CTotalSGST = oClsResponse2.Data.TotalSgstValue + oClsResponse2.Data.TotalUtgstValue;
            ViewBag.Gstr9_4CTotalIGST = oClsResponse2.Data.TotalIgstValue;
            ViewBag.Gstr9_4CTotalCESS = oClsResponse2.Data.TotalCessValue;
            // Gstr9_4C

            // Gstr9_4D
            ViewBag.Gstr9_4DTotalTaxableValue = oClsResponse3.Data.TotalTaxableValue;
            ViewBag.Gstr9_4DTotalCGST = oClsResponse3.Data.TotalCgstValue;
            ViewBag.Gstr9_4DTotalSGST = oClsResponse3.Data.TotalSgstValue + oClsResponse3.Data.TotalUtgstValue;
            ViewBag.Gstr9_4DTotalIGST = oClsResponse3.Data.TotalIgstValue;
            ViewBag.Gstr9_4DTotalCESS = oClsResponse3.Data.TotalCessValue;
            // Gstr9_4D

            // Gstr9_4F
            ViewBag.Gstr9_4FTotalTaxableValue = oClsResponse4.Data.TotalTaxableValue;
            ViewBag.Gstr9_4FTotalCGST = oClsResponse4.Data.TotalCgstValue;
            ViewBag.Gstr9_4FTotalSGST = oClsResponse4.Data.TotalSgstValue + oClsResponse4.Data.TotalUtgstValue;
            ViewBag.Gstr9_4FTotalIGST = oClsResponse4.Data.TotalIgstValue;
            ViewBag.Gstr9_4FTotalCESS = oClsResponse4.Data.TotalCessValue;
            // Gstr9_4F

            // Gstr9_4G
            ViewBag.Gstr9_4GTotalTaxableValue = oClsResponse5.Data.TotalTaxableValue;
            ViewBag.Gstr9_4GTotalCGST = oClsResponse5.Data.TotalCgstValue;
            ViewBag.Gstr9_4GTotalSGST = oClsResponse5.Data.TotalSgstValue + oClsResponse5.Data.TotalUtgstValue;
            ViewBag.Gstr9_4GTotalIGST = oClsResponse5.Data.TotalIgstValue;
            ViewBag.Gstr9_4GTotalCESS = oClsResponse5.Data.TotalCessValue;
            // Gstr9_4G

            // Gstr9_4I
            ViewBag.Gstr9_4ITotalTaxableValue = oClsResponse6.Data.TotalTaxableValue;
            ViewBag.Gstr9_4ITotalCGST = oClsResponse6.Data.TotalCgstValue;
            ViewBag.Gstr9_4ITotalSGST = oClsResponse6.Data.TotalSgstValue + oClsResponse6.Data.TotalUtgstValue;
            ViewBag.Gstr9_4ITotalIGST = oClsResponse6.Data.TotalIgstValue;
            ViewBag.Gstr9_4ITotalCESS = oClsResponse6.Data.TotalCessValue;
            // Gstr9_4I

            // Gstr9_4J
            ViewBag.Gstr9_4JTotalTaxableValue = oClsResponse7.Data.TotalTaxableValue;
            ViewBag.Gstr9_4JTotalCGST = oClsResponse7.Data.TotalCgstValue;
            ViewBag.Gstr9_4JTotalSGST = oClsResponse7.Data.TotalSgstValue + oClsResponse7.Data.TotalUtgstValue;
            ViewBag.Gstr9_4JTotalIGST = oClsResponse7.Data.TotalIgstValue;
            ViewBag.Gstr9_4JTotalCESS = oClsResponse7.Data.TotalCessValue;
            // Gstr9_4J

            // Gstr9_5A
            ViewBag.Gstr9_5ATotalTaxableValue = oClsResponse8.Data.TotalTaxableValue;
            ViewBag.Gstr9_5ATotalCGST = oClsResponse8.Data.TotalCgstValue;
            ViewBag.Gstr9_5ATotalSGST = oClsResponse8.Data.TotalSgstValue + oClsResponse8.Data.TotalUtgstValue;
            ViewBag.Gstr9_5ATotalIGST = oClsResponse8.Data.TotalIgstValue;
            ViewBag.Gstr9_5ATotalCESS = oClsResponse8.Data.TotalCessValue;
            // Gstr9_5A

            // Gstr9_5B
            ViewBag.Gstr9_5BTotalTaxableValue = oClsResponse9.Data.TotalTaxableValue;
            ViewBag.Gstr9_5BTotalCGST = oClsResponse9.Data.TotalCgstValue;
            ViewBag.Gstr9_5BTotalSGST = oClsResponse9.Data.TotalSgstValue + oClsResponse9.Data.TotalUtgstValue;
            ViewBag.Gstr9_5BTotalIGST = oClsResponse9.Data.TotalIgstValue;
            ViewBag.Gstr9_5BTotalCESS = oClsResponse9.Data.TotalCessValue;
            // Gstr9_5B

            // Gstr9_5C
            ViewBag.Gstr9_5CTotalTaxableValue = oClsResponse10.Data.TotalTaxableValue;
            ViewBag.Gstr9_5CTotalCGST = oClsResponse10.Data.TotalCgstValue;
            ViewBag.Gstr9_5CTotalSGST = oClsResponse10.Data.TotalSgstValue + oClsResponse10.Data.TotalUtgstValue;
            ViewBag.Gstr9_5CTotalIGST = oClsResponse10.Data.TotalIgstValue;
            ViewBag.Gstr9_5CTotalCESS = oClsResponse10.Data.TotalCessValue;
            // Gstr9_5C

            // Gstr9_5D
            ViewBag.Gstr9_5DTotalTaxableValue = oClsResponse11.Data.TotalTaxableValue;
            ViewBag.Gstr9_5DTotalCGST = oClsResponse11.Data.TotalCgstValue;
            ViewBag.Gstr9_5DTotalSGST = oClsResponse11.Data.TotalSgstValue + oClsResponse11.Data.TotalUtgstValue;
            ViewBag.Gstr9_5DTotalIGST = oClsResponse11.Data.TotalIgstValue;
            ViewBag.Gstr9_5DTotalCESS = oClsResponse11.Data.TotalCessValue;
            // Gstr9_5D

            // GstrGstr9_5E
            ViewBag.Gstr9_5ETotalTaxableValue = oClsResponse12.Data.TotalTaxableValue;
            ViewBag.Gstr9_5ETotalCGST = oClsResponse12.Data.TotalCgstValue;
            ViewBag.Gstr9_5ETotalSGST = oClsResponse12.Data.TotalSgstValue + oClsResponse12.Data.TotalUtgstValue;
            ViewBag.Gstr9_5ETotalIGST = oClsResponse12.Data.TotalIgstValue;
            ViewBag.Gstr9_5ETotalCESS = oClsResponse12.Data.TotalCessValue;
            // Gstr9_5E

            // Gstr9_5F
            ViewBag.Gstr9_5FTotalTaxableValue = oClsResponse13.Data.TotalTaxableValue;
            ViewBag.Gstr9_5FTotalCGST = oClsResponse13.Data.TotalCgstValue;
            ViewBag.Gstr9_5FTotalSGST = oClsResponse13.Data.TotalSgstValue + oClsResponse13.Data.TotalUtgstValue;
            ViewBag.Gstr9_5FTotalIGST = oClsResponse13.Data.TotalIgstValue;
            ViewBag.Gstr9_5FTotalCESS = oClsResponse13.Data.TotalCessValue;
            // Gstr9_5F

            // Gstr9_5H
            ViewBag.Gstr9_5HTotalTaxableValue = oClsResponse14.Data.TotalTaxableValue;
            ViewBag.Gstr9_5HTotalCGST = oClsResponse14.Data.TotalCgstValue;
            ViewBag.Gstr9_5HTotalSGST = oClsResponse14.Data.TotalSgstValue + oClsResponse14.Data.TotalUtgstValue;
            ViewBag.Gstr9_5HTotalIGST = oClsResponse14.Data.TotalIgstValue;
            ViewBag.Gstr9_5HTotalCESS = oClsResponse14.Data.TotalCessValue;
            // Gstr9_5H

            // Gstr9_5I
            ViewBag.Gstr9_5ITotalTaxableValue = oClsResponse15.Data.TotalTaxableValue;
            ViewBag.Gstr9_5ITotalCGST = oClsResponse15.Data.TotalCgstValue;
            ViewBag.Gstr9_5ITotalSGST = oClsResponse15.Data.TotalSgstValue + oClsResponse15.Data.TotalUtgstValue;
            ViewBag.Gstr9_5ITotalIGST = oClsResponse15.Data.TotalIgstValue;
            ViewBag.Gstr9_5ITotalCESS = oClsResponse15.Data.TotalCessValue;
            // Gstr9_5I

            // Gstr9_6BInputs
            ViewBag.Gstr9_6BInputsTotalCGST = oClsResponse16.Data.TotalCgstValue;
            ViewBag.Gstr9_6BInputsTotalSGST = oClsResponse16.Data.TotalSgstValue + oClsResponse16.Data.TotalUtgstValue;
            ViewBag.Gstr9_6BInputsTotalIGST = oClsResponse16.Data.TotalIgstValue;
            ViewBag.Gstr9_6BInputsTotalCESS = oClsResponse16.Data.TotalCessValue;
            // Gstr9_6BInputs

            // Gstr9_6BCapitalGoods
            ViewBag.Gstr9_6BCapitalGoodsTotalCGST = oClsResponse17.Data.TotalCgstValue;
            ViewBag.Gstr9_6BCapitalGoodsTotalSGST = oClsResponse17.Data.TotalSgstValue + oClsResponse17.Data.TotalUtgstValue;
            ViewBag.Gstr9_6BCapitalGoodsTotalIGST = oClsResponse17.Data.TotalIgstValue;
            ViewBag.Gstr9_6BCapitalGoodsTotalCESS = oClsResponse17.Data.TotalCessValue;
            // Gstr9_6BCapitalGoods

            // Gstr9_6BInputServices
            ViewBag.Gstr9_6BInputServicesTotalCGST = oClsResponse18.Data.TotalCgstValue;
            ViewBag.Gstr9_6BInputServicesTotalSGST = oClsResponse18.Data.TotalSgstValue + oClsResponse18.Data.TotalUtgstValue;
            ViewBag.Gstr9_6BInputServicesTotalIGST = oClsResponse18.Data.TotalIgstValue;
            ViewBag.Gstr9_6BInputServicesTotalCESS = oClsResponse18.Data.TotalCessValue;
            // Gstr9_6BInputServices

            // Gstr9_6CInputs
            ViewBag.Gstr9_6CInputsTotalCGST = oClsResponse19.Data.TotalCgstValue;
            ViewBag.Gstr9_6CInputsTotalSGST = oClsResponse19.Data.TotalSgstValue + oClsResponse19.Data.TotalUtgstValue;
            ViewBag.Gstr9_6CInputsTotalIGST = oClsResponse19.Data.TotalIgstValue;
            ViewBag.Gstr9_6CInputsTotalCESS = oClsResponse19.Data.TotalCessValue;
            // Gstr9_6CInputs

            // Gstr9_6CCapitalGoods
            ViewBag.Gstr9_6CCapitalGoodsTotalCGST = oClsResponse20.Data.TotalCgstValue;
            ViewBag.Gstr9_6CCapitalGoodsTotalSGST = oClsResponse20.Data.TotalSgstValue + oClsResponse20.Data.TotalUtgstValue;
            ViewBag.Gstr9_6CCapitalGoodsTotalIGST = oClsResponse20.Data.TotalIgstValue;
            ViewBag.Gstr9_6CCapitalGoodsTotalCESS = oClsResponse20.Data.TotalCessValue;
            // Gstr9_6CCapitalGoods

            // Gstr9_6CInputServices
            ViewBag.Gstr9_6CInputServicesTotalCGST = oClsResponse21.Data.TotalCgstValue;
            ViewBag.Gstr9_6CInputServicesTotalSGST = oClsResponse21.Data.TotalSgstValue + oClsResponse21.Data.TotalUtgstValue;
            ViewBag.Gstr9_6CInputServicesTotalIGST = oClsResponse21.Data.TotalIgstValue;
            ViewBag.Gstr9_6CInputServicesTotalCESS = oClsResponse21.Data.TotalCessValue;
            // Gstr9_6CInputServices

            // Gstr9_6DInputs
            ViewBag.Gstr9_6DInputsTotalCGST = oClsResponse22.Data.TotalCgstValue;
            ViewBag.Gstr9_6DInputsTotalSGST = oClsResponse22.Data.TotalSgstValue + oClsResponse22.Data.TotalUtgstValue;
            ViewBag.Gstr9_6DInputsTotalIGST = oClsResponse22.Data.TotalIgstValue;
            ViewBag.Gstr9_6DInputsTotalCESS = oClsResponse22.Data.TotalCessValue;
            // Gstr9_6DInputs

            // Gstr9_6DCapitalGoods
            ViewBag.Gstr9_6DCapitalGoodsTotalCGST = oClsResponse23.Data.TotalCgstValue;
            ViewBag.Gstr9_6DCapitalGoodsTotalSGST = oClsResponse23.Data.TotalSgstValue + oClsResponse23.Data.TotalUtgstValue;
            ViewBag.Gstr9_6DCapitalGoodsTotalIGST = oClsResponse23.Data.TotalIgstValue;
            ViewBag.Gstr9_6DCapitalGoodsTotalCESS = oClsResponse23.Data.TotalCessValue;
            // Gstr9_6DCapitalGoods

            // Gstr9_6DInputServices
            ViewBag.Gstr9_6DInputServicesTotalCGST = oClsResponse24.Data.TotalCgstValue;
            ViewBag.Gstr9_6DInputServicesTotalSGST = oClsResponse24.Data.TotalSgstValue + oClsResponse24.Data.TotalUtgstValue;
            ViewBag.Gstr9_6DInputServicesTotalIGST = oClsResponse24.Data.TotalIgstValue;
            ViewBag.Gstr9_6DInputServicesTotalCESS = oClsResponse24.Data.TotalCessValue;
            // Gstr9_6DInputServices

            // Gstr9_6EInputs
            ViewBag.Gstr9_6EInputsTotalIGST = oClsResponse25.Data.TotalIgstValue;
            ViewBag.Gstr9_6EInputsTotalCESS = oClsResponse25.Data.TotalCessValue;
            // Gstr9_6EInputs

            // Gstr9_6ECapitalGoods
            ViewBag.Gstr9_6ECapitalGoodsTotalIGST = oClsResponse26.Data.TotalIgstValue;
            ViewBag.Gstr9_6ECapitalGoodsTotalCESS = oClsResponse26.Data.TotalCessValue;
            // Gstr9_6ECapitalGoods

            // Gstr9_6F
            ViewBag.Gstr9_6FTotalIGST = oClsResponse27.Data.TotalIgstValue;
            ViewBag.Gstr9_6FTotalCESS = oClsResponse27.Data.TotalCessValue;
            // Gstr9_6F

            // Gstr9_10
            ViewBag.Gstr9_10TotalTaxableValue = oClsResponse28.Data.TotalTaxableValue;
            ViewBag.Gstr9_10TotalCGST = oClsResponse28.Data.TotalCgstValue;
            ViewBag.Gstr9_10TotalSGST = oClsResponse28.Data.TotalSgstValue + oClsResponse28.Data.TotalUtgstValue;
            ViewBag.Gstr9_10TotalIGST = oClsResponse28.Data.TotalIgstValue;
            ViewBag.Gstr9_10TotalCESS = oClsResponse28.Data.TotalCessValue;
            // Gstr9_10

            // Gstr9_11
            ViewBag.Gstr9_11TotalTaxableValue = oClsResponse29.Data.TotalTaxableValue;
            ViewBag.Gstr9_11TotalCGST = oClsResponse29.Data.TotalCgstValue;
            ViewBag.Gstr9_11TotalSGST = oClsResponse29.Data.TotalSgstValue + oClsResponse29.Data.TotalUtgstValue;
            ViewBag.Gstr9_11TotalIGST = oClsResponse29.Data.TotalIgstValue;
            ViewBag.Gstr9_11TotalCESS = oClsResponse29.Data.TotalCessValue;
            // Gstr9_11

            // Gstr9_16A
            ViewBag.Gstr9_16ATotalTaxableValue = oClsResponse30.Data.TotalTaxableValue;
            // Gstr9_16A

            // Gstr9_17
            ViewBag.Gstr9_17 = oClsResponse31.Data.Sales;
            // Gstr9_17

            // Gstr9_18
            ViewBag.Gstr9_18 = oClsResponse32.Data.Sales;
            // Gstr9_18

            // Gstr9_14
            ViewBag.Gstr9_14TotalCGST = oClsResponse33.Data.TotalCgstValue;
            ViewBag.Gstr9_14TotalSGST = oClsResponse33.Data.TotalSgstValue + oClsResponse33.Data.TotalUtgstValue;
            ViewBag.Gstr9_14TotalIGST = oClsResponse33.Data.TotalIgstValue;
            ViewBag.Gstr9_14TotalCESS = oClsResponse3.Data.TotalCessValue;
            // Gstr9_14

            ViewBag.TaxSetting = oClsResponse34.Data.TaxSetting;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9");
        }

        public async Task<ActionResult> Gstr9Details(ClsAccountVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Type = obj.Type };

            WebApi.TaxController taxController = new WebApi.TaxController();
            IHttpActionResult taxResult = null;

            if (obj.Type == "4A")
            {
                taxResult = await taxController.Gstr9_4A(salesObj);
                ViewBag.PageTitle = "4A. Supplies made to un-registered persons (B2C)";
            }
            else if (obj.Type == "4B")
            {
                taxResult = await taxController.Gstr9_4B(salesObj);
                ViewBag.PageTitle = "4B. Supplies made to registered persons (B2B)";
            }
            else if (obj.Type == "4C")
            {
                taxResult = await taxController.Gstr9_4C(salesObj);
                ViewBag.PageTitle = "4C. Zero rated supply (Export) on payment of tax (except supplies to SEZs)";
            }
            else if (obj.Type == "4D")
            {
                taxResult = await taxController.Gstr9_4D(salesObj);
                ViewBag.PageTitle = "4D. Supply to SEZs on payment of tax";
            }
            else if (obj.Type == "4F")
            {
                taxResult = await taxController.Gstr9_4F(salesObj);
                ViewBag.PageTitle = "4F. Advances on which tax has been paid but invoice has not been issued (not covered under (A) to (E) above)";
            }
            else if (obj.Type == "4G")
            {
                taxResult = await taxController.Gstr9_4G(salesObj);
                ViewBag.PageTitle = "4G. Inward supplies on which tax is to be paid on reverse charge basis";
            }
            else if (obj.Type == "4I")
            {
                taxResult = await taxController.Gstr9_4I(salesObj);
                ViewBag.PageTitle = "4I. Credit Notes issued in respect of transactions specified in (B) to (E) above (-)";
            }
            else if (obj.Type == "4J")
            {
                taxResult = await taxController.Gstr9_4J(salesObj);
                ViewBag.PageTitle = "4J. Debit Notes issued in respect of transactions specified in (B) to (E) above (+)";
            }
            else if (obj.Type == "5A")
            {
                taxResult = await taxController.Gstr9_5A(salesObj);
                ViewBag.PageTitle = "5A. Zero rated supply (Export) without payment of tax";
            }
            else if (obj.Type == "5B")
            {
                taxResult = await taxController.Gstr9_5B(salesObj);
                ViewBag.PageTitle = "5B. Supply to SEZs without payment of tax";
            }
            else if (obj.Type == "5C")
            {
                taxResult = await taxController.Gstr9_5C(salesObj);
                ViewBag.PageTitle = "5C. Supplies on which tax is to be paid by the recipient on\r\nreverse charge basis";
            }
            else if (obj.Type == "5D")
            {
                taxResult = await taxController.Gstr9_5D(salesObj);
                ViewBag.PageTitle = "5D. Exempted";
            }
            else if (obj.Type == "5E")
            {
                taxResult = await taxController.Gstr9_5E(salesObj);
                ViewBag.PageTitle = "5E. Nil Rated";
            }
            else if (obj.Type == "5F")
            {
                taxResult = await taxController.Gstr9_5F(salesObj);
                ViewBag.PageTitle = "5F. Non-GST supply";
            }
            else if (obj.Type == "5H")
            {
                taxResult = await taxController.Gstr9_5H(salesObj);
                ViewBag.PageTitle = "5H. Credit Notes issued in respect of transactions specified\r\nin A to F above (-)";
            }
            else if (obj.Type == "5I")
            {
                taxResult = await taxController.Gstr9_5I(salesObj);
                ViewBag.PageTitle = "5I. Debit Notes issued in respect of transactions specified\r\nin A to F above (+)";
            }
            else if (obj.Type == "6BInputs")
            {
                taxResult = await taxController.Gstr9_6BInputs(salesObj);
                ViewBag.PageTitle = "6B Inputs. Inward supplies (other than imports and inward supplies\r\nliable to reverse charge but includes services received\r\nfrom SEZs)";
            }
            else if (obj.Type == "6BCapitalGoods")
            {
                taxResult = await taxController.Gstr9_6BCapitalGoods(salesObj);
                ViewBag.PageTitle = "6B Capital Goods. Inward supplies (other than imports and inward supplies\r\nliable to reverse charge but includes services received\r\nfrom SEZs)";
            }
            else if (obj.Type == "6BInputServices")
            {
                taxResult = await taxController.Gstr9_6BInputServices(salesObj);
                ViewBag.PageTitle = "6B Input Services. Inward supplies (other than imports and inward supplies\r\nliable to reverse charge but includes services received\r\nfrom SEZs)";
            }
            else if (obj.Type == "6CInputs")
            {
                taxResult = await taxController.Gstr9_6CInputs(salesObj);
                ViewBag.PageTitle = "6C Inputs. Inward supplies received from unregistered persons\r\nliable to reverse charge (other than B above) on which\r\ntax is paid & ITC availed";
            }
            else if (obj.Type == "6CCapitalGoods")
            {
                taxResult = await taxController.Gstr9_6CCapitalGoods(salesObj);
                ViewBag.PageTitle = "6C Capital Goods. Inward supplies received from unregistered persons\r\nliable to reverse charge (other than B above) on which\r\ntax is paid & ITC availed";
            }
            else if (obj.Type == "6CInputServices")
            {
                taxResult = await taxController.Gstr9_6CInputServices(salesObj);
                ViewBag.PageTitle = "6C Input Services. Inward supplies received from unregistered persons\r\nliable to reverse charge (other than B above) on which\r\ntax is paid & ITC availed";
            }
            else if (obj.Type == "6DInputs")
            {
                taxResult = await taxController.Gstr9_6DInputs(salesObj);
                ViewBag.PageTitle = "6D Inputs. Inward supplies received from registered persons liable\r\nto reverse charge (other than B above) on which tax is\r\npaid and ITC availed";
            }
            else if (obj.Type == "6DCapitalGoods")
            {
                taxResult = await taxController.Gstr9_6DCapitalGoods(salesObj);
                ViewBag.PageTitle = "6D Capital Goods. Inward supplies received from registered persons liable\r\nto reverse charge (other than B above) on which tax is\r\npaid and ITC availed";
            }
            else if (obj.Type == "6DInputServices")
            {
                taxResult = await taxController.Gstr9_6DInputServices(salesObj);
                ViewBag.PageTitle = "6D Input Services. Inward supplies received from registered persons liable\r\nto reverse charge (other than B above) on which tax is\r\npaid and ITC availed";
            }
            else if (obj.Type == "6EInputs")
            {
                taxResult = await taxController.Gstr9_6EInputs(salesObj);
                ViewBag.PageTitle = "6E Inputs. Import of goods (including supplies from SEZs)";
            }
            else if (obj.Type == "6ECapitalGoods")
            {
                taxResult = await taxController.Gstr9_6ECapitalGoods(salesObj);
                ViewBag.PageTitle = "6E Capital Goods. Import of goods (including supplies from SEZs)";
            }
            else if (obj.Type == "6F")
            {
                taxResult = await taxController.Gstr9_6F(salesObj);
                ViewBag.PageTitle = "6F. Import of services (excluding inward supplies from SEZs)";
            }
            else if (obj.Type == "10")
            {
                taxResult = await taxController.Gstr9_10(salesObj);
                ViewBag.PageTitle = "Supplies / tax declared through Amendments (+) (net of debit notes)";
            }
            else if (obj.Type == "11")
            {
                taxResult = await taxController.Gstr9_11(salesObj);
                ViewBag.PageTitle = "Supplies / tax reduced through Amendments (-) (net of credit notes)";
            }
            else if (obj.Type == "16A")
            {
                taxResult = await taxController.Gstr9_16A(salesObj);
                ViewBag.PageTitle = "16A. Supplies received from Composition taxpayers";
            }
            else if (obj.Type == "17")
            {
                taxResult = await taxController.Gstr9_17Details(salesObj);
                ViewBag.PageTitle = "17. HSN Wise Summary of outward supplies";
            }
            else if (obj.Type == "18")
            {
                taxResult = await taxController.Gstr9_18Details(salesObj);
                ViewBag.PageTitle = "18. HSN Wise Summary of Inward supplies";
            }

            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.Sales = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr3b").FirstOrDefault();

            ViewBag.Type = obj.Type;

            return View();
        }


        public async Task<ActionResult> Gstr9A()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }

        public async Task<ActionResult> Gstr9AFetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            var b2cLargeResult = await taxController.B2CLarge(salesObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(b2cLargeResult);

            var b2cSmallResult = await taxController.B2CSmall(salesObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(b2cSmallResult);

            var exportsResult = await taxController.Exports(salesObj);
            ClsResponse oClsResponse3 = await oCommonController.ExtractResponseFromActionResult(exportsResult);

            var creditDebitRegisteredResult = await taxController.CreditDebitRegistered(salesObj);
            ClsResponse oClsResponse4 = await oCommonController.ExtractResponseFromActionResult(creditDebitRegisteredResult);

            var creditDebitUnRegisteredResult = await taxController.CreditDebitUnRegistered(salesObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(creditDebitUnRegisteredResult);

            var advancesReceivedResult = await taxController.AdvancesReceived(salesObj);
            ClsResponse oClsResponse6 = await oCommonController.ExtractResponseFromActionResult(advancesReceivedResult);

            var adjustmentOfAdvancesResult = await taxController.AdjustmentOfAdvances(salesObj);
            ClsResponse oClsResponse7 = await oCommonController.ExtractResponseFromActionResult(adjustmentOfAdvancesResult);

            var hsnWiseSummaryResult = await taxController.HsnWiseSummary(salesObj);
            ClsResponse oClsResponse8 = await oCommonController.ExtractResponseFromActionResult(hsnWiseSummaryResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }

        public async Task<ActionResult> TaxSummary()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            ClsBankPaymentVm bankPaymentObj = new ClsBankPaymentVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var taxSummaryReportResult = await taxController.TaxSummaryReport(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxSummaryReportResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.Taxs = oClsResponse.Data.Taxs;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            //ViewBag.CurrentPageIndex = obj.PageIndex;
            //ViewBag.PageSize = oClsResponse.Data.PageSize;
            //ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax summary").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> TaxSummaryFetch(ClsAccountVm obj)
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
            ClsBankPaymentVm bankPaymentObj = new ClsBankPaymentVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var taxSummaryReportResult = await taxController.TaxSummaryReport(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxSummaryReportResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.Taxs = oClsResponse.Data.Taxs;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            //ViewBag.CurrentPageIndex = obj.PageIndex;
            //ViewBag.PageSize = oClsResponse.Data.PageSize;
            //ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax summary").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialTaxSummary");
        }

        public async Task<ActionResult> TaxSummaryDetails(ClsBankPaymentVm obj)
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
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }
            ViewBag.Tax = obj.Tax;
            ClsBankPaymentVm bankPaymentObj = new ClsBankPaymentVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId, Tax = obj.Tax };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var taxSummaryDetailsReportResult = await taxController.TaxSummaryDetailsReport(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxSummaryDetailsReportResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            BranchController branchController = new BranchController();
            ClsBranchVm branchObj = new ClsBranchVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var branchResult = await branchController.ActiveBranchs(branchObj);
            ClsResponse oClsResponse25 = await oCommonController.ExtractResponseFromActionResult(branchResult);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;
            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / oClsResponse.Data.PageSize);
            //ViewBag.CurrentPageIndex = obj.PageIndex;
            //ViewBag.PageSize = oClsResponse.Data.PageSize;
            //ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax summary").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            ViewBag.Branchs = oClsResponse25.Data.Branchs;

            ViewBag.BranchId = obj.BranchId;

            return View();
        }
        public async Task<ActionResult> TaxSummaryDetailsFetch(ClsAccountVm obj)
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
            ClsBankPaymentVm bankPaymentObj = new ClsBankPaymentVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var taxSummaryDetailsReportResult = await taxController.TaxSummaryDetailsReport(bankPaymentObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(taxSummaryDetailsReportResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            ViewBag.BankPayments = oClsResponse.Data.BankPayments;

            ViewBag.TotalCount = oClsResponse.Data.TotalCount;

            //ViewBag.PageCount = (int)Math.Ceiling((double)oClsResponse.Data.TotalCount / obj.PageSize);
            //ViewBag.CurrentPageIndex = obj.PageIndex;
            //ViewBag.PageSize = oClsResponse.Data.PageSize;
            //ViewBag.PageIndex = obj.PageIndex;

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "tax summary").FirstOrDefault();
            ViewBag.ExpensePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.ContraPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "contra").FirstOrDefault();
            ViewBag.JournalPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "journal").FirstOrDefault();
            ViewBag.SalesPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "sales").FirstOrDefault();
            ViewBag.SalesReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "credit note").FirstOrDefault();
            ViewBag.PurchasePermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "purchase").FirstOrDefault();
            ViewBag.PurchaseReturnPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "debit note").FirstOrDefault();
            ViewBag.CustomerPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer payment").FirstOrDefault();
            ViewBag.CustomerRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "customer refund").FirstOrDefault();
            ViewBag.SupplierPaymentPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier payment").FirstOrDefault();
            ViewBag.SupplierRefundPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "supplier refund").FirstOrDefault();
            ViewBag.StockAdjustPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "stock adjust").FirstOrDefault();

            ViewBag.BranchId = obj.BranchId;

            return PartialView("PartialTaxSummaryDetails");
        }
        public async Task<ActionResult> TaxBySales()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> TaxBySalesFetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> TaxByPurchase()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> TaxByPurchaseFetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
        public async Task<ActionResult> TaxByExpense()
        {
            ClsAccountVm obj = new ClsAccountVm();
            string[] arr = { "", "", "" };
            if (Request.Cookies["data"] != null)
            {
                arr[0] = Request.Cookies["data"]["UserType"];
                arr[1] = Request.Cookies["data"]["Token"];
                arr[2] = Request.Cookies["data"]["Id"];
                obj.AddedBy = Convert.ToInt64(arr[2]);
                obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
                //ViewBag.CurrencySymbol = Request.Cookies["data"]["CurrencySymbol"];
                obj.PageIndex = 1;
                //obj.PageSize = 10;
            }

            obj.PageIndex = 1;
            obj.PageSize = 10000000;

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return View();
        }
        public async Task<ActionResult> TaxByExpenseFetch(ClsAccountVm obj)
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

            ClsSalesVm salesObj = new ClsSalesVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId, PageIndex = obj.PageIndex, PageSize = obj.PageSize, FromDate = obj.FromDate, ToDate = obj.ToDate, BranchId = obj.BranchId };

            WebApi.TaxController taxController = new WebApi.TaxController();
            var b2bResult = await taxController.B2B(salesObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(b2bResult);

            ClsMenuVm menuObj = new ClsMenuVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            MenuController menuController = new MenuController();
            var menuResult = await menuController.ControlsPermission(menuObj);
            ClsResponse oClsResponse35 = await oCommonController.ExtractResponseFromActionResult(menuResult);

            WebApi.BusinessSettingsController businessSettingsController = new WebApi.BusinessSettingsController();
            ClsBusinessSettingsVm businessSettingsObj = new ClsBusinessSettingsVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessSettingsResult = await businessSettingsController.BusinessSetting(businessSettingsObj);
            ClsResponse oClsResponse39 = await oCommonController.ExtractResponseFromActionResult(businessSettingsResult);

            TaxTypeController taxTypeController = new TaxTypeController();
            ClsTaxTypeVm taxTypeObj = new ClsTaxTypeVm { CompanyId = obj.CompanyId };
            var taxTypeResult = await taxTypeController.ActiveTaxTypes(taxTypeObj);
            ClsResponse oClsResponse51 = await oCommonController.ExtractResponseFromActionResult(taxTypeResult);

            WebApi.BusinessRegistrationNameController businessRegistrationNameController = new WebApi.BusinessRegistrationNameController();
            ClsBusinessRegistrationNameVm businessRegistrationNameObj = new ClsBusinessRegistrationNameVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var businessRegistrationNameResult = await businessRegistrationNameController.ActiveBusinessRegistrationNames(businessRegistrationNameObj);
            ClsResponse oClsResponse55 = await oCommonController.ExtractResponseFromActionResult(businessRegistrationNameResult);

            WebApi.TaxSettingController taxSettingController = new WebApi.TaxSettingController();
            ClsTaxSettingVm taxSettingObj = new ClsTaxSettingVm { AddedBy = obj.AddedBy, CompanyId = obj.CompanyId };
            var taxSettingResult = await taxSettingController.ActiveTaxSettings(taxSettingObj);
            ClsResponse oClsResponse56 = await oCommonController.ExtractResponseFromActionResult(taxSettingResult);

            ViewBag.FromDate = oClsResponse.Data.FromDate;
            ViewBag.ToDate = oClsResponse.Data.ToDate;

            ViewBag.B2B = oClsResponse.Data.Sales;

            ViewBag.MenuPermission = oClsResponse35.Data.MenuPermissions.Where(a => a.Menu.ToLower() == "gstr1").FirstOrDefault();
            ViewBag.BusinessSetting = oClsResponse39.Data.BusinessSetting;
            ViewBag.TaxTypes = oClsResponse51.Data.TaxTypes;
            ViewBag.BusinessRegistrationNames = oClsResponse55.Data.BusinessRegistrationNames;
            ViewBag.TaxSettings = oClsResponse56.Data.TaxSettings;

            return PartialView("PartialGstr9A");
        }
    }
}