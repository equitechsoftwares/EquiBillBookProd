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
    public class TaxTypeController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> ActiveTaxTypes(ClsTaxTypeVm obj)
        {
            obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a=>a.CompanyId == obj.CompanyId).Select(a=>a.CountryId).FirstOrDefault();

            var det = (from a in oConnectionContext.DbClsCountryTaxTypeMap
                       join b in oConnectionContext.DbClsTaxType
                       on a.TaxTypeId equals b.TaxTypeId
                       where a.CountryId == obj.CountryId && a.IsActive == true && a.IsDeleted == false
                       && b.IsActive == true && b.IsDeleted == false
                       select new
                       {
                           a.TaxTypeId,
                           b.TaxType
                       }).OrderBy(a=>a.TaxType).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TaxTypes = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
