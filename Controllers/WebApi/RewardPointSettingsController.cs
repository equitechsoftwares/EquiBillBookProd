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
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class RewardPointSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;

        public async Task<IHttpActionResult> RewardPointSetting(ClsRewardPointSettingsVm obj)
        {
            var det = oConnectionContext.DbClsRewardPointSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                RewardPointSettingsId = a.RewardPointSettingsId,
                EnableRewardPoint = a.EnableRewardPoint,
                DisplayName = a.DisplayName,
                AmountSpentForUnitPoint = a.AmountSpentForUnitPoint,
                MinOrderTotalToEarnReward = a.MinOrderTotalToEarnReward,
                MaxPointsPerOrder = a.MaxPointsPerOrder,
                RedeemAmountPerUnitPoint = a.RedeemAmountPerUnitPoint,
                MinimumOrderTotalToRedeemPoints = a.MinimumOrderTotalToRedeemPoints,
                MinimumRedeemPoint = a.MinimumRedeemPoint,
                MaximumRedeemPointPerOrder = a.MaximumRedeemPointPerOrder,
                ExpiryPeriod = a.ExpiryPeriod,
                ExpiryPeriodType = a.ExpiryPeriodType,
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    RewardPointSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> RewardPointSettingsUpdate(ClsRewardPointSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                
                // Validation
                if (obj.EnableRewardPoint)
                {
                    if (string.IsNullOrWhiteSpace(obj.DisplayName))
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divDisplayName" });
                        isError = true;
                    }

                    if (obj.AmountSpentForUnitPoint <= 0)
                    {
                        errors.Add(new ClsError { Message = "Amount Spent for 1 Point must be greater than 0", Id = "divAmountSpentForUnitPoint" });
                        isError = true;
                    }

                    if (obj.RedeemAmountPerUnitPoint <= 0)
                    {
                        errors.Add(new ClsError { Message = "Redeem Amount per Point must be greater than 0", Id = "divRedeemAmountPerUnitPoint" });
                        isError = true;
                    }
                }

                if (isError == true)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "",
                        Errors = errors,
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                long RewardPointSettingsId = oConnectionContext.DbClsRewardPointSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.RewardPointSettingsId).FirstOrDefault();

            ClsRewardPointSettings oClsRewardPointSettings = new ClsRewardPointSettings()
            {
                RewardPointSettingsId = RewardPointSettingsId,
                EnableRewardPoint = obj.EnableRewardPoint,
                DisplayName = obj.DisplayName,
                AmountSpentForUnitPoint = obj.AmountSpentForUnitPoint,
                MinOrderTotalToEarnReward = obj.MinOrderTotalToEarnReward,
                MaxPointsPerOrder = obj.MaxPointsPerOrder,
                RedeemAmountPerUnitPoint = obj.RedeemAmountPerUnitPoint,
                MinimumOrderTotalToRedeemPoints = obj.MinimumOrderTotalToRedeemPoints,
                MinimumRedeemPoint = obj.MinimumRedeemPoint,
                MaximumRedeemPointPerOrder = obj.MaximumRedeemPointPerOrder,
                ExpiryPeriod = obj.ExpiryPeriod,
                ExpiryPeriodType = obj.ExpiryPeriodType,
                ModifiedBy=obj.AddedBy,
                ModifiedOn = CurrentDate
            };

            oConnectionContext.DbClsRewardPointSettings.Attach(oClsRewardPointSettings);
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.RewardPointSettingsId).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.ModifiedBy).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.ModifiedOn).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.EnableRewardPoint).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.DisplayName).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.AmountSpentForUnitPoint).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.MinOrderTotalToEarnReward).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.MaxPointsPerOrder).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.RedeemAmountPerUnitPoint).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.MinimumOrderTotalToRedeemPoints).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.MinimumRedeemPoint).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.MaximumRedeemPointPerOrder).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.ExpiryPeriod).IsModified = true;
            oConnectionContext.Entry(oClsRewardPointSettings).Property(x => x.ExpiryPeriodType).IsModified = true;
            oConnectionContext.SaveChanges();

            // Process tiers if provided (tiers are optional)
            if (obj.RewardPointTiers != null && obj.RewardPointTiers.Count > 0)
            {
                long rewardPointSettingsId = RewardPointSettingsId;
                
                // Get existing tiers for this company (including deleted ones)
                var existingTiers = oConnectionContext.DbClsRewardPointTier
                    .Where(a => a.CompanyId == obj.CompanyId)
                    .ToList();

                // Process each tier
                foreach (var tierVm in obj.RewardPointTiers)
                {
                    // If this tier is marked for deletion
                    if (tierVm.IsDeleted)
                    {
                        if (tierVm.RewardPointTierId > 0)
                        {
                            // Find and soft delete the existing tier
                            var existingTier = existingTiers.FirstOrDefault(a => a.RewardPointTierId == tierVm.RewardPointTierId);
                            if (existingTier != null && !existingTier.IsDeleted)
                            {
                                existingTier.IsDeleted = true;
                                existingTier.ModifiedBy = obj.AddedBy;
                                existingTier.ModifiedOn = CurrentDate;
                                oConnectionContext.Entry(existingTier).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        // If tierId is 0 and IsDeleted is true, it means it's a new row that was deleted before saving - just skip it
                        continue;
                    }

                    // Validation for non-deleted tiers
                    if (tierVm.Priority < 1)
                    {
                        errors.Add(new ClsError { Message = "Priority must be at least 1", Id = "divTierPriority" });
                        isError = true;
                        continue;
                    }

                    if (tierVm.MinAmount < 0)
                    {
                        errors.Add(new ClsError { Message = "Minimum Amount must be 0 or greater", Id = "divTierMinAmount" });
                        isError = true;
                        continue;
                    }

                    if (tierVm.MaxAmount.HasValue && tierVm.MaxAmount.Value <= tierVm.MinAmount)
                    {
                        errors.Add(new ClsError { Message = "Maximum Amount must be greater than Minimum Amount", Id = "divTierMaxAmount" });
                        isError = true;
                        continue;
                    }

                    if (tierVm.AmountSpentForUnitPoint <= 0)
                    {
                        errors.Add(new ClsError { Message = "Amount Spent for 1 Point must be greater than 0", Id = "divTierAmountSpentForUnitPoint" });
                        isError = true;
                        continue;
                    }

                    if (isError) continue;

                    ClsRewardPointTier oClsRewardPointTier = null;
                    
                    if (tierVm.RewardPointTierId > 0)
                    {
                        // Update existing tier
                        oClsRewardPointTier = existingTiers.FirstOrDefault(a => a.RewardPointTierId == tierVm.RewardPointTierId);
                        if (oClsRewardPointTier != null)
                        {
                            oClsRewardPointTier.MinAmount = tierVm.MinAmount;
                            oClsRewardPointTier.MaxAmount = tierVm.MaxAmount;
                            oClsRewardPointTier.AmountSpentForUnitPoint = tierVm.AmountSpentForUnitPoint;
                            oClsRewardPointTier.Priority = tierVm.Priority;
                            oClsRewardPointTier.IsActive = tierVm.IsActive;
                            oClsRewardPointTier.IsDeleted = false; // Ensure it's not deleted
                            oClsRewardPointTier.ModifiedBy = obj.AddedBy;
                            oClsRewardPointTier.ModifiedOn = CurrentDate;
                            oConnectionContext.Entry(oClsRewardPointTier).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    if (oClsRewardPointTier == null)
                    {
                        // Create new tier
                        oClsRewardPointTier = new ClsRewardPointTier
                        {
                            RewardPointSettingsId = rewardPointSettingsId,
                            CompanyId = obj.CompanyId,
                            MinAmount = tierVm.MinAmount,
                            MaxAmount = tierVm.MaxAmount,
                            AmountSpentForUnitPoint = tierVm.AmountSpentForUnitPoint,
                            Priority = tierVm.Priority,
                            IsActive = tierVm.IsActive,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate
                        };
                        oConnectionContext.DbClsRewardPointTier.Add(oClsRewardPointTier);
                    }
                }

                if (isError == true)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "",
                        Errors = errors,
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Save tier changes
                oConnectionContext.SaveChanges();
            }

            ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
            {
                AddedBy = obj.AddedBy,
                Browser = obj.Browser,
                Category = "RewardPoint Settings - RewardPoint Information Update",
                CompanyId = obj.CompanyId,
                Description = "updated RewardPoint informations",
                Id = oClsRewardPointSettings.RewardPointSettingsId,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = "RewardPoint Settings"
            };
            oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

            data = new
            {
                Status = 1,
                Message = "Reward Point Info updated successfully.",
                Data = new
                {

                }
            };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        [HttpPost]
        public async Task<IHttpActionResult> GetTiers(ClsRewardPointSettingsVm obj)
        {
            var tiers = oConnectionContext.DbClsRewardPointTier
                .Where(a => a.CompanyId == obj.CompanyId && !a.IsDeleted)
                .OrderBy(a => a.Priority)
                .Select(a => new
                {
                    a.RewardPointTierId,
                    a.RewardPointSettingsId,
                    a.CompanyId,
                    a.MinAmount,
                    a.MaxAmount,
                    a.AmountSpentForUnitPoint,
                    a.Priority,
                    a.IsActive,
                    a.IsDeleted
                })
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    RewardPointTiers = tiers
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SaveTier(ClsRewardPointTierVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                // Validation
                if (obj.Priority < 1)
                {
                    errors.Add(new ClsError { Message = "Priority must be at least 1", Id = "divTierPriority" });
                    isError = true;
                }

                if (obj.MinAmount < 0)
                {
                    errors.Add(new ClsError { Message = "Minimum Amount must be 0 or greater", Id = "divTierMinAmount" });
                    isError = true;
                }

                if (obj.MaxAmount.HasValue && obj.MaxAmount.Value <= obj.MinAmount)
                {
                    errors.Add(new ClsError { Message = "Maximum Amount must be greater than Minimum Amount", Id = "divTierMaxAmount" });
                    isError = true;
                }

                if (obj.AmountSpentForUnitPoint <= 0)
                {
                    errors.Add(new ClsError { Message = "Amount Spent for 1 Point must be greater than 0", Id = "divTierAmountSpentForUnitPoint" });
                    isError = true;
                }

                if (isError == true)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "",
                        Errors = errors,
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Get or create RewardPointSettingsId if not provided
                if (obj.RewardPointSettingsId == 0)
                {
                    obj.RewardPointSettingsId = oConnectionContext.DbClsRewardPointSettings
                        .Where(a => a.CompanyId == obj.CompanyId)
                        .Select(a => a.RewardPointSettingsId)
                        .FirstOrDefault();
                }

                ClsRewardPointTier oClsRewardPointTier;
                if (obj.RewardPointTierId > 0)
                {
                    // Update existing tier
                    oClsRewardPointTier = oConnectionContext.DbClsRewardPointTier
                        .Where(a => a.RewardPointTierId == obj.RewardPointTierId && a.CompanyId == obj.CompanyId)
                        .FirstOrDefault();

                    if (oClsRewardPointTier == null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Tier not found",
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    oClsRewardPointTier.MinAmount = obj.MinAmount;
                    oClsRewardPointTier.MaxAmount = obj.MaxAmount;
                    oClsRewardPointTier.AmountSpentForUnitPoint = obj.AmountSpentForUnitPoint;
                    oClsRewardPointTier.Priority = obj.Priority;
                    oClsRewardPointTier.IsActive = obj.IsActive;
                    oClsRewardPointTier.ModifiedBy = obj.AddedBy;
                    oClsRewardPointTier.ModifiedOn = CurrentDate;

                    oConnectionContext.Entry(oClsRewardPointTier).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    // Create new tier
                    oClsRewardPointTier = new ClsRewardPointTier
                    {
                        RewardPointSettingsId = obj.RewardPointSettingsId,
                        CompanyId = obj.CompanyId,
                        MinAmount = obj.MinAmount,
                        MaxAmount = obj.MaxAmount,
                        AmountSpentForUnitPoint = obj.AmountSpentForUnitPoint,
                        Priority = obj.Priority,
                        IsActive = obj.IsActive,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate
                    };
                    oConnectionContext.DbClsRewardPointTier.Add(oClsRewardPointTier);
                }

                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "RewardPoint Settings - Tier " + (obj.RewardPointTierId > 0 ? "Update" : "Add"),
                    CompanyId = obj.CompanyId,
                    Description = (obj.RewardPointTierId > 0 ? "updated" : "added") + " tier rule",
                    Id = oClsRewardPointTier.RewardPointTierId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "RewardPoint Settings"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tier " + (obj.RewardPointTierId > 0 ? "updated" : "added") + " successfully.",
                    Data = new
                    {
                        RewardPointTierId = oClsRewardPointTier.RewardPointTierId
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> DeleteTier(ClsRewardPointTierVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                var oClsRewardPointTier = oConnectionContext.DbClsRewardPointTier
                    .Where(a => a.RewardPointTierId == obj.RewardPointTierId && a.CompanyId == obj.CompanyId && !a.IsDeleted)
                    .FirstOrDefault();

                if (oClsRewardPointTier == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Tier not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                oClsRewardPointTier.IsDeleted = true;
                oClsRewardPointTier.ModifiedBy = obj.AddedBy;
                oClsRewardPointTier.ModifiedOn = CurrentDate;

                oConnectionContext.Entry(oClsRewardPointTier).State = System.Data.Entity.EntityState.Modified;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "RewardPoint Settings - Tier Delete",
                    CompanyId = obj.CompanyId,
                    Description = "deleted tier rule",
                    Id = oClsRewardPointTier.RewardPointTierId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "RewardPoint Settings"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tier deleted successfully.",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

    }
}
