using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web.Http;
using Vonage.Messages.Webhooks;

namespace EquiBillBook.Controllers.WebApi.Common
{
    public class GstController : ApiController
    {
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here

        #region GSTR 1

        public ClsGstResult B2B(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft" && b.SalesType != "Debit Note" &&
       (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
       || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
       || b.GstTreatment == "Tax Deductor")
       //&& b.TotalTaxAmount > 0
       && oConnectionContext.DbClsSalesDetails
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                && oConnectionContext.DbClsSalesAdditionalCharges
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                       select new ClsGstSale
                       {
                           InvoiceType = (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply")
                           ? "Regular B2B" : ((b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" ||
                           b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 1 && b.PlaceOfSupplyId != BranchStateId) ? "SEZ supplies with payment" :
                           ((b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" ||
                           b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 1 && b.PlaceOfSupplyId == BranchStateId) ? "Intra-State supplies attracting IGST\r\n" :
                           ((b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" ||
                           b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 2) ? "SEZ supplies without payment"
                           : "Deemed Exp",
                           IsReverseCharge = b.IsReverseCharge,
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                          

                       }).ToList();

            foreach (var item in det)
            {
                var sd = (from c in oConnectionContext.DbClsSalesDetails
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.SalesId == item.SalesId
                                                           && z.SalesDetailsId == c.SalesDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

               var ad = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                 where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                 && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                 select new ClsGstSaleDetail
                 {
                     TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                     AmountExcTax = c.AmountExcTax,
                     TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                 select new ClsGstTaxType
                                 {
                                     TaxTypeId = x.TaxTypeId,
                                     TaxType = x.TaxType,
                                     TaxAmount = (from y in oConnectionContext.DbClsTax
                                                  join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                  on y.TaxId equals z.TaxId
                                                  where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.SalesId == item.SalesId
                                                  && z.SalesAdditionalChargesId == c.SalesAdditionalChargesId
                                                  select z.TaxAmount
                                                 ).DefaultIfEmpty().Sum()
                                 }).ToList()
                 }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var groupedResult = det.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                IsReverseCharge = a.IsReverseCharge,
                CustomerName = a.CustomerName,
                StateCode = a.StateCode,
                SalesId = a.SalesId,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                GrandTotal = a.GrandTotal,
                PlaceOfSupply = a.PlaceOfSupply,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                SalesDetails = a.SalesDetails.GroupBy(s => new { s.TaxPercent })
                    .Select(g => new ClsGstSaleDetail
                    {
                        TaxPercent = g.Key.TaxPercent,
                        AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                        TaxTypes = g.SelectMany(s => s.TaxTypes)
                                    .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                                    .Select(tg => new ClsGstTaxType
                                    {
                                        TaxTypeId = tg.Key.TaxTypeId,
                                        TaxType = tg.Key.TaxType,
                                        TaxAmount = tg.Sum(t => t.TaxAmount)
                                    }).ToList()
                    }).ToList()
            }).ToList();

            var finalResult = new ClsGstResult
            {
                Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult B2CLarge(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft" && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer") && b.GrandTotal > 100000 &&
       b.PlaceOfSupplyId != BranchStateId
       //&& b.TotalTaxAmount > 0
       && oConnectionContext.DbClsSalesDetails
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                && oConnectionContext.DbClsSalesAdditionalCharges
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                       select new ClsGstSale
                       {
                           //CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           //BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),                           
                       }).ToList();

            foreach (var item in det)
            {
                var sd = (from c in oConnectionContext.DbClsSalesDetails
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesDetailsId == c.SalesDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesAdditionalChargesId == c.SalesAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var groupedResult = det.Select(a => new ClsGstSale
            {
                //CustomerName = a.CustomerName,
                StateCode = a.StateCode,
                SalesId = a.SalesId,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                GrandTotal = a.GrandTotal,
                PlaceOfSupply = a.PlaceOfSupply,
                //BusinessRegistrationNo = a.BusinessRegistrationNo,
                SalesDetails = a.SalesDetails.GroupBy(s => new { s.TaxPercent })
                    .Select(g => new ClsGstSaleDetail
                    {
                        TaxPercent = g.Key.TaxPercent,
                        AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                        TaxTypes = g.SelectMany(s => s.TaxTypes)
                                    .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                                    .Select(tg => new ClsGstTaxType
                                    {
                                        TaxTypeId = tg.Key.TaxTypeId,
                                        TaxType = tg.Key.TaxType,
                                        TaxAmount = tg.Sum(t => t.TaxAmount)
                                    }).ToList()
                    }).ToList()
            }).ToList();

            var finalResult = new ClsGstResult
            {
                Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstSale B2CSmall(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from c in oConnectionContext.DbClsSalesDetails
                       join b in oConnectionContext.DbClsSales
                       on c.SalesId equals b.SalesId
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft"
       && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer") && b.GrandTotal <= 100000
        //&& b.TotalTaxAmount > 0
        && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                       select new ClsGstSaleDetail
                       {
                           SupplyType = b.PlaceOfSupplyId == BranchStateId ? "INTRA" : "INTER",
                           ParentGrandTotal = b.GrandTotal,
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                           && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                           TaxId = c.TaxId,
                           TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                           TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                       select new ClsGstTaxType
                                       {
                                           TaxTypeId = x.TaxTypeId,
                                           TaxType = x.TaxType,
                                           TaxAmount = (from y in oConnectionContext.DbClsTax
                                                        join z in oConnectionContext.DbClsSalesTaxJournal
                                                        on y.TaxId equals z.TaxId
                                                        where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.SalesId == b.SalesId
                                                        select z.TaxAmount
                                                       ).DefaultIfEmpty().Sum()
                                       }).ToList()
                       }).ToList();

            var detAdditional = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                                 join b in oConnectionContext.DbClsSales
                                 on c.SalesId equals b.SalesId
                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                 && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                 l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                 && b.Status != "Draft"
                 && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer") && b.GrandTotal <= 100000
                  //&& b.TotalTaxAmount > 0
                  && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                 select new ClsGstSaleDetail
                                 {
                                     SupplyType = b.PlaceOfSupplyId == BranchStateId ? "INTRA" : "INTER",
                                     ParentGrandTotal = b.GrandTotal,
                                     StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                     GrandTotal = b.GrandTotal,
                                     PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                     AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                                     && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                                     TaxId = c.TaxId,
                                     TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                     TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                 select new ClsGstTaxType
                                                 {
                                                     TaxTypeId = x.TaxTypeId,
                                                     TaxType = x.TaxType,
                                                     TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                  join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                                  on y.TaxId equals z.TaxId
                                                                  where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.SalesId == b.SalesId
                                                                  select z.TaxAmount
                                                                 ).DefaultIfEmpty().Sum()
                                                 }).ToList()
                                 }).ToList();

            det.AddRange(detAdditional);

            var groupedResult = det
            .GroupBy(s => new { s.StateCode, s.TaxPercent })
            .Select(g => new ClsGstSaleDetail
            {
                StateCode = g.Key.StateCode,
                TaxPercent = g.Key.TaxPercent,
                PlaceOfSupply = g.Select(b => b.PlaceOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsGstTaxType
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var finalResult = new ClsGstSale
            {
                SalesDetails = det,
                GroupedSalesDetails = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult Exports(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft" && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)"
                       select new ClsGstSale
                       {
                           InvoiceType = b.PayTaxForExport == 1 ? "WPAY" : "WOPAY",
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           GrandTotal = b.GrandTotal,
                           PortCode = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.PortCode).FirstOrDefault(),
                           ShippingBillNo = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.ShippingBillNo).FirstOrDefault(),
                           ShippingBillDate = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.ShippingBillDate).FirstOrDefault(),
                           //GrandTotal = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.GrandTotal).FirstOrDefault(),                           
                       }).ToList();

            foreach (var item in det)
            {
                var sd = (from c in oConnectionContext.DbClsSalesDetails
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesDetailsId == c.SalesDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesAdditionalChargesId == c.SalesAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var groupedResult = det.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesId = a.SalesId,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                PortCode = a.PortCode,
                ShippingBillNo = a.ShippingBillNo,
                ShippingBillDate = a.ShippingBillDate,
                GrandTotal = a.GrandTotal,
                SalesDetails = a.SalesDetails.GroupBy(s => new { s.TaxPercent })
                    .Select(g => new ClsGstSaleDetail
                    {
                        TaxPercent = g.Key.TaxPercent,
                        AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                        TaxTypes = g.SelectMany(s => s.TaxTypes)
                                    .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                                    .Select(tg => new ClsGstTaxType
                                    {
                                        TaxTypeId = tg.Key.TaxTypeId,
                                        TaxType = tg.Key.TaxType,
                                        TaxAmount = tg.Sum(t => t.TaxAmount)
                                    }).ToList()
                    }).ToList()
            }).ToList();

            var finalResult = new ClsGstResult
            {
                Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult CreditDebitRegistered(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var DebitNotes = (from b in oConnectionContext.DbClsSales
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
              && b.Status != "Draft" && b.SalesType == "Debit Note"
              && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
       || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
       || b.GstTreatment == "Tax Deductor")
               //&& b.TotalTaxAmount > 0
               && oConnectionContext.DbClsSalesDetails
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                && oConnectionContext.DbClsSalesAdditionalCharges
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                              select new ClsGstSale
                              {
                                  IsReverseCharge = b.IsReverseCharge,
                                  InvoiceType = (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply")
                           ? "Regular B2B" : ((b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" ||
                           b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 1 && b.PlaceOfSupplyId != BranchStateId) ? "SEZ supplies with payment" :
                           ((b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" ||
                           b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 1 && b.PlaceOfSupplyId == BranchStateId) ? "Intra-State supplies attracting IGST\r\n" :
                           ((b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" ||
                           b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 2) ? "SEZ supplies without payment"
                           : "Deemed Exp",
                                  BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                  CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                                  InvoiceNo = b.InvoiceNo,
                                  SalesDate = b.SalesDate,
                                  ParentInvoiceNo = oConnectionContext.DbClsSales.Where(d => d.SalesId == b.ParentId).Select(d => d.InvoiceNo).FirstOrDefault(),
                                  ParentSalesDate = oConnectionContext.DbClsSales.Where(d => d.SalesId == b.ParentId).Select(d => d.SalesDate).FirstOrDefault(),
                                  SalesType = "Debit Note",
                                  GrandTotal = b.GrandTotal,
                                  Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                                  StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                  SalesId = b.SalesId,
                                  PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                  AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                                  && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                                  PaymentType = "debit",
                                  
                              }).ToList();

            foreach (var item in DebitNotes)
            {
                var sd = (from c in oConnectionContext.DbClsSalesDetails
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesDetailsId == c.SalesDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();
                var ad = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesAdditionalChargesId == c.SalesAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var CreditNotes = (from b in oConnectionContext.DbClsSalesReturn
                               join f in oConnectionContext.DbClsSales
                               on b.SalesId equals f.SalesId
                               where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
               && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
               l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.Date) <= obj.ToDate
               && b.Status != "Draft"
               && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
       || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
       || b.GstTreatment == "Tax Deductor")
                //&& b.TotalTaxAmount > 0
                && f.IsDeleted == false && f.IsCancelled == false
                && oConnectionContext.DbClsSalesReturnDetails
                .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                && oConnectionContext.DbClsSalesReturnAdditionalCharges
                .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                               select new ClsGstSale
                               {
                                   IsReverseCharge = b.IsReverseCharge,
                                   InvoiceType = (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply")
                           ? "Regular B2B" : ((b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" ||
                           b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 1 && b.PlaceOfSupplyId != BranchStateId) ? "SEZ supplies with payment" :
                           ((b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" ||
                           b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 1 && b.PlaceOfSupplyId == BranchStateId) ? "Intra-State supplies attracting IGST\r\n" :
                           ((b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" ||
                           b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 2) ? "SEZ supplies without payment"
                           : "Deemed Exp",
                                   BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                                   CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                                   InvoiceNo = b.InvoiceNo,
                                   SalesDate = b.Date,
                                   ParentInvoiceNo = oConnectionContext.DbClsSales.Where(d => d.SalesId == b.SalesId).Select(d => d.InvoiceNo).FirstOrDefault(),
                                   ParentSalesDate = oConnectionContext.DbClsSales.Where(d => d.SalesId == b.SalesId).Select(d => d.SalesDate).FirstOrDefault(),
                                   SalesType = "Credit Note",
                                   GrandTotal = b.GrandTotal,
                                   Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                                   StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                   SalesId = b.SalesReturnId,
                                   PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                   AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(c => c.SalesReturnId == b.SalesReturnId && c.TaxAmount > 0
                                   && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                                   PaymentType = "credit",                                    
                               }).ToList();

            foreach (var item in CreditNotes)
            {
                var sd = (from c in oConnectionContext.DbClsSalesReturnDetails
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnDetailsId == c.SalesReturnDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesReturnAdditionalCharges
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnAdditionalChargesId == c.SalesReturnAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det = DebitNotes.Concat(CreditNotes).ToList();

            var groupedResult = det.Select(a => new ClsGstSale
            {
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                CustomerName = a.CustomerName,
                InvoiceNo = a.InvoiceNo,
                SalesDate = a.SalesDate,
                SalesType = a.SalesType,
                PlaceOfSupply = a.PlaceOfSupply,
                IsReverseCharge = a.IsReverseCharge,
                InvoiceType = a.InvoiceType,
                GrandTotal = a.GrandTotal,
                SalesDetails = a.SalesDetails.GroupBy(s => new { s.TaxPercent })
                   .Select(g => new ClsGstSaleDetail
                   {
                       TaxPercent = g.Key.TaxPercent,
                       AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                       TaxTypes = g.SelectMany(s => s.TaxTypes)
                                   .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                                   .Select(tg => new ClsGstTaxType
                                   {
                                       TaxTypeId = tg.Key.TaxTypeId,
                                       TaxType = tg.Key.TaxType,
                                       TaxAmount = tg.Sum(t => t.TaxAmount)
                                   }).ToList()
                   }).ToList()
            }).ToList();

            var finalResult = new ClsGstResult
            {
                Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult CreditDebitUnRegistered(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var DebitNotes = (from b in oConnectionContext.DbClsSales
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
              && b.Status != "Draft" && b.SalesType == "Debit Note"
              && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer" || b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)")
                //&& b.TotalTaxAmount > 0
                && oConnectionContext.DbClsSalesDetails
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                && oConnectionContext.DbClsSalesAdditionalCharges
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                              select new ClsGstSale
                              {
                                  InvoiceType = (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
                           ? "B2CL" : (b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && b.PayTaxForExport == 1) ? "EXPWP" : "EXPWOP",
                                  InvoiceNo = b.InvoiceNo,
                                  SalesDate = b.SalesDate,
                                  ParentInvoiceNo = oConnectionContext.DbClsSales.Where(d => d.SalesId == b.ParentId).Select(d => d.InvoiceNo).FirstOrDefault(),
                                  ParentSalesDate = oConnectionContext.DbClsSales.Where(d => d.SalesId == b.ParentId).Select(d => d.SalesDate).FirstOrDefault(),
                                  ParentGrandTotal = oConnectionContext.DbClsSales.Where(d => d.SalesId == b.ParentId).Select(d => d.GrandTotal).FirstOrDefault(),
                                  SalesType = "Debit Note",
                                  GrandTotal = b.GrandTotal,
                                  Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                                  StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                  SalesId = b.SalesId,
                                  PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                  AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                                  && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                                  PaymentType = "debit",
                                  
                              }).ToList();

            foreach (var item in DebitNotes)
            {
                var sd = (from c in oConnectionContext.DbClsSalesDetails
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesDetailsId == c.SalesDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesAdditionalChargesId == c.SalesAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var CreditNotes = (from b in oConnectionContext.DbClsSalesReturn
                               join f in oConnectionContext.DbClsSales
                               on b.SalesId equals f.SalesId
                               where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
               && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
               l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.Date) <= obj.ToDate
               && b.Status != "Draft"
               && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer" || b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)")
               && f.IsDeleted == false && f.IsCancelled == false
                && oConnectionContext.DbClsSalesReturnDetails
                .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                 && oConnectionContext.DbClsSalesReturnAdditionalCharges
                .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true &&
                          c.TaxId != ExemptedId && c.TaxId != NonGstId)
                               select new ClsGstSale
                               {
                                   InvoiceType = "B2CL",
                                   InvoiceNo = b.InvoiceNo,
                                   SalesDate = b.Date,
                                   ParentInvoiceNo = oConnectionContext.DbClsSales.Where(d => d.SalesId == b.SalesId).Select(d => d.InvoiceNo).FirstOrDefault(),
                                   ParentSalesDate = oConnectionContext.DbClsSales.Where(d => d.SalesId == b.SalesId).Select(d => d.SalesDate).FirstOrDefault(),
                                   ParentGrandTotal = oConnectionContext.DbClsSales.Where(d => d.SalesId == f.ParentId).Select(d => d.GrandTotal).FirstOrDefault(),
                                   SalesType = "Credit Note",
                                   GrandTotal = b.GrandTotal,
                                   Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                                   StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                   SalesId = b.SalesReturnId,
                                   PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                                   AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(c => c.SalesReturnId == b.SalesReturnId && c.TaxAmount > 0
                                   && c.IsDeleted == false && c.IsActive == true).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                                   PaymentType = "credit",                                   
                               }).ToList();

            foreach (var item in CreditNotes)
            {
                var sd = (from c in oConnectionContext.DbClsSalesReturnDetails
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnDetailsId == c.SalesReturnDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ac = (from c in oConnectionContext.DbClsSalesReturnAdditionalCharges
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnAdditionalChargesId == c.SalesReturnAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();


                item.SalesDetails = sd.Concat(ac).ToList();
            }

            var det = DebitNotes.Concat(CreditNotes).ToList();

            var groupedResult = det.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                InvoiceNo = a.InvoiceNo,
                SalesDate = a.SalesDate,
                SalesType = a.SalesType,
                PlaceOfSupply = a.PlaceOfSupply,
                GrandTotal = a.GrandTotal,
                SalesDetails = a.SalesDetails.GroupBy(s => new { s.TaxPercent })
                   .Select(g => new ClsGstSaleDetail
                   {
                       TaxPercent = g.Key.TaxPercent,
                       AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                       TaxTypes = g.SelectMany(s => s.TaxTypes)
                                   .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                                   .Select(tg => new ClsGstTaxType
                                   {
                                       TaxTypeId = tg.Key.TaxTypeId,
                                       TaxType = tg.Key.TaxType,
                                       TaxAmount = tg.Sum(t => t.TaxAmount)
                                   }).ToList()
                   }).ToList()
            }).ToList();

            var finalResult = new ClsGstResult
            {
                Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstSale AdvancesReceived(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsCustomerPayment
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
       && b.Type == "Customer Payment" && b.ParentId == 0
       //&& b.TaxAmount > 0 
       && b.SalesReturnId == 0
       && (b.TaxId != ExemptedId || b.TaxId != NonGstId)
                       select new ClsGstSaleDetail
                       {
                           SupplyType = (b.PlaceOfSupplyId == BranchStateId) ? "INTRA" : "INTER",
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           AmountRemaining = b.AmountRemaining,
                           //AmountExcTax = (100 * (b.AmountRemaining) / (100 +
                           //               oConnectionContext.DbClsTax.Where(a => a.TaxId == b.TaxId).Select(a => a.TaxPercent).FirstOrDefault())),
                           AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                           TaxId = b.TaxId,
                           TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                           TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                       select new ClsGstTaxType
                                       {
                                           TaxTypeId = x.TaxTypeId,
                                           TaxType = x.TaxType,
                                           // TaxAmount = (from y in oConnectionContext.DbClsTax
                                           //              join z in oConnectionContext.DbClsCustomerPaymentTaxJournal
                                           //              on y.TaxId equals z.TaxId
                                           //              where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId &&
                                           //              z.CustomerPaymentId == b.CustomerPaymentId
                                           //              select ((y.TaxPercent / 100) * (100 * (b.AmountRemaining) / (100 +
                                           //oConnectionContext.DbClsTax.Where(a => a.TaxId == b.TaxId).Select(a => a.TaxPercent).FirstOrDefault())))
                                           TaxAmount = (from y in oConnectionContext.DbClsTax
                                                        join z in oConnectionContext.DbClsCustomerPaymentTaxJournal
                                                        on y.TaxId equals z.TaxId
                                                        where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                        && z.CustomerPaymentId == b.CustomerPaymentId
                                                        select (y.TaxPercent / 100) * (b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum())
                                                       ).DefaultIfEmpty().Sum()
                                       }).ToList()
                       }).ToList();

            var groupedResult = det
            .GroupBy(s => new { s.SupplyType, s.StateCode, s.TaxPercent })
            .Select(g => new ClsGstSaleDetail
            {
                SupplyType = g.Key.SupplyType,
                StateCode = g.Key.StateCode,
                TaxPercent = g.Key.TaxPercent,
                PlaceOfSupply = g.Select(b => b.PlaceOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                AmountRemaining = g.Select(b => b.AmountRemaining).Sum(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsGstTaxType
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var finalResult = new ClsGstSale
            {
                SalesDetails = det,
                GroupedSalesDetails = groupedResult
            };

            return finalResult;
        }

        public ClsGstSale AdjustmentOfAdvances(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsCustomerPayment
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
       && b.Type == "Sales Payment" && b.ParentId > 0
        //&& b.TaxAmount > 0
        && (b.TaxId != ExemptedId || b.TaxId != NonGstId)
       && oConnectionContext.DbClsCustomerPayment.Any(u => u.CustomerPaymentId == b.ParentId && DbFunctions.TruncateTime(u.PaymentDate) < obj.FromDate)
                       select new ClsGstSaleDetail
                       {
                           SupplyType = (b.PlaceOfSupplyId == BranchStateId) ? "INTRA" : "INTER",
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           Amount = b.Amount,
                           AmountExcTax = b.AmountExcTax,
                           TaxId = b.TaxId,
                           TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                           TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                       select new ClsGstTaxType
                                       {
                                           TaxTypeId = x.TaxTypeId,
                                           TaxType = x.TaxType,
                                           TaxAmount = (from y in oConnectionContext.DbClsTax
                                                        join z in oConnectionContext.DbClsCustomerPaymentTaxJournal
                                                        on y.TaxId equals z.TaxId
                                                        where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId &&
                                                        z.CustomerPaymentId == b.CustomerPaymentId
                                                        select z.TaxAmount
                                                       ).DefaultIfEmpty().Sum()
                                       }).ToList()
                       }).ToList();

            var groupedResult = det
            .GroupBy(s => new { s.SupplyType, s.StateCode, s.TaxPercent })
            .Select(g => new ClsGstSaleDetail
            {
                SupplyType = g.Key.SupplyType,
                StateCode = g.Key.StateCode,
                TaxPercent = g.Key.TaxPercent,
                PlaceOfSupply = g.Select(b => b.PlaceOfSupply).FirstOrDefault(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                Amount = g.Select(b => b.Amount).Sum(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsGstTaxType
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            })
            .ToList();

            var finalResult = new ClsGstSale
            {
                SalesDetails = det,
                GroupedSalesDetails = groupedResult
            };

            return finalResult;
        }

        public ClsGstSale HsnWiseSummary(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
          && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            List<ClsGstSaleDetail> det1 = (from a in oConnectionContext.DbClsSalesDetails
                                           join b in oConnectionContext.DbClsSales
                                           on a.SalesId equals b.SalesId
                                           join e in oConnectionContext.DbClsItem
                                           on a.ItemId equals e.ItemId
                                           where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && a.IsDeleted == false && a.IsActive == true
                           && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                           l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                           && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                               DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                           && b.Status != "Draft"
                           //&& b.TotalTaxAmount > 0 
                           && e.ItemCodeId != 0 && e.ItemType == "Product" && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer")
                            && (a.TaxId != NilratedGstId && a.TaxId != NilratedIgstId && a.TaxId != ExemptedId && a.TaxId != NonGstId)
                                           select new ClsGstSaleDetail
                                           {
                                               CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                                               StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                               SalesId = b.SalesId,
                                               SalesDate = b.SalesDate,
                                               InvoiceNo = b.InvoiceNo,
                                               Code = oConnectionContext.DbClsItemCode.Where(d => d.ItemCodeId == e.ItemCodeId).Select(d => d.Code).FirstOrDefault(),
                                               Description = oConnectionContext.DbClsItemCode.Where(d => d.ItemCodeId == e.ItemCodeId).Select(d => d.Description).FirstOrDefault(),
                                               ItemId = a.ItemId,
                                               //ItemName = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.ItemName).FirstOrDefault(),
                                               //Unit = a.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(f => f.UnitId == e.UnitId).Select(f => f.UnitShortName).FirstOrDefault()
                                               //         : a.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(f => f.SecondaryUnitId == e.SecondaryUnitId).Select(f => f.SecondaryUnitShortName).FirstOrDefault()
                                               //         : a.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(f => f.TertiaryUnitId == e.TertiaryUnitId).Select(f => f.TertiaryUnitShortName).FirstOrDefault()
                                               //         : oConnectionContext.DbClsQuaternaryUnit.Where(f => f.QuaternaryUnitId == e.QuaternaryUnitId).Select(f => f.QuaternaryUnitShortName).FirstOrDefault(),
                                               Quantity = a.QuantitySold,
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == a.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = a.AmountExcTax,
                                               GrandTotal = b.GrandTotal,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new ClsGstTaxType
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsSalesTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.SalesId == b.SalesId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).ToList();

            List<ClsGstSaleDetail> det2 = (from a in oConnectionContext.DbClsSalesDetails
                                           join b in oConnectionContext.DbClsSales
                                           on a.SalesId equals b.SalesId
                                           join e in oConnectionContext.DbClsItem
                                           on a.ItemId equals e.ItemId
                                           where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && a.IsDeleted == false && a.IsActive == true
                           && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                           l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                           && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                               DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                           && b.Status != "Draft"
                           //&& b.TotalTaxAmount > 0 
                           && e.ItemCodeId != 0 && e.ItemType == "Product" && (b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer")
                            && (a.TaxId != ExemptedId && a.TaxId != NonGstId)
                                           select new ClsGstSaleDetail
                                           {
                                               CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                                               StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                                               SalesId = b.SalesId,
                                               SalesDate = b.SalesDate,
                                               InvoiceNo = b.InvoiceNo,
                                               Code = oConnectionContext.DbClsItemCode.Where(d => d.ItemCodeId == e.ItemCodeId).Select(d => d.Code).FirstOrDefault(),
                                               Description = oConnectionContext.DbClsItemCode.Where(d => d.ItemCodeId == e.ItemCodeId).Select(d => d.Description).FirstOrDefault(),
                                               ItemId = a.ItemId,
                                               //ItemName = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.ItemName).FirstOrDefault(),
                                               //Unit = a.UnitAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(f => f.UnitId == e.UnitId).Select(f => f.UnitShortName).FirstOrDefault()
                                               //         : a.UnitAddedFor == 2 ? oConnectionContext.DbClsSecondaryUnit.Where(f => f.SecondaryUnitId == e.SecondaryUnitId).Select(f => f.SecondaryUnitShortName).FirstOrDefault()
                                               //         : a.UnitAddedFor == 3 ? oConnectionContext.DbClsTertiaryUnit.Where(f => f.TertiaryUnitId == e.TertiaryUnitId).Select(f => f.TertiaryUnitShortName).FirstOrDefault()
                                               //         : oConnectionContext.DbClsQuaternaryUnit.Where(f => f.QuaternaryUnitId == e.QuaternaryUnitId).Select(f => f.QuaternaryUnitShortName).FirstOrDefault(),
                                               Quantity = a.QuantitySold,
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == a.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = a.AmountExcTax,
                                               GrandTotal = b.GrandTotal,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new ClsGstTaxType
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsSalesTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.SalesId == b.SalesId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).ToList();

            var det = det1.Union(det2).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                PrimaryShortUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitShortName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                SecondaryShortUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitShortName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                TertiaryShortUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitShortName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
                QuaternaryShortUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitShortName).FirstOrDefault(),
            }).ToList();

            foreach (var item in det)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    PrimaryShortUnit = a.PrimaryShortUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    SecondaryShortUnit = a.SecondaryShortUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    TertiaryShortUnit = a.TertiaryShortUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
                    QuaternaryShortUnit = a.QuaternaryShortUnit,
                }).FirstOrDefault();

                decimal TotalCurrentStock = item.Quantity;

                if (obj.PriceAddedFor == 0) { obj.PriceAddedFor = 1; }

                //TotalCurrentStock Stock
                if (conversionRates.UToSValue == 0 && conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    //TotalCurrentStock = TotalCurrentStock;
                }
                else if (conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.UToSValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        //TotalCurrentStock = TotalCurrentStock;
                    }
                }
                else if (conversionRates.TToQValue == 0)
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.UToSValue / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        //TotalCurrentStock = TotalCurrentStock;
                    }
                }
                else
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 4)
                    {
                        //TotalCurrentStock= TotalCurrentStock;
                    }
                }

                item.Quantity = TotalCurrentStock;
                item.UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit;
                item.UnitShortName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryShortUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryShortUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryShortUnit : conversionRates.QuaternaryShortUnit;
            }

            var groupedResult = det
            .GroupBy(s => new { s.UnitName, s.Code, s.TaxPercent })
            .Select(g => new ClsGstSaleDetail
            {
                Code = g.Key.Code,
                UnitName = g.Key.UnitName,
                UnitShortName = g.Select(b => b.UnitShortName).FirstOrDefault(),
                TaxPercent = g.Key.TaxPercent,
                Description = g.Select(b => b.Description).FirstOrDefault(),
                //ItemName = g.Select(b => b.ItemName).FirstOrDefault(),
                Quantity = g.Select(b => b.Quantity).Sum(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                GrandTotal = g.Select(b => b.GrandTotal).Sum(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new ClsGstTaxType
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            }).ToList();

            var finalResult = new ClsGstSale
            {
                SalesDetails = det,
                GroupedSalesDetails = groupedResult
            };

            return finalResult;
        }

        public ClsGstSale NilRated(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            List<ClsGstSaleDetail> SalesDetails = new List<ClsGstSaleDetail>();

            var NilRated = (from a in oConnectionContext.DbClsSalesDetails
                            join b in oConnectionContext.DbClsSales
                            on a.SalesId equals b.SalesId
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && a.IsDeleted == false && a.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
            && b.Status != "Draft"
            && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer")
            && (a.TaxId == NilratedGstId || a.TaxId == NilratedIgstId)
                            select new ClsGstSaleDetail
                            {
                                GstTreatment = b.GstTreatment,
                                PlaceOfSupplyId = b.PlaceOfSupplyId,
                                Quantity = a.Quantity,
                                SalesId = b.SalesId,
                                SalesDate = b.SalesDate,
                                InvoiceNo = b.InvoiceNo,
                                NilRatedAmount = a.AmountIncTax,
                                ExemptedAmount = (Decimal)(0),
                                NonGstAmount = (Decimal)(0),
                                PaymentType = "credit",
                            }).ToList();

            var Exempted = (from a in oConnectionContext.DbClsSalesDetails
                            join b in oConnectionContext.DbClsSales
                            on a.SalesId equals b.SalesId
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && a.IsDeleted == false && a.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
            && b.Status != "Draft"
            && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer")
            && a.TaxId == ExemptedId
                            select new ClsGstSaleDetail
                            {
                                GstTreatment = b.GstTreatment,
                                PlaceOfSupplyId = b.PlaceOfSupplyId,
                                Quantity = a.Quantity,
                                SalesId = b.SalesId,
                                SalesDate = b.SalesDate,
                                InvoiceNo = b.InvoiceNo,
                                NilRatedAmount = (Decimal)(0),
                                ExemptedAmount = a.AmountIncTax,
                                NonGstAmount = (Decimal)(0),
                                PaymentType = "credit",
                            }).ToList();

            var NonGst = (from a in oConnectionContext.DbClsSalesDetails
                          join b in oConnectionContext.DbClsSales
                          on a.SalesId equals b.SalesId
                          where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true && a.IsDeleted == false && a.IsActive == true
          && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
          l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
          && b.Status != "Draft"
          && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer")
          && a.TaxId == NonGstId
                          select new ClsGstSaleDetail
                          {
                              GstTreatment = b.GstTreatment,
                              PlaceOfSupplyId = b.PlaceOfSupplyId,
                              Quantity = a.Quantity,
                              SalesId = b.SalesId,
                              SalesDate = b.SalesDate,
                              InvoiceNo = b.InvoiceNo,
                              NilRatedAmount = (Decimal)(0),
                              ExemptedAmount = (Decimal)(0),
                              NonGstAmount = a.AmountIncTax,
                              PaymentType = "credit",
                          }).ToList();

            SalesDetails.Add(new ClsGstSaleDetail
            {
                Description = "Inter-State supplies to registered persons",
                NilRatedAmount = NilRated.Where(b => b.PlaceOfSupplyId != BranchStateId &&
                (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
       || b.GstTreatment == "Deemed Export"
       || b.GstTreatment == "Tax Deductor")).Select(b => b.NilRatedAmount).DefaultIfEmpty().Sum(),
                ExemptedAmount = Exempted.Where(b => b.PlaceOfSupplyId != BranchStateId &&
                (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
       || b.GstTreatment == "Deemed Export"
       || b.GstTreatment == "Tax Deductor")).Select(b => b.ExemptedAmount).DefaultIfEmpty().Sum(),
                NonGstAmount = NonGst.Where(b => b.PlaceOfSupplyId != BranchStateId &&
                (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
       || b.GstTreatment == "Deemed Export"
       || b.GstTreatment == "Tax Deductor")).Select(b => b.NonGstAmount).DefaultIfEmpty().Sum()
            });

            SalesDetails.Add(new ClsGstSaleDetail
            {
                Description = "Intra-State supplies to registered persons",
                NilRatedAmount = NilRated.Where(b => b.PlaceOfSupplyId == BranchStateId &&
                    (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
           || b.GstTreatment == "Deemed Export"
           || b.GstTreatment == "Tax Deductor")).Select(b => b.NilRatedAmount).DefaultIfEmpty().Sum(),
                ExemptedAmount = Exempted.Where(b => b.PlaceOfSupplyId == BranchStateId &&
                (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
       || b.GstTreatment == "Deemed Export"
       || b.GstTreatment == "Tax Deductor")).Select(b => b.ExemptedAmount).DefaultIfEmpty().Sum(),
                NonGstAmount = NonGst.Where(b => b.PlaceOfSupplyId == BranchStateId &&
                (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
       || b.GstTreatment == "Deemed Export"
       || b.GstTreatment == "Tax Deductor")).Select(b => b.NonGstAmount).DefaultIfEmpty().Sum()
            });

            SalesDetails.Add(new ClsGstSaleDetail
            {
                Description = "Inter-State supplies to unregistered persons",
                NilRatedAmount = NilRated.Where(b => b.PlaceOfSupplyId != BranchStateId &&
                    (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")).Select(b => b.NilRatedAmount).DefaultIfEmpty().Sum(),
                ExemptedAmount = Exempted.Where(b => b.PlaceOfSupplyId != BranchStateId &&
                (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")).Select(b => b.ExemptedAmount).DefaultIfEmpty().Sum(),
                NonGstAmount = NonGst.Where(b => b.PlaceOfSupplyId != BranchStateId &&
                (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")).Select(b => b.NonGstAmount).DefaultIfEmpty().Sum()
            });

            SalesDetails.Add(new ClsGstSaleDetail
            {
                Description = "Intra-State supplies to unregistered persons",
                NilRatedAmount = NilRated.Where(b => b.PlaceOfSupplyId == BranchStateId &&
                    (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")).Select(b => b.NilRatedAmount).DefaultIfEmpty().Sum(),
                ExemptedAmount = Exempted.Where(b => b.PlaceOfSupplyId == BranchStateId &&
                (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")).Select(b => b.ExemptedAmount).DefaultIfEmpty().Sum(),
                NonGstAmount = NonGst.Where(b => b.PlaceOfSupplyId == BranchStateId &&
                (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")).Select(b => b.NonGstAmount).DefaultIfEmpty().Sum()
            });

            var det = NilRated.Concat(Exempted).Concat(NonGst).ToList();

            var finalResult = new ClsGstSale
            {
                SalesDetails = SalesDetails,
                NilRated = NilRated,
                Exempted = Exempted,
                NonGst = NonGst
            };

            return finalResult;
        }

        public List<ClsTaxDocuments> SummaryOfDocuments(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            #region Invoices for outward supply
            var OutwardSupply = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
           a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
           {
               DocumentNumber = 1,
               PrefixId = a.PrefixId,
               NatureOfDocument = "Invoices for outward supply",
               From = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.SalesType != "Debit Note"
                       orderby b.SalesId ascending
                       select b.InvoiceNo).FirstOrDefault(),
               To = (from b in oConnectionContext.DbClsSales
                     where b.CompanyId == obj.CompanyId && b.IsDeleted == false
     && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
     l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
 && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
     DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
     && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.SalesType != "Debit Note"
                     orderby b.SalesId descending
                     select b.InvoiceNo).FirstOrDefault(),
               TotalNumber = (from b in oConnectionContext.DbClsSales
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
              && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.SalesType != "Debit Note"
                              select b.InvoiceNo).Count(),
               TotalCancelled = (from b in oConnectionContext.DbClsSales
                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                 && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                 l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                 && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.SalesType != "Debit Note" && b.IsCancelled == true
                                 select b.InvoiceNo).Count(),
           }).Distinct().ToList();
            #endregion

            #region Invoices for inward supply from unregistered person
            var InwardSupply = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
           a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
           {
               DocumentNumber = 2,
               PrefixId = a.PrefixId,
               NatureOfDocument = "Invoices for inward supply from unregistered person",
               From = (from b in oConnectionContext.DbClsPurchase
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
       && b.Status != "Draft" && b.PrefixId == a.PrefixId
       && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
                       orderby b.PurchaseId ascending
                       select b.ReferenceNo).FirstOrDefault(),
               To = (from b in oConnectionContext.DbClsPurchase
                     where b.CompanyId == obj.CompanyId && b.IsDeleted == false
     && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
     l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
 && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
     DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
     && b.Status != "Draft" && b.PrefixId == a.PrefixId
     && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
                     orderby b.PurchaseId descending
                     select b.ReferenceNo).FirstOrDefault(),
               TotalNumber = (from b in oConnectionContext.DbClsPurchase
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
              && b.Status != "Draft" && b.PrefixId == a.PrefixId
              && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
                              select b.ReferenceNo).Count(),
               TotalCancelled = (from b in oConnectionContext.DbClsPurchase
                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                 && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                 l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                 && b.Status != "Draft" && b.PrefixId == a.PrefixId
                 && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
                 && b.IsCancelled == true
                                 select b.ReferenceNo).Count(),
           }).Distinct().ToList();
            #endregion

            #region Debit Note
            var DebitNote = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
           a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
           {
               DocumentNumber = 4,
               PrefixId = a.PrefixId,
               NatureOfDocument = "Debit Note",
               From = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.SalesType == "Debit Note"
                       orderby b.SalesId ascending
                       select b.InvoiceNo).FirstOrDefault(),
               To = (from b in oConnectionContext.DbClsSales
                     where b.CompanyId == obj.CompanyId && b.IsDeleted == false
     && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
     l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
 && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
     DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
     && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.SalesType == "Debit Note"
                     orderby b.SalesId descending
                     select b.InvoiceNo).FirstOrDefault(),
               TotalNumber = (from b in oConnectionContext.DbClsSales
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
              && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.SalesType == "Debit Note"
                              select b.InvoiceNo).Count(),
               TotalCancelled = (from b in oConnectionContext.DbClsSales
                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                 && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                 l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                 && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.SalesType == "Debit Note" && b.IsCancelled == true
                                 select b.InvoiceNo).Count(),
           }).Distinct().ToList();
            #endregion

            #region Credit Note
            var CreditNote = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
           a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
           {
               DocumentNumber = 5,
               PrefixId = a.PrefixId,
               NatureOfDocument = "Credit Note",
               From = (from b in oConnectionContext.DbClsSalesReturn
                       join f in oConnectionContext.DbClsSales
                       on b.SalesId equals f.SalesId
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.Date) <= obj.ToDate
       && b.Status != "Draft" && f.IsDeleted == false && f.IsCancelled == false && b.PrefixId == a.PrefixId
                       orderby b.InvoiceNo ascending
                       select b.InvoiceNo).FirstOrDefault(),
               To = (from b in oConnectionContext.DbClsSalesReturn
                     join f in oConnectionContext.DbClsSales
                     on b.SalesId equals f.SalesId
                     where b.CompanyId == obj.CompanyId && b.IsDeleted == false
     && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
     l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
 && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
     DbFunctions.TruncateTime(b.Date) <= obj.ToDate
     && b.Status != "Draft" && f.IsDeleted == false && f.IsCancelled == false && b.PrefixId == a.PrefixId
                     orderby b.InvoiceNo descending
                     select b.InvoiceNo).FirstOrDefault(),
               TotalNumber = (from b in oConnectionContext.DbClsSalesReturn
                              join f in oConnectionContext.DbClsSales
                              on b.SalesId equals f.SalesId
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.Date) <= obj.ToDate
              && b.Status != "Draft" && f.IsDeleted == false && f.IsCancelled == false && b.PrefixId == a.PrefixId
              && b.IsCancelled == false
                              orderby b.InvoiceNo descending
                              select b.InvoiceNo).Count(),
               TotalCancelled = (from b in oConnectionContext.DbClsSalesReturn
                                 join f in oConnectionContext.DbClsSales
                                 on b.SalesId equals f.SalesId
                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                 && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                 l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                 && b.Status != "Draft" && f.IsDeleted == false && f.IsCancelled == false && b.PrefixId == a.PrefixId
                 && b.IsCancelled == true
                                 orderby b.InvoiceNo descending
                                 select b.InvoiceNo).Count(),
           }).Distinct().ToList();
            #endregion

            #region Receipt vouchers
            var ReceiptVouchers = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
           a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
           {
               DocumentNumber = 6,
               PrefixId = a.PrefixId,
               NatureOfDocument = "Receipt vouchers",
               From = (from b in oConnectionContext.DbClsCustomerPayment
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
       && b.Type == "Customer Payment" && b.ParentId == 0 && b.PrefixId == a.PrefixId
                       orderby b.CustomerPaymentId ascending
                       select b.ReferenceNo).FirstOrDefault(),
               To = (from b in oConnectionContext.DbClsCustomerPayment
                     where b.CompanyId == obj.CompanyId && b.IsDeleted == false
     && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
     l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
 && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
     DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
     && b.Type == "Customer Payment" && b.ParentId == 0 && b.PrefixId == a.PrefixId
                     orderby b.CustomerPaymentId descending
                     select b.ReferenceNo).FirstOrDefault(),
               TotalNumber = (from b in oConnectionContext.DbClsCustomerPayment
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
              && b.Type == "Customer Payment" && b.ParentId == 0 && b.PrefixId == a.PrefixId
                              select b.ReferenceNo).Count(),
               TotalCancelled = (from b in oConnectionContext.DbClsCustomerPayment
                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                 && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                 l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                 && b.Type == "Customer Payment" && b.ParentId == 0 && b.PrefixId == a.PrefixId && b.IsCancelled == true
                                 select b.ReferenceNo).Count(),
           }).Distinct().ToList();
            #endregion            

            #region Payment vouchers
            var PaymentVouchers = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
           a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
           {
               DocumentNumber = 7,
               PrefixId = a.PrefixId,
               NatureOfDocument = "Payment vouchers",
               From = (from b in oConnectionContext.DbClsSupplierPayment
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
       && b.Type == "Supplier Payment" && b.ParentId == 0 && b.PrefixId == a.PrefixId && b.IsReverseCharge == 1
                       orderby b.SupplierPaymentId ascending
                       select b.ReferenceNo).FirstOrDefault(),
               To = (from b in oConnectionContext.DbClsSupplierPayment
                     where b.CompanyId == obj.CompanyId && b.IsDeleted == false
     && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
     l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
 && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
     DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
     && b.Type == "Supplier Payment" && b.ParentId == 0 && b.PrefixId == a.PrefixId && b.IsReverseCharge == 1
                     orderby b.SupplierPaymentId descending
                     select b.ReferenceNo).FirstOrDefault(),
               TotalNumber = (from b in oConnectionContext.DbClsSupplierPayment
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
              && b.Type == "Supplier Payment" && b.ParentId == 0 && b.PrefixId == a.PrefixId && b.IsReverseCharge == 1
                              select b.ReferenceNo).Count(),
               TotalCancelled = (from b in oConnectionContext.DbClsSupplierPayment
                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                 && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                 l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                 && b.Type == "Supplier Payment" && b.ParentId == 0 && b.PrefixId == a.PrefixId && b.IsReverseCharge == 1 && b.IsCancelled == true
                                 select b.ReferenceNo).Count(),
           }).Distinct().ToList();
            #endregion

            #region Refund vouchers
            var RefundVouchers = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
           a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
           {
               DocumentNumber = 8,
               PrefixId = a.PrefixId,
               NatureOfDocument = "Refund vouchers",
               From = (from b in oConnectionContext.DbClsCustomerPayment
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
       && b.Type.ToLower() == "customer refund" && b.PrefixId == a.PrefixId
                       orderby b.CustomerPaymentId ascending
                       select b.ReferenceNo).FirstOrDefault(),
               To = (from b in oConnectionContext.DbClsCustomerPayment
                     where b.CompanyId == obj.CompanyId && b.IsDeleted == false
     && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
     l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
 && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
     DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
     && b.Type.ToLower() == "customer refund" && b.PrefixId == a.PrefixId
                     orderby b.CustomerPaymentId descending
                     select b.ReferenceNo).FirstOrDefault(),
               TotalNumber = (from b in oConnectionContext.DbClsCustomerPayment
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
              && b.Type.ToLower() == "customer refund" && b.PrefixId == a.PrefixId
                              select b.ReferenceNo).Count(),
               TotalCancelled = (from b in oConnectionContext.DbClsCustomerPayment
                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                 && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                 l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                 && b.Type.ToLower() == "customer refund" && b.PrefixId == a.PrefixId && b.IsCancelled == true
                                 select b.ReferenceNo).Count(),
           }).Distinct().ToList();
            #endregion

            #region Delivery Challan for job work
            var DeliveryChallanForJobWork = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
           a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
           {
               DocumentNumber = 9,
               PrefixId = a.PrefixId,
               NatureOfDocument = "Delivery Challan for job work",
               From = (from b in oConnectionContext.DbClsDeliveryChallan
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
       && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Job Work"
                       orderby b.DeliveryChallanId ascending
                       select b.InvoiceNo).FirstOrDefault(),
               To = (from b in oConnectionContext.DbClsDeliveryChallan
                     where b.CompanyId == obj.CompanyId && b.IsDeleted == false
     && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
     l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
 && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
     DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
     && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Job Work"
                     orderby b.DeliveryChallanId descending
                     select b.InvoiceNo).FirstOrDefault(),
               TotalNumber = (from b in oConnectionContext.DbClsDeliveryChallan
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
              && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Job Work"
                              select b.InvoiceNo).Count(),
               TotalCancelled = (from b in oConnectionContext.DbClsDeliveryChallan
                                 where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                 && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                 l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
                 && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Job Work" && b.IsCancelled == true
                                 select b.InvoiceNo).Count(),
           }).Distinct().ToList();
            #endregion

            #region Delivery Challan for supply on approval

            var DeliveryChallanForSupplyOnApproval = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
            a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
            {
                DocumentNumber = 10,
                PrefixId = a.PrefixId,
                NatureOfDocument = "Delivery Challan for supply on approval",
                From = (from b in oConnectionContext.DbClsDeliveryChallan
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false
             && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
             l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
             DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
             && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Supply on Approval"
                        orderby b.DeliveryChallanId ascending
                        select b.InvoiceNo).FirstOrDefault(),
                To = (from b in oConnectionContext.DbClsDeliveryChallan
                      where b.CompanyId == obj.CompanyId && b.IsDeleted == false
             && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
             l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
             DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
             && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Supply on Approval"
                      orderby b.DeliveryChallanId descending
                      select b.InvoiceNo).FirstOrDefault(),
                TotalNumber = (from b in oConnectionContext.DbClsDeliveryChallan
                               where b.CompanyId == obj.CompanyId && b.IsDeleted == false
               && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
               l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
               && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Supply on Approval"
                               select b.InvoiceNo).Count(),
                TotalCancelled = (from b in oConnectionContext.DbClsDeliveryChallan
                                  where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                  && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                  l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
              && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
                  && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Supply on Approval" && b.IsCancelled == true
                                  select b.InvoiceNo).Count(),
            }).Distinct().ToList();
            #endregion

            #region Delivery Challan in case of liquid gas

            var DeliveryChallanInCaseOfLiquidGas = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
            a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
            {
                DocumentNumber = 11,
                PrefixId = a.PrefixId,
                NatureOfDocument = "Delivery Challan in case of liquid gas",
                From = (from b in oConnectionContext.DbClsDeliveryChallan
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false
             && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
             l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
             DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
             && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Supply of Liquid Gas"
                        orderby b.DeliveryChallanId ascending
                        select b.InvoiceNo).FirstOrDefault(),
                To = (from b in oConnectionContext.DbClsDeliveryChallan
                      where b.CompanyId == obj.CompanyId && b.IsDeleted == false
             && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
             l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
             DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
             && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Supply of Liquid Gas"
                      orderby b.DeliveryChallanId descending
                      select b.InvoiceNo).FirstOrDefault(),
                TotalNumber = (from b in oConnectionContext.DbClsDeliveryChallan
                               where b.CompanyId == obj.CompanyId && b.IsDeleted == false
               && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
               l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
               && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Supply of Liquid Gas"
                               select b.InvoiceNo).Count(),
                TotalCancelled = (from b in oConnectionContext.DbClsDeliveryChallan
                                  where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                  && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                  l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
              && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
                  && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Supply of Liquid Gas" && b.IsCancelled == true
                                  select b.InvoiceNo).Count(),
            }).Distinct().ToList();
            #endregion

            #region Delivery Challan in case other than by way of supply (excluding at S no. 9 to 11)

            var DeliveryChallanInCaseOfOthers = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId &&
            a.TaxSettingId == obj.TaxSettingId && a.IsActive == true && a.IsDeleted == false).Select(a => new ClsTaxDocuments
            {
                DocumentNumber = 12,
                PrefixId = a.PrefixId,
                NatureOfDocument = "Delivery Challan in case other than by way of supply (excluding at S no. 9 to 11)",
                From = (from b in oConnectionContext.DbClsDeliveryChallan
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false
             && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
             l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
             DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
             && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Others"
                        orderby b.DeliveryChallanId ascending
                        select b.InvoiceNo).FirstOrDefault(),
                To = (from b in oConnectionContext.DbClsDeliveryChallan
                      where b.CompanyId == obj.CompanyId && b.IsDeleted == false
             && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
             l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
             DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
             && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Others"
                      orderby b.DeliveryChallanId descending
                      select b.InvoiceNo).FirstOrDefault(),
                TotalNumber = (from b in oConnectionContext.DbClsDeliveryChallan
                               where b.CompanyId == obj.CompanyId && b.IsDeleted == false
               && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
               l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
             && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
               && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Others"
                               select b.InvoiceNo).Count(),
                TotalCancelled = (from b in oConnectionContext.DbClsDeliveryChallan
                                  where b.CompanyId == obj.CompanyId && b.IsDeleted == false
                  && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                  l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
              && DbFunctions.TruncateTime(b.DeliveryChallanDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.DeliveryChallanDate) <= obj.ToDate
                  && b.Status != "Draft" && b.PrefixId == a.PrefixId && b.DeliveryChallanType == "Others" && b.IsCancelled == true
                                  select b.InvoiceNo).Count(),
            }).Distinct().ToList();
            #endregion

            var TaxDocuments = OutwardSupply.Union(InwardSupply).Union(DebitNote).Union(CreditNote).Union(ReceiptVouchers)
                .Union(RefundVouchers).Union(PaymentVouchers).Union(DeliveryChallanForJobWork).
                Union(DeliveryChallanForSupplyOnApproval).Union(DeliveryChallanInCaseOfLiquidGas).
                Union(DeliveryChallanInCaseOfOthers).ToList();

            return TaxDocuments;
        }

        #endregion

        #region Gstr 3B

        public ClsGstResult OutwardTaxable(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
           && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            //            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            //&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsSales
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer")
         //&& b.TotalTaxAmount > 0
         && oConnectionContext.DbClsSalesDetails
                 .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                           && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId))
                 && oConnectionContext.DbClsSalesAdditionalCharges
                 .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                           && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId))
                        select new ClsGstSale
                        {
                            InvoiceType = b.SalesType == "Sales Debit Note" ? "Sales Debit Note" : "Sales Invoice",
                            SalesDate = b.SalesDate,
                            InvoiceNo = b.InvoiceNo,
                            SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                            where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId)
                                            select new ClsGstSaleDetail
                                            {
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new ClsGstTaxType
                                                            {
                                                                TaxType = x.TaxType,
                                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                             join z in oConnectionContext.DbClsSalesTaxJournal
                                                                             on y.TaxId equals z.TaxId
                                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.SalesId == b.SalesId
                                                                             && z.SalesDetailsId == c.SalesDetailsId
                                                                             select z.TaxAmount
                                                                            ).DefaultIfEmpty().Sum()
                                                            }).ToList()
                                            }).ToList()

                        }).ToList();

            var det2 = (from b in oConnectionContext.DbClsSalesReturn
                               join f in oConnectionContext.DbClsSales
                               on b.SalesId equals f.SalesId
                               where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
               && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
               l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.Date) <= obj.ToDate
               && b.Status != "Draft"
               && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer")
                //&& b.TotalTaxAmount > 0
                && f.IsDeleted == false && f.IsCancelled == false
                && oConnectionContext.DbClsSalesReturnDetails
                 .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                           && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId))
                 && oConnectionContext.DbClsSalesReturnAdditionalCharges
                 .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                           && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId))
                        select new ClsGstSale
                               {
                                   InvoiceType = "Credit Note",
                                   SalesDate = b.Date,
                                   InvoiceNo = b.InvoiceNo,                                   
                               }).ToList();

            foreach (var item in det2)
            {
                var sd = (from c in oConnectionContext.DbClsSalesReturnDetails
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnDetailsId == c.SalesReturnDetailsId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesReturnAdditionalCharges
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnAdditionalChargesId == c.SalesReturnAdditionalChargesId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();
                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det3 = (from b in oConnectionContext.DbClsCustomerPayment
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
        && b.Type == "Customer Payment" && b.ParentId == 0
        //&& b.TaxAmount > 0 
        && b.SalesReturnId == 0
        && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer")
        && (b.TaxId != NilratedGstId && b.TaxId != NilratedIgstId && b.TaxId != ExemptedId)
                        select new ClsGstSaleDetail
                        {
                            InvoiceType = "Customer Advance Payment",
                            SalesDate = b.PaymentDate,
                            InvoiceNo = b.ReferenceNo,
                            AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsGstTaxType
                                        {
                                            TaxType = x.TaxType,
                                            TaxAmount = (from y in oConnectionContext.DbClsTax
                                                         join z in oConnectionContext.DbClsCustomerPaymentTaxJournal
                                                         on y.TaxId equals z.TaxId
                                                         where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                         && z.CustomerPaymentId == b.CustomerPaymentId
                                                         select (y.TaxPercent / 100) * (b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                        ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum())
                                                        ).DefaultIfEmpty().Sum()
                                        }).ToList()
                        }).ToList();

            var groupedResult1 = det1.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult2 = det2.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult3 = det3.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult = groupedResult1.Union(groupedResult2).Union(groupedResult3).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult OutwardTaxableZeroRated(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
           && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft" && (b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer")
        //&& b.TotalTaxAmount > 0
        && oConnectionContext.DbClsSalesDetails
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId))
                && oConnectionContext.DbClsSalesAdditionalCharges
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId))
                        select new ClsGstSale
                       {
                           InvoiceType = b.SalesType == "Sales Debit Note" ? "Sales Debit Note" : "Sales Invoice",
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,                          
                       }).ToList();

            foreach (var item in det1)
            {
                var sd = (from c in oConnectionContext.DbClsSalesDetails
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId)
                          select new ClsGstSaleDetail
                          {
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesDetailsId == c.SalesDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId)
                          select new ClsGstSaleDetail
                          {
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesAdditionalChargesId == c.SalesAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det2 = (from b in oConnectionContext.DbClsSalesReturn
                        join f in oConnectionContext.DbClsSales
                        on b.SalesId equals f.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && b.Status != "Draft"
        && (b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer")
         //&& b.TotalTaxAmount > 0
         && f.IsDeleted == false && f.IsCancelled == false
         && oConnectionContext.DbClsSalesReturnDetails
          .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                    && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId))
          && oConnectionContext.DbClsSalesReturnAdditionalCharges
          .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                    && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId))
                        select new ClsGstSale
                        {
                            InvoiceType = "Credit Note",
                            SalesDate = b.Date,
                            InvoiceNo = b.InvoiceNo,                            
                        }).ToList();

            foreach (var item in det2)
            {
                var sd = (from c in oConnectionContext.DbClsSalesReturnDetails
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnDetailsId == c.SalesReturnDetailsId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesReturnAdditionalCharges
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnAdditionalChargesId == c.SalesReturnAdditionalChargesId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det3 = (from b in oConnectionContext.DbClsCustomerPayment
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
        && b.Type == "Customer Payment" && b.ParentId == 0
        //&& b.TaxAmount > 0 
        && b.SalesReturnId == 0 &&
        (b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer")
        && (b.TaxId == NilratedGstId && b.TaxId == NilratedIgstId)
                        select new ClsGstSaleDetail
                        {
                            InvoiceType = "Customer Advance Payment",
                            SalesDate = b.PaymentDate,
                            InvoiceNo = b.ReferenceNo,
                            AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsGstTaxType
                                        {
                                            TaxType = x.TaxType,
                                            TaxAmount = (from y in oConnectionContext.DbClsTax
                                                         join z in oConnectionContext.DbClsCustomerPaymentTaxJournal
                                                         on y.TaxId equals z.TaxId
                                                         where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                         && z.CustomerPaymentId == b.CustomerPaymentId
                                                         select (y.TaxPercent / 100) * (b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                        ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum())
                                                        ).DefaultIfEmpty().Sum()
                                        }).ToList()
                        }).ToList();

            var groupedResult1 = det1.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult2 = det2.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult3 = det3.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult = groupedResult1.Union(groupedResult2).Union(groupedResult3).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult OtherOutwardNilRated(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
           && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            //            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            //&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft" && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer")
        //&& b.TotalTaxAmount > 0
        && oConnectionContext.DbClsSalesDetails
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId && c.TaxId == ExemptedId))
                && oConnectionContext.DbClsSalesAdditionalCharges
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId && c.TaxId == ExemptedId))
                        select new ClsGstSale
                       {
                           InvoiceType = b.SalesType == "Sales Debit Note" ? "Sales Debit Note" : "Sales Invoice",
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,                         
                       }).ToList();

            foreach (var item in det1)
            {
                var sd = (from c in oConnectionContext.DbClsSalesDetails
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId && c.TaxId == ExemptedId)
                          select new ClsGstSaleDetail
                          {
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesDetailsId == c.SalesDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId && c.TaxId == ExemptedId)
                          select new ClsGstSaleDetail
                          {
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesAdditionalChargesId == c.SalesAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det2 = (from b in oConnectionContext.DbClsSalesReturn
                        join f in oConnectionContext.DbClsSales
                        on b.SalesId equals f.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && b.Status != "Draft"
        && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer")
         //&& b.TotalTaxAmount > 0
         && f.IsDeleted == false && f.IsCancelled == false
         && oConnectionContext.DbClsSalesReturnDetails
          .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                    && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId && c.TaxId == ExemptedId))
          && oConnectionContext.DbClsSalesReturnAdditionalCharges
          .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                    && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId && c.TaxId == ExemptedId))
                        select new ClsGstSale
                        {
                            InvoiceType = "Credit Note",
                            SalesDate = b.Date,
                            InvoiceNo = b.InvoiceNo,                            
                        }).ToList();

            foreach (var item in det2)
            {
                var sd = (from c in oConnectionContext.DbClsSalesReturnDetails
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId && c.TaxId == ExemptedId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnDetailsId == c.SalesReturnDetailsId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesReturnAdditionalCharges
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId && c.TaxId == ExemptedId)
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnAdditionalChargesId == c.SalesReturnAdditionalChargesId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det3 = (from b in oConnectionContext.DbClsCustomerPayment
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
        && b.Type == "Customer Payment" && b.ParentId == 0
        //&& b.TaxAmount > 0 
        && b.SalesReturnId == 0 && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer")
        && (b.TaxId == NilratedGstId && b.TaxId == NilratedIgstId && b.TaxId == ExemptedId)
                        select new ClsGstSaleDetail
                        {
                            InvoiceType = "Customer Advance Payment",
                            SalesDate = b.PaymentDate,
                            InvoiceNo = b.ReferenceNo,
                            AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsGstTaxType
                                        {
                                            TaxType = x.TaxType,
                                            TaxAmount = (from y in oConnectionContext.DbClsTax
                                                         join z in oConnectionContext.DbClsCustomerPaymentTaxJournal
                                                         on y.TaxId equals z.TaxId
                                                         where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                         && z.CustomerPaymentId == b.CustomerPaymentId
                                                         select (y.TaxPercent / 100) * (b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                        ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum())
                                                        ).DefaultIfEmpty().Sum()
                                        }).ToList()
                        }).ToList();

            var groupedResult1 = det1.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult2 = det2.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult3 = det3.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult = groupedResult1.Union(groupedResult2).Union(groupedResult3).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult InwardReverseCharge(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsPurchase
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
       && b.Status != "Draft" && b.IsReverseCharge == 1
        && oConnectionContext.DbClsPurchaseDetails
                .Any(c => c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId))
                && oConnectionContext.DbClsPurchaseAdditionalCharges
                .Any(c => c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId))
                        select new ClsGstSale
                       {
                           InvoiceType = "Purchase Bill",
                           SalesDate = b.PurchaseDate,
                           InvoiceNo = b.ReferenceNo,
                           SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                           where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                           select new ClsGstSaleDetail
                                           {
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new ClsGstTaxType
                                                           {
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsPurchaseTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseId == b.PurchaseId
                                                                            && z.PurchaseDetailsId == c.PurchaseDetailsId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).Concat(from c in oConnectionContext.DbClsPurchaseAdditionalCharges
                                                     where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                                     && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                                     select new ClsGstSaleDetail
                                                     {
                                                         AmountExcTax = c.AmountExcTax,
                                                         TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                                     select new ClsGstTaxType
                                                                     {
                                                                         TaxType = x.TaxType,
                                                                         TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                                      join z in oConnectionContext.DbClsPurchaseAdditionalTaxJournal
                                                                                      on y.TaxId equals z.TaxId
                                                                                      where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseId == b.PurchaseId
                                                                                      && z.PurchaseAdditionalChargesId == c.PurchaseAdditionalChargesId
                                                                                      select z.TaxAmount
                                                                                     ).DefaultIfEmpty().Sum()
                                                                     }).ToList()
                                                     }).ToList()
                       }).ToList();

            var det2 = (from b in oConnectionContext.DbClsPurchaseReturn
                        join f in oConnectionContext.DbClsPurchase
                        on b.PurchaseId equals f.PurchaseId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && b.Status != "Draft" && b.IsReverseCharge == 1
         //&& b.TotalTaxAmount > 0
         && f.IsDeleted == false && f.IsCancelled == false
         && oConnectionContext.DbClsPurchaseReturnDetails
          .Any(c => c.PurchaseReturnId == b.PurchaseReturnId && c.IsDeleted == false && c.IsActive == true
                    && (c.TaxId != ExemptedId && c.TaxId != NonGstId))
          && oConnectionContext.DbClsPurchaseReturnAdditionalCharges
          .Any(c => c.PurchaseReturnId == b.PurchaseReturnId && c.IsDeleted == false && c.IsActive == true
                    && (c.TaxId != ExemptedId && c.TaxId != NonGstId))
                        select new ClsGstSale
                        {
                            InvoiceType = "Purchase Debit Note",
                            SalesDate = b.Date,
                            InvoiceNo = b.InvoiceNo,
                            SalesDetails = (from c in oConnectionContext.DbClsPurchaseReturnDetails
                                            where c.PurchaseReturnId == b.PurchaseReturnId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                            select new ClsGstSaleDetail
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = -c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new ClsGstTaxType
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
                                                                TaxType = x.TaxType,
                                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                             join z in oConnectionContext.DbClsPurchaseReturnTaxJournal
                                                                             on y.TaxId equals z.TaxId
                                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseReturnId == b.PurchaseReturnId
                                                                             && z.PurchaseReturnDetailsId == c.PurchaseReturnDetailsId
                                                                             select -z.TaxAmount
                                                                            ).DefaultIfEmpty().Sum()
                                                            }).ToList()
                                            }).Concat(from c in oConnectionContext.DbClsPurchaseReturnAdditionalCharges
                                                      where c.PurchaseReturnId == b.PurchaseReturnId && c.IsDeleted == false && c.IsActive == true
                                                      && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                                      select new ClsGstSaleDetail
                                                      {
                                                          TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                          AmountExcTax = -c.AmountExcTax,
                                                          TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                                      select new ClsGstTaxType
                                                                      {
                                                                          TaxTypeId = x.TaxTypeId,
                                                                          TaxType = x.TaxType,
                                                                          TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                                       join z in oConnectionContext.DbClsPurchaseReturnAdditionalTaxJournal
                                                                                       on y.TaxId equals z.TaxId
                                                                                       where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseReturnId == b.PurchaseReturnId
                                                                                       && z.PurchaseReturnAdditionalChargesId == c.PurchaseReturnAdditionalChargesId
                                                                                       select -z.TaxAmount
                                                                                      ).DefaultIfEmpty().Sum()
                                                                      }).ToList()
                                                      }).ToList()
                        }).ToList();

            var det3 = (from b in oConnectionContext.DbClsSupplierPayment
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
        && b.Type == "Supplier Payment" && b.ParentId == 0 && b.IsReverseCharge == 1
        //&& b.TaxAmount > 0 
        && b.PurchaseReturnId == 0 && (b.TaxId != ExemptedId && b.TaxId != NonGstId)
                        select new ClsGstSaleDetail
                        {
                            InvoiceType = "Supplier Advance Payment",
                            SalesDate = b.PaymentDate,
                            InvoiceNo = b.ReferenceNo,
                            AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
                                                 b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsGstTaxType
                                        {
                                            TaxType = x.TaxType,
                                            TaxAmount = (from y in oConnectionContext.DbClsTax
                                                         join z in oConnectionContext.DbClsSupplierPaymentTaxJournal
                                                         on y.TaxId equals z.TaxId
                                                         where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                         && z.SupplierPaymentId == b.SupplierPaymentId
                                                         select (y.TaxPercent / 100) * (b.AmountExcTax - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
                                                 b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                        ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum())
                                                        ).DefaultIfEmpty().Sum()
                                        }).ToList()
                        }).ToList();

            var groupedResult1 = det1.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult2 = det2.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult3 = det3.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult = groupedResult1.Union(groupedResult2).Union(groupedResult3).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult OutwardNonGst(ClsSalesVm obj)
        {
            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft"
        //&& b.TotalTaxAmount > 0
        && oConnectionContext.DbClsSalesDetails
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                          && c.TaxId == NonGstId)
                && oConnectionContext.DbClsSalesAdditionalCharges
                .Any(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                          && c.TaxId == NonGstId)
                        select new ClsGstSale
                       {
                           InvoiceType = b.SalesType == "Sales Debit Note" ? "Sales Debit Note" : "Sales Invoice",
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,                          
                       }).ToList();

            foreach (var item in det1)
            {
                var sd = (from c in oConnectionContext.DbClsSalesDetails
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && c.TaxId == NonGstId
                          select new ClsGstSaleDetail
                          {
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesDetailsId == c.SalesDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && c.TaxId == NonGstId
                          select new ClsGstSaleDetail
                          {
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesAdditionalChargesId == c.SalesAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det2 = (from b in oConnectionContext.DbClsSalesReturn
                        join f in oConnectionContext.DbClsSales
                        on b.SalesId equals f.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && b.Status != "Draft"
         //&& b.TotalTaxAmount > 0
         && f.IsDeleted == false && f.IsCancelled == false
         && oConnectionContext.DbClsSalesReturnDetails
          .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                    && c.TaxId == NonGstId)
          && oConnectionContext.DbClsSalesReturnAdditionalCharges
          .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                    && c.TaxId == NonGstId)
                        select new ClsGstSale
                        {
                            InvoiceType = "Credit Note",
                            SalesDate = b.Date,
                            InvoiceNo = b.InvoiceNo,                            
                        }).ToList();

            foreach (var item in det2)
            {
                var sd = (from c in oConnectionContext.DbClsSalesReturnDetails
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && c.TaxId == NonGstId
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnDetailsId == c.SalesReturnDetailsId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesReturnAdditionalCharges
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && c.TaxId == NonGstId
                          select new ClsGstSaleDetail
                          {
                              TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnAdditionalChargesId == c.SalesReturnAdditionalChargesId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det3 = (from b in oConnectionContext.DbClsCustomerPayment
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
        && b.Type == "Customer Payment" && b.ParentId == 0
        //&& b.TaxAmount > 0 
        && b.SalesReturnId == 0 && b.TaxId == NonGstId
                        select new ClsGstSaleDetail
                        {
                            InvoiceType = "Customer Advance Payment",
                            SalesDate = b.PaymentDate,
                            InvoiceNo = b.ReferenceNo,
                            AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsGstTaxType
                                        {
                                            TaxType = x.TaxType,
                                            TaxAmount = (from y in oConnectionContext.DbClsTax
                                                         join z in oConnectionContext.DbClsCustomerPaymentTaxJournal
                                                         on y.TaxId equals z.TaxId
                                                         where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                         && z.CustomerPaymentId == b.CustomerPaymentId
                                                         select (y.TaxPercent / 100) * (b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                        ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum())
                                                        ).DefaultIfEmpty().Sum()
                                        }).ToList()
                        }).ToList();

            var groupedResult1 = det1.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult2 = det2.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult3 = det3.Select(a => new ClsGstSale
            {
                InvoiceType = a.InvoiceType,
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult = groupedResult1.Union(groupedResult2).Union(groupedResult3).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult InterStateUnregistered(ClsSalesVm obj)
        {
            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
       && b.Status != "Draft" && b.PlaceOfSupplyId != BranchStateId && b.GstTreatment == "Taxable Supply to Unregistered Person" && b.TotalTaxAmount > 0
                       select new ClsGstSale
                       {
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),                          
                       }).ToList();

            foreach (var item in det1)
            {
                var sd = (from c in oConnectionContext.DbClsSalesDetails
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesDetailsId == c.SalesDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesAdditionalChargesId == c.SalesAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det2 = (from b in oConnectionContext.DbClsSalesReturn
                        join f in oConnectionContext.DbClsSales
                        on b.SalesId equals f.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && b.Status != "Draft"
         && b.PlaceOfSupplyId != BranchStateId && b.GstTreatment == "Taxable Supply to Unregistered Person" && b.TotalTaxAmount > 0
         && f.IsDeleted == false && f.IsCancelled == false
         && oConnectionContext.DbClsSalesReturnDetails
          .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                    && c.TaxId == NonGstId)
          && oConnectionContext.DbClsSalesReturnAdditionalCharges
          .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                    && c.TaxId == NonGstId)
                        select new ClsGstSale
                        {
                            InvoiceType = "Credit Note",
                            SalesDate = b.Date,
                            InvoiceNo = b.InvoiceNo,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),                            
                        }).ToList();

            foreach (var item in det2)
            {
                var sd = (from c in oConnectionContext.DbClsSalesReturnDetails
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && c.TaxId == NonGstId
                          select new ClsGstSaleDetail
                          {
                              //TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              //TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnDetailsId == c.SalesReturnDetailsId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesReturnAdditionalCharges
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && c.TaxId == NonGstId
                          select new ClsGstSaleDetail
                          {
                              //TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              //TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnAdditionalChargesId == c.SalesReturnAdditionalChargesId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det3 = (from b in oConnectionContext.DbClsCustomerPayment
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
        && b.Type == "Customer Payment" && b.ParentId == 0
        //&& b.TaxAmount > 0 
        && b.SalesReturnId == 0 && b.TaxId == NonGstId
        && b.PlaceOfSupplyId != BranchStateId && b.GstTreatment == "Taxable Supply to Unregistered Person" && b.TaxAmount > 0
                        select new ClsGstSaleDetail
                        {
                            InvoiceType = "Customer Advance Payment",
                            SalesDate = b.PaymentDate,
                            InvoiceNo = b.ReferenceNo,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsGstTaxType
                                        {
                                            TaxType = x.TaxType,
                                            TaxAmount = (from y in oConnectionContext.DbClsTax
                                                         join z in oConnectionContext.DbClsCustomerPaymentTaxJournal
                                                         on y.TaxId equals z.TaxId
                                                         where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                         && z.CustomerPaymentId == b.CustomerPaymentId
                                                         select (y.TaxPercent / 100) * (b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                        ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum())
                                                        ).DefaultIfEmpty().Sum()
                                        }).ToList()
                        }).ToList();           

            var groupedResult1 = det1.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                PlaceOfSupply = a.PlaceOfSupply,
                StateCode = a.StateCode,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult2 = det2.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                PlaceOfSupply = a.PlaceOfSupply,
                StateCode = a.StateCode,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult3 = det3.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                PlaceOfSupply = a.PlaceOfSupply,
                StateCode = a.StateCode,
                AmountExcTax = a.AmountExcTax,
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult = groupedResult1.Union(groupedResult2).Union(groupedResult3).ToList();

            if (obj.PlaceOfSupply != "" && obj.PlaceOfSupply != null)
            {
                groupedResult = groupedResult.Where(a => a.PlaceOfSupply == obj.PlaceOfSupply).ToList();
            }

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult InterStateComposition(ClsSalesVm obj)
        {
            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsSales
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft" && b.PlaceOfSupplyId != BranchStateId && b.GstTreatment == "Composition Taxable Supply" && b.TotalTaxAmount > 0
                        select new ClsGstSale
                        {
                            SalesDate = b.SalesDate,
                            InvoiceNo = b.InvoiceNo,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),                           
                        }).ToList();

            foreach (var item in det1)
            {
                var sd = (from c in oConnectionContext.DbClsSalesDetails
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesDetailsId == c.SalesDetailsId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesAdditionalCharges
                          where c.SalesId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                          select new ClsGstSaleDetail
                          {
                              AmountExcTax = c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesId == item.SalesId
                                                           && z.SalesAdditionalChargesId == c.SalesAdditionalChargesId
                                                           select z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det2 = (from b in oConnectionContext.DbClsSalesReturn
                        join f in oConnectionContext.DbClsSales
                        on b.SalesId equals f.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && b.Status != "Draft"
         && b.PlaceOfSupplyId != BranchStateId && b.GstTreatment == "Composition Taxable Supply" && b.TotalTaxAmount > 0
         && f.IsDeleted == false && f.IsCancelled == false
         && oConnectionContext.DbClsSalesReturnDetails
          .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                    && c.TaxId == NonGstId)
          && oConnectionContext.DbClsSalesReturnAdditionalCharges
          .Any(c => c.SalesReturnId == b.SalesReturnId && c.IsDeleted == false && c.IsActive == true
                    && c.TaxId == NonGstId)
                        select new ClsGstSale
                        {
                            InvoiceType = "Credit Note",
                            SalesDate = b.Date,
                            InvoiceNo = b.InvoiceNo,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),                            
                        }).ToList();

            foreach (var item in det2)
            {
                var sd = (from c in oConnectionContext.DbClsSalesReturnDetails
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && c.TaxId == NonGstId
                          select new ClsGstSaleDetail
                          {
                              //TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              //TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnDetailsId == c.SalesReturnDetailsId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                var ad = (from c in oConnectionContext.DbClsSalesReturnAdditionalCharges
                          where c.SalesReturnId == item.SalesId && c.IsDeleted == false && c.IsActive == true
                          && c.TaxId == NonGstId
                          select new ClsGstSaleDetail
                          {
                              //TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                              AmountExcTax = -c.AmountExcTax,
                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                          select new ClsGstTaxType
                                          {
                                              //TaxTypeId = x.TaxTypeId,
                                              TaxType = x.TaxType,
                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                           join z in oConnectionContext.DbClsSalesReturnAdditionalTaxJournal
                                                           on y.TaxId equals z.TaxId
                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                           && z.SalesReturnId == item.SalesId
                                                           && z.SalesReturnAdditionalChargesId == c.SalesReturnAdditionalChargesId
                                                           select -z.TaxAmount
                                                          ).DefaultIfEmpty().Sum()
                                          }).ToList()
                          }).ToList();

                item.SalesDetails = sd.Concat(ad).ToList();
            }

            var det3 = (from b in oConnectionContext.DbClsCustomerPayment
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
    && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
        && b.Type == "Customer Payment" && b.ParentId == 0
        //&& b.TaxAmount > 0 
        && b.SalesReturnId == 0 && b.TaxId == NonGstId
        && b.PlaceOfSupplyId != BranchStateId && b.GstTreatment == "Composition Taxable Supply" && b.TaxAmount > 0
                        select new ClsGstSaleDetail
                        {
                            InvoiceType = "Customer Advance Payment",
                            SalesDate = b.PaymentDate,
                            InvoiceNo = b.ReferenceNo,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                       ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsGstTaxType
                                        {
                                            TaxType = x.TaxType,
                                            TaxAmount = (from y in oConnectionContext.DbClsTax
                                                         join z in oConnectionContext.DbClsCustomerPaymentTaxJournal
                                                         on y.TaxId equals z.TaxId
                                                         where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                         && z.CustomerPaymentId == b.CustomerPaymentId
                                                         select (y.TaxPercent / 100) * (b.AmountExcTax - oConnectionContext.DbClsCustomerPayment.Where(c => c.ParentId ==
                                                 b.CustomerPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                                 && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                                 DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                        ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum())
                                                        ).DefaultIfEmpty().Sum()
                                        }).ToList()
                        }).ToList();

            var groupedResult1 = det1.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                PlaceOfSupply = a.PlaceOfSupply,
                StateCode = a.StateCode,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult2 = det2.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                PlaceOfSupply = a.PlaceOfSupply,
                StateCode = a.StateCode,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult3 = det3.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                PlaceOfSupply = a.PlaceOfSupply,
                StateCode = a.StateCode,
                AmountExcTax = a.AmountExcTax,
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var groupedResult = groupedResult1.Union(groupedResult2).Union(groupedResult3).ToList();

            if (obj.PlaceOfSupply != "" && obj.PlaceOfSupply != null)
            {
                groupedResult = groupedResult.Where(a => a.PlaceOfSupply == obj.PlaceOfSupply).ToList();
            }

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult ImportOfGoods(ClsSalesVm obj)
        {
            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsPurchase
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
       && b.Status != "Draft" && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)"
       && b.IsReverseCharge == 2
                       select new ClsGstSale
                       {
                           SalesDate = b.PurchaseDate,
                           InvoiceNo = b.ReferenceNo,
                           SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                           join d in oConnectionContext.DbClsItem
                                           on c.ItemId equals d.ItemId
                                           where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                           && d.ItemType.ToLower() == "product" && c.ITCType == "Eligible For ITC"
                                           select new ClsGstSaleDetail
                                           {
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new ClsGstTaxType
                                                           {
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsPurchaseTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseId == b.PurchaseId
                                                                            && z.PurchaseDetailsId == c.PurchaseDetailsId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult ImportOfServices(ClsSalesVm obj)
        {
            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsPurchase
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
       && b.Status != "Draft" && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" //&& b.TotalTaxAmount > 0
       && b.IsReverseCharge == 2
                       select new ClsGstSale
                       {
                           SalesDate = b.PurchaseDate,
                           InvoiceNo = b.ReferenceNo,
                           SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                           join d in oConnectionContext.DbClsItem
                                           on c.ItemId equals d.ItemId
                                           where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                           && d.ItemType.ToLower() == "service" && c.ITCType == "Eligible For ITC"
                                           select new ClsGstSaleDetail
                                           {
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new ClsGstTaxType
                                                           {
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsPurchaseTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseId == b.PurchaseId
                                                                            && z.PurchaseDetailsId == c.PurchaseDetailsId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult InwardLiableToReverseCharge(ClsSalesVm obj)
        {
            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsPurchase
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
       && b.Status != "Draft" && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" //&& b.TotalTaxAmount > 0
       && b.IsReverseCharge == 1
                       select new ClsGstSale
                       {
                           SalesDate = b.PurchaseDate,
                           InvoiceNo = b.ReferenceNo,
                           SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                           where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                            && c.ITCType == "Eligible For ITC"
                                           select new ClsGstSaleDetail
                                           {
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new ClsGstTaxType
                                                           {
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsPurchaseTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseId == b.PurchaseId
                                                                            && z.PurchaseDetailsId == c.PurchaseDetailsId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).Concat(from c in oConnectionContext.DbClsPurchaseAdditionalCharges
                                                     where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                                     && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                                      && c.ITCType == "Eligible For ITC"
                                                     select new ClsGstSaleDetail
                                                     {
                                                         AmountExcTax = c.AmountExcTax,
                                                         TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                                     select new ClsGstTaxType
                                                                     {
                                                                         TaxType = x.TaxType,
                                                                         TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                                      join z in oConnectionContext.DbClsPurchaseAdditionalTaxJournal
                                                                                      on y.TaxId equals z.TaxId
                                                                                      where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseId == b.PurchaseId
                                                                                      && z.PurchaseAdditionalChargesId == c.PurchaseAdditionalChargesId
                                                                                      select z.TaxAmount
                                                                                     ).DefaultIfEmpty().Sum()
                                                                     }).ToList()
                                                     }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult AllOtherItc(ClsSalesVm obj)
        {
            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsPurchase
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
       && b.Status != "Draft" && b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" //&& b.TotalTaxAmount > 0
       //&& b.IsReverseCharge == 1
                       select new ClsGstSale
                       {
                           SalesDate = b.PurchaseDate,
                           InvoiceNo = b.ReferenceNo,
                           SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                           where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != NilratedGstId && c.TaxId != NilratedIgstId && c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                            && c.ITCType == "Eligible For ITC"
                                           select new ClsGstSaleDetail
                                           {
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new ClsGstTaxType
                                                           {
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsPurchaseTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseId == b.PurchaseId
                                                                            && z.PurchaseDetailsId == c.PurchaseDetailsId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult InwardInterComposition(ClsSalesVm obj)
        {
            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var composition = (from b in oConnectionContext.DbClsPurchase
                               where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
               && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
               l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
           && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
               && b.Status != "Draft" && b.DestinationOfSupplyId != BranchStateId && b.GstTreatment == "Composition Taxable Supply"
                               select new ClsGstSale
                               {
                                   SalesDate = b.PurchaseDate,
                                   InvoiceNo = b.ReferenceNo,
                                   SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                                   where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                                   select new ClsGstSaleDetail
                                                   {
                                                       AmountExcTax = c.AmountExcTax,
                                                   }).ToList()

                               }).ToList();

            var nilrated = (from b in oConnectionContext.DbClsPurchase
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
            && b.Status != "Draft" && b.DestinationOfSupplyId != BranchStateId
                            select new ClsGstSale
                            {
                                SalesDate = b.PurchaseDate,
                                InvoiceNo = b.ReferenceNo,
                                SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                                where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                                && (c.TaxId == NilratedGstId || c.TaxId == NilratedIgstId || c.TaxId == ExemptedId)
                                                select new ClsGstSaleDetail
                                                {
                                                    AmountExcTax = c.AmountExcTax,
                                                }).ToList()

                            }).ToList();

            var det = composition.Union(nilrated);

            var groupedResult = det.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),                
            }).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult InwardIntraComposition(ClsSalesVm obj)
        {
            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NilratedGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "GST 0%").Select(a => a.TaxId).FirstOrDefault();

            long NilratedIgstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var composition = (from b in oConnectionContext.DbClsPurchase
                               where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
               && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
               l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
           && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
               && b.Status != "Draft" && b.DestinationOfSupplyId == BranchStateId && b.GstTreatment == "Composition Taxable Supply"
                               select new ClsGstSale
                               {
                                   SalesDate = b.PurchaseDate,
                                   InvoiceNo = b.ReferenceNo,
                                   SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                                   where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                                   select new ClsGstSaleDetail
                                                   {
                                                       AmountExcTax = c.AmountExcTax,
                                                   }).ToList()

                               }).ToList();

            var nilrated = (from b in oConnectionContext.DbClsPurchase
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
            && b.Status != "Draft" && b.DestinationOfSupplyId == BranchStateId
                            select new ClsGstSale
                            {
                                SalesDate = b.PurchaseDate,
                                InvoiceNo = b.ReferenceNo,
                                SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                                where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                                && (c.TaxId == NilratedGstId || c.TaxId == NilratedIgstId || c.TaxId == ExemptedId)
                                                select new ClsGstSaleDetail
                                                {
                                                    AmountExcTax = c.AmountExcTax,
                                                }).ToList()

                            }).ToList();

            var det = composition.Union(nilrated);

            var groupedResult = det.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
            }).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult InwardInterNonGst(ClsSalesVm obj)
        {
            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsPurchase
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
       && b.Status != "Draft" && b.DestinationOfSupplyId != BranchStateId
                       select new ClsGstSale
                       {
                           SalesDate = b.PurchaseDate,
                           InvoiceNo = b.ReferenceNo,
                           SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                           where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                           && c.TaxId == NonGstId
                                           select new ClsGstSaleDetail
                                           {
                                               AmountExcTax = c.AmountExcTax,
                                           }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
            }).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        public ClsGstResult InwardIntraNonGst(ClsSalesVm obj)
        {
            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsPurchase
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
       && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
       l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
       DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
       && b.Status != "Draft" && b.DestinationOfSupplyId == BranchStateId
                       select new ClsGstSale
                       {
                           SalesDate = b.PurchaseDate,
                           InvoiceNo = b.ReferenceNo,
                           SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                           where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                           && c.TaxId == NonGstId
                                           select new ClsGstSaleDetail
                                           {
                                               AmountExcTax = c.AmountExcTax,
                                           }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsGstSale
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
            }).ToList();

            var finalResult = new ClsGstResult
            {
                //Sales = det,
                GroupedSales = groupedResult
            };

            return finalResult;
        }

        #endregion

    }
}
