using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Website
{
    public class PricingController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: Pricing
        public async Task<ActionResult> Index(ClsUserVm obj)
        {
            // SEO Meta Tags for Pricing Page
            ViewBag.PageTitle = "Affordable GST Billing Software Plans | Starting ₹99/Month | EquiBillBook";
            ViewBag.MetaDescription = "Choose the perfect GST billing software plan for your business. Flexible pricing starting from ₹99/month with 30-day free trial. Custom plans, no hidden costs.";
            ViewBag.MetaKeywords = "GST billing software pricing, billing software cost India, accounting software price, GST software subscription, business software pricing, affordable billing software, free trial GST software, custom pricing plans India";

            // Open Graph Tags
            ViewBag.OgTitle = "Affordable GST Billing Software Plans | Starting ₹99/Month | EquiBillBook";
            ViewBag.OgDescription = "Affordable GST billing software pricing starting ₹99/month. 30-day free trial, custom plans, no hidden costs. Choose the perfect plan for your business size and needs.";
            ViewBag.OgImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";

            // Twitter Tags  
            ViewBag.TwitterTitle = "GST Billing Software Pricing | Starting ₹99/Month | EquiBillBook";
            ViewBag.TwitterDescription = "Flexible pricing starting ₹99/month. 30-day free trial, custom plans, no hidden costs. Find the perfect GST billing plan for your business!";
            ViewBag.TwitterImage = "https://equibillbook.com/Content/web-assets/images/innerbanner.png";

            // Canonical URL
            ViewBag.CanonicalUrl = "https://equibillbook.com/pricing";

            obj.Domain = Request.Url.Host.Replace("www.", "");
            ////obj.CountryId = 2;

            obj.TermLengthId = 4;

            // Initialize API controllers
            DomainController domainController = new DomainController();
            CountryController countryController = new CountryController();
            PlanController planController = new PlanController();
            TermLengthController termLengthController = new TermLengthController();
            PlanAddonsController planAddonsController = new PlanAddonsController();

            // Call API methods directly
            ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
            var domainResult = await domainController.DomainCheckForRedirection(domainObj);
            ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

            ClsUserCountryMapVm countryObj = new ClsUserCountryMapVm { Domain = obj.Domain };
            var countryResult = await countryController.ActiveCountrysMapped(countryObj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(countryResult);

            ClsPlanVm planObj = new ClsPlanVm();
            var planResult = await planController.ActivePlans(planObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(planResult);

            ClsTermLengthVm termLengthObj = new ClsTermLengthVm { Domain = obj.Domain, TermLengthId = obj.TermLengthId };
            var termLengthResult = await termLengthController.ActiveTermLengths(termLengthObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(termLengthResult);

            ClsPlanAddonsVm planAddonsObj = new ClsPlanAddonsVm { Domain = obj.Domain, TermLengthId = obj.TermLengthId };
            var planAddonsResult = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            ClsCountryVm mainCountryObj = new ClsCountryVm { Domain = obj.Domain };
            var mainCountryResult = await countryController.MainCountry(mainCountryObj);
            ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(mainCountryResult);

            ViewBag.Countrys = oClsResponse.Data.Countrys;
            ViewBag.Plans = oClsResponse2.Data.Plans;
            ViewBag.TermLengths = oClsResponse5.Data.TermLengths;
            ViewBag.PlanAddons = oClsResponse10.Data.PlanAddons;
            ViewBag.Country = oClsResponse10.Data.Country;
            ViewBag.MainCountry = oClsResponse19.Data.Country;
            ////ViewBag.IsAutoFetch = true;

            if (obj.Domain == "localhost" || obj.Domain == "equibillbook.com" || obj.Domain == "equitechsoftwares.in" || obj.Domain == "demo1.equitechsoftwares.com")
            {
                return View();
            }
            else
            {
                if (oClsResponse1.WhitelabelType == 0)
                {
                    return Redirect("/errorpage/domain");
                }
                else if (oClsResponse1.WhitelabelType == 2 || oClsResponse1.WhitelabelType == 1)
                {
                    return Redirect("/login");
                }
            }
            return View();
        }

        public async Task<ActionResult> FetchPricing(ClsUserVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            obj.TermLengthId = 4;

            // Initialize API controllers
            PlanController planController = new PlanController();
            TermLengthController termLengthController = new TermLengthController();
            PlanAddonsController planAddonsController = new PlanAddonsController();
            CountryController countryController = new CountryController();

            // Call API methods directly
            ClsPlanVm planObj = new ClsPlanVm();
            var planResult = await planController.ActivePlans(planObj);
            ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(planResult);

            ClsTermLengthVm termLengthObj = new ClsTermLengthVm { Domain = obj.Domain, TermLengthId = obj.TermLengthId };
            var termLengthResult = await termLengthController.ActiveTermLengths(termLengthObj);
            ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(termLengthResult);

            ClsPlanAddonsVm planAddonsObj = new ClsPlanAddonsVm { Domain = obj.Domain, TermLengthId = obj.TermLengthId };
            var planAddonsResult = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            ClsCountryVm mainCountryObj = new ClsCountryVm { Domain = obj.Domain };
            var mainCountryResult = await countryController.MainCountry(mainCountryObj);
            ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(mainCountryResult);

            ViewBag.Plans = oClsResponse2.Data.Plans;
            ViewBag.TermLengths = oClsResponse5.Data.TermLengths;
            ViewBag.PlanAddons = oClsResponse10.Data.PlanAddons;
            ViewBag.Country = oClsResponse10.Data.Country;
            ViewBag.MainCountry = oClsResponse19.Data.Country;
            //ViewBag.IsAutoFetch = true;

            return PartialView("PartialPricing");
        }

        public async Task<ActionResult> fetchAdons(ClsPlanAddonsVm obj)
        {
            obj.Domain = Request.Url.Host.Replace("www.", "");
            //if (Request.Cookies["data"] != null)
            //{
            //    obj.AddedBy = Convert.ToInt64(Request.Cookies["data"]["Id"]);
            //    obj.CompanyId = Convert.ToInt64(Request.Cookies["data"]["CompanyId"]);
            //}

            // Initialize API controllers
            PlanAddonsController planAddonsController = new PlanAddonsController();
            CountryController countryController = new CountryController();

            // Call API methods directly
            ClsPlanAddonsVm planAddonsObj = new ClsPlanAddonsVm { Domain = obj.Domain, TermLengthId = obj.TermLengthId, CompanyId = obj.CompanyId };
            var planAddonsResult = await planAddonsController.ActivePlanAddons(planAddonsObj);
            ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

            ClsCountryVm mainCountryObj = new ClsCountryVm { Domain = obj.Domain };
            var mainCountryResult = await countryController.MainCountry(mainCountryObj);
            ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(mainCountryResult);

            ViewBag.PlanAddons = oClsResponse10.Data.PlanAddons;
            ViewBag.MonthsLeft = oClsResponse10.Data.MonthsLeft;
            ViewBag.Country = oClsResponse10.Data.Country;
            ViewBag.MainCountry = oClsResponse19.Data.Country;
            ViewBag.IsAutoFetch = obj.IsAutoFetch;
            return PartialView("PartialAddons");
        }

        public async Task<ActionResult> InitPlanPrice(ClsTransactionVm obj)
        {
            // Initialize API controller
            TransactionController transactionController = new TransactionController();

            // Call API method directly
            var transactionResult = await transactionController.InitPlanPrice(obj);
            ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(transactionResult);

            return Json(oClsResponse);
        }

        public async Task<ActionResult> Whitelabel(ClsUserVm obj)
        {
            try
            {
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.TermLengthId = 4;

                // Initialize API controllers
                DomainController domainController = new DomainController();
                CountryController countryController = new CountryController();
                PlanController planController = new PlanController();
                TermLengthController termLengthController = new TermLengthController();
                PlanAddonsController planAddonsController = new PlanAddonsController();

                // Call API methods directly
                ClsDomainVm domainObj = new ClsDomainVm { Domain = obj.Domain };
                var domainResult = await domainController.DomainCheckForRedirection(domainObj);
                ClsResponse oClsResponse1 = await oCommonController.ExtractResponseFromActionResult(domainResult);

                ClsUserCountryMapVm countryObj = new ClsUserCountryMapVm { Domain = obj.Domain };
                var countryResult = await countryController.ActiveCountrysMapped(countryObj);
                ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(countryResult);

                ClsPlanVm planObj = new ClsPlanVm();
                var planResult = await planController.ActivePlans(planObj);
                ClsResponse oClsResponse2 = await oCommonController.ExtractResponseFromActionResult(planResult);

                ClsTermLengthVm termLengthObj = new ClsTermLengthVm { Domain = obj.Domain, TermLengthId = obj.TermLengthId };
                var termLengthResult = await termLengthController.ActiveTermLengths(termLengthObj);
                ClsResponse oClsResponse5 = await oCommonController.ExtractResponseFromActionResult(termLengthResult);

                ClsPlanAddonsVm planAddonsObj = new ClsPlanAddonsVm { Domain = obj.Domain, TermLengthId = obj.TermLengthId };
                var planAddonsResult = await planAddonsController.ActivePlanAddons(planAddonsObj);
                ClsResponse oClsResponse10 = await oCommonController.ExtractResponseFromActionResult(planAddonsResult);

                ClsCountryVm mainCountryObj = new ClsCountryVm { Domain = obj.Domain };
                var mainCountryResult = await countryController.MainCountry(mainCountryObj);
                ClsResponse oClsResponse19 = await oCommonController.ExtractResponseFromActionResult(mainCountryResult);

                ViewBag.Countrys = oClsResponse.Data.Countrys;
                ViewBag.Plans = oClsResponse2.Data.Plans;
                ViewBag.TermLengths = oClsResponse5.Data.TermLengths;
                ViewBag.PlanAddons = oClsResponse10.Data.PlanAddons;
                ViewBag.Country = oClsResponse10.Data.Country;
                ViewBag.MainCountry = oClsResponse19.Data.Country;
                //ViewBag.IsAutoFetch = true;

                return View();
            }
            catch (Exception ex)
            {
                return Redirect("/errorpage/domain");
            }
        }

    }
}