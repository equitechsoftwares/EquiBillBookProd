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
    public class CouponController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> ActiveCoupons(ClsCouponVm obj)
        {
            long Under = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();

            var currentDate = DateTime.Now.Date;
            var coupons = oConnectionContext.DbClsCoupon.Where(a => a.CompanyId == Under && a.IsDeleted == false && a.IsActive == true).AsEnumerable()
                .Where(a => 
                {
                    // If IsNeverExpires is true, only check StartDate
                    if (a.IsNeverExpires)
                    {
                        return currentDate >= a.StartDate.Date;
                    }
                    // If IsNeverExpires is false, check if current date is within range
                    return currentDate >= a.StartDate.Date && currentDate <= a.EndDate.Date;
                })
                .Select(a => new
                {
                    a.CouponId,
                    a.CouponCode,
                    a.Discount,
                    a.DiscountType,
                    a.CouponDescription,
                    a.MinimumPurchaseAmount,
                    a.MaximumDiscountAmount,
                    a.ApplyToBasePlan,
                    a.ApplyToAddons,
                    a.IsNeverExpires,
                    a.StartDate,
                    a.EndDate,
                    a.UsageType,
                    a.NoOfTimes,
                    a.OrderNo
                }).OrderBy(a => a.OrderNo).ThenBy(a => a.CouponCode).ToList();

            // Get term lengths and addons for each coupon
            var det = coupons.Select(c => new
            {
                c.CouponId,
                c.CouponCode,
                c.Discount,
                c.DiscountType,
                c.CouponDescription,
                c.MinimumPurchaseAmount,
                c.MaximumDiscountAmount,
                c.ApplyToBasePlan,
                c.ApplyToAddons,
                c.IsNeverExpires,
                c.StartDate,
                c.EndDate,
                c.UsageType,
                c.NoOfTimes,
                c.OrderNo,
                TermLengths = oConnectionContext.DbClsCouponTermLength
                    .Where(ctl => ctl.CouponId == c.CouponId && ctl.IsDeleted == false)
                    .Join(oConnectionContext.DbClsTermLength,
                        ctl => ctl.TermLengthId,
                        tl => tl.TermLengthId,
                        (ctl, tl) => new
                        {
                            tl.TermLengthId,
                            tl.Title,
                            tl.Months
                        }).ToList(),
                Addons = oConnectionContext.DbClsCouponAddon
                    .Where(ca => ca.CouponId == c.CouponId && ca.IsDeleted == false)
                    .Join(oConnectionContext.DbClsPlanAddons,
                        ca => ca.PlanAddonsId,
                        pa => pa.PlanAddonsId,
                        (ca, pa) => new
                        {
                            pa.PlanAddonsId,
                            pa.Title,
                            pa.Type
                        }).ToList()
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Coupons = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AllCoupons(ClsCouponVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);
            long Under = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();

            var det = oConnectionContext.DbClsCoupon.Where(a => a.CompanyId == Under && a.IsDeleted == false).Select(a => new
            {
                a.CouponId,
                a.CouponCode,
                a.CouponDescription,
                a.UsageType,
                a.NoOfTimes,
                a.StartDate,
                a.EndDate,
                a.DiscountType,
                a.Discount,
                a.OrderNo,
                a.IsActive,
                a.AddedOn,
                a.ModifiedOn,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault()
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => (a.CouponCode != null && a.CouponCode.ToLower().Contains(obj.Search.ToLower())) ||
                    (a.CouponDescription != null && a.CouponDescription.ToLower().Contains(obj.Search.ToLower()))).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Coupons = det.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Coupon(ClsCouponVm obj)
        {
            long Under = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();

            var coupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId && a.CompanyId == Under && a.IsDeleted == false).Select(a => new
            {
                a.CouponId,
                a.CouponCode,
                a.CouponDescription,
                a.UsageType,
                a.NoOfTimes,
                a.StartDate,
                a.EndDate,
                a.IsNeverExpires,
                a.DiscountType,
                a.Discount,
                a.MinimumPurchaseAmount,
                a.MaximumDiscountAmount,
                a.ApplyToBasePlan,
                a.ApplyToAddons,
                a.OrderNo,
                a.IsActive,
                a.AddedOn,
                a.ModifiedOn
            }).FirstOrDefault();

            if (coupon == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Coupon not found",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Get term lengths for this coupon
            var termLengths = oConnectionContext.DbClsCouponTermLength
                .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false)
                .Join(oConnectionContext.DbClsTermLength,
                    ctl => ctl.TermLengthId,
                    tl => tl.TermLengthId,
                    (ctl, tl) => new ClsCouponTermLengthVm
                    {
                        CouponTermLengthId = ctl.CouponTermLengthId,
                        CouponId = ctl.CouponId,
                        TermLengthId = ctl.TermLengthId,
                        TermLengthTitle = tl.Title,
                        Months = tl.Months,
                        IsSelected = true
                    }).ToList();

            // Get addons for this coupon
            var addons = oConnectionContext.DbClsCouponAddon
                .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false)
                .Join(oConnectionContext.DbClsPlanAddons,
                    ca => ca.PlanAddonsId,
                    pa => pa.PlanAddonsId,
                    (ca, pa) => new ClsCouponAddonVm
                    {
                        CouponAddonId = ca.CouponAddonId,
                        CouponId = ca.CouponId,
                        PlanAddonsId = ca.PlanAddonsId,
                        AddonTitle = pa.Title,
                        AddonType = pa.Type,
                        IsSelected = true
                    }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Coupon = coupon,
                    TermLengths = termLengths,
                    Addons = addons
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertCoupon(ClsCouponVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long Under = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();

                if (obj.CouponCode == null || obj.CouponCode == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCouponCode" });
                    isError = true;
                }

                if (obj.Discount == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDiscount" });
                    isError = true;
                }

                if (obj.DiscountType == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDiscountType" });
                    isError = true;
                }

                if (obj.UsageType == 0 || obj.UsageType > 3)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUsageType" });
                    isError = true;
                }               

                if (obj.UsageType == 3)
                {
                    if (obj.NoOfTimes == 0)
                    {
                        errors.Add(new ClsError { Message = "Number of times is required", Id = "divNoOfTimes" });
                        isError = true;
                    }
                }

                // Validate EndDate if IsNeverExpires is false (following RecurringSales pattern)
                if (obj.IsNeverExpires == false)
                {
                    if (obj.EndDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divEndDate" });
                        isError = true;
                    }
                }

                long count = oConnectionContext.DbClsCoupon.Where(a => a.CouponCode.ToLower() == obj.CouponCode.ToLower().Trim() && a.CompanyId == Under
                && a.IsDeleted == false).Count();
                if (count > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Coupon Code exists", Id = "divCouponCode" });
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

                ClsCoupon oClsCoupon = new ClsCoupon()
                {
                    CouponCode = obj.CouponCode.Trim(),
                    CouponDescription = obj.CouponDescription,
                    UsageType = obj.UsageType,
                    NoOfTimes = obj.NoOfTimes,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = Under,
                    StartDate = obj.StartDate,
                    EndDate = obj.EndDate,
                    IsNeverExpires = obj.IsNeverExpires,
                    Discount = obj.Discount,
                    DiscountType = obj.DiscountType,
                    MinimumPurchaseAmount = obj.MinimumPurchaseAmount,
                    MaximumDiscountAmount = obj.MaximumDiscountAmount,
                    ApplyToBasePlan = obj.ApplyToBasePlan,
                    ApplyToAddons = obj.ApplyToAddons,
                    OrderNo = obj.OrderNo,
                };
                oConnectionContext.DbClsCoupon.Add(oClsCoupon);
                oConnectionContext.SaveChanges();

                // Save term length restrictions
                if (obj.TermLengths != null && obj.TermLengths.Count > 0)
                {
                    foreach (var termLength in obj.TermLengths.Where(tl => tl.IsSelected))
                    {
                        ClsCouponTermLength oClsCouponTermLength = new ClsCouponTermLength()
                        {
                            CouponId = oClsCoupon.CouponId,
                            TermLengthId = termLength.TermLengthId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedOn = CurrentDate
                        };
                        oConnectionContext.DbClsCouponTermLength.Add(oClsCouponTermLength);
                    }
                    oConnectionContext.SaveChanges();
                }

                // Save addon restrictions
                if (obj.ApplyToAddons && obj.Addons != null && obj.Addons.Count > 0)
                {
                    foreach (var addon in obj.Addons.Where(a => a.IsSelected))
                    {
                        ClsCouponAddon oClsCouponAddon = new ClsCouponAddon()
                        {
                            CouponId = oClsCoupon.CouponId,
                            PlanAddonsId = addon.PlanAddonsId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedOn = CurrentDate
                        };
                        oConnectionContext.DbClsCouponAddon.Add(oClsCouponAddon);
                    }
                    oConnectionContext.SaveChanges();
                }

                data = new
                {
                    Status = 1,
                    Message = "Coupon created successfully",
                    Data = new
                    {
                        Coupon = oClsCoupon
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateCoupon(ClsCouponVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long Under = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();

                // Check if coupon has been used in any transactions
                long transactionCount = oConnectionContext.DbClsTransaction
                    .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false)
                    .Count();

                if (transactionCount > 0)
                {
                    errors.Add(new ClsError { Message = "Coupon has been used in transactions and cannot be updated", Id = "divCouponCode" });
                    isError = true;
                }

                if (obj.CouponCode == null || obj.CouponCode == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCouponCode" });
                    isError = true;
                }

                if (obj.Discount == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDiscount" });
                    isError = true;
                }

                if (obj.DiscountType == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDiscountType" });
                    isError = true;
                }

                if (obj.UsageType == 0 || obj.UsageType > 3)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divUsageType" });
                    isError = true;
                }

                if (obj.UsageType == 3)
                {
                    if (obj.NoOfTimes == 0)
                    {
                        errors.Add(new ClsError { Message = "Number of times is required", Id = "divNoOfTimes" });
                        isError = true;
                    }
                }

                // Validate EndDate if IsNeverExpires is false (following RecurringSales pattern)
                if (obj.IsNeverExpires == false)
                {
                    if (obj.EndDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divEndDate" });
                        isError = true;
                    }
                }

                long count = oConnectionContext.DbClsCoupon.Where(a => a.CouponCode.ToLower() == obj.CouponCode.ToLower().Trim() && a.CompanyId == Under
                && a.IsDeleted == false && a.CouponId != obj.CouponId).Count();
                if (count > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Coupon Code exists", Id = "divCouponCode" });
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

                var existingCoupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId && a.CompanyId == Under).FirstOrDefault();
                if (existingCoupon == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Coupon not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                existingCoupon.CouponCode = obj.CouponCode.Trim();
                existingCoupon.CouponDescription = obj.CouponDescription;
                existingCoupon.UsageType = obj.UsageType;
                existingCoupon.NoOfTimes = obj.NoOfTimes;
                existingCoupon.IsActive = obj.IsActive;
                existingCoupon.StartDate = obj.StartDate;
                existingCoupon.EndDate = obj.EndDate;
                existingCoupon.IsNeverExpires = obj.IsNeverExpires;
                existingCoupon.Discount = obj.Discount;
                existingCoupon.DiscountType = obj.DiscountType;
                existingCoupon.MinimumPurchaseAmount = obj.MinimumPurchaseAmount;
                existingCoupon.MaximumDiscountAmount = obj.MaximumDiscountAmount;
                existingCoupon.ApplyToBasePlan = obj.ApplyToBasePlan;
                existingCoupon.ApplyToAddons = obj.ApplyToAddons;
                existingCoupon.OrderNo = obj.OrderNo;
                existingCoupon.ModifiedBy = obj.ModifiedBy;
                existingCoupon.ModifiedOn = CurrentDate;

                oConnectionContext.SaveChanges();

                // Delete existing term length restrictions
                var existingTermLengths = oConnectionContext.DbClsCouponTermLength
                    .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false).ToList();
                foreach (var termLength in existingTermLengths)
                {
                    termLength.IsDeleted = true;
                }
                oConnectionContext.SaveChanges();

                // Add new term length restrictions
                if (obj.TermLengths != null && obj.TermLengths.Count > 0)
                {
                    foreach (var termLength in obj.TermLengths.Where(tl => tl.IsSelected))
                    {
                        ClsCouponTermLength oClsCouponTermLength = new ClsCouponTermLength()
                        {
                            CouponId = obj.CouponId,
                            TermLengthId = termLength.TermLengthId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedOn = CurrentDate
                        };
                        oConnectionContext.DbClsCouponTermLength.Add(oClsCouponTermLength);
                    }
                    oConnectionContext.SaveChanges();
                }

                // Delete existing addon restrictions
                var existingAddons = oConnectionContext.DbClsCouponAddon
                    .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false).ToList();
                foreach (var addon in existingAddons)
                {
                    addon.IsDeleted = true;
                }
                oConnectionContext.SaveChanges();

                // Add new addon restrictions
                if (obj.ApplyToAddons && obj.Addons != null && obj.Addons.Count > 0)
                {
                    foreach (var addon in obj.Addons.Where(a => a.IsSelected))
                    {
                        ClsCouponAddon oClsCouponAddon = new ClsCouponAddon()
                        {
                            CouponId = obj.CouponId,
                            PlanAddonsId = addon.PlanAddonsId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedOn = CurrentDate
                        };
                        oConnectionContext.DbClsCouponAddon.Add(oClsCouponAddon);
                    }
                    oConnectionContext.SaveChanges();
                }

                data = new
                {
                    Status = 1,
                    Message = "Coupon updated successfully",
                    Data = new
                    {
                        Coupon = existingCoupon
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CouponActiveInactive(ClsCouponVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long Under = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();

                var existingCoupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId && a.CompanyId == Under && a.IsDeleted == false).FirstOrDefault();
                if (existingCoupon == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Coupon not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                existingCoupon.IsActive = obj.IsActive;
                existingCoupon.ModifiedBy = obj.AddedBy;
                existingCoupon.ModifiedOn = CurrentDate;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Coupon " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> DeleteCoupon(ClsCouponVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long Under = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();

                var existingCoupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId && a.CompanyId == Under && a.IsDeleted == false).FirstOrDefault();
                if (existingCoupon == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Coupon not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                existingCoupon.IsDeleted = true;
                existingCoupon.ModifiedBy = obj.ModifiedBy;
                existingCoupon.ModifiedOn = CurrentDate;

                // Delete term length restrictions
                var existingTermLengths = oConnectionContext.DbClsCouponTermLength
                    .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false).ToList();
                foreach (var termLength in existingTermLengths)
                {
                    termLength.IsDeleted = true;
                }

                // Delete addon restrictions
                var existingAddons = oConnectionContext.DbClsCouponAddon
                    .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false).ToList();
                foreach (var addon in existingAddons)
                {
                    addon.IsDeleted = true;
                }

                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Coupon deleted successfully",
                    Data = new { }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ApplyCoupon(ClsCouponVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.CouponCode == null || obj.CouponCode == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCouponCode" });
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

                long Under = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();

                var coupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponCode.ToLower() == obj.CouponCode.ToLower() && a.IsActive == true
                && a.IsDeleted == false && a.CompanyId == Under).Select(a => new
                {
                    a.CouponId,
                    a.UsageType,
                    a.NoOfTimes,
                    a.StartDate,
                    a.EndDate,
                    a.IsNeverExpires,
                    a.DiscountType,
                    a.Discount,
                    a.MinimumPurchaseAmount,
                    a.MaximumDiscountAmount,
                    a.ApplyToBasePlan,
                    a.ApplyToAddons,
                }).FirstOrDefault();

                decimal calculatedDiscount = 0;

                if (coupon == null)
                {
                    errors.Add(new ClsError { Message = "Invalid Coupon", Id = "divCouponCode" });
                    isError = true;

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
                }
                else
                {
                    // Check term length restrictions if coupon has any
                    if (obj.TermLengthId > 0)
                    {
                        var couponTermLengths = oConnectionContext.DbClsCouponTermLength
                            .Where(a => a.CouponId == coupon.CouponId && a.IsDeleted == false).Select(a => a.TermLengthId).ToList();

                        // If coupon has term length restrictions, validate against selected term length
                        if (couponTermLengths.Count > 0)
                        {
                            if (!couponTermLengths.Contains(obj.TermLengthId))
                            {
                                errors.Add(new ClsError { Message = "This coupon is not valid for the selected plan duration", Id = "divCouponCode" });
                                isError = true;

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
                            }
                        }
                    }

                    // Validate coupon applicability - if coupon applies to both base plan and addons,
                    // at least one must be present. If it applies to only one, that one must be present.
                    if (coupon.ApplyToBasePlan && coupon.ApplyToAddons)
                    {
                        // Coupon applies to both - at least one must be present
                        if (obj.PlanSubTotal <= 0 && obj.AddonsSubTotal <= 0)
                        {
                            errors.Add(new ClsError { Message = "This coupon applies to base plan and addons, but no base plan or addons are selected", Id = "divCouponCode" });
                            isError = true;

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
                        }
                    }
                    else if (coupon.ApplyToBasePlan && !coupon.ApplyToAddons)
                    {
                        // Coupon applies only to base plan - base plan must be present
                        if (obj.PlanSubTotal <= 0)
                        {
                            errors.Add(new ClsError { Message = "This coupon applies to base plan, but no base plan is selected", Id = "divCouponCode" });
                            isError = true;

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
                        }
                    }
                    else if (!coupon.ApplyToBasePlan && coupon.ApplyToAddons)
                    {
                        // Coupon applies only to addons - addons must be present
                        if (obj.AddonsSubTotal <= 0)
                        {
                            errors.Add(new ClsError { Message = "This coupon applies to addons, but no addons are selected", Id = "divCouponCode" });
                            isError = true;

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
                        }
                    }

                    // Validate ApplyToAddons - check addon restrictions if addons are selected
                    if (coupon.ApplyToAddons && obj.AddonsSubTotal > 0)
                    {
                        // Validate addon restrictions if coupon has specific addon restrictions
                        var couponAddonIds = oConnectionContext.DbClsCouponAddon
                            .Where(a => a.CouponId == coupon.CouponId && a.IsDeleted == false)
                            .Select(a => a.PlanAddonsId).ToList();

                        // If coupon has addon restrictions, check if at least one valid addon is selected
                        if (couponAddonIds.Count > 0 && obj.SelectedAddonIds != null && obj.SelectedAddonIds.Count > 0)
                        {
                            // Find valid and invalid addons
                            var validAddons = obj.SelectedAddonIds.Where(addonId => couponAddonIds.Contains(addonId)).ToList();
                            var invalidAddons = obj.SelectedAddonIds.Where(addonId => !couponAddonIds.Contains(addonId)).ToList();
                            
                            // Check if at least one valid addon is selected
                            if (validAddons.Count == 0)
                            {
                                // No valid addons selected - this is an error
                                var invalidAddonNames = oConnectionContext.DbClsPlanAddons
                                    .Where(a => invalidAddons.Contains(a.PlanAddonsId) && a.IsDeleted == false)
                                    .Select(a => a.Title)
                                    .ToList();
                                
                                var errorMessage = "This coupon is not valid for any of the selected addons";
                                if (invalidAddonNames.Count > 0)
                                {
                                    errorMessage += ": " + string.Join(", ", invalidAddonNames);
                                }
                                
                                errors.Add(new ClsError { Message = errorMessage, Id = "divCouponCode" });
                                isError = true;

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
                            }
                            // If some addons are invalid but at least one is valid, we'll allow it and show a warning in the success message
                        }
                    }

                    // Validate minimum purchase amount
                    if (coupon.MinimumPurchaseAmount > 0 && obj.SubTotal < coupon.MinimumPurchaseAmount)
                    {
                        errors.Add(new ClsError { Message = $"Minimum purchase amount of {coupon.MinimumPurchaseAmount} required to use this coupon", Id = "divCouponCode" });
                        isError = true;

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
                    }

                    // Check if coupon has expired (skip check if IsNeverExpires is true)
                    if (!coupon.IsNeverExpires)
                    {
                        if (DateTime.Now.Date < coupon.StartDate.Date || DateTime.Now.Date > coupon.EndDate.Date)
                        {
                            errors.Add(new ClsError { Message = "Invalid Coupon", Id = "divCouponCode" });
                            isError = true;

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
                        }
                    }
                    else
                    {
                        // For never-expiring coupons, only check start date
                        if (DateTime.Now.Date < coupon.StartDate.Date)
                        {
                            errors.Add(new ClsError { Message = "Invalid Coupon", Id = "divCouponCode" });
                            isError = true;

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
                        }
                    }

                    if (coupon.UsageType != 1)
                    {
                        if (coupon.UsageType == 2)
                        {
                            long count = oConnectionContext.DbClsTransaction.Where(a => a.CouponId == coupon.CouponId
                            && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count();

                            if (count > 0)
                            {
                                errors.Add(new ClsError { Message = "Invalid Coupon", Id = "divCouponCode" });
                                isError = true;

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
                            }
                        }
                        else if (coupon.UsageType == 3)
                        {
                            long count = oConnectionContext.DbClsTransaction.Where(a => a.CouponId == coupon.CouponId && a.IsDeleted == false).Count();

                            if (coupon.NoOfTimes <= count)
                            {
                                errors.Add(new ClsError { Message = "Invalid Coupon", Id = "divCouponCode" });
                                isError = true;

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
                            }
                        }
                    }

                    // Calculate the actual discount amount that will be applied (with Maximum Discount Amount cap)
                    decimal couponSubTotal = 0;

                    // Calculate coupon applicable subtotal based on settings
                    if (coupon.ApplyToBasePlan)
                    {
                        couponSubTotal += obj.PlanSubTotal;
                    }

                    if (coupon.ApplyToAddons)
                    {
                        // Get allowed addon IDs for this coupon
                        var couponAddonIds = oConnectionContext.DbClsCouponAddon
                            .Where(a => a.CouponId == coupon.CouponId && a.IsDeleted == false)
                            .Select(a => a.PlanAddonsId).ToList();
                        
                        decimal applicableAddonsSubTotal = 0;
                        List<long> invalidAddons = new List<long>();
                        
                        if (couponAddonIds.Count == 0)
                        {
                            // No specific addons selected = apply to all addons
                            applicableAddonsSubTotal = obj.AddonsSubTotal;
                        }
                        else
                        {
                            // Apply only to specific valid addons
                            var validAddonIds = obj.SelectedAddonIds != null 
                                ? obj.SelectedAddonIds.Where(id => couponAddonIds.Contains(id)).ToList() 
                                : new List<long>();
                            
                            invalidAddons = obj.SelectedAddonIds != null 
                                ? obj.SelectedAddonIds.Where(id => !couponAddonIds.Contains(id)).ToList() 
                                : new List<long>();
                            
                            // Calculate subtotal for valid addons only
                            if (obj.AddonSubTotals != null && obj.AddonSubTotals.Count > 0)
                            {
                                // Use detailed addon subtotals if available (more accurate)
                                applicableAddonsSubTotal = obj.AddonSubTotals
                                    .Where(a => validAddonIds.Contains(a.PlanAddonsId))
                                    .Sum(a => a.SubTotal);
                            }
                            else
                            {
                                // Fallback: use proportional calculation if detailed subtotals not available
                                if (invalidAddons.Count > 0 && validAddonIds.Count > 0)
                                {
                                    decimal validAddonsRatio = (decimal)validAddonIds.Count / (decimal)(validAddonIds.Count + invalidAddons.Count);
                                    applicableAddonsSubTotal = obj.AddonsSubTotal * validAddonsRatio;
                                }
                                else if (validAddonIds.Count > 0)
                                {
                                    // All selected addons are valid
                                    applicableAddonsSubTotal = obj.AddonsSubTotal;
                                }
                                else
                                {
                                    // No valid addons (should have been caught by validation, but just in case)
                                    applicableAddonsSubTotal = 0;
                                }
                            }
                        }
                        
                        couponSubTotal += applicableAddonsSubTotal;
                    }

                    // Calculate discount
                    if (coupon.DiscountType == 1)  // Fixed amount
                    {
                        calculatedDiscount = coupon.Discount;
                    }
                    else  // Percentage
                    {
                        calculatedDiscount = (coupon.Discount / 100) * couponSubTotal;
                    }

                    // Apply maximum discount cap
                    if (coupon.MaximumDiscountAmount > 0 && calculatedDiscount > coupon.MaximumDiscountAmount)
                    {
                        calculatedDiscount = coupon.MaximumDiscountAmount;
                    }
                }

                data = new
                {
                    Status = 1,
                    Message = "Coupon Applied Successfully",
                    Data = new
                    {
                        Coupon = new
                        {
                            coupon.CouponId,
                            coupon.UsageType,
                            coupon.NoOfTimes,
                            coupon.StartDate,
                            coupon.EndDate,
                            coupon.IsNeverExpires,
                            coupon.DiscountType,
                            coupon.Discount,
                            coupon.MinimumPurchaseAmount,
                            coupon.MaximumDiscountAmount,
                            coupon.ApplyToBasePlan,
                            coupon.ApplyToAddons,
                            CalculatedDiscount = calculatedDiscount
                        }
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

    }
}
