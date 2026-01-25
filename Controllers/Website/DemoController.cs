using EquiBillBook.Controllers.WebApi;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.Website
{
    public class DemoController : Controller
    {
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        // GET: Demo
        public async Task<ActionResult> Index(ClsUserVm obj)
        {
            try
            {
                obj.IpAddress = Request.UserHostAddress;
                obj.Browser = Request.Browser.Browser;
                obj.Platform = "Web";
                obj.Domain = Request.Url.Host.Replace("www.", "");
                obj.UserType = "user";
                //obj.EmailId = "demo@equibillbook.com";
                //obj.Password = "1234";

                // Initialize API controller
                UserController userController = new UserController();

                // Call API method directly
                var loginResult = await userController.Login(obj);
                ClsResponse oClsResponse = await oCommonController.ExtractResponseFromActionResult(loginResult);

                if (oClsResponse.Data.User != null)
                {
                    int days = 0;
                    //if (obj.IsRememberMe == true)
                    //{
                        days = 365;
                    //}
                    //else
                    //{
                    //    days = 1;
                    //}

                    Response.Cookies["data"]["Token"] = oClsResponse.Data.User.Token;
                    //Response.Cookies["data"]["Name"] = Convert.ToString(oClsResponse.Data.User.Name);
                    Response.Cookies["data"]["Id"] = Convert.ToString(oClsResponse.Data.User.UserId);
                    Response.Cookies["data"]["CompanyId"] = Convert.ToString(oClsResponse.Data.User.CompanyId);
                    Response.Cookies["data"]["LoginDetailsId"] = Convert.ToString(oClsResponse.Data.User.LoginDetailsId);
                    Response.Cookies["data"]["UserType"] = Convert.ToString(oClsResponse.Data.User.UserType);
                    //if (obj.Browser.ToLower() == "safari")
                    //{
                    //    Response.Cookies["data"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencyCode + " ");
                    //}
                    //else
                    //{
                    Response.Cookies["data"]["CurrencySymbol"] = Convert.ToString(oClsResponse.Data.User.CurrencySymbol);
                    //}
                    Response.Cookies["data"]["DialingCode"] = Convert.ToString(oClsResponse.Data.User.DialingCode);
                    Response.Cookies["data"]["IsDemo"] = "False";
                    Response.Cookies["data"]["BusinessName"] = Convert.ToString(oClsResponse.Data.User.BusinessName);
                    Response.Cookies["data"]["WhitelabelBusinessName"] = Convert.ToString(oClsResponse.Data.User.WhitelabelBusinessName);
                    Response.Cookies["data"].Expires = DateTime.Today.AddDays(days);


                    //Response.Cookies["BusinessSetting"]["DefaultProfitPercent"] = Convert.ToString(oClsResponse.Data.BusinessSetting.DefaultProfitPercent);
                    Response.Cookies["BusinessSetting"]["DateFormat"] = Convert.ToString(oClsResponse.Data.BusinessSetting.DateFormat);
                    Response.Cookies["BusinessSetting"]["TimeFormat"] = Convert.ToString(oClsResponse.Data.BusinessSetting.TimeFormat);
                    Response.Cookies["BusinessSetting"]["CurrencySymbolPlacement"] = Convert.ToString(oClsResponse.Data.BusinessSetting.CurrencySymbolPlacement);
                    Response.Cookies["BusinessSetting"].Expires = DateTime.Today.AddDays(days);

                    Response.Cookies["SystemSetting"]["ShowHelpText"] = Convert.ToString(oClsResponse.Data.BusinessSetting.ShowHelpText);
                    Response.Cookies["SystemSetting"]["EnableDarkMode"] = Convert.ToString(oClsResponse.Data.BusinessSetting.EnableDarkMode);
                    Response.Cookies["SystemSetting"]["FixedHeader"] = Convert.ToString(oClsResponse.Data.BusinessSetting.FixedHeader);
                    Response.Cookies["SystemSetting"]["FixedFooter"] = Convert.ToString(oClsResponse.Data.BusinessSetting.FixedFooter);
                    if (obj.Browser.ToLower() == "safari")
                    {
                        Response.Cookies["SystemSetting"]["EnableSound"] = "False";
                    }
                    else
                    {
                        Response.Cookies["SystemSetting"]["EnableSound"] = Convert.ToString(oClsResponse.Data.BusinessSetting.EnableSound);
                    }
                    Response.Cookies["SystemSetting"]["CollapseSidebar"] = Convert.ToString(oClsResponse.Data.BusinessSetting.CollapseSidebar);
                    Response.Cookies["SystemSetting"].Expires = DateTime.Today.AddDays(days);

                    foreach (var item in oClsResponse.Data.ShortCutKeySettings)
                    {
                        Response.Cookies["ShortCutKeySetting"][item.Title] = Convert.ToString(item.ShortCutKey) + "_" + Convert.ToString(item.Url);
                    }
                    //Response.Cookies["ShortCutKeySetting"]["AddNewForm"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.AddNewForm);
                    //Response.Cookies["ShortCutKeySetting"]["SaveForm"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.SaveForm);
                    //Response.Cookies["ShortCutKeySetting"]["SaveAddAnother"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.SaveAddAnother);
                    //Response.Cookies["ShortCutKeySetting"]["UpdateForm"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.UpdateForm);
                    //Response.Cookies["ShortCutKeySetting"]["UpdateAddAnother"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.UpdateAddAnother);
                    //Response.Cookies["ShortCutKeySetting"]["GoBack"] = Convert.ToString(oClsResponse.Data.ShortCutKeySetting.GoBack);
                    Response.Cookies["ShortCutKeySetting"].Expires = DateTime.Today.AddDays(days);

                    Response.Cookies["ItemSetting"]["ExpiryDateFormat"] = Convert.ToString(oClsResponse.Data.ItemSetting.ExpiryDateFormat);
                    Response.Cookies["ItemSetting"].Expires = DateTime.Today.AddDays(days);

                }
                return Redirect("/dashboard?type=login");
            }
            catch (Exception ex)
            {
                return Redirect("/errorpage/domain");
            }
        }
    }
}