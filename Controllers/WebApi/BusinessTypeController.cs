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
    public class BusinessTypeController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        [AllowAnonymous]
        public async Task<IHttpActionResult> ActiveBusinessTypes(ClsBusinessTypeVm obj)
        {
            var det = oConnectionContext.DbClsBusinessType.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                a.Sequence,
                a.Name,
                a.BusinessTypeId
            }).OrderBy(a => a.Name).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BusinessTypes = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
