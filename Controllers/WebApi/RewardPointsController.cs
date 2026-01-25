using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class RewardPointsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        /// <summary>
        /// Get customer's current reward points balance
        /// </summary>
        [HttpPost]
        public async Task<IHttpActionResult> GetCustomerRewardPoints(ClsCustomerRewardPointsVm obj)
        {
            // Reward points are now stored directly in tblUser
            var customer = oConnectionContext.DbClsUser
                .Where(a => a.UserId == obj.CustomerId 
                    && a.CompanyId == obj.CompanyId 
                    && !a.IsDeleted)
                .Select(a => new
                {
                    AvailablePoints = a.AvailableRewardPoints,
                    TotalPointsEarned = a.TotalRewardPointsEarned,
                    TotalPointsRedeemed = a.TotalRewardPointsRedeemed,
                    ExpiredPoints = a.ExpiredRewardPoints
                })
                .FirstOrDefault();

            if (customer == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Customer not found",
                    Data = new
                    {
                        AvailablePoints = 0m,
                        TotalPointsEarned = 0m,
                        TotalPointsRedeemed = 0m,
                        ExpiredPoints = 0m
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = customer
            };

            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Calculate points discount for given redeem points
        /// </summary>
        public async Task<IHttpActionResult> CalculatePointsDiscount(ClsRewardPointSettingsVm obj)
        {
            var settings = oConnectionContext.DbClsRewardPointSettings
                .Where(a => a.CompanyId == obj.CompanyId && a.EnableRewardPoint && !a.IsDeleted)
                .Select(a => new
                {
                    a.RedeemAmountPerUnitPoint,
                    a.MinimumOrderTotalToRedeemPoints,
                    a.MinimumRedeemPoint,
                    a.MaximumRedeemPointPerOrder
                })
                .FirstOrDefault();

            if (settings == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Reward points not enabled",
                    Data = new { DiscountAmount = 0m }
                };
                return await Task.FromResult(Ok(data));
            }

            decimal discountAmount = 0;
            if (obj.RedeemPoints > 0 && settings.RedeemAmountPerUnitPoint > 0)
            {
                discountAmount = obj.RedeemPoints * settings.RedeemAmountPerUnitPoint;
            }

            data = new
            {
                Status = 1,
                Message = "calculated",
                Data = new
                {
                    DiscountAmount = discountAmount,
                    Settings = settings
                }
            };

            return await Task.FromResult(Ok(data));
        }

        /// <summary>
        /// Calculate points that will be earned for given order amount
        /// </summary>
        public async Task<IHttpActionResult> CalculatePointsEarned(ClsRewardPointSettingsVm obj)
        {
            var settings = oConnectionContext.DbClsRewardPointSettings
                .Where(a => a.CompanyId == obj.CompanyId && a.EnableRewardPoint && !a.IsDeleted)
                .Select(a => new
                {
                    a.AmountSpentForUnitPoint,
                    a.MinOrderTotalToEarnReward,
                    a.MaxPointsPerOrder
                })
                .FirstOrDefault();

            if (settings == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Reward points not enabled",
                    Data = new { PointsEarned = 0m }
                };
                return await Task.FromResult(Ok(data));
            }

            decimal pointsEarned = 0;
            if (obj.OrderAmount >= settings.MinOrderTotalToEarnReward)
            {
                // Get applicable tier or use default rate
                decimal amountSpentForUnitPoint = settings.AmountSpentForUnitPoint;
                var applicableTier = oConnectionContext.DbClsRewardPointTier
                    .Where(a => a.CompanyId == obj.CompanyId && 
                               a.IsActive && 
                               !a.IsDeleted &&
                               a.MinAmount <= obj.OrderAmount &&
                               (a.MaxAmount == null || a.MaxAmount > obj.OrderAmount))
                    .OrderBy(a => a.Priority)
                    .FirstOrDefault();

                if (applicableTier != null)
                {
                    amountSpentForUnitPoint = applicableTier.AmountSpentForUnitPoint;
                }

                if (amountSpentForUnitPoint > 0)
                {
                    pointsEarned = Math.Floor(obj.OrderAmount / amountSpentForUnitPoint);
                    
                    if (settings.MaxPointsPerOrder > 0 && pointsEarned > settings.MaxPointsPerOrder)
                    {
                        pointsEarned = settings.MaxPointsPerOrder;
                    }
                }
            }

            data = new
            {
                Status = 1,
                Message = "calculated",
                Data = new { PointsEarned = pointsEarned }
            };

            return await Task.FromResult(Ok(data));
        }
    }
}

