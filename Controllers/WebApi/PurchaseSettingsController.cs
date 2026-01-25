using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Threading.Tasks;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class PurchaseSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        dynamic data = null;

        public async Task<IHttpActionResult> PurchaseSetting(ClsPurchaseSettingsVm obj)
        {
            var det = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                PurchaseSettingsId = a.PurchaseSettingsId,
                EnableEditingProductPrice = a.EnableEditingProductPrice,
                EnablePurchaseStatus = a.EnablePurchaseStatus,
                a.EnablePurchaseQuotation,
                EnablePurchaseOrder = a.EnablePurchaseOrder,
                EnableSms = a.EnableSms,
                a.EnableEmail,
                a.EnableWhatsapp,
                a.EnableFreeQuantity,
                a.EnableRoundOff,
                a.AutoPrintInvoicePurchaseQuotation,
                a.AutoPrintInvoicePurchaseOrder,
                a.AutoPrintInvoicePurchaseBill,
                a.AutoPrintInvoicePurchaseReturn,
                a.DiscountAccountId,
                a.RoundOffAccountId,
                a.PaymentTermId,
                a.EnableSpecialDiscount,
                a.SpecialDiscountAccountId
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseSettingsUpdate(ClsPurchaseSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PurchaseSettingsId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.PurchaseSettingsId).FirstOrDefault();

                ClsPurchaseSettings oClsPurchaseSettings = new ClsPurchaseSettings()
                {
                    PurchaseSettingsId = PurchaseSettingsId,
                    EnableEditingProductPrice = obj.EnableEditingProductPrice,
                    EnablePurchaseStatus = obj.EnablePurchaseStatus,
                    EnablePurchaseQuotation = obj.EnablePurchaseQuotation,
                    EnablePurchaseOrder = obj.EnablePurchaseOrder,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    EnableSms = obj.EnableSms,
                    EnableEmail = obj.EnableEmail,
                    EnableWhatsapp = obj.EnableWhatsapp,
                    EnableFreeQuantity= obj.EnableFreeQuantity,
                    EnableRoundOff = obj.EnableRoundOff,
                    AutoPrintInvoicePurchaseQuotation = obj.AutoPrintInvoicePurchaseQuotation,
                    AutoPrintInvoicePurchaseOrder = obj.AutoPrintInvoicePurchaseOrder,
                    AutoPrintInvoicePurchaseBill = obj.AutoPrintInvoicePurchaseBill,
                    AutoPrintInvoicePurchaseReturn = obj.AutoPrintInvoicePurchaseReturn,
                    DiscountAccountId = obj.DiscountAccountId,
                    RoundOffAccountId = obj.RoundOffAccountId,
                    //VendorAdvanceAccountId = obj.VendorAdvanceAccountId,
                    PaymentTermId = obj.PaymentTermId,
                    EnableSpecialDiscount = obj.EnableSpecialDiscount,
                    SpecialDiscountAccountId = obj.SpecialDiscountAccountId,
                };

                oConnectionContext.DbClsPurchaseSettings.Attach(oClsPurchaseSettings);
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.PurchaseSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnableEditingProductPrice).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnablePurchaseStatus).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnablePurchaseQuotation).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnablePurchaseOrder).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnableSms).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnableEmail).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnableWhatsapp).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnableFreeQuantity).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnableRoundOff).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.AutoPrintInvoicePurchaseQuotation).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.AutoPrintInvoicePurchaseOrder).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.AutoPrintInvoicePurchaseBill).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.AutoPrintInvoicePurchaseReturn).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.DiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.RoundOffAccountId).IsModified = true;
                //oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.VendorAdvanceAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.EnableSpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseSettings).Property(x => x.SpecialDiscountAccountId).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Purchase\" updated",
                    Id = oClsPurchaseSettings.PurchaseSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Info updated successfully.",
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
