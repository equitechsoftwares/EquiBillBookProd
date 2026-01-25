using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class TermLengthController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        [AllowAnonymous]
        public async Task<IHttpActionResult> ActiveTermLengths(ClsTermLengthVm obj)
        {
            decimal conversionRate = 1;

            obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

            if (obj.CompanyId != 0)
            {
                obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();
            }
            else if (obj.CountryId == 0)
            {
                obj.CountryId = oConnectionContext.DbClsUserCountryMap.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => a.CountryId).FirstOrDefault();
            }

            if (obj.CountryId == oConnectionContext.DbClsUserCountryMap.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => a.CountryId).FirstOrDefault())
            {
                conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => a.ConversionRate).FirstOrDefault();
            }
            else
            {
                conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == 3).Select(a => a.ConversionRate).FirstOrDefault();
            }

            //decimal PriceHikePercentage = oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => a.PriceHikePercentage / 100).FirstOrDefault();
            
            decimal PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(a=>a.CountryId== obj.CountryId && a.CompanyId == obj.Under).Select(a=>a.PriceHikePercentage).FirstOrDefault();

            var det = oConnectionContext.DbClsTermLength.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                a.TermLengthId,
                a.Title,
                a.Description,
                SellingPrice = ((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate,
                MRP = ((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate,
                a.DiscountPercentage,
                a.OrderNo,
                a.Months,
                a.CountryId,
                a.IsChecked
            }).OrderBy(a => a.OrderNo).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TermLengths = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
