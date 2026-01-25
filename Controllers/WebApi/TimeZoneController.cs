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
    public class TimeZoneController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        [AllowAnonymous]
        public async Task<IHttpActionResult> ActiveTimeZones(ClsTimeZoneVm obj)
        {
            var det = oConnectionContext.DbClsTimeZone.Where(a => a.IsDeleted == false && a.IsActive == true).AsEnumerable().Select(a => new
            {
                TimeZoneId = a.TimeZoneId,
                a.DisplayName,
                Utc = a.SupportsDaylightSavingTime == false ? a.o1String : a.o2String,
                UtcValue = a.SupportsDaylightSavingTime == false ? Convert.ToDecimal(a.o1String.Replace(':', '.')) : Convert.ToDecimal(a.o2String.Replace(':', '.')),
                a.SupportsDaylightSavingTime
            }).OrderBy(a => a.UtcValue).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TimeZones = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TimeZone(ClsTimeZoneVm obj)
        {
            var det = oConnectionContext.DbClsTimeZone.Where(a => a.TimeZoneId == obj.TimeZoneId).Select(a => new
            {
                TimeZoneId = a.TimeZoneId,
                a.DisplayName,
                Utc = a.SupportsDaylightSavingTime == false ? a.o1String : a.o2String,
                //UtcValue = a.SupportsDaylightSavingTime == false ? Convert.ToDecimal(a.o1String.Replace(':', '.')) : Convert.ToDecimal(a.o2String.Replace(':', '.')),
                a.SupportsDaylightSavingTime
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TimeZone = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
