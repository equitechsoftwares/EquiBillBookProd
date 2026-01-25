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
    public class SaleSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;

        public async Task<IHttpActionResult> SaleSetting(ClsSaleSettingsVm obj)
        {
            var det = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                SaleSettingsId = a.SaleSettingsId,
                DefaultSaleDiscount = a.DefaultSaleDiscount,
                a.DiscountType,
                DefaultSaleTaxId = a.DefaultSaleTaxId,
                SalePriceIsMinSellingPrice = a.SalePriceIsMinSellingPrice,
                AllowOverSelling = a.AllowOverSelling,
                IsPayTermRequired = a.IsPayTermRequired,
                SalesCommissionAgent = a.SalesCommissionAgent,
                CommissionCalculationType = a.CommissionCalculationType,
                IsCommissionAgentRequired = a.IsCommissionAgentRequired,
                a.AllowOnlinePayment,
                EnableSms = a.EnableSms,
                a.EnableEmail,
                a.EnableWhatsapp,
                a.EnableFreeQuantity,
                a.EnableRoundOff,
                a.InvoiceType,
                a.AutoPrintInvoiceQuotation,
                a.AutoPrintInvoiceOrder,
                a.AutoPrintInvoiceDeliveryChallan,
                a.AutoPrintInvoiceProforma,
                a.AutoPrintInvoiceBill,
                a.AutoPrintInvoiceSalesReturn,
                a.DiscountAccountId,
                a.RoundOffAccountId,
                a.EnableCustomerGroup,
                a.EnableSalesQuotation,
                a.EnableSalesOrder,
                a.EnableSalesProforma,
                a.EnableDeliveryChallan,
                a.PaymentTermId,
                a.EnablePos,
                a.EnableSpecialDiscount,
                a.SpecialDiscountAccountId,
                a.EnableNotes,
                a.EnableTerms,
                a.EnableRecurringSales,
                a.DefaultNotes,
                a.DefaultTerms
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SaleSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SaleSettingsUpdate(ClsSaleSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long SaleSettingsId = oConnectionContext.DbClsSaleSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.SaleSettingsId).FirstOrDefault();

                ClsSaleSettings oClsSaleSettings = new ClsSaleSettings()
                {
                    SaleSettingsId = SaleSettingsId,
                    DefaultSaleDiscount = obj.DefaultSaleDiscount,
                    DiscountType = obj.DiscountType,
                    DefaultSaleTaxId = obj.DefaultSaleTaxId,
                    SalePriceIsMinSellingPrice = obj.SalePriceIsMinSellingPrice,
                    AllowOverSelling = obj.AllowOverSelling,
                    IsPayTermRequired = obj.IsPayTermRequired,
                    SalesCommissionAgent = obj.SalesCommissionAgent,
                    CommissionCalculationType = obj.CommissionCalculationType,
                    IsCommissionAgentRequired = obj.IsCommissionAgentRequired,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    AllowOnlinePayment = obj.AllowOnlinePayment,
                    EnableSms = obj.EnableSms,
                    EnableEmail = obj.EnableEmail,
                    EnableWhatsapp = obj.EnableWhatsapp,
                    EnableFreeQuantity = obj.EnableFreeQuantity,
                    EnableRoundOff = obj.EnableRoundOff,
                    InvoiceType = obj.InvoiceType,
                    AutoPrintInvoiceQuotation = obj.AutoPrintInvoiceQuotation,
                    AutoPrintInvoiceOrder = obj.AutoPrintInvoiceOrder,
                    AutoPrintInvoiceDeliveryChallan= obj.AutoPrintInvoiceDeliveryChallan,
                    AutoPrintInvoiceProforma = obj.AutoPrintInvoiceProforma,
                    AutoPrintInvoiceBill = obj.AutoPrintInvoiceBill,
                    AutoPrintInvoiceSalesReturn = obj.AutoPrintInvoiceSalesReturn,
                    DiscountAccountId = obj.DiscountAccountId,
                    RoundOffAccountId = obj.RoundOffAccountId,
                    EnableCustomerGroup = obj.EnableCustomerGroup,
                    EnableSalesQuotation = obj.EnableSalesQuotation,
                    EnableSalesOrder = obj.EnableSalesOrder,
                    EnableSalesProforma = obj.EnableSalesProforma,
                    EnableDeliveryChallan = obj.EnableDeliveryChallan,
                    PaymentTermId = obj.PaymentTermId,
                    EnablePos = obj.EnablePos,
                    EnableSpecialDiscount = obj.EnableSpecialDiscount,
                    SpecialDiscountAccountId = obj.SpecialDiscountAccountId,
                    EnableNotes = obj.EnableNotes,
                    EnableTerms = obj.EnableTerms,
                    EnableRecurringSales = obj.EnableRecurringSales,
                    DefaultNotes = obj.DefaultNotes,
                    DefaultTerms = obj.DefaultTerms
                };

                oConnectionContext.DbClsSaleSettings.Attach(oClsSaleSettings);
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.SaleSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.DefaultSaleDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.DiscountType).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.DefaultSaleTaxId).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.SalePriceIsMinSellingPrice).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.AllowOverSelling).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.IsPayTermRequired).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.SalesCommissionAgent).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.CommissionCalculationType).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.IsCommissionAgentRequired).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.AllowOnlinePayment).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableSms).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableEmail).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableWhatsapp).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableFreeQuantity).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableRoundOff).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.InvoiceType).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.AutoPrintInvoiceQuotation).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.AutoPrintInvoiceOrder).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.AutoPrintInvoiceDeliveryChallan).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.AutoPrintInvoiceProforma).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.AutoPrintInvoiceBill).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.AutoPrintInvoiceSalesReturn).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.DiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.RoundOffAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableCustomerGroup).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableSalesQuotation).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableSalesOrder).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableSalesProforma).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableDeliveryChallan).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnablePos).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableSpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.SpecialDiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableNotes).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableTerms).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.EnableRecurringSales).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.DefaultNotes).IsModified = true;
                oConnectionContext.Entry(oClsSaleSettings).Property(x => x.DefaultTerms).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Sale\" updated",
                    Id = oClsSaleSettings.SaleSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Sale Info updated successfully.",
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
