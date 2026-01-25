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
    public class ReligionController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> ActiveReligions(ClsReligionVm obj)
        {
            var det = oConnectionContext.DbClsReligion.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                ReligionId = a.ReligionId,
                ReligionName = a.ReligionName,
            }).OrderBy(a => a.ReligionName).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Religions = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}
