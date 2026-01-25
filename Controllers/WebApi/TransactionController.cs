using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class TransactionController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllTransactions(ClsTransactionVm obj)
        {
            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();

            var det = oConnectionContext.DbClsTransaction.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsTrial == false).Select(a => new
            {
                CurrencySymbol = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == a.CurrencyId).Select(c => c.CurrencySymbol).FirstOrDefault(),
                CurrencySymbolPlacement = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == obj.Under).Select(bb => bb.CurrencySymbolPlacement).FirstOrDefault(),
                TransactionId = a.TransactionId,
                a.PaidOn,
                a.TransactionNo,
                a.PayableCost,
                a.Status,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            }).ToList();

            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            {
                det = det.Where(a => a.AddedOn != null && a.AddedOn.Date >= obj.FromDate && a.AddedOn.Date <= obj.ToDate).ToList();
            }

            if (obj.Status != 0)
            {
                det = det.Where(a => a.Status == obj.Status).ToList();
            }

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.TransactionNo.Contains(obj.Search)).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Transactions = det.OrderByDescending(a => a.TransactionId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    //ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    //InactiveCount = det.Where(a => a.IsActive == false).Count(),
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Transaction(ClsTransactionVm obj)
        {
            //obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();

            var det = oConnectionContext.DbClsTransaction.Where(a => a.TransactionNo == obj.TransactionNo
            //&& a.CompanyId == obj.CompanyId
            ).Select(a => new
            {
                CurrencySymbol = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == a.CurrencyId).Select(c => c.CurrencySymbol).FirstOrDefault(),
                CurrencySymbolPlacement = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == obj.Under).Select(bb => bb.CurrencySymbolPlacement).FirstOrDefault(),
                MainCountryId = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.CountryId).FirstOrDefault(),
                CountryId = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.CompanyId).Select(b => b.CountryId).FirstOrDefault(),
                CurrentPlanId = oConnectionContext.DbClsTransaction.OrderByDescending(b => b.TransactionId).Where(b => b.CompanyId == a.CompanyId &&
          b.StartDate != null && b.Status == 2 && b.IsActive == true).Select(b => b.TransactionId).FirstOrDefault(),
                ParentTransactionId = a.ParentTransactionId,
                TransactionId = a.TransactionId,
                a.PaidOn,
                a.LiveTransactionId,
                a.PaymentGatewayType,
                a.PaymentMethodType,
                a.TransactionNo,
                a.SubTotal,
                a.CouponDiscount,
                a.YearlyPlanDiscount,
                a.YearlyPlanDiscountPercentage,
                a.PayableCost,
                a.Status,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.SpecialDiscount,
                a.Months,
                //Title = oConnectionContext.DbClsTermLength.Where(b=>b.TermLengthId == a.TermLengthId).Select(b=>b.Title).FirstOrDefault(),
                TransactionDetails = oConnectionContext.DbClsTransactionDetails.Where(b => b.TransactionId == a.TransactionId).Select(b => new
                {
                    IsCheckbox = oConnectionContext.DbClsPlanAddons.Where(c => c.PlanAddonsId == b.PlanAddonsId).Select(c => c.IsCheckbox).FirstOrDefault(),
                    PricingPer = oConnectionContext.DbClsPlanAddons.Where(c => c.PlanAddonsId == b.PlanAddonsId).Select(c => c.PricingPer).FirstOrDefault(),
                    PricingType = oConnectionContext.DbClsPlanAddons.Where(c => c.PlanAddonsId == b.PlanAddonsId).Select(c => c.PricingType).FirstOrDefault(),
                    //b.Discount,
                    b.PlanAddonsId,
                    Title = oConnectionContext.DbClsPlanAddons.Where(c => c.PlanAddonsId == b.PlanAddonsId).Count() == 0 ? "Base Plan" :
                     oConnectionContext.DbClsPlanAddons.Where(c => c.PlanAddonsId == b.PlanAddonsId).Select(c => c.Title).FirstOrDefault(),
                    b.Quantity,
                    b.SubTotal,
                    //b.PayableCost,
                    b.MRP,
                    //b.UnitCost,
                    b.TransactionDetailsId
                }).ToList(),
                BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == a.CompanyId).Select(b => new
                {
                    //CurrencySymbolPlacement = b.CountryId == oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == obj.Under).Select(bb => bb.CountryId).FirstOrDefault()
                    //? b.CurrencySymbolPlacement : 1,
                    //CurrencySymbol = b.CountryId == oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == obj.Under).Select(bb => bb.CountryId).FirstOrDefault()
                    //? oConnectionContext.DbClsCountry.Where(bb => bb.CountryId == b.CountryId).Select(bb => bb.CurrencySymbol).FirstOrDefault()
                    //: "$",
                    b.BusinessLogo,
                    b.BusinessName,
                }).FirstOrDefault(),
                Branch = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == a.CompanyId && b.IsMain == true).Select(b => new
                {
                    b.Branch,
                    b.Mobile,
                    b.Email,
                    b.TaxNo,
                    Tax = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == b.TaxId).Select(bb => bb.Tax).FirstOrDefault(),
                    b.Address,
                    City = oConnectionContext.DbClsCity.Where(cc => cc.CityId == b.CityId).Select(cc => cc.City).FirstOrDefault(),
                    State = oConnectionContext.DbClsState.Where(cc => cc.StateId == b.StateId).Select(cc => cc.State).FirstOrDefault(),
                    Country = oConnectionContext.DbClsCountry.Where(cc => cc.CountryId == b.CountryId).Select(cc => cc.Country).FirstOrDefault(),
                    b.Zipcode
                }).FirstOrDefault(),
                From = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == obj.Under).Select(bb => new
                {
                    bb.BusinessLogo,
                    bb.BusinessName,
                    Mobile = oConnectionContext.DbClsBranch.Where(bc => bc.CompanyId == obj.Under && bc.IsMain == true).Select(bc => bc.Mobile).FirstOrDefault(),
                    AltMobileNo = oConnectionContext.DbClsBranch.Where(bc => bc.CompanyId == obj.Under && bc.IsMain == true).Select(bc => bc.AltMobileNo).FirstOrDefault(),
                    Address = oConnectionContext.DbClsBranch.Where(bc => bc.CompanyId == obj.Under && bc.IsMain == true).Select(bc => bc.Address).FirstOrDefault(),
                    Email = oConnectionContext.DbClsBranch.Where(bc => bc.CompanyId == obj.Under && bc.IsMain == true).Select(bc => bc.Email).FirstOrDefault(),
                    City = oConnectionContext.DbClsCity.Where(cc => cc.CityId == oConnectionContext.DbClsBranch.Where(bc => bc.CompanyId == obj.Under && bc.IsMain == true).Select(bc => bc.CityId).FirstOrDefault()).Select(cc => cc.City).FirstOrDefault(),
                    State = oConnectionContext.DbClsState.Where(cc => cc.StateId == oConnectionContext.DbClsBranch.Where(bc => bc.CompanyId == obj.Under && bc.IsMain == true).Select(bc => bc.StateId).FirstOrDefault()).Select(cc => cc.State).FirstOrDefault(),
                    Country = oConnectionContext.DbClsCountry.Where(cc => cc.CountryId == oConnectionContext.DbClsBranch.Where(bc => bc.CompanyId == obj.Under && bc.IsMain == true).Select(bc => bc.CountryId).FirstOrDefault()).Select(cc => cc.Country).FirstOrDefault(),
                    Zipcode = oConnectionContext.DbClsBranch.Where(bc => bc.CompanyId == obj.Under && bc.IsMain == true).Select(bc => bc.Zipcode).FirstOrDefault()
                }).FirstOrDefault(),
                OnlinePaymentSettings = oConnectionContext.DbClsOnlinePaymentSettings.Where(aa => aa.IsDeleted == false && aa.IsActive == true
                    && aa.CompanyId == obj.Under).Select(aa => new ClsOnlinePaymentSettingsVm
                    {
                        OnlinePaymentSettingsId = aa.OnlinePaymentSettingsId,
                        OnlinePaymentService = aa.OnlinePaymentService,
                        RazorpayKey = aa.RazorpayKey,
                        RazorpayCurrencyId = aa.RazorpayCurrencyId,
                        PaypalClientId = aa.PaypalClientId,
                        PaypalCurrencyId = aa.PaypalCurrencyId,
                        IsDefault = aa.IsDefault,
                        SaveAs = aa.SaveAs,
                        PaypalCurrencyCode = oConnectionContext.DbClsCurrency.Where(bb => bb.CurrencyId == aa.PaypalCurrencyId).Select(bb => bb.CurrencyCode).FirstOrDefault(),
                        RazorpayCurrencyCode = oConnectionContext.DbClsCurrency.Where(bb => bb.CurrencyId == aa.RazorpayCurrencyId).Select(bb => bb.CurrencyCode).FirstOrDefault()
                    }).OrderBy(aa => aa.OnlinePaymentSettingsId).ToList()
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Transaction = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertTransaction(ClsTransactionVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int User = oConnectionContext.DbClsPlan.Where(a => a.Type == "User").Select(a => a.Quantity).FirstOrDefault();
                int Branch = oConnectionContext.DbClsPlan.Where(a => a.Type == "Branch").Select(a => a.Quantity).FirstOrDefault();
                int Item = oConnectionContext.DbClsPlan.Where(a => a.Type == "Item").Select(a => a.Quantity).FirstOrDefault();
                int Order = oConnectionContext.DbClsPlan.Where(a => a.Type == "Order").Select(a => a.Quantity).FirstOrDefault();
                int Bill = oConnectionContext.DbClsPlan.Where(a => a.Type == "Bill").Select(a => a.Quantity).FirstOrDefault();
                int TaxSetting = oConnectionContext.DbClsPlan.Where(a => a.Type == "Tax Setting").Select(a => a.Quantity).FirstOrDefault();

                var CurrentPlan = new
                {
                    BaseBranch = Branch,
                    BaseItem = Item,
                    BaseOrder = Order,
                    BaseUser = User,
                    BaseBill = Bill,
                    BaseTaxSetting = TaxSetting
                };

                //    var CurrentPlan = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
                //a.StartDate != null && a.Status == 2).Select(a => new
                //{
                //    a.BaseBranch,
                //    a.BaseItem,
                //    a.BaseOrder,
                //    a.BaseUser,
                //    a.BaseBill,
                //    a.BaseTaxSetting
                //}).FirstOrDefault();

                obj.TransactionNo = DateTime.Now.ToFileTime().ToString();

                decimal conversionRate = 1;

                //obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
                obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();

                obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();
                if (obj.CountryId == oConnectionContext.DbClsUserCountryMap.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => a.CountryId).FirstOrDefault())
                {
                    obj.CurrencyId = (from a in oConnectionContext.DbClsUserCurrencyMap
                                      join b in oConnectionContext.DbClsCurrency
               on a.CurrencyId equals b.CurrencyId
                                      where a.CompanyId == obj.Under && a.IsMain == true
                                      select b.CurrencyId).FirstOrDefault();
                    conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => a.ConversionRate).FirstOrDefault();
                }
                else
                {
                    obj.CurrencyId = 3;
                    conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == 3).Select(a => a.ConversionRate).FirstOrDefault();
                }
                //decimal PriceHikePercentage = oConnectionContext.DbClsCountry.Where(a => a.CountryId == CountryId).Select(a => a.PriceHikePercentage / 100).FirstOrDefault();

                decimal PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(a => a.CountryId == obj.CountryId && a.CompanyId == obj.Under).Select(a => a.PriceHikePercentage).FirstOrDefault();

                var TermLength = oConnectionContext.DbClsTermLength.Where(a => a.IsActive == true &&
                a.IsDeleted == false && a.TermLengthId == obj.TermLengthId).Select(a => new { a.MRP, a.SellingPrice, a.Months, a.DiscountPercentage }).FirstOrDefault();

                decimal addonsSubTotal = 0;
                foreach (var item in obj.TransactionDetails)
                {
                    item.MRP = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                        (((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate)).FirstOrDefault();

                    //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                    //    (((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate) - ((TermLength.DiscountPercentage / 100) *
                    //    (((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate))).FirstOrDefault();

                    int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();
                    if (pricingType == 2)
                    {
                        item.SubTotal = item.Quantity * Math.Round(item.MRP, 2);
                        //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2);
                    }
                    else
                    {
                        item.SubTotal = item.Quantity * Math.Round(item.MRP, 2) * TermLength.Months;
                        //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2) * TermLength.Months;
                    }

                    //item.PayableCost = item.SubTotal;
                    addonsSubTotal = addonsSubTotal + item.SubTotal;
                }

                decimal planSubTotal = Math.Round((((TermLength.MRP * PriceHikePercentage) + TermLength.MRP) / conversionRate), 2) * TermLength.Months;
                obj.SubTotal = planSubTotal + addonsSubTotal;

                // Calculate Nominal SubTotal (base price without price hikes)
                decimal nominalPlanSubTotal = Math.Round((TermLength.MRP / conversionRate), 2) * TermLength.Months;
                decimal nominalAddonsSubTotal = CalculateNominalSubTotal(obj, conversionRate, TermLength.Months);
                obj.NominalSubTotal = nominalPlanSubTotal + nominalAddonsSubTotal;

                if (obj.CouponId != 0)
                {
                    var coupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId).Select(a => new
                    {
                        a.Discount,
                        a.DiscountType,
                        a.MinimumPurchaseAmount,
                        a.MaximumDiscountAmount,
                        a.ApplyToBasePlan,
                        a.ApplyToAddons
                    }).FirstOrDefault();

                    if (coupon != null)
                    {
                        // Validate term length restrictions
                        var couponTermLengths = oConnectionContext.DbClsCouponTermLength
                            .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false).Select(a => a.TermLengthId).ToList();

                        // If coupon has term length restrictions, validate against selected term length
                        if (couponTermLengths.Count > 0 && !couponTermLengths.Contains(obj.TermLengthId))
                        {
                            // Coupon not valid for this term length - set discount to 0
                            obj.CouponDiscount = 0;
                        }
                        else
                        {
                            // Calculate coupon applicable subtotal based on settings
                            decimal couponSubTotal = 0;
                            
                            if (coupon.ApplyToBasePlan)
                            {
                                couponSubTotal += planSubTotal;
                            }
                            
                            if (coupon.ApplyToAddons)
                            {
                                // Get allowed addon IDs for this coupon
                                var allowedAddonIds = oConnectionContext.DbClsCouponAddon
                                    .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false)
                                    .Select(a => a.PlanAddonsId).ToList();
                                
                                decimal applicableAddonsSubTotal = 0;
                                
                                if (allowedAddonIds.Count == 0)
                                {
                                    // No specific addons selected = apply to all addons
                                    applicableAddonsSubTotal = addonsSubTotal;
                                }
                                else
                                {
                                    // Apply only to specific selected addons
                                    foreach (var item in obj.TransactionDetails)
                                    {
                                        if (item.PlanAddonsId > 0 && allowedAddonIds.Contains(item.PlanAddonsId))
                                        {
                                            applicableAddonsSubTotal += item.SubTotal;
                                        }
                                    }
                                }
                                
                                couponSubTotal += applicableAddonsSubTotal;
                            }

                            // Calculate discount
                            decimal calculatedDiscount = 0;
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

                            obj.CouponDiscount = calculatedDiscount;
                        }
                    }
                }

                obj.YearlyPlanDiscountPercentage = TermLength.DiscountPercentage;
                obj.YearlyPlanDiscount = ((TermLength.DiscountPercentage / 100) * obj.SubTotal);

                obj.PayableCost = obj.SubTotal - obj.CouponDiscount - obj.YearlyPlanDiscount;

                long RootId = oConnectionContext.DbClsUser.Where(a => a.IsRootAccount == true).Select(a => a.UserId).FirstOrDefault();
                obj.WhitelabelCommissionPercent = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.Under).Select(a => a.CommissionPercent).FirstOrDefault();
                obj.ResellerCommissionPercent = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsUser.Where(b => b.UserId == obj.AddedBy).Select(b => b.ResellerId).FirstOrDefault()).Select(a => a.CommissionPercent).FirstOrDefault();
                if (RootId == obj.Under)
                {
                    obj.RootAccountCommissionPercent = 100 - obj.ResellerCommissionPercent;
                }
                else
                {
                    obj.RootAccountCommissionPercent = 100 - obj.WhitelabelCommissionPercent;
                    obj.WhitelabelCommissionPercent = obj.WhitelabelCommissionPercent - obj.ResellerCommissionPercent;
                }

                // Calculate commission amounts
                // RootAccount commission based on NominalSubTotal (as per whitelabel terms)
                obj.RootAccountCommission = (obj.RootAccountCommissionPercent / 100) * obj.NominalSubTotal;

                // Whitelabel and Reseller commissions based on PayableCost (includes price hikes)
                obj.WhitelabelCommission = (obj.WhitelabelCommissionPercent / 100) * obj.PayableCost;
                obj.ResellerCommission = (obj.ResellerCommissionPercent / 100) * obj.PayableCost;

                ClsTransaction oClsTransaction = new ClsTransaction()
                {
                    TransactionNo = obj.TransactionNo,
                    ParentTransactionId = 0,
                    CouponDiscount = obj.CouponDiscount,
                    PayableCost = obj.PayableCost,
                    PlanId = oConnectionContext.DbClsPlan.Select(a => a.PlanId).FirstOrDefault(),
                    Status = 1,
                    SubTotal = obj.SubTotal,
                    TermLengthId = obj.TermLengthId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    BaseBranch = CurrentPlan.BaseBranch,
                    BaseOrder = CurrentPlan.BaseOrder,
                    BaseItem = CurrentPlan.BaseItem,
                    BaseUser = CurrentPlan.BaseUser,
                    BaseBill = CurrentPlan.BaseBill,
                    BaseTaxSetting = CurrentPlan.BaseTaxSetting,
                    IsTrial = false,
                    RootAccountCommissionPercent = obj.RootAccountCommissionPercent,
                    WhitelabelCommissionPercent = obj.WhitelabelCommissionPercent,
                    ResellerCommissionPercent = obj.ResellerCommissionPercent,
                    NominalSubTotal = obj.NominalSubTotal,
                    RootAccountCommission = obj.RootAccountCommission,
                    WhitelabelCommission = obj.WhitelabelCommission,
                    ResellerCommission = obj.ResellerCommission,
                    CouponId = obj.CouponId,
                    CurrencyId = obj.CurrencyId,
                    Months = TermLength.Months,
                    YearlyPlanDiscount = obj.YearlyPlanDiscount,
                    YearlyPlanDiscountPercentage = obj.YearlyPlanDiscountPercentage
                };
                oConnectionContext.DbClsTransaction.Add(oClsTransaction);
                oConnectionContext.SaveChanges();

                foreach (var item in obj.TransactionDetails)
                {
                    if (item.PlanAddonsId == 0)
                    {
                        item.MRP = ((TermLength.MRP * PriceHikePercentage) + TermLength.MRP) / conversionRate;
                        //item.UnitCost = ((TermLength.SellingPrice * PriceHikePercentage) + TermLength.SellingPrice) / conversionRate;
                        item.SubTotal = item.MRP * TermLength.Months;
                        //item.PayableCost = item.UnitCost * TermLength.Months;
                    }
                    else
                    {
                        int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();
                        item.MRP = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => ((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate).FirstOrDefault();
                        //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => ((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate).FirstOrDefault();
                        if (pricingType == 2)
                        {
                            item.SubTotal = item.Quantity * item.MRP;
                            //item.PayableCost = item.Quantity * item.UnitCost;
                        }
                        else
                        {
                            item.SubTotal = item.Quantity * item.MRP * TermLength.Months;
                            //item.PayableCost = item.Quantity * item.UnitCost * TermLength.Months;
                        }
                        //item.PayableCost = item.SubTotal;
                    }

                    ClsTransactionDetails oClsTransactionDetails = new ClsTransactionDetails()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        //Discount = item.Discount,
                        IsActive = item.IsActive,
                        IsDeleted = false,
                        PlanAddonsId = item.PlanAddonsId,
                        Quantity = item.Quantity,
                        SubTotal = item.SubTotal,
                        //PayableCost = item.PayableCost,
                        TransactionId = oClsTransaction.TransactionId,
                        MRP = item.MRP,
                        //UnitCost = item.UnitCost,
                        Type = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Count() == 0 ? "Base Plan" :
                        oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.Type).DefaultIfEmpty().FirstOrDefault()
                    };
                    oConnectionContext.DbClsTransactionDetails.Add(oClsTransactionDetails);
                    oConnectionContext.SaveChanges();
                }

                var Transaction = new ClsTransactionVm
                {
                    MainCountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.CountryId).FirstOrDefault(),
                    CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault(),
                    BusinessLogo = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.BusinessLogo).FirstOrDefault(),
                    BusinessName = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.BusinessName).FirstOrDefault(),
                    TransactionId = oClsTransaction.TransactionId,
                    TransactionNo = obj.TransactionNo,
                    PayableCost = obj.PayableCost,
                    Name = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.Name).FirstOrDefault(),
                    EmailId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.EmailId).FirstOrDefault(),
                    MobileNo = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.MobileNo).FirstOrDefault(),
                    OnlinePaymentSettings = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.IsDeleted == false && a.IsActive == true
                    && a.CompanyId == obj.Under).Select(a => new ClsOnlinePaymentSettingsVm
                    {
                        OnlinePaymentSettingsId = a.OnlinePaymentSettingsId,
                        OnlinePaymentService = a.OnlinePaymentService,
                        RazorpayKey = a.RazorpayKey,
                        RazorpayCurrencyId = a.RazorpayCurrencyId,
                        PaypalClientId = a.PaypalClientId,
                        PaypalCurrencyId = a.PaypalCurrencyId,
                        IsDefault = a.IsDefault,
                        SaveAs = a.SaveAs,
                        PaypalCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.PaypalCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault(),
                        RazorpayCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.RazorpayCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault()
                    }).OrderBy(a => a.OnlinePaymentSettingsId).ToList()
                };

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Subscription Plan",
                    CompanyId = obj.CompanyId,
                    Description = "Subscription Plan \"" + obj.TransactionNo + "\" created",
                    Id = oClsTransaction.TransactionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Transaction created successfully",
                    Data = new
                    {
                        Transaction = Transaction
                    }
                };
                dbContextTransaction.Complete();
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertTransaction_Paid(ClsTransactionVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int User = oConnectionContext.DbClsPlan.Where(a => a.Type == "User").Select(a => a.Quantity).FirstOrDefault();
                int Branch = oConnectionContext.DbClsPlan.Where(a => a.Type == "Branch").Select(a => a.Quantity).FirstOrDefault();
                int Item = oConnectionContext.DbClsPlan.Where(a => a.Type == "Item").Select(a => a.Quantity).FirstOrDefault();
                int Order = oConnectionContext.DbClsPlan.Where(a => a.Type == "Order").Select(a => a.Quantity).FirstOrDefault();
                int Bill = oConnectionContext.DbClsPlan.Where(a => a.Type == "Bill").Select(a => a.Quantity).FirstOrDefault();
                int TaxSetting = oConnectionContext.DbClsPlan.Where(a => a.Type == "Tax Setting").Select(a => a.Quantity).FirstOrDefault();

                var CurrentPlan = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
          a.StartDate != null && a.Status == 2).Select(a => new
          {
              BaseBranch = Branch,
              BaseItem=Item,
              BaseOrder=Order,
              BaseUser=User,
              BaseBill=Bill,
              BaseTaxSetting=TaxSetting,
              a.TransactionId,
              a.StartDate,
              a.ExpiryDate
          }).FirstOrDefault();

                obj.TransactionNo = DateTime.Now.ToFileTime().ToString();

                //obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
                obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();

                decimal conversionRate = 1;
                obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();
                if (obj.CountryId == oConnectionContext.DbClsUserCountryMap.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => a.CountryId).FirstOrDefault())
                {
                    obj.CurrencyId = (from a in oConnectionContext.DbClsUserCurrencyMap
                                      join b in oConnectionContext.DbClsCurrency
               on a.CurrencyId equals b.CurrencyId
                                      where a.CompanyId == obj.Under && a.IsMain == true
                                      select b.CurrencyId).FirstOrDefault();
                    conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => a.ConversionRate).FirstOrDefault();
                }
                else
                {
                    obj.CurrencyId = 3;
                    conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == 3).Select(a => a.ConversionRate).FirstOrDefault();
                }
                //decimal PriceHikePercentage = oConnectionContext.DbClsCountry.Where(a => a.CountryId == CountryId).Select(a => a.PriceHikePercentage / 100).FirstOrDefault();

                decimal PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(a => a.CountryId == obj.CountryId && a.CompanyId == obj.Under).Select(a => a.PriceHikePercentage).FirstOrDefault();

                var TermLength = oConnectionContext.DbClsTermLength.Where(a => a.IsActive == true &&
                a.IsDeleted == false && a.TermLengthId == obj.TermLengthId).Select(a => new { a.MRP, a.SellingPrice, a.Months, a.DiscountPercentage }).FirstOrDefault();

                decimal addonsSubTotal = 0;
                foreach (var item in obj.TransactionDetails)
                {
                    item.MRP = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                         (((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate)).FirstOrDefault();

                    //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                    //     (((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate) - ((TermLength.DiscountPercentage / 100) *
                    //     (((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate))).FirstOrDefault();

                    int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();
                    if (pricingType == 2)
                    {
                        item.SubTotal = item.Quantity * Math.Round(item.MRP, 2);
                        //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2);
                    }
                    else
                    {
                        item.SubTotal = item.Quantity * Math.Round(item.MRP, 2) * TermLength.Months;
                        //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2) * TermLength.Months;
                    }
                    //item.PayableCost = item.SubTotal;
                    addonsSubTotal = addonsSubTotal + item.SubTotal;
                }

                obj.SubTotal = (Math.Round((((TermLength.MRP * PriceHikePercentage) + TermLength.MRP) / conversionRate), 2) * TermLength.Months) + addonsSubTotal;

                if (obj.CouponId != 0)
                {
                    var coupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId).Select(a => new
                    {
                        a.Discount,
                        a.DiscountType
                    }).FirstOrDefault();

                    if (coupon != null)
                    {
                        if (coupon.DiscountType == 1)
                        {
                            obj.CouponDiscount = coupon.Discount;
                        }
                        else
                        {
                            obj.CouponDiscount = (coupon.Discount / 100) * obj.SubTotal;
                        }
                    }
                }

                obj.YearlyPlanDiscountPercentage = TermLength.DiscountPercentage;
                obj.YearlyPlanDiscount = ((TermLength.DiscountPercentage / 100) * obj.SubTotal);

                obj.PayableCost = obj.SubTotal - obj.CouponDiscount - obj.YearlyPlanDiscount;

                long RootId = oConnectionContext.DbClsUser.Where(a => a.IsRootAccount == true).Select(a => a.UserId).FirstOrDefault();
                obj.WhitelabelCommissionPercent = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.Under).Select(a => a.CommissionPercent).FirstOrDefault();
                obj.ResellerCommissionPercent = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsUser.Where(b => b.UserId == obj.AddedBy).Select(b => b.ResellerId).FirstOrDefault()).Select(a => a.CommissionPercent).FirstOrDefault();
                if (RootId == obj.Under)
                {
                    obj.RootAccountCommissionPercent = 100 - obj.ResellerCommissionPercent;
                }
                else
                {
                    obj.RootAccountCommissionPercent = 100 - obj.WhitelabelCommissionPercent;
                    obj.WhitelabelCommissionPercent = obj.WhitelabelCommissionPercent - obj.ResellerCommissionPercent;
                }

                // Calculate commission amounts
                // RootAccount commission based on NominalSubTotal (as per whitelabel terms)
                obj.RootAccountCommission = (obj.RootAccountCommissionPercent / 100) * obj.NominalSubTotal;

                // Whitelabel and Reseller commissions based on PayableCost (includes price hikes)
                obj.WhitelabelCommission = (obj.WhitelabelCommissionPercent / 100) * obj.PayableCost;
                obj.ResellerCommission = (obj.ResellerCommissionPercent / 100) * obj.PayableCost;

                ClsTransaction oClsTransaction = new ClsTransaction()
                {
                    TransactionNo = obj.TransactionNo,
                    ParentTransactionId = 0,
                    CouponDiscount = obj.CouponDiscount,
                    PayableCost = obj.PayableCost,
                    PlanId = oConnectionContext.DbClsPlan.Select(a => a.PlanId).FirstOrDefault(),
                    SubTotal = obj.SubTotal,
                    TermLengthId = obj.TermLengthId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    BaseBranch = CurrentPlan.BaseBranch,
                    BaseOrder = CurrentPlan.BaseOrder,
                    BaseItem = CurrentPlan.BaseItem,
                    BaseUser = CurrentPlan.BaseUser,
                    BaseBill = CurrentPlan.BaseBill,
                    BaseTaxSetting = CurrentPlan.BaseTaxSetting,
                    IsTrial = false,
                    PaidOn = CurrentDate,
                    Status = 2,
                    LiveTransactionId = obj.LiveTransactionId,
                    PaymentGatewayType = obj.PaymentGatewayType,
                    PaymentMethodType = obj.PaymentMethodType,
                    RootAccountCommissionPercent = obj.RootAccountCommissionPercent,
                    WhitelabelCommissionPercent = obj.WhitelabelCommissionPercent,
                    ResellerCommissionPercent = obj.ResellerCommissionPercent,
                    NominalSubTotal = obj.NominalSubTotal,
                    RootAccountCommission = obj.RootAccountCommission,
                    WhitelabelCommission = obj.WhitelabelCommission,
                    ResellerCommission = obj.ResellerCommission,
                    CouponId = obj.CouponId,
                    CurrencyId = obj.CurrencyId,
                    Months = TermLength.Months,
                    YearlyPlanDiscount = obj.YearlyPlanDiscount,
                    YearlyPlanDiscountPercentage = obj.YearlyPlanDiscountPercentage
                };
                oConnectionContext.DbClsTransaction.Add(oClsTransaction);
                oConnectionContext.SaveChanges();

                foreach (var item in obj.TransactionDetails)
                {
                    if (item.PlanAddonsId == 0)
                    {
                        item.MRP = ((TermLength.MRP * PriceHikePercentage) + TermLength.MRP) / conversionRate;
                        //item.UnitCost = ((TermLength.SellingPrice * PriceHikePercentage) + TermLength.SellingPrice) / conversionRate;
                        item.SubTotal = item.MRP * TermLength.Months;
                        //item.PayableCost = item.UnitCost * TermLength.Months;
                    }
                    else
                    {
                        int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();
                        item.MRP = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => ((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate).FirstOrDefault();
                        //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => ((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate).FirstOrDefault();
                        if (pricingType == 2)
                        {
                            item.SubTotal = item.Quantity * item.MRP;
                            //item.PayableCost = item.Quantity * item.UnitCost;
                        }
                        else
                        {
                            item.SubTotal = item.Quantity * item.MRP * TermLength.Months;
                            //item.PayableCost = item.Quantity * item.UnitCost * TermLength.Months;
                        }
                        //item.PayableCost = item.SubTotal;
                    }

                    ClsTransactionDetails oClsTransactionDetails = new ClsTransactionDetails()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        //Discount = item.Discount,
                        IsActive = item.IsActive,
                        IsDeleted = false,
                        PlanAddonsId = item.PlanAddonsId,
                        Quantity = item.Quantity,
                        SubTotal = item.SubTotal,
                        //PayableCost = item.PayableCost,
                        TransactionId = oClsTransaction.TransactionId,
                        MRP = item.MRP,
                        //UnitCost = item.UnitCost,
                        Type = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Count() == 0 ? "Base Plan" :
                        oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.Type).DefaultIfEmpty().FirstOrDefault()
                    };
                    oConnectionContext.DbClsTransactionDetails.Add(oClsTransactionDetails);
                    oConnectionContext.SaveChanges();
                }

                //     var CurrentPlan = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.AddedBy &&
                //a.StartDate != null && a.Status == 2).Select(a => new { a.TransactionId, a.StartDate, a.ExpiryDate }).FirstOrDefault();
                if (CurrentPlan.ExpiryDate.Value.Date < CurrentDate)
                {
                    //string query = "update tblTransaction set isactive=0 where TransactionId=" + CurrentPlan.TransactionId;
                    //oConnectionContext.Database.ExecuteSqlCommand(query);

                    var NextPlan = oConnectionContext.DbClsTransaction.OrderBy(a => a.TransactionId).Where(a => a.CompanyId == obj.AddedBy &&
            a.StartDate == null && a.Status == 2).Select(a => new { a.TransactionId, a.StartDate, a.ExpiryDate }).FirstOrDefault();

                    if (NextPlan != null)
                    {
                        ClsTransaction oClsTransaction1 = new ClsTransaction()
                        {
                            TransactionId = NextPlan.TransactionId,
                            StartDate = CurrentDate,
                            ExpiryDate = CurrentDate.AddMonths(oConnectionContext.DbClsTransactionDetails.Where(b => b.TransactionId == NextPlan.TransactionId).Select(b => b.Quantity).FirstOrDefault()),
                            //ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            Status = 2,
                            IsActive = true
                        };
                        ConnectionContext ocon = new ConnectionContext();
                        ocon.DbClsTransaction.Attach(oClsTransaction1);
                        ocon.Entry(oClsTransaction1).Property(x => x.StartDate).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.ExpiryDate).IsModified = true;
                        //oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedBy).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.ModifiedOn).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.Status).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.IsActive).IsModified = true;
                        ocon.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Subscription Plan",
                    CompanyId = obj.CompanyId,
                    Description = "Subscription Plan \"" + obj.TransactionNo + "\" purchased",
                    Id = oClsTransaction.TransactionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                dbContextTransaction.Complete();

                var Transaction = new ClsTransactionVm
                {
                    TransactionNo = obj.TransactionNo,
                    //CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault(),
                    //TransactionId = oClsTransaction.TransactionId,
                    //PayableCost = obj.PayableCost,
                    //Name = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.Name).FirstOrDefault(),
                    //EmailId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.EmailId).FirstOrDefault(),
                    //MobileNo = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.MobileNo).FirstOrDefault(),
                };

                data = new
                {
                    Status = 1,
                    Message = "Transaction created successfully",
                    Data = new
                    {
                        Transaction = Transaction
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertAddonTransaction(ClsTransactionVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.TransactionNo = DateTime.Now.ToFileTime().ToString();

                long ParentTransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == obj.CompanyId &&
               a.StartDate != null).Select(a => a.TransactionId).FirstOrDefault();

                int MonthsLeft = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == obj.CompanyId &&
          a.StartDate != null).Select(a => Math.Abs(12 * (CurrentDate.Year - a.ExpiryDate.Value.Year) + CurrentDate.Month - a.ExpiryDate.Value.Month)).FirstOrDefault();

                decimal conversionRate = 1;
                //obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
                obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();
                obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();
                if (obj.CountryId == oConnectionContext.DbClsUserCountryMap.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => a.CountryId).FirstOrDefault())
                {
                    obj.CurrencyId = (from a in oConnectionContext.DbClsUserCurrencyMap
                                      join b in oConnectionContext.DbClsCurrency
               on a.CurrencyId equals b.CurrencyId
                                      where a.CompanyId == obj.Under && a.IsMain == true
                                      select b.CurrencyId).FirstOrDefault();
                    conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => a.ConversionRate).FirstOrDefault();
                }
                else
                {
                    obj.CurrencyId = 3;
                    conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == 3).Select(a => a.ConversionRate).FirstOrDefault();
                }

                decimal PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(a => a.CountryId == obj.CountryId && a.CompanyId == obj.Under).Select(a => a.PriceHikePercentage).FirstOrDefault();

                decimal DiscountPercentage = oConnectionContext.DbClsTermLength.Where(a => a.TermLengthId ==
                (oConnectionContext.DbClsTransaction.Where(b => b.TransactionId == ParentTransactionId &&
                b.CompanyId == obj.CompanyId).Select(b => b.TermLengthId).FirstOrDefault())
                ).Select(a => a.DiscountPercentage).FirstOrDefault();

                decimal addonsSubTotal = 0;
                foreach (var item in obj.TransactionDetails)
                {
                    int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();

                    item.MRP = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                    (((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate)).FirstOrDefault();

                    //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                    //(((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate) - ((DiscountPercentage / 100) * (((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate))).FirstOrDefault();

                    if (pricingType == 2)
                    {
                        item.SubTotal = item.Quantity * Math.Round(item.MRP, 2);
                        //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2);
                    }
                    else
                    {
                        item.SubTotal = item.Quantity * Math.Round(item.MRP, 2) * MonthsLeft;
                        //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2) * MonthsLeft;
                    }
                    //item.PayableCost = item.SubTotal;
                    addonsSubTotal = addonsSubTotal + item.SubTotal;
                }

                obj.SubTotal = addonsSubTotal;

                if (obj.CouponId != 0)
                {
                    var coupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId).Select(a => new
                    {
                        a.Discount,
                        a.DiscountType
                    }).FirstOrDefault();

                    if (coupon != null)
                    {
                        if (coupon.DiscountType == 1)
                        {
                            obj.CouponDiscount = coupon.Discount;
                        }
                        else
                        {
                            obj.CouponDiscount = (coupon.Discount / 100) * obj.SubTotal;
                        }
                    }
                }

                obj.YearlyPlanDiscountPercentage = DiscountPercentage;

                obj.YearlyPlanDiscount = ((DiscountPercentage / 100) * obj.SubTotal);

                obj.PayableCost = obj.SubTotal - obj.CouponDiscount - obj.YearlyPlanDiscount;

                long RootId = oConnectionContext.DbClsUser.Where(a => a.IsRootAccount == true).Select(a => a.UserId).FirstOrDefault();
                obj.WhitelabelCommissionPercent = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.Under).Select(a => a.CommissionPercent).FirstOrDefault();
                obj.ResellerCommissionPercent = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsUser.Where(b => b.UserId == obj.AddedBy).Select(b => b.ResellerId).FirstOrDefault()).Select(a => a.CommissionPercent).FirstOrDefault();
                if (RootId == obj.Under)
                {
                    obj.RootAccountCommissionPercent = 100 - obj.ResellerCommissionPercent;
                }
                else
                {
                    obj.RootAccountCommissionPercent = 100 - obj.WhitelabelCommissionPercent;
                    obj.WhitelabelCommissionPercent = obj.WhitelabelCommissionPercent - obj.ResellerCommissionPercent;
                }

                // Calculate commission amounts
                // RootAccount commission based on NominalSubTotal (as per whitelabel terms)
                obj.RootAccountCommission = (obj.RootAccountCommissionPercent / 100) * obj.NominalSubTotal;

                // Whitelabel and Reseller commissions based on PayableCost (includes price hikes)
                obj.WhitelabelCommission = (obj.WhitelabelCommissionPercent / 100) * obj.PayableCost;
                obj.ResellerCommission = (obj.ResellerCommissionPercent / 100) * obj.PayableCost;

                ClsTransaction oClsTransaction = new ClsTransaction()
                {
                    TransactionNo = obj.TransactionNo,
                    ParentTransactionId = ParentTransactionId,
                    CouponDiscount = obj.CouponDiscount,
                    PayableCost = obj.PayableCost,
                    PlanId = 0,
                    Status = 1,
                    SubTotal = obj.SubTotal,
                    TermLengthId = obj.TermLengthId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    BaseBranch = 0,
                    BaseOrder = 0,
                    BaseItem = 0,
                    BaseUser = 0,
                    IsTrial = false,
                    RootAccountCommissionPercent = obj.RootAccountCommissionPercent,
                    WhitelabelCommissionPercent = obj.WhitelabelCommissionPercent,
                    ResellerCommissionPercent = obj.ResellerCommissionPercent,
                    NominalSubTotal = obj.NominalSubTotal,
                    RootAccountCommission = obj.RootAccountCommission,
                    WhitelabelCommission = obj.WhitelabelCommission,
                    ResellerCommission = obj.ResellerCommission,
                    CouponId = obj.CouponId,
                    CurrencyId = obj.CurrencyId,
                    Months = MonthsLeft,
                    YearlyPlanDiscount = obj.YearlyPlanDiscount,
                    YearlyPlanDiscountPercentage = obj.YearlyPlanDiscountPercentage
                };
                oConnectionContext.DbClsTransaction.Add(oClsTransaction);
                oConnectionContext.SaveChanges();

                foreach (var item in obj.TransactionDetails)
                {
                    item.MRP = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => ((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate).FirstOrDefault();
                    //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => ((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate).FirstOrDefault();

                    int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();

                    if (pricingType == 2)
                    {
                        item.SubTotal = item.Quantity * item.MRP;
                        //item.PayableCost = item.Quantity * item.UnitCost;
                    }
                    else
                    {
                        item.SubTotal = item.Quantity * item.MRP * MonthsLeft;
                        //item.PayableCost = item.Quantity * item.UnitCost * MonthsLeft;
                    }
                    //item.PayableCost = item.SubTotal;

                    ClsTransactionDetails oClsTransactionDetails = new ClsTransactionDetails()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        //Discount = item.Discount,
                        IsActive = item.IsActive,
                        IsDeleted = false,
                        PlanAddonsId = item.PlanAddonsId,
                        Quantity = item.Quantity,
                        SubTotal = item.SubTotal,
                        //PayableCost = item.PayableCost,
                        TransactionId = oClsTransaction.TransactionId,
                        MRP = item.MRP,
                        //UnitCost = item.UnitCost,
                        Type = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.Type).DefaultIfEmpty().FirstOrDefault()
                    };
                    oConnectionContext.DbClsTransactionDetails.Add(oClsTransactionDetails);
                    oConnectionContext.SaveChanges();
                }

                var Transaction = new ClsTransactionVm
                {
                    MainCountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.CountryId).FirstOrDefault(),
                    CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault(),
                    BusinessLogo = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.BusinessLogo).FirstOrDefault(),
                    BusinessName = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.BusinessName).FirstOrDefault(),
                    TransactionId = oClsTransaction.TransactionId,
                    TransactionNo = obj.TransactionNo,
                    PayableCost = obj.PayableCost,
                    Name = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.Name).FirstOrDefault(),
                    EmailId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.EmailId).FirstOrDefault(),
                    MobileNo = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.MobileNo).FirstOrDefault(),
                    OnlinePaymentSettings = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.IsDeleted == false && a.IsActive == true
                    && a.CompanyId == obj.Under).Select(a => new ClsOnlinePaymentSettingsVm
                    {
                        OnlinePaymentSettingsId = a.OnlinePaymentSettingsId,
                        OnlinePaymentService = a.OnlinePaymentService,
                        RazorpayKey = a.RazorpayKey,
                        RazorpayCurrencyId = a.RazorpayCurrencyId,
                        PaypalClientId = a.PaypalClientId,
                        PaypalCurrencyId = a.PaypalCurrencyId,
                        IsDefault = a.IsDefault,
                        SaveAs = a.SaveAs,
                        PaypalCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.PaypalCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault(),
                        RazorpayCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.RazorpayCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault()
                    }).OrderBy(a => a.OnlinePaymentSettingsId).ToList()
                };

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Subscription Plan",
                    CompanyId = obj.CompanyId,
                    Description = "Subscription Plan \"" + obj.TransactionNo + "\" created",
                    Id = oClsTransaction.TransactionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                dbContextTransaction.Complete();
                data = new
                {
                    Status = 1,
                    Message = "Transaction created successfully",
                    Data = new
                    {
                        Transaction = Transaction
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertAddonTransaction_Paid(ClsTransactionVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.TransactionNo = DateTime.Now.ToFileTime().ToString();

                long ParentTransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == obj.CompanyId &&
               a.StartDate != null).Select(a => a.TransactionId).FirstOrDefault();

                int MonthsLeft = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == obj.CompanyId &&
          a.StartDate != null).Select(a => Math.Abs(12 * (CurrentDate.Year - a.ExpiryDate.Value.Year) + CurrentDate.Month - a.ExpiryDate.Value.Month)).FirstOrDefault();

                decimal conversionRate = 1;
                //obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
                obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();
                obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();
                if (obj.CountryId == oConnectionContext.DbClsUserCountryMap.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => a.CountryId).FirstOrDefault())
                {
                    obj.CurrencyId = (from a in oConnectionContext.DbClsUserCurrencyMap
                                      join b in oConnectionContext.DbClsCurrency
               on a.CurrencyId equals b.CurrencyId
                                      where a.CompanyId == obj.Under && a.IsMain == true
                                      select b.CurrencyId).FirstOrDefault();
                    conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => a.ConversionRate).FirstOrDefault();
                }
                else
                {
                    obj.CurrencyId = 3;
                    conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == 3).Select(a => a.ConversionRate).FirstOrDefault();
                }

                decimal PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(a => a.CountryId == obj.CountryId && a.CompanyId == obj.Under).Select(a => a.PriceHikePercentage).FirstOrDefault();

                decimal DiscountPercentage = oConnectionContext.DbClsTermLength.Where(a => a.TermLengthId ==
                (oConnectionContext.DbClsTransaction.Where(b => b.TransactionId == ParentTransactionId &&
                b.CompanyId == obj.CompanyId).Select(b => b.TermLengthId).FirstOrDefault())
                ).Select(a => a.DiscountPercentage).FirstOrDefault();

                decimal addonsSubTotal = 0;
                foreach (var item in obj.TransactionDetails)
                {
                    item.MRP = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                    (((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate)).FirstOrDefault();

                    //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                    //(((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate) - ((DiscountPercentage / 100) * (((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate))).FirstOrDefault();

                    int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();
                    if (pricingType == 2)
                    {
                        item.SubTotal = item.Quantity * Math.Round(item.MRP, 2);
                        //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2);
                    }
                    else
                    {
                        item.SubTotal = item.Quantity * Math.Round(item.MRP, 2) * MonthsLeft;
                        //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2) * MonthsLeft;
                    }
                    //item.PayableCost = item.SubTotal;
                    addonsSubTotal = addonsSubTotal + item.SubTotal;
                }

                obj.SubTotal = addonsSubTotal;

                // Calculate Nominal SubTotal (base price without price hikes - only addons in this case)
                decimal nominalAddonsSubTotal = 0;
                foreach (var item in obj.TransactionDetails)
                {
                    if (item.PlanAddonsId > 0)
                    {
                        decimal nominalAddonMRP = oConnectionContext.DbClsPlanAddons
                            .Where(a => a.PlanAddonsId == item.PlanAddonsId)
                            .Select(a => a.MRP / conversionRate).FirstOrDefault();
                        
                        int pricingType = oConnectionContext.DbClsPlanAddons
                            .Where(a => a.PlanAddonsId == item.PlanAddonsId)
                            .Select(a => a.PricingType).FirstOrDefault();
                        
                        if (pricingType == 2)
                        {
                            nominalAddonsSubTotal += item.Quantity * Math.Round(nominalAddonMRP, 2);
                        }
                        else
                        {
                            nominalAddonsSubTotal += item.Quantity * Math.Round(nominalAddonMRP, 2) * MonthsLeft;
                        }
                    }
                }
                obj.NominalSubTotal = nominalAddonsSubTotal;

                if (obj.CouponId != 0)
                {
                    var coupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId).Select(a => new
                    {
                        a.Discount,
                        a.DiscountType
                    }).FirstOrDefault();

                    if (coupon != null)
                    {
                        if (coupon.DiscountType == 1)
                        {
                            obj.CouponDiscount = coupon.Discount;
                        }
                        else
                        {
                            obj.CouponDiscount = (coupon.Discount / 100) * obj.SubTotal;
                        }
                    }
                }

                obj.YearlyPlanDiscountPercentage = DiscountPercentage;
                obj.YearlyPlanDiscount = ((DiscountPercentage / 100) * obj.SubTotal);

                obj.PayableCost = obj.SubTotal - obj.CouponDiscount - obj.YearlyPlanDiscount;

                long RootId = oConnectionContext.DbClsUser.Where(a => a.IsRootAccount == true).Select(a => a.UserId).FirstOrDefault();
                obj.WhitelabelCommissionPercent = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.Under).Select(a => a.CommissionPercent).FirstOrDefault();
                obj.ResellerCommissionPercent = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsUser.Where(b => b.UserId == obj.AddedBy).Select(b => b.ResellerId).FirstOrDefault()).Select(a => a.CommissionPercent).FirstOrDefault();
                if (RootId == obj.Under)
                {
                    obj.RootAccountCommissionPercent = 100 - obj.ResellerCommissionPercent;
                }
                else
                {
                    obj.RootAccountCommissionPercent = 100 - obj.WhitelabelCommissionPercent;
                    obj.WhitelabelCommissionPercent = obj.WhitelabelCommissionPercent - obj.ResellerCommissionPercent;
                }

                // Calculate commission amounts
                // RootAccount commission based on NominalSubTotal (as per whitelabel terms)
                obj.RootAccountCommission = (obj.RootAccountCommissionPercent / 100) * obj.NominalSubTotal;

                // Whitelabel and Reseller commissions based on PayableCost (includes price hikes)
                obj.WhitelabelCommission = (obj.WhitelabelCommissionPercent / 100) * obj.PayableCost;
                obj.ResellerCommission = (obj.ResellerCommissionPercent / 100) * obj.PayableCost;

                ClsTransaction oClsTransaction = new ClsTransaction()
                {
                    TransactionNo = obj.TransactionNo,
                    ParentTransactionId = ParentTransactionId,
                    CouponDiscount = obj.CouponDiscount,
                    PayableCost = obj.PayableCost,
                    PlanId = 0,
                    SubTotal = obj.SubTotal,
                    TermLengthId = obj.TermLengthId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    BaseBranch = 0,
                    BaseOrder = 0,
                    BaseItem = 0,
                    BaseUser = 0,
                    IsTrial = false,
                    PaidOn = CurrentDate,
                    Status = 2,
                    LiveTransactionId = obj.LiveTransactionId,
                    PaymentGatewayType = obj.PaymentGatewayType,
                    PaymentMethodType = obj.PaymentMethodType,
                    RootAccountCommissionPercent = obj.RootAccountCommissionPercent,
                    WhitelabelCommissionPercent = obj.WhitelabelCommissionPercent,
                    ResellerCommissionPercent = obj.ResellerCommissionPercent,
                    NominalSubTotal = obj.NominalSubTotal,
                    RootAccountCommission = obj.RootAccountCommission,
                    WhitelabelCommission = obj.WhitelabelCommission,
                    ResellerCommission = obj.ResellerCommission,
                    CouponId = obj.CouponId,
                    CurrencyId = obj.CurrencyId,
                    Months = MonthsLeft,
                    YearlyPlanDiscount = obj.YearlyPlanDiscount,
                    YearlyPlanDiscountPercentage = obj.YearlyPlanDiscountPercentage
                };
                oConnectionContext.DbClsTransaction.Add(oClsTransaction);
                oConnectionContext.SaveChanges();

                foreach (var item in obj.TransactionDetails)
                {
                    int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();
                    item.MRP = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => ((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate).FirstOrDefault();
                    //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => ((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate).FirstOrDefault();
                    if (pricingType == 2)
                    {
                        item.SubTotal = item.Quantity * item.MRP;
                        //item.PayableCost = item.Quantity * item.UnitCost;
                    }
                    else
                    {
                        item.SubTotal = item.Quantity * item.MRP * MonthsLeft;
                        //item.PayableCost = item.Quantity * item.UnitCost * MonthsLeft;
                    }
                    //item.PayableCost = item.SubTotal;

                    ClsTransactionDetails oClsTransactionDetails = new ClsTransactionDetails()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        //Discount = item.Discount,
                        IsActive = item.IsActive,
                        IsDeleted = false,
                        PlanAddonsId = item.PlanAddonsId,
                        Quantity = item.Quantity,
                        SubTotal = item.SubTotal,
                        //PayableCost = item.PayableCost,
                        TransactionId = oClsTransaction.TransactionId,
                        MRP = item.MRP,
                        //UnitCost = item.UnitCost,
                        Type = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.Type).DefaultIfEmpty().FirstOrDefault()
                    };
                    oConnectionContext.DbClsTransactionDetails.Add(oClsTransactionDetails);
                    oConnectionContext.SaveChanges();
                }

                var CurrentPlan = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.AddedBy &&
           a.StartDate != null && a.Status == 2).Select(a => new { a.TransactionId, a.StartDate, a.ExpiryDate }).FirstOrDefault();
                if (CurrentPlan.ExpiryDate.Value.Date < CurrentDate.Date)
                {
                    //string query = "update tblTransaction set isactive=0 where TransactionId=" + CurrentPlan.TransactionId;
                    //oConnectionContext.Database.ExecuteSqlCommand(query);

                    var NextPlan = oConnectionContext.DbClsTransaction.OrderBy(a => a.TransactionId).Where(a => a.CompanyId == obj.AddedBy &&
            a.StartDate == null && a.Status == 2).Select(a => new { a.TransactionId, a.StartDate, a.ExpiryDate }).FirstOrDefault();

                    if (NextPlan != null)
                    {
                        ClsTransaction oClsTransaction1 = new ClsTransaction()
                        {
                            TransactionId = NextPlan.TransactionId,
                            StartDate = CurrentDate,
                            ExpiryDate = CurrentDate.AddMonths(oConnectionContext.DbClsTransactionDetails.Where(b => b.TransactionId == NextPlan.TransactionId).Select(b => b.Quantity).FirstOrDefault()),
                            //ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            Status = 2,
                            IsActive = true
                        };
                        ConnectionContext ocon = new ConnectionContext();
                        ocon.DbClsTransaction.Attach(oClsTransaction1);
                        ocon.Entry(oClsTransaction1).Property(x => x.StartDate).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.ExpiryDate).IsModified = true;
                        //oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedBy).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.ModifiedOn).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.Status).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.IsActive).IsModified = true;
                        ocon.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Subscription Plan",
                    CompanyId = obj.CompanyId,
                    Description = "Subscription Plan \"" + obj.TransactionNo + "\" purchased",
                    Id = oClsTransaction.TransactionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                dbContextTransaction.Complete();

                var Transaction = new ClsTransactionVm
                {
                    TransactionNo = obj.TransactionNo,
                    //TransactionId = oClsTransaction.TransactionId,
                    //PayableCost = obj.PayableCost,
                    //Name = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.Name).FirstOrDefault(),
                    //EmailId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.EmailId).FirstOrDefault(),
                    //MobileNo = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.MobileNo).FirstOrDefault(),
                };

                data = new
                {
                    Status = 1,
                    Message = "Transaction created successfully",
                    Data = new
                    {
                        Transaction = Transaction
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TransactionDelete(ClsTransactionVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsTransaction oClsTransaction = new ClsTransaction()
                {
                    TransactionId = obj.TransactionId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsTransaction.Attach(oClsTransaction);
                oConnectionContext.Entry(oClsTransaction).Property(x => x.TransactionId).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Subscription Plan",
                    CompanyId = obj.CompanyId,
                    Description = "Subscription Plan \"" + oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == obj.TransactionId).Select(a => a.TransactionNo).FirstOrDefault() + "\" deleted",
                    Id = oClsTransaction.TransactionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Invoice deleted successfully",
                    Data = new
                    {
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CurrentPlan(ClsTransaction obj)
        {
            long TransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate != null && a.Status == 2).Select(a => a.TransactionId).FirstOrDefault();

            var Transaction = oConnectionContext.DbClsTransaction.Where(a => a.TransactionId == TransactionId).Select(a => new ClsTransactionVm
            {
                StartDate = a.StartDate,
                IsActive = a.IsActive,
                AddedOn = a.AddedOn,
                IsTrial = a.IsTrial,
                TransactionId = a.TransactionId,
                PaidOn = a.PaidOn,
                ExpiryDate = a.ExpiryDate,
                Status = a.Status,
                //TransactionDetails = oConnectionContext.DbClsTransactionDetails.Where(b => b.TransactionId == a.TransactionId &&
                //b.PlanAddonsId != 0).Select(b => new ClsTransactionDetailsVm
                //{
                //    Title = oConnectionContext.DbClsPlanAddons.Where(c => c.PlanAddonsId == b.PlanAddonsId).Select(c => c.Title).FirstOrDefault(),
                //    Quantity = b.Quantity,
                //}).ToList(),
            }).FirstOrDefault();

            var PlanAddons = oConnectionContext.DbClsPlanAddons.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new ClsPlanAddonsVm
            {
                Type = a.Type,
                PlanAddonsId = a.PlanAddonsId,
                Title = a.Title,
                OrderNo = a.OrderNo,
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

            var UnusedPlans = oConnectionContext.DbClsTransaction.Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate == null && a.Status == 2 && a.ParentTransactionId == 0).Select(a => new ClsTransactionVm
            {
                TransactionId = a.TransactionId,
                TransactionNo = a.TransactionNo,
                PaidOn = a.PaidOn,
                PayableCost = a.PayableCost
            }).ToList();

            int TotalUser = oCommonController.fetchPlanQuantity(obj.CompanyId, "User");
            int TotalBranch = oCommonController.fetchPlanQuantity(obj.CompanyId, "Branch");
            int TotalItem = oCommonController.fetchPlanQuantity(obj.CompanyId, "Item");
            int TotalOrder = oCommonController.fetchPlanQuantity(obj.CompanyId, "Order");
            int TotalDomain = oCommonController.fetchPlanQuantity(obj.CompanyId, "Domain");
            int TotalCatalogue = oCommonController.fetchPlanQuantity(obj.CompanyId, "Catalogue");
            int TotalSms = oCommonController.fetchPlanQuantity(obj.CompanyId, "Sms");
            int TotalEmail = oCommonController.fetchPlanQuantity(obj.CompanyId, "Email");
            int TotalBill = oCommonController.fetchPlanQuantity(obj.CompanyId, "Bill");
            int TotalTaxSetting = oCommonController.fetchPlanQuantity(obj.CompanyId, "Tax Setting");

            int TotalUserUsed = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.UserType == "User").Count();
            int TotalBranchUsed = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Count();
            int TotalItemUsed = oConnectionContext.DbClsItemDetails.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false).Count();
            int TotalOrderUsed = oConnectionContext.DbClsSales.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            && (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();
            int TotalDomainUsed = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false).Count();
            int TotalCatalogueUsed = oConnectionContext.DbClsCatalogue.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false).Count();
            int TotalSmsUsed = oConnectionContext.DbClsSmsUsed.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
           && (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();
            int TotalEmailUsed = oConnectionContext.DbClsEmailUsed.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
           && (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();
            int TotalBillUsed = oConnectionContext.DbClsPurchase.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            && (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count() +
            oConnectionContext.DbClsExpense.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && (a.AddedOn.Date >= Transaction.StartDate.Value.Date && a.AddedOn.Date <= Transaction.ExpiryDate.Value.Date)).Count();
            int TotalTaxSettingUsed = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Count();

            decimal UserPercentageRemaining = 0;
            decimal BranchPercentageRemaining = 0;
            decimal ItemPercentageRemaining = 0;
            decimal OrderPercentageRemaining = 0;
            decimal DomainPercentageRemaining = 0;
            decimal CataloguePercentageRemaining = 0;
            decimal SmsPercentageRemaining = 0;
            decimal EmailPercentageRemaining = 0;
            decimal BillPercentageRemaining = 0;
            decimal TaxSettingPercentageRemaining = 0;

            if (TotalUser != 0)
            {
                UserPercentageRemaining = 100 - ((TotalUserUsed * 100) / TotalUser);
            }

            if (TotalBranch != 0)
            {
                BranchPercentageRemaining = 100 - ((TotalBranchUsed * 100) / TotalBranch);
            }

            if (TotalItem != 0)
            {
                ItemPercentageRemaining = Math.Round(100 - (((decimal)TotalItemUsed * 100) / (decimal)TotalItem), 2);
            }

            if (TotalOrder != 0)
            {
                OrderPercentageRemaining = 100 - ((TotalOrderUsed * 100) / TotalOrder);
            }

            if (TotalDomain != 0)
            {
                DomainPercentageRemaining = 100 - ((TotalDomainUsed * 100) / TotalDomain);
            }

            if (TotalCatalogue != 0)
            {
                CataloguePercentageRemaining = 100 - ((TotalCatalogueUsed * 100) / TotalCatalogue);
            }

            if (TotalSms != 0)
            {
                SmsPercentageRemaining = 100 - ((TotalSmsUsed * 100) / TotalSms);
            }

            if (TotalEmail != 0)
            {
                EmailPercentageRemaining = 100 - ((TotalEmailUsed * 100) / TotalEmail);
            }

            if (TotalBill != 0)
            {
                BillPercentageRemaining = 100 - ((TotalBillUsed * 100) / TotalBill);
            }

            if (TotalTaxSetting != 0)
            {
                TaxSettingPercentageRemaining = 100 - ((TotalTaxSettingUsed * 100) / TotalTaxSetting);
            }

            ClsMyPlan det = new ClsMyPlan
            {
                TotalUser = TotalUser,
                TotalBranch = TotalBranch,
                TotalItem = TotalItem,
                TotalOrder = TotalOrder,
                TotalDomain = TotalDomain,
                TotalCatalogue = TotalCatalogue,
                TotalSms = TotalSms,
                TotalEmail = TotalEmail,
                TotalBill = TotalBill,
                TotalTaxSetting = TotalTaxSetting,
                TotalUserUsed = TotalUserUsed,
                TotalBranchUsed = TotalBranchUsed,
                TotalItemUsed = TotalItemUsed,
                TotalOrderUsed = TotalOrderUsed,
                TotalDomainUsed = TotalDomainUsed,
                TotalCatalogueUsed = TotalCatalogueUsed,
                TotalSmsUsed = TotalSmsUsed,
                TotalEmailUsed = TotalEmailUsed,
                TotalBillUsed = TotalBillUsed,
                TotalTaxSettingUsed = TotalTaxSettingUsed,
                UserPercentageRemaining = UserPercentageRemaining,
                BranchPercentageRemaining = BranchPercentageRemaining,
                ItemPercentageRemaining = ItemPercentageRemaining,
                OrderPercentageRemaining = OrderPercentageRemaining,
                DomainPercentageRemaining = DomainPercentageRemaining,
                CataloguePercentageRemaining = CataloguePercentageRemaining,
                SmsPercentageRemaining = SmsPercentageRemaining,
                EmailPercentageRemaining = EmailPercentageRemaining,
                BillPercentageRemaining = BillPercentageRemaining,
                TaxSettingPercentageRemaining = TaxSettingPercentageRemaining,
                Transaction = Transaction,
                UnusedPlans = UnusedPlans,
                PlanAddons = PlanAddons
            };

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    MyPlan = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PaymentSuccess(ClsTransactionVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //var Quantity = oConnectionContext.DbClsTransactionDetails.Where(a => a.TransactionId == obj.TransactionId
                //&& a.Type == "Base Plan").Select(a => a.Quantity).FirstOrDefault();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                obj.TransactionId = oConnectionContext.DbClsTransaction.Where(a => a.TransactionNo == obj.TransactionNo).Select(a => a.TransactionId).FirstOrDefault();

                ClsTransaction oClsTransaction = new ClsTransaction()
                {
                    TransactionId = obj.TransactionId,
                    PaidOn = CurrentDate,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    Status = 2,
                    LiveTransactionId = obj.LiveTransactionId,
                    PaymentGatewayType = obj.PaymentGatewayType,
                    PaymentMethodType = obj.PaymentMethodType
                };
                oConnectionContext.DbClsTransaction.Attach(oClsTransaction);
                oConnectionContext.Entry(oClsTransaction).Property(x => x.PaidOn).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.LiveTransactionId).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.PaymentGatewayType).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.PaymentMethodType).IsModified = true;
                oConnectionContext.SaveChanges();

                var CurrentPlan = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
               a.StartDate != null && a.Status == 2).Select(a => new { a.TransactionId, a.StartDate, a.ExpiryDate }).FirstOrDefault();
                if (CurrentPlan.ExpiryDate.Value.Date < CurrentDate.Date)
                {
                    //string query = "update tblTransaction set isactive=0 where TransactionId=" + CurrentPlan.TransactionId;
                    //oConnectionContext.Database.ExecuteSqlCommand(query);

                    var NextPlan = oConnectionContext.DbClsTransaction.OrderBy(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
            a.StartDate == null && a.Status == 2).Select(a => new { a.TransactionId, a.StartDate, a.ExpiryDate }).FirstOrDefault();

                    if (NextPlan != null)
                    {
                        ClsTransaction oClsTransaction1 = new ClsTransaction()
                        {
                            TransactionId = NextPlan.TransactionId,
                            StartDate = CurrentDate,
                            ExpiryDate = CurrentDate.AddMonths(oConnectionContext.DbClsTransactionDetails.Where(b => b.TransactionId == NextPlan.TransactionId).Select(b => b.Quantity).FirstOrDefault()),
                            //ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            Status = 2,
                            IsActive = true
                        };
                        ConnectionContext ocon = new ConnectionContext();
                        ocon.DbClsTransaction.Attach(oClsTransaction1);
                        ocon.Entry(oClsTransaction1).Property(x => x.StartDate).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.ExpiryDate).IsModified = true;
                        //oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedBy).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.ModifiedOn).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.Status).IsModified = true;
                        ocon.Entry(oClsTransaction1).Property(x => x.IsActive).IsModified = true;
                        ocon.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Subscription Plan",
                    CompanyId = obj.CompanyId,
                    Description = "Subscription Plan \"" + obj.TransactionNo + "\" paid",
                    Id = obj.TransactionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment done successfully",
                    Data = new
                    {
                        Transaction = new ClsTransaction
                        {
                            TransactionNo = obj.TransactionNo
                        }
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActivatePlan(ClsTransactionVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                int currentUserQuantity = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.UserType == "User").Count();
                int currentBranchQuantity = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Count();
                int currentDomainQuantity = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Count();
                int currentTaxSettingQuantity = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Count();
                //int currentItemQuantity = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false).Count();

                var TransactionId = oConnectionContext.DbClsTransaction.Where(a => a.TransactionNo == obj.TransactionNo).Select(a => a.TransactionId).FirstOrDefault();

                string query = "update \"tblTransaction\" set \"IsActive\"=False where \"CompanyId\"=" + obj.CompanyId + " and \"IsActive\"=True";
                oConnectionContext.Database.ExecuteSqlCommand(query);

                ClsTransaction oClsTransaction = new ClsTransaction()
                {
                    TransactionId = TransactionId,
                    StartDate = CurrentDate,
                    ExpiryDate = CurrentDate.AddMonths(oConnectionContext.DbClsTransactionDetails.Where(b => b.TransactionId == TransactionId).Select(b => b.Quantity).FirstOrDefault()),
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    Status = 2,
                    IsActive = true
                };
                oConnectionContext.DbClsTransaction.Attach(oClsTransaction);
                oConnectionContext.Entry(oClsTransaction).Property(x => x.StartDate).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.ExpiryDate).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.SaveChanges();

                int nextBranchQuantity = 0, nextUserQuantity = 0, nextDomainQuantity = 0, nextTaxSettingQuantity = 0; //nextItemQuantity = 0;

                nextUserQuantity = oCommonController.fetchPlanQuantity(obj.CompanyId, "User");
                nextBranchQuantity = oCommonController.fetchPlanQuantity(obj.CompanyId, "Branch");
                nextDomainQuantity = oCommonController.fetchPlanQuantity(obj.CompanyId, "Domain");
                nextTaxSettingQuantity = oCommonController.fetchPlanQuantity(obj.CompanyId, "TaxSetting");
                //nextItemQuantity = oCommonController.fetchPlanQuantity(obj.CompanyId, "Item");

                int reducedQuantity = 0;
                if (currentUserQuantity > nextUserQuantity)
                {
                    reducedQuantity = currentUserQuantity - nextUserQuantity;
                    query = "update \"tblUser\" set \"IsActive\"=False where \"UserId\" in (select \"UserId\" from tblUser where \"UserType\"=\"user\" and \"IsCompany\" == False and \"CompanyId\"=" + obj.CompanyId + " order by \"UserId\" desc limit " + reducedQuantity + ")";
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                if (currentBranchQuantity > nextBranchQuantity)
                {
                    reducedQuantity = currentBranchQuantity - nextBranchQuantity;
                    query = "update \"tblBranch\" set \"IsActive\"=False where \"BranchId\" in (select \"BranchId\" from \"tblBranch\" where \"CompanyId\"=" + obj.CompanyId + " order by \"BranchId\" desc limit " + reducedQuantity + ")";
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                if (currentDomainQuantity > nextDomainQuantity)
                {
                    reducedQuantity = currentDomainQuantity - nextDomainQuantity;
                    query = "update \"tblDomain\" set \"IsActive\"=False where \"DomainId\" in (select \"DomainId\" from \"tblDomain\" where \"CompanyId\"=" + obj.CompanyId + " order by \"DomainId\" desc limit " + reducedQuantity + ")";
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                if (currentTaxSettingQuantity > nextTaxSettingQuantity)
                {
                    reducedQuantity = currentTaxSettingQuantity - nextTaxSettingQuantity;
                    query = "update \"tblTaxSetting\" set \"IsActive\"=False where \"TaxSettingId\" in (select \"TaxSettingId\" from \"tblTaxSetting\" where \"CompanyId\"=" + obj.CompanyId + " order by \"TaxSettingId\" desc limit " + reducedQuantity + ")";
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                //if (currentItemQuantity > nextItemQuantity)
                //{
                //    reducedQuantity = currentItemQuantity - nextItemQuantity;
                //    query = "update \"tblItem\" set \"IsActive\"=False where \"ItemId\" in (select top " + reducedQuantity + " \"ItemId\" from \"tblItem\" where \"CompanyId\"=" + obj.CompanyId + " order by \"ItemId\" desc)";
                //    oConnectionContext.Database.ExecuteSqlCommand(query);
                //}

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Plan Activation",
                    CompanyId = obj.CompanyId,
                    Description = "activated plan for " + obj.TransactionNo,
                    Id = obj.TransactionId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Plan activated successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InitPaymentGateway(ClsTransactionVm obj)
        {
            var Transaction = new ClsTransactionVm
            {
                TransactionId = oConnectionContext.DbClsTransaction.Where(a => a.TransactionNo == obj.TransactionNo).Select(a => a.TransactionId).FirstOrDefault(),
                PayableCost = oConnectionContext.DbClsTransaction.Where(a => a.TransactionNo == obj.TransactionNo).Select(a => a.PayableCost).FirstOrDefault(),
                Name = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.Name).FirstOrDefault(),
                EmailId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.EmailId).FirstOrDefault(),
                MobileNo = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.MobileNo).FirstOrDefault(),
            };

            data = new
            {
                Status = 1,
                Message = "",
                Data = new
                {
                    Transaction = Transaction
                }
            };

            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> InitPlanPrice(ClsTransactionVm obj)
        {
            string warningMsg = "";
            int branchAddonQuantity = 0, userAddonQuantity = 0, itemAddonQuantity = 0;//, orderAddonQuantity = 0;
            decimal conversionRate = 1;

            obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();

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

            decimal PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(a => a.CountryId == obj.CountryId && a.CompanyId == obj.Under).Select(a => a.PriceHikePercentage).FirstOrDefault();

            var TermLength = oConnectionContext.DbClsTermLength.Where(a => a.IsActive == true &&
            a.IsDeleted == false && a.TermLengthId == obj.TermLengthId).Select(a => new { a.MRP, a.SellingPrice, a.Months, a.DiscountPercentage }).FirstOrDefault();

            decimal addonsSubTotal = 0;
            foreach (var item in obj.TransactionDetails)
            {
                item.MRP = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                (((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate)).FirstOrDefault();

                //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                //(((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate) - ((TermLength.DiscountPercentage / 100) * (((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate))).FirstOrDefault();

                int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();
                if (pricingType == 2)
                {
                    item.SubTotal = item.Quantity * Math.Round(item.MRP, 2);
                    //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2);
                }
                else
                {
                    item.SubTotal = item.Quantity * Math.Round(item.MRP, 2) * TermLength.Months;
                    //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2) * TermLength.Months;
                }

                //item.PayableCost = item.SubTotal;
                addonsSubTotal = addonsSubTotal + item.SubTotal;

                if (item.Type == "User")
                {
                    userAddonQuantity = item.Quantity;
                }
                else if (item.Type == "Branch")
                {
                    branchAddonQuantity = item.Quantity;
                }
                else if (item.Type == "Item")
                {
                    itemAddonQuantity = item.Quantity;
                }
                //else if (item.Type == "Order")
                //{
                //    orderAddonQuantity = item.Quantity;
                //}
            }

            int previousQuantity = 0;
            ClsTransactionVm CurrentPlan = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).Where(a => a.CompanyId == obj.CompanyId &&
             a.StartDate != null && a.Status == 2).Select(a => new ClsTransactionVm
             {
                 TransactionId = a.TransactionId,
                 BaseBranch = a.BaseBranch,
                 BaseItem = a.BaseItem,
                 BaseOrder = a.BaseOrder,
                 BaseUser = a.BaseUser
             }).FirstOrDefault();

            if (CurrentPlan == null)
            {
                CurrentPlan = new ClsTransactionVm
                {
                    TransactionId = 0,
                    BaseBranch = 0,
                    BaseItem = 0,
                    BaseOrder = 0,
                    BaseUser = 0
                };
            }

            previousQuantity = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.UserType == "User").Count();
            if (previousQuantity > (userAddonQuantity + CurrentPlan.BaseUser))
            {
                warningMsg = warningMsg + "<br/>" + "You have more no of user in your software";
            }

            previousQuantity = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Count();
            if (previousQuantity > (branchAddonQuantity + CurrentPlan.BaseBranch))
            {
                warningMsg = warningMsg + "<br/>" + "You have more no of branch in your software";
            }

            previousQuantity = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false).Count();
            if (previousQuantity > (itemAddonQuantity + CurrentPlan.BaseItem))
            {
                warningMsg = warningMsg + "<br/>" + "You have more no of item in your software";
            }

            //previousQuantity = oCommonController.fetchPlanQuantity(obj.CompanyId, "Order");
            //if (previousQuantity > (orderAddonQuantity + CurrentPlan.BaseOrder))
            //{
            //    warningMsg = warningMsg + "<br/>" + "You have more no of item in your current plan";
            //}

            decimal planSubTotal2 = Math.Round((((TermLength.MRP * PriceHikePercentage) + TermLength.MRP) / conversionRate), 2) * TermLength.Months;
            obj.SubTotal = planSubTotal2 + addonsSubTotal;

            if (obj.CouponId != 0)
            {
                var coupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId).Select(a => new
                {
                    a.Discount,
                    a.DiscountType,
                    a.MinimumPurchaseAmount,
                    a.MaximumDiscountAmount,
                    a.ApplyToBasePlan,
                    a.ApplyToAddons
                }).FirstOrDefault();

                if (coupon != null)
                {
                    // Validate term length restrictions
                    var couponTermLengths = oConnectionContext.DbClsCouponTermLength
                        .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false).Select(a => a.TermLengthId).ToList();

                    // If coupon has term length restrictions, validate against selected term length
                    if (couponTermLengths.Count > 0 && !couponTermLengths.Contains(obj.TermLengthId))
                    {
                        // Coupon not valid for this term length - set discount to 0
                        obj.CouponDiscount = 0;
                    }
                    else
                    {
                        // Calculate coupon applicable subtotal based on settings
                        decimal couponSubTotal = 0;
                        
                        if (coupon.ApplyToBasePlan)
                        {
                            couponSubTotal += planSubTotal2;
                        }
                        
                        if (coupon.ApplyToAddons)
                        {
                            // Get allowed addon IDs for this coupon
                            var allowedAddonIds = oConnectionContext.DbClsCouponAddon
                                .Where(a => a.CouponId == obj.CouponId && a.IsDeleted == false)
                                .Select(a => a.PlanAddonsId).ToList();
                            
                            decimal applicableAddonsSubTotal = 0;
                            
                            if (allowedAddonIds.Count == 0)
                            {
                                // No specific addons selected = apply to all addons
                                applicableAddonsSubTotal = addonsSubTotal;
                            }
                            else
                            {
                                // Apply only to specific selected addons
                                foreach (var item in obj.TransactionDetails)
                                {
                                    if (item.PlanAddonsId > 0 && allowedAddonIds.Contains(item.PlanAddonsId))
                                    {
                                        applicableAddonsSubTotal += item.SubTotal;
                                    }
                                }
                            }
                            
                            couponSubTotal += applicableAddonsSubTotal;
                        }

                        // Calculate discount
                        decimal calculatedDiscount = 0;
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

                        obj.CouponDiscount = calculatedDiscount;
                    }
                }
            }

            obj.YearlyPlanDiscountPercentage = TermLength.DiscountPercentage;
            obj.YearlyPlanDiscount = ((TermLength.DiscountPercentage / 100) * obj.SubTotal);

            obj.PayableCost = obj.SubTotal - obj.CouponDiscount - obj.YearlyPlanDiscount;

            var Transaction = new ClsTransactionVm
            {
                PayableCost = obj.PayableCost,
                Name = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.Name).FirstOrDefault(),
                EmailId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.EmailId).FirstOrDefault(),
                MobileNo = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.MobileNo).FirstOrDefault(),
                CountryId = obj.CountryId,
                MainCountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.CountryId).FirstOrDefault(),
                BusinessLogo = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.BusinessLogo).FirstOrDefault(),
                BusinessName = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.BusinessName).FirstOrDefault(),
                WarningMsg = warningMsg,
                OnlinePaymentSettings = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.IsDeleted == false && a.IsActive == true
                    && a.CompanyId == obj.Under).Select(a => new ClsOnlinePaymentSettingsVm
                    {
                        OnlinePaymentSettingsId = a.OnlinePaymentSettingsId,
                        OnlinePaymentService = a.OnlinePaymentService,
                        RazorpayKey = a.RazorpayKey,
                        RazorpayCurrencyId = a.RazorpayCurrencyId,
                        PaypalClientId = a.PaypalClientId,
                        PaypalCurrencyId = a.PaypalCurrencyId,
                        IsDefault = a.IsDefault,
                        SaveAs = a.SaveAs,
                        PaypalCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.PaypalCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault(),
                        RazorpayCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.RazorpayCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault()
                    }).OrderBy(a => a.OnlinePaymentSettingsId).ToList()
            };

            data = new
            {
                Status = 1,
                Message = "",
                Data = new
                {
                    Transaction = Transaction
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InitAddonPrice(ClsTransactionVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            long ParentTransactionId = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == obj.CompanyId &&
              a.StartDate != null).Select(a => a.TransactionId).FirstOrDefault();

            int MonthsLeft = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == obj.CompanyId &&
          a.StartDate != null).Select(a => Math.Abs(12 * (CurrentDate.Year - a.ExpiryDate.Value.Year) + CurrentDate.Month - a.ExpiryDate.Value.Month)).FirstOrDefault();

            decimal conversionRate = 1;
            //obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();
            obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();
            if (obj.CountryId == oConnectionContext.DbClsUserCountryMap.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => a.CountryId).FirstOrDefault())
            {
                conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => a.ConversionRate).FirstOrDefault();
            }
            else
            {
                conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == 3).Select(a => a.ConversionRate).FirstOrDefault();
            }

            decimal PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(a => a.CountryId == obj.CountryId && a.CompanyId == obj.Under).Select(a => a.PriceHikePercentage).FirstOrDefault();

            decimal DiscountPercentage = oConnectionContext.DbClsTermLength.Where(a => a.TermLengthId ==
                (oConnectionContext.DbClsTransaction.Where(b => b.TransactionId == ParentTransactionId &&
                b.CompanyId == obj.CompanyId).Select(b => b.TermLengthId).FirstOrDefault())
                ).Select(a => a.DiscountPercentage).FirstOrDefault();

            decimal addonsSubTotal = 0;
            foreach (var item in obj.TransactionDetails)
            {
                item.MRP = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
               (((a.MRP * PriceHikePercentage) + a.MRP) / conversionRate)).FirstOrDefault();

                //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a =>
                //(((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate) - ((DiscountPercentage / 100) * (((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate))).FirstOrDefault();

                int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();
                if (pricingType == 2)
                {
                    item.SubTotal = item.Quantity * Math.Round(item.MRP, 2);
                    //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2);
                }
                else
                {
                    item.SubTotal = item.Quantity * Math.Round(item.MRP, 2) * MonthsLeft;
                    //item.PayableCost = item.Quantity * Math.Round(item.UnitCost, 2) * MonthsLeft;
                }
                //item.PayableCost = item.SubTotal;
                addonsSubTotal = addonsSubTotal + item.SubTotal;
            }

            obj.SubTotal = addonsSubTotal;

            if (obj.CouponId != 0)
            {
                var coupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId).Select(a => new
                {
                    a.Discount,
                    a.DiscountType
                }).FirstOrDefault();

                if (coupon != null)
                {
                    if (coupon.DiscountType == 1)
                    {
                        obj.CouponDiscount = coupon.Discount;
                    }
                    else
                    {
                        obj.CouponDiscount = (coupon.Discount / 100) * obj.SubTotal;
                    }
                }
            }

            obj.YearlyPlanDiscountPercentage = DiscountPercentage;
            obj.YearlyPlanDiscount = ((DiscountPercentage / 100) * obj.SubTotal);

            obj.PayableCost = obj.SubTotal - obj.CouponDiscount - obj.YearlyPlanDiscount;

            var Transaction = new ClsTransactionVm
            {
                PayableCost = obj.PayableCost,
                Name = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.Name).FirstOrDefault(),
                EmailId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.EmailId).FirstOrDefault(),
                MobileNo = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.MobileNo).FirstOrDefault(),
                CountryId = obj.CountryId,
                MainCountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.CountryId).FirstOrDefault(),
                BusinessLogo = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.BusinessLogo).FirstOrDefault(),
                BusinessName = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.BusinessName).FirstOrDefault(),
                OnlinePaymentSettings = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.IsDeleted == false && a.IsActive == true
                    && a.CompanyId == obj.Under).Select(a => new ClsOnlinePaymentSettingsVm
                    {
                        OnlinePaymentSettingsId = a.OnlinePaymentSettingsId,
                        OnlinePaymentService = a.OnlinePaymentService,
                        RazorpayKey = a.RazorpayKey,
                        RazorpayCurrencyId = a.RazorpayCurrencyId,
                        PaypalClientId = a.PaypalClientId,
                        PaypalCurrencyId = a.PaypalCurrencyId,
                        IsDefault = a.IsDefault,
                        SaveAs = a.SaveAs,
                        PaypalCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.PaypalCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault(),
                        RazorpayCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.RazorpayCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault()
                    }).OrderBy(a => a.OnlinePaymentSettingsId).ToList(),
            };

            data = new
            {
                Status = 1,
                Message = "",
                Data = new
                {
                    Transaction = Transaction
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PaymentSuccessAdmin(ClsTransactionVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //var Quantity = oConnectionContext.DbClsTransactionDetails.Where(a => a.TransactionId == obj.TransactionId
                //&& a.Type == "Base Plan").Select(a => a.Quantity).FirstOrDefault();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                obj.TransactionId = oConnectionContext.DbClsTransaction.Where(a => a.TransactionNo == obj.TransactionNo).Select(a => a.TransactionId).FirstOrDefault();

                ClsTransaction oClsTransaction = new ClsTransaction()
                {
                    TransactionId = obj.TransactionId,
                    PaidOn = CurrentDate,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    Status = 2,
                    LiveTransactionId = obj.LiveTransactionId,
                    PaymentGatewayType = obj.PaymentGatewayType,
                    PaymentMethodType = obj.PaymentMethodType
                };
                oConnectionContext.DbClsTransaction.Attach(oClsTransaction);
                oConnectionContext.Entry(oClsTransaction).Property(x => x.PaidOn).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.LiveTransactionId).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.PaymentGatewayType).IsModified = true;
                oConnectionContext.Entry(oClsTransaction).Property(x => x.PaymentMethodType).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Payment done successfully",
                    Data = new
                    {
                        Transaction = new ClsTransaction
                        {
                            TransactionNo = obj.TransactionNo
                        }
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AllTransactionsAdmin(ClsTransactionVm obj)
        {
            if (obj.Under == 0)
            {
                //obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
                obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();
            }

            if (obj.FromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.FromDate > DateTime.Now)
                {
                    obj.FromDate = obj.FromDate.AddYears(-1);
                }

                obj.ToDate = obj.FromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

                obj.ToDate = obj.ToDate.AddDays(days - 1);
            }

            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => a.UserType).FirstOrDefault();

            var det = (from a in oConnectionContext.DbClsTransaction
                       join b in oConnectionContext.DbClsUser
on a.CompanyId equals b.UserId
                       where b.Under == obj.Under && a.IsDeleted == false && a.IsTrial == false
                       select new
                       {
                           CurrencySymbol = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == a.CurrencyId).Select(c => c.CurrencySymbol).FirstOrDefault(),
                           CurrencySymbolPlacement = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == obj.Under).Select(bb => bb.CurrencySymbolPlacement).FirstOrDefault(),
                           b.ResellerId,
                           TransactionId = a.TransactionId,
                           a.PaidOn,
                           a.TransactionNo,
                           a.PayableCost,
                           a.Status,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           CompanyId = a.CompanyId,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           Name = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.Name).FirstOrDefault(),
                           MobileNo = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.MobileNo).FirstOrDefault(),
                           EmailId = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.EmailId).FirstOrDefault(),
                       }).ToList();

            if (UserType.ToLower() == "reseller")
            {
                det = det.Where(a => a.ResellerId == obj.AddedBy).ToList();
            }

            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            {
                det = det.Where(a => a.AddedOn != null && a.AddedOn.Date >= obj.FromDate && a.AddedOn.Date <= obj.ToDate).ToList();
            }

            if (obj.Status != 0)
            {
                det = det.Where(a => a.Status == obj.Status).ToList();
            }

            if (obj.TransactionNo != "" && obj.TransactionNo != null)
            {
                det = det.Where(a => a.TransactionNo.Contains(obj.TransactionNo)).ToList();
            }

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.TransactionNo.Contains(obj.Search)).ToList();
            }

            if (obj.UserId != 0)
            {
                det = det.Where(a => a.CompanyId == obj.UserId).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Transactions = det.OrderByDescending(a => a.TransactionId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    //ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    //InactiveCount = det.Where(a => a.IsActive == false).Count(),
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TransactionSummaryAdmin(ClsUserVm obj)
        {
            if (obj.Under == 0)
            {
                //obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
                obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();
            }

            string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => a.UserType).FirstOrDefault();

            var Transactions = (from a in oConnectionContext.DbClsTransaction
                                join b in oConnectionContext.DbClsUser on a.CompanyId equals b.UserId
                                where b.Under == obj.Under && a.IsDeleted == false && a.IsTrial == false
                                //&& a.StartDate != null 
                                && a.Status == 2
                                select new
                                {
                                    b.Under,
                                    b.ResellerId,
                                    //CommissionPercent = UserType.ToLower() == "reseller" ? a.ResellerCommissionPercent : a.WhitelabelCommissionPercent,
                                    ResellerCommission = (a.ResellerCommissionPercent / 100) * a.PayableCost,
                                    RootAccountCommission = (a.RootAccountCommissionPercent / 100) * a.PayableCost,
                                    WhitelabelCommission = (a.WhitelabelCommissionPercent / 100) * a.PayableCost,
                                    PayableCost = a.PayableCost,
                                    Month = a.AddedOn.Month,
                                    Year = a.AddedOn.Year,
                                    CurrencySymbol = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == a.CurrencyId).Select(c => c.CurrencySymbol).FirstOrDefault(),
                                    CurrencySymbolPlacement = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == obj.Under).Select(bb => bb.CurrencySymbolPlacement).FirstOrDefault(),
                                }).ToList();

            if (UserType.ToLower() == "reseller")
            {
                Transactions = Transactions.Where(a => a.ResellerId == obj.UserId).Select(a => a).ToList();
            }

            var _transactions = Transactions
            .GroupBy(l => new { l.Month, l.Year, l.CurrencySymbol })
            .Select(cl => new
            {
                Under = cl.FirstOrDefault().Under,
                //CommissionPercent = cl.FirstOrDefault().CommissionPercent,
                ResellerCommission = cl.Sum(c => c.ResellerCommission),
                RootAccountCommission = cl.Sum(c => c.RootAccountCommission),
                WhitelabelCommission = cl.Sum(c => c.WhitelabelCommission),
                Month = cl.FirstOrDefault().Month,
                Year = cl.FirstOrDefault().Year,
                TotalNoOfSales = cl.Count(),
                TotalSales = cl.Sum(c => c.PayableCost),
                CurrencySymbol = cl.FirstOrDefault().CurrencySymbol,
                CurrencySymbolPlacement = cl.FirstOrDefault().CurrencySymbolPlacement
            }).OrderByDescending(a => a.Year).ThenByDescending(a => a.Month).ToList();

            var finalTransactions = _transactions.Select(a => new
            {
                Under = a.Under,
                UserId = obj.UserId,
                //CommissionPercent = a.CommissionPercent,
                ResellerCommission = a.ResellerCommission,
                RootAccountCommission = a.RootAccountCommission,
                WhitelabelCommission = a.WhitelabelCommission,
                PayableCommission = UserType.ToLower() == "reseller" ? a.ResellerCommission : a.WhitelabelCommission,
                Month = a.Month,
                Year = a.Year,
                TotalNoOfSales = a.TotalNoOfSales,
                TotalSales = a.TotalSales,
                CurrencySymbol = a.CurrencySymbol,
                CurrencySymbolPlacement = a.CurrencySymbolPlacement,
                IsPaid = oConnectionContext.DbClsResellerPayment.Where(b => b.Month == a.Month && b.Year == a.Year
                && b.CompanyId == obj.UserId && b.IsActive == true && b.IsDeleted == false && b.Type == 2).Count() == 0 ? false : true
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "Successfull",
                Data = new
                {
                    Transactions = finalTransactions
                }
            };
            return await Task.FromResult(Ok(data));

        }

        public async Task<IHttpActionResult> TransactionByMonthAdmin(ClsUserVm obj)
        {
            if (obj.Under == 0)
            {
                //obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
                obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();
            }

            //if (obj.PageSize == 0)
            //{
            //    obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            //}

            //int skip = obj.PageSize * (obj.PageIndex - 1);

            string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => a.UserType).FirstOrDefault();

            var det = (from a in oConnectionContext.DbClsTransaction
                       join b in oConnectionContext.DbClsUser on a.CompanyId equals b.UserId
                       where b.Under == obj.Under && a.AddedOn.Month == obj.Month
                       && a.AddedOn.Year == obj.Year && a.IsDeleted == false && a.IsTrial == false
                       //&& a.StartDate != null 
                       && a.Status == 2
                       select new
                       {
                           a.ResellerCommissionPercent,
                           a.RootAccountCommissionPercent,
                           a.WhitelabelCommissionPercent,
                           ResellerCommission = (a.ResellerCommissionPercent / 100) * a.PayableCost,
                           RootAccountCommission = (a.RootAccountCommissionPercent / 100) * a.PayableCost,
                           WhitelabelCommission = (a.WhitelabelCommissionPercent / 100) * a.PayableCost,
                           CurrencySymbol = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == a.CurrencyId).Select(c => c.CurrencySymbol).FirstOrDefault(),
                           CurrencySymbolPlacement = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == obj.Under).Select(bb => bb.CurrencySymbolPlacement).FirstOrDefault(),
                           b.ResellerId,
                           TransactionId = a.TransactionId,
                           a.PaidOn,
                           a.TransactionNo,
                           a.PayableCost,
                           a.Status,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           CompanyId = a.CompanyId,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           Name = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.Name).FirstOrDefault(),
                           MobileNo = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.MobileNo).FirstOrDefault(),
                           EmailId = oConnectionContext.DbClsUser.Where(z => z.UserId == a.CompanyId).Select(z => z.EmailId).FirstOrDefault(),
                       }).OrderByDescending(a => a.TransactionId).ToList();

            if (UserType.ToLower() == "reseller")
            {
                det = det.Where(a => a.ResellerId == obj.UserId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Transactions = det,//.OrderByDescending(a => a.TransactionId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    //ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    //InactiveCount = det.Where(a => a.IsActive == false).Count(),
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CheckMaxSpecialDiscount(ClsTransactionVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //if (obj.Under == 0)
                //{
                //    obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsCancelled == false && a.IsCancelled == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a => a.Under).FirstOrDefault();
                //}

                ClsTransactionVm det = oConnectionContext.DbClsTransaction.Where(a => a.TransactionNo == obj.TransactionNo).Select(a => new ClsTransactionVm
                {
                    ParentTransactionId = a.ParentTransactionId,
                    CompanyId = a.CompanyId,
                    CurrencySymbol = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == oConnectionContext.DbClsUserCurrencyMap.Where(x => x.CompanyId == a.CompanyId).Select(x => x.CurrencyId).FirstOrDefault()).Select(c => c.CurrencySymbol).FirstOrDefault(),
                    TransactionId = a.TransactionId,
                    SubTotal = a.SubTotal,
                    CouponDiscount = a.CouponDiscount,
                    PayableCost = a.PayableCost,
                    TransactionDetails = oConnectionContext.DbClsTransactionDetails.Where(b =>
                    b.TransactionId == a.TransactionId).Select(b => new ClsTransactionDetailsVm
                    {
                        //Discount = b.Discount,
                        PlanAddonsId = b.PlanAddonsId,
                        Quantity = b.Quantity,
                        SubTotal = b.SubTotal,
                        //PayableCost = b.PayableCost,
                        //UnitCost = b.UnitCost,
                        TransactionDetailsId = b.TransactionDetailsId
                    }).ToList(),
                }).FirstOrDefault();

                //      if (obj.Under == 0)
                //      {
                //          obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == det.CompanyId).Select(a => a.Under).FirstOrDefault();
                //      }

                //      decimal conversionRate = 1;

                //      obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => a.CountryId).FirstOrDefault();
                //      if (obj.CountryId == oConnectionContext.DbClsUserCountryMap.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => a.CountryId).FirstOrDefault())
                //      {
                //          conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => a.ConversionRate).FirstOrDefault();
                //      }
                //      else
                //      {
                //          conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == 3).Select(a => a.ConversionRate).FirstOrDefault();
                //      }

                //      //decimal PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(a => a.CountryId == obj.CountryId && a.CompanyId == obj.Under).Select(a => a.PriceHikePercentage).FirstOrDefault();

                //      decimal PriceHikePercentage = 0;

                //      var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                //      int Months = 0;

                //      if (det.ParentTransactionId == 0)
                //      {
                //          Months = oConnectionContext.DbClsTransactionDetails.Where(b => b.TransactionId == det.TransactionId).Select(b => b.Quantity).FirstOrDefault();
                //      }
                //      else
                //      {
                //          Months = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == det.CompanyId &&
                //a.StartDate != null).Select(a => Math.Abs(12 * (CurrentDate.Year - a.ExpiryDate.Value.Year) + CurrentDate.Month - a.ExpiryDate.Value.Month) + 1).FirstOrDefault();

                //      }

                //      decimal addonsPrice = 0;
                //      foreach (var item in det.TransactionDetails)
                //      {
                //          if (item.PlanAddonsId == 0)
                //          {
                //              decimal SellingPrice = oConnectionContext.DbClsTermLength.Where(b => b.Months == Months).Select(b => b.SellingPrice).FirstOrDefault();

                //              item.SubTotal = addonsPrice + (Math.Round((((SellingPrice * PriceHikePercentage) + SellingPrice) / conversionRate), 2) * Months);
                //          }
                //          else
                //          {
                //              //item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => ((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate).FirstOrDefault();
                //              int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();
                //              if (pricingType == 2)
                //              {
                //                  item.SubTotal = item.Quantity * Math.Round(item.MRP, 2);
                //              }
                //              else
                //              {
                //                  item.SubTotal = item.Quantity * Math.Round(item.MRP, 2) * Months;
                //              }

                //              //item.TotalCost = item.SubTotal;

                //          }
                //          addonsPrice = addonsPrice + item.SubTotal;
                //      }

                //      obj.SubTotal = addonsPrice;

                //      if (obj.CouponId != 0)
                //      {
                //          var coupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId).Select(a => new
                //          {
                //              a.Discount,
                //              a.DiscountType
                //          }).FirstOrDefault();

                //          if (coupon != null)
                //          {
                //              if (coupon.DiscountType == 1)
                //              {
                //                  obj.CouponDiscount = coupon.Discount;
                //              }
                //              else
                //              {
                //                  obj.CouponDiscount = (coupon.Discount / 100) * obj.SubTotal;
                //              }
                //          }
                //      }

                //      obj.PayableCost = obj.SubTotal - obj.CouponDiscount;

                //      decimal MaxSpecialDiscount = det.SubTotal - obj.PayableCost;

                decimal MaxSpecialDiscount = (50m / 100m) * det.SubTotal;

                data = new
                {
                    Status = 1,
                    Message = "",
                    Data = new
                    {
                        Transaction = new
                        {
                            MinPrice = obj.PayableCost,
                            PayableCost = det.PayableCost,
                            SpecialDiscount = MaxSpecialDiscount,
                            CurrencySymbol = det.CurrencySymbol
                        }
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));

        }

        public async Task<IHttpActionResult> UpdateSpecialDiscount(ClsTransactionVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                ClsTransactionVm det = oConnectionContext.DbClsTransaction.Where(a => a.TransactionNo == obj.TransactionNo).Select(a => new ClsTransactionVm
                {
                    ParentTransactionId = a.ParentTransactionId,
                    CompanyId = a.CompanyId,
                    CurrencySymbol = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == oConnectionContext.DbClsUserCurrencyMap.Where(x => x.CompanyId == a.CompanyId).Select(x => x.CurrencyId).FirstOrDefault()).Select(c => c.CurrencySymbol).FirstOrDefault(),
                    TransactionId = a.TransactionId,
                    SubTotal = a.SubTotal,
                    CouponDiscount = a.CouponDiscount,
                    YearlyPlanDiscount = a.YearlyPlanDiscount,
                    PayableCost = a.PayableCost,
                    TransactionDetails = oConnectionContext.DbClsTransactionDetails.Where(b =>
                    b.TransactionId == a.TransactionId).Select(b => new ClsTransactionDetailsVm
                    {
                        //Discount = b.Discount,
                        PlanAddonsId = b.PlanAddonsId,
                        Quantity = b.Quantity,
                        SubTotal = b.SubTotal,
                        //PayableCost = b.PayableCost,
                        //UnitCost = b.UnitCost,
                        TransactionDetailsId = b.TransactionDetailsId
                    }).ToList(),
                }).FirstOrDefault();

                //      if (obj.Under == 0)
                //      {
                //          obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == det.CompanyId).Select(a => a.Under).FirstOrDefault();
                //      }

                //      decimal conversionRate = 1;

                //      obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => a.CountryId).FirstOrDefault();
                //      if (obj.CountryId == oConnectionContext.DbClsUserCountryMap.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => a.CountryId).FirstOrDefault())
                //      {
                //          conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == obj.CountryId).Select(a => a.ConversionRate).FirstOrDefault();
                //      }
                //      else
                //      {
                //          conversionRate = oConnectionContext.DbClsCountry.Where(a => a.CountryId == 3).Select(a => a.ConversionRate).FirstOrDefault();
                //      }

                //      //decimal PriceHikePercentage = oConnectionContext.DbClsUserCountryMap.Where(a => a.CountryId == obj.CountryId && a.CompanyId == obj.Under).Select(a => a.PriceHikePercentage).FirstOrDefault();

                //      decimal PriceHikePercentage = 0;

                //      var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                //      int Months = 0;

                //      if (det.ParentTransactionId == 0)
                //      {
                //          Months = oConnectionContext.DbClsTransactionDetails.Where(b => b.TransactionId == det.TransactionId).Select(b => b.Quantity).FirstOrDefault();
                //      }
                //      else
                //      {
                //          Months = oConnectionContext.DbClsTransaction.OrderByDescending(a => a.TransactionId).AsEnumerable().Where(a => a.CompanyId == det.CompanyId &&
                //a.StartDate != null).Select(a => Math.Abs(12 * (CurrentDate.Year - a.ExpiryDate.Value.Year) + CurrentDate.Month - a.ExpiryDate.Value.Month) + 1).FirstOrDefault();

                //      }

                //      decimal addonsPrice = 0;
                //      foreach (var item in det.TransactionDetails)
                //      {
                //          if (item.PlanAddonsId == 0)
                //          {
                //              decimal SellingPrice = oConnectionContext.DbClsTermLength.Where(b => b.Months == Months).Select(b => b.SellingPrice).FirstOrDefault();

                //              item.SubTotal = addonsPrice + (Math.Round((((SellingPrice * PriceHikePercentage) + SellingPrice) / conversionRate), 2) * Months);
                //          }
                //          else
                //          {
                //              item.UnitCost = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => ((a.SellingPrice * PriceHikePercentage) + a.SellingPrice) / conversionRate).FirstOrDefault();
                //              int pricingType = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.PricingType).FirstOrDefault();
                //              if (pricingType == 2)
                //              {
                //                  item.SubTotal = item.Quantity * Math.Round(item.UnitCost, 2);
                //              }
                //              else
                //              {
                //                  item.SubTotal = item.Quantity * Math.Round(item.UnitCost, 2) * Months;
                //              }

                //              //item.TotalCost = item.SubTotal;

                //          }
                //          addonsPrice = addonsPrice + item.SubTotal;
                //      }

                //      obj.SubTotal = addonsPrice;

                //      if (obj.CouponId != 0)
                //      {
                //          var coupon = oConnectionContext.DbClsCoupon.Where(a => a.CouponId == obj.CouponId).Select(a => new
                //          {
                //              a.Discount,
                //              a.DiscountType
                //          }).FirstOrDefault();

                //          if (coupon != null)
                //          {
                //              if (coupon.DiscountType == 1)
                //              {
                //                  obj.CouponDiscount = coupon.Discount;
                //              }
                //              else
                //              {
                //                  obj.CouponDiscount = (coupon.Discount / 100) * obj.SubTotal;
                //              }
                //          }
                //      }

                //      obj.PayableCost = obj.SubTotal - obj.CouponDiscount;

                //      decimal MaxSpecialDiscount = det.SubTotal - obj.PayableCost;

                decimal MaxSpecialDiscount = (50m / 100m) * det.SubTotal;

                long RootId = oConnectionContext.DbClsUser.Where(a => a.IsRootAccount == true).Select(a => a.UserId).FirstOrDefault();
                if (RootId != obj.AddedBy)
                {
                    if (obj.SpecialDiscount > MaxSpecialDiscount)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Maximum discount " + MaxSpecialDiscount + " is allowed",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                string query = "update \"tblTransaction\" set \"SpecialDiscount\"=" + obj.SpecialDiscount + ",\"PayableCost\"=(" + (det.SubTotal - (det.CouponDiscount + det.YearlyPlanDiscount + obj.SpecialDiscount)) + ") where \"TransactionId\"=" + det.TransactionId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                data = new
                {
                    Status = 1,
                    Message = "Special discount added successfully",
                    Data = new
                    {

                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));

        }

        private decimal CalculateNominalSubTotal(ClsTransactionVm obj, decimal conversionRate, decimal TermLengthMonths)
        {
            decimal nominalAddonsSubTotal = 0;
            
            foreach (var item in obj.TransactionDetails)
            {
                if (item.PlanAddonsId > 0)
                {
                    decimal nominalAddonMRP = oConnectionContext.DbClsPlanAddons
                        .Where(a => a.PlanAddonsId == item.PlanAddonsId)
                        .Select(a => a.MRP / conversionRate).FirstOrDefault();
                    
                    int pricingType = oConnectionContext.DbClsPlanAddons
                        .Where(a => a.PlanAddonsId == item.PlanAddonsId)
                        .Select(a => a.PricingType).FirstOrDefault();
                    
                    if (pricingType == 2)
                    {
                        nominalAddonsSubTotal += item.Quantity * Math.Round(nominalAddonMRP, 2);
                    }
                    else
                    {
                        nominalAddonsSubTotal += item.Quantity * Math.Round(nominalAddonMRP, 2) * TermLengthMonths;
                    }
                }
            }
            
            return nominalAddonsSubTotal;
        }

    }
}
