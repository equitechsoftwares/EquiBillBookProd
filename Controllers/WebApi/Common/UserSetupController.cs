using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi.Common
{
    public class UserSetupController : ApiController
    {
        ConnectionContext oConnectionContext = new ConnectionContext();
        public void PrefixSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            var PrefixUserMaps = oConnectionContext.DbClsPrefixMaster.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a => new
            {
                a.PrefixMasterId,
                a.NoOfDigits,
                a.Prefix
            }).ToList();

            ClsPrefix oPrefix = new ClsPrefix()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                IsActive = true,
                IsDeleted = false,
                CompanyId = obj.UserId,
                PrefixName = "Default",
            };

            oConnectionContext.DbClsPrefix.Add(oPrefix);
            oConnectionContext.SaveChanges();

            foreach (var item in PrefixUserMaps)
            {
                ClsPrefixUserMap oClsPrefixUserMap = new ClsPrefixUserMap()
                {
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    IsActive = true,
                    IsDeleted = false,
                    CompanyId = obj.UserId,
                    Counter = 1,
                    PrefixMasterId = item.PrefixMasterId,
                    NoOfDigits = item.NoOfDigits,
                    Prefix = item.Prefix,
                    PrefixId = oPrefix.PrefixId
                };
                oConnectionContext.DbClsPrefixUserMap.Add(oClsPrefixUserMap);
                oConnectionContext.SaveChanges();
            }
        }

        public void BranchSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            long PrefixId = oConnectionContext.DbClsPrefix.Where(a => a.CompanyId == obj.UserId).Select(a => a.PrefixId).FirstOrDefault();

            ClsBranch oClsBranch = new ClsBranch()
            {
                Branch = "Main",
                BranchCode = "Main",
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                IsMain = true,
                CountryId = obj.CountryId,
                StateId = obj.StateId,
                PrefixId = PrefixId
            };
            oConnectionContext.DbClsBranch.Add(oClsBranch);
            oConnectionContext.SaveChanges();

            // Auto-initialize Restaurant Settings for main branch
            ClsRestaurantSettings defaultRestaurantSettings = new ClsRestaurantSettings()
            {
                EnableKitchenDisplay = true,
                AutoPrintKot = true,
                EnableTableBooking = true,
                EnableRecurringBooking = false,
                BookingAdvanceDays = 30,
                DefaultBookingDuration = 120,
                RequireDeposit = false,
                DepositMode = "Fixed",
                DepositFixedAmount = 0,
                DepositPerGuestAmount = 0,
                EnablePublicBooking = false,
                PublicBookingSlug = null,
                PublicBookingAdvanceDays = 30,
                PublicBookingRequireDeposit = false,
                PublicBookingDepositPercentage = 0,
                PublicBookingDepositMode = "Fixed",
                PublicBookingDepositFixedAmount = 0,
                PublicBookingDepositPerGuestAmount = 0,
                PublicBookingAutoConfirm = false,
                EnablePublicBookingCancellation = true,
                AllowCancelAfterConfirm = true,
                PublicBookingCancellationDaysBefore = 0,
                PublicBookingCancellationChargeMode = "None",
                PublicBookingCancellationFixedCharge = 0,
                PublicBookingCancellationPercentage = 0,
                PublicBookingCancellationPerGuestCharge = 0,
                BranchId = oClsBranch.BranchId,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                ModifiedBy = obj.UserId
            };
            oConnectionContext.DbClsRestaurantSettings.Add(defaultRestaurantSettings);
            oConnectionContext.SaveChanges();
            // Auto-initialize Restaurant Settings for main branch

            ClsUserBranchMap oClsUserBranchMap = new ClsUserBranchMap()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                BranchId = oClsBranch.BranchId,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                UserType = "User",
                ModifiedBy = obj.UserId,
                UserId = obj.UserId
            };
            //ConnectionContext ocon = new ConnectionContext();
            oConnectionContext.DbClsUserBranchMap.Add(oClsUserBranchMap);
            oConnectionContext.SaveChanges();
        }

        public void BusinessSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsBusinessSettings oClsBusinessSettings = new ClsBusinessSettings()
            {
                StartDate = CurrentDate,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                FinancialYearStartMonth = 4,
                TransactionEditDays = 30,
                DateFormat = "dd/MM/yyyy",
                TimeFormat = "hh:mm tt",
                CompanyId = obj.UserId,
                //EnableInlineTax = true,
                IsActive = true,
                IsDeleted = false,
                TimeZoneId = obj.TimeZoneId,
                CurrencySymbolPlacement = 1,
                SetupStatus = 0,
                //ExpiryDate = DateTime.Now.AddDays(30),
                //PlanId = 0,
                CountryId = obj.CountryId,
                DatatablePageEntries = 10,
                ShowHelpText = true,
                EnableSound = true,
                CompetitorId = obj.CompetitorId,
                FreeTrialDays = 7,
                CollapseSidebar = true,
                IsBusinessRegistered = obj.IsBusinessRegistered,
                BusinessRegistrationType = obj.BusinessRegistrationType
            };
            oConnectionContext.DbClsBusinessSettings.Add(oClsBusinessSettings);
            oConnectionContext.SaveChanges();
        }

        public void ItemSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsItemSettings oClsItemSettings = new ClsItemSettings()
            {
                AddedOn = CurrentDate,
                AddedBy = obj.UserId,
                EnableItemExpiry = false,
                ExpiryType = 0,
                EnableBrands = false,
                EnableCategory = true,
                EnableSubCategory = false,
                EnablePosition = false,
                EnableRacks = false,
                EnableRow = false,
                EnableSubSubCategory = false,
                EnableSecondaryUnit = false,
                EnableTertiaryUnit = false,
                EnableQuaternaryUnit = false,
                EnableTax_PriceInfo = true,
                EnableWarranty = false,
                CompanyId = obj.UserId,
                IsDeleted = false,
                IsActive = true,
                DefaultProfitPercent = 0,
                StockAccountingMethod = 1,
                EnablePrintLabel = false,
                EnableStockAdjustment = false,
                EnableStockTransfer = false,
                EnableSellingPriceGroup = false,
                TaxType = "Exclusive",
                EnableSalt = false,
                EnableItemImage = false,
            };
            oConnectionContext.DbClsItemSettings.Add(oClsItemSettings);
            oConnectionContext.SaveChanges();
        }

        public void CurrencySetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsUserCurrencyMap oClsUserCurrencyMap = new ClsUserCurrencyMap()
            {
                AddedOn = CurrentDate,
                AddedBy = obj.UserId,
                CompanyId = obj.UserId,
                IsDeleted = false,
                IsActive = true,
                CurrencyId = obj.CurrencyId,
                IsMain = true
            };
            oConnectionContext.DbClsUserCurrencyMap.Add(oClsUserCurrencyMap);
            oConnectionContext.SaveChanges();
        }

        public void SaleSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsSaleSettings oClsSaleSettings = new ClsSaleSettings()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                IsActive = true,
                IsDeleted = false,
                AllowOverSelling = false,
                CompanyId = obj.UserId,
                DefaultSaleDiscount = 0,
                IsCommissionAgentRequired = false,
                IsPayTermRequired = false,
                SalePriceIsMinSellingPrice = false,
                SalesCommissionAgent = 1,
                CommissionCalculationType = 1,
                EnableFreeQuantity = false,
                InvoiceType = 1,
                EnableCustomerGroup = false,
                EnableSalesOrder = false,
                EnablePos = false,
                EnableSpecialDiscount = false,
                EnableNotes = false,
                EnableTerms = false
            };
            oConnectionContext.DbClsSaleSettings.Add(oClsSaleSettings);
            oConnectionContext.SaveChanges();
        }

        public void PosSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsPosSettings oClsPosSettings = new ClsPosSettings()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                DisableDicount = false,
                DisableDraft = false,
                DisableExpressCheckout = false,
                DisableMultiplePay = false,
                DisableOrderTax = false,
                DontShowProductSuggestion = false,
                DontShowRecentTransactions = false,
                EnableServiceStaff = false,
                EnableTransactionDate = false,
                EnableWeighingScale = false,
                IsServiceStaffRequired = false,
                PrintInvoiceOnHold = false,
                ProductSkuLength = 0,
                QuantityFractionalPartLength = 0,
                QuantityIntegerPartLength = 0,
                AllowOnlinePayment = false,
                ShowInvoiceLayoutDropdown = false,
                ShowPricingOnProductSuggestionTooltip = false,
                ShowInvoiceScheme = false,
                SubTotalEditable = false,
                WeighingScaleBarcodePrefix = "",
                DisableCreditSale = false,
                DisableHold = false,
                DisableProforma = false,
                DisableQuotation = false,
                EnableFreeQuantity = false,
                Draft = "shift+a",
                Quotation = "shift+q",
                Proforma = "shift+c",
                CreditSale = "shift+d",
                EditShippingCharge = "shift+e",
                EditPackagingCharge = "shift+f",
                EditDiscount = "shift+g",
                Multiple = "shift+h",
                //EditOrderTax = "shift+i",
                EditSpecialDiscount = "shift+i",
                Hold = "shift+j",
                AddPaymentRow = "shift+k",
                Cancel = "shift+l",
                FinalisePayment = "shift+m",
                GoToProductQuantity = "f2",
                AddNewProduct = "f4",
                WeighingScale = "shift+x",
                RecentTransactions = "shift+r",
                HoldList = "alt+h",
                Calculator = "alt+c",
                FullScreen = "f11",
                RegisterDetails = "alt+r",
                PosExit = "alt+e",
                InvoiceType = 1,
                EnablePlaceOfSupply = false,
                ShowItemImage = false,
                ShowItemSellingPrice = false,
                ShowItemMrp = false
            };
            oConnectionContext.DbClsPosSettings.Add(oClsPosSettings);
            oConnectionContext.SaveChanges();
        }

        public void PurchaseSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsPurchaseSettings oClsPurchaseSettings = new ClsPurchaseSettings()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                EnableEditingProductPrice = true,
                //EnableLotNo = true,
                EnablePurchaseOrder = false,
                EnablePurchaseStatus = true,
                EnableFreeQuantity = false,
                EnablePurchaseQuotation = false,
                EnableSpecialDiscount = false,
            };

            oConnectionContext.DbClsPurchaseSettings.Add(oClsPurchaseSettings);
            oConnectionContext.SaveChanges();
        }

        public void RewardPointSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsRewardPointSettings oClsRewardPointSettings = new ClsRewardPointSettings()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                AmountSpentForUnitPoint = 0,
                DisplayName = "",
                EnableRewardPoint = true,
                ExpiryPeriod = 0,
                ExpiryPeriodType = 1,
                MaximumRedeemPointPerOrder = 0,
                MaxPointsPerOrder = 0,
                MinimumOrderTotalToRedeemPoints = 0,
                MinimumRedeemPoint = 0,
                MinOrderTotalToEarnReward = 0,
                RedeemAmountPerUnitPoint = 0,
            };
            oConnectionContext.DbClsRewardPointSettings.Add(oClsRewardPointSettings);
            oConnectionContext.SaveChanges();
        }

        public void ShortCutKeysSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            List<ClsShortCutKeySetting> shortcutKeys = new List<ClsShortCutKeySetting>
            {
                //new ClsShortCutKeySetting { MenuId = 0, Title = "Add New Form", ShortCutKey= "shift+n" },
                new ClsShortCutKeySetting { MenuId = 0, Title = "Save", ShortCutKey= "shift+s" },
                new ClsShortCutKeySetting { MenuId = 0, Title = "Save & Add Another", ShortCutKey= "shift+a" },
                new ClsShortCutKeySetting { MenuId = 0, Title = "Update", ShortCutKey= "shift+u" },
                new ClsShortCutKeySetting { MenuId = 0, Title = "Update & Add Another", ShortCutKey= "shift+g" },
                new ClsShortCutKeySetting { MenuId = 0, Title = "Go Back", ShortCutKey= "shift+b" },
                new ClsShortCutKeySetting { MenuId = 153, Title = "User Role Add", ShortCutKey= "alt+r" },
                new ClsShortCutKeySetting { MenuId = 154, Title = "User Add", ShortCutKey= "shift+r" },
                new ClsShortCutKeySetting { MenuId = 155, Title = "Customer Add", ShortCutKey= "alt+c" },
                new ClsShortCutKeySetting { MenuId = 170, Title = "Category Add", ShortCutKey= "shift+c" },
                new ClsShortCutKeySetting { MenuId = 167, Title = "Unit Add", ShortCutKey= "alt+u" },
                new ClsShortCutKeySetting { MenuId = 160, Title = "Item Add", ShortCutKey= "alt+i" },
                new ClsShortCutKeySetting { MenuId = 174, Title = "Sales Add", ShortCutKey= "alt+s" },
            };

            foreach (var item in shortcutKeys)
            {
                ClsShortCutKeySetting oClsShortCutKeySetting = new ClsShortCutKeySetting()
                {
                    MenuId = item.MenuId,
                    Title = item.Title,
                    ShortCutKey = item.ShortCutKey,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                    IsActive = true,
                    IsDeleted = false,
                };
                oConnectionContext.DbClsShortCutKeySetting.Add(oClsShortCutKeySetting);
                oConnectionContext.SaveChanges();
            }
        }

        public void AddressSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsAddress oClsAddress = new ClsAddress()
            {
                AddedBy = obj.AddedBy,
                AddedOn = CurrentDate,
                Address = "",
                CityId = 0,
                CountryId = obj.CountryId,
                EmailId = obj.EmailId,
                IsActive = true,
                IsDeleted = false,
                Landmark = "",
                Latitude = "",
                Locality = "",
                Longitude = "",
                MobileNo = obj.MobileNo,
                MobileNo2 = "",
                Name = obj.Name,
                StateId = 0,
                Type = "",
                UserId = obj.UserId,
                Zipcode = 0
            };
            oConnectionContext.DbClsAddress.Add(oClsAddress);
            oConnectionContext.SaveChanges();

            oConnectionContext.DbClsAddress.Add(oClsAddress);
            oConnectionContext.SaveChanges();

        }

        public void TrialTransactionSetup(ClsUserVm obj, DateTime CurrentDate, int FreeTrialDays)
        {
            int User = oConnectionContext.DbClsPlan.Where(a => a.Type == "User").Select(a => a.Quantity).FirstOrDefault();
            int Branch = oConnectionContext.DbClsPlan.Where(a => a.Type == "Branch").Select(a => a.Quantity).FirstOrDefault();
            int Item = oConnectionContext.DbClsPlan.Where(a => a.Type == "Item").Select(a => a.Quantity).FirstOrDefault();
            int Order = oConnectionContext.DbClsPlan.Where(a => a.Type == "Order").Select(a => a.Quantity).FirstOrDefault();
            int PurchaseBill = oConnectionContext.DbClsPlan.Where(a => a.Type == "Bill").Select(a => a.Quantity).FirstOrDefault();
            int TaxSetting = oConnectionContext.DbClsPlan.Where(a => a.Type == "Tax Setting").Select(a => a.Quantity).FirstOrDefault();

            ClsTransaction oClsTransaction = new ClsTransaction()
            {
                TransactionNo = DateTime.Now.ToFileTime().ToString(),
                ParentTransactionId = 0,
                CouponDiscount = 0,
                PayableCost = 0,
                PlanId = oConnectionContext.DbClsPlan.Select(a => a.PlanId).FirstOrDefault(),
                Status = 2,
                SubTotal = 0,
                TermLengthId = 1,
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                BaseBranch = Branch,
                BaseOrder = Order,
                BaseItem = Item,
                BaseUser = User,
                BaseBill = PurchaseBill,
                BaseTaxSetting = TaxSetting,
                IsTrial = true,
                ExpiryDate = DateTime.Now.AddDays(FreeTrialDays),
                StartDate = CurrentDate,
            };
            oConnectionContext.DbClsTransaction.Add(oClsTransaction);
            oConnectionContext.SaveChanges();

            ClsTransactionDetails oClsTransactionDetails = new ClsTransactionDetails()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                //Discount = 0,
                IsActive = true,
                IsDeleted = false,
                PlanAddonsId = 0,
                Quantity = 1,
                SubTotal = 0,
                //PayableCost = 0,
                TransactionId = oClsTransaction.TransactionId,
                //UnitCost = 0,
                Type = "Base Plan"
            };
            oConnectionContext.DbClsTransactionDetails.Add(oClsTransactionDetails);
            oConnectionContext.SaveChanges();

            var PlanAddons = oConnectionContext.DbClsPlanAddons.Where(a => a.IsDeleted == false && a.IsActive == true && a.IsTrial == true).Select(a => new
            {
                PlanAddonsId = a.PlanAddonsId,
                a.Title,
                a.Description,
                a.SellingPrice,
                a.MRP,
                a.DiscountPercentage,
                a.OrderNo,
                a.IsCheckbox,
                a.Type,
            }).OrderBy(a => a.OrderNo).ToList();

            foreach (var item in PlanAddons)
            {
                ClsTransactionDetails oClsTransactionDetails1 = new ClsTransactionDetails()
                {
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                    //Discount = 0,
                    IsActive = true,
                    IsDeleted = false,
                    PlanAddonsId = item.PlanAddonsId,
                    Quantity = 1,
                    SubTotal = 0,
                    TransactionId = oClsTransaction.TransactionId,
                    //UnitCost = 0,
                    Type = oConnectionContext.DbClsPlanAddons.Where(a => a.PlanAddonsId == item.PlanAddonsId).Select(a => a.Type).DefaultIfEmpty().FirstOrDefault()
                };
                oConnectionContext.DbClsTransactionDetails.Add(oClsTransactionDetails1);
                oConnectionContext.SaveChanges();
            }
        }

        public void PaymentTypeSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsPaymentType oPaymentType5 = new ClsPaymentType()
            {
                PaymentType = "Payment Gateway",
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                //IsPosShown = true,
                ShortCutKey = "shift+g",
                IsAdvance = false,
                IsOnlyView = true,
                IsPaymentGateway = true
            };
            oConnectionContext.DbClsPaymentType.Add(oPaymentType5);
            oConnectionContext.SaveChanges();

            ClsPaymentType oPaymentType2 = new ClsPaymentType()
            {
                PaymentType = "Advance",
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                //IsPosShown = true,
                ShortCutKey = "shift+p",
                IsAdvance = true,
                IsOnlyView = true,
                IsPaymentGateway = false
            };
            oConnectionContext.DbClsPaymentType.Add(oPaymentType2);
            oConnectionContext.SaveChanges();

            ClsPaymentType oPaymentType = new ClsPaymentType()
            {
                PaymentType = "Cash",
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                //IsPosShown = true,
                ShortCutKey = "shift+o",
                IsAdvance = false,
                IsOnlyView = true,
                IsPaymentGateway = false
            };
            oConnectionContext.DbClsPaymentType.Add(oPaymentType);
            oConnectionContext.SaveChanges();

            ClsPaymentType oPaymentType1 = new ClsPaymentType()
            {
                PaymentType = "Card",
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                //IsPosShown = true,
                ShortCutKey = "shift+u",
                IsAdvance = false,
                IsOnlyView = true,
                IsPaymentGateway = false
            };
            oConnectionContext.DbClsPaymentType.Add(oPaymentType1);
            oConnectionContext.SaveChanges();

            ClsPaymentType oPaymentType3 = new ClsPaymentType()
            {
                PaymentType = "Cheque",
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                //IsPosShown = true,
                ShortCutKey = "shift+v",
                IsAdvance = false,
                IsOnlyView = true,
                IsPaymentGateway = false
            };
            oConnectionContext.DbClsPaymentType.Add(oPaymentType3);
            oConnectionContext.SaveChanges();

            ClsPaymentType oPaymentType4 = new ClsPaymentType()
            {
                PaymentType = "Bank Transfer",
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                //IsPosShown = true,
                ShortCutKey = "shift+w",
                IsAdvance = false,
                IsOnlyView = true,
                IsPaymentGateway = false
            };
            oConnectionContext.DbClsPaymentType.Add(oPaymentType4);
            oConnectionContext.SaveChanges();

            //map advance payment with main branch
            long BranchId = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.UserId).Select(a => a.BranchId).FirstOrDefault();

            ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap5 = new ClsBranchPaymentTypeMap()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                PaymentTypeId = oPaymentType5.PaymentTypeId,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                ModifiedBy = obj.UserId,
                BranchId = BranchId,
                AccountId = 0,
                IsDefault = false
            };
            oConnectionContext.DbClsBranchPaymentTypeMap.Add(oClsBranchPaymentTypeMap5);
            oConnectionContext.SaveChanges();

            ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap2 = new ClsBranchPaymentTypeMap()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                PaymentTypeId = oPaymentType2.PaymentTypeId,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                ModifiedBy = obj.UserId,
                BranchId = BranchId,
                AccountId = 0,
                IsDefault = false
            };
            oConnectionContext.DbClsBranchPaymentTypeMap.Add(oClsBranchPaymentTypeMap2);
            oConnectionContext.SaveChanges();

            ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap = new ClsBranchPaymentTypeMap()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                PaymentTypeId = oPaymentType.PaymentTypeId,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                ModifiedBy = obj.UserId,
                BranchId = BranchId,
                AccountId = 0,
                IsPosShown = true,
                IsDefault = true
            };
            oConnectionContext.DbClsBranchPaymentTypeMap.Add(oClsBranchPaymentTypeMap);
            oConnectionContext.SaveChanges();

            ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap1 = new ClsBranchPaymentTypeMap()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                PaymentTypeId = oPaymentType1.PaymentTypeId,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                ModifiedBy = obj.UserId,
                BranchId = BranchId,
                AccountId = 0,
                IsPosShown = true,
                IsDefault = false
            };
            oConnectionContext.DbClsBranchPaymentTypeMap.Add(oClsBranchPaymentTypeMap1);
            oConnectionContext.SaveChanges();

            //ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap3 = new ClsBranchPaymentTypeMap()
            //{
            //    AddedBy = obj.UserId,
            //    AddedOn = CurrentDate,
            //    PaymentTypeId = oPaymentType3.PaymentTypeId,
            //    CompanyId = obj.UserId,
            //    IsActive = true,
            //    IsDeleted = false,
            //    ModifiedBy = obj.UserId,
            //    BranchId = BranchId,
            //    AccountId = 0,
            //    IsPosShown = true
            //};
            //oConnectionContext.DbClsBranchPaymentTypeMap.Add(oClsBranchPaymentTypeMap3);
            //oConnectionContext.SaveChanges();

            //ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap4 = new ClsBranchPaymentTypeMap()
            //{
            //    AddedBy = obj.UserId,
            //    AddedOn = CurrentDate,
            //    PaymentTypeId = oPaymentType4.PaymentTypeId,
            //    CompanyId = obj.UserId,
            //    IsActive = true,
            //    IsDeleted = false,
            //    ModifiedBy = obj.UserId,
            //    BranchId = BranchId,
            //    AccountId = 0,
            //    IsPosShown = true
            //};
            //oConnectionContext.DbClsBranchPaymentTypeMap.Add(oClsBranchPaymentTypeMap4);
            //oConnectionContext.SaveChanges();
        }

        public void EmailSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsEmailSettings oClsEmailSettings = new ClsEmailSettings()
            {
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
            };
            oConnectionContext.DbClsEmailSettings.Add(oClsEmailSettings);
            oConnectionContext.SaveChanges();
        }

        public void SmsSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsSmsSettings oClsSmsSettings = new ClsSmsSettings()
            {
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
            };
            oConnectionContext.DbClsSmsSettings.Add(oClsSmsSettings);
            oConnectionContext.SaveChanges();
        }

        public void WhatsappSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsWhatsappSettings oClsWhatsappSettings = new ClsWhatsappSettings()
            {
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
            };
            oConnectionContext.DbClsWhatsappSettings.Add(oClsWhatsappSettings);
            oConnectionContext.SaveChanges();
        }

        //public void OnlinePaymentSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        //{
        //    ClsOnlinePaymentSettings oClsOnlinePaymentSettings = new ClsOnlinePaymentSettings()
        //    {
        //        IsActive = true,
        //        IsDeleted = false,
        //        AddedBy = obj.UserId,
        //        AddedOn = CurrentDate,
        //        CompanyId = obj.UserId,
        //    };
        //    oConnectionContext.DbClsOnlinePaymentSettings.Add(oClsOnlinePaymentSettings);
        //    oConnectionContext.SaveChanges();
        //}

        public void NotificationTemplateSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            var NotificationModulesDetails = oConnectionContext.DbClsNotificationTemplates.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a =>
               new
               {
                   a.Sequence,
                   a.NotificationModulesDetailsId,
                   a.NotificationModulesId,
                   a.NotificationTemplatesId,
                   a.EmailSubject,
                   a.EmailBody,
                   a.SmsBody,
                   a.WhatsappBody
               }).OrderBy(a => a.Sequence).ToList();

            foreach (var item in NotificationModulesDetails)
            {
                ClsNotificationModulesSettings oClsNotificationModulesSettings = new ClsNotificationModulesSettings()
                {
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                    EmailBody = item.EmailBody,
                    EmailSubject = item.EmailSubject,
                    SmsBody = item.SmsBody,
                    WhatsappBody = item.WhatsappBody,
                    NotificationModulesId = item.NotificationModulesId,
                    NotificationModulesDetailsId = item.NotificationModulesDetailsId,
                    NotificationTemplatesId = item.NotificationTemplatesId,
                };
                oConnectionContext.DbClsNotificationModulesSettings.Add(oClsNotificationModulesSettings);
                oConnectionContext.SaveChanges();
            }
        }

        public void ReminderTemplateSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            var ReminderModulesDetails = oConnectionContext.DbClsReminderModulesDetails.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a =>
               new
               {
                   a.Sequence,
                   a.ReminderModulesDetailsId,
                   a.ReminderModulesId,
                   a.EmailSubject,
                   a.EmailBody,
                   a.SmsBody,
                   a.WhatsappBody,
                   a.ReminderType,
                   a.Name,
                   a.Description,
                   a.ReminderBeforeAfter
               }).OrderBy(a => a.Sequence).ToList();

            foreach (var item in ReminderModulesDetails)
            {
                ClsReminderModulesSettings oClsReminderModulesSettings = new ClsReminderModulesSettings()
                {
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    IsActive = true,
                    IsDeleted = false,
                    CompanyId = obj.UserId,
                    ReminderModulesId = item.ReminderModulesId,
                    ReminderModulesDetailsId = item.ReminderModulesDetailsId,
                    ReminderType = item.ReminderType,
                    Name = item.Name,
                    Description = item.Description,
                    ReminderBeforeAfter = item.ReminderBeforeAfter,
                    ReminderInDays = 0,
                    ReminderTo = "Customer",
                    TotalDue = 0,
                    AutoSendEmail = false,
                    AutoSendSms = false,
                    AutoSendWhatsapp = false,
                    BCC = "",
                    CC = "",
                    EmailBody = item.EmailBody,
                    EmailSubject = item.EmailSubject,
                    SmsBody = item.SmsBody,
                    WhatsappBody = item.WhatsappBody
                };
                oConnectionContext.DbClsReminderModulesSettings.Add(oClsReminderModulesSettings);
                oConnectionContext.SaveChanges();
            }
        }

        public void CustomerSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            obj.CurrencyId = oConnectionContext.DbClsUserCurrencyMap.Where(a => a.IsMain == true && a.CompanyId == obj.UserId).Select(a => a.CurrencyId).FirstOrDefault();
            long PaymentTermId = oConnectionContext.DbClsPaymentTerm.Where(a => a.CompanyId == obj.UserId).Select(a => a.PaymentTermId).FirstOrDefault();
            long TaxPreferenceId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.UserId && a.Tax == "Taxable").Select(a => a.TaxId).FirstOrDefault();

            ClsUser oClsUser = new ClsUser()
            {
                IsShippingAddressDifferent = obj.IsShippingAddressDifferent,
                UserRoleId = 0,
                UserGroupId = 0,
                Name = "Walk-In Customer",
                Username = "walkin",
                Password = "",
                EmailId = "",
                MobileNo = "0000000000",
                AltMobileNo = "",
                ReligionId = 0,
                DOB = obj.DOB,
                Gender = obj.Gender,
                BusinessName = obj.BusinessName,
                TaxNo = obj.TaxNo,
                Notes = obj.Notes,
                ProfilePic = obj.ProfilePic,
                CommissionPercent = obj.CommissionPercent,
                CreditLimit = obj.CreditLimit,
                OpeningBalance = obj.OpeningBalance,
                //PayTermNo = obj.PayTermNo,
                //PayTerm = obj.PayTerm,
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                IsCompany = false,
                CompanyId = obj.UserId,
                ExpiryDate = obj.ExpiryDate,
                //JoiningDate = obj.JoiningDate != null ? obj.JoiningDate.Value.AddHours(5).AddMinutes(30) : obj.JoiningDate,
                JoiningDate = obj.JoiningDate.AddHours(5).AddMinutes(30),
                UserType = "Customer",
                TaxId = obj.TaxId,
                CurrencyId = obj.CurrencyId,
                AdvanceBalance = 0 - obj.OpeningBalance,
                IsWalkin = true,
                Under = obj.Under,
                PlaceOfSupplyId = obj.StateId,
                PaymentTermId = PaymentTermId,
                GstTreatment = "Taxable Supply to Consumer",
                TaxPreferenceId = TaxPreferenceId
            };

            oConnectionContext.DbClsUser.Add(oClsUser);
            oConnectionContext.SaveChanges();

            ClsAddress oClsAddress = new ClsAddress()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                Address = "",
                CityId = 0,
                CountryId = 0,
                EmailId = "",
                IsActive = true,
                IsDeleted = false,
                Landmark = "",
                Latitude = "",
                Locality = "",
                Longitude = "",
                MobileNo = "",
                MobileNo2 = "",
                Name = "",
                StateId = 0,
                Type = "",
                UserId = oClsUser.UserId,
                Zipcode = 0
            };
            oConnectionContext.DbClsAddress.Add(oClsAddress);
            oConnectionContext.SaveChanges();

            ClsAddress oClsAddress1 = new ClsAddress()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                Address = "",
                CityId = 0,
                CountryId = 0,
                EmailId = "",
                IsActive = true,
                IsDeleted = false,
                Landmark = "",
                Latitude = "",
                Locality = "",
                Longitude = "",
                MobileNo = "",
                MobileNo2 = "",
                Name = "",
                StateId = 0,
                Type = "",
                UserId = oClsUser.UserId,
                Zipcode = 0
            };
            oConnectionContext.DbClsAddress.Add(oClsAddress1);
            oConnectionContext.SaveChanges();

            ////Map with Branch

            long BranchId = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.UserId).Select(a => a.BranchId).FirstOrDefault();

            ClsUserBranchMap oClsUserBranchMap = new ClsUserBranchMap()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                BranchId = BranchId,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                UserType = "Customer",
                ModifiedBy = obj.UserId,
                UserId = oClsUser.UserId
            };
            //ConnectionContext ocon = new ConnectionContext();
            oConnectionContext.DbClsUserBranchMap.Add(oClsUserBranchMap);
            oConnectionContext.SaveChanges();
        }

        public void CountrySetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsUserCountryMap oClsUserCountryMap = new ClsUserCountryMap()
            {
                CountryId = obj.CountryId,
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                PriceHikePercentage = 0,
                IsMain = true
            };
            oConnectionContext.DbClsUserCountryMap.Add(oClsUserCountryMap);
            oConnectionContext.SaveChanges();
        }

        public void ResellerPaymentMethodSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsResellerPaymentMethod oClsResellerPaymentMethod = new ClsResellerPaymentMethod()
            {
                AddedOn = CurrentDate,
                AddedBy = obj.UserId,
                CompanyId = obj.UserId,
                IsDeleted = false,
                IsActive = true,
            };
            oConnectionContext.DbClsResellerPaymentMethod.Add(oClsResellerPaymentMethod);
            oConnectionContext.SaveChanges();
        }

        public void AccountsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            // DateTime CurrentDate
            var AccountSubTypeMasters = oConnectionContext.DbClsAccountSubTypeMaster.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a => new
            {
                a.AccountTypeId,
                a.AccountSubType,
                a.ParentId,
                a.DisplayAs,
                a.Type,
                a.CanDelete,
                a.Sequence
            }).OrderBy(a => a.AccountTypeId).ThenBy(a => a.Sequence).ToList();

            foreach (var item in AccountSubTypeMasters)
            {
                ClsAccountSubType oAccountSubType = new ClsAccountSubType()
                {
                    AccountSubType = item.AccountSubType,
                    AccountSubTypeCode = "",
                    AccountTypeId = item.AccountTypeId,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                    ParentId = 0,
                    DisplayAs = item.DisplayAs,
                    Type = item.Type,
                    CanDelete = item.CanDelete
                };
                oConnectionContext.DbClsAccountSubType.Add(oAccountSubType);
                oConnectionContext.SaveChanges();
            }

            var AccountMasters = oConnectionContext.DbClsAccountMaster.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a => new
            {
                a.ParentId,
                a.DisplayAs,
                a.CountryId,
                a.AccountTypeId,
                a.Type,
                a.AccountName,
                AccountSubTypeId = oConnectionContext.DbClsAccountSubType.Where(b => b.CompanyId == obj.UserId && b.AccountSubType.ToLower() ==
                oConnectionContext.DbClsAccountSubTypeMaster.Where(c => c.AccountSubTypeMasterId == a.AccountSubTypeMasterId).
                Select(c => c.AccountSubType.ToLower()).FirstOrDefault()).Select(b => b.AccountSubTypeId).FirstOrDefault(),
                a.CanDelete,
                a.Sequence
            }).OrderBy(a => a.AccountTypeId).ThenBy(a => a.Sequence).ToList();

            long SalesDiscountAccountId = 0, SalesSpecialDiscountAccountId = 0, SalesRoundOffAccountId = 0;
            long PurchaseDiscountAccountId = 0, PurchaseSpecialDiscountAccountId =0, PurchaseRoundOffAccountId = 0;

            foreach (var item in AccountMasters)
            {
                bool isInsert = true;
                if (item.CountryId == 2)
                {
                    if (obj.CountryId == 2)
                    {
                        isInsert = true;
                    }
                    else
                    {
                        isInsert = false;
                    }
                }

                if (isInsert == true)
                {
                    ClsAccount oClsAccount = new ClsAccount()
                    {
                        Type = item.Type,
                        AccountName = item.AccountName,
                        AccountNumber = "",
                        AccountSubTypeId = item.AccountSubTypeId,
                        AccountTypeId = item.AccountTypeId,
                        Notes = obj.Notes,
                        //OpeningBalance = obj.OpeningBalance,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.UserId,
                        AddedOn = CurrentDate,
                        CompanyId = obj.UserId,
                        BankName = "",
                        BranchCode = "",
                        BranchName = "",
                        CurrencyId = obj.CurrencyId,
                        CanDelete = item.CanDelete,
                        ParentId = item.ParentId,
                        DisplayAs = item.DisplayAs
                    };
                    oConnectionContext.DbClsAccount.Add(oClsAccount);
                    oConnectionContext.SaveChanges();

                    if (item.Type == "Discount")
                    {
                        SalesDiscountAccountId = oClsAccount.AccountId;
                    }
                    else if (item.Type == "Sales Round Off")
                    {
                        SalesRoundOffAccountId = oClsAccount.AccountId;
                    }
                    else if (item.Type == "Special Discount")
                    {
                        SalesSpecialDiscountAccountId = oClsAccount.AccountId;
                    }
                    else if (item.Type == "Purchase Discounts")
                    {
                        PurchaseDiscountAccountId = oClsAccount.AccountId;
                    }
                    else if (item.Type == "Purchase Round Off")
                    {
                        PurchaseRoundOffAccountId = oClsAccount.AccountId;
                    }
                    else if (item.Type == "Purchase Special Discounts")
                    {
                        PurchaseSpecialDiscountAccountId = oClsAccount.AccountId;
                    }
                }
            }

            ClsAccountSettings oClsAccountSettings = new ClsAccountSettings()
            {
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                IsActive = true,
                IsDeleted = false,
                MigrationDate = CurrentDate
            };
            oConnectionContext.DbClsAccountSettings.Add(oClsAccountSettings);
            oConnectionContext.SaveChanges();

            string query = "update \"tblSaleSettings\" set \"DiscountAccountId\"=" + SalesDiscountAccountId + ",\"SpecialDiscountAccountId\"=" + SalesSpecialDiscountAccountId + ",\"RoundOffAccountId\"=" + SalesRoundOffAccountId + " where \"CompanyId\"=" + obj.UserId;
            oConnectionContext.Database.ExecuteSqlCommand(query);

            query = "update \"tblPurchaseSettings\" set \"DiscountAccountId\"=" + PurchaseDiscountAccountId + ",\"SpecialDiscountAccountId\"=" + PurchaseSpecialDiscountAccountId + ",\"RoundOffAccountId\"=" + PurchaseRoundOffAccountId + " where \"CompanyId\"=" + obj.UserId;
            oConnectionContext.Database.ExecuteSqlCommand(query);
        }

        public void PaymentTermSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsPaymentTerm oPaymentTerm = new ClsPaymentTerm()
            {
                PaymentTerm = "Due upon Receipt",
                Days = 0,
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                IsDueUponReceipt = true
            };
            oConnectionContext.DbClsPaymentTerm.Add(oPaymentTerm);
            oConnectionContext.SaveChanges();
        }

        public void TaxSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            long InputTaxAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Input Tax").Select(a => a.AccountId).FirstOrDefault();

            long OutputTaxAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Output Tax").Select(a => a.AccountId).FirstOrDefault();

            long OutputSGSTAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
               && a.Type == "Output SGST").Select(a => a.AccountId).FirstOrDefault();

            long OutputCGSTAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Output CGST").Select(a => a.AccountId).FirstOrDefault();

            long OutputIGSTAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Output IGST").Select(a => a.AccountId).FirstOrDefault();

            long OutputCESSAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Output CESS").Select(a => a.AccountId).FirstOrDefault();

            long OutputUTGSTAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
 && a.Type == "Output UTGST").Select(a => a.AccountId).FirstOrDefault();

            long InputSGSTAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
&& a.Type == "Input SGST").Select(a => a.AccountId).FirstOrDefault();

            long InputCGSTAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
&& a.Type == "Input CGST").Select(a => a.AccountId).FirstOrDefault();

            long InputIGSTAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
&& a.Type == "Input IGST").Select(a => a.AccountId).FirstOrDefault();

            long InputCESSAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
&& a.Type == "Input CESS").Select(a => a.AccountId).FirstOrDefault();

            long InputUTGSTAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.IsActive == true && a.IsDeleted == false
&& a.Type == "Input UTGST").Select(a => a.AccountId).FirstOrDefault();

            var TaxMasters = oConnectionContext.DbClsTaxMaster.Where(a => a.CountryId == obj.CountryId && a.IsActive == true
            && a.IsDeleted == false).Select(a => new
            {
                a.Tax,
                a.TaxPercent,
                a.ForTaxGroupOnly,
                a.IsTaxGroup,
                a.TaxTypeId,
                TaxType = oConnectionContext.DbClsTaxType.Where(c => c.TaxTypeId == a.TaxTypeId).Select(c => c.TaxType).FirstOrDefault(),
                a.IsPredefined,
                a.IsCompositionScheme
            }).ToList();

            var TaxMasterMaps = (from b in oConnectionContext.DbClsTaxMasterMap
                                 join a in oConnectionContext.DbClsTaxMaster
                                 on b.TaxMasterId equals a.TaxMasterId
                                 where a.CountryId == obj.CountryId && b.IsActive == true && b.IsDeleted == false && a.IsActive == true && a.IsDeleted == false
                                 select new
                                 {
                                     b.TaxMasterId,
                                     b.SubTaxMasterId,
                                 }).ToList();

            ClsTax oTax11 = new ClsTax()
            {
                Tax = "Taxable",
                TaxPercent = 0,
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                IsTaxGroup = false,
                ForTaxGroupOnly = false,
                PurchaseAccountId = InputTaxAccountId,
                SalesAccountId = OutputTaxAccountId,
                ExpenseAccountId = InputTaxAccountId,
                TaxTypeId = 0,
                CanDelete = false,
                IsCompositionScheme = false
            };
            oConnectionContext.DbClsTax.Add(oTax11);
            oConnectionContext.SaveChanges();

            ClsTax oTax1 = new ClsTax()
            {
                Tax = "Non-Taxable",
                TaxPercent = 0,
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                IsTaxGroup = false,
                ForTaxGroupOnly = false,
                PurchaseAccountId = InputTaxAccountId,
                SalesAccountId = OutputTaxAccountId,
                ExpenseAccountId = InputTaxAccountId,
                TaxTypeId = 0,
                CanDelete = false,
                IsCompositionScheme = false
            };
            oConnectionContext.DbClsTax.Add(oTax1);
            oConnectionContext.SaveChanges();

            if (obj.CountryId == 2)
            {
                ClsTax oTax2 = new ClsTax()
                {
                    Tax = "Out of Scope",
                    TaxPercent = 0,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                    IsTaxGroup = false,
                    ForTaxGroupOnly = false,
                    PurchaseAccountId = InputTaxAccountId,
                    SalesAccountId = OutputTaxAccountId,
                    ExpenseAccountId = InputTaxAccountId,
                    TaxTypeId = 0,
                    CanDelete = false,
                    IsCompositionScheme = false
                };
                oConnectionContext.DbClsTax.Add(oTax2);
                oConnectionContext.SaveChanges();

                ClsTax oTax3 = new ClsTax()
                {
                    Tax = "Non-GST Supply",
                    TaxPercent = 0,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                    IsTaxGroup = false,
                    ForTaxGroupOnly = false,
                    PurchaseAccountId = InputTaxAccountId,
                    SalesAccountId = OutputTaxAccountId,
                    ExpenseAccountId = InputTaxAccountId,
                    TaxTypeId = 0,
                    CanDelete = false,
                    IsCompositionScheme = false
                };
                oConnectionContext.DbClsTax.Add(oTax3);
                oConnectionContext.SaveChanges();
            }

            foreach (var item in TaxMasters)
            {
                long PurchaseAccountId = InputTaxAccountId;
                long SalesAccountId = OutputTaxAccountId;
                long ExpenseAccountId = InputTaxAccountId;

                if (item.IsTaxGroup == true)
                {
                    PurchaseAccountId = 0;
                    SalesAccountId = 0;
                    ExpenseAccountId = 0;
                }

                if (item.TaxType == "CGST")
                {
                    PurchaseAccountId = InputCGSTAccountId;
                    SalesAccountId = OutputCGSTAccountId;
                    ExpenseAccountId = InputCGSTAccountId;
                }
                else if (item.TaxType == "SGST")
                {
                    PurchaseAccountId = InputSGSTAccountId;
                    SalesAccountId = OutputSGSTAccountId;
                    ExpenseAccountId = InputSGSTAccountId;
                }
                else if (item.TaxType == "IGST")
                {
                    PurchaseAccountId = InputIGSTAccountId;
                    SalesAccountId = OutputIGSTAccountId;
                    ExpenseAccountId = InputIGSTAccountId;
                }
                else if (item.TaxType == "UTGST")
                {
                    PurchaseAccountId = InputUTGSTAccountId;
                    SalesAccountId = OutputUTGSTAccountId;
                    ExpenseAccountId = InputUTGSTAccountId;
                }
                else if (item.TaxType == "CESS")
                {
                    PurchaseAccountId = InputCESSAccountId;
                    SalesAccountId = OutputCESSAccountId;
                    ExpenseAccountId = InputCESSAccountId;
                }

                ClsTax oTax = new ClsTax()
                {
                    Tax = item.Tax,
                    TaxPercent = item.TaxPercent,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                    IsTaxGroup = item.IsTaxGroup,
                    ForTaxGroupOnly = item.ForTaxGroupOnly,
                    PurchaseAccountId = PurchaseAccountId,
                    SalesAccountId = SalesAccountId,
                    ExpenseAccountId = ExpenseAccountId,
                    TaxTypeId = item.TaxTypeId,
                    CanDelete = true,
                    IsPredefined = item.IsPredefined,
                    IsCompositionScheme = item.IsCompositionScheme
                };
                oConnectionContext.DbClsTax.Add(oTax);
                oConnectionContext.SaveChanges();
            }

            foreach (var item in TaxMasterMaps)
            {
                long TaxId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.UserId &&
                a.Tax == oConnectionContext.DbClsTaxMaster.Where(b => b.TaxMasterId == item.TaxMasterId).Select(b => b.Tax).FirstOrDefault()).Select(a => a.TaxId).FirstOrDefault();

                long SubTaxMasterId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.UserId &&
                a.Tax == oConnectionContext.DbClsTaxMaster.Where(b => b.TaxMasterId == item.SubTaxMasterId).Select(b => b.Tax).FirstOrDefault()).Select(a => a.TaxId).FirstOrDefault();

                ClsTaxMap oClsTaxMap = new ClsTaxMap()
                {
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                    IsActive = true,
                    IsDeleted = false,
                    TaxId = TaxId,
                    SubTaxId = SubTaxMasterId
                };
                oConnectionContext.DbClsTaxMap.Add(oClsTaxMap);
                oConnectionContext.SaveChanges();
            }
        }

        public void PwaSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsPwaSettings oClsPwaSettings = new ClsPwaSettings()
            {
                BackgroundColor = "#fdfdfd",
                ThemeColor = "#db4938",
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                IsActive = true,
                IsDeleted = false,
                CompanyId = obj.UserId
            };
            oConnectionContext.DbClsPwaSettings.Add(oClsPwaSettings);
            oConnectionContext.SaveChanges();
        }

        public void UnitSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            var Units = oConnectionContext.DbClsUnitMaster.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a => new
            {
                a.UnitName,
                a.UnitShortName,
                a.IsPredefined,
                a.AllowDecimal
            }).ToList();

            foreach (var item in Units)
            {
                ClsUnit oUnit = new ClsUnit()
                {
                    UnitName = item.UnitName,
                    UnitShortName = item.UnitShortName,
                    UnitCode = "",
                    AllowDecimal = item.AllowDecimal,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                    IsPredefined = item.IsPredefined
                };
                oConnectionContext.DbClsUnit.Add(oUnit);
                oConnectionContext.SaveChanges();
            }
        }

        public void ExpenseSettingsSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            long UnitId = oConnectionContext.DbClsUnit.Where(a => a.CompanyId == obj.UserId).Select(a => a.UnitId).FirstOrDefault();

            long MileageAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.UserId && a.Type == "Fuel/Mileage Expenses").Select(a => a.AccountId).FirstOrDefault();

            ClsExpenseSettings oClsExpenseSettings = new ClsExpenseSettings()
            {
                EnableMileage = false,
                UnitId = UnitId,
                MileageAccountId = MileageAccountId,
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
            };
            oConnectionContext.DbClsExpenseSettings.Add(oClsExpenseSettings);
            oConnectionContext.SaveChanges();
        }

        public void StockAdjustmentReasonSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            List<ClsStockAdjustmentReason> Reasons = new List<ClsStockAdjustmentReason>
        {
            new ClsStockAdjustmentReason { StockAdjustmentReason = "Overstock", Description= "Excess inventory due to over-purchasing or inaccurate demand forecasting" },
            new ClsStockAdjustmentReason { StockAdjustmentReason = "Understock", Description= "Insufficient inventory resulting from inaccurate stock tracking or high demand" },
            new ClsStockAdjustmentReason { StockAdjustmentReason = "Damaged Goods", Description= "Items that have been damaged and are no longer sellable" },
            new ClsStockAdjustmentReason { StockAdjustmentReason = "Expired Products", Description= "Perishable or time-sensitive items that are past their expiration date" },
            new ClsStockAdjustmentReason { StockAdjustmentReason = "Theft or Loss", Description= "Inventory lost due to theft or misplacement" },
            new ClsStockAdjustmentReason { StockAdjustmentReason = "Returns", Description= "Returned items that need to be added back to stock or adjusted for restocking" },
            new ClsStockAdjustmentReason { StockAdjustmentReason = "Spoilage", Description= "Products, particularly in food or pharmaceuticals, that have deteriorated or gone bad" },
            new ClsStockAdjustmentReason { StockAdjustmentReason = "Shrinkage", Description= "Loss of inventory due to theft, errors in counting, or damage that wasn’t initially noticed" },
            new ClsStockAdjustmentReason { StockAdjustmentReason = "Inventory System Error", Description= "Adjustments made due to discrepancies in recorded stock and physical counts" },
        };

            foreach (var item in Reasons)
            {
                ClsStockAdjustmentReason oStockAdjustmentReason = new ClsStockAdjustmentReason()
                {
                    StockAdjustmentReason = item.StockAdjustmentReason,
                    Description = item.Description,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                };
                oConnectionContext.DbClsStockAdjustmentReason.Add(oStockAdjustmentReason);
                oConnectionContext.SaveChanges();
            }
        }

        public void StockTransferReasonSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            List<ClsStockTransferReason> Reasons = new List<ClsStockTransferReason>
        {
            new ClsStockTransferReason { StockTransferReason = "Replenishment", Description= "Moving stock from central warehouses to retail outlets or branches" },
            new ClsStockTransferReason { StockTransferReason = "Demand Shifts", Description= "Addressing increased demand in specific locations" },
            new ClsStockTransferReason { StockTransferReason = "Excess Stock", Description= "Relocating overstocked items to areas with lower inventory" },
            new ClsStockTransferReason { StockTransferReason = "Seasonal Adjustments", Description= "Transferring seasonal goods to regions where they are in demand" },
            new ClsStockTransferReason { StockTransferReason = "New Store Setup", Description= "Supplying inventory to newly opened locations" },
            new ClsStockTransferReason { StockTransferReason = "Consolidation", Description= "Centralizing inventory for cost efficiency" },
            new ClsStockTransferReason { StockTransferReason = "Returns or Repairs", Description= "Returning faulty or excess stock to warehouses or repair centers" },
            new ClsStockTransferReason { StockTransferReason = "Promotional Events", Description= "Allocating stock to locations hosting special promotions" },
            new ClsStockTransferReason { StockTransferReason = "Stock Optimization", Description= "Balancing inventory across locations to prevent shortages or surpluses" },
            new ClsStockTransferReason { StockTransferReason = "Business Restructuring", Description= "Adjusting inventory due to mergers, closures, or relocations" },
        };

            foreach (var item in Reasons)
            {
                ClsStockTransferReason oStockTransferReason = new ClsStockTransferReason()
                {
                    StockTransferReason = item.StockTransferReason,
                    Description = item.Description,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                };
                oConnectionContext.DbClsStockTransferReason.Add(oStockTransferReason);
                oConnectionContext.SaveChanges();
            }
        }

        public void SalesCreditNoteReasonSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            List<ClsSalesCreditNoteReason> Reasons = new List<ClsSalesCreditNoteReason>
        {
            new ClsSalesCreditNoteReason { SalesCreditNoteReason = "Price Adjustment", Description= "Incorrect pricing on the original invoice" },
            new ClsSalesCreditNoteReason { SalesCreditNoteReason = "Overbilling", Description= "Customer was overcharged" },
            new ClsSalesCreditNoteReason { SalesCreditNoteReason = "Quantity Error", Description= "Wrong quantity billed or delivered" },
            new ClsSalesCreditNoteReason { SalesCreditNoteReason = "Product Returns", Description= "Goods returned due to defects or dissatisfaction" },
            new ClsSalesCreditNoteReason { SalesCreditNoteReason = "Discounts", Description= "Applying agreed discounts after invoicing" },
            new ClsSalesCreditNoteReason { SalesCreditNoteReason = "Service Issues", Description= "Poor or incomplete service provided" },
            new ClsSalesCreditNoteReason { SalesCreditNoteReason = "Tax Correction", Description= "Errors in tax calculation or application" },
            new ClsSalesCreditNoteReason { SalesCreditNoteReason = "Promotional Adjustments", Description= "Applying promotional deals retrospectively" },
            new ClsSalesCreditNoteReason { SalesCreditNoteReason = "Cancellation", Description= "Order or service canceled after invoicing" },
            new ClsSalesCreditNoteReason { SalesCreditNoteReason = "Wrong Customer", Description= "Invoice issued to the wrong client" },
        };

            foreach (var item in Reasons)
            {
                ClsSalesCreditNoteReason oSalesCreditNoteReason = new ClsSalesCreditNoteReason()
                {
                    SalesCreditNoteReason = item.SalesCreditNoteReason,
                    Description = item.Description,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                };
                oConnectionContext.DbClsSalesCreditNoteReason.Add(oSalesCreditNoteReason);
                oConnectionContext.SaveChanges();
            }
        }

        public void SalesDebitNoteReasonSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            List<ClsSalesDebitNoteReason> Reasons = new List<ClsSalesDebitNoteReason>
        {
            new ClsSalesDebitNoteReason { SalesDebitNoteReason = "Price Adjustment", Description= "Incorrect pricing in the original invoice, requiring an upward correction" },
            new ClsSalesDebitNoteReason { SalesDebitNoteReason = "Additional Charges", Description= "Adding extra costs like shipping, taxes, or service fees not included in the original invoice" },
            new ClsSalesDebitNoteReason { SalesDebitNoteReason = "Returned Goods", Description= "Customer returns goods but not all items qualify for a full credit adjustment" },
            new ClsSalesDebitNoteReason { SalesDebitNoteReason = "Error Correction", Description= "Rectifying mistakes in the original invoice, such as underbilled quantities or rates" },
            new ClsSalesDebitNoteReason { SalesDebitNoteReason = "Penalties or Interest", Description= "Adding late payment fees or interest for delayed payments by the customer" },
            new ClsSalesDebitNoteReason { SalesDebitNoteReason = "Exchange Rate Difference", Description= "Adjustments due to currency fluctuations in international transactions" },
            new ClsSalesDebitNoteReason { SalesDebitNoteReason = "Services Not Invoiced", Description= "Billing for additional services rendered after the original invoice" },
        };

            foreach (var item in Reasons)
            {
                ClsSalesDebitNoteReason oSalesDebitNoteReason = new ClsSalesDebitNoteReason()
                {
                    SalesDebitNoteReason = item.SalesDebitNoteReason,
                    Description = item.Description,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                };
                oConnectionContext.DbClsSalesDebitNoteReason.Add(oSalesDebitNoteReason);
                oConnectionContext.SaveChanges();
            }
        }

        public void PurchaseDebitNoteReasonSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            List<ClsPurchaseDebitNoteReason> Reasons = new List<ClsPurchaseDebitNoteReason>
        {
            new ClsPurchaseDebitNoteReason { PurchaseDebitNoteReason = "Return of Goods", Description= "Received defective, damaged, or incorrect items" },
            new ClsPurchaseDebitNoteReason { PurchaseDebitNoteReason = "Short Supply", Description= "Fewer goods received than ordered" },
            new ClsPurchaseDebitNoteReason { PurchaseDebitNoteReason = "Price Discrepancy", Description= "Incorrect pricing on the invoice" },
            new ClsPurchaseDebitNoteReason { PurchaseDebitNoteReason = "Quality Issues", Description= "Goods not meeting agreed specifications" },
            new ClsPurchaseDebitNoteReason { PurchaseDebitNoteReason = "Service Non-Compliance", Description= "Service not delivered as per agreement" },
            new ClsPurchaseDebitNoteReason { PurchaseDebitNoteReason = "Overbilling", Description= "Invoice amount higher than agreed" },
            new ClsPurchaseDebitNoteReason { PurchaseDebitNoteReason = "Promotional Allowances", Description= "Discounts or rebates not applied" },
            new ClsPurchaseDebitNoteReason { PurchaseDebitNoteReason = "Cancellations", Description= "Order partially or fully canceled" },
        };

            foreach (var item in Reasons)
            {
                ClsPurchaseDebitNoteReason oPurchaseDebitNoteReason = new ClsPurchaseDebitNoteReason()
                {
                    PurchaseDebitNoteReason = item.PurchaseDebitNoteReason,
                    Description = item.Description,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = CurrentDate,
                    CompanyId = obj.UserId,
                };
                oConnectionContext.DbClsPurchaseDebitNoteReason.Add(oPurchaseDebitNoteReason);
                oConnectionContext.SaveChanges();
            }
        }

        public void TaxExemptionSetup(ClsUserVm obj, DateTime CurrentDate)
        {
            ClsTaxExemption oTaxExemption = new ClsTaxExemption()
            {
                TaxExemptionType = "Item",
                Reason = "Bill Of Supply",
                Description = "",
                IsActive = true,
                IsDeleted = false,
                AddedBy = obj.UserId,
                AddedOn = CurrentDate,
                CompanyId = obj.UserId,
                CanDelete = false
            };
            oConnectionContext.DbClsTaxExemption.Add(oTaxExemption);
            oConnectionContext.SaveChanges();
        }

        public void NotificationTemplateSetup1(ClsUserVm obj)
        {
            var NotificationModulesDetails = oConnectionContext.DbClsNotificationTemplates.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a =>
               new
               {
                   a.Sequence,
                   a.NotificationModulesDetailsId,
                   a.NotificationModulesId,
                   a.NotificationTemplatesId,
                   a.EmailSubject,
                   a.EmailBody,
                   a.SmsBody,
                   a.WhatsappBody
               }).OrderBy(a => a.Sequence).ToList();

            CommonController oCommonController = new CommonController();

            foreach (var item in NotificationModulesDetails)
            {
                ClsNotificationModulesSettings oClsNotificationModulesSettings = new ClsNotificationModulesSettings()
                {
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.UserId,
                    AddedOn = oCommonController.CurrentDate(obj.UserId),
                    CompanyId = obj.UserId,
                    EmailBody = item.EmailBody,
                    EmailSubject = item.EmailSubject,
                    SmsBody = item.SmsBody,
                    WhatsappBody = item.WhatsappBody,
                    NotificationModulesId = item.NotificationModulesId,
                    NotificationModulesDetailsId = item.NotificationModulesDetailsId,
                    NotificationTemplatesId = item.NotificationTemplatesId,
                };
                oConnectionContext.DbClsNotificationModulesSettings.Add(oClsNotificationModulesSettings);
                oConnectionContext.SaveChanges();
            }
        }

        public void ReminderTemplateSetup1(ClsUserVm obj)
        {
            var ReminderModulesDetails = oConnectionContext.DbClsReminderModulesDetails.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a =>
               new
               {
                   a.Sequence,
                   a.ReminderModulesDetailsId,
                   a.ReminderModulesId,
                   a.EmailSubject,
                   a.EmailBody,
                   a.SmsBody,
                   a.WhatsappBody,
                   a.ReminderType,
                   a.Name,
                   a.Description,
                   a.ReminderBeforeAfter
               }).OrderBy(a => a.Sequence).ToList();

            CommonController oCommonController = new CommonController();

            foreach (var item in ReminderModulesDetails)
            {
                ClsReminderModulesSettings oClsReminderModulesSettings = new ClsReminderModulesSettings()
                {
                    AddedBy = obj.UserId,
                    AddedOn =oCommonController.CurrentDate(obj.UserId),
                    IsActive = true,
                    IsDeleted = false,
                    CompanyId = obj.UserId,
                    ReminderModulesId = item.ReminderModulesId,
                    ReminderModulesDetailsId = item.ReminderModulesDetailsId,
                    ReminderType = item.ReminderType,
                    Name = item.Name,
                    Description = item.Description,
                    ReminderBeforeAfter = item.ReminderBeforeAfter,
                    ReminderInDays = 0,
                    ReminderTo = "Customer",
                    TotalDue = 0,
                    AutoSendEmail = false,
                    AutoSendSms = false,
                    AutoSendWhatsapp = false,
                    BCC = "",
                    CC = "",
                    EmailBody = item.EmailBody,
                    EmailSubject = item.EmailSubject,
                    SmsBody = item.SmsBody,
                    WhatsappBody = item.WhatsappBody
                };
                oConnectionContext.DbClsReminderModulesSettings.Add(oClsReminderModulesSettings);
                oConnectionContext.SaveChanges();
            }
        }

    }
}
