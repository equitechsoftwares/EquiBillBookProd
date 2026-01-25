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
    public class PosSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;

        public async Task<IHttpActionResult> PosSetting(ClsPosSettingsVm obj)
        {
            var det = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                PosSettingsId = a.PosSettingsId,
                DisableMultiplePay = a.DisableMultiplePay,
                DisableDraft = a.DisableDraft,
                DisableExpressCheckout = a.DisableExpressCheckout,
                DontShowProductSuggestion = a.DontShowProductSuggestion,
                DontShowRecentTransactions = a.DontShowRecentTransactions,
                DisableDicount = a.DisableDicount,
                DisableOrderTax = a.DisableOrderTax,
                SubTotalEditable = a.SubTotalEditable,
                EnableTransactionDate = a.EnableTransactionDate,
                EnableServiceStaff = a.EnableServiceStaff,
                IsServiceStaffRequired = a.IsServiceStaffRequired,
                EnableWeighingScale = a.EnableWeighingScale,
                ShowInvoiceScheme = a.ShowInvoiceScheme,
                ShowInvoiceLayoutDropdown = a.ShowInvoiceLayoutDropdown,
                PrintInvoiceOnHold = a.PrintInvoiceOnHold,
                ShowPricingOnProductSuggestionTooltip = a.ShowPricingOnProductSuggestionTooltip,
                WeighingScaleBarcodePrefix = a.WeighingScaleBarcodePrefix,
                ProductSkuLength = a.ProductSkuLength,
                QuantityIntegerPartLength = a.QuantityIntegerPartLength,
                QuantityFractionalPartLength = a.QuantityFractionalPartLength,
                AllowOnlinePayment = a.AllowOnlinePayment,
                DisableCreditSale = a.DisableCreditSale,
                DisableHold = a.DisableHold,
                DisableProforma = a.DisableProforma,
                DisableQuotation = a.DisableQuotation,
                EnableSms = a.EnableSms,
                a.EnableEmail,
                a.EnableWhatsapp,
                a.EnableNotes,
                a.EnableFreeQuantity,
                a.SalePriceIsMinSellingPrice,
                a.Draft,
                a.Quotation,
                a.Proforma,
                a.CreditSale,
                a.EditShippingCharge,
                a.EditPackagingCharge,
                Multiple = a.Multiple,
                Hold = a.Hold,
                Cancel = a.Cancel,
                GoToProductQuantity = a.GoToProductQuantity,
                WeighingScale = a.WeighingScale,
                EditDiscount = a.EditDiscount,
                EditOrderTax = a.EditOrderTax,
                AddPaymentRow = a.AddPaymentRow,
                FinalisePayment = a.FinalisePayment,
                AddNewProduct = a.AddNewProduct,
                a.RecentTransactions,
                a.HoldList,
                a.Calculator,
                a.FullScreen,
                a.RegisterDetails,
                a.PosExit,
                a.EnableRoundOff,
                a.InvoiceType,
                a.AutoPrintInvoiceFinal,
                //a.AutoPrintInvoiceDraft,
                //a.AutoPrintInvoiceQuotation,
                //a.AutoPrintInvoiceProforma,
                a.AutoPrintInvoiceSalesReturn,
                //a.ChangeReturnAccountId
                a.EnableSpecialDiscount,
                a.EditSpecialDiscount,
                a.EnablePlaceOfSupply,
                a.EnableSellingPriceGroup,
                a.ShowItemImage,
                a.ShowItemSellingPrice,
                a.ShowItemMrp,
                a.EnableKot,
                a.EnableTableBooking,
                a.AutoCreateKot,
                a.AllowLinkExistingKot
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PosSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PosSettingsUpdate(ClsPosSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PosSettingsId = oConnectionContext.DbClsPosSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.PosSettingsId).FirstOrDefault();

                ClsPosSettings oClsPosSettings = new ClsPosSettings()
                {
                    PosSettingsId = PosSettingsId,
                    DisableMultiplePay = obj.DisableMultiplePay,
                    DisableDraft = obj.DisableDraft,
                    DisableExpressCheckout = obj.DisableExpressCheckout,
                    DontShowProductSuggestion = obj.DontShowProductSuggestion,
                    DontShowRecentTransactions = obj.DontShowRecentTransactions,
                    DisableDicount = obj.DisableDicount,
                    DisableOrderTax = obj.DisableOrderTax,
                    SubTotalEditable = obj.SubTotalEditable,
                    EnableTransactionDate = obj.EnableTransactionDate,
                    EnableServiceStaff = obj.EnableServiceStaff,
                    IsServiceStaffRequired = obj.IsServiceStaffRequired,
                    EnableWeighingScale = obj.EnableWeighingScale,
                    ShowInvoiceScheme = obj.ShowInvoiceScheme,
                    ShowInvoiceLayoutDropdown = obj.ShowInvoiceLayoutDropdown,
                    ShowPricingOnProductSuggestionTooltip = obj.ShowPricingOnProductSuggestionTooltip,
                    WeighingScaleBarcodePrefix = obj.WeighingScaleBarcodePrefix,
                    ProductSkuLength = obj.ProductSkuLength,
                    QuantityIntegerPartLength = obj.QuantityIntegerPartLength,
                    QuantityFractionalPartLength = obj.QuantityFractionalPartLength,
                    AllowOnlinePayment = obj.AllowOnlinePayment,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    DisableCreditSale= obj.DisableCreditSale,
                    DisableHold= obj.DisableHold,
                    DisableProforma= obj.DisableProforma,
                    DisableQuotation= obj.DisableQuotation,
                    PrintInvoiceOnHold= obj.PrintInvoiceOnHold,
                    EnableSms = obj.EnableSms,
                    EnableEmail= obj.EnableEmail,
                    EnableWhatsapp= obj.EnableWhatsapp,
                    EnableNotes= obj.EnableNotes,
                    EnableFreeQuantity = obj.EnableFreeQuantity,
                    SalePriceIsMinSellingPrice= obj.SalePriceIsMinSellingPrice,
                    Draft = obj.Draft,
                    Quotation = obj.Quotation,
                    Proforma = obj.Proforma,
                    CreditSale = obj.CreditSale,
                    EditShippingCharge = obj.EditShippingCharge,
                    EditPackagingCharge = obj.EditPackagingCharge,
                    Multiple = obj.Multiple,
                    Hold = obj.Hold,
                    Cancel = obj.Cancel,
                    GoToProductQuantity = obj.GoToProductQuantity,
                    WeighingScale = obj.WeighingScale,
                    EditDiscount = obj.EditDiscount,
                    EditOrderTax = obj.EditOrderTax,
                    AddPaymentRow = obj.AddPaymentRow,
                    FinalisePayment = obj.FinalisePayment,
                    AddNewProduct = obj.AddNewProduct,
                    RecentTransactions = obj.RecentTransactions,
                    HoldList= obj.HoldList,
                    Calculator= obj.Calculator,
                    FullScreen= obj.FullScreen,
                    RegisterDetails = obj.RegisterDetails,
                    PosExit = obj.PosExit,
                    EnableRoundOff = obj.EnableRoundOff,
                    InvoiceType = obj.InvoiceType,
                    AutoPrintInvoiceFinal = obj.AutoPrintInvoiceFinal,
                    //AutoPrintInvoiceDraft = obj.AutoPrintInvoiceDraft,
                    //AutoPrintInvoiceQuotation = obj.AutoPrintInvoiceQuotation,
                    //AutoPrintInvoiceProforma = obj.AutoPrintInvoiceProforma,
                    AutoPrintInvoiceSalesReturn = obj.AutoPrintInvoiceSalesReturn,
                    //ChangeReturnAccountId = obj.ChangeReturnAccountId,
                    EnableSpecialDiscount = obj.EnableSpecialDiscount,
                    EditSpecialDiscount = obj.EditSpecialDiscount,
                    EnablePlaceOfSupply = obj.EnablePlaceOfSupply,
                    EnableSellingPriceGroup = obj.EnableSellingPriceGroup,
                    ShowItemImage= obj.ShowItemImage,
                    ShowItemSellingPrice = obj.ShowItemSellingPrice,
                    ShowItemMrp = obj.ShowItemMrp,
                    EnableKot = obj.EnableKot,
                    EnableTableBooking = obj.EnableTableBooking,
                    AutoCreateKot = obj.AutoCreateKot,
                    AllowLinkExistingKot = obj.AllowLinkExistingKot
                };

                oConnectionContext.DbClsPosSettings.Attach(oClsPosSettings);
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DisableMultiplePay).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DisableDraft).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DisableExpressCheckout).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DontShowProductSuggestion).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DontShowRecentTransactions).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DisableDicount).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DisableOrderTax).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.SubTotalEditable).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableTransactionDate).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableServiceStaff).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.IsServiceStaffRequired).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableWeighingScale).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.ShowInvoiceScheme).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.ShowInvoiceLayoutDropdown).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.ShowPricingOnProductSuggestionTooltip).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.WeighingScaleBarcodePrefix).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.ProductSkuLength).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.QuantityIntegerPartLength).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.QuantityFractionalPartLength).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.AllowOnlinePayment).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DisableCreditSale).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DisableHold).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DisableProforma).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.DisableQuotation).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.PrintInvoiceOnHold).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableSms).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableEmail).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableWhatsapp).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableNotes).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableFreeQuantity).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.SalePriceIsMinSellingPrice).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.Draft).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.Quotation).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.Proforma).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.CreditSale).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EditShippingCharge).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EditPackagingCharge).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.Multiple).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.Hold).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.Cancel).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.GoToProductQuantity).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.WeighingScale).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EditDiscount).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EditOrderTax).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.AddPaymentRow).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.FinalisePayment).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.AddNewProduct).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.RecentTransactions).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.HoldList).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.Calculator).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.FullScreen).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.RegisterDetails).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.PosExit).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableRoundOff).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.InvoiceType).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.AutoPrintInvoiceFinal).IsModified = true;
                //oConnectionContext.Entry(oClsPosSettings).Property(x => x.AutoPrintInvoiceDraft).IsModified = true;
                //oConnectionContext.Entry(oClsPosSettings).Property(x => x.AutoPrintInvoiceQuotation).IsModified = true;
                //oConnectionContext.Entry(oClsPosSettings).Property(x => x.AutoPrintInvoiceProforma).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.AutoPrintInvoiceSalesReturn).IsModified = true;
                //oConnectionContext.Entry(oClsPosSettings).Property(x => x.ChangeReturnAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableSpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EditSpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnablePlaceOfSupply).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableSellingPriceGroup).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.ShowItemImage).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.ShowItemSellingPrice).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.ShowItemMrp).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableKot).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.EnableTableBooking).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.AutoCreateKot).IsModified = true;
                oConnectionContext.Entry(oClsPosSettings).Property(x => x.AllowLinkExistingKot).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.AdditionalCharges != null)
                {
                    foreach (var item in obj.AdditionalCharges)
                    {
                        ClsAdditionalCharge oClsAdditionalCharge = new ClsAdditionalCharge()
                        {
                            AdditionalChargeId = item.AdditionalChargeId,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            ShortCutKey = item.ShortCutKey
                        };
                        oConnectionContext.DbClsAdditionalCharge.Attach(oClsAdditionalCharge);
                        oConnectionContext.Entry(oClsAdditionalCharge).Property(x => x.AdditionalChargeId).IsModified = true;
                        oConnectionContext.Entry(oClsAdditionalCharge).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oClsAdditionalCharge).Property(x => x.ModifiedOn).IsModified = true;
                        oConnectionContext.Entry(oClsAdditionalCharge).Property(x => x.ShortCutKey).IsModified = true;
                        oConnectionContext.SaveChanges();
                    }
                }

                if (obj.PaymentTypes != null)
                {
                    foreach (var item in obj.PaymentTypes)
                    {
                        ClsPaymentType oPaymentType = new ClsPaymentType()
                        {
                            PaymentTypeId = item.PaymentTypeId,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            //IsPosShown = item.IsPosShown,
                            ShortCutKey = item.ShortCutKey
                        };
                        oConnectionContext.DbClsPaymentType.Attach(oPaymentType);
                        oConnectionContext.Entry(oPaymentType).Property(x => x.PaymentTypeId).IsModified = true;
                        oConnectionContext.Entry(oPaymentType).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oPaymentType).Property(x => x.ModifiedOn).IsModified = true;
                        //oConnectionContext.Entry(oPaymentType).Property(x => x.IsPosShown).IsModified = true;
                        oConnectionContext.Entry(oPaymentType).Property(x => x.ShortCutKey).IsModified = true;
                        oConnectionContext.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"POS\" updated",
                    Id = oClsPosSettings.PosSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "POS Info updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
    }
}
