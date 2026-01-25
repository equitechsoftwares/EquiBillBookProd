using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
//using Twilio.TwiML.Voice;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class PlanAddonsController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        [AllowAnonymous]
        public async Task<IHttpActionResult> ActivePlanAddons(ClsPlanAddonsVm obj)
        {
            if (obj.TermLengthId == 0)
            {
                long ParentTransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == obj.CompanyId &&
               a.StartDate != null).Select(a => a.TransactionId).FirstOrDefault();

                obj.TermLengthId = oConnectionContext.DbClsTransaction.Where(b => b.TransactionId == ParentTransactionId &&
                 b.CompanyId == obj.CompanyId).Select(b => b.TermLengthId).FirstOrDefault();
            }
            decimal DiscountPercentage = oConnectionContext.DbClsTermLength.Where(a => a.TermLengthId == obj.TermLengthId).Select(a => a.DiscountPercentage).FirstOrDefault();

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

            decimal PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(a => a.CountryId == obj.CountryId && a.CompanyId == obj.Under).Select(a => a.PriceHikePercentage).FirstOrDefault();

            ClsTransactionVm Transaction = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == obj.CompanyId &&
        a.StartDate != null).Select(a => new ClsTransactionVm
        {
            TransactionId = a.TransactionId,
            MonthsLeft = (a.ExpiryDate.Value.Month + a.ExpiryDate.Value.Year * 12) - (DateTime.Now.Month + DateTime.Now.Year * 12)
            //MonthsLeft = Math.Abs(12 * (DateTime.Now.Year - a.ExpiryDate.Value.Year) + DateTime.Now.Month - a.ExpiryDate.Value.Month) + 1
        }).FirstOrDefault();

            if (Transaction == null)
            {
                Transaction = new ClsTransactionVm
                {
                    TransactionId = 0,
                    MonthsLeft = 0
                };
            }

            int MonthsLeft = Transaction.MonthsLeft;

            //if (MonthsLeft < 1)
            //{
            //    MonthsLeft = 1;
            //}
            var det = oConnectionContext.DbClsPlanAddons.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                DiscountPercentage = DiscountPercentage,
                a.PricingType,
                a.PricingPer,
                PlanAddonsId = a.PlanAddonsId,
                a.Title,
                a.Description,
                SellingPrice = Math.Round(((((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate) - ((DiscountPercentage / 100) * (((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate))),2),
                MRP = Math.Round((((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate),2),
                //MRP = ((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate,
                //a.DiscountPercentage,
                a.OrderNo,
                a.IsCheckbox,
                a.Type,
                Quantity = (from aa in oConnectionContext.DbClsTransaction
                            join bb in oConnectionContext.DbClsTransactionDetails
    on aa.TransactionId equals bb.TransactionId
                            where aa.TransactionId == Transaction.TransactionId && aa.Status == 2
                            && bb.PlanAddonsId == a.PlanAddonsId
                            select bb.Quantity).DefaultIfEmpty().Sum() + (from aa in oConnectionContext.DbClsTransaction
                                                                          join bb in oConnectionContext.DbClsTransactionDetails
                                          on aa.TransactionId equals bb.TransactionId
                                                                          where aa.ParentTransactionId == Transaction.TransactionId && aa.Status == 2
                                                                          && bb.PlanAddonsId == a.PlanAddonsId
                                                                          select bb.Quantity).DefaultIfEmpty().Sum(),
                IsTaken = ((from aa in oConnectionContext.DbClsTransaction
                            join bb in oConnectionContext.DbClsTransactionDetails
on aa.TransactionId equals bb.TransactionId
                            where aa.TransactionId == Transaction.TransactionId && aa.Status == 2
                            && bb.PlanAddonsId == a.PlanAddonsId
                            select bb.PlanAddonsId).Count() > 0
                || (from aa in oConnectionContext.DbClsTransaction
                    join bb in oConnectionContext.DbClsTransactionDetails
on aa.TransactionId equals bb.TransactionId
                    where aa.ParentTransactionId == Transaction.TransactionId && aa.Status == 2
                    && bb.PlanAddonsId == a.PlanAddonsId
                    select bb.PlanAddonsId).Count() > 0) ? true : false
            }).OrderBy(a => a.OrderNo).ToList();

            ClsCountryVm oClsCountryVm = new ClsCountryVm()
            {
                CountryId = obj.CountryId,
            };

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PlanAddons = det,
                    MonthsLeft = MonthsLeft,
                    Country = oClsCountryVm
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PlanAddonPermissions(ClsPlanAddonsVm obj)
        {
            long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate != null && a.Status == 2 && a.IsActive == true).Select(a => a.TransactionId).FirstOrDefault();

            var PlanAddons = (from aa in oConnectionContext.DbClsTransaction
                              join bb in oConnectionContext.DbClsTransactionDetails
  on aa.TransactionId equals bb.TransactionId
                              where aa.TransactionId == TransactionId && aa.Status == 2
                              select new { bb.Type }).Union(from aa in oConnectionContext.DbClsTransaction
                                                            join bb in oConnectionContext.DbClsTransactionDetails
                                on aa.TransactionId equals bb.TransactionId
                                                            where aa.ParentTransactionId == TransactionId && aa.Status == 2
                                                            select new { bb.Type }).ToList();


            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PlanAddons = PlanAddons,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
