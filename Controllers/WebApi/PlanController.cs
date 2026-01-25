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
    public class PlanController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        [AllowAnonymous]
        public async Task<IHttpActionResult> ActivePlans(ClsPlanVm obj)
        {
            //if (obj.CompanyId != 0)
            //{
            //    var CurrentPlan = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            //        a.StartDate != null && a.Status == 2).Select(a => new
            //        {
            //            a.BaseBranch,
            //            a.BaseItem,
            //            a.BaseOrder,
            //            a.BaseUser,
            //            a.BaseBill,
            //            a.BaseTaxSetting,
            //            a.TransactionId,
            //            a.StartDate,
            //            a.ExpiryDate
            //        }).FirstOrDefault();

            //    var det = oConnectionContext.DbClsPlan.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
            //    {
            //        a.Type,
            //        a.PlanId,
            //        a.Title,
            //        a.Description,
            //        a.OrderNo,
            //        Quantity = a.Type == "Branch" ? CurrentPlan.BaseBranch : a.Type == "Item" ?
            //        CurrentPlan.BaseItem : a.Type == "Order" ? CurrentPlan.BaseOrder : a.Type == "User" ? CurrentPlan.BaseUser :
            //        a.Type == "Bill" ? CurrentPlan.BaseBill : a.Type == "Tax Setting" ? CurrentPlan.BaseTaxSetting : a.Quantity,
            //        a.ShortDescription
            //    }).OrderBy(a => a.OrderNo).ToList();

            //    data = new
            //    {
            //        Status = 1,
            //        Message = "found",
            //        Data = new
            //        {
            //            Plans = det,
            //        }
            //    };
            //}
            //else
            //{
                var det = oConnectionContext.DbClsPlan.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
                {
                    a.Type,
                    a.PlanId,
                    a.Title,
                    a.Description,
                    a.OrderNo,
                    Quantity = a.Quantity,
                    a.ShortDescription
                }).OrderBy(a => a.OrderNo).ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Plans = det,
                    }
                };
            //}

            return await Task.FromResult(Ok(data));
        }
    }
}
