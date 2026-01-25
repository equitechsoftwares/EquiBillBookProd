using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using Vonage.Pricing;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class PurchaseReturnController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();

        public async Task<IHttpActionResult> AllPurchaseReturns(ClsPurchaseVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //    oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //      && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
            //}).FirstOrDefault();

            //if (obj.BranchId == 0)
            //{
            //    obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
            //}

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

            List<ClsPurchaseReturnVm> det;
            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsPurchaseReturn
                           //                    join b in oConnectionContext.DbClsPurchase
                           //on a.PurchaseId equals b.PurchaseId
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                       select new ClsPurchaseReturnVm
                       {
                           IsReverseCharge = a.IsReverseCharge,
                           NetAmountReverseCharge = a.NetAmountReverseCharge,
                           RoundOffReverseCharge = a.RoundOffReverseCharge,
                           GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                           PurchaseDebitNoteReasonId = a.PurchaseDebitNoteReasonId,
                           IsCancelled = a.IsCancelled,
                           SupplierPaymentId = oConnectionContext.DbClsSupplierPayment.Where(b => b.PurchaseReturnId == a.PurchaseReturnId && b.IsDeleted == false && b.IsCancelled == false
                           && b.IsActive == true && b.CompanyId == obj.CompanyId).Select(b => b.SupplierPaymentId).FirstOrDefault(),
                           AmountRemaining = oConnectionContext.DbClsSupplierPayment.Where(b => b.PurchaseReturnId == a.PurchaseReturnId && b.IsDeleted == false && b.IsCancelled == false
                           && b.IsActive == true && b.CompanyId == obj.CompanyId).Select(b => b.AmountRemaining).FirstOrDefault(),
                           TotalTaxAmount = a.TotalTaxAmount,
                           InvoiceId = a.InvoiceId,
                           IsDirectReturn = a.IsDirectReturn,
                           BranchId = a.BranchId,
                           InvoiceUrl = oCommonController.webUrl,// + "/purchase/PurchaseReturnInvoice?InvoiceNo=" + a.InvoiceNo+"&Id="+a.CompanyId,
                           PurchaseId = a.PurchaseId,
                           Branch = oConnectionContext.DbClsBranch.Where(bb => bb.BranchId == a.BranchId).Select(bb => bb.Branch).FirstOrDefault(),
                           PurchaseReturnId = a.PurchaseReturnId,
                           GrandTotal = a.GrandTotal,
                           Notes = a.Notes,
                           //OtherCharges = b.OtherCharges,
                           Date = a.Date,
                           PurchaseInvoiceNo = oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.ReferenceNo).FirstOrDefault(),
                           InvoiceNo = a.InvoiceNo,
                           Subtotal = a.Subtotal,
                           SupplierId = a.SupplierId,
                           SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                           SupplierMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.MobileNo).FirstOrDefault(),
                           CompanyId = a.CompanyId,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           //        Paid = oConnectionContext.DbClsSupplierRefund.Where(bb => bb.Type.ToLower() == "supplier refund" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.PurchaseReturnId == a.PurchaseReturnId).Count() == 0 ? 0 :
                           //oConnectionContext.DbClsSupplierRefund.Where(bb => bb.Type.ToLower() == "supplier refund" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.PurchaseReturnId == a.PurchaseReturnId).Select(bb => bb.Amount).DefaultIfEmpty().Sum(),
                           //PaymentStatus = a.PaymentStatus,
                           //        Due = oConnectionContext.DbClsSupplierRefund.Where(bb => bb.Type.ToLower() == "supplier refund" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.PurchaseReturnId == a.PurchaseReturnId).Count() == 0 ? a.GrandTotal :
                           //a.GrandTotal - oConnectionContext.DbClsSupplierRefund.Where(bb => bb.Type.ToLower() == "supplier refund" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.PurchaseReturnId == a.PurchaseReturnId).Select(bb => bb.Amount).DefaultIfEmpty().Sum(),
                           TotalQuantity = a.TotalQuantity,
                           PaidQuantity = oConnectionContext.DbClsPurchaseReturnDetails.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                           FreeQuantity = oConnectionContext.DbClsPurchaseReturnDetails.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                           ReferenceNo = a.InvoiceNo,
                           Status = a.Status,
                           TotalItems = oConnectionContext.DbClsPurchaseReturnDetails.Where(c => c.PurchaseReturnId == a.PurchaseReturnId &&
                    c.IsDeleted == false).Count()
                       }).ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsPurchaseReturn
                           //                    join b in oConnectionContext.DbClsPurchase
                           //on a.PurchaseId equals b.PurchaseId
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                        && a.BranchId == obj.BranchId
                       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                       select new ClsPurchaseReturnVm
                       {
                           IsReverseCharge = a.IsReverseCharge,
                           NetAmountReverseCharge = a.NetAmountReverseCharge,
                           RoundOffReverseCharge = a.RoundOffReverseCharge,
                           GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                           PurchaseDebitNoteReasonId = a.PurchaseDebitNoteReasonId,
                           IsCancelled = a.IsCancelled,
                           SupplierPaymentId = oConnectionContext.DbClsSupplierPayment.Where(b => b.PurchaseReturnId == a.PurchaseReturnId && b.IsDeleted == false && b.IsCancelled == false
&& b.IsActive == true && b.CompanyId == obj.CompanyId).Select(b => b.SupplierPaymentId).FirstOrDefault(),
                           AmountRemaining = oConnectionContext.DbClsSupplierPayment.Where(b => b.PurchaseReturnId == a.PurchaseReturnId && b.IsDeleted == false && b.IsCancelled == false
                           && b.IsActive == true && b.CompanyId == obj.CompanyId).Select(b => b.AmountRemaining).FirstOrDefault(),
                           TotalTaxAmount = a.TotalTaxAmount,
                           InvoiceId = a.InvoiceId,
                           IsDirectReturn = a.IsDirectReturn,
                           BranchId = a.BranchId,
                           InvoiceUrl = oCommonController.webUrl,// + "/purchase/PurchaseReturnInvoice?InvoiceNo=" + a.InvoiceNo + "&Id=" + a.CompanyId,
                           PurchaseId = a.PurchaseId,
                           Branch = oConnectionContext.DbClsBranch.Where(bb => bb.BranchId == a.BranchId).Select(bb => bb.Branch).FirstOrDefault(),
                           PurchaseReturnId = a.PurchaseReturnId,
                           GrandTotal = a.GrandTotal,
                           Notes = a.Notes,
                           //OtherCharges = b.OtherCharges,
                           Date = a.Date,
                           PurchaseInvoiceNo = oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.ReferenceNo).FirstOrDefault(),
                           InvoiceNo = a.InvoiceNo,
                           Subtotal = a.Subtotal,
                           SupplierId = a.SupplierId,
                           SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                           SupplierMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.MobileNo).FirstOrDefault(),
                           CompanyId = a.CompanyId,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           //        Paid = oConnectionContext.DbClsSupplierRefund.Where(bb => bb.Type.ToLower() == "supplier refund" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.PurchaseReturnId == a.PurchaseReturnId).Count() == 0 ? 0 :
                           //oConnectionContext.DbClsSupplierRefund.Where(bb => bb.Type.ToLower() == "supplier refund" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.PurchaseReturnId == a.PurchaseReturnId).Select(bb => bb.Amount).DefaultIfEmpty().Sum(),
                           //        //PaymentStatus = a.PaymentStatus,
                           //        Due = oConnectionContext.DbClsSupplierRefund.Where(bb => bb.Type.ToLower() == "supplier refund" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.PurchaseReturnId == a.PurchaseReturnId).Count() == 0 ? a.GrandTotal :
                           //a.GrandTotal - oConnectionContext.DbClsSupplierRefund.Where(bb => bb.Type.ToLower() == "supplier refund" && bb.IsDeleted == false && bb.IsCancelled == false && b.IsCancelled == false && bb.PurchaseReturnId == a.PurchaseReturnId).Select(bb => bb.Amount).DefaultIfEmpty().Sum(),
                           TotalQuantity = a.TotalQuantity,
                           PaidQuantity = oConnectionContext.DbClsPurchaseReturnDetails.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.IsDeleted == false).Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                           FreeQuantity = oConnectionContext.DbClsPurchaseReturnDetails.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.IsDeleted == false).Select(c => c.FreeQuantity).DefaultIfEmpty().Sum(),
                           ReferenceNo = a.InvoiceNo,
                           Status = a.Status,
                           TotalItems = oConnectionContext.DbClsPurchaseReturnDetails.Where(c => c.PurchaseReturnId == a.PurchaseReturnId &&
                    c.IsDeleted == false).Count()
                       }).ToList();
            }

            if (obj.ReferenceNo != null && obj.ReferenceNo != "")
            {
                det = det.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower()).Select(a => a).ToList();
            }

            if (obj.Status != "" && obj.Status != null)
            {
                det = det.Where(a => a.Status == obj.Status).Select(a => a).ToList();
            }

            //if (obj.PaymentStatus != null && obj.PaymentStatus != "")
            //{
            //    if (obj.PaymentStatus.ToLower() == "due")
            //    {
            //        det = det.Where(a => a.PaymentStatus.ToLower() == "partially paid" || a.PaymentStatus.ToLower() == "due" ||
            //        a.PaymentStatus.ToLower() == "overdue").Select(a => a).ToList();
            //    }
            //    else if (obj.PaymentStatus.ToLower() == "payment")
            //    {
            //        det = det.Where(a => a.PaymentStatus.ToLower() == "partially paid" || a.PaymentStatus.ToLower() == "paid").Select(a => a).ToList();
            //    }
            //    else
            //    {
            //        det = det.Where(a => a.PaymentStatus.ToLower() == obj.PaymentStatus.ToLower()).Select(a => a).ToList();
            //    }
            //}

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.SupplierId == obj.SupplierId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseReturns = det.OrderByDescending(a => a.PurchaseReturnId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseReturn(ClsPurchaseReturnVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            // if (obj.PurchaseReturnId == 0)
            // {
            //     obj.PurchaseReturnId = oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseId == obj.PurchaseId && a.CompanyId == obj.CompanyId && a.IsActive == true &&
            //a.IsDeleted == false && a.IsCancelled == false).Select(a => a.PurchaseReturnId).FirstOrDefault();
            // }

            if (obj.PurchaseReturnId == 0)
            {
                var det = (from bb in oConnectionContext.DbClsPurchase
                           where bb.PurchaseId == obj.PurchaseId && bb.CompanyId == obj.CompanyId
                           select new
                           {
                               bb.TaxableAmount,
                               bb.Discount,
                               bb.DiscountType,
                               bb.TotalDiscount,
                               bb.IsReverseCharge,
                               bb.IsCancelled,
                               bb.SourceOfSupplyId,
                               bb.DestinationOfSupplyId,
                               bb.TotalTaxAmount,
                               CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == bb.SupplierId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                               bb.PurchaseId,
                               bb.PurchaseDate,
                               PurchaseInvoiceNo = bb.ReferenceNo,
                               bb.SupplierId,
                               bb.BranchId,
                               Branch = oConnectionContext.DbClsBranch.Where(z => z.BranchId == bb.BranchId).Select(z => z.Branch).FirstOrDefault(),
                               SupplierName = oConnectionContext.DbClsUser.Where(z => z.UserId == bb.SupplierId).Select(z => z.Name).FirstOrDefault(),
                               PurchaseReturnId = 0,
                               SmsSettingsId = bb.SmsSettingsId,
                               EmailSettingsId = bb.EmailSettingsId,
                               WhatsappSettingsId = bb.WhatsappSettingsId,
                               DueDate = CurrentDate,
                               PaymentTermId = oConnectionContext.DbClsPaymentTerm.Where(z => z.CompanyId == obj.CompanyId && z.IsDueUponReceipt == true).Select(z => z.PaymentTermId).FirstOrDefault(),
                               PurchaseDetails = (from b in oConnectionContext.DbClsPurchaseDetails
                                                  join c in oConnectionContext.DbClsItemDetails
                                                  on b.ItemDetailsId equals c.ItemDetailsId
                                                  join d in oConnectionContext.DbClsItem
                                                  on c.ItemId equals d.ItemId
                                                  where b.PurchaseId == obj.PurchaseId && b.IsDeleted == false
                                                  select new
                                                  {
                                                      b.DiscountType,
                                                      d.IsManageStock,
                                                      b.ExtraDiscount,
                                                      b.PurchaseDetailsId,
                                                      QuantityReturned = 0,
                                                      QuantityReturnedPriceAddedFor = b.PriceAddedFor,
                                                      PurchaseReturnUnitCost = b.PurchaseIncTax,
                                                      FreeQuantityReturned = 0,
                                                      //FreeQuantityReturnedPriceAddedFor = b.FreeQuantityPriceAddedFor,
                                                      b.QuantityRemaining,
                                                      PurchaseReturnPrice = b.PurchaseIncTax,
                                                      AmountExcTax = 0,
                                                      AmountIncTax = 0,
                                                      b.Discount,
                                                      Quantity = b.Quantity,
                                                      b.FreeQuantity,
                                                      //b.FreeQuantityPriceAddedFor,
                                                      b.TaxId,
                                                      b.UnitCost,
                                                      d.ItemId,
                                                      d.ProductType,
                                                      c.ItemDetailsId,
                                                      d.ItemName,
                                                      SKU = d.ProductType == "Single" ? d.SkuCode : c.SKU,
                                                      c.VariationDetailsId,
                                                      VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                                      b.PurchaseExcTax,
                                                      b.PurchaseIncTax,
                                                      c.TotalCost,
                                                      Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                                                      TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                                                      d.TaxType,
                                                      d.ItemCode,
                                                      UnitId = d.UnitId,
                                                      SecondaryUnitId = d.SecondaryUnitId,
                                                      TertiaryUnitId = d.TertiaryUnitId,
                                                      QuaternaryUnitId = d.QuaternaryUnitId,
                                                      UnitShortName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitShortName).FirstOrDefault(),
                                                      SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitShortName).FirstOrDefault(),
                                                      TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitShortName).FirstOrDefault(),
                                                      QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitShortName).FirstOrDefault(),
                                                      UToSValue = d.UToSValue,
                                                      SToTValue = d.SToTValue,
                                                      TToQValue = d.TToQValue,
                                                      AllowDecimal = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.AllowDecimal).FirstOrDefault(),
                                                      SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                                      TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                                      QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                                      PriceAddedFor = b.PriceAddedFor,
                                                  }).ToList(),
                            PurchaseReturnAdditionalCharges = oConnectionContext.DbClsAdditionalCharge.Where(b => b.IsDeleted == false && b.IsActive == true && b.CompanyId == obj.CompanyId
                                ).Select(b => new ClsPurchaseReturnAdditionalChargesVm
                                {
                                    ITCType = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == bb.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.ITCType).FirstOrDefault(),
                                    PurchaseReturnAdditionalChargesId = 0,
                                    Name = b.Name,
                                    AdditionalChargeId = b.AdditionalChargeId,
                                    PurchaseReturnId =0,
                                    TaxId = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == bb.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.TaxId).FirstOrDefault(),
                                    AmountExcTax = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == bb.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).FirstOrDefault(),
                                    AmountIncTax = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == bb.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountIncTax).FirstOrDefault(),
                                    TaxAmount = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == bb.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => (c.AmountIncTax - c.AmountExcTax)).FirstOrDefault(),
                                    AccountId = oConnectionContext.DbClsPurchaseAdditionalCharges.Where(c => c.PurchaseId == bb.PurchaseId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AccountId).FirstOrDefault(),
                                    ItemCodeId = b.ItemCodeId,
                                    TaxExemptionId = b.TaxExemptionId,
                                    TaxExemptionReason = oConnectionContext.DbClsTaxExemption.Where(c => c.TaxExemptionId == b.TaxExemptionId).Select(c => c.Reason).FirstOrDefault(),
                                }).ToList(),
                           }).FirstOrDefault();

                var AllTaxs = oConnectionContext.DbClsPurchase.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.PurchaseId == det.PurchaseId).Select(a => new
                {
                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                    a.TaxId,
                    AmountExcTax = a.Subtotal - a.TotalDiscount
                }).Concat(oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseId == det.PurchaseId && a.IsDeleted == false).Select(a => new
                {
                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                    a.TaxId,
                    AmountExcTax = a.AmountExcTax
                })).Concat(oConnectionContext.DbClsPurchaseAdditionalCharges.Where(a => a.PurchaseId == det.PurchaseId
                                    && a.IsDeleted == false).Select(a => new
                                    {
                                        IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                        a.TaxId,
                                        AmountExcTax = a.AmountExcTax
                                    })).ToList();

                List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                foreach (var item in AllTaxs)
                {
                    bool CanDelete = oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => a.CanDelete).FirstOrDefault();
                    if (CanDelete == true)
                    {
                        decimal AmountExcTax = item.AmountExcTax;
                        var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                        {
                            a.TaxId,
                            a.Tax,
                            a.TaxPercent,
                        }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                       where a.TaxId == item.TaxId
                                       select new
                                       {
                                           TaxId = a.SubTaxId,
                                           Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                           TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                       }).ToList();

                        foreach (var tax in taxs)
                        {
                            oClsTaxVm.Add(new ClsTaxVm
                            {
                                TaxId = tax.TaxId,
                                Tax = tax.Tax,
                                TaxPercent = tax.TaxPercent,
                                TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                            });
                        }
                    }
                }

                var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                         (k, c) => new
                         {
                             //TaxId = k,
                             Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                             TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                             TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                         }
                        ).ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        PurchaseReturn = det,
                        Taxs = finalTaxs,
                    }
                };
            }
            else
            {
                //obj.IsDirectReturn = oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId).Select(a => a.IsDirectReturn).FirstOrDefault();

                //if(obj.IsDirectReturn == true)
                //{

                //}

                var det = (from a in oConnectionContext.DbClsPurchaseReturn
                               //                       join b in oConnectionContext.DbClsPurchase
                               //on a.PurchaseId equals b.PurchaseId
                           where a.PurchaseReturnId == obj.PurchaseReturnId && a.CompanyId == obj.CompanyId
                           select new
                           {
                               a.TaxableAmount,
                               a.IsReverseCharge,
                               NetAmountReverseCharge = a.NetAmountReverseCharge,
                               RoundOffReverseCharge = a.RoundOffReverseCharge,
                               GrandTotalReverseCharge = a.GrandTotalReverseCharge,
                               a.PurchaseDebitNoteReasonId,
                               IsCancelled = a.IsCancelled,
                               a.SourceOfSupplyId,
                               a.DestinationOfSupplyId,
                               a.TotalTaxAmount,
                               a.InvoiceId,
                               a.IsDirectReturn,
                               a.RoundOff,
                               SpecialDiscount = a.SpecialDiscount,
                               a.NetAmount,
                               CurrencySymbol = oConnectionContext.DbClsCurrency.Where(bb => bb.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == a.SupplierId).Select(c => c.CurrencyId).FirstOrDefault()).Select(bb => bb.CurrencySymbol).FirstOrDefault(),
                               a.Date,
                               a.PurchaseReturnId,
                               PurchaseInvoiceNo = oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.ReferenceNo).FirstOrDefault(),
                               a.SupplierId,
                               a.BranchId,
                               Branch = oConnectionContext.DbClsBranch.Where(bb => bb.BranchId == a.BranchId).Select(bb => bb.Branch).FirstOrDefault(),
                               SupplierName = oConnectionContext.DbClsUser.Where(bb => bb.UserId == a.SupplierId).Select(bb => bb.Name).FirstOrDefault(),
                               a.Status,
                               //a.PayTerm,
                               //a.PayTermNo,
                               PaymentTermId = a.PaymentTermId,
                               DueDate = a.DueDate,
                               a.AttachDocument,
                               PurchaseId = a.PurchaseId,
                               a.GrandTotal,
                               a.TotalDiscount,
                               a.TotalQuantity,
                               a.Discount,
                               a.DiscountType,
                               a.Notes,
                               PurchaseDate = oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.PurchaseDate).FirstOrDefault(),
                               a.InvoiceNo,
                               a.Subtotal,
                               CompanyId = a.CompanyId,
                               IsActive = a.IsActive,
                               IsDeleted = a.IsDeleted,
                               AddedBy = a.AddedBy,
                               AddedOn = a.AddedOn,
                               ModifiedBy = a.ModifiedBy,
                               ModifiedOn = a.ModifiedOn,
                               a.SmsSettingsId,
                               a.EmailSettingsId,
                               a.WhatsappSettingsId,
                               a.TaxId,
                               a.TaxAmount,
                               a.ReturnType,
                               PurchaseDetails = (from z in oConnectionContext.DbClsPurchaseReturnDetails
                                                      //join b in oConnectionContext.DbClsPurchaseDetails
                                                      //on z.PurchaseDetailsId equals b.PurchaseDetailsId
                                                  join c in oConnectionContext.DbClsItemDetails
                                                  on z.ItemDetailsId equals c.ItemDetailsId
                                                  join d in oConnectionContext.DbClsItem
                                                  on c.ItemId equals d.ItemId
                                                  where z.PurchaseReturnId == a.PurchaseReturnId && z.IsDeleted == false
                                                  //&& b.IsDeleted == false && b.IsCancelled == false
                                                  select new
                                                  {
                                                      z.TotalTaxAmount,
                                                      z.DiscountType,
                                                      z.ITCType,
                                                      d.IsManageStock,
                                                      z.ExtraDiscount,
                                                      Unit = z.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(e => e.UnitId == d.UnitId).Select(e => e.UnitShortName).FirstOrDefault()
                                    : z.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(e => e.SecondaryUnitId == d.SecondaryUnitId).Select(e => e.SecondaryUnitShortName).FirstOrDefault()
                                    : z.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(e => e.TertiaryUnitId == d.TertiaryUnitId).Select(e => e.TertiaryUnitShortName).FirstOrDefault()
                                    : oConnectionContext.DbClsQuaternaryUnit.Where(e => e.QuaternaryUnitId == d.QuaternaryUnitId).Select(e => e.QuaternaryUnitShortName).FirstOrDefault(),
                                                      z.PurchaseDetailsId,
                                                      QuantityRemaining = a.Status.ToLower() != "draft" ?
                                                      oConnectionContext.DbClsPurchaseDetails.Where(e => e.PurchaseDetailsId == z.PurchaseDetailsId).Select(e => e.QuantityRemaining).FirstOrDefault() +
                                                      z.QuantityRemaining : oConnectionContext.DbClsPurchaseDetails.Where(e => e.PurchaseDetailsId == z.PurchaseDetailsId).Select(e => e.QuantityRemaining).FirstOrDefault(),
                                                      QuantityReturned = z.Quantity,
                                                      QuantityReturnedPriceAddedFor = z.PriceAddedFor,
                                                      //PurchaseReturnUnitCost = z.UnitCost,
                                                      //PurchaseReturnAmount = z.Quantity * z.UnitCost,
                                                      FreeQuantityReturned = z.FreeQuantity,
                                                      PurchaseReturnDetailsId = z.PurchaseReturnDetailsId,
                                                      z.AmountExcTax,
                                                      z.AmountIncTax,
                                                      z.Discount,
                                                      Quantity = oConnectionContext.DbClsPurchaseDetails.Where(e => e.PurchaseDetailsId == z.PurchaseDetailsId).Select(e => e.Quantity).FirstOrDefault(),
                                                      z.TaxId,
                                                      z.UnitCost,
                                                      d.ItemId,
                                                      d.ProductType,
                                                      c.ItemDetailsId,
                                                      d.ItemName,
                                                      SKU = d.ProductType == "Single" ? d.SkuCode : c.SKU,
                                                      c.VariationDetailsId,
                                                      VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                                      z.PurchaseExcTax,
                                                      z.PurchaseIncTax,
                                                      c.TotalCost,
                                                      z.TaxAmount,
                                                      Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == z.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                                                      TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == z.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                                                      d.TaxType,
                                                      d.ItemCode,
                                                      UnitId = d.UnitId,
                                                      SecondaryUnitId = d.SecondaryUnitId,
                                                      TertiaryUnitId = d.TertiaryUnitId,
                                                      QuaternaryUnitId = d.QuaternaryUnitId,
                                                      UnitShortName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitShortName).FirstOrDefault(),
                                                      SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitShortName).FirstOrDefault(),
                                                      TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitShortName).FirstOrDefault(),
                                                      QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitShortName).FirstOrDefault(),
                                                      UToSValue = d.UToSValue,
                                                      SToTValue = d.SToTValue,
                                                      TToQValue = d.TToQValue,
                                                      AllowDecimal = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.AllowDecimal).FirstOrDefault(),
                                                      SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                                      TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                                      QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                                  }).ToList(),
                PurchaseReturnAdditionalCharges = oConnectionContext.DbClsAdditionalCharge.Where(b => b.IsDeleted == false && b.IsActive == true && b.CompanyId == obj.CompanyId
                    ).Select(b => new ClsPurchaseReturnAdditionalChargesVm
                    {
                        ITCType = oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.ITCType).FirstOrDefault(),
                        PurchaseReturnAdditionalChargesId = oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.PurchaseReturnAdditionalChargesId).FirstOrDefault(),
                        Name = b.Name,
                        AdditionalChargeId = b.AdditionalChargeId,
                        PurchaseReturnId = a.PurchaseReturnId,
                        TaxId = oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.TaxId).FirstOrDefault(),
                        AmountExcTax = oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).FirstOrDefault(),
                        AmountIncTax = oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountIncTax).FirstOrDefault(),
                        TaxAmount = oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => (c.AmountIncTax - c.AmountExcTax)).FirstOrDefault(),
                        AccountId = oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Where(c => c.PurchaseReturnId == a.PurchaseReturnId && c.AdditionalChargeId == b.AdditionalChargeId && c.IsDeleted == false && c.IsActive == true).Select(c => c.AccountId).FirstOrDefault(),
                        ItemCodeId = b.ItemCodeId,
                        TaxExemptionId = b.TaxExemptionId,
                        TaxExemptionReason = oConnectionContext.DbClsTaxExemption.Where(c => c.TaxExemptionId == b.TaxExemptionId).Select(c => c.Reason).FirstOrDefault(),
                    }).ToList(),
                           }).FirstOrDefault();

                var AllTaxs = oConnectionContext.DbClsPurchaseReturn.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.PurchaseReturnId == det.PurchaseReturnId).Select(a => new
                {
                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                    a.TaxId,
                    AmountExcTax = a.Subtotal - a.TotalDiscount
                }).Concat(oConnectionContext.DbClsPurchaseReturnDetails.Where(a => a.PurchaseReturnId == det.PurchaseReturnId && a.IsDeleted == false).Select(a => new
                {
                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                    a.TaxId,
                    AmountExcTax = a.AmountExcTax
                })).Concat(oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Where(a => a.PurchaseReturnId == det.PurchaseReturnId
                                && a.IsDeleted == false && a.AmountExcTax > 0).Select(a => new
                                {
                                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                    a.TaxId,
                                    AmountExcTax = a.AmountExcTax
                                })).ToList();

                List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                foreach (var item in AllTaxs)
                {
                    bool CanDelete = oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => a.CanDelete).FirstOrDefault();
                    if (CanDelete == true)
                    {
                        decimal AmountExcTax = item.AmountExcTax;
                        var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                        {
                            a.TaxId,
                            a.Tax,
                            a.TaxPercent,
                        }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                       where a.TaxId == item.TaxId
                                       select new
                                       {
                                           TaxId = a.SubTaxId,
                                           Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                           TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                       }).ToList();

                        foreach (var tax in taxs)
                        {
                            oClsTaxVm.Add(new ClsTaxVm
                            {
                                TaxId = tax.TaxId,
                                Tax = tax.Tax,
                                TaxPercent = tax.TaxPercent,
                                TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                            });
                        }
                    }
                }

                var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                         (k, c) => new
                         {
                             //TaxId = k,
                             Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                             TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                             TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                         }
                        ).ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        PurchaseReturn = det,
                        Taxs = finalTaxs,
                    }
                };
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPurchaseReturn(ClsPurchaseReturnVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                long PrefixUserMapId = 0;
                long CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.SupplierId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSupplier" });
                    isError = true;
                }

                if (obj.SupplierId != 0)
                {
                    if (CountryId == 2)
                    {
                        string GstTreatment = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.GstTreatment).FirstOrDefault();
                        if (GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.SourceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSourceOfSupply" });
                                isError = true;
                            }
                        }

                        if (obj.DestinationOfSupplyId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divDestinationOfSupply" });
                            isError = true;
                        }
                    }
                }

                if (obj.Date == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDate" });
                    isError = true;
                }

                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divStatus" });
                    isError = true;
                }

                if (obj.PurchaseId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseInvoice" });
                    isError = true;
                }

                if (obj.PurchaseDebitNoteReasonId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseDebitNoteReason" });
                    isError = true;
                }

                if (obj.InvoiceNo != "" && obj.InvoiceNo != null)
                {
                    if (oConnectionContext.DbClsPurchaseReturn.Where(a => a.InvoiceNo.ToLower() == obj.InvoiceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Purchase Return# exists", Id = "divReferenceNo" });
                        isError = true;
                    }
                }

                if (obj.PurchaseReturnDetails == null || obj.PurchaseReturnDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.PurchaseReturnDetails != null)
                {
                    foreach (var item in obj.PurchaseReturnDetails)
                    {
                        if (item.IsDeleted == false)
                        {
                            if (item.Quantity <= 0)
                            {
                                errors.Add(new ClsError { Message = "", Id = "divQuantity" + item.DivId });
                                isError = true;
                            }
                            if (item.UnitCost <= 0)
                            {
                                errors.Add(new ClsError { Message = "", Id = "divUnitCost" + item.DivId });
                                isError = true;
                            }
                        }
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

                long PrefixId = 0;
                if (obj.InvoiceNo == "" || obj.InvoiceNo == null)
                {
                    // Hybrid approach: Check Supplier PrefixId first, then fall back to Branch PrefixId
                    long supplierPrefixId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => a.PrefixId).FirstOrDefault();
                    
                    if (supplierPrefixId != 0)
                    {
                        // Use Supplier's PrefixId if set
                        PrefixId = supplierPrefixId;
                    }
                    else
                    {
                        // Fall back to Branch PrefixId
                        PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    }
                    
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == "debit note"
                                          && b.PrefixId == PrefixId
                                          select new
                                          {
                                              b.PrefixUserMapId,
                                              b.Prefix,
                                              b.NoOfDigits,
                                              b.Counter
                                          }).FirstOrDefault();
                    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    obj.InvoiceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }

                bool isItemAdded = false;
                if (obj.PurchaseReturnDetails != null)
                {
                    foreach (var PurchaseReturn in obj.PurchaseReturnDetails)
                    {
                        isItemAdded = true;
                        if (PurchaseReturn.PurchaseReturnDetailsId == 0)
                        {
                            if (PurchaseReturn.Quantity != 0)
                            {
                                //decimal QuantityRemaining = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseId == obj.PurchaseId
                                //&& a.ItemId == PurchaseReturn.ItemId && a.ItemDetailsId == PurchaseReturn.ItemDetailsId).Select(a => a.QuantityRemaining).FirstOrDefault();

                                decimal remainingQty = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == PurchaseReturn.PurchaseDetailsId).Select(a => a.QuantityRemaining).FirstOrDefault();

                                decimal convertedStock = 0;
                                foreach (var inner in obj.PurchaseReturnDetails)
                                {
                                    bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == PurchaseReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                    if (IsManageStock_Inner == true)
                                    {
                                        if (PurchaseReturn.PurchaseDetailsId == inner.PurchaseDetailsId)
                                        {
                                            convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
                                        }
                                    }
                                }
                                if (remainingQty < convertedStock)
                                {
                                    //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+Sales.DivId });
                                    errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantityReturned" + PurchaseReturn.DivId });
                                    isError = true;
                                }

                                //decimal convertedStock = oCommonController.StockConversion(PurchaseReturn.Quantity + PurchaseReturn.FreeQuantity, PurchaseReturn.ItemId, PurchaseReturn.PriceAddedFor);

                                //if (convertedStock > (QuantityRemaining))
                                //{
                                //    data = new
                                //    {
                                //        Status = 0,
                                //        Message = "Only " + QuantityRemaining + "items is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == PurchaseReturn.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                                //        Data = new
                                //        {
                                //        }
                                //    };
                                //    return await Task.FromResult(Ok(data));
                                //}
                            }
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
                }

                if (isItemAdded == false)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Add return quantity first",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.PurchaseId != 0)
                {
                    var purchase = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => new
                    {
                        a.BranchId,
                        a.SupplierId
                    }).FirstOrDefault();

                    obj.SupplierId = purchase.SupplierId;
                    obj.BranchId = purchase.BranchId;
                }

                long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
           && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();

                long DiscountAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DiscountAccountId).FirstOrDefault();
                long RoundOffAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.RoundOffAccountId).FirstOrDefault();
                long SpecialDiscountAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.SpecialDiscountAccountId).FirstOrDefault();
                long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();

                var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    a.IsBusinessRegistered,
                    a.GstTreatment,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo
                }).FirstOrDefault();

                ClsPurchaseReturn oClsPurchaseReturn = new ClsPurchaseReturn()
                {
                    TotalTaxAmount = obj.TotalTaxAmount,
                    TotalDiscount = obj.TotalDiscount,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    Discount = obj.Discount,
                    DiscountType = obj.DiscountType,
                    GrandTotal = obj.IsReverseCharge == 1 ? obj.GrandTotalReverseCharge : obj.GrandTotal,
                    TaxableAmount = obj.GrandTotal,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    Notes = obj.Notes,
                    //PayTerm = obj.PayTerm,
                    //PayTermNo = obj.PayTermNo,
                    PaymentTermId = obj.PaymentTermId,
                    DueDate = obj.Date.AddHours(5).AddMinutes(30),//obj.DueDate,
                    PurchaseReturnId = obj.PurchaseReturnId,
                    Subtotal = obj.Subtotal,
                    TotalQuantity = obj.TotalQuantity,
                    Status = obj.Status,
                    Date = obj.Date.AddHours(5).AddMinutes(30),
                    InvoiceNo = obj.InvoiceNo,
                    PurchaseId = obj.PurchaseId,
                    SmsSettingsId = obj.SmsSettingsId,
                    EmailSettingsId = obj.EmailSettingsId,
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    TaxId = obj.TaxId,
                    TaxAmount = obj.TaxAmount,
                    BranchId = obj.BranchId,
                    SupplierId = obj.SupplierId,
                    ReturnType = obj.ReturnType,
                    ExpiredBefore = obj.ExpiredBefore,
                    QuantityLessThan = obj.QuantityLessThan,
                    IsDirectReturn = obj.IsDirectReturn,
                    InvoiceId = oCommonController.CreateToken(),
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    AccountId = AccountId,
                    DiscountAccountId = DiscountAccountId,
                    //OtherChargesAccountId = OtherChargesAccountId,
                    RoundOffAccountId = RoundOffAccountId,
                    TaxAccountId = TaxAccountId,
                    SourceOfSupplyId = obj.SourceOfSupplyId,
                    DestinationOfSupplyId = obj.DestinationOfSupplyId,
                    IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = userDet.GstTreatment,
                    BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                    BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                    BusinessLegalName = userDet.BusinessLegalName,
                    BusinessTradeName = userDet.BusinessTradeName,
                    PanNo = userDet.PanNo,
                    IsCancelled = false,
                    PrefixId = PrefixId,
                    PurchaseDebitNoteReasonId = obj.PurchaseDebitNoteReasonId,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    IsReverseCharge = obj.IsReverseCharge,
                    SpecialDiscountAccountId = SpecialDiscountAccountId
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/PurchaseReturn/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);


                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/PurchaseReturn/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsPurchaseReturn.AttachDocument = filepathPass;
                }

                oConnectionContext.DbClsPurchaseReturn.Add(oClsPurchaseReturn);
                oConnectionContext.SaveChanges();

                if (obj.PurchaseReturnAdditionalCharges != null)
                {
                    foreach (var item in obj.PurchaseReturnAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                        a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.PurchaseAccountId }).FirstOrDefault();

                        string AccountType = "";

                        var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == item.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                        List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                        //decimal AmountExcTax = obj.IsReverseCharge == 1 ? PurchaseReturn.AmountIncTax : PurchaseReturn.AmountExcTax;
                        decimal AmountExcTax = item.AmountExcTax;
                        var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                        {
                            a.TaxId,
                            a.Tax,
                            a.TaxPercent,
                        }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                       where a.TaxId == item.TaxId
                                       select new
                                       {
                                           TaxId = a.SubTaxId,
                                           Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                           TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                       }).ToList();

                        foreach (var tax in taxs)
                        {
                            oClsTaxVm.Add(new ClsTaxVm
                            {
                                TaxId = tax.TaxId,
                                Tax = tax.Tax,
                                TaxPercent = tax.TaxPercent,
                                TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                            });
                        }

                        var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                                 (k, c) => new
                                 {
                                     TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                                     Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                                     TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                                     TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                                 }
                                ).ToList();

                        List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                        if (obj.CountryId == 2)
                        {
                            if (item.ITCType == "Eligible For ITC")
                            {
                                if (obj.IsReverseCharge == 1)
                                {
                                    AccountType = "Reverse Charge Tax Input but not due";

                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Reverse Charge",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                                else
                                {
                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Normal",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                            }
                            else
                            {
                                if (obj.IsReverseCharge == 1)
                                {
                                    AccountType = "Tax Paid Expense";

                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Reverse Charge",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                                else
                                {
                                    AccountType = "Tax Paid Expense";
                                }
                            }
                        }
                        else
                        {
                            taxList = finalTaxs.Select(a => new ClsTaxVm
                            {
                                TaxType = "Normal",
                                TaxId = a.TaxId,
                                TaxPercent = a.TaxPercent,
                                TaxAmount = a.TaxAmount,
                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                            }).ToList();
                        }

                        ClsPurchaseReturnAdditionalCharges oClsPurchaseReturnAdditionalCharges = new ClsPurchaseReturnAdditionalCharges()
                        {
                            AdditionalChargeId = item.AdditionalChargeId,
                            PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                            TaxId = item.TaxId,
                            AmountExcTax = item.AmountExcTax,
                            AmountIncTax = item.AmountIncTax,
                            TaxAmount = item.AmountIncTax - item.AmountExcTax,
                            AccountId = AdditionalCharge.PurchaseAccountId,
                            ItemCodeId = AdditionalCharge.ItemCodeId,
                            ITCType = item.ITCType,
                            TaxExemptionId = item.TaxExemptionId,
                            IsActive = item.IsActive,
                            IsDeleted = item.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId
                        };
                        oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Add(oClsPurchaseReturnAdditionalCharges);
                        oConnectionContext.SaveChanges();

                        if (AccountType != "")
                        {
                            AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                            && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                            ClsPurchaseReturnAdditionalTaxJournal oClsPurchaseReturnAdditionalTaxJournal = new ClsPurchaseReturnAdditionalTaxJournal()
                            {
                                PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                PurchaseReturnAdditionalChargesId = oClsPurchaseReturnAdditionalCharges.PurchaseReturnAdditionalChargesId,
                                TaxId = item.TaxId,
                                TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : (item.AmountIncTax - item.AmountExcTax),
                                AccountId = AccountId,
                                PurchaseReturnTaxJournalType = "Normal"
                            };
                            oConnectionContext.DbClsPurchaseReturnAdditionalTaxJournal.Add(oClsPurchaseReturnAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }

                        foreach (var taxJournal in taxList)
                        {
                            ClsPurchaseReturnAdditionalTaxJournal oClsPurchaseReturnAdditionalTaxJournal = new ClsPurchaseReturnAdditionalTaxJournal()
                            {
                                PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                PurchaseReturnAdditionalChargesId = oClsPurchaseReturnAdditionalCharges.PurchaseReturnAdditionalChargesId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                PurchaseReturnTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsPurchaseReturnAdditionalTaxJournal.Add(oClsPurchaseReturnAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.PurchaseReturnDetails != null)
                {
                    foreach (var PurchaseReturn in obj.PurchaseReturnDetails)
                    {
                        if (PurchaseReturn.Quantity != 0)
                        {
                            decimal convertedStock = oCommonController.StockConversion(PurchaseReturn.Quantity, PurchaseReturn.ItemId, PurchaseReturn.PriceAddedFor);
                            decimal freeConvertedStock = oCommonController.StockConversion(PurchaseReturn.FreeQuantity, PurchaseReturn.ItemId, PurchaseReturn.PriceAddedFor);

                            long PurchaseAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == PurchaseReturn.ItemDetailsId).Select(a => a.PurchaseAccountId).FirstOrDefault();
                            TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == PurchaseReturn.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();

                            string AccountType = "";

                            var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == PurchaseReturn.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                            //decimal AmountExcTax = obj.IsReverseCharge == 1 ? PurchaseReturn.AmountIncTax : PurchaseReturn.AmountExcTax;
                            decimal AmountExcTax = PurchaseReturn.AmountExcTax;
                            var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == PurchaseReturn.TaxId).Select(a => new
                            {
                                a.TaxId,
                                a.Tax,
                                a.TaxPercent,
                            }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                           where a.TaxId == PurchaseReturn.TaxId
                                           select new
                                           {
                                               TaxId = a.SubTaxId,
                                               Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                               TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                           }).ToList();

                            foreach (var tax in taxs)
                            {
                                oClsTaxVm.Add(new ClsTaxVm
                                {
                                    TaxId = tax.TaxId,
                                    Tax = tax.Tax,
                                    TaxPercent = tax.TaxPercent,
                                    TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                                });
                            }

                            var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                                     (k, c) => new
                                     {
                                         TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                                         Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                                         TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                                         TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                                     }
                                    ).ToList();

                            List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                            if (obj.CountryId == 2)
                            {
                                if (PurchaseReturn.ITCType == "Eligible For ITC")
                                {
                                    if (obj.IsReverseCharge == 1)
                                    {
                                        AccountType = "Reverse Charge Tax Input but not due";

                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Reverse Charge",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                    else
                                    {
                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Normal",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                }
                                else
                                {
                                    if (obj.IsReverseCharge == 1)
                                    {
                                        AccountType = "Tax Paid Expense";

                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Reverse Charge",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                    else
                                    {
                                        AccountType = "Tax Paid Expense";
                                    }
                                }
                            }
                            else
                            {
                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                {
                                    TaxType = "Normal",
                                    TaxId = a.TaxId,
                                    TaxPercent = a.TaxPercent,
                                    TaxAmount = a.TaxAmount,
                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                }).ToList();
                            }

                            ClsPurchaseReturnDetails oClsPurchaseReturnDetails = new ClsPurchaseReturnDetails()
                            {
                                //Amount = PurchaseReturn.Amount,
                                ItemId = PurchaseReturn.ItemId,
                                ItemDetailsId = PurchaseReturn.ItemDetailsId,
                                PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                //PurchaseReturnPrice = PurchaseReturn.PurchaseReturnPrice,
                                //Tax = PurchaseReturn.Tax,
                                //TaxId = PurchaseReturn.TaxId,
                                //Discount = PurchaseReturn.Discount,
                                //Quantity = PurchaseReturn.Quantity,
                                IsActive = PurchaseReturn.IsActive,
                                IsDeleted = PurchaseReturn.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                Quantity = PurchaseReturn.Quantity,
                                PriceAddedFor = PurchaseReturn.PriceAddedFor,
                                QuantityRemaining = convertedStock + freeConvertedStock,
                                FreeQuantity = PurchaseReturn.FreeQuantity,
                                PurchaseDetailsId = PurchaseReturn.PurchaseDetailsId,
                                UnitAddedFor = PurchaseReturn.UnitAddedFor,
                                QuantityReturned = convertedStock + freeConvertedStock,
                                TaxId = PurchaseReturn.TaxId,
                                DiscountType = PurchaseReturn.DiscountType,
                                Discount = PurchaseReturn.Discount,
                                UnitCost = PurchaseReturn.UnitCost,
                                AmountExcTax = PurchaseReturn.AmountExcTax,
                                TaxAmount = PurchaseReturn.TaxAmount,
                                AmountIncTax = PurchaseReturn.AmountIncTax,
                                PurchaseExcTax = PurchaseReturn.PurchaseExcTax,
                                PurchaseIncTax = PurchaseReturn.PurchaseIncTax,
                                AccountId = PurchaseAccountId,
                                DiscountAccountId = DiscountAccountId,
                                TaxAccountId = TaxAccountId,
                                ExtraDiscount = PurchaseReturn.ExtraDiscount,
                                TotalTaxAmount = PurchaseReturn.TotalTaxAmount
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsPurchaseReturnDetails.Add(oClsPurchaseReturnDetails);
                            oConnectionContext.SaveChanges();

                            //string query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (\"convertedStock\" + \"freeConvertedStock\") + " where \"PurchaseId\"=" + obj.PurchaseId + " and \"Itemid\"=" + PurchaseReturn.ItemId + " and \"ItemDetailsId\"=" + PurchaseReturn.ItemDetailsId;
                            //oConnectionContext.Database.ExecuteSqlCommand(query);

                            //query = "update tblItemBranchMap set Quantity=Quantity,0)-" + (convertedStock + freeConvertedStock) + " where BranchId=" + obj.BranchId + " and ItemId=" + PurchaseReturn.ItemId + " and ItemDetailsId=" + PurchaseReturn.ItemDetailsId;
                            //oConnectionContext.Database.ExecuteSqlCommand(query);

                            if (AccountType != "")
                            {
                                AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                                ClsPurchaseReturnTaxJournal oClsPurchaseReturnTaxJournal = new ClsPurchaseReturnTaxJournal()
                                {
                                    PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                    PurchaseReturnDetailsId = oClsPurchaseReturnDetails.PurchaseReturnDetailsId,
                                    TaxId = PurchaseReturn.TaxId,
                                    TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : PurchaseReturn.TaxAmount,
                                    AccountId = AccountId,
                                    PurchaseReturnTaxJournalType = "Normal"
                                };
                                oConnectionContext.DbClsPurchaseReturnTaxJournal.Add(oClsPurchaseReturnTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                            foreach (var taxJournal in taxList)
                            {
                                ClsPurchaseReturnTaxJournal oClsPurchaseReturnTaxJournal = new ClsPurchaseReturnTaxJournal()
                                {
                                    PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                    PurchaseReturnDetailsId = oClsPurchaseReturnDetails.PurchaseReturnDetailsId,
                                    TaxId = taxJournal.TaxId,
                                    TaxAmount = taxJournal.TaxAmount,
                                    AccountId = taxJournal.AccountId,
                                    PurchaseReturnTaxJournalType = taxJournal.TaxType
                                };
                                oConnectionContext.DbClsPurchaseReturnTaxJournal.Add(oClsPurchaseReturnTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                            if (obj.Status.ToLower() != "draft")
                            {
                                string query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + ",\"QuantitySold\"=\"QuantitySold\"+" + (convertedStock + freeConvertedStock) + " where \"PurchaseDetailsId\"=" + PurchaseReturn.PurchaseDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);

                                query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + PurchaseReturn.ItemId + " and \"ItemDetailsId\"=" + PurchaseReturn.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                    }
                }

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                if (obj.Status.ToLower() != "draft")
                {
                    decimal PurchaseDue = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                    (oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment") && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == obj.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum());

                    ClsSupplierPaymentVm oClsSupplierPayment = new ClsSupplierPaymentVm
                    {
                        PurchaseId = obj.PurchaseId,
                        PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                        CompanyId = obj.CompanyId,
                        BranchId = obj.BranchId,
                        AddedBy = obj.AddedBy,
                        IsActive = true,
                        IsDeleted = false,
                        Amount = obj.GrandTotal,
                        PaymentDate = CurrentDate,
                        SupplierId = obj.SupplierId,
                        Type = "Supplier Payment",
                        AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Petty Cash").Select(a => a.AccountId).FirstOrDefault(),
                        AmountExcTax = obj.GrandTotal,
                        TaxAccountId = 0,
                        TaxAmount = 0,
                        TaxId = 0,
                        IsReverseCharge = obj.IsReverseCharge
                    };

                    if (PurchaseDue >= obj.GrandTotal)
                    {
                        oClsSupplierPayment.SupplierPaymentIds = new List<ClsSupplierPaymentIds> { new ClsSupplierPaymentIds()
                    {
                        Due = PurchaseDue,
                        Amount= obj.GrandTotal,
                        PurchaseId = obj.PurchaseId,
                        Type = "Purchase Payment"
                    }
                    };
                    }
                    else if (PurchaseDue < obj.GrandTotal)
                    {
                        oClsSupplierPayment.SupplierPaymentIds = new List<ClsSupplierPaymentIds> { new ClsSupplierPaymentIds()
                    {
                        Due = PurchaseDue,
                        Amount= PurchaseDue,
                        PurchaseId = obj.PurchaseId,
                        Type = "Purchase Payment"
                    }
                    };
                    }

                    oCommonController.DebitNoteCreditsApply(oClsSupplierPayment);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Return",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Return \"" + obj.InvoiceNo + "\" created",
                    Id = oClsPurchaseReturn.PurchaseReturnId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Debit Note", obj.CompanyId, oClsPurchaseReturn.PurchaseReturnId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Return created successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Purchase = new
                        {
                            InvoiceId = oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == oClsPurchaseReturn.PurchaseReturnId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        PurchaseSetting = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.AutoPrintInvoicePurchaseQuotation, a.AutoPrintInvoicePurchaseOrder, a.AutoPrintInvoicePurchaseBill, a.AutoPrintInvoicePurchaseReturn }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseReturnDelete(ClsPurchaseReturnVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //obj.BranchId = oConnectionContext.DbClsPurchase.
                //    Where(a => a.PurchaseId == oConnectionContext.DbClsPurchaseReturn.
                //    Where(b => b.PurchaseReturnId == obj.PurchaseReturnId).Select(b => b.PurchaseId).FirstOrDefault()).Select(a => a.BranchId).FirstOrDefault();

                obj.BranchId = oConnectionContext.DbClsPurchaseReturn.
                     Where(b => b.PurchaseReturnId == obj.PurchaseReturnId).Select(b => b.BranchId).FirstOrDefault();

                //obj.PurchaseId = oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId).Select(a => a.PurchaseId).FirstOrDefault();
                var details = oConnectionContext.DbClsPurchaseReturnDetails.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId && a.IsDeleted == false).Select(a => new
                {
                    a.ItemDetailsId,
                    a.ItemId,
                    a.Quantity,
                    a.FreeQuantity,
                    a.PriceAddedFor,
                    a.PurchaseDetailsId
                }).ToList();

                ClsPurchaseReturn oClsPurchaseReturn = new ClsPurchaseReturn()
                {
                    PurchaseReturnId = obj.PurchaseReturnId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPurchaseReturn.Attach(oClsPurchaseReturn);
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.PurchaseReturnId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "";
                foreach (var item in details)
                {
                    decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                    decimal freeConvertedStock = oCommonController.StockConversion(item.FreeQuantity, item.ItemId, item.PriceAddedFor);

                    query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + (convertedStock + freeConvertedStock) + ",\"QuantitySold\"=\"QuantitySold\"-" + (convertedStock) + " where \"PurchaseDetailsId\"=" + item.PurchaseDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Return",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Return \"" + oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" deleted",
                    Id = oClsPurchaseReturn.PurchaseReturnId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Return deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseReturnCancel(ClsPurchaseReturnVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //obj.BranchId = oConnectionContext.DbClsPurchase.
                //    Where(a => a.PurchaseId == oConnectionContext.DbClsPurchaseReturn.
                //    Where(b => b.PurchaseReturnId == obj.PurchaseReturnId).Select(b => b.PurchaseId).FirstOrDefault()).Select(a => a.BranchId).FirstOrDefault();

                obj.BranchId = oConnectionContext.DbClsPurchaseReturn.
                     Where(b => b.PurchaseReturnId == obj.PurchaseReturnId).Select(b => b.BranchId).FirstOrDefault();

                //obj.PurchaseId = oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId).Select(a => a.PurchaseId).FirstOrDefault();
                var details = oConnectionContext.DbClsPurchaseReturnDetails.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId && a.IsDeleted == false).Select(a => new
                {
                    a.ItemDetailsId,
                    a.ItemId,
                    a.Quantity,
                    a.FreeQuantity,
                    a.PriceAddedFor,
                    a.PurchaseDetailsId
                }).ToList();

                ClsPurchaseReturn oClsPurchaseReturn = new ClsPurchaseReturn()
                {
                    PurchaseReturnId = obj.PurchaseReturnId,
                    IsCancelled = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPurchaseReturn.Attach(oClsPurchaseReturn);
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.PurchaseReturnId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.IsCancelled).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "";
                foreach (var item in details)
                {
                    decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);
                    decimal freeConvertedStock = oCommonController.StockConversion(item.FreeQuantity, item.ItemId, item.PriceAddedFor);

                    query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + (convertedStock + freeConvertedStock) + ",\"QuantitySold\"=\"QuantitySold\"-" + (convertedStock) + " where \"PurchaseDetailsId\"=" + item.PurchaseDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Return",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Return \"" + oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" updated",
                    Id = oClsPurchaseReturn.PurchaseReturnId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Debit Note", obj.CompanyId, oClsPurchaseReturn.PurchaseReturnId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Return cancelled successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePurchaseReturn(ClsPurchaseReturnVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            long CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.SupplierId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSupplier" });
                    isError = true;
                }

                if (obj.SupplierId != 0)
                {
                    if (CountryId == 2)
                    {
                        string GstTreatment = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.GstTreatment).FirstOrDefault();
                        if (GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.SourceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSourceOfSupply" });
                                isError = true;
                            }
                        }

                        if (obj.DestinationOfSupplyId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divDestinationOfSupply" });
                            isError = true;
                        }
                    }
                }

                if (obj.Date == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDate" });
                    isError = true;
                }

                //if (obj.Status == "" || obj.Status == null)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divStatus" });
                //    isError = true;
                //}

                if (obj.PurchaseId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseInvoice" });
                    isError = true;
                }

                if (obj.PurchaseDebitNoteReasonId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseDebitNoteReason" });
                    isError = true;
                }

                if (obj.PurchaseReturnDetails == null || obj.PurchaseReturnDetails.Where(a => a.IsDeleted == false).Count() == 0)
                {
                    errors.Add(new ClsError { Message = "Search item first", Id = "divtags" });
                    isError = true;
                }

                if (obj.PurchaseReturnDetails != null)
                {
                    foreach (var item in obj.PurchaseReturnDetails)
                    {
                        if (item.IsDeleted == false)
                        {
                            if (item.Quantity <= 0)
                            {
                                errors.Add(new ClsError { Message = "", Id = "divQuantity" + item.DivId });
                                isError = true;
                            }
                            if (item.UnitCost <= 0)
                            {
                                errors.Add(new ClsError { Message = "", Id = "divUnitCost" + item.DivId });
                                isError = true;
                            }
                        }
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

                //if (obj.PurchaseReturnDetails == null)
                //{
                //    data = new
                //    {
                //        Status = 0,
                //        Message = "Add products first",
                //        Data = new
                //        {
                //        }
                //    }; return await Task.FromResult(Ok(data));
                //}

                if (obj.PurchaseId != 0)
                {
                    var purchase = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => new
                    {
                        a.BranchId,
                        a.SupplierId
                    }).FirstOrDefault();

                    obj.SupplierId = purchase.SupplierId;
                    obj.BranchId = purchase.BranchId;
                }

                if (obj.PurchaseReturnDetails != null)
                {
                    foreach (var PurchaseReturn in obj.PurchaseReturnDetails)
                    {
                        if (PurchaseReturn.PurchaseReturnDetailsId != 0)
                        {
                            //    decimal QuantityRemaining = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == PurchaseReturn.PurchaseDetailsId).Select(a => a.QuantityRemaining).FirstOrDefault();

                            //    decimal convertedStock = oCommonController.StockConversion(PurchaseReturn.Quantity + PurchaseReturn.FreeQuantity, PurchaseReturn.ItemId, PurchaseReturn.PriceAddedFor);

                            //    if (convertedStock > (QuantityRemaining))
                            //    {
                            //        data = new
                            //        {
                            //            Status = 0,
                            //            Message = "Only " + QuantityRemaining + "items is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == PurchaseReturn.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                            //            Data = new
                            //            {
                            //            }
                            //        };
                            //        return await Task.FromResult(Ok(data));
                            //    }
                            //}
                            //else
                            //{
                            decimal PreviousConvertedStock = oCommonController.StockConversion(
                                oConnectionContext.DbClsPurchaseReturnDetails.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId
                            && a.ItemId == PurchaseReturn.ItemId && a.ItemDetailsId == PurchaseReturn.ItemDetailsId).Select(a => a.Quantity + a.FreeQuantity).
                            FirstOrDefault(), PurchaseReturn.ItemId, oConnectionContext.DbClsPurchaseReturnDetails.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId
                            && a.ItemId == PurchaseReturn.ItemId && a.ItemDetailsId == PurchaseReturn.ItemDetailsId).Select(a => a.PriceAddedFor).FirstOrDefault());

                            string query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + PreviousConvertedStock + ",\"QuantitySold\"=\"QuantitySold\"-" + PreviousConvertedStock + " where \"PurchaseDetailsId\"=" + PurchaseReturn.PurchaseDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+(" + PreviousConvertedStock + ") where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + PurchaseReturn.ItemId + " and \"ItemDetailsId\"=" + PurchaseReturn.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            if (PurchaseReturn.IsDeleted == true)
                            {
                                query = "update \"tblPurchaseReturnDetails\" set \"IsDeleted\"=True where \"PurchaseReturnDetailsId\"=" + PurchaseReturn.PurchaseReturnDetailsId;
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }

                            //decimal QuantityRemaining = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == PurchaseReturn.PurchaseDetailsId).Select(a => a.QuantityRemaining).FirstOrDefault();

                            //decimal convertedStock = oCommonController.StockConversion(PurchaseReturn.Quantity + PurchaseReturn.FreeQuantity, PurchaseReturn.ItemId, PurchaseReturn.PriceAddedFor);

                            //if (convertedStock > QuantityRemaining)
                            //{
                            //    data = new
                            //    {
                            //        Status = 0,
                            //        Message = "Only " + QuantityRemaining + "items is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == PurchaseReturn.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                            //        Data = new
                            //        {
                            //        }
                            //    };
                            //    return await Task.FromResult(Ok(data));
                            //}
                        }
                    }
                }

                obj.PurchaseReturnDetails.RemoveAll(r => r.IsDeleted == true);

                if (obj.PurchaseId == 0)
                {
                    bool isItemAdded = false;
                    if (obj.PurchaseReturnDetails != null)
                    {
                        foreach (var PurchaseReturn in obj.PurchaseReturnDetails)
                        {
                            isItemAdded = true;
                            if (PurchaseReturn.PurchaseReturnDetailsId == 0)
                            {
                                if (PurchaseReturn.Quantity != 0)
                                {

                                    //decimal QuantityRemaining = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseId == obj.PurchaseId
                                    //&& a.ItemId == PurchaseReturn.ItemId && a.ItemDetailsId == PurchaseReturn.ItemDetailsId).Select(a => a.QuantityRemaining).FirstOrDefault();

                                    decimal remainingQty = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == PurchaseReturn.PurchaseDetailsId).Select(a => a.QuantityRemaining).FirstOrDefault();

                                    decimal convertedStock = 0;
                                    foreach (var inner in obj.PurchaseReturnDetails)
                                    {
                                        bool IsManageStock_Inner = oConnectionContext.DbClsItem.Where(a => a.ItemId == PurchaseReturn.ItemId).Select(a => a.IsManageStock).FirstOrDefault();
                                        if (IsManageStock_Inner == true)
                                        {
                                            if (PurchaseReturn.PurchaseDetailsId == inner.PurchaseDetailsId)
                                            {
                                                convertedStock = convertedStock + oCommonController.StockConversion(inner.Quantity + inner.FreeQuantity, inner.ItemId, inner.PriceAddedFor);
                                            }
                                        }
                                    }
                                    if (remainingQty < convertedStock)
                                    {
                                        //errors.Add(new ClsError { Message = "Only " + remainingQty + " quantity is available for Lot No: " + LotNo, Id = "divQuantity"+Sales.DivId });
                                        errors.Add(new ClsError { Message = "Insufficient Qty", Id = "divQuantityReturned" + PurchaseReturn.DivId });
                                        isError = true;
                                    }

                                    //decimal convertedStock = oCommonController.StockConversion(PurchaseReturn.Quantity + PurchaseReturn.FreeQuantity, PurchaseReturn.ItemId, PurchaseReturn.PriceAddedFor);

                                    //if (convertedStock > (QuantityRemaining))
                                    //{
                                    //    data = new
                                    //    {
                                    //        Status = 0,
                                    //        Message = "Only " + QuantityRemaining + "items is available for " + oConnectionContext.DbClsItem.Where(a => a.ItemId == PurchaseReturn.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                                    //        Data = new
                                    //        {
                                    //        }
                                    //    };
                                    //    return await Task.FromResult(Ok(data));
                                    //}
                                }
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
                    }

                    if (isItemAdded == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Add return quantity first",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                //decimal Paid = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == obj.PurchaseReturnId).Select(b => b.Amount).DefaultIfEmpty().Sum();
                //if (Paid == obj.GrandTotal)
                //{
                //    obj.Status = "Paid";
                //}
                //else if (Paid > obj.GrandTotal)
                //{
                //    data = new
                //    {
                //        Status = 0,
                //        Message = "More amount is already paid",
                //        Data = new
                //        {
                //        }
                //    };
                //    return await Task.FromResult(Ok(data));
                //}
                //else if (Paid != 0 && Paid < obj.GrandTotal)
                //{
                //    obj.Status = "Partially Paid";
                //}
                //else
                //{
                //    obj.Status = "Due";
                //}

                long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
           && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();

                long DiscountAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DiscountAccountId).FirstOrDefault();
                long RoundOffAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.RoundOffAccountId).FirstOrDefault();
                long SpecialDiscountAccountId = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.SpecialDiscountAccountId).FirstOrDefault();
                long TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();

                ClsPurchaseReturn oClsPurchaseReturn = new ClsPurchaseReturn()
                {
                    TotalTaxAmount = obj.TotalTaxAmount,
                    TotalDiscount = obj.TotalDiscount,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    Discount = obj.Discount,
                    DiscountType = obj.DiscountType,
                    GrandTotal = obj.IsReverseCharge == 1 ? obj.GrandTotalReverseCharge : obj.GrandTotal,
                    TaxableAmount = obj.GrandTotal,
                    Notes = obj.Notes,
                    //PayTerm = obj.PayTerm,
                    //PayTermNo = obj.PayTermNo,
                    PaymentTermId = obj.PaymentTermId,
                    DueDate = obj.DueDate,
                    PurchaseReturnId = obj.PurchaseReturnId,
                    Subtotal = obj.Subtotal,
                    TotalQuantity = obj.TotalQuantity,
                    Date = obj.Date.AddHours(5).AddMinutes(30),
                    SmsSettingsId = obj.SmsSettingsId,
                    EmailSettingsId = obj.EmailSettingsId,
                    WhatsappSettingsId = obj.WhatsappSettingsId,
                    TaxId = obj.TaxId,
                    TaxAmount = obj.TaxAmount,
                    RoundOff = obj.RoundOff,
                    SpecialDiscount = obj.SpecialDiscount,
                    NetAmount = obj.NetAmount,
                    //Status = obj.Status,
                    AccountId = AccountId,
                    DiscountAccountId = DiscountAccountId,
                    //OtherChargesAccountId = OtherChargesAccountId,
                    RoundOffAccountId = RoundOffAccountId,
                    TaxAccountId = TaxAccountId,
                    SourceOfSupplyId = obj.SourceOfSupplyId,
                    DestinationOfSupplyId = obj.DestinationOfSupplyId,
                    PurchaseDebitNoteReasonId = obj.PurchaseDebitNoteReasonId,
                    NetAmountReverseCharge = obj.NetAmountReverseCharge,
                    RoundOffReverseCharge = obj.RoundOffReverseCharge,
                    GrandTotalReverseCharge = obj.GrandTotalReverseCharge,
                    IsReverseCharge = obj.IsReverseCharge,
                    SpecialDiscountAccountId = SpecialDiscountAccountId
                };

                string pic1 = oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId).Select(a => a.AttachDocument).FirstOrDefault();
                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/PurchaseReturn/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/PurchaseReturn/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsPurchaseReturn.AttachDocument = filepathPass;
                }
                else
                {
                    oClsPurchaseReturn.AttachDocument = pic1;
                }

                oConnectionContext.DbClsPurchaseReturn.Attach(oClsPurchaseReturn);
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.TotalDiscount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.Discount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.DiscountType).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.TaxableAmount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.DueDate).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.PurchaseReturnId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.TotalQuantity).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.Date).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.SmsSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.EmailSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.WhatsappSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.TaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.RoundOff).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.SpecialDiscount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.NetAmount).IsModified = true;
                //oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.TotalTaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.DiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.RoundOffAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.SpecialDiscountAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.TaxAccountId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.AttachDocument).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.SourceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.DestinationOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.PurchaseDebitNoteReasonId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.NetAmountReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.RoundOffReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.GrandTotalReverseCharge).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.IsReverseCharge).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.PurchaseReturnAdditionalCharges != null)
                {
                    foreach (var item in obj.PurchaseReturnAdditionalCharges)
                    {
                        var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId &&
                            a.AdditionalChargeId == item.AdditionalChargeId).Select(a => new { a.ItemCodeId, a.PurchaseAccountId }).FirstOrDefault();

                        string AccountType = "";

                        var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == item.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                        List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                        //decimal AmountExcTax = obj.IsReverseCharge == 1 ? PurchaseReturn.AmountIncTax : PurchaseReturn.AmountExcTax;
                        decimal AmountExcTax = item.AmountExcTax;
                        var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                        {
                            a.TaxId,
                            a.Tax,
                            a.TaxPercent,
                        }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                       where a.TaxId == item.TaxId
                                       select new
                                       {
                                           TaxId = a.SubTaxId,
                                           Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                           TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                       }).ToList();

                        foreach (var tax in taxs)
                        {
                            oClsTaxVm.Add(new ClsTaxVm
                            {
                                TaxId = tax.TaxId,
                                Tax = tax.Tax,
                                TaxPercent = tax.TaxPercent,
                                TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                            });
                        }

                        var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                                 (k, c) => new
                                 {
                                     TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                                     Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                                     TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                                     TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                                 }
                                ).ToList();

                        List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                        if (obj.CountryId == 2)
                        {
                            if (item.ITCType == "Eligible For ITC")
                            {
                                if (obj.IsReverseCharge == 1)
                                {
                                    AccountType = "Reverse Charge Tax Input but not due";

                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Reverse Charge",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                                else
                                {
                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Normal",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                            }
                            else
                            {
                                if (obj.IsReverseCharge == 1)
                                {
                                    AccountType = "Tax Paid Expense";

                                    taxList = finalTaxs.Select(a => new ClsTaxVm
                                    {
                                        TaxType = "Reverse Charge",
                                        TaxId = a.TaxId,
                                        TaxPercent = a.TaxPercent,
                                        TaxAmount = a.TaxAmount,
                                        AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                    }).ToList();
                                }
                                else
                                {
                                    AccountType = "Tax Paid Expense";
                                }
                            }
                        }
                        else
                        {
                            taxList = finalTaxs.Select(a => new ClsTaxVm
                            {
                                TaxType = "Normal",
                                TaxId = a.TaxId,
                                TaxPercent = a.TaxPercent,
                                TaxAmount = a.TaxAmount,
                                AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                            }).ToList();
                        }

                        long PurchaseReturnAdditionalChargesId = 0;
                        if (item.PurchaseReturnAdditionalChargesId == 0)
                        {
                            ClsPurchaseReturnAdditionalCharges oClsPurchaseReturnAdditionalCharges = new ClsPurchaseReturnAdditionalCharges()
                            {
                                AdditionalChargeId = item.AdditionalChargeId,
                                PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                TaxId = item.TaxId,
                                AmountExcTax = item.AmountExcTax,
                                AmountIncTax = item.AmountIncTax,
                                TaxAmount = item.AmountIncTax - item.AmountExcTax,
                                AccountId = AdditionalCharge.PurchaseAccountId,
                                ItemCodeId = AdditionalCharge.ItemCodeId,
                                ITCType = item.ITCType,
                                TaxExemptionId = item.TaxExemptionId,
                                IsActive = item.IsActive,
                                IsDeleted = item.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId
                            };
                            oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Add(oClsPurchaseReturnAdditionalCharges);
                            oConnectionContext.SaveChanges();

                            PurchaseReturnAdditionalChargesId = oClsPurchaseReturnAdditionalCharges.PurchaseReturnAdditionalChargesId;
                        }
                        else
                        {
                            ClsPurchaseReturnAdditionalCharges oClsPurchaseReturnAdditionalCharges = new ClsPurchaseReturnAdditionalCharges()
                            {
                                PurchaseReturnAdditionalChargesId = item.PurchaseReturnAdditionalChargesId,
                                AdditionalChargeId = item.AdditionalChargeId,
                                PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                TaxId = item.TaxId,
                                AmountExcTax = item.AmountExcTax,
                                AmountIncTax = item.AmountIncTax,
                                TaxAmount = item.AmountIncTax - item.AmountExcTax,
                                AccountId = AdditionalCharge.PurchaseAccountId,
                                ItemCodeId = AdditionalCharge.ItemCodeId,
                                ITCType = item.ITCType,
                                TaxExemptionId = item.TaxExemptionId,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                            };
                            oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Attach(oClsPurchaseReturnAdditionalCharges);
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.AdditionalChargeId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.PurchaseReturnId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.ItemCodeId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.ITCType).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnAdditionalCharges).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();

                            PurchaseReturnAdditionalChargesId = oClsPurchaseReturnAdditionalCharges.PurchaseReturnAdditionalChargesId;
                        }

                        string query = "delete from \"tblPurchaseReturnAdditionalTaxJournal\" where \"PurchaseReturnId\"=" + oClsPurchaseReturn.PurchaseReturnId + " and \"PurchaseReturnAdditionalChargesId\"=" + PurchaseReturnAdditionalChargesId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);

                        if (AccountType != "")
                        {
                            AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                            && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                            ClsPurchaseReturnAdditionalTaxJournal oClsPurchaseReturnAdditionalTaxJournal = new ClsPurchaseReturnAdditionalTaxJournal()
                            {
                                PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                PurchaseReturnAdditionalChargesId = PurchaseReturnAdditionalChargesId,
                                TaxId = item.TaxId,
                                TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : (item.AmountIncTax - item.AmountExcTax),
                                AccountId = AccountId,
                                PurchaseReturnTaxJournalType = "Normal"
                            };
                            oConnectionContext.DbClsPurchaseReturnAdditionalTaxJournal.Add(oClsPurchaseReturnAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }

                        foreach (var taxJournal in taxList)
                        {
                            ClsPurchaseReturnAdditionalTaxJournal oClsPurchaseReturnAdditionalTaxJournal = new ClsPurchaseReturnAdditionalTaxJournal()
                            {
                                PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                PurchaseReturnAdditionalChargesId = PurchaseReturnAdditionalChargesId,
                                TaxId = taxJournal.TaxId,
                                TaxAmount = taxJournal.TaxAmount,
                                AccountId = taxJournal.AccountId,
                                PurchaseReturnTaxJournalType = taxJournal.TaxType
                            };
                            oConnectionContext.DbClsPurchaseReturnAdditionalTaxJournal.Add(oClsPurchaseReturnAdditionalTaxJournal);
                            oConnectionContext.SaveChanges();
                        }

                    }
                }

                if (obj.PurchaseReturnDetails != null)
                {
                    foreach (var PurchaseReturn in obj.PurchaseReturnDetails)
                    {
                        //if (PurchaseReturn.IsDeleted == true)
                        //{
                        //    decimal convertedStock = oCommonController.StockConversion(PurchaseReturn.Quantity, PurchaseReturn.ItemId, PurchaseReturn.PriceAddedFor);
                        //    decimal freeConvertedStock = oCommonController.StockConversion(PurchaseReturn.FreeQuantity, PurchaseReturn.ItemId, PurchaseReturn.PriceAddedFor);

                        //    string query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + (convertedStock + freeConvertedStock) + ",\"QuantitySold\"=\"QuantitySold\"-" + (convertedStock) + " where \"PurchaseDetailsId\"=" + PurchaseReturn.PurchaseDetailsId;
                        //    oConnectionContext.Database.ExecuteSqlCommand(query);

                        //    query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + PurchaseReturn.ItemId + " and \"ItemDetailsId\"=" + PurchaseReturn.ItemDetailsId;
                        //    oConnectionContext.Database.ExecuteSqlCommand(query);
                        //}
                        //else
                        //{
                        decimal convertedStock = oCommonController.StockConversion(PurchaseReturn.Quantity, PurchaseReturn.ItemId, PurchaseReturn.PriceAddedFor);
                        decimal freeConvertedStock = oCommonController.StockConversion(PurchaseReturn.FreeQuantity, PurchaseReturn.ItemId, PurchaseReturn.PriceAddedFor);

                        long PurchaseAccountId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == PurchaseReturn.ItemDetailsId).Select(a => a.PurchaseAccountId).FirstOrDefault();
                        TaxAccountId = oConnectionContext.DbClsTax.Where(a => a.TaxId == PurchaseReturn.TaxId).Select(a => a.PurchaseAccountId).FirstOrDefault();

                        if (PurchaseReturn.PurchaseReturnDetailsId == 0)
                        {
                            string AccountType = "";

                            var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == PurchaseReturn.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                            //decimal AmountExcTax = obj.IsReverseCharge == 1 ? PurchaseReturn.AmountIncTax : PurchaseReturn.AmountExcTax;
                            decimal AmountExcTax = PurchaseReturn.AmountExcTax;
                            var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == PurchaseReturn.TaxId).Select(a => new
                            {
                                a.TaxId,
                                a.Tax,
                                a.TaxPercent,
                            }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                           where a.TaxId == PurchaseReturn.TaxId
                                           select new
                                           {
                                               TaxId = a.SubTaxId,
                                               Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                               TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                           }).ToList();

                            foreach (var tax in taxs)
                            {
                                oClsTaxVm.Add(new ClsTaxVm
                                {
                                    TaxId = tax.TaxId,
                                    Tax = tax.Tax,
                                    TaxPercent = tax.TaxPercent,
                                    TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                                });
                            }

                            var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                                     (k, c) => new
                                     {
                                         TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                                         Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                                         TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                                         TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                                     }
                                    ).ToList();

                            List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                            if (obj.CountryId == 2)
                            {
                                if (PurchaseReturn.ITCType == "Eligible For ITC")
                                {
                                    if (obj.IsReverseCharge == 1)
                                    {
                                        AccountType = "Reverse Charge Tax Input but not due";

                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Reverse Charge",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                    else
                                    {
                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Normal",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                }
                                else
                                {
                                    if (obj.IsReverseCharge == 1)
                                    {
                                        AccountType = "Tax Paid Expense";

                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Reverse Charge",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                    else
                                    {
                                        AccountType = "Tax Paid Expense";
                                    }
                                }
                            }
                            else
                            {
                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                {
                                    TaxType = "Normal",
                                    TaxId = a.TaxId,
                                    TaxPercent = a.TaxPercent,
                                    TaxAmount = a.TaxAmount,
                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                }).ToList();
                            }

                            ClsPurchaseReturnDetails oClsPurchaseReturnDetails = new ClsPurchaseReturnDetails()
                            {
                                //Amount = PurchaseReturn.Amount,
                                ItemId = PurchaseReturn.ItemId,
                                ItemDetailsId = PurchaseReturn.ItemDetailsId,
                                PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                //PurchaseReturnPrice = PurchaseReturn.PurchaseReturnPrice,
                                //Tax = PurchaseReturn.Tax,
                                //TaxId = PurchaseReturn.TaxId,
                                //Discount = PurchaseReturn.Discount,
                                //Quantity = PurchaseReturn.Quantity,
                                IsActive = PurchaseReturn.IsActive,
                                IsDeleted = PurchaseReturn.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                Quantity = PurchaseReturn.Quantity,
                                PriceAddedFor = PurchaseReturn.PriceAddedFor,
                                QuantityRemaining = convertedStock + freeConvertedStock,
                                FreeQuantity = PurchaseReturn.FreeQuantity,
                                UnitAddedFor = PurchaseReturn.UnitAddedFor,
                                QuantityReturned = convertedStock + freeConvertedStock,
                                TaxId = PurchaseReturn.TaxId,
                                DiscountType = PurchaseReturn.DiscountType,
                                Discount = PurchaseReturn.Discount,
                                UnitCost = PurchaseReturn.UnitCost,
                                AmountExcTax = PurchaseReturn.AmountExcTax,
                                TaxAmount = PurchaseReturn.TaxAmount,
                                AmountIncTax = PurchaseReturn.AmountIncTax,
                                PurchaseExcTax = PurchaseReturn.PurchaseExcTax,
                                PurchaseIncTax = PurchaseReturn.PurchaseIncTax,
                                AccountId = PurchaseAccountId,
                                DiscountAccountId = DiscountAccountId,
                                TaxAccountId = TaxAccountId,
                                PurchaseDetailsId = PurchaseReturn.PurchaseDetailsId,
                                ExtraDiscount = PurchaseReturn.ExtraDiscount,
                                ITCType = PurchaseReturn.ITCType,
                                TaxExemptionId = PurchaseReturn.TaxExemptionId,
                                TotalTaxAmount = PurchaseReturn.TotalTaxAmount
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsPurchaseReturnDetails.Add(oClsPurchaseReturnDetails);
                            oConnectionContext.SaveChanges();

                            if (AccountType != "")
                            {
                                AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                                ClsPurchaseReturnTaxJournal oClsPurchaseReturnTaxJournal = new ClsPurchaseReturnTaxJournal()
                                {
                                    PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                    PurchaseReturnDetailsId = oClsPurchaseReturnDetails.PurchaseReturnDetailsId,
                                    TaxId = PurchaseReturn.TaxId,
                                    TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : PurchaseReturn.TaxAmount,
                                    AccountId = AccountId,
                                    PurchaseReturnTaxJournalType = "Normal"
                                };
                                oConnectionContext.DbClsPurchaseReturnTaxJournal.Add(oClsPurchaseReturnTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                            foreach (var taxJournal in taxList)
                            {
                                ClsPurchaseReturnTaxJournal oClsPurchaseReturnTaxJournal = new ClsPurchaseReturnTaxJournal()
                                {
                                    PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                    PurchaseReturnDetailsId = oClsPurchaseReturnDetails.PurchaseReturnDetailsId,
                                    TaxId = taxJournal.TaxId,
                                    TaxAmount = taxJournal.TaxAmount,
                                    AccountId = taxJournal.AccountId,
                                    PurchaseReturnTaxJournalType = taxJournal.TaxType
                                };
                                oConnectionContext.DbClsPurchaseReturnTaxJournal.Add(oClsPurchaseReturnTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                            if (obj.Status.ToLower() != "draft")
                            {
                                string query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + ",\"QuantitySold\"=\"QuantitySold\"+" + (convertedStock + freeConvertedStock) + " where \"PurchaseId\"=" + obj.PurchaseId + " and \"ItemId\"=" + PurchaseReturn.ItemId + " and \"ItemDetailsId\"=" + PurchaseReturn.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);

                                query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + PurchaseReturn.ItemId + " and \"ItemDetailsId\"=" + PurchaseReturn.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query);
                            }
                        }
                        else
                        {
                            string AccountType = "";

                            var IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == PurchaseReturn.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault();

                            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                            //decimal AmountExcTax = obj.IsReverseCharge == 1 ? PurchaseReturn.AmountIncTax : PurchaseReturn.AmountExcTax;
                            decimal AmountExcTax = PurchaseReturn.AmountExcTax;
                            var taxs = IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == PurchaseReturn.TaxId).Select(a => new
                            {
                                a.TaxId,
                                a.Tax,
                                a.TaxPercent,
                            }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                           where a.TaxId == PurchaseReturn.TaxId
                                           select new
                                           {
                                               TaxId = a.SubTaxId,
                                               Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                               TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                           }).ToList();

                            foreach (var tax in taxs)
                            {
                                oClsTaxVm.Add(new ClsTaxVm
                                {
                                    TaxId = tax.TaxId,
                                    Tax = tax.Tax,
                                    TaxPercent = tax.TaxPercent,
                                    TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                                });
                            }

                            var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                                     (k, c) => new
                                     {
                                         TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                                         Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                                         TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                                         TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                                     }
                                    ).ToList();

                            List<ClsTaxVm> taxList = new List<ClsTaxVm>();

                            if (obj.CountryId == 2)
                            {
                                if (PurchaseReturn.ITCType == "Eligible For ITC")
                                {
                                    if (obj.IsReverseCharge == 1)
                                    {
                                        AccountType = "Reverse Charge Tax Input but not due";

                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Reverse Charge",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                    else
                                    {
                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Normal",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                }
                                else
                                {
                                    if (obj.IsReverseCharge == 1)
                                    {
                                        AccountType = "Tax Paid Expense";

                                        taxList = finalTaxs.Select(a => new ClsTaxVm
                                        {
                                            TaxType = "Reverse Charge",
                                            TaxId = a.TaxId,
                                            TaxPercent = a.TaxPercent,
                                            TaxAmount = a.TaxAmount,
                                            AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.SalesAccountId).FirstOrDefault()
                                        }).ToList();
                                    }
                                    else
                                    {
                                        AccountType = "Tax Paid Expense";
                                    }
                                }
                            }
                            else
                            {
                                taxList = finalTaxs.Select(a => new ClsTaxVm
                                {
                                    TaxType = "Normal",
                                    TaxId = a.TaxId,
                                    TaxPercent = a.TaxPercent,
                                    TaxAmount = a.TaxAmount,
                                    AccountId = oConnectionContext.DbClsTax.Where(css => css.TaxId == a.TaxId).Select(css => css.PurchaseAccountId).FirstOrDefault()
                                }).ToList();
                            }

                            ClsPurchaseReturnDetails oClsPurchaseReturnDetails = new ClsPurchaseReturnDetails()
                            {
                                PurchaseReturnDetailsId = PurchaseReturn.PurchaseReturnDetailsId,
                                //Amount = PurchaseReturn.Amount,
                                ItemId = PurchaseReturn.ItemId,
                                ItemDetailsId = PurchaseReturn.ItemDetailsId,
                                PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                //PurchaseReturnPrice = PurchaseReturn.PurchaseReturnPrice,
                                //Tax = PurchaseReturn.Tax,
                                //TaxId = PurchaseReturn.TaxId,
                                //Discount = PurchaseReturn.Discount,
                                //Quantity = PurchaseReturn.Quantity,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                Quantity = PurchaseReturn.Quantity,
                                PriceAddedFor = PurchaseReturn.PriceAddedFor,
                                QuantityRemaining = convertedStock + freeConvertedStock,
                                FreeQuantity = PurchaseReturn.FreeQuantity,
                                UnitAddedFor = PurchaseReturn.UnitAddedFor,
                                QuantityReturned = convertedStock + freeConvertedStock,
                                TaxId = PurchaseReturn.TaxId,
                                DiscountType = PurchaseReturn.DiscountType,
                                Discount = PurchaseReturn.Discount,
                                UnitCost = PurchaseReturn.UnitCost,
                                AmountExcTax = PurchaseReturn.AmountExcTax,
                                TaxAmount = PurchaseReturn.TaxAmount,
                                AmountIncTax = PurchaseReturn.AmountIncTax,
                                PurchaseExcTax = PurchaseReturn.PurchaseExcTax,
                                PurchaseIncTax = PurchaseReturn.PurchaseIncTax,
                                AccountId = PurchaseAccountId,
                                DiscountAccountId = DiscountAccountId,
                                TaxAccountId = TaxAccountId,
                                ExtraDiscount = PurchaseReturn.ExtraDiscount,
                                ITCType = PurchaseReturn.ITCType,
                                TaxExemptionId = PurchaseReturn.TaxExemptionId,
                                TotalTaxAmount = PurchaseReturn.TotalTaxAmount
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsPurchaseReturnDetails.Attach(oClsPurchaseReturnDetails);
                            //oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.Amount).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.ItemId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.ItemDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.PurchaseReturnId).IsModified = true;
                            //oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.PurchaseReturnPrice).IsModified = true;
                            //oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.TaxId).IsModified = true;
                            //oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.Discount).IsModified = true;
                            //oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.Quantity).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.UnitCost).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.Quantity).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.PriceAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.QuantityRemaining).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.FreeQuantity).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.UnitAddedFor).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.QuantityReturned).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.DiscountType).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.Discount).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.UnitCost).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.PurchaseExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.PurchaseIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.DiscountAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.TaxAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.ExtraDiscount).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.ITCType).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.TaxExemptionId).IsModified = true;
                            oConnectionContext.Entry(oClsPurchaseReturnDetails).Property(x => x.TotalTaxAmount).IsModified = true;
                            oConnectionContext.SaveChanges();

                            string query = "delete from \"tblPurchaseReturnTaxJournal\" where \"PurchaseReturnId\"=" + oClsPurchaseReturn.PurchaseReturnId + " and \"PurchaseReturnDetailsId\"=" + oClsPurchaseReturnDetails.PurchaseReturnDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            if (AccountType != "")
                            {
                                AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                                && a.IsActive == true && a.Type == AccountType).Select(a => a.AccountId).FirstOrDefault();

                                ClsPurchaseReturnTaxJournal oClsPurchaseReturnTaxJournal = new ClsPurchaseReturnTaxJournal()
                                {
                                    PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                    PurchaseReturnDetailsId = oClsPurchaseReturnDetails.PurchaseReturnDetailsId,
                                    TaxId = PurchaseReturn.TaxId,
                                    TaxAmount = obj.IsReverseCharge == 1 ? taxList.Select(a => a.TaxAmount).DefaultIfEmpty().Sum() : PurchaseReturn.TaxAmount,
                                    AccountId = AccountId,
                                    PurchaseReturnTaxJournalType = "Normal"
                                };
                                oConnectionContext.DbClsPurchaseReturnTaxJournal.Add(oClsPurchaseReturnTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                            foreach (var taxJournal in taxList)
                            {
                                ClsPurchaseReturnTaxJournal oClsPurchaseReturnTaxJournal = new ClsPurchaseReturnTaxJournal()
                                {
                                    PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                                    PurchaseReturnDetailsId = oClsPurchaseReturnDetails.PurchaseReturnDetailsId,
                                    TaxId = taxJournal.TaxId,
                                    TaxAmount = taxJournal.TaxAmount,
                                    AccountId = taxJournal.AccountId,
                                    PurchaseReturnTaxJournalType = taxJournal.TaxType
                                };
                                oConnectionContext.DbClsPurchaseReturnTaxJournal.Add(oClsPurchaseReturnTaxJournal);
                                oConnectionContext.SaveChanges();
                            }

                            if (obj.Status.ToLower() != "draft")
                            {
                                string query1 = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (convertedStock + freeConvertedStock) + ",\"QuantitySold\"=\"QuantitySold\"+" + (convertedStock + freeConvertedStock) + " where \"PurchaseDetailsId\"=" + PurchaseReturn.PurchaseDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query1);

                                query1 = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + (convertedStock + freeConvertedStock) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + PurchaseReturn.ItemId + " and \"ItemDetailsId\"=" + PurchaseReturn.ItemDetailsId;
                                oConnectionContext.Database.ExecuteSqlCommand(query1);
                            }
                        }
                        //}
                    }
                }

                if (obj.Status.ToLower() != "draft")
                {
                    decimal PurchaseDue = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                  (oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment") && b.IsDeleted == false && b.IsCancelled == false
                  && b.PurchaseId == obj.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                  oConnectionContext.DbClsSupplierPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false
                  && b.PurchaseId == obj.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum());

                    ClsSupplierPaymentVm oClsSupplierPayment = new ClsSupplierPaymentVm
                    {
                        PurchaseId = obj.PurchaseId,
                        PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                        CompanyId = obj.CompanyId,
                        BranchId = obj.BranchId,
                        AddedBy = obj.AddedBy,
                        IsActive = true,
                        IsDeleted = false,
                        Amount = obj.GrandTotal,
                        PaymentDate = CurrentDate,
                        SupplierId = obj.SupplierId,
                        Type = "Supplier Payment",
                        AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Petty Cash").Select(a => a.AccountId).FirstOrDefault(),
                        AmountExcTax = obj.GrandTotal,
                        TaxAccountId = 0,
                        TaxAmount = 0,
                        TaxId = 0,
                        IsReverseCharge = obj.IsReverseCharge,
                    };

                    if (PurchaseDue >= obj.GrandTotal)
                    {
                        oClsSupplierPayment.SupplierPaymentIds = new List<ClsSupplierPaymentIds> { new ClsSupplierPaymentIds()
                    {
                        Due= PurchaseDue,
                        Amount= obj.GrandTotal,
                        PurchaseId = obj.PurchaseId,
                        Type = "Purchase Payment"
                    }
                    };
                    }
                    else if (PurchaseDue < obj.GrandTotal)
                    {
                        oClsSupplierPayment.SupplierPaymentIds = new List<ClsSupplierPaymentIds> { new ClsSupplierPaymentIds()
                    {
                        Due= PurchaseDue,
                        Amount= PurchaseDue,
                        PurchaseId = obj.PurchaseId,
                        Type = "Purchase Payment"
                    }
                    };
                    }
                    oCommonController.DebitNoteCreditsApply(oClsSupplierPayment);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Return",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Return \"" + oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" updated",
                    Id = oClsPurchaseReturn.PurchaseReturnId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Debit Note", obj.CompanyId, oClsPurchaseReturn.PurchaseReturnId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Return updated successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Purchase = new
                        {
                            InvoiceId = oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == oClsPurchaseReturn.PurchaseReturnId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        PurchaseSetting = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.AutoPrintInvoicePurchaseQuotation, a.AutoPrintInvoicePurchaseOrder, a.AutoPrintInvoicePurchaseBill, a.AutoPrintInvoicePurchaseReturn }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseReturnDetailsDelete(ClsPurchaseReturnDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.PurchaseReturnId != 0)
                {
                    obj.BranchId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == oConnectionContext.DbClsPurchaseReturn.
                    Where(b => b.PurchaseReturnId == obj.PurchaseReturnId).Select(b => b.PurchaseId).FirstOrDefault()).Select(a => a.BranchId).FirstOrDefault();

                    var details = oConnectionContext.DbClsPurchaseReturnDetails.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId && a.IsDeleted == false).Select(a => new
                    {
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                        a.PriceAddedFor,
                        a.PurchaseDetailsId
                    }).ToList();

                    string query = "update \"tblPurchaseReturnDetails\" set \"IsDeleted\"=True where \"PurchaseReturnId\"=" + obj.PurchaseReturnId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    foreach (var item in details)
                    {
                        decimal convertedStock = oCommonController.StockConversion(item.Quantity, item.ItemId, item.PriceAddedFor);

                        query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + convertedStock + ",\"QuantitySold\"=\"QuantitySold\"-" + convertedStock + " where \"PurchaseDetailsId\"=" + item.PurchaseDetailsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);

                        query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + convertedStock + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + item.ItemId + " and \"ItemDetailsId\"=" + item.ItemDetailsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query);
                    }
                }
                else
                {
                    obj.BranchId = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == oConnectionContext.DbClsPurchaseReturn.
                    Where(b => b.PurchaseReturnId == oConnectionContext.DbClsPurchaseReturnDetails.Where(c => c.PurchaseReturnDetailsId == obj.PurchaseReturnDetailsId).Select(c => c.PurchaseReturnId).FirstOrDefault()).Select(b => b.PurchaseId).FirstOrDefault()).Select(a => a.BranchId).FirstOrDefault();

                    var details = oConnectionContext.DbClsPurchaseReturnDetails.Where(a => a.PurchaseReturnDetailsId == obj.PurchaseReturnDetailsId).Select(a => new
                    {
                        a.ItemDetailsId,
                        a.ItemId,
                        a.Quantity,
                        a.PriceAddedFor,
                        a.PurchaseDetailsId
                    }).FirstOrDefault();

                    string query = "update \"tblPurchaseReturnDetails\" set \"IsDeleted\"=True where \"PurchaseReturnDetailsId\"=" + obj.PurchaseReturnDetailsId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    decimal convertedStock = oCommonController.StockConversion(details.Quantity, details.ItemId, details.PriceAddedFor);

                    query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"+" + convertedStock + ",\"QuantitySold\"=\"QuantitySold\"-" + convertedStock + " where \"PurchaseDetailsId\"=" + details.PurchaseDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"+" + convertedStock + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + details.ItemId + " and \"ItemDetailsId\"=" + details.ItemDetailsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                data = new
                {
                    Status = 1,
                    Message = "Deleted successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePurchaseReturnStatus(ClsPurchaseReturnVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Status == "" || obj.Status == null)
                {
                    errors.Add(new ClsError { Message = "Purchase return Status is required", Id = "divPurchaseReturnStatus" });
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

                obj = (from a in oConnectionContext.DbClsPurchaseReturn
                       where a.PurchaseReturnId == obj.PurchaseReturnId && a.CompanyId == obj.CompanyId
                       select new ClsPurchaseReturnVm
                       {
                           CompanyId = a.CompanyId,
                           GrandTotal = a.GrandTotal,
                           SupplierId = a.SupplierId,
                           PurchaseId = a.PurchaseId,
                           Status = obj.Status,
                           PurchaseReturnId = obj.PurchaseReturnId,
                           BranchId = a.BranchId,
                           IsReverseCharge = a.IsReverseCharge,
                           PurchaseReturnDetails = (from z in oConnectionContext.DbClsPurchaseReturnDetails
                                                    join b in oConnectionContext.DbClsPurchaseDetails
                                                    on z.PurchaseDetailsId equals b.PurchaseDetailsId
                                                    join c in oConnectionContext.DbClsItemDetails
                                                    on b.ItemDetailsId equals c.ItemDetailsId
                                                    join d in oConnectionContext.DbClsItem
                                                    on c.ItemId equals d.ItemId
                                                    where z.PurchaseReturnId == a.PurchaseReturnId && z.IsDeleted == false
                                                    && b.IsDeleted == false
                                                    select new ClsPurchaseReturnDetailsVm
                                                    {
                                                        QuantityReturned = z.QuantityReturned,
                                                        ItemId = d.ItemId,
                                                        PurchaseDetailsId = b.PurchaseDetailsId,
                                                        ItemDetailsId = b.ItemDetailsId,
                                                    }).ToList(),
                       }).FirstOrDefault();

                ClsPurchaseReturn oClsPurchaseReturn = new ClsPurchaseReturn()
                {
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    PurchaseReturnId = obj.PurchaseReturnId,
                    Status = obj.Status,
                };

                oConnectionContext.DbClsPurchaseReturn.Attach(oClsPurchaseReturn);
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.PurchaseReturnId).IsModified = true;
                oConnectionContext.Entry(oClsPurchaseReturn).Property(x => x.Status).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.PurchaseReturnDetails != null)
                {
                    foreach (var PurchaseReturn in obj.PurchaseReturnDetails)
                    {
                        if (obj.Status.ToLower() != "draft")
                        {
                            string query = "update \"tblPurchaseDetails\" set \"QuantityRemaining\"=\"QuantityRemaining\"-" + (PurchaseReturn.QuantityReturned) + ",\"QuantitySold\"=\"QuantitySold\"+" + (PurchaseReturn.QuantityReturned) + " where \"PurchaseDetailsId\"=" + PurchaseReturn.PurchaseDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);

                            query = "update \"tblItemBranchMap\" set \"Quantity\"=\"Quantity\"-" + (PurchaseReturn.QuantityReturned) + " where \"BranchId\"=" + obj.BranchId + " and \"ItemId\"=" + PurchaseReturn.ItemId + " and \"ItemDetailsId\"=" + PurchaseReturn.ItemDetailsId;
                            oConnectionContext.Database.ExecuteSqlCommand(query);
                        }
                    }
                }

                if (obj.Status.ToLower() != "draft")
                {
                    decimal PurchaseDue = oConnectionContext.DbClsPurchase.Where(a => a.PurchaseId == obj.PurchaseId).Select(a => a.GrandTotal).DefaultIfEmpty().Sum() -
                  (oConnectionContext.DbClsSupplierPayment.Where(b => (b.Type.ToLower() == "purchase payment") && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == obj.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum() -
                  oConnectionContext.DbClsSupplierPayment.Where(b => b.Type == "Change Return" && b.IsDeleted == false && b.IsCancelled == false
                  && b.PurchaseId == obj.PurchaseId).Select(b => b.Amount).DefaultIfEmpty().Sum());

                    ClsSupplierPaymentVm oClsSupplierPayment = new ClsSupplierPaymentVm
                    {
                        PurchaseReturnId = oClsPurchaseReturn.PurchaseReturnId,
                        CompanyId = obj.CompanyId,
                        BranchId = obj.BranchId,
                        AddedBy = obj.AddedBy,
                        IsActive = true,
                        IsDeleted = false,
                        Amount = obj.GrandTotal,
                        PaymentDate = CurrentDate,
                        SupplierId = obj.SupplierId,
                        Type = "Supplier Payment",
                        AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                && a.Type == "Petty Cash").Select(a => a.AccountId).FirstOrDefault(),
                        AmountExcTax = obj.GrandTotal,
                        TaxAccountId = 0,
                        TaxAmount = 0,
                        TaxId = 0,
                        IsReverseCharge = obj.IsReverseCharge,
                    };

                    if (PurchaseDue >= obj.GrandTotal)
                    {
                        oClsSupplierPayment.SupplierPaymentIds = new List<ClsSupplierPaymentIds> { new ClsSupplierPaymentIds()
                    {
                        Due= PurchaseDue,
                        Amount= obj.GrandTotal,
                        PurchaseId = obj.PurchaseId,
                        Type = "Purchase Payment"
                    }
                    };
                    }
                    else if (PurchaseDue < obj.GrandTotal)
                    {
                        oClsSupplierPayment.SupplierPaymentIds = new List<ClsSupplierPaymentIds> { new ClsSupplierPaymentIds()
                    {
                        Due= PurchaseDue,
                        Amount= PurchaseDue,
                        PurchaseId = obj.PurchaseId,
                        Type = "Purchase Payment"
                    }
                    };
                    }
                    oCommonController.DebitNoteCreditsApply(oClsSupplierPayment);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Purchase Return",
                    CompanyId = obj.CompanyId,
                    Description = "Purchase Return \"" + oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == obj.PurchaseReturnId).Select(a => a.InvoiceNo).FirstOrDefault() + "\" updated",
                    Id = obj.PurchaseReturnId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Purchase Debit Note", obj.CompanyId, oClsPurchaseReturn.PurchaseReturnId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Purchase Return status changed successfully",
                    //WhatsappUrl = arr[2],
                    Data = new
                    {
                        Purchase = new
                        {
                            InvoiceId = oConnectionContext.DbClsPurchaseReturn.Where(a => a.PurchaseReturnId == oClsPurchaseReturn.PurchaseReturnId).Select(a => a.InvoiceId).FirstOrDefault(),
                        },
                        PurchaseSetting = oConnectionContext.DbClsPurchaseSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new { a.AutoPrintInvoicePurchaseQuotation, a.AutoPrintInvoicePurchaseOrder, a.AutoPrintInvoicePurchaseBill, a.AutoPrintInvoicePurchaseReturn }).FirstOrDefault(),
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseReturnReport(ClsPurchaseVm obj)
        {
            var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            {
                a.IsCompany,
                a.UserRoleId,
                BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
              && b.IsDeleted == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
               oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
                 && b.IsDeleted == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
            }).FirstOrDefault();

            if (obj.BranchId == 0)
            {
                obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
            }

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = (from a in oConnectionContext.DbClsPurchaseReturn
                       join b in oConnectionContext.DbClsPurchase
   on a.PurchaseId equals b.PurchaseId
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.BranchId == obj.BranchId
                       select new
                       {
                           a.PurchaseId,
                           b.BranchId,
                           Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == b.BranchId).Select(b => b.Branch).FirstOrDefault(),
                           PurchaseReturnId = a.PurchaseReturnId,
                           a.GrandTotal,
                           a.Notes,
                           a.Date,
                           a.InvoiceNo,
                           a.Subtotal,
                           b.SupplierId,
                           SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == b.SupplierId).Select(c => c.Name).FirstOrDefault(),
                           CompanyId = a.CompanyId,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,

                           ModifiedOn = a.ModifiedOn,
                           Paid = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseReturnId).Count() == 0 ? 0 :
                       oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseReturnId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                           a.Status,
                           Due = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseReturnId).Count() == 0 ? a.GrandTotal :
                       a.GrandTotal - oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseId == a.PurchaseReturnId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                           a.TotalQuantity,
                           b.ReferenceNo

                       }).ToList();

            if (obj.ReferenceNo != null && obj.ReferenceNo != "")
            {
                det = det.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower()).Select(a => a).ToList();
            }
            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            {
                det = det.Where(a => a.Date.Date >= obj.FromDate && a.Date.Date <= obj.ToDate).ToList();
            }

            if (obj.Status != null && obj.Status != "")
            {
                det = det.Where(a => a.Status.ToLower() == obj.Status.ToLower()).Select(a => a).ToList();
            }

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.SupplierId == obj.SupplierId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Purchases = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Invoice(ClsPurchaseReturnVm obj)
        {
            var det = (from a in oConnectionContext.DbClsPurchaseReturn
                           //                       join z in oConnectionContext.DbClsPurchase
                           //on a.PurchaseId equals z.PurchaseId
                       where a.IsDeleted == false && a.IsCancelled == false && a.InvoiceId == obj.InvoiceId
                       select new
                       {
                           a.IsCancelled,
                           a.PurchaseReturnId,
                           User = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => new
                           {
                               c.Name,
                               c.MobileNo,
                               c.EmailId,
                               //c.TaxNo,
                               //Tax = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == c.TaxId).Select(bb => bb.Tax).FirstOrDefault(),
                               TaxNo = c.BusinessRegistrationNo,
                               Addresses = oConnectionContext.DbClsAddress.Where(b => b.UserId == a.SupplierId).Select(b => new
                               {
                                   b.MobileNo,
                                   b.Name,
                                   b.EmailId,
                                   b.Address,
                                   City = oConnectionContext.DbClsCity.Where(cc => cc.CityId == b.CityId).Select(cc => cc.City).FirstOrDefault(),
                                   State = oConnectionContext.DbClsState.Where(cc => cc.StateId == b.StateId).Select(cc => cc.State).FirstOrDefault(),
                                   Country = oConnectionContext.DbClsCountry.Where(cc => cc.CountryId == b.CountryId).Select(cc => cc.Country).FirstOrDefault(),
                                   b.Zipcode
                               }).ToList(),
                           }).FirstOrDefault(),
                           Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => new
                           {
                               b.Branch,
                               b.Mobile,
                               b.Email,
                               //b.TaxNo,
                               //Tax = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == b.TaxId).Select(bb => bb.Tax).FirstOrDefault(),
                               TaxNo = oConnectionContext.DbClsTaxSetting.Where(c => c.IsDeleted == false
                    && c.TaxSettingId == b.TaxSettingId).Select(c => c.BusinessRegistrationNo).FirstOrDefault(),
                               Tax = oConnectionContext.DbClsBusinessRegistrationName.Where(d => d.BusinessRegistrationNameId ==
                               oConnectionContext.DbClsTaxSetting.Where(c => c.IsDeleted == false
                               && c.TaxSettingId == b.TaxSettingId).Select(c => c.BusinessRegistrationNameId).FirstOrDefault()).Select(d => d.Name).FirstOrDefault(),
                               b.Address,
                               b.AltMobileNo,
                               City = oConnectionContext.DbClsCity.Where(cc => cc.CityId == b.CityId).Select(cc => cc.City).FirstOrDefault(),
                               State = oConnectionContext.DbClsState.Where(cc => cc.StateId == b.StateId).Select(cc => cc.State).FirstOrDefault(),
                               Country = oConnectionContext.DbClsCountry.Where(cc => cc.CountryId == b.CountryId).Select(cc => cc.Country).FirstOrDefault(),
                               b.Zipcode
                           }).FirstOrDefault(),
                           a.Status,
                           //         Due = oConnectionContext.DbClsSupplierRefund.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseReturnId == a.PurchaseReturnId).Count() == 0 ? a.GrandTotal :
                           //a.GrandTotal - oConnectionContext.DbClsSupplierRefund.Where(b => b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false && b.PurchaseReturnId == a.PurchaseReturnId).Select(b => b.Amount).DefaultIfEmpty().Sum(),
                           a.Notes,
                           a.InvoiceNo,
                           a.Date,
                           a.DueDate,
                           a.Subtotal,
                           a.Discount,
                           a.DiscountType,
                           a.TotalDiscount,
                           a.GrandTotal,
                           a.TotalQuantity,
                           a.CompanyId,
                           a.TaxAmount,
                           a.TaxId,
                           a.RoundOff,
                           SpecialDiscount = a.SpecialDiscount,
                           a.NetAmount,
                           //Payments = oConnectionContext.DbClsSupplierRefund.Where(b => b.PurchaseReturnId == a.PurchaseReturnId && b.Type.ToLower() == "supplier refund" && b.IsDeleted == false && b.IsCancelled == false).Select(b => new
                           //{
                           //    b.PaymentDate,
                           //    b.ReferenceNo,
                           //    b.Notes,
                           //    b.Amount,
                           //    b.PaymentTypeId,
                           //    PaymentType = oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId).Select(c => c.PaymentType).FirstOrDefault()
                           //}),
                           PurchaseDetails = (from b in oConnectionContext.DbClsPurchaseReturnDetails
                                              join c in oConnectionContext.DbClsItemDetails
                                              on b.ItemDetailsId equals c.ItemDetailsId
                                              join d in oConnectionContext.DbClsItem
                                              on c.ItemId equals d.ItemId
                                              where b.PurchaseReturnId == a.PurchaseReturnId && b.IsDeleted == false
                                              select new
                                              {
                                                  d.ProductImage,
                                                  b.TotalTaxAmount,
                                                  Unit = b.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(e => e.UnitId == d.UnitId).Select(e => e.UnitShortName).FirstOrDefault()
                                    : b.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(e => e.SecondaryUnitId == d.SecondaryUnitId).Select(e => e.SecondaryUnitShortName).FirstOrDefault()
                                    : b.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(e => e.TertiaryUnitId == d.TertiaryUnitId).Select(e => e.TertiaryUnitShortName).FirstOrDefault()
                                    : oConnectionContext.DbClsQuaternaryUnit.Where(e => e.QuaternaryUnitId == d.QuaternaryUnitId).Select(e => e.QuaternaryUnitShortName).FirstOrDefault(),
                                                  //b.DiscountType,
                                                  //b.SalesDetailsId,
                                                  //b.PriceIncTax,
                                                  //b.OtherInfo,
                                                  //b.Amount,
                                                  //b.Discount,
                                                  b.PurchaseReturnId,
                                                  b.Quantity,
                                                  b.FreeQuantity,
                                                  //b.TaxId,
                                                  b.UnitCost,
                                                  d.ItemId,
                                                  d.ProductType,
                                                  c.ItemDetailsId,
                                                  d.ItemName,
                                                  SKU = c.SKU == null ? d.SkuCode : c.SKU,
                                                  c.VariationDetailsId,
                                                  VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                                  UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                                  c.SalesExcTax,
                                                  c.SalesIncTax,
                                                  c.TotalCost,
                                                  //Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                                                  //TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                                                  d.TaxType,
                                                  d.ItemCode,
                                                  b.PurchaseExcTax,
                                                  b.PurchaseIncTax,
                                                  //b.Amount,
                                                  b.DiscountType,
                                                  b.Discount,
                                                  b.AmountExcTax,
                                                  b.TaxAmount,
                                                  b.AmountIncTax
                                              }).ToList(),
                           PurchaseAdditionalCharges = oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Where(b => b.PurchaseReturnId == a.PurchaseReturnId
                  && b.IsDeleted == false && b.IsActive == true).Select(b => new ClsPurchaseReturnAdditionalChargesVm
                  {
                      PurchaseReturnAdditionalChargesId = b.PurchaseReturnAdditionalChargesId,
                      Name = oConnectionContext.DbClsAdditionalCharge.Where(c => c.AdditionalChargeId == b.AdditionalChargeId).Select(c => c.Name).FirstOrDefault(),
                      AdditionalChargeId = b.AdditionalChargeId,
                      PurchaseReturnId = b.PurchaseReturnId,
                      TaxId = b.TaxId,
                      AmountExcTax = b.AmountExcTax,
                      AmountIncTax = b.AmountIncTax,
                      TaxAmount = b.AmountIncTax - b.AmountExcTax,
                      AccountId = b.AccountId,
                      ItemCodeId = b.ItemCodeId,
                      TaxExemptionId = b.TaxExemptionId,
                      TaxExemptionReason = oConnectionContext.DbClsTaxExemption.Where(c => c.TaxExemptionId == b.TaxExemptionId).Select(c => c.Reason).FirstOrDefault(),
                  }).ToList(),
                       }).FirstOrDefault();

            var AllTaxs = oConnectionContext.DbClsPurchaseReturn.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.PurchaseReturnId == det.PurchaseReturnId).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.Subtotal - a.TotalDiscount
            }).Concat(oConnectionContext.DbClsPurchaseReturnDetails.Where(a => a.PurchaseReturnId == det.PurchaseReturnId && a.IsDeleted == false).Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                AmountExcTax = a.AmountExcTax
            })).Concat(oConnectionContext.DbClsPurchaseReturnAdditionalCharges.Where(a => a.PurchaseReturnId == det.PurchaseReturnId
                                && a.IsDeleted == false && a.AmountExcTax>0).Select(a => new
                                {
                                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                                    a.TaxId,
                                    AmountExcTax = a.AmountExcTax
                                })).ToList();

            List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
            foreach (var item in AllTaxs)
            {
                bool CanDelete = oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => a.CanDelete).FirstOrDefault();
                if (CanDelete == true)
                {
                    decimal AmountExcTax = item.AmountExcTax;
                    var taxs = item.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == item.TaxId).Select(a => new
                    {
                        a.TaxId,
                        a.Tax,
                        a.TaxPercent,
                    }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                                   where a.TaxId == item.TaxId
                                   select new
                                   {
                                       TaxId = a.SubTaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                                   }).ToList();

                    foreach (var tax in taxs)
                    {
                        oClsTaxVm.Add(new ClsTaxVm
                        {
                            TaxId = tax.TaxId,
                            Tax = tax.Tax,
                            TaxPercent = tax.TaxPercent,
                            TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                        });
                    }
                }
            }

            var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                     (k, c) => new
                     {
                         //TaxId = k,
                         Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                         TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                         TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                     }
                    ).ToList();

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => new
            {
                a.BusinessLogo,
                a.BusinessName,
                a.DateFormat,
                a.TimeFormat,
                a.CurrencySymbolPlacement,
                CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
                CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Purchase = det,
                    BusinessSetting = BusinessSetting,
                    Taxs = finalTaxs,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseReturnJournal(ClsPurchaseReturnVm obj)
        {
            var taxList = (from q in oConnectionContext.DbClsPurchaseReturnTaxJournal
                           join a in oConnectionContext.DbClsPurchaseReturnDetails
                           on q.PurchaseReturnDetailsId equals a.PurchaseReturnDetailsId
                           join b in oConnectionContext.DbClsPurchaseReturn
                        on a.PurchaseReturnId equals b.PurchaseReturnId
                           //join c in oConnectionContext.DbClsTax
                           //     on q.TaxId equals c.TaxId
                           where q.PurchaseReturnId == obj.PurchaseReturnId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                           && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           //&& a.TaxAmount != 0
                           //&& c.TaxTypeId != 0
                           select new
                           {
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = (q.PurchaseReturnTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                               Credit = (q.PurchaseReturnTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                               AccountId = q.AccountId
                           }).Concat(from q in oConnectionContext.DbClsPurchaseReturnAdditionalTaxJournal
                                     join a in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                     on q.PurchaseReturnAdditionalChargesId equals a.PurchaseReturnAdditionalChargesId
                                     join b in oConnectionContext.DbClsPurchaseReturn
                                  on a.PurchaseReturnId equals b.PurchaseReturnId
                                     //join c in oConnectionContext.DbClsTax
                                     //     on q.TaxId equals c.TaxId
                                     where q.PurchaseReturnId == obj.PurchaseReturnId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                     //&& a.TaxAmount != 0
                                     //&& c.TaxTypeId != 0
                                     && a.AmountExcTax > 0
                                     select new
                                     {
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == q.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = (q.PurchaseReturnTaxJournalType == "Normal") ? 0 : (b.IsReverseCharge == 1 ? q.TaxAmount : 0),
                                         Credit = (q.PurchaseReturnTaxJournalType == "Normal") ? q.TaxAmount : (b.IsReverseCharge == 1 ? 0 : q.TaxAmount),
                                         AccountId = q.AccountId
                                     }).ToList();

            var journal = (from a in oConnectionContext.DbClsPurchaseReturn
                               //   join b in oConnectionContext.DbClsPurchaseReturnDetails
                               //on a.PurchaseReturnId equals b.PurchaseReturnId
                           where a.PurchaseReturnId == obj.PurchaseReturnId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                           //&& b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                           select new ClsBankPaymentVm
                           {
                               //Account Payable
                               AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                               Debit = a.IsReverseCharge == 1 ? a.GrandTotalReverseCharge : a.GrandTotal,//a.GrandTotal,
                               Credit = 0
                           }).Concat(from a in oConnectionContext.DbClsPurchaseReturnDetails
                                     join b in oConnectionContext.DbClsPurchaseReturn
                                  on a.PurchaseReturnId equals b.PurchaseReturnId
                                     where a.PurchaseReturnId == obj.PurchaseReturnId && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                     group a by a.AccountId into stdGroup
                                     select new ClsBankPaymentVm
                                     {
                                         // Purchase account
                                         AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == stdGroup.Key).Select(c => c.AccountName).FirstOrDefault(),
                                         Debit = 0,
                                         Credit = stdGroup.Select(x => x.UnitCost * x.Quantity).DefaultIfEmpty().Sum(),
                                     }).Concat(from a in oConnectionContext.DbClsPurchaseReturn
                                                         where a.PurchaseReturnId == obj.PurchaseReturnId && a.CompanyId == obj.CompanyId
                                                         && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                         select new ClsBankPaymentVm
                                                         {
                                                             // Round off charge
                                                             AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.RoundOffAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                             Debit = a.RoundOff < 0 ? (a.RoundOff * -1) : 0,
                                                             Credit = a.RoundOff > 0 ? a.RoundOff : 0,
                                                         }).Concat(from a in oConnectionContext.DbClsPurchaseReturn
                                                                   where a.PurchaseReturnId == obj.PurchaseReturnId && a.CompanyId == obj.CompanyId
                                                                   && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                   select new ClsBankPaymentVm
                                                                   {
                                                                       // special discount 
                                                                       AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.SpecialDiscountAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                                       Debit = a.SpecialDiscount,
                                                                       Credit = 0,
                                                                   }).Concat(from a in oConnectionContext.DbClsPurchaseReturn
                                                                   where a.PurchaseReturnId == obj.PurchaseReturnId && a.CompanyId == obj.CompanyId
                                                                   && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
                                                                   select new ClsBankPaymentVm
                                                                   {
                                                                       // discount 
                                                                       AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.DiscountAccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                                       Debit = a.TotalDiscount,
                                                                       Credit = 0,
                                                                   })
                                                         .Concat(from a in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                                                 where a.PurchaseReturnId == obj.PurchaseReturnId && a.CompanyId == obj.CompanyId
                                                                 && a.IsDeleted == false && a.IsActive == true
                                                                 select new ClsBankPaymentVm
                                                                 {
                                                                     // Write Off journal account 
                                                                     AccountName = oConnectionContext.DbClsAccount.Where(c => c.AccountId == a.AccountId).Select(c => c.AccountName).FirstOrDefault(),
                                                                     Debit = 0,
                                                                     Credit = a.AmountExcTax,
                                                                 }).ToList();

            journal = journal.Concat(from a in taxList
                                     group a by a.AccountId into taxGroup
                                     select new ClsBankPaymentVm
                                     {
                                         // tax 
                                         AccountName = taxGroup.FirstOrDefault()?.AccountName,
                                         Debit = taxGroup.Select(x => x.Debit).DefaultIfEmpty().Sum(),
                                         Credit = taxGroup.Select(x => x.Credit).DefaultIfEmpty().Sum(),
                                         IsTaxAccount = true
                                     }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = journal
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseReturnSearchItems(ClsPurchaseReturnVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            var Item = oConnectionContext.DbClsItem.Where(a => a.SkuCode == obj.ItemCode.Trim() && a.IsActive == true
            && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.ItemId,
                ItemDetailsId = 0L,
                //a.ProductType,
                a.ItemType,
            }).FirstOrDefault();

            if (Item == null)
            {
                Item = oConnectionContext.DbClsItemDetails.Where(a => a.SKU == obj.ItemCode.Trim() && a.IsActive == true
            && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.ItemId,
                a.ItemDetailsId,
                //a.ProductType,
                ItemType = oConnectionContext.DbClsItem.Where(b => b.ItemId == a.ItemId).Select(b => b.ItemType).FirstOrDefault(),
            }).FirstOrDefault();
            }

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.CountryId
            }).FirstOrDefault();

            //if (Item.ItemType.ToLower() == "product")
            //{
            //    decimal QuantityRemaining = (from b in oConnectionContext.DbClsPurchaseDetails
            //                                 join c in oConnectionContext.DbClsItemDetails
            //                                 on b.ItemDetailsId equals c.ItemDetailsId
            //                                 join d in oConnectionContext.DbClsItem
            //                                 on c.ItemId equals d.ItemId
            //                                 where b.PurchaseId == obj.PurchaseId && b.IsDeleted == false && b.IsCancelled == false
            //                                 && d.ItemId == Item.ItemId
            //                                 select b.QuantityRemaining).FirstOrDefault();

            //    if (QuantityRemaining == 0)
            //    {
            //        data = new
            //        {
            //            Status = 0,
            //            Message = "Sorry this item is not associated with this Purchase Bill",
            //            Data = new
            //            {

            //            }
            //        };
            //    }
            //}

            if (Item.ItemType.ToLower() == "product")
            {
                var det = (from b in oConnectionContext.DbClsPurchaseDetails
                           join c in oConnectionContext.DbClsItemDetails
                           on b.ItemDetailsId equals c.ItemDetailsId
                           join d in oConnectionContext.DbClsItem
                           on c.ItemId equals d.ItemId
                           where b.PurchaseId == obj.PurchaseId && b.IsDeleted == false
                           && d.ItemId == Item.ItemId && b.QuantityRemaining > 0
                           select new
                           {
                               b.DiscountType,
                               d.IsManageStock,
                               b.ExtraDiscount,
                               b.PurchaseDetailsId,
                               QuantityReturned = 0,
                               QuantityReturnedPriceAddedFor = b.PriceAddedFor,
                               PurchaseReturnUnitCost = b.PurchaseIncTax,
                               FreeQuantityReturned = 0,
                               //FreeQuantityReturnedPriceAddedFor = b.FreeQuantityPriceAddedFor,
                               b.QuantityRemaining,
                               PurchaseReturnPrice = b.PurchaseIncTax,
                               AmountExcTax = 0,
                               AmountIncTax = 0,
                               b.Discount,
                               Quantity = b.Quantity,
                               b.FreeQuantity,
                               //b.FreeQuantityPriceAddedFor,
                               b.TaxId,
                               InterStateTaxId = b.TaxId,
                               b.UnitCost,
                               d.ItemId,
                               d.ProductType,
                               c.ItemDetailsId,
                               d.ItemName,
                               SKU = d.ProductType == "Single" ? d.SkuCode : c.SKU,
                               c.VariationDetailsId,
                               VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                               b.PurchaseExcTax,
                               b.PurchaseIncTax,
                               c.TotalCost,
                               Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                               TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == b.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                               d.TaxType,
                               d.ItemCode,
                               UnitId = d.UnitId,
                               SecondaryUnitId = d.SecondaryUnitId,
                               TertiaryUnitId = d.TertiaryUnitId,
                               QuaternaryUnitId = d.QuaternaryUnitId,
                               UnitShortName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitShortName).FirstOrDefault(),
                               SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitShortName).FirstOrDefault(),
                               TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitShortName).FirstOrDefault(),
                               QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitShortName).FirstOrDefault(),
                               UToSValue = d.UToSValue,
                               SToTValue = d.SToTValue,
                               TToQValue = d.TToQValue,
                               AllowDecimal = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.AllowDecimal).FirstOrDefault(),
                               SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitAllowDecimal).FirstOrDefault(),
                               TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitAllowDecimal).FirstOrDefault(),
                               QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                               PriceAddedFor = b.PriceAddedFor,
                           }).ToList();

                if (det == null || det.Count == 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Sorry this item is not associated with this Purchase Bill",
                        Data = new
                        {

                        }
                    };
                }
                else
                {
                    if (Item.ItemDetailsId != 0)
                    {
                        det = det.Where(a => a.ItemDetailsId == Item.ItemDetailsId).ToList();
                    }

                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            ItemDetails = det,
                            BusinessSetting = new
                            {
                                CountryId = BusinessSetting.CountryId,
                            }
                        }
                    };
                }
            }
            else
            {
                var det = (from b in oConnectionContext.DbClsItemBranchMap
                           join c in oConnectionContext.DbClsItemDetails
                           on b.ItemDetailsId equals c.ItemDetailsId
                           join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                           where b.ItemId == Item.ItemId && c.ItemId == Item.ItemId
                           && b.BranchId == obj.BranchId
                           && b.IsActive == true && d.IsActive == true && d.IsDeleted == false
                           select new
                           {
                               d.IsManageStock,
                               //b.ExtraDiscount,
                               //b.PurchaseDetailsId,
                               QuantityReturned = 0,
                               QuantityReturnedPriceAddedFor = c.PriceAddedFor,
                               PurchaseReturnUnitCost = c.PurchaseIncTax,
                               FreeQuantityReturned = 0,
                               //FreeQuantityReturnedPriceAddedFor = b.FreeQuantityPriceAddedFor,
                               QuantityRemaining = b.Quantity,
                               PurchaseReturnPrice = c.PurchaseIncTax,
                               AmountExcTax = 0,
                               AmountIncTax = 0,
                               //b.Discount,
                               //Quantity = b.Quantity,
                               //b.FreeQuantity,
                               d.TaxId,
                               InterStateTaxId = d.InterStateTaxId,
                               UnitCost = c.PurchaseExcTax,
                               d.ItemId,
                               d.ProductType,
                               c.ItemDetailsId,
                               d.ItemName,
                               SKU = d.ProductType == "Single" ? d.SkuCode : c.SKU,
                               c.VariationDetailsId,
                               VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                               c.PurchaseExcTax,
                               c.PurchaseIncTax,
                               c.TotalCost,
                               Tax = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == d.TaxId).Select(cc => cc.Tax).FirstOrDefault(),
                               TaxPercent = oConnectionContext.DbClsTax.Where(cc => cc.TaxId == d.TaxId).Select(cc => cc.TaxPercent).FirstOrDefault(),
                               d.TaxType,
                               d.ItemCode,
                               UnitId = d.UnitId,
                               SecondaryUnitId = d.SecondaryUnitId,
                               TertiaryUnitId = d.TertiaryUnitId,
                               QuaternaryUnitId = d.QuaternaryUnitId,
                               UnitShortName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitShortName).FirstOrDefault(),
                               SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitShortName).FirstOrDefault(),
                               TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitShortName).FirstOrDefault(),
                               QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitShortName).FirstOrDefault(),
                               UToSValue = d.UToSValue,
                               SToTValue = d.SToTValue,
                               TToQValue = d.TToQValue,
                               AllowDecimal = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.AllowDecimal).FirstOrDefault(),
                               SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == d.SecondaryUnitId).Select(cc => cc.SecondaryUnitAllowDecimal).FirstOrDefault(),
                               TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == d.TertiaryUnitId).Select(cc => cc.TertiaryUnitAllowDecimal).FirstOrDefault(),
                               QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == d.QuaternaryUnitId).Select(cc => cc.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                               PriceAddedFor = c.PriceAddedFor,
                           }).ToList();

                if (Item.ItemDetailsId != 0)
                {
                    det = det.Where(a => a.ItemDetailsId == Item.ItemDetailsId).ToList();
                }

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        ItemDetails = det,
                        BusinessSetting = new
                        {
                            CountryId = BusinessSetting.CountryId,
                        }
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

    }
}
