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
    public class BusinessRegistrationNameController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> ActiveBusinessRegistrationNames(ClsBusinessRegistrationNameVm obj)
        {
            long CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

            var det = oConnectionContext.DbClsBusinessRegistrationName.Where(a =>a.CountryId == CountryId && 
            a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                a.Name,
                a.CountryId,
                a.BusinessRegistrationNameId
            }).OrderBy(a => a.Name).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BusinessRegistrationNames = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

    }
}
