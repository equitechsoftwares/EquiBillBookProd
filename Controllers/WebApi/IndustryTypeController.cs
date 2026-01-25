using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class IndustryTypeController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        [AllowAnonymous]
        public async Task<IHttpActionResult> ActiveIndustryTypes(ClsIndustryTypeVm obj)
        {
            var det = oConnectionContext.DbClsIndustryType.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                a.Sequence,
                a.Name,
                a.IndustryTypeId
            }).OrderBy(a => a.Name).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    IndustryTypes = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
