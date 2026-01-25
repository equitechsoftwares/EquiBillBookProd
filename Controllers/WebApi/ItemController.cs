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
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    //[IdentityBasicAuthenticationAttribute]
    public class ItemController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> AllItems(ClsItemVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //   oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //     && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
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

            List<ClsItemVm> det;
            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsItem
                       join e in oConnectionContext.DbClsItemBranchMap
on a.ItemId equals e.ItemId
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false
               //&& DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
               //    DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == e.BranchId)
                       select new ClsItemVm
                       {
                           //BranchId = e.BranchId,
                           ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemId == a.ItemId && b.IsDeleted == false).Select(b => b.ItemDetailsId).FirstOrDefault(),
                           ItemType = a.ItemType,
                           Brand = oConnectionContext.DbClsBrand.Where(b => b.BrandId == a.BrandId).Select(b => b.Brand).FirstOrDefault(),
                           UnitName = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                           ItemId = a.ItemId,
                           ItemCode = a.ItemCode,
                           ItemName = a.ItemName,
                           Description = a.Description,
                           SkuCode = a.SkuCode,
                           HsnCode = oConnectionContext.DbClsItemCode.Where(b => b.ItemCodeId == a.ItemCodeId).Select(b => b.Code).FirstOrDefault(),
                           BarcodeType = a.BarcodeType,
                           UnitId = a.UnitId,
                           BrandId = a.BrandId,
                           CategoryId = a.CategoryId,
                           Category = oConnectionContext.DbClsCategory.Where(b => b.CategoryId == a.CategoryId).Select(b => b.Category).FirstOrDefault(),
                           SubCategoryId = a.SubCategoryId,
                           SubCategory = oConnectionContext.DbClsSubCategory.Where(b => b.SubCategoryId == a.SubCategoryId).Select(b => b.SubCategory).FirstOrDefault(),
                           SubSubCategoryId = a.SubSubCategoryId,
                           SubSubCategory = oConnectionContext.DbClsSubSubCategory.Where(b => b.SubSubCategoryId == a.SubSubCategoryId).Select(b => b.SubSubCategory).FirstOrDefault(),
                           IsManageStock = a.IsManageStock,
                           AlertQuantity = a.AlertQuantity,
                           ProductImage = a.ProductImage,
                           ProductBrochure = a.ProductBrochure,
                           TaxId = a.TaxId,
                           TaxType = a.TaxType,
                           ProductType = a.ProductType,
                           CompanyId = a.CompanyId,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           EnableImei = a.EnableImei
                       }).Distinct().ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsItem
                       join e in oConnectionContext.DbClsItemBranchMap
on a.ItemId equals e.ItemId
                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false
                //&& DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                //    DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                && e.BranchId == obj.BranchId
                       select new ClsItemVm
                       {
                           //BranchId = e.BranchId,
                           ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemId == a.ItemId && b.IsDeleted == false).Select(b => b.ItemDetailsId).FirstOrDefault(),
                           ItemType = a.ItemType,
                           Brand = oConnectionContext.DbClsBrand.Where(b => b.BrandId == a.BrandId).Select(b => b.Brand).FirstOrDefault(),
                           UnitName = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                           ItemId = a.ItemId,
                           ItemCode = a.ItemCode,
                           ItemName = a.ItemName,
                           Description = a.Description,
                           SkuCode = a.SkuCode,
                           HsnCode = oConnectionContext.DbClsItemCode.Where(b => b.ItemCodeId == a.ItemCodeId).Select(b => b.Code).FirstOrDefault(),
                           BarcodeType = a.BarcodeType,
                           UnitId = a.UnitId,
                           BrandId = a.BrandId,
                           CategoryId = a.CategoryId,
                           Category = oConnectionContext.DbClsCategory.Where(b => b.CategoryId == a.CategoryId).Select(b => b.Category).FirstOrDefault(),
                           SubCategoryId = a.SubCategoryId,
                           SubCategory = oConnectionContext.DbClsSubCategory.Where(b => b.SubCategoryId == a.SubCategoryId).Select(b => b.SubCategory).FirstOrDefault(),
                           SubSubCategoryId = a.SubSubCategoryId,
                           SubSubCategory = oConnectionContext.DbClsSubSubCategory.Where(b => b.SubSubCategoryId == a.SubSubCategoryId).Select(b => b.SubSubCategory).FirstOrDefault(),
                           IsManageStock = a.IsManageStock,
                           AlertQuantity = a.AlertQuantity,
                           ProductImage = a.ProductImage,
                           ProductBrochure = a.ProductBrochure,
                           TaxId = a.TaxId,
                           TaxType = a.TaxType,
                           ProductType = a.ProductType,
                           CompanyId = a.CompanyId,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           EnableImei = a.EnableImei
                       }).Distinct().ToList();
            }

            if (obj.ItemType != null && obj.ItemType != "")
            {
                det = det.Where(a => a.ItemType == obj.ItemType).Select(a => a).ToList();
            }

            if (obj.CategoryId != 0)
            {
                det = det.Where(a => a.CategoryId == obj.CategoryId).Select(a => a).ToList();
            }

            if (obj.SubCategoryId != 0)
            {
                det = det.Where(a => a.SubCategoryId == obj.SubCategoryId).Select(a => a).ToList();
            }

            if (obj.SubSubCategoryId != 0)
            {
                det = det.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).Select(a => a).ToList();
            }

            if (obj.BrandId != 0)
            {
                det = det.Where(a => a.BrandId == obj.BrandId).Select(a => a).ToList();
            }

            if (obj.UnitId != 0)
            {
                det = det.Where(a => a.UnitId == obj.UnitId).Select(a => a).ToList();
            }

            if (obj.SkuCode != null && obj.SkuCode != "")
            {
                det = det.Where(a => a.SkuCode.ToLower().Contains(obj.SkuCode.ToLower())).Select(a => a).ToList();
            }

            if (obj.HsnCode != null && obj.HsnCode != "")
            {
                det = det.Where(a => a.HsnCode.ToLower().Contains(obj.HsnCode.ToLower())).Select(a => a).ToList();
            }

            if (obj.ItemName != null && obj.HsnCode != "")
            {
                det = det.Where(a => a.ItemName.ToLower().Contains(obj.ItemName.ToLower())).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Items = det.OrderByDescending(a => a.ItemId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> Item(ClsItem obj)
        {
            var det = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                TaxPreference = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxPreferenceId).Select(b => b.Tax).FirstOrDefault(),
                a.TaxPreferenceId,
                a.TaxExemptionId,
                a.ItemCodeId,
                //ItemCodeType = oConnectionContext.DbClsItemCode.Where(b=>b.ItemCodeId == a.ItemCodeId).Select(b=>b.Type).FirstOrDefault(),
                Warranty = oConnectionContext.DbClsWarranty.Where(b => b.WarrantyId == a.WarrantyId).Select(b => b.Warranty).FirstOrDefault(),
                a.WarrantyId,
                a.ItemType,
                a.ExpiryPeriod,
                a.ExpiryPeriodType,
                ItemId = a.ItemId,
                ItemCode = a.ItemCode,
                ItemName = a.ItemName,
                Description = a.Description,
                SkuCode = a.SkuCode,
                HsnCode = oConnectionContext.DbClsItemCode.Where(b => b.ItemCodeId == a.ItemCodeId).Select(b => b.Code).FirstOrDefault(),
                BarcodeType = a.BarcodeType,
                UnitId = a.UnitId,
                UnitName = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                UnitShortName = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitShortName).FirstOrDefault(),
                a.SecondaryUnitId,
                SecondaryUnitName = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitShortName).FirstOrDefault(),
                a.TertiaryUnitId,
                TertiaryUnitName = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitShortName).FirstOrDefault(),
                a.QuaternaryUnitId,
                QuaternaryUnitName = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
                QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitShortName).FirstOrDefault(),
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                BrandId = a.BrandId,
                CategoryId = a.CategoryId,
                SubCategoryId = a.SubCategoryId,
                SubSubCategoryId = a.SubSubCategoryId,
                IsManageStock = a.IsManageStock,
                AlertQuantity = a.AlertQuantity,
                ProductImage = a.ProductImage == null ? "/Content/assets/img/item.jpg" : a.ProductImage,
                ProductBrochure = a.ProductBrochure == null ? "/Content/assets/img/item.jpg" : a.ProductBrochure,
                TaxId = a.TaxId,
                a.InterStateTaxId,
                TaxType = a.TaxType,
                ProductType = a.ProductType,
                CompanyId = a.CompanyId,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                a.BranchIds,
                a.PriceAddedFor,
                a.EnableImei,
                a.SaltId,
                Branchs = oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true && b.IsDeleted == false).Select(b => new
                {
                    b.Branch,
                    b.BranchId,
                    IsChecked = oConnectionContext.DbClsItemBranchMap.Where(c => c.BranchId == b.BranchId && c.ItemId == a.ItemId && c.IsActive == true && c.IsDeleted == false).Count() == 0 ? false : true,
                    Rack = oConnectionContext.DbClsItemBranchMap.Where(c => c.BranchId == b.BranchId && c.ItemId == a.ItemId && c.IsActive == true && c.IsDeleted == false).Select(c => c.Rack).DefaultIfEmpty().FirstOrDefault(),
                    Row = oConnectionContext.DbClsItemBranchMap.Where(c => c.BranchId == b.BranchId && c.ItemId == a.ItemId && c.IsActive == true && c.IsDeleted == false).Select(c => c.Row).DefaultIfEmpty().FirstOrDefault(),
                    Position = oConnectionContext.DbClsItemBranchMap.Where(c => c.BranchId == b.BranchId && c.ItemId == a.ItemId && c.IsActive == true && c.IsDeleted == false).Select(c => c.Position).DefaultIfEmpty().FirstOrDefault(),
                }).ToList(),
                //Branchs = a.BranchIds.Split(','),
                TotalCost = oConnectionContext.DbClsItemDetails.Where(b => b.ItemId == a.ItemId && b.IsDeleted == false).Select(b => b.TotalCost).DefaultIfEmpty().Sum(),
                ItemDetails = oConnectionContext.DbClsItemDetails.Where(b => b.ItemId == a.ItemId && b.IsDeleted == false).GroupBy(p =>
                p.VariationId, (key, g) => new
                {
                    VariationId = key,
                    Variation = oConnectionContext.DbClsVariation.Where(c => c.VariationId == key).Select(c => c.Variation).FirstOrDefault(),
                    VariationDetails = g.ToList().Select(h => new
                    {
                        h.SalesAccountId,
                        h.PurchaseAccountId,
                        h.InventoryAccountId,
                        h.DefaultMrp,
                        h.DefaultProfitMargin,
                        h.ItemDetailsId,
                        h.ItemId,
                        ProductImage = h.ProductImage == null ? "/Content/assets/img/item.jpg" : h.ProductImage,
                        h.PurchaseExcTax,
                        h.PurchaseIncTax,
                        h.Quantity,
                        h.SalesExcTax,
                        h.SalesIncTax,
                        h.SKU,
                        h.VariationDetailsId,
                        VariationDetails = oConnectionContext.DbClsVariationDetails.Where(z => z.VariationDetailsId == h.VariationDetailsId).Select(z => z.VariationDetails).FirstOrDefault(),
                        h.VariationId,
                        h.TotalCost,
                        AttributeMappings = oConnectionContext.DbClsItemDetailsVariationMap
                            .Where(m => m.ItemDetailsId == h.ItemDetailsId && m.IsDeleted == false)
                            .Select(m => new
                            {
                                ItemDetailsVariationMapId = m.ItemDetailsVariationMapId,
                                VariationId = m.VariationId,
                                VariationDetailsId = m.VariationDetailsId,
                                VariationName = oConnectionContext.DbClsVariation.Where(v => v.VariationId == m.VariationId).Select(v => v.Variation).FirstOrDefault(),
                                VariationDetailsName = oConnectionContext.DbClsVariationDetails.Where(vd => vd.VariationDetailsId == m.VariationDetailsId).Select(vd => vd.VariationDetails).FirstOrDefault()
                            }).ToList(),
                        ComboItemDetailsId = h.ComboItemDetailsId,
                        ComboUnitName = oConnectionContext.DbClsUnit.Where(bb => bb.UnitId == oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.ItemId).FirstOrDefault()).Select(b => b.UnitId).FirstOrDefault()).Select(b => b.UnitName).FirstOrDefault(),
                        ComboVariationName = oConnectionContext.DbClsVariationDetails.Where(bb => bb.VariationDetailsId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.VariationDetailsId).FirstOrDefault()).Select(b => b.VariationDetails).FirstOrDefault(),
                        ComboProductName = oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.ItemId).FirstOrDefault()).Select(b => b.ItemName).FirstOrDefault(),
                        ComboSku = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == h.ComboItemDetailsId).Select(b => b.SKU).FirstOrDefault(),
                        ComboPurchaseExcTax = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == h.ComboItemDetailsId).Select(b => b.PurchaseExcTax).FirstOrDefault(),
                        //ComboPurchaseIncTax = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == h.ComboItemDetailsId).Select(b => b.PurchaseIncTax).FirstOrDefault(),
                        UnitId = oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.ItemId).FirstOrDefault()).Select(b => b.UnitId).FirstOrDefault(),
                        SecondaryUnitId = oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.ItemId).FirstOrDefault()).Select(b => b.SecondaryUnitId).FirstOrDefault(),
                        TertiaryUnitId = oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.ItemId).FirstOrDefault()).Select(b => b.TertiaryUnitId).FirstOrDefault(),
                        QuaternaryUnitId = oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.ItemId).FirstOrDefault()).Select(b => b.QuaternaryUnitId).FirstOrDefault(),
                        UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(cc => cc.ItemDetailsId == h.ComboItemDetailsId).Select(cc => cc.ItemId).FirstOrDefault()).Select(b => b.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                        SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(c => c.SecondaryUnitId == oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(cc => cc.ItemDetailsId == h.ComboItemDetailsId).Select(cc => cc.ItemId).FirstOrDefault()).Select(b => b.SecondaryUnitId).FirstOrDefault()).Select(c => c.SecondaryUnitShortName).FirstOrDefault(),
                        TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(c => c.TertiaryUnitId == oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(cc => cc.ItemDetailsId == h.ComboItemDetailsId).Select(cc => cc.ItemId).FirstOrDefault()).Select(b => b.TertiaryUnitId).FirstOrDefault()).Select(c => c.TertiaryUnitShortName).FirstOrDefault(),
                        QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(c => c.QuaternaryUnitId == oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(cc => cc.ItemDetailsId == h.ComboItemDetailsId).Select(cc => cc.ItemId).FirstOrDefault()).Select(b => b.QuaternaryUnitId).FirstOrDefault()).Select(c => c.QuaternaryUnitShortName).FirstOrDefault(),
                        //AllowDecimal = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(cc => cc.ItemDetailsId == h.ComboItemDetailsId).Select(cc => cc.ItemId).FirstOrDefault()).Select(b => b.UnitId).FirstOrDefault()).Select(c => c.AllowDecimal).FirstOrDefault(),
                        //SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(c => c.SecondaryUnitId == oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(cc => cc.ItemDetailsId == h.ComboItemDetailsId).Select(cc => cc.ItemId).FirstOrDefault()).Select(b => b.SecondaryUnitId).FirstOrDefault()).Select(c => c.SecondaryUnitAllowDecimal).FirstOrDefault(),
                        //TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(c => c.TertiaryUnitId == oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(cc => cc.ItemDetailsId == h.ComboItemDetailsId).Select(cc => cc.ItemId).FirstOrDefault()).Select(b => b.TertiaryUnitId).FirstOrDefault()).Select(c => c.TertiaryUnitAllowDecimal).FirstOrDefault(),
                        //QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(c => c.QuaternaryUnitId == oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(cc => cc.ItemDetailsId == h.ComboItemDetailsId).Select(cc => cc.ItemId).FirstOrDefault()).Select(b => b.QuaternaryUnitId).FirstOrDefault()).Select(c => c.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                        UToSValue = oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.ItemId).FirstOrDefault()).Select(b => b.UToSValue).FirstOrDefault(),
                        SToTValue = oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.ItemId).FirstOrDefault()).Select(b => b.SToTValue).FirstOrDefault(),
                        TToQValue = oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.ItemId).FirstOrDefault()).Select(b => b.TToQValue).FirstOrDefault(),
                        PriceAddedFor = h.PriceAddedFor,
                        //PriceAddedFor = oConnectionContext.DbClsItem.Where(b => b.ItemId == oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == h.ComboItemDetailsId).Select(c => c.ItemId).FirstOrDefault()).Select(b => b.PriceAddedFor).FirstOrDefault(),
                    })
                }).ToList()
            }).FirstOrDefault();

            var Units = oConnectionContext.DbClsUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                UnitId = a.UnitId,
                a.UnitCode,
                UnitName = a.UnitName,
                a.UnitShortName
            }).OrderBy(a => a.UnitName).ToList();

            var SecondaryUnits = oConnectionContext.DbClsSecondaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && a.IsActive == true && a.UnitId == det.UnitId).Select(a => new
                 {
                     SecondaryUnitId = a.SecondaryUnitId,
                     a.SecondaryUnitCode,
                     SecondaryUnitName = a.SecondaryUnitName,
                     a.SecondaryUnitShortName
                 }).OrderBy(a => a.SecondaryUnitName).ToList();

            var TertiaryUnits = oConnectionContext.DbClsTertiaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.IsActive == true && a.SecondaryUnitId == det.SecondaryUnitId).Select(a => new
            {
                TertiaryUnitId = a.TertiaryUnitId,
                a.TertiaryUnitCode,
                TertiaryUnitName = a.TertiaryUnitName,
                a.TertiaryUnitShortName
            }).OrderBy(a => a.TertiaryUnitName).ToList();

            var QuaternaryUnits = oConnectionContext.DbClsQuaternaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && a.IsActive == true && a.TertiaryUnitId == det.TertiaryUnitId).Select(a => new
            {
                QuaternaryUnitId = a.QuaternaryUnitId,
                a.QuaternaryUnitCode,
                QuaternaryUnitName = a.QuaternaryUnitName,
                a.QuaternaryUnitShortName
            }).OrderBy(a => a.QuaternaryUnitName).ToList();

            var Brands = oConnectionContext.DbClsBrand.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                BrandId = a.BrandId,
                //a.BrandCode,
                Brand = a.Brand,
            }).OrderBy(a => a.Brand).ToList();

            var Categories = oConnectionContext.DbClsCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                CategoryId = a.CategoryId,
                //a.CategoryCode,
                Category = a.Category,
            }).OrderBy(a => a.Category).ToList();

            var SubCategories = oConnectionContext.DbClsSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true &&
            a.CategoryId == det.CategoryId).Select(a => new
            {
                SubCategoryId = a.SubCategoryId,
                //a.SubCategoryCode,
                SubCategory = a.SubCategory,
            }).OrderBy(a => a.SubCategory).ToList();

            var SubSubCategories = oConnectionContext.DbClsSubSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true &&
            a.SubCategoryId == det.SubCategoryId).Select(a => new
            {
                SubSubCategoryId = a.SubSubCategoryId,
                //a.SubSubCategoryCode,
                SubSubCategory = a.SubSubCategory,
            }).OrderBy(a => a.SubSubCategory).ToList();

            var Branchs = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                BranchId = a.BranchId,
                BranchCode = a.BranchCode,
                Branch = a.Branch,
            }).OrderBy(a => a.Branch).ToList();

            var Taxs = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
            && a.ForTaxGroupOnly == false).Select(a => new
            {
                TaxId = a.TaxId,
                a.TaxPercent,
                Tax = a.Tax,
                a.CanDelete,
                a.TaxTypeId
            }).OrderBy(a => a.TaxId).ToList();

            var Variations = oConnectionContext.DbClsVariation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                VariationId = a.VariationId,
                Variation = a.Variation,
            }).OrderBy(a => a.Variation).ToList();

            var Warrantys = oConnectionContext.DbClsWarranty.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                WarrantyId = a.WarrantyId,
                Warranty = a.Warranty,
            }).OrderBy(a => a.Warranty).ToList();

            string ItemCodeType = "";
            if (det.ItemType == "Product")
            {
                ItemCodeType = "HSN";
            }
            else
            {
                ItemCodeType = "SAC";
            }
            var ItemCodes = oConnectionContext.DbClsItemCode.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
                 && a.IsActive == true && a.ItemCodeType == ItemCodeType).Select(a => new ClsItemCodeVm
                 {
                     ItemCodeType = a.ItemCodeType,
                     ItemCodeId = a.ItemCodeId,
                     Code = a.Code,
                 }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Item = det,
                    Units = Units,
                    Brands = Brands,
                    Categories = Categories,
                    SubCategories = SubCategories,
                    SubSubCategories = SubSubCategories,
                    Branchs = Branchs,
                    Taxs = Taxs,
                    Variations = Variations,
                    Warrantys = Warrantys,
                    SecondaryUnits = SecondaryUnits,
                    TertiaryUnits = TertiaryUnits,
                    QuaternaryUnits = QuaternaryUnits,
                    ItemCodes = ItemCodes
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertItem(ClsItemVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //int TotalItemUsed = (from a in oConnectionContext.DbClsItemDetails
                //                     join b in oConnectionContext.DbClsItem
                //                        on a.ItemId equals b.ItemId
                //                     where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                //                        && b.IsDeleted == false && b.IsCancelled == false
                //                     select a.ItemDetailsId).Count();
                //int TotalItem = oCommonController.fetchPlanQuantity(obj.CompanyId, "Item");
                //if (TotalItemUsed >= TotalItem)
                //{
                //    data = new
                //    {
                //        Status = 0,
                //        Message = "Item quota already used. Please upgrade addons from My Plan Menu",
                //        Data = new
                //        {
                //        }
                //    };
                //    return await Task.FromResult(Ok(data));
                //}

                long PrefixUserMapId = 0;

                if (obj.SkuCode == "" || obj.SkuCode == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSkuCode" });
                    isError = true;
                }

                if (obj.ItemName == null || obj.ItemName == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divItemName" });
                    isError = true;
                }

                if (obj.ItemType == "Product")
                {
                    if (obj.UnitId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divUnit" });
                        isError = true;
                    }
                }

                if (obj.CategoryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCategory" });
                    isError = true;
                }

                if (obj.TaxType == "" || obj.TaxType == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTaxType" });
                    isError = true;
                }

                if (obj.ProductType == "" || obj.ProductType == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divProductType" });
                    isError = true;
                }

                if (obj.ItemBranchMaps == null || obj.ItemBranchMaps.Count == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranches" });
                    isError = true;
                }

                if (obj.SkuCode != "" && obj.SkuCode != null)
                {
                    if (oConnectionContext.DbClsItem.Where(a => a.SkuCode.ToLower() == obj.SkuCode.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate SKU exists", Id = "divSkuCode" });
                        isError = true;
                    }
                }

                //if (obj.HsnCode != "" && obj.HsnCode != null)
                //{
                //    if (oConnectionContext.DbClsItem.Where(a => a.HsnCode.ToLower() == obj.HsnCode.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "Duplicate Hsn exists", Id = "divHsnCode" });
                //        isError = true;
                //    }
                //}

                if (obj.TaxPreference == "Taxable")
                {
                    var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                    { CountryId = a.CountryId, IsBusinessRegistered = a.IsBusinessRegistered, BusinessRegistrationType = a.BusinessRegistrationType }).FirstOrDefault();

                    if (BusinessSetting.CountryId == 2)
                    {
                        if (BusinessSetting.IsBusinessRegistered == 1 && BusinessSetting.BusinessRegistrationType == "Regular")
                        {
                            if (obj.TaxId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divTax" });
                                isError = true;
                            }

                            if (obj.InterStateTaxId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divInterStateTax" });
                                isError = true;
                            }
                        }
                    }
                    else
                    {
                        if (obj.TaxId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divTax" });
                            isError = true;
                        }
                    }
                }
                else if (obj.TaxPreference == "Non-Taxable")
                {
                    if (obj.TaxExemptionId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divTaxExemption" });
                        isError = true;
                    }
                }

                int skuCount = 1;
                ClsMenuVm oClsMenuVm = new ClsMenuVm();
                oClsMenuVm.AddedBy = obj.AddedBy;
                oClsMenuVm.CompanyId = obj.CompanyId;

                bool IsPurchaseAddon = oCommonController.PlanAddons(oClsMenuVm).Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
                if (obj.ProductType == "Single")
                {
                    if (IsPurchaseAddon == true)
                    {
                        if (obj.ItemType == "Product")
                        {
                            if (obj.ItemDetails[0].PurchaseExcTax == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseExcTax" });
                                isError = true;
                            }
                        }

                        //if (obj.ItemDetails[0].DefaultProfitMargin == 0)
                        //{
                        //    errors.Add(new ClsError { Message = "This field is required", Id = "divDefaultProfitMargin" });
                        //    isError = true;
                        //}
                    }
                    else
                    {
                        obj.ItemDetails[0].PurchaseExcTax = obj.ItemDetails[0].SalesExcTax;
                        obj.ItemDetails[0].PurchaseIncTax = obj.ItemDetails[0].SalesIncTax;
                    }
                    if (obj.ItemDetails[0].SalesExcTax == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSalesExcTax" });
                        isError = true;
                    }
                }
                else if (obj.ProductType == "Variable")
                {
                    if (obj.ItemDetails == null || obj.ItemDetails.Count == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divVariationValues" });
                        errors.Add(new ClsError { Message = "This field is required", Id = "divAttributeSelections" });
                        isError = true;
                    }
                    else
                    {
                        foreach (var detail in obj.ItemDetails)
                        {
                            NormalizeAttributeMappings(detail);
                        }
                        foreach (var item in obj.ItemDetails)
                        {
                            int innerSkuCount = 1;
                            foreach (var inner in obj.ItemDetails)
                            {
                                if (item.SKU != "" && item.SKU != null)
                                {
                                    if (item.SKU.ToLower() == inner.SKU.ToLower() && skuCount != innerSkuCount)
                                    {
                                        errors.Add(new ClsError { Message = "Duplicate SKU exists" + item.DivId, Id = "divSku" + item.DivId });
                                        isError = true;
                                    }
                                }

                                innerSkuCount++;
                            }
                            skuCount++;
                        }

                        foreach (var item in obj.ItemDetails)
                        {
                            if (oConnectionContext.DbClsItem.Where(a => a.SkuCode.ToLower() == item.SKU.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                            {
                                errors.Add(new ClsError { Message = "Duplicate SKU exists", Id = "divSku" + item.DivId });
                                isError = true;
                            }

                            if (oConnectionContext.DbClsItemDetails.Where(a => a.SKU.ToLower() == item.SKU.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                            {
                                errors.Add(new ClsError { Message = "Duplicate SKU exists", Id = "divSku" + item.DivId });
                                isError = true;
                            }

                            if (item.SKU == "" || item.SKU == null)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSku" + item.DivId });
                                isError = true;
                            }
                            bool hasAttributeMappings = item.AttributeMappings != null && item.AttributeMappings.Count > 0;
                            var variationErrorId = hasAttributeMappings ? "divAttributeSelections" : "divVariation" + item.DivId;
                            if (!hasAttributeMappings && item.VariationId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = variationErrorId });
                                isError = true;
                            }

                            if (IsPurchaseAddon == true)
                            {
                                if (obj.ItemType == "Product")
                                {
                                    if (item.PurchaseExcTax == 0)
                                    {
                                        errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseExcTax" + item.DivId });
                                        isError = true;
                                    }
                                    //if (item.DefaultProfitMargin == 0)
                                    //{
                                    //    errors.Add(new ClsError { Message = "This field is required", Id = "divDefaultProfitMargin" + item.DivId });
                                    //    isError = true;
                                    //}
                                }
                            }
                            else
                            {
                                item.PurchaseExcTax = item.SalesExcTax;
                                item.PurchaseIncTax = item.SalesIncTax;
                            }

                            if (item.SalesExcTax == 0 && item.SalesIncTax == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSalesTax" + item.DivId });
                                isError = true;
                            }

                            if (hasAttributeMappings)
                            {
                                item.VariationName = string.IsNullOrWhiteSpace(item.VariationName) ? FormatAttributeSummary(item.AttributeMappings) : item.VariationName;
                            }
                            else if (string.IsNullOrWhiteSpace(item.VariationName))
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divValue" + item.DivId });
                                isError = true;
                            }
                        }
                    }

                }
                else
                {
                    if (obj.ItemDetails == null || obj.ItemDetails.Count == 0)
                    {
                        errors.Add(new ClsError { Message = "Search Item first", Id = "divtags" });
                        isError = true;
                    }
                    else
                    {
                        foreach (var item in obj.ItemDetails)
                        {
                            if (item.Quantity == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divComboQuantity" + item.DivId });
                                isError = true;
                            }
                            if (item.SalesExcTax == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divComboSalesExcTax" });
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

                //long PrefixId = 0;
                //if (obj.SkuCode == "" || obj.SkuCode == null)
                //{
                //    PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixMasterId equals b.PrefixMasterId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType.ToLower() == "item"
                //                          && b.PrefixId == PrefixId
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.SkuCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                ClsItem oClsItem = new ClsItem()
                {
                    ItemType = obj.ItemType,
                    ExpiryPeriod = obj.ExpiryPeriod,
                    ExpiryPeriodType = obj.ExpiryPeriodType,
                    ItemCode = obj.ItemCode,
                    ItemName = obj.ItemName,
                    Description = obj.Description,
                    SkuCode = obj.SkuCode.Trim(),
                    //HsnCode = obj.HsnCode,
                    BarcodeType = obj.BarcodeType,
                    UnitId = obj.UnitId,
                    SecondaryUnitId = obj.SecondaryUnitId,
                    TertiaryUnitId = obj.TertiaryUnitId,
                    QuaternaryUnitId = obj.QuaternaryUnitId,
                    UToSValue = obj.UToSValue,
                    SToTValue = obj.SToTValue,
                    TToQValue = obj.TToQValue,
                    ItemId = obj.ItemId,
                    BrandId = obj.BrandId,
                    CategoryId = obj.CategoryId,
                    SubCategoryId = obj.SubCategoryId,
                    SubSubCategoryId = obj.SubSubCategoryId,
                    IsManageStock = obj.IsManageStock,
                    AlertQuantity = obj.AlertQuantity,
                    TaxId = obj.TaxId,
                    TaxType = obj.TaxType,
                    InterStateTaxId = obj.InterStateTaxId,
                    ProductType = obj.ProductType,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    //BranchIds = obj.BranchIds.TrimStart(','),
                    WarrantyId = obj.WarrantyId,
                    PriceAddedFor = obj.PriceAddedFor,
                    EnableImei = obj.EnableImei,
                    ItemCodeId = obj.ItemCodeId,
                    TaxPreferenceId = obj.TaxPreferenceId,
                    TaxExemptionId = obj.TaxExemptionId,
                    //PrefixId = PrefixId,
                    SaltId = obj.SaltId
                };

                if (obj.ProductImage != "" && obj.ProductImage != null && !obj.ProductImage.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/ProductImage/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionProductImage;

                    string base64 = obj.ProductImage.Replace(obj.ProductImage.Substring(0, obj.ProductImage.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProductImage");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ProductImage.Replace(obj.ProductImage.Substring(0, obj.ProductImage.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsItem.ProductImage = filepathPass;
                }

                if (obj.ProductBrochure != "" && obj.ProductBrochure != null && !obj.ProductBrochure.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/ProductBrochure/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionProductBrochure;

                    string base64 = obj.ProductBrochure.Replace(obj.ProductBrochure.Substring(0, obj.ProductBrochure.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProductBrochure");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ProductBrochure.Replace(obj.ProductBrochure.Substring(0, obj.ProductBrochure.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsItem.ProductBrochure = filepathPass;
                }

                oConnectionContext.DbClsItem.Add(oClsItem);
                oConnectionContext.SaveChanges();

                ////increase counter
                //string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                int counter = 10; int c = 0;
                foreach (var item in obj.ItemDetails)
                {
                    if (obj.ProductType == "Variable")
                    {
                        if (item.VariationDetailsId == 0)
                        {
                            ClsVariationDetails oClsVariationDetails = new ClsVariationDetails()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                VariationDetails = item.VariationName,
                                CompanyId = obj.CompanyId,
                                IsActive = true,
                                IsDeleted = false,
                                VariationId = item.VariationId
                            };
                            ConnectionContext ocon1 = new ConnectionContext();
                            ocon1.DbClsVariationDetails.Add(oClsVariationDetails);
                            ocon1.SaveChanges();
                            item.VariationDetailsId = oClsVariationDetails.VariationDetailsId;
                        }

                        //if (item.SKU == "" || item.SKU == null)
                        //{
                        //    //long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                        //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                        //                          join b in oConnectionContext.DbClsPrefixUserMap
                        //                           on a.PrefixMasterId equals b.PrefixMasterId
                        //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                        //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                        //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType.ToLower() == "item"
                        //                          && b.PrefixId == PrefixId
                        //                          select new
                        //                          {
                        //                              b.PrefixUserMapId,
                        //                              b.Prefix,
                        //                              b.NoOfDigits,
                        //                              b.Counter
                        //                          }).FirstOrDefault();
                        //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                        //    item.SKU = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                        //    //increase counter
                        //    q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                        //    oConnectionContext.Database.ExecuteSqlCommand(q);
                        //    //increase counter
                        //}
                    }
                    else
                    {
                        item.SKU = oClsItem.SkuCode;
                    }

                    ClsItemDetails oClsItemDetails = new ClsItemDetails()
                    {
                        ItemDetailsId = item.ItemDetailsId,
                        SKU = item.SKU.Trim(),
                        PurchaseExcTax = item.PurchaseExcTax,
                        PurchaseIncTax = item.PurchaseIncTax,
                        DefaultProfitMargin = item.DefaultProfitMargin,
                        SalesExcTax = item.SalesExcTax,
                        SalesIncTax = item.SalesIncTax,
                        ItemId = oClsItem.ItemId,
                        Quantity = item.Quantity,
                        TotalCost = item.TotalCost,
                        VariationId = item.VariationId,
                        VariationDetailsId = item.VariationDetailsId,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        ComboItemDetailsId = item.ComboItemDetailsId,
                        DefaultMrp = item.DefaultMrp,
                        SalesAccountId = item.SalesAccountId,
                        PurchaseAccountId = item.PurchaseAccountId,
                        InventoryAccountId = item.InventoryAccountId,
                        PriceAddedFor = item.PriceAddedFor,
                        //PrefixId = PrefixId
                    };

                    if (item.ProductImage != "" && item.ProductImage != null && !item.ProductImage.Contains("http"))
                    {
                        string filepathPass = "";

                        filepathPass = "/ExternalContents/Images/ProductImage/" + counter.ToString() + DateTime.Now.ToString("ddmmyyyyhhmmss") + item.FileExtensionProductImage;

                        string base64 = item.ProductImage.Replace(item.ProductImage.Substring(0, item.ProductImage.IndexOf(',') + 1), "");
                        byte[] imageCheque = Convert.FromBase64String(base64);
                        Stream strm = new MemoryStream(imageCheque);
                        var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                        var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProductImage");
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        //string base64 = item.ProductImage.Replace(item.ProductImage.Substring(0, item.ProductImage.IndexOf(',') + 1), "");
                        //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                        oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                        oClsItemDetails.ProductImage = filepathPass;
                    }

                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.DbClsItemDetails.Add(oClsItemDetails);
                    oConnectionContext.SaveChanges();
                    SaveItemDetailAttributes(item.AttributeMappings, oClsItemDetails.ItemDetailsId, obj.CompanyId, obj.AddedBy, CurrentDate);

                    counter++;

                    if (c == 0)
                    {
                        if (obj.ItemBranchMaps != null)
                        {
                            foreach (var innerBranch in obj.ItemBranchMaps)
                            {
                                ClsItemBranchMap oClsItemBranchMap = new ClsItemBranchMap()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    BranchId = innerBranch.BranchId,
                                    CompanyId = obj.CompanyId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ItemDetailsId = oClsItemDetails.ItemDetailsId,
                                    ItemId = oClsItem.ItemId,
                                    Quantity = 0,
                                    Rack = innerBranch.Rack,
                                    Row = innerBranch.Row,
                                    Position = innerBranch.Position
                                };
                                oConnectionContext.DbClsItemBranchMap.Add(oClsItemBranchMap);
                                oConnectionContext.SaveChanges();
                            }
                            if (obj.ProductType == "Combo")
                            {
                                c = c + 1;
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Items",
                    CompanyId = obj.CompanyId,
                    Description = "Item \"" + obj.SkuCode + "\" created",
                    Id = oClsItem.ItemId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Item created successfully",
                    Data = new
                    {
                        Item = oClsItem
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateItem(ClsItemVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                long PrefixUserMapId = 0;

                if (obj.ItemName == null || obj.ItemName == "")
                {
                    if (obj.ItemType.ToLower() == "product")
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divItemName" });
                    }
                    else
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divItemName" });
                    }
                    isError = true;
                }

                if (obj.ItemType == "Product")
                {
                    if (obj.UnitId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divUnit" });
                        isError = true;
                    }
                }

                if (obj.CategoryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCategory" });
                    isError = true;
                }

                if (obj.TaxType == "" || obj.TaxType == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTaxType" });
                    isError = true;
                }

                if (obj.ProductType == "" || obj.ProductType == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divProductType" });
                    isError = true;
                }

                if (obj.ItemBranchMaps == null || obj.ItemBranchMaps.Count == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranches" });
                    isError = true;
                }

                if (obj.TaxPreference == "Taxable")
                {
                    var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
                    { CountryId = a.CountryId, IsBusinessRegistered = a.IsBusinessRegistered, BusinessRegistrationType = a.BusinessRegistrationType }).FirstOrDefault();

                    if (BusinessSetting.CountryId == 2)
                    {
                        if (BusinessSetting.IsBusinessRegistered == 1 && BusinessSetting.BusinessRegistrationType == "Regular")
                        {
                            if (obj.TaxId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divTax" });
                                isError = true;
                            }

                            if (obj.InterStateTaxId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divInterStateTax" });
                                isError = true;
                            }
                        }
                    }
                    else
                    {
                        if (obj.TaxId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divTax" });
                            isError = true;
                        }
                    }
                }
                else if (obj.TaxPreference == "Non-Taxable")
                {
                    if (obj.TaxExemptionId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divTaxExemption" });
                        isError = true;
                    }
                }

                int skuCount = 1;
                ClsMenuVm oClsMenuVm = new ClsMenuVm();
                oClsMenuVm.AddedBy = obj.AddedBy;
                oClsMenuVm.CompanyId = obj.CompanyId;

                bool IsPurchaseAddon = oCommonController.PlanAddons(oClsMenuVm).Where(a => a.Type == "Purchase").Count() == 0 ? false : true;
                if (obj.ProductType == "Single")
                {
                    if (IsPurchaseAddon == true)
                    {
                        if (obj.ItemType == "Product")
                        {
                            if (obj.ItemDetails[0].PurchaseExcTax == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseExcTax" });
                                isError = true;
                            }
                        }

                        //if (obj.ItemDetails[0].DefaultProfitMargin == 0)
                        //{
                        //    errors.Add(new ClsError { Message = "This field is required", Id = "divDefaultProfitMargin" });
                        //    isError = true;
                        //}
                    }
                    else
                    {
                        obj.ItemDetails[0].PurchaseExcTax = obj.ItemDetails[0].SalesExcTax;
                        obj.ItemDetails[0].PurchaseIncTax = obj.ItemDetails[0].SalesIncTax;
                    }
                    if (obj.ItemDetails[0].SalesExcTax == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSalesExcTax" });
                        isError = true;
                    }
                }
                else if (obj.ProductType == "Variable")
                {
                    if (obj.ItemDetails == null || obj.ItemDetails.Count == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divVariationValues" });
                        errors.Add(new ClsError { Message = "This field is required", Id = "divAttributeSelections" });
                        isError = true;
                    }
                    else
                    {
                        foreach (var detail in obj.ItemDetails)
                        {
                            NormalizeAttributeMappings(detail);
                        }
                        foreach (var item in obj.ItemDetails)
                        {
                            int innerSkuCount = 1;
                            foreach (var inner in obj.ItemDetails)
                            {
                                if (item.SKU != "" && item.SKU != null)
                                {
                                    if (item.SKU.ToLower() == inner.SKU.ToLower() && skuCount != innerSkuCount)
                                    {
                                        errors.Add(new ClsError { Message = "Duplicate SKU exists" + item.DivId, Id = "divSku" + item.DivId });
                                        isError = true;
                                    }
                                }

                                innerSkuCount++;
                            }
                            skuCount++;
                        }
                    }

                    foreach (var item in obj.ItemDetails)
                    {
                        if (oConnectionContext.DbClsItem.Where(a => a.SkuCode.ToLower() == item.SKU.ToLower() && a.ItemId != obj.ItemId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate SKU exists", Id = "divSku" + item.DivId });
                            isError = true;
                        }
                        if (oConnectionContext.DbClsItemDetails.Where(a => a.SKU.ToLower() == item.SKU.ToLower() && a.ItemDetailsId != item.ItemDetailsId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate SKU exists", Id = "divSku" + item.DivId });
                            isError = true;
                        }
                        if (item.SKU == "" || item.SKU == null)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divSku" + item.DivId });
                            isError = true;
                        }
                        bool hasAttributeMappings = item.AttributeMappings != null && item.AttributeMappings.Count > 0;
                        var variationErrorId = hasAttributeMappings ? "divAttributeSelections" : "divVariation" + item.DivId;
                        if (!hasAttributeMappings && item.VariationId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = variationErrorId });
                            isError = true;
                        }

                        if (IsPurchaseAddon == true)
                        {
                            if (obj.ItemType == "Product")
                            {
                                if (item.PurchaseExcTax == 0)
                                {
                                    errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseExcTax" + item.DivId });
                                    isError = true;
                                }
                                //if (item.DefaultProfitMargin == 0)
                                //{
                                //    errors.Add(new ClsError { Message = "This field is required", Id = "divDefaultProfitMargin" + item.DivId });
                                //    isError = true;
                                //}
                            }
                        }
                        else
                        {
                            item.PurchaseExcTax = item.SalesExcTax;
                            item.PurchaseIncTax = item.SalesIncTax;
                        }

                        if (item.SalesExcTax == 0 && item.SalesIncTax == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divSalesTax" + item.DivId });
                            isError = true;
                        }

                        if (hasAttributeMappings)
                        {
                            item.VariationName = string.IsNullOrWhiteSpace(item.VariationName) ? FormatAttributeSummary(item.AttributeMappings) : item.VariationName;
                        }
                        else if (string.IsNullOrWhiteSpace(item.VariationName))
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divValue" + item.DivId });
                            isError = true;
                        }
                    }
                }
                else
                {
                    if (obj.ItemDetails == null || obj.ItemDetails.Count == 0)
                    {
                        errors.Add(new ClsError { Message = "Search Item first", Id = "divtags" });
                        isError = true;
                    }
                    else
                    {
                        foreach (var item in obj.ItemDetails)
                        {
                            if (item.Quantity == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divComboQuantity" + item.DivId });
                                isError = true;
                            }
                            if (item.SalesExcTax == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divComboSalesExcTax" });
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

                //obj.BranchIds = "";
                //if (obj.Branchs != null)
                //{
                //    foreach (var item in obj.Branchs)
                //    {
                //        obj.BranchIds = obj.BranchIds + "," + item;
                //    }
                //}
                ClsItem oClsItem = new ClsItem()
                {
                    WarrantyId = obj.WarrantyId,
                    //ItemType=obj.ItemType,
                    //HsnCode = obj.HsnCode,
                    ExpiryPeriodType = obj.ExpiryPeriodType,
                    ExpiryPeriod = obj.ExpiryPeriod,
                    ItemName = obj.ItemName,
                    Description = obj.Description,
                    BarcodeType = obj.BarcodeType,
                    //UnitId = obj.UnitId,
                    //SecondaryUnitId = obj.SecondaryUnitId,
                    //TertiaryUnitId = obj.TertiaryUnitId,
                    //QuaternaryUnitId = obj.QuaternaryUnitId,
                    //UToSValue = obj.UToSValue,
                    //SToTValue = obj.SToTValue,
                    //TToQValue = obj.TToQValue,
                    ItemId = obj.ItemId,
                    BrandId = obj.BrandId,
                    CategoryId = obj.CategoryId,
                    SubCategoryId = obj.SubCategoryId,
                    SubSubCategoryId = obj.SubSubCategoryId,
                    IsManageStock = obj.IsManageStock,
                    AlertQuantity = obj.AlertQuantity,
                    TaxId = obj.TaxId,
                    InterStateTaxId = obj.InterStateTaxId,
                    TaxType = obj.TaxType,
                    ProductType = obj.ProductType,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    PriceAddedFor = obj.PriceAddedFor,
                    EnableImei = obj.EnableImei,
                    //BranchIds = obj.BranchIds.TrimStart(',')
                    ItemCodeId = obj.ItemCodeId,
                    TaxPreferenceId = obj.TaxPreferenceId,
                    TaxExemptionId = obj.TaxExemptionId,
                    SaltId = obj.SaltId
                };

                string pic1 = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => a.ProductImage).FirstOrDefault();
                if (obj.ProductImage != "" && obj.ProductImage != null && !obj.ProductImage.Contains("http"))
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/ProductImage/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionProductImage;

                    string base64 = obj.ProductImage.Replace(obj.ProductImage.Substring(0, obj.ProductImage.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProductImage");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ProductImage.Replace(obj.ProductImage.Substring(0, obj.ProductImage.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsItem.ProductImage = filepathPass;
                }
                else
                {
                    oClsItem.ProductImage = pic1;
                }

                pic1 = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => a.ProductBrochure).FirstOrDefault();
                if (obj.ProductBrochure != "" && obj.ProductBrochure != null && !obj.ProductBrochure.Contains("http"))
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/ProductBrochure/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionProductBrochure;

                    string base64 = obj.ProductBrochure.Replace(obj.ProductBrochure.Substring(0, obj.ProductBrochure.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProductBrochure");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ProductBrochure.Replace(obj.ProductBrochure.Substring(0, obj.ProductBrochure.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsItem.ProductBrochure = filepathPass;
                }
                else
                {
                    oClsItem.ProductBrochure = pic1;
                }

                oConnectionContext.DbClsItem.Attach(oClsItem);
                //oConnectionContext.Entry(oClsItem).Property(x => x.ItemType).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.WarrantyId).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.ItemName).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.Description).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.BarcodeType).IsModified = true;
                //oConnectionContext.Entry(oClsItem).Property(x => x.UnitId).IsModified = true;
                //oConnectionContext.Entry(oClsItem).Property(x => x.SecondaryUnitId).IsModified = true;
                //oConnectionContext.Entry(oClsItem).Property(x => x.TertiaryUnitId).IsModified = true;
                //oConnectionContext.Entry(oClsItem).Property(x => x.QuaternaryUnitId).IsModified = true;
                //oConnectionContext.Entry(oClsItem).Property(x => x.UToSValue).IsModified = true;
                //oConnectionContext.Entry(oClsItem).Property(x => x.SToTValue).IsModified = true;
                //oConnectionContext.Entry(oClsItem).Property(x => x.TToQValue).IsModified = true;
                //oConnectionContext.Entry(oClsItem).Property(x => x.BranchIds).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.BrandId).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.CategoryId).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.SubCategoryId).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.SubSubCategoryId).IsModified = true;
                //oConnectionContext.Entry(oClsItem).Property(x => x.IsManageStock).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.AlertQuantity).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.InterStateTaxId).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.TaxType).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.ProductType).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.ExpiryPeriod).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.ExpiryPeriodType).IsModified = true;
                //oConnectionContext.Entry(oClsItem).Property(x => x.HsnCode).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.PriceAddedFor).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.ProductBrochure).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.ProductImage).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.EnableImei).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.ItemCodeId).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.TaxPreferenceId).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.TaxExemptionId).IsModified = true;
                oConnectionContext.Entry(oClsItem).Property(x => x.SaltId).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "update \"tblItemBranchMap\" set \"IsActive\"= False where \"ItemId\"=" + obj.ItemId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                int counter = 10; int c = 0;
                foreach (var item in obj.ItemDetails)
                {
                    if (item.IsDeleted == true)
                    {
                        string query1 = "update \"tblitemdetails\" set \"IsDeleted\"=True where \"ItemDetailsId\"=" + item.ItemDetailsId;
                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                        
                        // Also mark attribute mappings as deleted
                        var mappingsToDelete = oConnectionContext.DbClsItemDetailsVariationMap
                            .Where(m => m.ItemDetailsId == item.ItemDetailsId && m.IsDeleted == false)
                            .ToList();
                        foreach (var mapping in mappingsToDelete)
                        {
                            mapping.IsDeleted = true;
                            mapping.ModifiedBy = obj.AddedBy;
                            mapping.ModifiedOn = CurrentDate;
                            oConnectionContext.Entry(mapping).Property(x => x.IsDeleted).IsModified = true;
                            oConnectionContext.Entry(mapping).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(mapping).Property(x => x.ModifiedOn).IsModified = true;
                        }
                        if (mappingsToDelete.Count > 0)
                        {
                            oConnectionContext.SaveChanges();
                        }
                    }
                    else
                    {
                        if (obj.ProductType == "Variable")
                        {
                            if (item.VariationDetailsId == 0)
                            {
                                ClsVariationDetails oClsVariationDetails = new ClsVariationDetails()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    VariationDetails = item.VariationName,
                                    CompanyId = obj.CompanyId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    VariationId = item.VariationId
                                };
                                //ConnectionContext ocon = new ConnectionContext();
                                oConnectionContext.DbClsVariationDetails.Add(oClsVariationDetails);
                                oConnectionContext.SaveChanges();
                                item.VariationDetailsId = oClsVariationDetails.VariationDetailsId;
                            }

                            //if (item.SKU == "" || item.SKU == null)
                            //{
                            //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                            //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                            //                          join b in oConnectionContext.DbClsPrefixUserMap
                            //                           on a.PrefixMasterId equals b.PrefixMasterId
                            //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                            //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                            //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType.ToLower() == "item"
                            //                          && b.PrefixId == PrefixId
                            //                          select new
                            //                          {
                            //                              b.PrefixUserMapId,
                            //                              b.Prefix,
                            //                              b.NoOfDigits,
                            //                              b.Counter
                            //                          }).FirstOrDefault();
                            //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                            //    item.SKU = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                            //    //increase counter
                            //    string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                            //    oConnectionContext.Database.ExecuteSqlCommand(q);
                            //    //increase counter
                            //}
                        }
                        else
                        {
                            if (item.SKU == "" || item.SKU == null)
                            {
                                item.SKU = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => a.SkuCode).FirstOrDefault();
                            }
                        }

                        if (item.ItemDetailsId == 0)
                        {
                            ClsItemDetails oClsItemDetails = new ClsItemDetails()
                            {
                                ItemDetailsId = item.ItemDetailsId,
                                SKU = item.SKU.Trim(),
                                PurchaseExcTax = item.PurchaseExcTax,
                                PurchaseIncTax = item.PurchaseIncTax,
                                DefaultProfitMargin = item.DefaultProfitMargin,
                                SalesExcTax = item.SalesExcTax,
                                SalesIncTax = item.SalesIncTax,
                                ItemId = oClsItem.ItemId,
                                Quantity = item.Quantity,
                                TotalCost = item.TotalCost,
                                VariationId = item.VariationId,
                                VariationDetailsId = item.VariationDetailsId,
                                IsActive = true,
                                IsDeleted = false,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                CompanyId = obj.CompanyId,
                                ComboItemDetailsId = item.ComboItemDetailsId,
                                DefaultMrp = item.DefaultMrp,
                                SalesAccountId = item.SalesAccountId,
                                PurchaseAccountId = item.PurchaseAccountId,
                                InventoryAccountId = item.InventoryAccountId,
                                PriceAddedFor = item.PriceAddedFor
                            };

                            if (item.ProductImage != "" && item.ProductImage != null && !item.ProductImage.Contains("http"))
                            {
                                string filepathPass = "";

                                filepathPass = "/ExternalContents/Images/ProductImage/" + counter.ToString() + DateTime.Now.ToString("ddmmyyyyhhmmss") + item.FileExtensionProductImage;

                                string base64 = item.ProductImage.Replace(item.ProductImage.Substring(0, item.ProductImage.IndexOf(',') + 1), "");
                                byte[] imageCheque = Convert.FromBase64String(base64);
                                Stream strm = new MemoryStream(imageCheque);
                                var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                                var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProductImage");
                                if (!Directory.Exists(folder))
                                {
                                    Directory.CreateDirectory(folder);
                                }

                                //string base64 = item.ProductImage.Replace(item.ProductImage.Substring(0, item.ProductImage.IndexOf(',') + 1), "");
                                //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                                oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                                oClsItemDetails.ProductImage = filepathPass;
                            }

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsItemDetails.Add(oClsItemDetails);
                            oConnectionContext.SaveChanges();

                            item.ItemDetailsId = oClsItemDetails.ItemDetailsId;
                            SaveItemDetailAttributes(item.AttributeMappings, oClsItemDetails.ItemDetailsId, obj.CompanyId, obj.AddedBy, CurrentDate);
                        }
                        else
                        {
                            ClsItemDetails oClsItemDetails = new ClsItemDetails()
                            {
                                ItemDetailsId = item.ItemDetailsId,
                                SKU = item.SKU.Trim(),
                                PurchaseExcTax = item.PurchaseExcTax,
                                PurchaseIncTax = item.PurchaseIncTax,
                                DefaultProfitMargin = item.DefaultProfitMargin,
                                SalesExcTax = item.SalesExcTax,
                                SalesIncTax = item.SalesIncTax,
                                Quantity = item.Quantity,
                                TotalCost = item.TotalCost,
                                VariationId = item.VariationId,
                                VariationDetailsId = item.VariationDetailsId,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                                ComboItemDetailsId = item.ComboItemDetailsId,
                                DefaultMrp = item.DefaultMrp,
                                SalesAccountId = item.SalesAccountId,
                                PurchaseAccountId = item.PurchaseAccountId,
                                InventoryAccountId = item.InventoryAccountId,
                                PriceAddedFor = item.PriceAddedFor
                            };

                            string picPath = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == item.ItemDetailsId).Select(a => a.ProductImage).FirstOrDefault();
                            if (item.ProductImage != "" && item.ProductImage != null && !item.ProductImage.ToLower().Contains("http") && !item.ProductImage.ToLower().Contains("content"))
                            {
                                string filepathPass = "";

                                filepathPass = "/ExternalContents/Images/ProductImage/" + counter.ToString() + DateTime.Now.ToString("ddmmyyyyhhmmss") + item.FileExtensionProductImage;

                                string base64 = item.ProductImage.Replace(item.ProductImage.Substring(0, item.ProductImage.IndexOf(',') + 1), "");
                                byte[] imageCheque = Convert.FromBase64String(base64);
                                Stream strm = new MemoryStream(imageCheque);
                                var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                                var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProductImage");
                                if (!Directory.Exists(folder))
                                {
                                    Directory.CreateDirectory(folder);
                                }

                                //string base64 = item.ProductImage.Replace(item.ProductImage.Substring(0, item.ProductImage.IndexOf(',') + 1), "");
                                //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                                oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                                oClsItemDetails.ProductImage = filepathPass;
                            }
                            else
                            {
                                oClsItemDetails.ProductImage = picPath;
                            }

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsItemDetails.Attach(oClsItemDetails);
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.SKU).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.PurchaseExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.PurchaseIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.DefaultProfitMargin).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.SalesExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.SalesIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.Quantity).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.TotalCost).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.VariationId).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.VariationDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.ProductImage).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.ComboItemDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.DefaultMrp).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.SalesAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.PurchaseAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.InventoryAccountId).IsModified = true;
                            oConnectionContext.Entry(oClsItemDetails).Property(x => x.PriceAddedFor).IsModified = true;
                            oConnectionContext.SaveChanges();
                            
                            // Update attribute mappings
                            UpdateItemDetailAttributes(item.AttributeMappings, item.ItemDetailsId, obj.CompanyId, obj.AddedBy, CurrentDate);
                        }
                        counter++;
                        if (c == 0)
                        {
                            if (obj.ItemBranchMaps != null)
                            {
                                foreach (var innerBranch in obj.ItemBranchMaps)
                                {
                                    long _BranchId = innerBranch.BranchId;
                                    long ItemBranchMapId = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == _BranchId && a.ItemId == obj.ItemId &&
                                    a.ItemDetailsId == item.ItemDetailsId).Select(a => a.ItemBranchMapId).FirstOrDefault();

                                    if (ItemBranchMapId == 0)
                                    {
                                        ClsItemBranchMap oClsItemBranchMap = new ClsItemBranchMap()
                                        {
                                            AddedBy = obj.AddedBy,
                                            AddedOn = CurrentDate,
                                            BranchId = innerBranch.BranchId,
                                            CompanyId = obj.CompanyId,
                                            IsActive = true,
                                            IsDeleted = false,
                                            ItemDetailsId = item.ItemDetailsId,
                                            ItemId = oClsItem.ItemId,
                                            //Quantity = 0,
                                            Rack = innerBranch.Rack,
                                            Row = innerBranch.Row,
                                            Position = innerBranch.Position
                                        };
                                        oConnectionContext.DbClsItemBranchMap.Add(oClsItemBranchMap);
                                        oConnectionContext.SaveChanges();
                                    }
                                    else
                                    {
                                        ClsItemBranchMap oClsItemBranchMap = new ClsItemBranchMap()
                                        {
                                            ItemBranchMapId = ItemBranchMapId,
                                            ModifiedBy = obj.AddedBy,
                                            ModifiedOn = CurrentDate,
                                            BranchId = innerBranch.BranchId,
                                            IsActive = true,
                                            ItemDetailsId = item.ItemDetailsId,
                                            ItemId = oClsItem.ItemId,
                                            Quantity = 0,
                                            Rack = innerBranch.Rack,
                                            Row = innerBranch.Row,
                                            Position = innerBranch.Position
                                        };
                                        oConnectionContext.DbClsItemBranchMap.Attach(oClsItemBranchMap);
                                        oConnectionContext.Entry(oClsItemBranchMap).Property(x => x.ModifiedBy).IsModified = true;
                                        oConnectionContext.Entry(oClsItemBranchMap).Property(x => x.ModifiedOn).IsModified = true;
                                        oConnectionContext.Entry(oClsItemBranchMap).Property(x => x.BranchId).IsModified = true;
                                        oConnectionContext.Entry(oClsItemBranchMap).Property(x => x.IsActive).IsModified = true;
                                        oConnectionContext.Entry(oClsItemBranchMap).Property(x => x.ItemDetailsId).IsModified = true;
                                        oConnectionContext.Entry(oClsItemBranchMap).Property(x => x.ItemId).IsModified = true;
                                        //oConnectionContext.Entry(oClsItemBranchMap).Property(x => x.Quantity).IsModified = true;
                                        oConnectionContext.Entry(oClsItemBranchMap).Property(x => x.Rack).IsModified = true;
                                        oConnectionContext.Entry(oClsItemBranchMap).Property(x => x.Row).IsModified = true;
                                        oConnectionContext.Entry(oClsItemBranchMap).Property(x => x.Position).IsModified = true;
                                        oConnectionContext.SaveChanges();
                                    }
                                }
                                if (obj.ProductType == "Combo")
                                {
                                    c = c + 1;
                                }
                            }
                        }
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Items",
                    CompanyId = obj.CompanyId,
                    Description = "Item \"" + oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => a.SkuCode).FirstOrDefault() + "\" updated",
                    Id = oClsItem.ItemId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Item updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemActiveInactive(ClsItemVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                //if (obj.IsActive == true)
                //{
                //    int TotalItem = oCommonController.fetchPlanQuantity(obj.CompanyId, "Item");
                //    int TotalItemUsed = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false).Count();
                //    if (TotalItemUsed >= TotalItem)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "Item quota already used. Please upgrade addons from My Plan Menu",
                //            Data = new
                //            {
                //            }
                //        };
                //        return await Task.FromResult(Ok(data));
                //    }
                //}

                ClsItem oClsRole = new ClsItem()
                {
                    ItemId = obj.ItemId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsItem.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.ItemId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Items",
                    CompanyId = obj.CompanyId,
                    Description = "Item \"" + oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => a.SkuCode).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.ItemId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Item " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemDelete(ClsItemVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int ItemCount = 0;
                var ItemDetails = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == obj.ItemId).Select(a => new
                {
                    a.ItemDetailsId
                }).ToList();

                foreach (var item in ItemDetails)
                {
                    ItemCount = ItemCount + (from a in oConnectionContext.DbClsItem
                                             join b in oConnectionContext.DbClsItemDetails
                                                on a.ItemId equals b.ItemId
                                             where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                            && a.ProductType == "Combo" && b.ComboItemDetailsId == item.ItemDetailsId
                                             select b.ComboItemDetailsId).Count();


                }

                int StockAdjustmentCount = (from a in oConnectionContext.DbClsStockAdjustment
                                            join b in oConnectionContext.DbClsStockAdjustmentDetails
                                               on a.StockAdjustmentId equals b.StockAdjustmentId
                                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.ItemId == obj.ItemId
                                            select a.StockAdjustmentId).Count();

                int StockTransferCount = (from a in oConnectionContext.DbClsStockTransfer
                                          join b in oConnectionContext.DbClsStockTransferDetails
                                             on a.StockTransferId equals b.StockTransferId
                                          where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.ItemId == obj.ItemId
                                          select a.StockTransferId).Count();

                int PurchaseQuotationCount = (from a in oConnectionContext.DbClsPurchaseQuotation
                                              join b in oConnectionContext.DbClsPurchaseQuotationDetails
                                           on a.PurchaseQuotationId equals b.PurchaseQuotationId
                                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.ItemId == obj.ItemId
                                              select a.PurchaseQuotationId).Count();

                int PurchaseOrderCount = (from a in oConnectionContext.DbClsPurchaseOrder
                                          join b in oConnectionContext.DbClsPurchaseOrderDetails
                                           on a.PurchaseOrderId equals b.PurchaseOrderId
                                          where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.ItemId == obj.ItemId
                                          select a.PurchaseOrderId).Count();

                int PurchaseCount = (from a in oConnectionContext.DbClsPurchase
                                     join b in oConnectionContext.DbClsPurchaseDetails
                                   on a.PurchaseId equals b.PurchaseId
                                     where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                   && b.ItemId == obj.ItemId
                                     select a.PurchaseId).Count();

                int SalesQuotationCount = (from a in oConnectionContext.DbClsSalesQuotation
                                           join b in oConnectionContext.DbClsSalesQuotationDetails
                                           on a.SalesQuotationId equals b.SalesQuotationId
                                           where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.ItemId == obj.ItemId
                                           select a.SalesQuotationId).Count();

                int SalesOrderCount = (from a in oConnectionContext.DbClsSalesOrder
                                       join b in oConnectionContext.DbClsSalesOrderDetails
                                           on a.SalesOrderId equals b.SalesOrderId
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.ItemId == obj.ItemId
                                       select a.SalesOrderId).Count();

                int SalesProformaCount = (from a in oConnectionContext.DbClsSalesProforma
                                          join b in oConnectionContext.DbClsSalesProformaDetails
                                           on a.SalesProformaId equals b.SalesProformaId
                                          where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.ItemId == obj.ItemId
                                          select a.SalesProformaId).Count();

                int DeliveryChallanCount = (from a in oConnectionContext.DbClsDeliveryChallan
                                            join b in oConnectionContext.DbClsDeliveryChallanDetails
                                           on a.DeliveryChallanId equals b.DeliveryChallanId
                                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                           && b.ItemId == obj.ItemId
                                            select a.DeliveryChallanId).Count();

                int SalesCount = (from a in oConnectionContext.DbClsSales
                                  join b in oConnectionContext.DbClsSalesDetails
                                   on a.SalesId equals b.SalesId
                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                   && b.ItemId == obj.ItemId
                                  select a.SalesId).Count();

                if (ItemCount > 0 || StockAdjustmentCount > 0 || StockTransferCount > 0 || PurchaseQuotationCount > 0
                    || PurchaseOrderCount > 0 || PurchaseCount > 0 || SalesQuotationCount > 0 || SalesOrderCount > 0 || SalesProformaCount > 0
                    || DeliveryChallanCount > 0 || SalesCount > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete as it is already in use",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsItem oClsRole = new ClsItem()
                {
                    ItemId = obj.ItemId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsItem.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.ItemId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Items",
                    CompanyId = obj.CompanyId,
                    Description = "Item \"" + oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => a.SkuCode).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.ItemId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Item deleted successfully",
                    Data = new
                    {
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveItems(ClsItemVm obj)
        {
            //var det = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
            //{
            //    ItemId = a.ItemId,
            //    a.ItemCode,
            //    ItemName = a.ItemName,
            //}).OrderBy(a => a.ItemName).ToList();

            List<ClsItemDetailsVm> det;
            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsItemBranchMap
                       join b in oConnectionContext.DbClsItemDetails
                       on a.ItemDetailsId equals b.ItemDetailsId
                       join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                       where a.CompanyId == obj.CompanyId &&
                       //a.BranchId == obj.BranchId 
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && c.ProductType != "Combo"
                       && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                       select new ClsItemDetailsVm
                       {
                           IsManageStock = c.IsManageStock,
                           ItemId = b.ItemId,
                           ItemDetailsId = a.ItemDetailsId,
                           ItemName = c.ItemName,
                           SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                       }).Union(from a in oConnectionContext.DbClsItemBranchMap
                                join b in oConnectionContext.DbClsItemDetails
                                on a.ItemDetailsId equals b.ItemDetailsId
                                join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId &&
                                //a.BranchId == obj.BranchId 
                                oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                 l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                && c.ProductType == "Combo"
                                && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                                select new ClsItemDetailsVm
                                {
                                    IsManageStock = c.IsManageStock,
                                    ItemId = b.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ItemName = c.ItemName,
                                    SKU = c.SkuCode,
                                    VariationName = "",
                                }).OrderBy(a => a.ItemName).ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsItemBranchMap
                       join b in oConnectionContext.DbClsItemDetails
                       on a.ItemDetailsId equals b.ItemDetailsId
                       join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                       where a.CompanyId == obj.CompanyId &&
                       a.BranchId == obj.BranchId
                       && c.ProductType != "Combo"
                       && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                       select new ClsItemDetailsVm
                       {
                           IsManageStock = c.IsManageStock,
                           ItemId = b.ItemId,
                           ItemDetailsId = a.ItemDetailsId,
                           ItemName = c.ItemName,
                           SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                       }).Union(from a in oConnectionContext.DbClsItemBranchMap
                                join b in oConnectionContext.DbClsItemDetails
                                on a.ItemDetailsId equals b.ItemDetailsId
                                join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId &&
                                a.BranchId == obj.BranchId
                                && c.ProductType == "Combo"
                                && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                                select new ClsItemDetailsVm
                                {
                                    IsManageStock = c.IsManageStock,
                                    ItemId = b.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    ItemName = c.ItemName,
                                    SKU = c.SkuCode,
                                    VariationName = "",
                                }).OrderBy(a => a.ItemName).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Items = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemAutocomplete(ClsItemVm obj)
        {
            dynamic det = null;
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            if (obj.MenuType.ToLower() != "combo")
            {
                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = obj.MenuType.ToLower() == "stock transfer" ? "divFromBranch" : "divBranch" });
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

            if (obj.MenuType.ToLower() == "purchase")
            {
                det = (from c in oConnectionContext.DbClsItemBranchMap
                       join a in oConnectionContext.DbClsItemDetails
                      on c.ItemDetailsId equals a.ItemDetailsId
                       join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                       where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                      (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                      && b.IsActive == true && b.IsDeleted == false && b.ProductType.ToLower() == "single" // && b.IsManageStock == true
                       //&& c.Quantity > 0
                       select b.ItemName + " ~ " + b.SkuCode).Union(from c in oConnectionContext.DbClsItemBranchMap
                                                                    join a in oConnectionContext.DbClsItemDetails
                                                                   on c.ItemDetailsId equals a.ItemDetailsId
                                                                    join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                                                    where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                                                   (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                                                   && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false
                                                                    //&& b.IsManageStock == true
                                                                    //&& c.Quantity > 0
                                                                    select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                                                       ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList().Take(10);
            }
            else if (obj.MenuType.ToLower() == "sale")
            {
                var ComboStock = (from c in oConnectionContext.DbClsItemBranchMap
                                  join a in oConnectionContext.DbClsItemDetails
                                  on c.ItemDetailsId equals a.ItemDetailsId
                                  join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                  where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                  (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                  && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false && b.ProductType.ToLower() == "combo"
                                  select new { 
                                  b.ItemId,
                                  b.ItemName,
                                  b.SkuCode
                                  }).ToList();

                List<string> ComboStock1 = new List<string>();

                if (ComboStock != null && ComboStock.Count > 0)
                {
                    foreach (var _comboStock in ComboStock)
                    {
                        var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == _comboStock.ItemId).Select(a => new
                        {
                            ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                            //ComboItemDetailsId = a.ComboItemDetailsId,
                            IsManageStock = false,
                            Quantity = a.Quantity,
                            //a.PriceAddedFor,
                            QtyForSell = 0,
                            AvailableQuantity = oConnectionContext.DbClsItemBranchMap.Where(b => b.ItemDetailsId == a.ComboItemDetailsId && b.BranchId == obj.BranchId && b.IsActive == true && b.IsDeleted == false).Select(b => b.Quantity).FirstOrDefault(),
                        }).ToList();

                        var itemsWithQty = combo.Select(item => new
                        {
                            IsManageStock = oConnectionContext.DbClsItem.Where(b => b.ItemId == item.ItemId).Select(b => b.IsManageStock).FirstOrDefault(),
                            item.Quantity,
                            item.AvailableQuantity,
                            QtyForSell = item.AvailableQuantity != 0 ? (int)(item.AvailableQuantity / item.Quantity) : 0
                        }).ToList();

                        //var minQty = itemsWithQty.Where(a=>a.IsManageStock == true).DefaultIfEmpty().Min(item => item.QtyForSell);

                        var minQty = itemsWithQty
                           .Where(a => a != null && a.IsManageStock == true)
                           .Select(item => item.QtyForSell)
                           .DefaultIfEmpty()
                           .Min();

                        bool IsManageStock = itemsWithQty.Where(a => a.IsManageStock == true).Count() > 0 ? true : false;

                        if(IsManageStock == true)
                        {
                            if (minQty > 0)
                            {
                                ComboStock1.Add(_comboStock.ItemName + " ~ " + _comboStock.SkuCode);
                            }                            
                        }
                        else
                        {
                            ComboStock1.Add(_comboStock.ItemName + " ~ " + _comboStock.SkuCode);
                        }
                    }
                }

                var StockNotManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                       join a in oConnectionContext.DbClsItemDetails
                                       on c.ItemDetailsId equals a.ItemDetailsId
                                       join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                       where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                       (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                       && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false && b.ProductType.ToLower() == "single"
                                       select b.ItemName + " ~ " + b.SkuCode).ToList();

                var variable_StockNotManaged = (
                                 from c in oConnectionContext.DbClsItemBranchMap
                                 join a in oConnectionContext.DbClsItemDetails
                                 on c.ItemDetailsId equals a.ItemDetailsId
                                 join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                 where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                 (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                 && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false
                                 select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                                 ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList();

                var StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                    join a in oConnectionContext.DbClsItemDetails
                                   on c.ItemDetailsId equals a.ItemDetailsId
                                    join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                    where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                   (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                   && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true && b.ProductType.ToLower() == "single"
                                   && c.Quantity > 0
                                    select b.ItemName + " ~ " + b.SkuCode).ToList();

                var variable_StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                             join a in oConnectionContext.DbClsItemDetails
                                            on c.ItemDetailsId equals a.ItemDetailsId
                                             join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                             where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                            (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                            && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                                            && c.Quantity > 0
                                             select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                                ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList();

                det = ComboStock1.Union(StockNotManaged).Union(variable_StockNotManaged).Union(StockManaged).Union(variable_StockManaged).Distinct().ToList().Take(10);
            }
            else if (obj.MenuType.ToLower() == "stock transfer")
            {
                var StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                    join a in oConnectionContext.DbClsItemDetails
                                   on c.ItemDetailsId equals a.ItemDetailsId
                                    join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                    where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                   (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                   && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                                   && c.Quantity > 0 && b.ProductType.ToLower() == "single"
                                    select b.ItemName + " ~ " + b.SkuCode).ToList();

                var variable_StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                             join a in oConnectionContext.DbClsItemDetails
                                            on c.ItemDetailsId equals a.ItemDetailsId
                                             join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                             where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                            (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                            && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                                            && c.Quantity > 0
                                             select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                                ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList();

                det = StockManaged.Union(variable_StockManaged).Distinct().ToList().Take(10);
            }
            else if (obj.MenuType.ToLower() == "sales return")
            {
                var StockNotManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                       join a in oConnectionContext.DbClsItemDetails
                                       on c.ItemDetailsId equals a.ItemDetailsId
                                       join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                       where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                       (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                       && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false
                                       select b.ItemName + " ~ " + b.SkuCode).ToList();

                var variable_StockNotManaged = (
                                 from c in oConnectionContext.DbClsItemBranchMap
                                 join a in oConnectionContext.DbClsItemDetails
                                 on c.ItemDetailsId equals a.ItemDetailsId
                                 join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                 where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                 (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                 && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false
                                 select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                                 ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList();

                var StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                    join a in oConnectionContext.DbClsItemDetails
                                   on c.ItemDetailsId equals a.ItemDetailsId
                                    join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                    where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                   (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                   && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                                    //&& c.Quantity > 0
                                    select b.ItemName + " ~ " + b.SkuCode).ToList();

                var variable_StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                             join a in oConnectionContext.DbClsItemDetails
                                            on c.ItemDetailsId equals a.ItemDetailsId
                                             join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                             where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                            (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                            && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                                             //&& c.Quantity > 0
                                             select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                                ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList();

                det = StockNotManaged.Union(variable_StockNotManaged).Union(StockManaged).Union(variable_StockManaged).Distinct().ToList().Take(10);
            }
            else if (obj.MenuType.ToLower() == "stock adjustment")
            {
                if (obj.Type.ToLower() == "credit")
                {
                    det = (from c in oConnectionContext.DbClsItemBranchMap
                           join a in oConnectionContext.DbClsItemDetails
                          on c.ItemDetailsId equals a.ItemDetailsId
                           join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                           where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                          (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                          && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true && b.ProductType.ToLower() == "single"
                           select b.ItemName + " ~ " + b.SkuCode).
                           Union(from c in oConnectionContext.DbClsItemBranchMap
                                 join a in oConnectionContext.DbClsItemDetails
                                 on c.ItemDetailsId equals a.ItemDetailsId
                                 join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                 where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                 (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                 && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                                 && c.Quantity > 0
                                 select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                     ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).Distinct().ToList().Take(10);
                }
                else
                {
                    det = (from c in oConnectionContext.DbClsItemBranchMap
                           join a in oConnectionContext.DbClsItemDetails
                          on c.ItemDetailsId equals a.ItemDetailsId
                           join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                           where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                          (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                          && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true && b.ProductType.ToLower() == "single"
                          && c.Quantity > 0
                           select b.ItemName + " ~ " + b.SkuCode).Union(from c in oConnectionContext.DbClsItemBranchMap
                                                                        join a in oConnectionContext.DbClsItemDetails
                                                                       on c.ItemDetailsId equals a.ItemDetailsId
                                                                        join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                                                        where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                                                       (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                                                       && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                                                                       && c.Quantity > 0
                                                                        select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                                                           ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList().Take(10);
                }
            }
            else if (obj.MenuType.ToLower() == "print labels")
            {
                var ComboStock = (from c in oConnectionContext.DbClsItemBranchMap
                                  join a in oConnectionContext.DbClsItemDetails
                                  on c.ItemDetailsId equals a.ItemDetailsId
                                  join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                  where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                  (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                  && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false && b.ProductType.ToLower() == "combo"
                                  select new
                                  {
                                      b.ItemId,
                                      b.ItemName,
                                      b.SkuCode
                                  }).ToList();

                List<string> ComboStock1 = new List<string>();

                if (ComboStock != null && ComboStock.Count > 0)
                {
                    foreach (var _comboStock in ComboStock)
                    {
                        var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == _comboStock.ItemId).Select(a => new
                        {
                            ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                            //ComboItemDetailsId = a.ComboItemDetailsId,
                            IsManageStock = false,
                            Quantity = a.Quantity,
                            //a.PriceAddedFor,
                            QtyForSell = 0,
                            AvailableQuantity = oConnectionContext.DbClsItemBranchMap.Where(b => b.ItemDetailsId == a.ComboItemDetailsId && b.BranchId == obj.BranchId && b.IsActive == true && b.IsDeleted == false).Select(b => b.Quantity).FirstOrDefault(),
                        }).ToList();

                        ComboStock1.Add(_comboStock.ItemName + " ~ " + _comboStock.SkuCode);
                    }
                }

                var StockNotManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                       join a in oConnectionContext.DbClsItemDetails
                                       on c.ItemDetailsId equals a.ItemDetailsId
                                       join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                       where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                       (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || 
                                       b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                       && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false 
                                       && b.ProductType.ToLower() == "single"
                                       select b.ItemName + " ~ " + b.SkuCode).ToList();

                var variable_StockNotManaged = (
                                 from c in oConnectionContext.DbClsItemBranchMap
                                 join a in oConnectionContext.DbClsItemDetails
                                 on c.ItemDetailsId equals a.ItemDetailsId
                                 join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                 where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                 (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) 
                                 && a.IsActive == true && a.IsDeleted == false
                                 && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && 
                                 b.IsManageStock == false
                                 select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => 
                                 c.VariationDetailsId == a.VariationDetailsId
                                 ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList();

                var StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                    join a in oConnectionContext.DbClsItemDetails
                                   on c.ItemDetailsId equals a.ItemDetailsId
                                    join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                    where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                   (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                   && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true && b.ProductType.ToLower() == "single"
                                   //&& c.Quantity > 0
                                    select b.ItemName + " ~ " + b.SkuCode).ToList();

                var variable_StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                             join a in oConnectionContext.DbClsItemDetails
                                            on c.ItemDetailsId equals a.ItemDetailsId
                                             join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                             where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                            (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                            && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                                            //&& c.Quantity > 0
                                             select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                                ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList();

                det = ComboStock1.Union(StockNotManaged).Union(variable_StockNotManaged).Union(StockManaged).Union(variable_StockManaged).Distinct().ToList().Take(10);

                //var StockNotManaged = (from c in oConnectionContext.DbClsItemBranchMap
                //                       join a in oConnectionContext.DbClsItemDetails
                //                       on c.ItemDetailsId equals a.ItemDetailsId
                //                       join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                //                       where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                //                       (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                //                       && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false
                //                       select b.ItemName + " ~ " + b.SkuCode).ToList();

                //var variable_StockNotManaged = (
                //                 from c in oConnectionContext.DbClsItemBranchMap
                //                 join a in oConnectionContext.DbClsItemDetails
                //                 on c.ItemDetailsId equals a.ItemDetailsId
                //                 join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                //                 where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                //                 (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                //                 && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false
                //                 select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                //                 ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList();

                //var StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                //                    join a in oConnectionContext.DbClsItemDetails
                //                   on c.ItemDetailsId equals a.ItemDetailsId
                //                    join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                //                    where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                //                   (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                //                   && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                //                    //&& c.Quantity > 0
                //                    select b.ItemName + " ~ " + b.SkuCode).ToList();

                //var variable_StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                //                             join a in oConnectionContext.DbClsItemDetails
                //                            on c.ItemDetailsId equals a.ItemDetailsId
                //                             join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                //                             where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                //                            (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                //                            && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                //                             //&& c.Quantity > 0
                //                             select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                //                ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).ToList();

                //det = StockNotManaged.Union(variable_StockNotManaged).Union(StockManaged).Union(variable_StockManaged).Distinct().ToList().Take(10);


            }
            else
            {
                det = (from a in oConnectionContext.DbClsItemDetails
                       join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                       where a.CompanyId == obj.CompanyId &&
                      (b.SkuCode.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower()))
                      && a.IsActive == true && a.IsDeleted == false
                      && b.ProductType.ToLower() != "combo" && b.IsActive == true && b.IsDeleted == false //&& b.IsManageStock == true
                       select b.ItemName + " ~ " + b.SkuCode).
                           Union(from a in oConnectionContext.DbClsItemDetails
                                 join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                 where a.CompanyId == obj.CompanyId &&
                                 (a.SKU.ToLower().Contains(obj.Search.ToLower()) || b.ItemName.ToLower().Contains(obj.Search.ToLower())) && a.IsActive == true && a.IsDeleted == false
                                 && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false //&& b.IsManageStock == true
                                 //&& c.Quantity > 0
                                 select b.ItemName + "(" + oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                     ).Select(c => c.VariationDetails).FirstOrDefault() + ")" + " ~ " + a.SKU).Distinct().ToList().Take(10);
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ItemsArray = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SearchItems(ClsItemVm obj)
        {
            List<ClsItemDetailsVm> ItemDetails = new List<ClsItemDetailsVm>();
            decimal customerGroupDiscountPercentage = 0;
            if (obj.CustomerId != 0)
            {
                long UserGroupId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.UserGroupId).DefaultIfEmpty().FirstOrDefault();

                if (UserGroupId != 0)
                {
                    var UserGroup = oConnectionContext.DbClsUserGroup.Where
                    (a => a.UserGroupId == UserGroupId && a.IsActive == true && a.IsDeleted == false).Select(a => new
                    {
                        a.CalculationPercentage,
                        a.PriceCalculationType,
                        a.SellingPriceGroupId
                    }).FirstOrDefault();

                    if (UserGroup.PriceCalculationType == 1)
                    {
                        customerGroupDiscountPercentage = (UserGroup.CalculationPercentage / 100);
                    }
                    else
                    {
                        obj.SellingPriceGroupId = UserGroup.SellingPriceGroupId;
                    }
                }
            }

            bool EnableLotNo = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableLotNo).FirstOrDefault();
            bool EnableItemExpiry = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableItemExpiry).FirstOrDefault();

            var Item = oConnectionContext.DbClsItem.Where(a => a.SkuCode == obj.ItemCode.Trim() && a.IsActive == true
            && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.ItemId,
                a.ProductType,
                //a.ItemType
            }).FirstOrDefault();

            if (Item == null)
            {
                long ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(a => a.SKU.ToLower().Trim() == obj.ItemCode.ToLower().Trim() && a.IsActive == true &&
                a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => a.ItemDetailsId).FirstOrDefault();

                ItemDetails = (from c in oConnectionContext.DbClsItemBranchMap
                               join a in oConnectionContext.DbClsItemDetails
                                on c.ItemDetailsId equals a.ItemDetailsId
                               join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                               where c.ItemDetailsId == ItemDetailsId && a.ItemDetailsId == ItemDetailsId
                               && c.BranchId == obj.BranchId
                               && c.IsActive == true && b.IsActive == true && b.IsDeleted == false
                               //&& (c.Quantity > 0 || b.IsManageStock == false)
                               select new ClsItemDetailsVm
                               {
                                   ItemCodeId = b.ItemCodeId,
                                   ItemType = b.ItemType,
                                   WarrantyId = b.WarrantyId,
                                   //EnableWarranty = oConnectionContext.DbClsItemSettings.Where(f => f.CompanyId == obj.CompanyId).Select(f => f.EnableWarranty).FirstOrDefault(),
                                   DefaultProfitMargin = a.DefaultProfitMargin,
                                   TaxType = b.TaxType,
                                   IsManageStock = b.IsManageStock,
                                   //ItemBranchMapId =c.ItemBranchMapId,
                                   Quantity = c.Quantity,
                                   ItemId = b.ItemId,
                                   ProductType = b.ProductType,
                                   ItemDetailsId = a.ItemDetailsId,
                                   ItemName = b.ItemName,
                                   SKU = a.SKU == null ? b.SkuCode : a.SKU,
                                   VariationDetailsId = a.VariationDetailsId,
                                   VariationName = oConnectionContext.DbClsVariationDetails.Where(d => d.VariationDetailsId == a.VariationDetailsId).Select(d => d.VariationDetails).FirstOrDefault(),
                                   UnitId = b.UnitId,
                                   SecondaryUnitId = b.SecondaryUnitId,
                                   TertiaryUnitId = b.TertiaryUnitId,
                                   QuaternaryUnitId = b.QuaternaryUnitId,
                                   UnitShortName = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.UnitShortName).FirstOrDefault(),
                                   SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitShortName).FirstOrDefault(),
                                   TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitShortName).FirstOrDefault(),
                                   QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitShortName).FirstOrDefault(),
                                   UToSValue = b.UToSValue,
                                   SToTValue = b.SToTValue,
                                   TToQValue = b.TToQValue,
                                   PriceAddedFor = b.PriceAddedFor,
                                   PurchaseExcTax = a.PurchaseExcTax,
                                   PurchaseIncTax = a.PurchaseIncTax,
                                   SalesExcTax = a.SalesExcTax + (customerGroupDiscountPercentage * a.SalesExcTax),
                                   SalesIncTax = a.SalesIncTax + (customerGroupDiscountPercentage * a.SalesIncTax),
                                   TotalCost = a.TotalCost + (customerGroupDiscountPercentage * a.TotalCost),
                                   TaxId = b.TaxId,
                                   Tax = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.Tax).FirstOrDefault(),
                                   InterStateTaxId = b.InterStateTaxId,
                                   InterStateTax = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.Tax).FirstOrDefault(),
                                   InterStateTaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                   TaxExemptionId = b.TaxExemptionId,
                                   TaxPreferenceId = b.TaxPreferenceId,
                                   AllowDecimal = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.AllowDecimal).FirstOrDefault(),
                                   SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                   TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                   QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                   EnableLotNo = EnableLotNo,
                                   EnableImei = b.EnableImei,
                                   DefaultMrp = a.DefaultMrp,
                               }).ToList();
            }
            else
            {
                if (Item.ProductType == "Combo")
                {
                    var combo = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == Item.ItemId).Select(a => new
                    {
                        ItemId = oConnectionContext.DbClsItemDetails.Where(b => b.ItemDetailsId == a.ComboItemDetailsId).Select(b => b.ItemId).FirstOrDefault(),
                        //ItemDetailsId = a.ItemDetailsId,
                        //ComboItemDetailsId = a.ComboItemDetailsId,
                        IsManageStock = false,
                        Quantity = a.Quantity,
                        //a.PriceAddedFor,
                        QtyForSell = 0,
                        AvailableQuantity = oConnectionContext.DbClsItemBranchMap.Where(b => b.ItemDetailsId == a.ComboItemDetailsId && b.BranchId == obj.BranchId && b.IsActive == true && b.IsDeleted == false).Select(b => b.Quantity).FirstOrDefault(),
                    }).ToList();

                    var itemsWithQty = combo.Select(item => new
                    {
                        IsManageStock = oConnectionContext.DbClsItem.Where(b => b.ItemId == item.ItemId).Select(b => b.IsManageStock).FirstOrDefault(),
                        item.Quantity,
                        item.AvailableQuantity,
                        QtyForSell = item.AvailableQuantity != 0 ? (int)(item.AvailableQuantity / item.Quantity) : 0
                    }).ToList();

                    //var minQty = itemsWithQty.Where(a=>a.IsManageStock == true).DefaultIfEmpty().Min(item => item.QtyForSell);

                    var minQty = itemsWithQty
                       .Where(a => a != null && a.IsManageStock == true)
                       .Select(item => item.QtyForSell)
                       .DefaultIfEmpty()
                       .Min();
                    bool IsManageStock = itemsWithQty.Where(a => a.IsManageStock == true).Count() > 0 ? true : false;

                    ItemDetails = (from c in oConnectionContext.DbClsItemBranchMap
                                   join a in oConnectionContext.DbClsItemDetails
                                   on c.ItemId equals a.ItemId
                                   join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                   where c.ItemId == Item.ItemId && a.ItemId == Item.ItemId
                                   && c.BranchId == obj.BranchId
                                   && c.IsActive == true && b.IsActive == true && b.IsDeleted == false
                                   //&& (c.Quantity > 0 || b.IsManageStock == false)
                                   select new ClsItemDetailsVm
                                   {
                                       ItemCodeId = b.ItemCodeId,
                                       ItemType = b.ItemType,
                                       WarrantyId = b.WarrantyId,
                                       //EnableWarranty = oConnectionContext.DbClsItemSettings.Where(f => f.CompanyId == obj.CompanyId).Select(f => f.EnableWarranty).FirstOrDefault(),
                                       DefaultProfitMargin = a.DefaultProfitMargin,
                                       TaxType = b.TaxType,
                                       IsManageStock = IsManageStock,//b.IsManageStock,
                                       Quantity = minQty,//c.Quantity,
                                       ItemId = b.ItemId,
                                       ProductType = b.ProductType,
                                       ItemDetailsId = a.ItemDetailsId,
                                       ItemName = b.ItemName,
                                       SKU = a.SKU == null ? b.SkuCode : a.SKU,
                                       VariationDetailsId = a.VariationDetailsId,
                                       VariationName = oConnectionContext.DbClsVariationDetails.Where(d => d.VariationDetailsId == a.VariationDetailsId).Select(d => d.VariationDetails).FirstOrDefault(),
                                       UnitId = b.UnitId,
                                       SecondaryUnitId = b.SecondaryUnitId,
                                       TertiaryUnitId = b.TertiaryUnitId,
                                       QuaternaryUnitId = b.QuaternaryUnitId,
                                       UnitShortName = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.UnitShortName).FirstOrDefault(),
                                       SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitShortName).FirstOrDefault(),
                                       TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitShortName).FirstOrDefault(),
                                       QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitShortName).FirstOrDefault(),
                                       UToSValue = b.UToSValue,
                                       SToTValue = b.SToTValue,
                                       TToQValue = b.TToQValue,
                                       PriceAddedFor = b.PriceAddedFor,
                                       PurchaseExcTax = a.SalesExcTax + (customerGroupDiscountPercentage * a.SalesExcTax),
                                       PurchaseIncTax = obj.MenuType == "sale" ? a.SalesIncTax + (customerGroupDiscountPercentage * a.SalesIncTax) : a.SalesIncTax + (customerGroupDiscountPercentage * a.SalesIncTax),
                                       SalesExcTax = a.SalesExcTax + (customerGroupDiscountPercentage * a.SalesExcTax),
                                       //SalesIncTax = obj.MenuType == "sale" ? a.SalesIncTax + (customerGroupDiscountPercentage * a.SalesIncTax) : a.SalesIncTax + (customerGroupDiscountPercentage * a.SalesIncTax),
                                       SalesIncTax = a.SalesIncTax + (customerGroupDiscountPercentage * a.SalesIncTax),
                                       TotalCost = a.TotalCost + (customerGroupDiscountPercentage * a.TotalCost),
                                       TaxId = b.TaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.Tax).FirstOrDefault(),
                                       InterStateTaxId = b.InterStateTaxId,
                                       InterStateTax = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.Tax).FirstOrDefault(),
                                       InterStateTaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                       TaxExemptionId = b.TaxExemptionId,
                                       TaxPreferenceId = b.TaxPreferenceId,
                                       AllowDecimal = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.AllowDecimal).FirstOrDefault(),
                                       SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                       TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == d.TertiaryUnitId).Select(d => d.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                       QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                       EnableLotNo = EnableLotNo,
                                       EnableImei = b.EnableImei,
                                       DefaultMrp = a.DefaultMrp
                                   }).OrderBy(a => a.ItemDetailsId).Take(1).ToList();
                }
                else
                {
                    ItemDetails = (from c in oConnectionContext.DbClsItemBranchMap
                                   join a in oConnectionContext.DbClsItemDetails
                                   on c.ItemDetailsId equals a.ItemDetailsId
                                   join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                   where c.ItemId == Item.ItemId && a.ItemId == Item.ItemId
                                   && c.BranchId == obj.BranchId
                                   && c.IsActive == true && b.IsActive == true && b.IsDeleted == false
                                   //&& (c.Quantity > 0 || b.IsManageStock == false)
                                   select new ClsItemDetailsVm
                                   {
                                       ItemCodeId = b.ItemCodeId,
                                       ItemType = b.ItemType,
                                       WarrantyId = b.WarrantyId,
                                       //EnableWarranty = oConnectionContext.DbClsItemSettings.Where(f => f.CompanyId == obj.CompanyId).Select(f => f.EnableWarranty).FirstOrDefault(),
                                       DefaultProfitMargin = a.DefaultProfitMargin,
                                       TaxType = b.TaxType,
                                       IsManageStock = b.IsManageStock,
                                       //ItemBranchMapId=c.ItemBranchMapId,
                                       Quantity = c.Quantity,
                                       ItemId = b.ItemId,
                                       ProductType = b.ProductType,
                                       ItemDetailsId = a.ItemDetailsId,
                                       ItemName = b.ItemName,
                                       SKU = a.SKU == null ? b.SkuCode : a.SKU,
                                       VariationDetailsId = a.VariationDetailsId,
                                       VariationName = oConnectionContext.DbClsVariationDetails.Where(d => d.VariationDetailsId == a.VariationDetailsId).Select(d => d.VariationDetails).FirstOrDefault(),
                                       UnitId = b.UnitId,
                                       SecondaryUnitId = b.SecondaryUnitId,
                                       TertiaryUnitId = b.TertiaryUnitId,
                                       QuaternaryUnitId = b.QuaternaryUnitId,
                                       UnitShortName = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.UnitShortName).FirstOrDefault(),
                                       SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitShortName).FirstOrDefault(),
                                       TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitShortName).FirstOrDefault(),
                                       QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitShortName).FirstOrDefault(),
                                       UToSValue = b.UToSValue,
                                       SToTValue = b.SToTValue,
                                       TToQValue = b.TToQValue,
                                       PriceAddedFor = b.PriceAddedFor,
                                       PurchaseExcTax = a.PurchaseExcTax,
                                       PurchaseIncTax = a.PurchaseIncTax,
                                       SalesExcTax = a.SalesExcTax + (customerGroupDiscountPercentage * a.SalesExcTax),
                                       //SalesIncTax = obj.MenuType == "sale" ? c.SalesIncTax + (customerGroupDiscountPercentage * c.SalesIncTax) : a.SalesIncTax + (customerGroupDiscountPercentage * a.SalesIncTax),
                                       SalesIncTax = a.SalesIncTax + (customerGroupDiscountPercentage * a.SalesIncTax),
                                       TotalCost = a.TotalCost + (customerGroupDiscountPercentage * a.TotalCost),
                                       TaxId = b.TaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                       InterStateTaxId = b.InterStateTaxId,
                                       InterStateTax = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.Tax).FirstOrDefault(),
                                       InterStateTaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                       TaxExemptionId = b.TaxExemptionId,
                                       TaxPreferenceId = b.TaxPreferenceId,
                                       AllowDecimal = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.AllowDecimal).FirstOrDefault(),
                                       SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                       TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                       QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                       EnableLotNo = EnableLotNo,
                                       EnableImei = b.EnableImei,
                                       DefaultMrp = a.DefaultMrp
                                   }).Distinct().ToList();
                }
            }

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.CountryId
            }).FirstOrDefault();

            long BranchStateId = 0;
            string CustomerTaxPreference = "";
            string GstTreatment = "";

            if (BusinessSetting.CountryId == 2)
            {
                if (obj.Type == "Sales")
                {
                    BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.StateId).FirstOrDefault();

                    if (obj.CustomerId != 0)
                    {
                        GstTreatment = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.GstTreatment).FirstOrDefault();
                        CustomerTaxPreference = oConnectionContext.DbClsTax.Where(a => a.TaxId ==
                        oConnectionContext.DbClsUser.Where(b => b.UserId == obj.CustomerId).Select(b => b.TaxPreferenceId).FirstOrDefault()).Select(a => a.Tax).FirstOrDefault();

                        if (GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" || GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || GstTreatment == "Deemed Export" || GstTreatment == "Supply by SEZ Developer")
                        {
                            long TaxId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.Tax == "IGST 0%").Select(a => a.TaxId).FirstOrDefault();

                            foreach (var item in ItemDetails)
                            {
                                item.PurchaseIncTax = item.PurchaseExcTax;
                                item.SalesIncTax = item.SalesExcTax;
                                item.TaxId = TaxId;
                                item.TaxExemptionId = 0;
                            }
                        }
                        else
                        {
                            if (CustomerTaxPreference == "Taxable")
                            {
                                foreach (var item in ItemDetails)
                                {
                                    string ItemTaxPreference = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId
                                    && a.TaxId == item.TaxPreferenceId).Select(a => a.Tax).FirstOrDefault();

                                    if (ItemTaxPreference == "Taxable")
                                    {
                                        decimal TaxPercent = 0;
                                        long TaxId = 0;
                                        if (BranchStateId == obj.PlaceOfSupplyId)
                                        {
                                            TaxPercent = item.TaxPercent;
                                            TaxId = item.TaxId;
                                        }
                                        else
                                        {
                                            TaxPercent = item.InterStateTaxPercent;
                                            TaxId = item.InterStateTaxId;
                                        }

                                        string TaxType = item.TaxType;
                                        if (TaxType == "Exclusive")
                                        {
                                            item.SalesIncTax = ((TaxPercent / 100) * item.SalesExcTax) + item.SalesExcTax;
                                            item.PurchaseIncTax = ((TaxPercent / 100) * item.PurchaseExcTax) + item.PurchaseExcTax;
                                        }
                                        else
                                        {
                                            item.SalesIncTax = item.SalesExcTax;
                                            item.PurchaseIncTax = item.PurchaseExcTax;

                                            item.SalesExcTax = (item.SalesIncTax) / (1 + (TaxPercent) / 100);
                                            item.PurchaseExcTax = (item.PurchaseIncTax) / (1 + (TaxPercent) / 100);
                                        }

                                        item.TaxPercent = TaxPercent;
                                        item.TaxId = TaxId;
                                        item.TaxExemptionId = 0;
                                    }
                                    else
                                    {
                                        item.PurchaseIncTax = item.PurchaseExcTax;
                                        item.SalesIncTax = item.SalesExcTax;
                                        item.TaxId = item.TaxPreferenceId;
                                    }
                                }
                            }
                            else
                            {
                                long TaxId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();
                                //long TaxExemptionId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.Tax == "Tax Exemption").Select(a => a.TaxId).FirstOrDefault();

                                foreach (var item in ItemDetails)
                                {
                                    item.PurchaseIncTax = item.PurchaseExcTax;
                                    item.SalesIncTax = item.SalesExcTax;
                                    item.TaxId = TaxId;
                                    item.TaxExemptionId = item.TaxExemptionId;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in ItemDetails)
                        {
                            decimal TaxPercent = 0;
                            long TaxId = 0;
                            if (BranchStateId == obj.PlaceOfSupplyId)
                            {
                                TaxPercent = item.TaxPercent;
                                TaxId = item.TaxId;
                            }
                            else
                            {
                                TaxPercent = item.InterStateTaxPercent;
                                TaxId = item.InterStateTaxId;
                            }

                            string TaxType = item.TaxType;
                            if (TaxType == "Exclusive")
                            {
                                item.SalesIncTax = ((TaxPercent / 100) * item.SalesExcTax) + item.SalesExcTax;
                                item.PurchaseIncTax = ((TaxPercent / 100) * item.PurchaseExcTax) + item.PurchaseExcTax;
                            }
                            else
                            {
                                item.SalesIncTax = item.SalesExcTax;
                                item.PurchaseIncTax = item.PurchaseExcTax;

                                item.SalesExcTax = (item.SalesIncTax) / (1 + (TaxPercent) / 100);
                                item.PurchaseExcTax = (item.PurchaseIncTax) / (1 + (TaxPercent) / 100);
                            }

                            item.TaxPercent = TaxPercent;
                            item.TaxId = TaxId;
                            item.TaxExemptionId = 0;
                        }
                    }
                }
                else if (obj.Type == "Purchase")
                {
                    GstTreatment = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.GstTreatment).FirstOrDefault();
                    if (GstTreatment != "Taxable Supply to Unregistered Person")
                    {
                        foreach (var item in ItemDetails)
                        {
                            string ItemTaxPreference = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId
                            && a.TaxId == item.TaxPreferenceId).Select(a => a.Tax).FirstOrDefault();

                            if (ItemTaxPreference == "Taxable")
                            {
                                decimal TaxPercent = 0;
                                long TaxId = 0;
                                if (obj.SourceOfSupplyId == obj.DestinationOfSupplyId)
                                {
                                    TaxPercent = item.TaxPercent;
                                    TaxId = item.TaxId;
                                }
                                else
                                {
                                    TaxPercent = item.InterStateTaxPercent;
                                    TaxId = item.InterStateTaxId;
                                }

                                string TaxType = item.TaxType;
                                if (TaxType == "Exclusive")
                                {
                                    item.SalesIncTax = ((TaxPercent / 100) * item.SalesExcTax) + item.SalesExcTax;
                                    item.PurchaseIncTax = ((TaxPercent / 100) * item.PurchaseExcTax) + item.PurchaseExcTax;
                                }
                                else
                                {
                                    item.SalesIncTax = item.SalesExcTax;
                                    item.PurchaseIncTax = item.PurchaseExcTax;

                                    item.SalesExcTax = (item.SalesIncTax) / (1 + (TaxPercent) / 100);
                                    item.PurchaseExcTax = (item.PurchaseIncTax) / (1 + (TaxPercent) / 100);
                                }

                                item.TaxPercent = TaxPercent;
                                item.TaxId = TaxId;
                                item.TaxExemptionId = 0;
                            }
                            else
                            {
                                item.PurchaseIncTax = item.PurchaseExcTax;
                                item.SalesIncTax = item.SalesExcTax;
                                item.TaxId = item.TaxPreferenceId;
                            }
                        }
                    }
                    else
                    {
                        long TaxId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();
                        //long TaxExemptionId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.Tax == "Tax Exemption").Select(a => a.TaxId).FirstOrDefault();

                        foreach (var item in ItemDetails)
                        {
                            item.PurchaseIncTax = item.PurchaseExcTax;
                            item.SalesIncTax = item.SalesExcTax;
                            item.TaxId = TaxId;
                            item.TaxExemptionId = item.TaxExemptionId;
                        }
                    }
                }
            }
            else
            {
                if (obj.CustomerId != 0)
                {
                    CustomerTaxPreference = oConnectionContext.DbClsTax.Where(a => a.TaxId ==
                    oConnectionContext.DbClsUser.Where(b => b.UserId == obj.CustomerId).Select(b => b.TaxPreferenceId).FirstOrDefault()).Select(a => a.Tax).FirstOrDefault();

                    if (CustomerTaxPreference == "Taxable")
                    {
                        foreach (var item in ItemDetails)
                        {
                            string ItemTaxPreference = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId
                            && a.TaxId == item.TaxPreferenceId).Select(a => a.Tax).FirstOrDefault();

                            if (ItemTaxPreference == "Taxable")
                            {
                                string TaxType = item.TaxType;
                                if (TaxType == "Exclusive")
                                {
                                    item.SalesIncTax = ((item.TaxPercent / 100) * item.SalesExcTax) + item.SalesExcTax;
                                    item.PurchaseIncTax = ((item.TaxPercent / 100) * item.PurchaseExcTax) + item.PurchaseExcTax;
                                }
                                else
                                {
                                    item.SalesIncTax = item.SalesExcTax;
                                    item.PurchaseIncTax = item.PurchaseExcTax;

                                    item.SalesExcTax = (item.SalesIncTax) / (1 + (item.TaxPercent) / 100);
                                    item.PurchaseExcTax = (item.PurchaseIncTax) / (1 + (item.TaxPercent) / 100);
                                }
                            }
                            else
                            {
                                item.PurchaseIncTax = item.PurchaseExcTax;
                                item.SalesIncTax = item.SalesExcTax;
                                item.TaxId = item.TaxPreferenceId;
                            }
                        }
                    }
                    else
                    {
                        long TaxId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();
                        //long TaxExemptionId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.Tax == "Tax Exemption").Select(a => a.TaxId).FirstOrDefault();

                        foreach (var item in ItemDetails)
                        {
                            item.PurchaseIncTax = item.PurchaseExcTax;
                            item.SalesIncTax = item.SalesExcTax;
                            item.TaxId = TaxId;
                            item.TaxExemptionId = item.TaxExemptionId;
                        }
                    }
                }
            }

            if (obj.SellingPriceGroupId != 0)
            {
                ItemDetails = ItemDetails.Select(a => new ClsItemDetailsVm
                {
                    ItemCodeId = a.ItemCodeId,
                    ItemType = a.ItemType,
                    WarrantyId = a.WarrantyId,
                    //EnableWarranty = oConnectionContext.DbClsItemSettings.Where(f => f.CompanyId == obj.CompanyId).Select(f => f.EnableWarranty).FirstOrDefault(),
                    DefaultProfitMargin = a.DefaultProfitMargin,
                    TaxType = a.TaxType,
                    IsManageStock = a.IsManageStock,
                    //ItemBranchMapId=c.ItemBranchMapId,
                    Quantity = a.Quantity,
                    ItemId = a.ItemId,
                    ProductType = a.ProductType,
                    ItemDetailsId = a.ItemDetailsId,
                    ItemName = a.ItemName,
                    SKU = a.SKU,
                    VariationDetailsId = a.VariationDetailsId,
                    VariationName = a.VariationName,
                    UnitId = a.UnitId,
                    SecondaryUnitId = a.SecondaryUnitId,
                    TertiaryUnitId = a.TertiaryUnitId,
                    QuaternaryUnitId = a.QuaternaryUnitId,
                    UnitShortName = a.UnitShortName,
                    SecondaryUnitShortName = a.SecondaryUnitShortName,
                    TertiaryUnitShortName = a.TertiaryUnitShortName,
                    QuaternaryUnitShortName = a.QuaternaryUnitShortName,
                    UToSValue = a.UToSValue,
                    SToTValue = a.SToTValue,
                    TToQValue = a.TToQValue,
                    PriceAddedFor = a.PriceAddedFor,
                    PurchaseExcTax = a.PurchaseExcTax,
                    PurchaseIncTax = a.PurchaseIncTax,
                    SalesExcTax = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId && b.SellingPrice > 0).Count() == 0 ? a.SalesExcTax :
                    (oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.DiscountType).FirstOrDefault() == "Fixed" ?
                    oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.SellingPrice).FirstOrDefault()
                    : ((oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.SellingPrice).FirstOrDefault() / 100) * a.SalesExcTax)),
                    SalesIncTax = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId && b.SellingPrice > 0).Count() == 0 ? a.SalesIncTax :
                    (oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.DiscountType).FirstOrDefault() == "Fixed" ?
                    oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.SellingPrice).FirstOrDefault()
                    : ((oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.SellingPrice).FirstOrDefault() / 100) * a.SalesIncTax)),
                    TotalCost = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId && b.SellingPrice > 0).Count() == 0 ? a.SalesIncTax :
                    (oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.DiscountType).FirstOrDefault() == "Fixed" ?
                    oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.SellingPrice).FirstOrDefault() :
                    ((oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == a.ItemId && b.ItemDetailsId == a.ItemDetailsId &&
                    b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.SellingPrice).FirstOrDefault() / 100) * a.SalesIncTax)),
                    TaxId = a.TaxId,
                    Tax = a.Tax,
                    TaxPercent = a.TaxPercent,
                    InterStateTaxId = a.InterStateTaxId,
                    InterStateTax = a.InterStateTax,
                    InterStateTaxPercent = a.InterStateTaxPercent,
                    TaxExemptionId = a.TaxExemptionId,
                    TaxPreferenceId = a.TaxPreferenceId,
                    AllowDecimal = a.AllowDecimal,
                    SecondaryUnitAllowDecimal = a.SecondaryUnitAllowDecimal,
                    TertiaryUnitAllowDecimal = a.TertiaryUnitAllowDecimal,
                    QuaternaryUnitAllowDecimal = a.QuaternaryUnitAllowDecimal,
                    EnableLotNo = a.EnableLotNo,
                    EnableImei = a.EnableImei,
                    DefaultMrp = a.DefaultMrp
                }).ToList();
            }

            if (EnableLotNo == true)
            {
                if (ItemDetails != null && ItemDetails.Count > 0)
                {
                    if (ItemDetails[0].IsManageStock == true)
                    {
                        foreach (var item in ItemDetails)
                        {
                            List<ClsAvailableLots> availableLots = (from a in oConnectionContext.DbClsPurchase
                                                                    join b in oConnectionContext.DbClsPurchaseDetails
                                                                     on a.PurchaseId equals b.PurchaseId
                                                                    where a.BranchId == obj.BranchId
                                                                    && b.ItemDetailsId == item.ItemDetailsId && b.QuantityRemaining > 0
                                                                    && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && b.IsActive == true
                                                                    && b.IsDeleted == false && b.IsStopSelling == false
                                                                    select new ClsAvailableLots
                                                                    {
                                                                        LotNo = b.LotNo,
                                                                        ExpiryDate = b.ExpiryDate,
                                                                        ManufacturingDate = b.ManufacturingDate,
                                                                        Type = "purchase",
                                                                        Id = b.PurchaseDetailsId
                                                                    }).Union(from a in oConnectionContext.DbClsOpeningStock
                                                                             where a.BranchId == obj.BranchId && a.ItemDetailsId == item.ItemDetailsId && a.QuantityRemaining > 0
                                                                                  && a.IsActive == true && a.IsDeleted == false && a.IsActive == true
                                                                                  && a.IsDeleted == false && a.IsStopSelling == false
                                                                             select new ClsAvailableLots
                                                                             {
                                                                                 LotNo = a.LotNo,
                                                                                 ExpiryDate = a.ExpiryDate,
                                                                                 ManufacturingDate = a.ManufacturingDate,
                                                                                 Type = "openingstock",
                                                                                 Id = a.OpeningStockId
                                                                             })
                                                                    .Union(from a in oConnectionContext.DbClsStockTransfer
                                                                           join b in oConnectionContext.DbClsStockTransferDetails
                                                                           on a.StockTransferId equals b.StockTransferId
                                                                           where a.ToBranchId == obj.BranchId && b.ItemDetailsId == item.ItemDetailsId && b.QuantityRemaining > 0
                                                                                  && a.IsActive == true && a.IsDeleted == false && b.IsActive == true && b.IsDeleted == false
                                                                                  && b.IsStopSelling == false
                                                                           select new ClsAvailableLots
                                                                           {
                                                                               LotNo = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault()
                                      : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault(),
                                                                               ExpiryDate = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                                      : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault(),
                                                                               ManufacturingDate = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                                      : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault(),
                                                                               Type = "stocktransfer",
                                                                               Id = b.StockTransferDetailsId
                                                                           })
                                                      //                              .Union(from a in oConnectionContext.DbClsStockTransfer
                                                      //                                     join b in oConnectionContext.DbClsStockTransferDetails
                                                      //                                     on a.StockTransferId equals b.StockTransferId
                                                      //                                     where a.ToBranchId == obj.BranchId && b.ItemDetailsId == item.ItemDetailsId && b.QuantityRemaining > 0
                                                      //                                            && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false && b.IsActive == true && b.IsDeleted == false && b.IsCancelled == false
                                                      //                                            && b.IsStopSelling == false
                                                      //                                     select new ClsAvailableLots
                                                      //                                     {
                                                      //                                         LotNo = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.LotNo + (EnableItemExpiry == true ? "- Exp Date: " + f.ExpiryDate : "")).FirstOrDefault()
                                                      //: b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.LotNo + (EnableItemExpiry == true ? "- Exp Date: " + f.ExpiryDate : "")).FirstOrDefault()
                                                      //: "Default Stock Accounting Method",
                                                      //                                         ExpiryDate = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                                                      //: b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                                                      //: DateTime.MinValue,
                                                      //                                         ManufacturingDate = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                                                      //: b.LotType == "openingstock" ? oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                                                      //: DateTime.MinValue,
                                                      //                                         Type = "stocktransfer",
                                                      //                                         Id = b.StockTransferDetailsId
                                                      //                                     })
                                                      .ToList();

                            item.AvailableLots = availableLots;

                        }
                    }
                }

            }

            if (obj.IsBillOfSupply == true)
            {
                long TaxId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

                foreach (var item in ItemDetails)
                {
                    item.PurchaseIncTax = item.PurchaseExcTax;
                    item.SalesIncTax = item.SalesExcTax;
                    item.TaxId = TaxId;
                    item.TaxExemptionId = item.TaxExemptionId;
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ItemDetails = ItemDetails,
                    UserGroup = new
                    {
                        SellingPriceGroupId = obj.SellingPriceGroupId,
                        CalculationPercentage = customerGroupDiscountPercentage
                    },
                    BusinessSetting = new
                    {
                        CountryId = BusinessSetting.CountryId,
                        StateId = BranchStateId,
                        CustomerTaxPreference = CustomerTaxPreference,
                        GstTreatment = GstTreatment
                    }
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SearchItemsWithoutBranch(ClsItemVm obj)
        {
            var Item = oConnectionContext.DbClsItem.Where(a => a.SkuCode == obj.ItemCode.Trim() && a.IsActive == true &&
            a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.ItemId,
                a.ProductType
            }).FirstOrDefault();

            if (Item == null)
            {
                long ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(a => a.SKU.ToLower().Trim() == obj.ItemCode.ToLower().Trim() && a.IsActive == true
                && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Select(a => a.ItemDetailsId).FirstOrDefault();

                var ItemDetails = (from a in oConnectionContext.DbClsItemDetails
                                   join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                   where a.ItemDetailsId == ItemDetailsId && b.IsActive == true && b.IsDeleted == false
                                   select new
                                   {
                                       UnitCost = b.TaxId == 0 ? a.PurchaseIncTax : a.PurchaseExcTax,
                                       b.ItemId,
                                       b.ProductType,
                                       a.ItemDetailsId,
                                       b.ItemName,
                                       SKU = a.SKU == null ? b.SkuCode : a.SKU,
                                       a.VariationDetailsId,
                                       VariationName = oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId).Select(c => c.VariationDetails).FirstOrDefault(),
                                       UnitName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == b.UnitId).Select(c => c.UnitName).FirstOrDefault(),
                                       PurchaseExcTax = b.TaxType == "Inclusive" ? (100 / (100 + (oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault()))) * a.PurchaseExcTax : a.PurchaseExcTax,
                                       PurchaseIncTax = b.TaxType == "Inclusive" ? a.PurchaseExcTax : ((oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault() / 100) * a.PurchaseExcTax),
                                       SalesExcTax = b.TaxType == "Inclusive" ? (100 / (100 + (oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault()))) * a.SalesExcTax : a.SalesExcTax,
                                       SalesIncTax = b.TaxType == "Inclusive" ? a.SalesExcTax : ((oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault() / 100) * a.SalesExcTax),
                                       a.TotalCost,
                                       b.TaxId,
                                       Tax = oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.Tax).FirstOrDefault(),
                                       TaxPercent = oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault(),
                                       b.TaxType,
                                       b.ItemCode,
                                       UnitId = b.UnitId,
                                       SecondaryUnitId = b.SecondaryUnitId,
                                       TertiaryUnitId = b.TertiaryUnitId,
                                       QuaternaryUnitId = b.QuaternaryUnitId,
                                       UnitShortName = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.UnitShortName).FirstOrDefault(),
                                       SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitShortName).FirstOrDefault(),
                                       TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitShortName).FirstOrDefault(),
                                       QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitShortName).FirstOrDefault(),
                                       UToSValue = b.UToSValue,
                                       SToTValue = b.SToTValue,
                                       TToQValue = b.TToQValue,
                                       PriceAddedFor = b.PriceAddedFor,
                                       AllowDecimal = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.AllowDecimal).FirstOrDefault(),
                                       SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                       TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                       QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                   }).ToList();
                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        ItemDetails = ItemDetails,
                    }
                };
            }
            else
            {
                if (Item.ProductType == "Combo")
                {
                    var ItemDetails = (from a in oConnectionContext.DbClsItemDetails
                                       join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                       where a.ItemId == Item.ItemId && b.IsActive == true && b.IsDeleted == false
                                       select new
                                       {
                                           UnitCost = b.TaxId == 0 ? a.PurchaseIncTax : a.PurchaseExcTax,
                                           b.ItemId,
                                           b.ProductType,
                                           a.ItemDetailsId,
                                           b.ItemName,
                                           SKU = a.SKU == null ? b.SkuCode : a.SKU,
                                           a.VariationDetailsId,
                                           VariationName = oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId).Select(c => c.VariationDetails).FirstOrDefault(),
                                           UnitName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == b.UnitId).Select(c => c.UnitName).FirstOrDefault(),
                                           PurchaseExcTax = b.TaxType == "Inclusive" ? (100 / (100 + (oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault()))) * a.PurchaseExcTax : a.PurchaseExcTax,
                                           PurchaseIncTax = b.TaxType == "Inclusive" ? a.PurchaseExcTax : ((oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault() / 100) * a.PurchaseExcTax),
                                           SalesExcTax = b.TaxType == "Inclusive" ? (100 / (100 + (oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault()))) * a.SalesExcTax : a.SalesExcTax,
                                           SalesIncTax = b.TaxType == "Inclusive" ? a.SalesExcTax : ((oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault() / 100) * a.SalesExcTax),
                                           a.TotalCost,
                                           b.TaxId,
                                           Tax = oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.Tax).FirstOrDefault(),
                                           TaxPercent = oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault(),
                                           b.TaxType,
                                           b.ItemCode,
                                           UnitId = b.UnitId,
                                           SecondaryUnitId = b.SecondaryUnitId,
                                           TertiaryUnitId = b.TertiaryUnitId,
                                           QuaternaryUnitId = b.QuaternaryUnitId,
                                           UnitShortName = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.UnitShortName).FirstOrDefault(),
                                           SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitShortName).FirstOrDefault(),
                                           TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitShortName).FirstOrDefault(),
                                           QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitShortName).FirstOrDefault(),
                                           UToSValue = b.UToSValue,
                                           SToTValue = b.SToTValue,
                                           TToQValue = b.TToQValue,
                                           PriceAddedFor = b.PriceAddedFor,
                                           AllowDecimal = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.AllowDecimal).FirstOrDefault(),
                                           SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                           TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                           QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                       }).ToList().Take(1);

                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            ItemDetails = ItemDetails,
                        }
                    };
                }
                else
                {
                    var ItemDetails = (from a in oConnectionContext.DbClsItemDetails
                                       join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                       where a.ItemId == Item.ItemId
                                       && b.IsActive == true && b.IsDeleted == false
                                       select new
                                       {
                                           UnitCost = b.TaxId == 0 ? a.PurchaseIncTax : a.PurchaseExcTax,
                                           b.ItemId,
                                           b.ProductType,
                                           a.ItemDetailsId,
                                           b.ItemName,
                                           SKU = a.SKU == null ? b.SkuCode : a.SKU,
                                           a.VariationDetailsId,
                                           VariationName = oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId).Select(c => c.VariationDetails).FirstOrDefault(),
                                           UnitName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == b.UnitId).Select(c => c.UnitName).FirstOrDefault(),
                                           PurchaseExcTax = b.TaxType == "Inclusive" ? (100 / (100 + (oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault()))) * a.PurchaseExcTax : a.PurchaseExcTax,
                                           PurchaseIncTax = b.TaxType == "Inclusive" ? a.PurchaseExcTax : ((oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault() / 100) * a.PurchaseExcTax),
                                           SalesExcTax = b.TaxType == "Inclusive" ? (100 / (100 + (oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault()))) * a.SalesExcTax : a.SalesExcTax,
                                           SalesIncTax = b.TaxType == "Inclusive" ? a.SalesExcTax : ((oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault() / 100) * a.SalesExcTax),
                                           a.TotalCost,
                                           b.TaxId,
                                           Tax = oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.Tax).FirstOrDefault(),
                                           TaxPercent = oConnectionContext.DbClsTax.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxPercent).FirstOrDefault(),
                                           b.TaxType,
                                           b.ItemCode,
                                           UnitId = b.UnitId,
                                           SecondaryUnitId = b.SecondaryUnitId,
                                           TertiaryUnitId = b.TertiaryUnitId,
                                           QuaternaryUnitId = b.QuaternaryUnitId,
                                           UnitShortName = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.UnitShortName).FirstOrDefault(),
                                           SecondaryUnitShortName = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitShortName).FirstOrDefault(),
                                           TertiaryUnitShortName = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitShortName).FirstOrDefault(),
                                           QuaternaryUnitShortName = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitShortName).FirstOrDefault(),
                                           UToSValue = b.UToSValue,
                                           SToTValue = b.SToTValue,
                                           TToQValue = b.TToQValue,
                                           PriceAddedFor = b.PriceAddedFor,
                                           AllowDecimal = oConnectionContext.DbClsUnit.Where(d => d.UnitId == b.UnitId).Select(d => d.AllowDecimal).FirstOrDefault(),
                                           SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(d => d.SecondaryUnitId == b.SecondaryUnitId).Select(d => d.SecondaryUnitAllowDecimal).FirstOrDefault(),
                                           TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(d => d.TertiaryUnitId == b.TertiaryUnitId).Select(d => d.TertiaryUnitAllowDecimal).FirstOrDefault(),
                                           QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(d => d.QuaternaryUnitId == b.QuaternaryUnitId).Select(d => d.QuaternaryUnitAllowDecimal).FirstOrDefault(),
                                       }).ToList();

                    data = new
                    {
                        Status = 1,
                        Message = "found",
                        Data = new
                        {
                            ItemDetails = ItemDetails,
                        }
                    };
                }
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CheckSku(ClsItemVm obj)
        {
            if (obj.Type == "insert")
            {
                if (oConnectionContext.DbClsItem.Where(a => a.SkuCode == obj.ItemCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Duplicate SKU exists",
                        Data = new
                        {
                        }
                    }; return await Task.FromResult(Ok(data));
                }

                if (oConnectionContext.DbClsItemDetails.Where(a => a.SKU == obj.ItemCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Duplicate SKU exists",
                        Data = new
                        {
                        }
                    }; return await Task.FromResult(Ok(data));
                }
            }
            else
            {
                if (oConnectionContext.DbClsItem.Where(a => a.SkuCode == obj.ItemCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false
                && a.ItemId != obj.ItemId).Count() > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "SKU already exists",
                        Data = new
                        {
                        }
                    }; return await Task.FromResult(Ok(data));
                }

                if (oConnectionContext.DbClsItemDetails.Where(a => a.SKU == obj.ItemCode && a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
                a.ItemDetailsId != obj.ItemId).Count() > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "SKU already exists",
                        Data = new
                        {
                        }
                    }; return await Task.FromResult(Ok(data));
                }
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {

                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> itemDetailsDelete(ClsItemDetailsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.ItemId != 0 && obj.VariationId != 0)
                {
                    string query = "update \"tblitemdetails\" set \"IsDeleted\"=True where \"ItemId\"=" + obj.ItemId + " and \"VariationId\"=" + obj.VariationId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                else if (obj.ItemId != 0 && obj.VariationId == 0)
                {
                    string query = "update \"tblitemdetails\" set \"IsDeleted\"=True where \"ItemId\"=" + obj.ItemId;
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }
                else
                {
                    string query = "update \"tblItemDetails\" set \"IsDeleted\"=True where \"ItemDetailsId\"=" + obj.ItemDetailsId;
                    //ConnectionContext ocon = new ConnectionContext();
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

        public async Task<IHttpActionResult> AvailableBranches(ClsItem obj)
        {
            var Branchs = (from a in oConnectionContext.DbClsItemBranchMap
                           join b in oConnectionContext.DbClsBranch
                           on a.BranchId equals b.BranchId
                           where a.ItemId == obj.ItemId && a.IsDeleted == false && a.IsActive == true
                           && b.IsDeleted == false && b.IsActive == true
                           select new
                           {
                               BranchId = a.BranchId,
                               Branch = b.Branch
                           }).Distinct().ToList();
            //var Branchs = oConnectionContext.DbClsItemBranchMap.Where(a => a.ItemId == obj.ItemId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
            //{
            //    BranchId = a.BranchId,
            //    Branch = oConnectionContext.DbClsBranch.Where(aa => aa.BranchId == a.BranchId).Select(aa => aa.Branch).FirstOrDefault()
            //}).Distinct().ToList();

            var ItemDetails = (from b in oConnectionContext.DbClsItemDetails
                               join d in oConnectionContext.DbClsItem
                               on b.ItemId equals d.ItemId
                               where b.ItemId == obj.ItemId && b.IsDeleted == false
                               && d.IsDeleted == false
                               //&& d.ProductType.ToLower() == "variable"
                               select new
                               {
                                   //Notes = "",
                                   //OpeningStockDetailsId = 0,
                                   //Quantity = 0,
                                   //UnitCost = b.SalesExcTax,
                                   //d.ItemId,
                                   //SubTotal = 0,
                                   d.ProductType,
                                   b.ItemDetailsId,
                                   //d.ItemName,
                                   b.SKU,
                                   //b.VariationDetailsId,
                                   VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                   //UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                   //b.TotalCost,
                                   //d.ItemCode
                               }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Branchs = Branchs,
                    ItemDetails = ItemDetails
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemsPos(ClsItemBranchMapVm obj)
        {
            var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).ToList().Select(a => new
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

            var StockNotManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                       //join a in oConnectionContext.DbClsItemDetails
                                       //on c.ItemDetailsId equals a.ItemDetailsId
                                   join b in oConnectionContext.DbClsItem on c.ItemId equals b.ItemId
                                   where b.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true
                                   //&& a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                   && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false
                                   && b.ProductType.ToLower() != "variable"
                                   select new
                                   {
                                       ItemName = b.ItemName,
                                       SKU = b.SkuCode,
                                       Quantity = c.Quantity,
                                       ProductImage = b.ProductImage == null ? "/Content/assets/img/item.jpg" : b.ProductImage,
                                       ProductBrochure = b.ProductBrochure == null ? "/Content/assets/img/item.jpg" : b.ProductBrochure,
                                       ItemId = b.ItemId,
                                       ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == b.ItemId && a.IsActive == true && a.IsDeleted == false).Select(a => a.ItemDetailsId).FirstOrDefault(),
                                       VariationDetailsId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == b.ItemId && a.IsActive == true && a.IsDeleted == false).Select(a => a.VariationDetailsId).FirstOrDefault(),
                                       VariationName = "",
                                       BrandId = b.BrandId,
                                       CategoryId = b.CategoryId,
                                       SubCategoryId = b.SubCategoryId,
                                       SubSubCategoryId = b.SubSubCategoryId,
                                   }).ToList();

            var variable_StockNotManaged = (
                             from c in oConnectionContext.DbClsItemBranchMap
                             join a in oConnectionContext.DbClsItemDetails
                             on c.ItemDetailsId equals a.ItemDetailsId
                             join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                             where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true && a.IsActive == true && a.IsDeleted == false
                             && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == false
                             select new
                             {
                                 ItemName = b.ItemName,
                                 SKU = a.SKU,
                                 Quantity = c.Quantity,
                                 ProductImage = b.ProductImage == null ? "/Content/assets/img/item.jpg" : b.ProductImage,
                                 ProductBrochure = b.ProductBrochure == null ? "/Content/assets/img/item.jpg" : b.ProductBrochure,
                                 ItemId = b.ItemId,
                                 ItemDetailsId = a.ItemDetailsId,
                                 VariationDetailsId = a.VariationDetailsId,
                                 VariationName = oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                             ).Select(c => c.VariationDetails).FirstOrDefault(),
                                 BrandId = b.BrandId,
                                 CategoryId = b.CategoryId,
                                 SubCategoryId = b.SubCategoryId,
                                 SubSubCategoryId = b.SubSubCategoryId,
                             }).ToList();

            var StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                    // join a in oConnectionContext.DbClsItemDetails
                                    //on c.ItemDetailsId equals a.ItemDetailsId
                                join b in oConnectionContext.DbClsItem on c.ItemId equals b.ItemId
                                where b.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true
                                //a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                               && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                               && c.Quantity > 0 && b.ProductType.ToLower() != "variable"
                                select new
                                {
                                    ItemName = b.ItemName,
                                    SKU = b.SkuCode,
                                    Quantity = c.Quantity,
                                    ProductImage = b.ProductImage == null ? "/Content/assets/img/item.jpg" : b.ProductImage,
                                    ProductBrochure = b.ProductBrochure == null ? "/Content/assets/img/item.jpg" : b.ProductBrochure,
                                    ItemId = b.ItemId,
                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == b.ItemId && a.IsActive == true && a.IsDeleted == false).Select(a => a.ItemDetailsId).FirstOrDefault(),
                                    VariationDetailsId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemId == b.ItemId && a.IsActive == true && a.IsDeleted == false).Select(a => a.VariationDetailsId).FirstOrDefault(),
                                    VariationName = "",
                                    BrandId = b.BrandId,
                                    CategoryId = b.CategoryId,
                                    SubCategoryId = b.SubCategoryId,
                                    SubSubCategoryId = b.SubSubCategoryId,
                                }).ToList();

            var variable_StockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                         join a in oConnectionContext.DbClsItemDetails
                                        on c.ItemDetailsId equals a.ItemDetailsId
                                         join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                         where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive == true &&
                                         a.IsActive == true && a.IsDeleted == false
                                        && b.ProductType.ToLower() == "variable" && b.IsActive == true && b.IsDeleted == false && b.IsManageStock == true
                                        && c.Quantity > 0
                                         select new
                                         {
                                             ItemName = b.ItemName,
                                             SKU = a.SKU,
                                             Quantity = c.Quantity,
                                             ProductImage = b.ProductImage == null ? "/Content/assets/img/item.jpg" : b.ProductImage,
                                             ProductBrochure = b.ProductBrochure == null ? "/Content/assets/img/item.jpg" : b.ProductBrochure,
                                             ItemId = b.ItemId,
                                             ItemDetailsId = a.ItemDetailsId,
                                             VariationDetailsId = a.VariationDetailsId,
                                             VariationName = oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId
                            ).Select(c => c.VariationDetails).FirstOrDefault(),
                                             BrandId = b.BrandId,
                                             CategoryId = b.CategoryId,
                                             SubCategoryId = b.SubCategoryId,
                                             SubSubCategoryId = b.SubSubCategoryId,
                                         }).ToList();


            var ItemDetails = StockNotManaged.Union(variable_StockNotManaged).Union(StockManaged).Union(variable_StockManaged).Distinct().ToList();

            if (obj.BrandId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.BrandId == obj.BrandId).ToList();
            }
            if (obj.CategoryId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.CategoryId == obj.CategoryId).ToList();
            }
            if (obj.SubCategoryId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.SubCategoryId == obj.SubCategoryId).ToList();
            }
            if (obj.SubSubCategoryId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ItemDetails = ItemDetails,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemsBranchWise(ClsItemVm obj)
        {
            var Items = (from a in oConnectionContext.DbClsItemBranchMap
                         join b in oConnectionContext.DbClsItemDetails
                         on a.ItemDetailsId equals b.ItemDetailsId
                         join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                         where a.BranchId == obj.BranchId && c.ProductType != "Combo"
                         select new
                         {
                             b.ItemId,
                             a.ItemDetailsId,
                             c.ItemName,
                             SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                             VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                         }).Union((from a in oConnectionContext.DbClsItem
                                   where a.ProductType == "Combo" &&
oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
                                   select new
                                   {
                                       a.ItemId,
                                       ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                       a.ItemName,
                                       SKU = a.SkuCode,
                                       VariationName = ""
                                   })).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Items = Items,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockReport(ClsItemVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsItemDetailsVm> ItemDetails;
            if (obj.BranchId == 0)
            {
                ItemDetails = (from a in oConnectionContext.DbClsItemBranchMap
                               join b in oConnectionContext.DbClsItemDetails
                               on a.ItemDetailsId equals b.ItemDetailsId
                               join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                               where
                               //a.BranchId == obj.BranchId && 
                               oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                               c.ProductType != "Combo"
                               && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                               && c.IsManageStock == true
                               select new ClsItemDetailsVm
                               {
                                   BranchName = oConnectionContext.DbClsBranch.Where(cc => cc.BranchId == a.BranchId).Select(cc => cc.Branch).FirstOrDefault(),
                                   IsManageStock = c.IsManageStock,
                                   BranchId = a.BranchId,
                                   BrandId = c.BrandId,
                                   CategoryId = c.CategoryId,
                                   SubCategoryId = c.SubCategoryId,
                                   SubSubCategoryId = c.SubSubCategoryId,
                                   VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                   ItemId = b.ItemId,
                                   ItemDetailsId = a.ItemDetailsId,
                                   ItemName = c.ItemName,
                                   UnitCost = b.SalesIncTax != 0 ? b.SalesIncTax : b.SalesExcTax,
                                   SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                   Quantity = a.Quantity,
                                   //UnitName = c.QuaternaryUnitId != 0 ? oConnectionContext.DbClsQuaternaryUnit.Where(bb => bb.QuaternaryUnitId == c.QuaternaryUnitId).Select(bb => bb.QuaternaryUnitName).FirstOrDefault()
                                   //: c.TertiaryUnitId != 0 ? oConnectionContext.DbClsTertiaryUnit.Where(bb => bb.TertiaryUnitId == c.TertiaryUnitId).Select(bb => bb.TertiaryUnitName).FirstOrDefault()
                                   //: c.SecondaryUnitId != 0 ? oConnectionContext.DbClsSecondaryUnit.Where(bb => bb.SecondaryUnitId == c.SecondaryUnitId).Select(bb => bb.SecondaryUnitName).FirstOrDefault()
                                   //: oConnectionContext.DbClsUnit.Where(bb => bb.UnitId == c.UnitId).Select(bb => bb.UnitName).FirstOrDefault(),
                                   StockValueByPurchasePrice = b.PurchaseExcTax != 0 ? a.Quantity * b.PurchaseExcTax : a.Quantity * b.PurchaseIncTax,
                                   StockValueBySalesPrice = b.SalesExcTax != 0 ? a.Quantity * b.SalesExcTax : a.Quantity * b.SalesIncTax,
                                   PotentialProfit = 0,
                                   TotalUnitSold = (from d in oConnectionContext.DbClsSales
                                                    join e in oConnectionContext.DbClsSalesDetails
           on d.SalesId equals e.SalesId
                                                    where d.Status != "Draft" && e.ItemDetailsId == a.ItemDetailsId && d.IsDeleted == false && d.IsCancelled == false
                                                    && e.IsDeleted == false && d.CompanyId == obj.CompanyId &&
                                                    oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                                    select e.QuantitySold).DefaultIfEmpty().Sum() -
                                                    (from e in oConnectionContext.DbClsSalesReturn
                                                     join f in oConnectionContext.DbClsSalesReturnDetails
                                                     on e.SalesReturnId equals f.SalesReturnId
                                                     join d in oConnectionContext.DbClsSales on e.SalesId equals d.SalesId
                                                     where e.CompanyId == obj.CompanyId && f.ItemDetailsId == a.ItemDetailsId
                                                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                                     && e.IsDeleted == false && e.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && f.IsDeleted == false
                                                     select f.QuantityReturned).DefaultIfEmpty().Sum(),
                                   TotalUnitTransferred = (from d in oConnectionContext.DbClsStockTransfer
                                                           join e in oConnectionContext.DbClsStockTransferDetails
                  on d.StockTransferId equals e.StockTransferId
                                                           where e.ItemDetailsId == a.ItemDetailsId && d.IsDeleted == false && e.IsDeleted == false && d.CompanyId == obj.CompanyId
                                                           &&
                                                           //d.FromBranchId == obj.BranchId
                                                           oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.FromBranchId)
                                                           select e.QuantityTransferred).DefaultIfEmpty().Sum(),
                                   TotalUnitAdjusted = (from d in oConnectionContext.DbClsStockAdjustment
                                                        join e in oConnectionContext.DbClsStockAdjustmentDetails
               on d.StockAdjustmentId equals e.StockAdjustmentId
                                                        where e.ItemDetailsId == a.ItemDetailsId && d.IsDeleted == false && e.IsDeleted == false && d.CompanyId == obj.CompanyId
                                                        &&
                                                        //d.BranchId == obj.BranchId
                                                        oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                                        select e.QuantityAdjusted).DefaultIfEmpty().Sum(),
                               }).ToList();
            }
            else
            {
                ItemDetails = (from a in oConnectionContext.DbClsItemBranchMap
                               join b in oConnectionContext.DbClsItemDetails
                               on a.ItemDetailsId equals b.ItemDetailsId
                               join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                               where a.BranchId == obj.BranchId && c.ProductType != "Combo"
                               && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                               && c.IsManageStock == true
                               select new ClsItemDetailsVm
                               {
                                   BranchName = oConnectionContext.DbClsBranch.Where(cc => cc.BranchId == a.BranchId).Select(cc => cc.Branch).FirstOrDefault(),
                                   IsManageStock = c.IsManageStock,
                                   BranchId = a.BranchId,
                                   BrandId = c.BrandId,
                                   CategoryId = c.CategoryId,
                                   SubCategoryId = c.SubCategoryId,
                                   SubSubCategoryId = c.SubSubCategoryId,
                                   VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                   ItemId = b.ItemId,
                                   ItemDetailsId = a.ItemDetailsId,
                                   ItemName = c.ItemName,
                                   UnitCost = b.SalesIncTax != 0 ? b.SalesIncTax : b.SalesExcTax,
                                   SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                   Quantity = a.Quantity,
                                   //UnitName = c.QuaternaryUnitId != 0 ? oConnectionContext.DbClsQuaternaryUnit.Where(bb => bb.QuaternaryUnitId == c.QuaternaryUnitId).Select(bb => bb.QuaternaryUnitName).FirstOrDefault()
                                   //: c.TertiaryUnitId != 0 ? oConnectionContext.DbClsTertiaryUnit.Where(bb => bb.TertiaryUnitId == c.TertiaryUnitId).Select(bb => bb.TertiaryUnitName).FirstOrDefault()
                                   //: c.SecondaryUnitId != 0 ? oConnectionContext.DbClsSecondaryUnit.Where(bb => bb.SecondaryUnitId == c.SecondaryUnitId).Select(bb => bb.SecondaryUnitName).FirstOrDefault()
                                   //: oConnectionContext.DbClsUnit.Where(bb => bb.UnitId == c.UnitId).Select(bb => bb.UnitName).FirstOrDefault(),
                                   StockValueByPurchasePrice = b.PurchaseExcTax != 0 ? a.Quantity * b.PurchaseExcTax : a.Quantity * b.PurchaseIncTax,
                                   StockValueBySalesPrice = b.SalesExcTax != 0 ? a.Quantity * b.SalesExcTax : a.Quantity * b.SalesIncTax,
                                   PotentialProfit = 0,
                                   TotalUnitSold = (from d in oConnectionContext.DbClsSales
                                                    join e in oConnectionContext.DbClsSalesDetails
           on d.SalesId equals e.SalesId
                                                    where e.ItemDetailsId == a.ItemDetailsId && d.IsDeleted == false && d.IsCancelled == false && e.IsDeleted == false && d.CompanyId == obj.CompanyId
                                                    && d.BranchId == obj.BranchId
                                                    select e.QuantitySold).DefaultIfEmpty().Sum() -
                                                    (from e in oConnectionContext.DbClsSalesReturn
                                                     join f in oConnectionContext.DbClsSalesReturnDetails
                                                     on e.SalesReturnId equals f.SalesReturnId
                                                     join d in oConnectionContext.DbClsSales on e.SalesId equals d.SalesId
                                                     where e.CompanyId == obj.CompanyId && f.ItemDetailsId == a.ItemDetailsId
                                                     && d.BranchId == obj.BranchId
                                                     && e.IsDeleted == false && e.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && f.IsDeleted == false
                                                     select f.QuantityReturned).DefaultIfEmpty().Sum(),
                                   TotalUnitTransferred = (from d in oConnectionContext.DbClsStockTransfer
                                                           join e in oConnectionContext.DbClsStockTransferDetails
                  on d.StockTransferId equals e.StockTransferId
                                                           where e.ItemDetailsId == a.ItemDetailsId && d.IsDeleted == false && e.IsDeleted == false && d.CompanyId == obj.CompanyId
                                                           && d.FromBranchId == obj.BranchId
                                                           select e.QuantityTransferred).DefaultIfEmpty().Sum(),
                                   TotalUnitAdjusted = (from d in oConnectionContext.DbClsStockAdjustment
                                                        join e in oConnectionContext.DbClsStockAdjustmentDetails
               on d.StockAdjustmentId equals e.StockAdjustmentId
                                                        where e.ItemDetailsId == a.ItemDetailsId && d.IsDeleted == false && e.IsDeleted == false && d.CompanyId == obj.CompanyId
                                                        && d.BranchId == obj.BranchId
                                                        select e.QuantityAdjusted).DefaultIfEmpty().Sum(),
                               }).ToList();
            }

            if (obj.BrandId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.BrandId == obj.BrandId).ToList();
            }

            if (obj.CategoryId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.CategoryId == obj.CategoryId).ToList();
            }

            if (obj.SubCategoryId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.SubCategoryId == obj.SubCategoryId).ToList();
            }

            if (obj.SubSubCategoryId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).ToList();
            }

            if (obj.ItemDetailsId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.ItemDetailsId == obj.ItemDetailsId).ToList();
            }

            decimal StockValueBySalesPrice = ItemDetails.Select(a => a.StockValueBySalesPrice).DefaultIfEmpty().Sum();
            decimal StockValueByPurchasePrice = ItemDetails.Select(a => a.StockValueByPurchasePrice).DefaultIfEmpty().Sum();
            decimal PotentialProfit = StockValueBySalesPrice - StockValueByPurchasePrice;
            decimal ProfitMargin = PotentialProfit == 0 ? 0 : (PotentialProfit / StockValueBySalesPrice) * 100;

            List<ClsItemDetailsVm> _ItemDetails1 = new List<ClsItemDetailsVm>();
            List<ClsItemDetailsVm> _ItemDetails2 = new List<ClsItemDetailsVm>();

            _ItemDetails1 = ItemDetails.OrderByDescending(a => a.ItemDetailsId).Skip(skip).Take(obj.PageSize).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            foreach (var item in _ItemDetails1)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
                }).FirstOrDefault();

                decimal TotalCurrentStock = item.Quantity;
                decimal TotalUnitSold = item.TotalUnitSold;
                decimal TotalUnitTransferred = item.TotalUnitTransferred;
                decimal TotalUnitAdjusted = item.TotalUnitAdjusted;

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
                        TotalUnitSold = TotalUnitSold / conversionRates.UToSValue;
                        TotalUnitTransferred = TotalUnitTransferred / conversionRates.UToSValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.UToSValue;
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
                        TotalUnitSold = TotalUnitSold / conversionRates.UToSValue / conversionRates.SToTValue;
                        TotalUnitTransferred = TotalUnitTransferred / conversionRates.UToSValue / conversionRates.SToTValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.UToSValue / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue;
                        TotalUnitSold = TotalUnitSold / conversionRates.SToTValue;
                        TotalUnitTransferred = TotalUnitTransferred / conversionRates.SToTValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.SToTValue;
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
                        TotalUnitSold = TotalUnitSold / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitTransferred = TotalUnitTransferred / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitSold = TotalUnitSold / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitTransferred = TotalUnitTransferred / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.TToQValue;
                        TotalUnitSold = TotalUnitSold / conversionRates.TToQValue;
                        TotalUnitTransferred = TotalUnitTransferred / conversionRates.TToQValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 4)
                    {
                        //TotalCurrentStock= TotalCurrentStock;
                    }
                }

                _ItemDetails2.Add(new ClsItemDetailsVm
                {
                    BranchName = item.BranchName,
                    IsManageStock = item.IsManageStock,
                    BranchId = item.BranchId,
                    BrandId = item.BrandId,
                    CategoryId = item.CategoryId,
                    SubCategoryId = item.SubCategoryId,
                    SubSubCategoryId = item.SubSubCategoryId,
                    VariationName = item.VariationName,
                    ItemId = item.ItemId,
                    ItemDetailsId = item.ItemDetailsId,
                    ItemName = item.ItemName,
                    UnitCost = item.UnitCost,
                    SKU = item.SKU,
                    Quantity = TotalCurrentStock,
                    UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit,
                    StockValueByPurchasePrice = item.StockValueByPurchasePrice,
                    StockValueBySalesPrice = item.StockValueBySalesPrice,
                    PotentialProfit = 0,
                    TotalUnitSold = TotalUnitSold,
                    TotalUnitTransferred = TotalUnitTransferred,
                    TotalUnitAdjusted = TotalUnitAdjusted
                });
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    StockValueBySalesPrice = StockValueBySalesPrice,
                    StockValueByPurchasePrice = StockValueByPurchasePrice,
                    PotentialProfit = PotentialProfit,
                    ProfitMargin = ProfitMargin,
                    ItemDetails = _ItemDetails2,
                    TotalCount = ItemDetails.Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockDetailsReport(ClsItemDetailsVm obj)
        {
            dynamic userDetails = null;
            dynamic ItemDetails = null;
            if (obj.ItemDetailsId == 0)
            {
                userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => new
                {
                    a.IsCompany,
                    a.UserRoleId,
                    BranchIds = oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
                   && b.IsDeleted == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
                }).FirstOrDefault();

                if (obj.BranchId == 0)
                {
                    obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
                }

                ItemDetails = (from a in oConnectionContext.DbClsItemBranchMap
                               join b in oConnectionContext.DbClsItemDetails
                               on a.ItemDetailsId equals b.ItemDetailsId
                               join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                               where a.BranchId == obj.BranchId && c.ProductType != "Combo" && a.ItemId == obj.ItemId
                               && a.IsActive == true && a.IsDeleted == false && b.IsActive == true && b.IsDeleted == false
                               select new
                               {
                                   b.ItemId,
                                   a.ItemDetailsId,
                                   c.ItemName,
                                   SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                   b.VariationDetailsId,
                                   VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                   c.ItemCode
                               }).Union((from a in oConnectionContext.DbClsItem
                                         join b in oConnectionContext.DbClsItemDetails
                              on a.ItemId equals b.ItemId
                                         where a.ProductType == "Combo" && a.ItemId == obj.ItemId &&
      oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
      && a.IsActive == true && a.IsDeleted == false && b.IsActive == true && b.IsDeleted == false
                                         select new
                                         {
                                             b.ItemId,
                                             b.ItemDetailsId,
                                             a.ItemName,
                                             SKU = a.ProductType == "Single" ? a.SkuCode : b.SKU,
                                             b.VariationDetailsId,
                                             VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                             a.ItemCode
                                         })).ToList();

                obj.ItemDetailsId = ItemDetails[0].ItemDetailsId;
            }

            var conversionRates = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => new
            {
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                PrimaryUnitAllowDecimal = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.AllowDecimal).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitAllowDecimal).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitAllowDecimal).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
                QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitAllowDecimal).FirstOrDefault(),
            }).FirstOrDefault();

            decimal TotalCurrentStock = oConnectionContext.DbClsItemBranchMap.Where(a => a.BranchId == obj.BranchId &&
            a.ItemId == obj.ItemId && a.ItemDetailsId == obj.ItemDetailsId).Select(a => a.Quantity).FirstOrDefault();

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
                    TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue; ;
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

            var purchaseStocksQty = (from a in oConnectionContext.DbClsPurchase
                                     join b in oConnectionContext.DbClsPurchaseDetails
                                      on a.PurchaseId equals b.PurchaseId
                                     where a.BranchId == obj.BranchId
                                     && b.ItemDetailsId == obj.ItemDetailsId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                     && b.IsActive == true && b.IsDeleted == false
                                     && a.Status.ToLower() != "draft" && a.IsCancelled == false
                                     select new ClsStockDetails
                                     {
                                         Balance = 0,
                                         //IsDebit = false,
                                         Date = a.PurchaseDate,
                                         ReferenceNo = a.ReferenceNo,
                                         Type = "Purchase",
                                         Quantity = b.Quantity,
                                         Credit = b.Quantity + b.FreeQuantity,
                                         Debit = 0,
                                         Id = b.PurchaseDetailsId,
                                         ItemId = b.ItemId,
                                         PrimaryCredit = 0,
                                         SecondaryCredit = 0,
                                         TertiaryCredit = 0,
                                         QuaternaryCredit = 0,
                                         PrimaryDebit = 0,
                                         SecondaryDebit = 0,
                                         TertiaryDebit = 0,
                                         QuaternaryDebit = 0,
                                         PrimaryBalance = 0,
                                         SecondaryBalance = 0,
                                         TertiaryBalance = 0,
                                         QuaternaryBalance = 0,
                                         PriceAddedFor = b.PriceAddedFor,
                                         PrimaryUnitAllowDecimal = false,
                                         SecondaryUnitAllowDecimal = false,
                                         TertiaryUnitAllowDecimal = false,
                                         QuaternaryUnitAllowDecimal = false,
                                     }).ToList();

            var purchaseExpiryStocksQty = (from a in oConnectionContext.DbClsPurchase
                                           join b in oConnectionContext.DbClsPurchaseDetails
                                            on a.PurchaseId equals b.PurchaseId
                                           where a.BranchId == obj.BranchId
                                           && b.ItemDetailsId == obj.ItemDetailsId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                           && b.IsActive == true && b.IsDeleted == false
                                           && a.Status.ToLower() != "draft" && b.IsStopSelling == true && a.IsCancelled == false
                                           select new ClsStockDetails
                                           {
                                               Balance = 0,
                                               //IsDebit = false,
                                               Date = b.ExpiryDate.Value,
                                               ReferenceNo = a.ReferenceNo,
                                               Type = "Stock Expired",
                                               Quantity = b.QuantityRemaining,
                                               Credit = 0,
                                               Debit = b.QuantityRemaining,
                                               Id = b.PurchaseDetailsId,
                                               ItemId = b.ItemId,
                                               PrimaryCredit = 0,
                                               SecondaryCredit = 0,
                                               TertiaryCredit = 0,
                                               QuaternaryCredit = 0,
                                               PrimaryDebit = 0,
                                               SecondaryDebit = 0,
                                               TertiaryDebit = 0,
                                               QuaternaryDebit = 0,
                                               PrimaryBalance = 0,
                                               SecondaryBalance = 0,
                                               TertiaryBalance = 0,
                                               QuaternaryBalance = 0,
                                               PriceAddedFor = b.PriceAddedFor,
                                               PrimaryUnitAllowDecimal = false,
                                               SecondaryUnitAllowDecimal = false,
                                               TertiaryUnitAllowDecimal = false,
                                               QuaternaryUnitAllowDecimal = false
                                           }).ToList();

            var purchaseReturnStocksQty = (from c in oConnectionContext.DbClsPurchase
                                           join a in oConnectionContext.DbClsPurchaseReturn
                                                on c.PurchaseId equals a.PurchaseId
                                           join b in oConnectionContext.DbClsPurchaseReturnDetails
                                            on a.PurchaseReturnId equals b.PurchaseReturnId
                                           where a.BranchId == obj.BranchId
                                           && b.ItemDetailsId == obj.ItemDetailsId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                     && b.IsActive == true && b.IsDeleted == false && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                     && c.Status.ToLower() != "draft" && c.IsCancelled == false
                                           select new ClsStockDetails
                                           {
                                               Balance = 0,
                                               //IsDebit = true,
                                               Date = a.Date,
                                               ReferenceNo = a.InvoiceNo,
                                               Type = "Purchase Return",
                                               Quantity = b.Quantity,
                                               Id = b.PurchaseReturnDetailsId,
                                               Credit = 0,
                                               Debit = b.Quantity + b.FreeQuantity,
                                               ItemId = b.ItemId,
                                               PrimaryCredit = 0,
                                               SecondaryCredit = 0,
                                               TertiaryCredit = 0,
                                               QuaternaryCredit = 0,
                                               PrimaryDebit = 0,
                                               SecondaryDebit = 0,
                                               TertiaryDebit = 0,
                                               QuaternaryDebit = 0,
                                               PrimaryBalance = 0,
                                               SecondaryBalance = 0,
                                               TertiaryBalance = 0,
                                               QuaternaryBalance = 0,
                                               PriceAddedFor = b.PriceAddedFor,
                                               PrimaryUnitAllowDecimal = false,
                                               SecondaryUnitAllowDecimal = false,
                                               TertiaryUnitAllowDecimal = false,
                                               QuaternaryUnitAllowDecimal = false,
                                           }).ToList();

            var purchaseReturnNoParentStocksQty = (from a in oConnectionContext.DbClsPurchaseReturn
                                                   join b in oConnectionContext.DbClsPurchaseReturnDetails
                                                    on a.PurchaseReturnId equals b.PurchaseReturnId
                                                   where a.BranchId == obj.BranchId
                                                   && b.ItemDetailsId == obj.ItemDetailsId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                             && b.IsActive == true && b.IsDeleted == false && a.PurchaseId == 0 && a.IsCancelled == false
                                                   select new ClsStockDetails
                                                   {
                                                       Balance = 0,
                                                       //IsDebit = true,
                                                       Date = a.Date,
                                                       ReferenceNo = a.InvoiceNo,
                                                       Type = "Purchase Return",
                                                       Quantity = b.Quantity,
                                                       Id = b.PurchaseReturnDetailsId,
                                                       Credit = 0,
                                                       Debit = b.Quantity + b.FreeQuantity,
                                                       ItemId = b.ItemId,
                                                       PrimaryCredit = 0,
                                                       SecondaryCredit = 0,
                                                       TertiaryCredit = 0,
                                                       QuaternaryCredit = 0,
                                                       PrimaryDebit = 0,
                                                       SecondaryDebit = 0,
                                                       TertiaryDebit = 0,
                                                       QuaternaryDebit = 0,
                                                       PrimaryBalance = 0,
                                                       SecondaryBalance = 0,
                                                       TertiaryBalance = 0,
                                                       QuaternaryBalance = 0,
                                                       PriceAddedFor = b.PriceAddedFor,
                                                       PrimaryUnitAllowDecimal = false,
                                                       SecondaryUnitAllowDecimal = false,
                                                       TertiaryUnitAllowDecimal = false,
                                                       QuaternaryUnitAllowDecimal = false,
                                                   }).ToList();

            var openingStocksQty = (from a in oConnectionContext.DbClsOpeningStock
                                    where a.BranchId == obj.BranchId && a.ItemDetailsId == obj.ItemDetailsId &&
                                    a.IsActive == true && a.IsDeleted == false
                                     && a.Quantity > 0
                                    select new ClsStockDetails
                                    {
                                        Balance = 0,
                                        //IsDebit = false,
                                        Date = a.Date,
                                        ReferenceNo = "",
                                        Type = "Opening Stock",
                                        Quantity = a.Quantity,
                                        Id = a.OpeningStockId,
                                        Credit = a.Quantity,
                                        Debit = 0,
                                        ItemId = a.ItemId,
                                        PrimaryCredit = 0,
                                        SecondaryCredit = 0,
                                        TertiaryCredit = 0,
                                        QuaternaryCredit = 0,
                                        PrimaryDebit = 0,
                                        SecondaryDebit = 0,
                                        TertiaryDebit = 0,
                                        QuaternaryDebit = 0,
                                        PrimaryBalance = 0,
                                        SecondaryBalance = 0,
                                        TertiaryBalance = 0,
                                        QuaternaryBalance = 0,
                                        PriceAddedFor = a.PriceAddedFor,
                                        PrimaryUnitAllowDecimal = false,
                                        SecondaryUnitAllowDecimal = false,
                                        TertiaryUnitAllowDecimal = false,
                                        QuaternaryUnitAllowDecimal = false
                                    }).ToList();

            var openingExpiryStocksQty = (from a in oConnectionContext.DbClsOpeningStock
                                          where a.BranchId == obj.BranchId && a.ItemDetailsId == obj.ItemDetailsId && a.IsActive == true
                                          && a.IsDeleted == false && a.IsStopSelling == true
                                           && a.QuantityRemaining > 0
                                          select new ClsStockDetails
                                          {
                                              Balance = 0,
                                              //IsDebit = false,
                                              Date = a.ExpiryDate.Value,
                                              ReferenceNo = "",
                                              Type = "Stock Expired",
                                              Quantity = a.QuantityRemaining,
                                              Id = a.OpeningStockId,
                                              Credit = 0,
                                              Debit = a.QuantityRemaining,
                                              ItemId = a.ItemId,
                                              PrimaryCredit = 0,
                                              SecondaryCredit = 0,
                                              TertiaryCredit = 0,
                                              QuaternaryCredit = 0,
                                              PrimaryDebit = 0,
                                              SecondaryDebit = 0,
                                              TertiaryDebit = 0,
                                              QuaternaryDebit = 0,
                                              PrimaryBalance = 0,
                                              SecondaryBalance = 0,
                                              TertiaryBalance = 0,
                                              QuaternaryBalance = 0,
                                              PriceAddedFor = a.PriceAddedFor,
                                              PrimaryUnitAllowDecimal = false,
                                              SecondaryUnitAllowDecimal = false,
                                              TertiaryUnitAllowDecimal = false,
                                              QuaternaryUnitAllowDecimal = false
                                          }).ToList();

            var stockAdjustmentQty = (from a in oConnectionContext.DbClsStockAdjustment
                                      join b in oConnectionContext.DbClsStockAdjustmentDetails
                                      on a.StockAdjustmentId equals b.StockAdjustmentId
                                      where a.BranchId == obj.BranchId && b.ItemDetailsId == obj.ItemDetailsId &&
                                      a.IsActive == true && a.IsDeleted == false
                                     && b.IsActive == true && b.IsDeleted == false
                                      select new ClsStockDetails
                                      {
                                          Balance = 0,
                                          //IsDebit = a.AdjustmentType.ToLower() == "credit" ? false : true,
                                          Date = a.AdjustmentDate,
                                          ReferenceNo = a.ReferenceNo,
                                          Type = "Stock Adjustment",
                                          Quantity = b.Quantity,
                                          Id = b.StockAdjustmentDetailsId,
                                          Credit = a.AdjustmentType.ToLower() == "credit" ? b.Quantity : 0,
                                          Debit = a.AdjustmentType.ToLower() == "credit" ? 0 : b.Quantity,
                                          ItemId = b.ItemId,
                                          PrimaryCredit = 0,
                                          SecondaryCredit = 0,
                                          TertiaryCredit = 0,
                                          QuaternaryCredit = 0,
                                          PrimaryDebit = 0,
                                          SecondaryDebit = 0,
                                          TertiaryDebit = 0,
                                          QuaternaryDebit = 0,
                                          PrimaryBalance = 0,
                                          SecondaryBalance = 0,
                                          TertiaryBalance = 0,
                                          QuaternaryBalance = 0,
                                          PriceAddedFor = b.PriceAddedFor,
                                          PrimaryUnitAllowDecimal = false,
                                          SecondaryUnitAllowDecimal = false,
                                          TertiaryUnitAllowDecimal = false,
                                          QuaternaryUnitAllowDecimal = false
                                      }).ToList();

            var salesQty = (from a in oConnectionContext.DbClsSales
                            join b in oConnectionContext.DbClsSalesDetails
                            on a.SalesId equals b.SalesId
                            where a.BranchId == obj.BranchId && b.ItemDetailsId == obj.ItemDetailsId &&
                            a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                           && b.IsActive == true && b.IsDeleted == false && a.Status.ToLower() != "draft"
                           && b.IsComboItems == false && a.IsCancelled == false
                            select new ClsStockDetails
                            {
                                Balance = 0,
                                //IsDebit = true,
                                Date = a.SalesDate,
                                ReferenceNo = a.InvoiceNo,
                                Type = a.SalesType == "pos" ? "Pos" : "Sales",
                                Quantity = b.Quantity,
                                Id = b.SalesDetailsId,
                                Credit = 0,
                                Debit = b.Quantity + b.FreeQuantity,
                                ItemId = b.ItemId,
                                PrimaryCredit = 0,
                                SecondaryCredit = 0,
                                TertiaryCredit = 0,
                                QuaternaryCredit = 0,
                                PrimaryDebit = 0,
                                SecondaryDebit = 0,
                                TertiaryDebit = 0,
                                QuaternaryDebit = 0,
                                PrimaryBalance = 0,
                                SecondaryBalance = 0,
                                TertiaryBalance = 0,
                                QuaternaryBalance = 0,
                                PriceAddedFor = b.PriceAddedFor,
                                PrimaryUnitAllowDecimal = false,
                                SecondaryUnitAllowDecimal = false,
                                TertiaryUnitAllowDecimal = false,
                                QuaternaryUnitAllowDecimal = false,
                            }).ToList();

            var salesComboQty = (from a in oConnectionContext.DbClsSales
                                 join b in oConnectionContext.DbClsSalesDetails
                                 on a.SalesId equals b.SalesId
                                 where a.BranchId == obj.BranchId && b.ItemDetailsId == obj.ItemDetailsId &&
                                 a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                && b.IsActive == true && b.IsDeleted == false && a.Status.ToLower() != "draft"
                                && b.IsComboItems == true && a.IsCancelled == false
                                 select new ClsStockDetails
                                 {
                                     Balance = 0,
                                     //IsDebit = true,
                                     Date = a.SalesDate,
                                     ReferenceNo = a.InvoiceNo,
                                     Type = a.SalesType == "pos" ? "Pos" : "Sales",
                                     Quantity = b.Quantity,
                                     Id = b.SalesDetailsId,
                                     Credit = 0,
                                     Debit = b.Quantity + b.FreeQuantity,
                                     ItemId = b.ItemId,
                                     PrimaryCredit = 0,
                                     SecondaryCredit = 0,
                                     TertiaryCredit = 0,
                                     QuaternaryCredit = 0,
                                     PrimaryDebit = 0,
                                     SecondaryDebit = 0,
                                     TertiaryDebit = 0,
                                     QuaternaryDebit = 0,
                                     PrimaryBalance = 0,
                                     SecondaryBalance = 0,
                                     TertiaryBalance = 0,
                                     QuaternaryBalance = 0,
                                     PriceAddedFor = b.PriceAddedFor,
                                     PrimaryUnitAllowDecimal = false,
                                     SecondaryUnitAllowDecimal = false,
                                     TertiaryUnitAllowDecimal = false,
                                     QuaternaryUnitAllowDecimal = false,
                                 }).ToList();

            var salesReturnQty = (from c in oConnectionContext.DbClsSales
                                  join a in oConnectionContext.DbClsSalesReturn on c.SalesId equals a.SalesId
                                  join b in oConnectionContext.DbClsSalesReturnDetails
                                  on a.SalesReturnId equals b.SalesReturnId
                                  where c.BranchId == obj.BranchId && b.ItemDetailsId == obj.ItemDetailsId &&
                                  a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                 && b.IsActive == true && b.IsDeleted == false && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                 && c.Status.ToLower() != "draft"
                                 && b.IsComboItems == false && a.IsCancelled == false
                                  select new ClsStockDetails
                                  {
                                      Balance = 0,
                                      //IsDebit = false,
                                      Date = a.Date,
                                      ReferenceNo = a.InvoiceNo,
                                      Type = c.SalesType == "pos" ? "Pos Return" : "Sales Return",
                                      Quantity = b.Quantity,
                                      Id = b.SalesReturnDetailsId,
                                      Credit = b.Quantity + b.FreeQuantity,
                                      Debit = 0,
                                      ItemId = b.ItemId,
                                      PrimaryCredit = 0,
                                      SecondaryCredit = 0,
                                      TertiaryCredit = 0,
                                      QuaternaryCredit = 0,
                                      PrimaryDebit = 0,
                                      SecondaryDebit = 0,
                                      TertiaryDebit = 0,
                                      QuaternaryDebit = 0,
                                      PrimaryBalance = 0,
                                      SecondaryBalance = 0,
                                      TertiaryBalance = 0,
                                      QuaternaryBalance = 0,
                                      PriceAddedFor = b.PriceAddedFor,
                                      PrimaryUnitAllowDecimal = false,
                                      SecondaryUnitAllowDecimal = false,
                                      TertiaryUnitAllowDecimal = false,
                                      QuaternaryUnitAllowDecimal = false,
                                  }).ToList();

            var salesReturnComboQty = (from c in oConnectionContext.DbClsSales
                                       join a in oConnectionContext.DbClsSalesReturn on c.SalesId equals a.SalesId
                                       join b in oConnectionContext.DbClsSalesReturnDetails
                                       on a.SalesReturnId equals b.SalesReturnId
                                       where c.BranchId == obj.BranchId && b.ItemDetailsId == obj.ItemDetailsId &&
                                       a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                      && b.IsActive == true && b.IsDeleted == false && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                      && c.Status.ToLower() != "draft" && b.IsComboItems == true && a.IsCancelled == false
                                       select new ClsStockDetails
                                       {
                                           Balance = 0,
                                           //IsDebit = false,
                                           Date = a.Date,
                                           ReferenceNo = a.InvoiceNo,
                                           Type = c.SalesType == "pos" ? "Pos Return" : "Sales Return",
                                           Quantity = b.Quantity,
                                           Id = b.SalesReturnDetailsId,
                                           Credit = b.Quantity + b.FreeQuantity,
                                           Debit = 0,
                                           ItemId = b.ItemId,
                                           PrimaryCredit = 0,
                                           SecondaryCredit = 0,
                                           TertiaryCredit = 0,
                                           QuaternaryCredit = 0,
                                           PrimaryDebit = 0,
                                           SecondaryDebit = 0,
                                           TertiaryDebit = 0,
                                           QuaternaryDebit = 0,
                                           PrimaryBalance = 0,
                                           SecondaryBalance = 0,
                                           TertiaryBalance = 0,
                                           QuaternaryBalance = 0,
                                           PriceAddedFor = b.PriceAddedFor,
                                           PrimaryUnitAllowDecimal = false,
                                           SecondaryUnitAllowDecimal = false,
                                           TertiaryUnitAllowDecimal = false,
                                           QuaternaryUnitAllowDecimal = false,
                                       }).ToList();

            var stockTransferQty = (from a in oConnectionContext.DbClsStockTransfer
                                    join b in oConnectionContext.DbClsStockTransferDetails
                                    on a.StockTransferId equals b.StockTransferId
                                    where a.FromBranchId == obj.BranchId && b.ItemDetailsId == obj.ItemDetailsId && a.IsActive == true
                                    && a.IsDeleted == false && b.IsActive == true && b.IsDeleted == false && a.Status == 3
                                    select new ClsStockDetails
                                    {
                                        Balance = 0,
                                        //IsDebit = false,
                                        Date = a.Date,
                                        ReferenceNo = a.ReferenceNo,
                                        //Type = "Stock Transfer To " + oConnectionContext.DbClsBranch.Where(g => g.BranchId == a.ToBranchId).Select(g => g.Branch).FirstOrDefault(),
                                        Type = "Stock Transfer",
                                        Quantity = b.Quantity,
                                        Id = b.StockTransferDetailsId,
                                        Credit = 0,
                                        Debit = b.Quantity,
                                        ItemId = b.ItemId,
                                        PrimaryCredit = 0,
                                        SecondaryCredit = 0,
                                        TertiaryCredit = 0,
                                        QuaternaryCredit = 0,
                                        PrimaryDebit = 0,
                                        SecondaryDebit = 0,
                                        TertiaryDebit = 0,
                                        QuaternaryDebit = 0,
                                        PrimaryBalance = 0,
                                        SecondaryBalance = 0,
                                        TertiaryBalance = 0,
                                        QuaternaryBalance = 0,
                                        PriceAddedFor = b.PriceAddedFor,
                                        PrimaryUnitAllowDecimal = false,
                                        SecondaryUnitAllowDecimal = false,
                                        TertiaryUnitAllowDecimal = false,
                                        QuaternaryUnitAllowDecimal = false
                                    }).ToList();

            var stockReceivedQty = (from a in oConnectionContext.DbClsStockTransfer
                                    join b in oConnectionContext.DbClsStockTransferDetails
                                    on a.StockTransferId equals b.StockTransferId
                                    where a.ToBranchId == obj.BranchId && b.ItemDetailsId == obj.ItemDetailsId &&
                                    a.IsActive == true && a.IsDeleted == false
                                   && b.IsActive == true && b.IsDeleted == false && a.Status == 3
                                    select new ClsStockDetails
                                    {
                                        Balance = 0,
                                        //IsDebit = false,
                                        Date = a.Date,
                                        ReferenceNo = a.ReferenceNo,
                                        //Type = "Stock Received From " + oConnectionContext.DbClsBranch.Where(g => g.BranchId == a.FromBranchId).Select(g => g.Branch).FirstOrDefault(),
                                        Type = "Stock Received",
                                        Quantity = b.Quantity,
                                        Id = b.StockTransferDetailsId,
                                        Credit = b.Quantity,
                                        Debit = 0,
                                        ItemId = b.ItemId,
                                        PrimaryCredit = 0,
                                        SecondaryCredit = 0,
                                        TertiaryCredit = 0,
                                        QuaternaryCredit = 0,
                                        PrimaryDebit = 0,
                                        SecondaryDebit = 0,
                                        TertiaryDebit = 0,
                                        QuaternaryDebit = 0,
                                        PrimaryBalance = 0,
                                        SecondaryBalance = 0,
                                        TertiaryBalance = 0,
                                        QuaternaryBalance = 0,
                                        PriceAddedFor = b.PriceAddedFor,
                                        PrimaryUnitAllowDecimal = false,
                                        SecondaryUnitAllowDecimal = false,
                                        TertiaryUnitAllowDecimal = false,
                                        QuaternaryUnitAllowDecimal = false
                                    }).ToList();

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var _stock = (purchaseStocksQty.Union(purchaseReturnStocksQty).Union(purchaseReturnNoParentStocksQty).Union(openingStocksQty).Union(stockAdjustmentQty)
                .Union(salesQty).Union(salesComboQty).Union(salesReturnQty).Union(salesReturnComboQty).Union(stockTransferQty).Union(stockReceivedQty)).OrderBy(a => a.Date).ThenBy(a => a.Id);

            var _expiredStock = purchaseExpiryStocksQty.Union(openingExpiryStocksQty).OrderBy(a => a.Date).ThenBy(a => a.Id);

            //decimal PrimaryBalance = 0, SecondaryBalance = 0, TertiaryBalance = 0, QuaternaryBalance = 0;
            foreach (var item in _stock)
            {
                if (conversionRates.UToSValue == 0 && conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    item.PrimaryCredit = item.Credit;
                    item.PrimaryDebit = item.Debit;
                }
                else if (conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    if (item.PriceAddedFor == 4)
                    {
                        item.PrimaryCredit = item.Credit / conversionRates.UToSValue;
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue;

                        item.SecondaryCredit = item.Credit;
                        item.SecondaryDebit = item.Debit;
                    }
                    else if (item.PriceAddedFor == 3)
                    {
                        item.PrimaryCredit = item.Credit;
                        item.PrimaryDebit = item.Debit;

                        item.SecondaryCredit = item.Credit * (1 * conversionRates.UToSValue);
                        item.SecondaryDebit = item.Debit * (1 * conversionRates.UToSValue);
                    }
                }
                else if (conversionRates.TToQValue == 0)
                {
                    if (item.PriceAddedFor == 4)
                    {
                        item.PrimaryCredit = item.Credit / conversionRates.UToSValue / conversionRates.SToTValue;
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue / conversionRates.SToTValue;

                        item.SecondaryCredit = item.Credit / conversionRates.SToTValue;
                        item.SecondaryDebit = item.Debit / conversionRates.SToTValue;

                        item.TertiaryCredit = item.Credit;
                        item.TertiaryDebit = item.Debit;
                    }
                    else if (item.PriceAddedFor == 3)
                    {
                        item.PrimaryCredit = item.Credit / conversionRates.UToSValue; ;
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue;

                        item.SecondaryCredit = item.Credit;
                        item.SecondaryDebit = item.Debit;

                        item.TertiaryCredit = item.Credit * conversionRates.SToTValue;
                        item.TertiaryDebit = item.Debit * conversionRates.SToTValue;
                    }
                    else if (item.PriceAddedFor == 2)
                    {
                        item.PrimaryCredit = item.Credit;
                        item.PrimaryDebit = item.Debit;

                        item.SecondaryCredit = item.Credit * conversionRates.UToSValue;
                        item.SecondaryDebit = item.Debit * conversionRates.UToSValue;

                        item.TertiaryCredit = item.Credit * conversionRates.UToSValue * conversionRates.SToTValue;
                        item.TertiaryDebit = item.Debit * conversionRates.UToSValue * conversionRates.SToTValue;
                    }
                }
                else
                {
                    if (item.PriceAddedFor == 4)
                    {
                        item.PrimaryCredit = item.Credit / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;

                        item.SecondaryCredit = item.Credit / conversionRates.SToTValue / conversionRates.TToQValue;
                        item.SecondaryDebit = item.Debit / conversionRates.SToTValue / conversionRates.TToQValue;

                        item.TertiaryCredit = item.Credit / conversionRates.TToQValue;
                        item.TertiaryDebit = item.Debit / conversionRates.TToQValue;

                        item.QuaternaryCredit = item.Credit;
                        item.QuaternaryDebit = item.Debit;
                    }
                    else if (item.PriceAddedFor == 3)
                    {
                        item.PrimaryCredit = item.Credit / conversionRates.UToSValue / conversionRates.SToTValue;
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue / conversionRates.SToTValue;

                        item.SecondaryCredit = item.Credit / conversionRates.SToTValue;
                        item.SecondaryDebit = item.Debit / conversionRates.SToTValue;

                        item.TertiaryCredit = item.Credit;
                        item.TertiaryDebit = item.Debit;

                        item.QuaternaryCredit = item.Credit * conversionRates.TToQValue;
                        item.QuaternaryDebit = item.Debit * conversionRates.TToQValue;
                    }
                    else if (item.PriceAddedFor == 2)
                    {
                        item.PrimaryCredit = item.Credit / conversionRates.UToSValue;
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue;

                        item.SecondaryCredit = item.Credit;
                        item.SecondaryDebit = item.Debit;

                        item.TertiaryCredit = item.Credit * conversionRates.SToTValue;
                        item.TertiaryDebit = item.Debit * conversionRates.SToTValue;

                        item.QuaternaryCredit = item.Credit * conversionRates.SToTValue * conversionRates.TToQValue;
                        item.QuaternaryDebit = item.Debit * conversionRates.SToTValue * conversionRates.TToQValue;
                    }
                    else if (item.PriceAddedFor == 1)
                    {
                        item.PrimaryCredit = item.Credit;
                        item.PrimaryDebit = item.Debit;

                        item.SecondaryCredit = item.Credit * (1 * conversionRates.UToSValue);
                        item.SecondaryDebit = item.Debit * (1 * conversionRates.UToSValue);

                        item.TertiaryCredit = item.Credit * (1 * conversionRates.UToSValue * conversionRates.SToTValue);
                        item.TertiaryDebit = item.Debit * (1 * conversionRates.UToSValue * conversionRates.SToTValue);

                        item.QuaternaryCredit = item.Credit * (1 * conversionRates.UToSValue * conversionRates.SToTValue * conversionRates.TToQValue);
                        item.QuaternaryDebit = item.Debit * (1 * conversionRates.UToSValue * conversionRates.SToTValue * conversionRates.TToQValue);
                    }
                }


                //if (item.FreeQuantity != 0)
                //{
                //    if (conversionRates.UToSValue == 0 && conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                //    {
                //        item.PrimaryCredit = item.PrimaryCredit + item.FreeQuantity;
                //    }
                //    else if (conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                //    {
                //        if (item.PriceAddedFor == 4)
                //        {
                //            item.PrimaryCredit = item.PrimaryCredit + item.FreeQuantity / conversionRates.UToSValue;

                //            item.SecondaryCredit = item.SecondaryCredit + item.FreeQuantity;
                //        }
                //        else if (item.PriceAddedFor == 3)
                //        {
                //            item.PrimaryCredit = item.PrimaryCredit + item.FreeQuantity;

                //            item.SecondaryCredit = item.SecondaryCredit + item.FreeQuantity * (1 * conversionRates.UToSValue);
                //        }
                //    }
                //    else if (conversionRates.TToQValue == 0)
                //    {
                //        if (item.PriceAddedFor == 4)
                //        {
                //            item.PrimaryCredit = item.PrimaryCredit + item.FreeQuantity / conversionRates.UToSValue / conversionRates.SToTValue;

                //            item.SecondaryCredit = item.SecondaryCredit + item.FreeQuantity / conversionRates.SToTValue;

                //            item.TertiaryCredit = item.TertiaryCredit + item.FreeQuantity;
                //        }
                //        else if (item.PriceAddedFor == 3)
                //        {
                //            item.PrimaryCredit = item.PrimaryCredit + item.FreeQuantity / conversionRates.UToSValue; ;

                //            item.SecondaryCredit = item.SecondaryCredit + item.FreeQuantity;

                //            item.TertiaryCredit = item.TertiaryCredit + item.FreeQuantity * conversionRates.SToTValue;
                //        }
                //        else if (item.PriceAddedFor == 2)
                //        {
                //            item.PrimaryCredit = item.PrimaryCredit + item.FreeQuantity;

                //            item.SecondaryCredit = item.SecondaryCredit + item.FreeQuantity * conversionRates.UToSValue;

                //            item.TertiaryCredit = item.TertiaryCredit + item.FreeQuantity * conversionRates.UToSValue * conversionRates.SToTValue;
                //        }
                //    }
                //    else
                //    {
                //        if (item.PriceAddedFor == 4)
                //        {
                //            item.PrimaryCredit = item.PrimaryCredit + item.FreeQuantity / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;

                //            item.SecondaryCredit = item.SecondaryCredit + item.FreeQuantity / conversionRates.SToTValue / conversionRates.TToQValue;

                //            item.TertiaryCredit = item.TertiaryCredit + item.FreeQuantity / conversionRates.TToQValue;

                //            item.QuaternaryCredit = item.QuaternaryCredit + item.FreeQuantity;
                //        }
                //        else if (item.PriceAddedFor == 3)
                //        {
                //            item.PrimaryCredit = item.PrimaryCredit + item.FreeQuantity / conversionRates.UToSValue / conversionRates.SToTValue;

                //            item.SecondaryCredit = item.SecondaryCredit + item.FreeQuantity / conversionRates.SToTValue;

                //            item.TertiaryCredit = item.TertiaryCredit + item.FreeQuantity;

                //            item.QuaternaryCredit = item.QuaternaryCredit + item.FreeQuantity * conversionRates.TToQValue;
                //        }
                //        else if (item.PriceAddedFor == 2)
                //        {
                //            item.PrimaryCredit = item.PrimaryCredit + item.FreeQuantity / conversionRates.UToSValue;

                //            item.SecondaryCredit = item.SecondaryCredit + item.FreeQuantity;

                //            item.TertiaryCredit = item.TertiaryCredit + item.FreeQuantity * conversionRates.SToTValue;

                //            item.QuaternaryCredit = item.QuaternaryCredit + item.FreeQuantity * conversionRates.SToTValue * conversionRates.TToQValue;
                //        }
                //        else if (item.PriceAddedFor == 1)
                //        {
                //            item.PrimaryCredit = item.PrimaryCredit + item.FreeQuantity;

                //            item.SecondaryCredit = item.SecondaryCredit + item.FreeQuantity * (1 * conversionRates.UToSValue);

                //            item.TertiaryCredit = item.TertiaryCredit + item.FreeQuantity * (1 * conversionRates.UToSValue * conversionRates.SToTValue);

                //            item.QuaternaryCredit = item.QuaternaryCredit + item.FreeQuantity * (1 * conversionRates.UToSValue * conversionRates.SToTValue * conversionRates.TToQValue);
                //        }
                //    }
                //}
            }

            foreach (var item in _expiredStock)
            {
                if (conversionRates.UToSValue == 0 && conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    item.PrimaryDebit = item.Debit;
                }
                else if (conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    if (item.PriceAddedFor == 4)
                    {
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue;

                        item.SecondaryDebit = item.Debit;
                    }
                    else if (item.PriceAddedFor == 3)
                    {
                        item.PrimaryDebit = item.Debit;

                        item.SecondaryDebit = item.Debit * (1 * conversionRates.UToSValue);
                    }
                }
                else if (conversionRates.TToQValue == 0)
                {
                    if (item.PriceAddedFor == 4)
                    {
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue / conversionRates.SToTValue;

                        item.SecondaryDebit = item.Debit / conversionRates.SToTValue;

                        item.TertiaryDebit = item.Debit;
                    }
                    else if (item.PriceAddedFor == 3)
                    {
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue;

                        item.SecondaryDebit = item.Debit;

                        item.TertiaryDebit = item.Debit * conversionRates.SToTValue;
                    }
                    else if (item.PriceAddedFor == 2)
                    {
                        item.PrimaryDebit = item.Debit;

                        item.SecondaryDebit = item.Debit * conversionRates.UToSValue;

                        item.TertiaryDebit = item.Debit * conversionRates.UToSValue * conversionRates.SToTValue;
                    }
                }
                else
                {
                    if (item.PriceAddedFor == 4)
                    {
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;

                        item.SecondaryDebit = item.Debit / conversionRates.SToTValue / conversionRates.TToQValue;

                        item.TertiaryDebit = item.Debit / conversionRates.TToQValue;

                        item.QuaternaryDebit = item.Debit;
                    }
                    else if (item.PriceAddedFor == 3)
                    {
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue / conversionRates.SToTValue;

                        item.SecondaryDebit = item.Debit / conversionRates.SToTValue;

                        item.TertiaryDebit = item.Debit;

                        item.QuaternaryDebit = item.Debit * conversionRates.TToQValue;
                    }
                    else if (item.PriceAddedFor == 2)
                    {
                        item.PrimaryDebit = item.Debit / conversionRates.UToSValue;

                        item.SecondaryDebit = item.Debit;

                        item.TertiaryDebit = item.Debit * conversionRates.SToTValue;

                        item.QuaternaryDebit = item.Debit * conversionRates.SToTValue * conversionRates.TToQValue;
                    }
                    else if (item.PriceAddedFor == 1)
                    {
                        item.PrimaryDebit = item.Debit;

                        item.SecondaryDebit = item.Debit * (1 * conversionRates.UToSValue);

                        item.TertiaryDebit = item.Debit * (1 * conversionRates.UToSValue * conversionRates.SToTValue);

                        item.QuaternaryDebit = item.Debit * (1 * conversionRates.UToSValue * conversionRates.SToTValue * conversionRates.TToQValue);
                    }
                }
            }

            var stockDetails = _stock.Union(_expiredStock);

            decimal Balance = 0;

            foreach (var item in stockDetails)
            {
                if (obj.PriceAddedFor == 1) { item.Credit = item.PrimaryCredit; item.Debit = item.PrimaryDebit; Balance = Balance + (item.PrimaryCredit - item.PrimaryDebit); /*item.UnitName = conversionRates.PrimaryUnit;*/ }
                else if (obj.PriceAddedFor == 2) { item.Credit = item.SecondaryCredit; item.Debit = item.SecondaryDebit; Balance = Balance + (item.SecondaryCredit - item.SecondaryDebit); /*item.UnitName = conversionRates.SecondaryUnit;*/ }
                else if (obj.PriceAddedFor == 3) { item.Credit = item.TertiaryCredit; item.Debit = item.TertiaryDebit; Balance = Balance + (item.TertiaryCredit - item.TertiaryDebit); /*item.UnitName = conversionRates.TertiaryUnit;*/ }
                else { item.Credit = item.QuaternaryCredit; item.Debit = item.QuaternaryDebit; Balance = Balance + (item.QuaternaryCredit - item.QuaternaryDebit); /*item.UnitName = conversionRates.QuaternaryUnit; */}
                //Balance = Balance + (item.Credit - item.Debit);
                item.Balance = Balance;
            }

            decimal TotalPurchase = stockDetails.Where(a => a.Type == "Purchase").Select(a => a.Credit == 0 ? a.Debit : a.Credit).DefaultIfEmpty().Sum();
            decimal TotalOpeningStock = stockDetails.Where(a => a.Type == "Opening Stock").Select(a => a.Credit == 0 ? a.Debit : a.Credit).DefaultIfEmpty().Sum();
            decimal TotalSalesReturn = stockDetails.Where(a => a.Type == "Pos Return" || a.Type == "Sales Return").Select(a => a.Credit == 0 ? a.Debit : a.Credit).DefaultIfEmpty().Sum();
            decimal TotalStockTransferIn = stockDetails.Where(a => a.Type == "Stock Received").Select(a => a.Credit == 0 ? a.Debit : a.Credit).DefaultIfEmpty().Sum();
            decimal TotalSales = stockDetails.Where(a => a.Type == "Pos" || a.Type == "Sales").Select(a => a.Credit == 0 ? a.Debit : a.Credit).DefaultIfEmpty().Sum();
            decimal TotalStockAdjustment = stockDetails.Where(a => a.Type == "Stock Adjustment").Select(a => a.Credit == 0 ? a.Debit : a.Credit).DefaultIfEmpty().Sum();
            decimal TotalPurchaseReturn = stockDetails.Where(a => a.Type == "Purchase Return").Select(a => a.Credit == 0 ? a.Debit : a.Credit).DefaultIfEmpty().Sum();
            decimal TotalStockTransferOut = stockDetails.Where(a => a.Type == "Stock Transfer").Select(a => a.Credit == 0 ? a.Debit : a.Credit).DefaultIfEmpty().Sum();

            var Stock = new
            {
                ItemName = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                VariationName = (from b in oConnectionContext.DbClsItemDetails where b.ItemDetailsId == obj.ItemDetailsId select oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault()).FirstOrDefault(),
                SKU = (from a in oConnectionContext.DbClsItem
                       join b in oConnectionContext.DbClsItemDetails
on a.ItemId equals b.ItemId
                       where a.ItemId == obj.ItemId && b.ItemDetailsId == obj.ItemDetailsId
                       select a.ProductType.ToLower() == "variable" ? b.SKU : a.SkuCode).FirstOrDefault(),
                TotalPurchase = TotalPurchase,
                TotalOpeningStock = TotalOpeningStock,
                TotalSalesReturn = TotalSalesReturn,
                TotalStockTransferIn = TotalStockTransferIn,
                TotalSales = TotalSales,
                TotalStockAdjustment = TotalStockAdjustment,
                TotalPurchaseReturn = TotalPurchaseReturn,
                TotalStockTransferOut = TotalStockTransferOut,
                TotalCurrentStock = TotalCurrentStock,
                //TotalWastage = TotalWastage,
                UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit,
                //UnitId = conversionRates.UnitId,
                //SecondaryUnitId = conversionRates.SecondaryUnitId,
                //TertiaryUnitId=conversionRates.TertiaryUnitId,
                //QuaternaryUnitId=conversionRates.QuaternaryUnitId,
                //SecondaryUnit=conversionRates.SecondaryUnit,
                StockDetails = stockDetails.OrderByDescending(a => a.Date).ThenByDescending(a => a.Id).Skip(skip).Take(obj.PageSize).ToList(),
            };
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Stock = Stock,
                    TotalCount = stockDetails.Count(),
                    Branchs = userDetails?.BranchIds,
                    ItemDetails = ItemDetails,
                    PageSize = obj.PageSize
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CurrentStock(ClsItemDetailsVm obj)
        {
            obj.ToDate = DateTime.Now;

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsItemDetailsVm> ItemDetails;
            if (obj.BranchId == 0)
            {
                ItemDetails = (from a in oConnectionContext.DbClsItemBranchMap
                               join b in oConnectionContext.DbClsItemDetails
                               on a.ItemDetailsId equals b.ItemDetailsId
                               join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                               where
                               //a.BranchId == obj.BranchId && 
                               oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                               c.ProductType != "Combo"
                               && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                               && c.IsManageStock == true
                               select new ClsItemDetailsVm
                               {
                                   ItemId = c.ItemId,
                                   ItemDetailsId = b.ItemDetailsId,
                                   ItemName = c.ItemName,
                                   SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                   VariationName = oConnectionContext.DbClsVariationDetails.Where(cc =>
                                   cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                   Quantity = a.Quantity,
                                   SaltId = c.SaltId,
                                   SaltName = oConnectionContext.DbClsSalt.Where(d => d.SaltId == c.SaltId).Select(d => d.SaltName).FirstOrDefault()
                               }).ToList();
            }
            else
            {
                ItemDetails = (from a in oConnectionContext.DbClsItemBranchMap
                               join b in oConnectionContext.DbClsItemDetails
                               on a.ItemDetailsId equals b.ItemDetailsId
                               join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                               where a.BranchId == obj.BranchId && c.ProductType != "Combo"
                               && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                               && c.IsManageStock == true
                               select new ClsItemDetailsVm
                               {
                                   ItemId = c.ItemId,
                                   ItemDetailsId = b.ItemDetailsId,
                                   ItemName = c.ItemName,
                                   SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                                   VariationName = oConnectionContext.DbClsVariationDetails.Where(cc =>
                                   cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                   Quantity = a.Quantity,
                                   SaltId = c.SaltId,
                                   SaltName = oConnectionContext.DbClsSalt.Where(d => d.SaltId == c.SaltId).Select(d => d.SaltName).FirstOrDefault()
                               }).ToList();
            }

            List<ClsItemDetailsVm> _ItemDetails1 = new List<ClsItemDetailsVm>();
            //List<ClsItemDetailsVm> _ItemDetails2 = new List<ClsItemDetailsVm>();

            if (obj.ItemDetailsId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.ItemDetailsId == obj.ItemDetailsId).ToList();
            }

            ItemDetails = ItemDetails.GroupBy(p => p.ItemDetailsId,
                     (k, c) => new ClsItemDetailsVm
                     {
                         ItemId = c.Select(cs => cs.ItemId).FirstOrDefault(),
                         ItemDetailsId = c.Select(cs => cs.ItemDetailsId).FirstOrDefault(),
                         ItemName = c.Select(cs => cs.ItemName).FirstOrDefault(),
                         SKU = c.Select(cs => cs.SKU).FirstOrDefault(),
                         VariationName = c.Select(cs => cs.VariationName).FirstOrDefault(),
                         Quantity = c.Select(cs => cs.Quantity).Sum(),
                         SaltId = c.Select(cs => cs.SaltId).FirstOrDefault(),
                         SaltName = c.Select(cs => cs.SaltName).FirstOrDefault(),
                     }).ToList();

            if (obj.SaltName != "" && obj.SaltName != null)
            {
                ItemDetails = ItemDetails.Where(a => a.SaltName != null && a.SaltName != "" && a.SaltName.ToLower() == obj.SaltName.ToLower()).Select(a => a).ToList();
            }

            if (obj.SaltId != 0)
            {
                ItemDetails = ItemDetails.Where(a => a.SaltId == obj.SaltId).Select(a => a).ToList();
            }

            _ItemDetails1 = ItemDetails.OrderByDescending(a => a.ItemDetailsId).Skip(skip).Take(obj.PageSize).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            foreach (var item in _ItemDetails1)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
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

                item.UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2
                           ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit;
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ItemDetails = _ItemDetails1,
                    TotalCount = ItemDetails.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> LotReport(ClsItemVm obj)
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

            List<ClsPurchaseDetailsVm> det;
            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsPurchase
                       join b in oConnectionContext.DbClsPurchaseDetails
on a.PurchaseId equals b.PurchaseId
                       where
                       //a.BranchId == obj.BranchId && 
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       //&& b.QuantityRemaining > 0
                       && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && a.IsCancelled == false && b.LotNo != null
                       && DbFunctions.TruncateTime(b.AddedOn) >= obj.FromDate &&
DbFunctions.TruncateTime(b.AddedOn) <= obj.ToDate
                       select new ClsPurchaseDetailsVm
                       {
                           BranchId = a.BranchId,
                           CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                           SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                           SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                           BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                           //Category = oConnectionContext.DbClsCategory.Where(c => c.CategoryId== oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault()).Select(c => c.Category).FirstOrDefault(),
                           //SubCategory = oConnectionContext.DbClsSubCategory.Where(c => c.SubCategoryId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault()).Select(c => c.SubCategory).FirstOrDefault(),
                           //SubSubCategory = oConnectionContext.DbClsSubSubCategory.Where(c => c.SubSubCategoryId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault()).Select(c => c.SubSubCategory).FirstOrDefault(),
                           //Brand= oConnectionContext.DbClsBrand.Where(c => c.BrandId== oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault()).Select(c => c.Brand).FirstOrDefault(),
                           //Branch = oConnectionContext.DbClsBranch.Where(c=>c.BranchId== a.BranchId).Select(c=>c.Branch).FirstOrDefault(),
                           AddedOn = b.AddedOn,
                           LotNo = b.LotNo,
                           ExpiryDate = b.ExpiryDate,
                           ManufacturingDate = b.ManufacturingDate,
                           ItemId = b.ItemId,
                           UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                           ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                           ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                           SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                               oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                               oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                           oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           Quantity = b.Quantity,
                           QuantityRemaining = b.QuantityRemaining,
                           QuantitySold = oConnectionContext.DbClsSalesDeductionId.Where(x => x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                           AdjustedQuantity = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(x => x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                           //TransferredQuantity = oConnectionContext.DbClsStockTransferDeductionId.Where(x => x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                       }).Union(from a in oConnectionContext.DbClsOpeningStock
                                where
                                //a.BranchId == obj.BranchId && 
                                oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                //&& a.QuantityRemaining > 0
                                && a.IsDeleted == false && a.LotNo != null
                                && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
         DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                                select new ClsPurchaseDetailsVm
                                {
                                    BranchId = a.BranchId,
                                    CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                                    SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                                    SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                                    BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                                    AddedOn = a.AddedOn,
                                    LotNo = a.LotNo,
                                    ExpiryDate = a.ExpiryDate,
                                    ManufacturingDate = a.ManufacturingDate,
                                    ItemId = a.ItemId,
                                    UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                                        oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == a.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                                        oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == a.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                    Quantity = a.Quantity,
                                    QuantityRemaining = a.QuantityRemaining,
                                    QuantitySold = oConnectionContext.DbClsSalesDeductionId.Where(x => x.Type.ToLower() == "openingstock" && x.Id == a.OpeningStockId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                    AdjustedQuantity = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(x => x.Type.ToLower() == "openingstock" && x.Id == a.OpeningStockId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                    //TransferredQuantity = oConnectionContext.DbClsStockTransferDeductionId.Where(x => x.Type.ToLower() == "openingstock" && x.Id == a.OpeningStockId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                }).Union(from a in oConnectionContext.DbClsStockTransfer
                                         join b in oConnectionContext.DbClsStockTransferDetails
                  on a.StockTransferId equals b.StockTransferId
                                         where
                                         oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                  l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.ToBranchId)
                                         //&& b.QuantityRemaining > 0
                                         && a.IsDeleted == false && b.IsDeleted == false && b.LotTypeForLotNoChecking != null
                                         && DbFunctions.TruncateTime(b.AddedOn) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.AddedOn) <= obj.ToDate
                                         select new ClsPurchaseDetailsVm
                                         {
                                             BranchId = a.ToBranchId,
                                             CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                                             SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                                             SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                                             BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                                             //Category = oConnectionContext.DbClsCategory.Where(c => c.CategoryId== oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault()).Select(c => c.Category).FirstOrDefault(),
                                             //SubCategory = oConnectionContext.DbClsSubCategory.Where(c => c.SubCategoryId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault()).Select(c => c.SubCategory).FirstOrDefault(),
                                             //SubSubCategory = oConnectionContext.DbClsSubSubCategory.Where(c => c.SubSubCategoryId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault()).Select(c => c.SubSubCategory).FirstOrDefault(),
                                             //Brand= oConnectionContext.DbClsBrand.Where(c => c.BrandId== oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault()).Select(c => c.Brand).FirstOrDefault(),
                                             //Branch = oConnectionContext.DbClsBranch.Where(c=>c.BranchId== a.BranchId).Select(c=>c.Branch).FirstOrDefault(),
                                             AddedOn = b.AddedOn,
                                             LotNo = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault()
                                      : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault(),
                                             ExpiryDate = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                                      : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault(),
                                             ManufacturingDate = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                                      : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault(),
                                             ItemId = b.ItemId,
                                             UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                                             ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                             ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                                             SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                                                 oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                                                 oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                                             VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                                             oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                             Quantity = b.Quantity,
                                             QuantityRemaining = b.QuantityRemaining,
                                             QuantitySold = oConnectionContext.DbClsSalesDeductionId.Where(x => x.Type.ToLower() == "stocktransfer" && x.Id == b.StockTransferDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                             AdjustedQuantity = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(x => x.Type.ToLower() == "stocktransfer" && x.Id == b.StockTransferDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                             //TransferredQuantity = oConnectionContext.DbClsStockTransferDeductionId.Where(x => x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                         })

                                             //                       .Union(from a in oConnectionContext.DbClsStockAdjustment
                                             //                                join b in oConnectionContext.DbClsStockAdjustmentDetails
                                             //on a.StockAdjustmentId equals b.StockAdjustmentId
                                             //                                where a.BranchId == obj.BranchId && b.QuantityRemaining > 0
                                             //                                && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false && b.LotNo != null
                                             //                                select new ClsPurchaseDetailsVm
                                             //                                {
                                             //                                    CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                                             //                                    SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                                             //                                    SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                                             //                                    BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                                             //                                    AddedOn = b.AddedOn,
                                             //                                    LotNo = b.LotNo,
                                             //                                    ExpiryDate = b.ExpiryDate,
                                             //                                    ManufacturingDate = b.ManufacturingDate,
                                             //                                    ItemId = b.ItemId,
                                             //                                    UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                                             //                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                             //                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                                             //                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                             //                                    oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                             //                                    oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                                             //                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                                             //                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                             //                                    Quantity = b.Quantity,
                                             //                                    QuantityRemaining = b.QuantityRemaining,
                                             //                                    QuantitySold = b.QuantitySold
                                             //                                })
                                             .ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsPurchase
                       join b in oConnectionContext.DbClsPurchaseDetails
on a.PurchaseId equals b.PurchaseId
                       where
                       a.BranchId == obj.BranchId
                       //&& b.QuantityRemaining > 0
                       && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && a.IsCancelled == false && b.LotNo != null
                       && DbFunctions.TruncateTime(b.AddedOn) >= obj.FromDate &&
DbFunctions.TruncateTime(b.AddedOn) <= obj.ToDate
                       select new ClsPurchaseDetailsVm
                       {
                           BranchId = a.BranchId,
                           CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                           SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                           SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                           BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                           //Category = oConnectionContext.DbClsCategory.Where(c => c.CategoryId== oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault()).Select(c => c.Category).FirstOrDefault(),
                           //SubCategory = oConnectionContext.DbClsSubCategory.Where(c => c.SubCategoryId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault()).Select(c => c.SubCategory).FirstOrDefault(),
                           //SubSubCategory = oConnectionContext.DbClsSubSubCategory.Where(c => c.SubSubCategoryId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault()).Select(c => c.SubSubCategory).FirstOrDefault(),
                           //Brand= oConnectionContext.DbClsBrand.Where(c => c.BrandId== oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault()).Select(c => c.Brand).FirstOrDefault(),
                           //Branch = oConnectionContext.DbClsBranch.Where(c=>c.BranchId== a.BranchId).Select(c=>c.Branch).FirstOrDefault(),
                           AddedOn = b.AddedOn,
                           LotNo = b.LotNo,
                           ExpiryDate = b.ExpiryDate,
                           ManufacturingDate = b.ManufacturingDate,
                           ItemId = b.ItemId,
                           UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                           ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                           ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                           SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                               oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                               oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                           oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           Quantity = b.Quantity,
                           QuantityRemaining = b.QuantityRemaining,
                           QuantitySold = oConnectionContext.DbClsSalesDeductionId.Where(x => x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                           AdjustedQuantity = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(x => x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                           //TransferredQuantity = oConnectionContext.DbClsStockTransferDeductionId.Where(x => x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                       }).Union(from a in oConnectionContext.DbClsOpeningStock
                                where
                                a.BranchId == obj.BranchId
                                //&& a.QuantityRemaining > 0
                                && a.IsDeleted == false && a.LotNo != null
                                && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
         DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                                select new ClsPurchaseDetailsVm
                                {
                                    BranchId = a.BranchId,
                                    CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                                    SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                                    SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                                    BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                                    AddedOn = a.AddedOn,
                                    LotNo = a.LotNo,
                                    ExpiryDate = a.ExpiryDate,
                                    ManufacturingDate = a.ManufacturingDate,
                                    ItemId = a.ItemId,
                                    UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                                        oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == a.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                                        oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == a.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                    Quantity = a.Quantity,
                                    QuantityRemaining = a.QuantityRemaining,
                                    QuantitySold = oConnectionContext.DbClsSalesDeductionId.Where(x => x.Type.ToLower() == "openingstock" && x.Id == a.OpeningStockId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                    AdjustedQuantity = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(x => x.Type.ToLower() == "openingstock" && x.Id == a.OpeningStockId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                    //TransferredQuantity = oConnectionContext.DbClsStockTransferDeductionId.Where(x => x.Type.ToLower() == "openingstock" && x.Id == a.OpeningStockId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                }).Union(from a in oConnectionContext.DbClsStockTransfer
                                         join b in oConnectionContext.DbClsStockTransferDetails
                  on a.StockTransferId equals b.StockTransferId
                                         where
                                         a.ToBranchId == obj.BranchId
                                         //&& b.QuantityRemaining > 0
                                         && a.IsDeleted == false && b.IsDeleted == false && b.LotTypeForLotNoChecking != null
                                         && DbFunctions.TruncateTime(b.AddedOn) >= obj.FromDate &&
                  DbFunctions.TruncateTime(b.AddedOn) <= obj.ToDate
                                         select new ClsPurchaseDetailsVm
                                         {
                                             BranchId = a.ToBranchId,
                                             CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                                             SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                                             SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                                             BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                                             //Category = oConnectionContext.DbClsCategory.Where(c => c.CategoryId== oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault()).Select(c => c.Category).FirstOrDefault(),
                                             //SubCategory = oConnectionContext.DbClsSubCategory.Where(c => c.SubCategoryId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault()).Select(c => c.SubCategory).FirstOrDefault(),
                                             //SubSubCategory = oConnectionContext.DbClsSubSubCategory.Where(c => c.SubSubCategoryId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault()).Select(c => c.SubSubCategory).FirstOrDefault(),
                                             //Brand= oConnectionContext.DbClsBrand.Where(c => c.BrandId== oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault()).Select(c => c.Brand).FirstOrDefault(),
                                             //Branch = oConnectionContext.DbClsBranch.Where(c=>c.BranchId== a.BranchId).Select(c=>c.Branch).FirstOrDefault(),
                                             AddedOn = b.AddedOn,
                                             LotNo = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault()
                                      : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.LotNo).FirstOrDefault(),
                                             ExpiryDate = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault()
                                      : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ExpiryDate).FirstOrDefault(),
                                             ManufacturingDate = b.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault()
                                      : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == b.LotIdForLotNoChecking).Select(f => f.ManufacturingDate).FirstOrDefault(),
                                             ItemId = b.ItemId,
                                             UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                                             ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                             ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                                             SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                                                 oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                                                 oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                                             VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                                             oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                             Quantity = b.Quantity,
                                             QuantityRemaining = b.QuantityRemaining,
                                             QuantitySold = oConnectionContext.DbClsSalesDeductionId.Where(x => x.Type.ToLower() == "stocktransfer" && x.Id == b.StockTransferDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                             AdjustedQuantity = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(x => x.Type.ToLower() == "stocktransfer" && x.Id == b.StockTransferDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                             //TransferredQuantity = oConnectionContext.DbClsStockTransferDeductionId.Where(x => x.Type.ToLower() == "purchase" && x.Id == b.PurchaseDetailsId).Select(x => x.Quantity).DefaultIfEmpty().Sum(),
                                         })

                                             //                       .Union(from a in oConnectionContext.DbClsStockAdjustment
                                             //                                join b in oConnectionContext.DbClsStockAdjustmentDetails
                                             //on a.StockAdjustmentId equals b.StockAdjustmentId
                                             //                                where a.BranchId == obj.BranchId && b.QuantityRemaining > 0
                                             //                                && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false && b.LotNo != null
                                             //                                select new ClsPurchaseDetailsVm
                                             //                                {
                                             //                                    CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                                             //                                    SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                                             //                                    SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                                             //                                    BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                                             //                                    AddedOn = b.AddedOn,
                                             //                                    LotNo = b.LotNo,
                                             //                                    ExpiryDate = b.ExpiryDate,
                                             //                                    ManufacturingDate = b.ManufacturingDate,
                                             //                                    ItemId = b.ItemId,
                                             //                                    UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                                             //                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                             //                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                                             //                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                             //                                    oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                             //                                    oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                                             //                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                                             //                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                             //                                    Quantity = b.Quantity,
                                             //                                    QuantityRemaining = b.QuantityRemaining,
                                             //                                    QuantitySold = b.QuantitySold
                                             //                                })
                                             .ToList();
            }

            if (obj.CategoryId != 0)
            {
                det = det.Where(a => a.CategoryId == obj.CategoryId).ToList();
            }

            if (obj.SubCategoryId != 0)
            {
                det = det.Where(a => a.SubCategoryId == obj.SubCategoryId).ToList();
            }

            if (obj.SubSubCategoryId != 0)
            {
                det = det.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).ToList();
            }
            if (obj.BrandId != 0)
            {
                det = det.Where(a => a.BrandId == obj.BrandId).ToList();
            }
            if (obj.LotNo != "" && obj.LotNo != null)
            {
                det = det.Where(a => a.LotNo.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            if (obj.ItemDetailsId != 0)
            {
                det = det.Where(a => a.ItemDetailsId == obj.ItemDetailsId).ToList();
            }

            List<ClsPurchaseDetailsVm> _det1 = new List<ClsPurchaseDetailsVm>();
            List<ClsPurchaseDetailsVm> _det2 = new List<ClsPurchaseDetailsVm>();
            _det1 = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            foreach (var item in _det1)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
                }).FirstOrDefault();

                decimal TotalCurrentStock = item.QuantityRemaining;
                decimal TotalUnitSold = item.QuantitySold;
                decimal TotalUnitAdjusted = item.AdjustedQuantity;

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
                        TotalUnitSold = TotalUnitSold / conversionRates.UToSValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.UToSValue;
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
                        TotalUnitSold = TotalUnitSold / conversionRates.UToSValue / conversionRates.SToTValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.UToSValue / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue;
                        TotalUnitSold = TotalUnitSold / conversionRates.SToTValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.SToTValue;
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
                        TotalUnitSold = TotalUnitSold / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitSold = TotalUnitSold / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.TToQValue;
                        TotalUnitSold = TotalUnitSold / conversionRates.TToQValue;
                        TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 4)
                    {
                        //TotalCurrentStock= TotalCurrentStock;
                    }
                }

                _det2.Add(new ClsPurchaseDetailsVm
                {
                    BranchId = item.BranchId,
                    CategoryId = item.CategoryId,
                    SubCategoryId = item.SubCategoryId,
                    SubSubCategoryId = item.SubSubCategoryId,
                    BrandId = item.BrandId,
                    AddedOn = item.AddedOn,
                    LotNo = item.LotNo,
                    ExpiryDate = item.ExpiryDate,
                    ManufacturingDate = item.ManufacturingDate,
                    ItemId = item.ItemId,
                    UnitShortName = item.UnitShortName,
                    ItemDetailsId = item.ItemDetailsId,
                    ItemName = item.ItemName,
                    SKU = item.SKU,
                    VariationName = item.VariationName,
                    Quantity = item.Quantity,
                    QuantityRemaining = TotalCurrentStock,
                    QuantitySold = TotalUnitSold,
                    AdjustedQuantity = TotalUnitAdjusted,
                    UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit,
                });
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseDetails = _det2,
                    TotalCount = det.Count(),
                    //Branchs = userDetails?.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockExpiryReport(ClsItemVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //   oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //     && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
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

            List<ClsPurchaseDetailsVm> det;
            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsPurchase
                       join b in oConnectionContext.DbClsPurchaseDetails
on a.PurchaseId equals b.PurchaseId
                       where
                       //a.BranchId == obj.BranchId 
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && b.QuantityRemaining > 0
                       && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.ExpiryDate != null
                       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
&& a.Status.ToLower() != "draft" && a.IsCancelled == false
                       select new ClsPurchaseDetailsVm
                       {
                           PurchaseDetailsId = b.PurchaseDetailsId,
                           BranchId = a.BranchId,
                           CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                           SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                           SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                           BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                           //Branch = oConnectionContext.DbClsBranch.Where(c=>c.BranchId== a.BranchId).Select(c=>c.Branch).FirstOrDefault(),
                           AddedOn = b.AddedOn,
                           LotNo = b.LotNo,
                           ExpiryDate = b.ExpiryDate,
                           ManufacturingDate = b.ManufacturingDate,
                           ItemId = b.ItemId,
                           UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                           ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                           ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                           SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                               oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                               oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                           oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           Quantity = b.Quantity,
                           QuantityRemaining = b.QuantityRemaining,
                           QuantitySold = b.QuantitySold
                       }).Union(from a in oConnectionContext.DbClsOpeningStock
                                where
                                //a.BranchId == obj.BranchId && 
                                oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) &&
                                a.QuantityRemaining > 0
                                && a.IsDeleted == false && a.ExpiryDate != null
                                && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
         DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                                select new ClsPurchaseDetailsVm
                                {
                                    PurchaseDetailsId = a.OpeningStockId,
                                    BranchId = a.BranchId,
                                    CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                                    SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                                    SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                                    BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                                    AddedOn = a.AddedOn,
                                    LotNo = a.LotNo,
                                    ExpiryDate = a.ExpiryDate,
                                    ManufacturingDate = a.ManufacturingDate,
                                    ItemId = a.ItemId,
                                    UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                                        oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == a.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                                        oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == a.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                    Quantity = a.Quantity,
                                    QuantityRemaining = a.QuantityRemaining,
                                    QuantitySold = a.QuantitySold,
                                    //AdjustedQuantity = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(x => x.Type.ToLower() == "openingstock" && x.Id == a.OpeningStockId).Select(x => x.Quantity).Sum(),
                                })
                                                             //                       .Union(from a in oConnectionContext.DbClsStockAdjustment
                                                             //                                join b in oConnectionContext.DbClsStockAdjustmentDetails
                                                             //on a.StockAdjustmentId equals b.StockAdjustmentId
                                                             //                                where a.BranchId == obj.BranchId && b.QuantityRemaining > 0
                                                             //                                && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false && b.ExpiryDate != null
                                                             //                                select new ClsPurchaseDetailsVm
                                                             //                                {
                                                             //                                    CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                                                             //                                    SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                                                             //                                    SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                                                             //                                    BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                                                             //                                    AddedOn = b.AddedOn,
                                                             //                                    LotNo = b.LotNo,
                                                             //                                    ExpiryDate = b.ExpiryDate,
                                                             //                                    ManufacturingDate = b.ManufacturingDate,
                                                             //                                    ItemId = b.ItemId,
                                                             //                                    UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                                                             //                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                                             //                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                                                             //                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                                             //                                    oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                                             //                                    oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                                                             //                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                                                             //                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                                             //                                    Quantity = b.Quantity,
                                                             //                                    QuantityRemaining = b.QuantityRemaining,
                                                             //                                    QuantitySold = b.QuantitySold
                                                             //                                })
                                                             .ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsPurchase
                       join b in oConnectionContext.DbClsPurchaseDetails
on a.PurchaseId equals b.PurchaseId
                       where
                       a.BranchId == obj.BranchId
                       //&& b.QuantityRemaining > 0
                       && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.ExpiryDate != null
                       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
&& a.Status.ToLower() != "draft" && a.IsCancelled == false
                       select new ClsPurchaseDetailsVm
                       {
                           BranchId = a.BranchId,
                           CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                           SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                           SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                           BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                           //Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == a.BranchId).Select(c => c.Branch).FirstOrDefault(),
                           AddedOn = b.AddedOn,
                           LotNo = b.LotNo,
                           ExpiryDate = b.ExpiryDate,
                           ManufacturingDate = b.ManufacturingDate,
                           ItemId = b.ItemId,
                           UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                           ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                           ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                           SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                               oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                               oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                           oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           Quantity = b.Quantity,
                           QuantityRemaining = b.QuantityRemaining,
                           QuantitySold = b.QuantitySold
                       }).Union(from a in oConnectionContext.DbClsOpeningStock
                                where
                                a.BranchId == obj.BranchId &&
                                a.QuantityRemaining > 0
                                && a.IsDeleted == false && a.ExpiryDate != null
                                && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
         DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                                select new ClsPurchaseDetailsVm
                                {
                                    BranchId = a.BranchId,
                                    CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                                    SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                                    SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                                    BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                                    AddedOn = a.AddedOn,
                                    LotNo = a.LotNo,
                                    ExpiryDate = a.ExpiryDate,
                                    ManufacturingDate = a.ManufacturingDate,
                                    ItemId = a.ItemId,
                                    UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == a.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                                        oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == a.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                                        oConnectionContext.DbClsItem.Where(c => c.ItemId == a.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == a.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                    Quantity = a.Quantity,
                                    QuantityRemaining = a.QuantityRemaining,
                                    QuantitySold = a.QuantitySold,
                                    //AdjustedQuantity = oConnectionContext.DbClsStockAdjustmentDeductionId.Where(x => x.Type.ToLower() == "openingstock" && x.Id == a.OpeningStockId).Select(x => x.Quantity).Sum(),
                                })
                                                             //                       .Union(from a in oConnectionContext.DbClsStockAdjustment
                                                             //                                join b in oConnectionContext.DbClsStockAdjustmentDetails
                                                             //on a.StockAdjustmentId equals b.StockAdjustmentId
                                                             //                                where a.BranchId == obj.BranchId && b.QuantityRemaining > 0
                                                             //                                && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false && b.ExpiryDate != null
                                                             //                                select new ClsPurchaseDetailsVm
                                                             //                                {
                                                             //                                    CategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.CategoryId).FirstOrDefault(),
                                                             //                                    SubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubCategoryId).FirstOrDefault(),
                                                             //                                    SubSubCategoryId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.SubSubCategoryId).FirstOrDefault(),
                                                             //                                    BrandId = oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.BrandId).FirstOrDefault(),
                                                             //                                    AddedOn = b.AddedOn,
                                                             //                                    LotNo = b.LotNo,
                                                             //                                    ExpiryDate = b.ExpiryDate,
                                                             //                                    ManufacturingDate = b.ManufacturingDate,
                                                             //                                    ItemId = b.ItemId,
                                                             //                                    UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                                                             //                                    ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                                             //                                    ItemName = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ItemName).FirstOrDefault(),
                                                             //                                    SKU = oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.ProductType).FirstOrDefault() == "Variable" ?
                                                             //                                    oConnectionContext.DbClsItemDetails.Where(c => c.ItemDetailsId == b.ItemDetailsId).Select(c => c.SKU).FirstOrDefault() :
                                                             //                                    oConnectionContext.DbClsItem.Where(c => c.ItemId == b.ItemId).Select(c => c.SkuCode).FirstOrDefault(),
                                                             //                                    VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId ==
                                                             //                                    oConnectionContext.DbClsItemDetails.Where(d => d.ItemDetailsId == b.ItemDetailsId).Select(d => d.VariationDetailsId).FirstOrDefault()).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                                             //                                    Quantity = b.Quantity,
                                                             //                                    QuantityRemaining = b.QuantityRemaining,
                                                             //                                    QuantitySold = b.QuantitySold
                                                             //                                })
                                                             .ToList();
            }


            //if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            //{
            //    det = det.Where(a => a.AddedOn.Date >= obj.FromDate.AddHours(5).AddMinutes(30)
            //    && a.AddedOn.Date <= obj.ToDate.AddHours(5).AddMinutes(30)).ToList();
            //}

            if (obj.CategoryId != 0)
            {
                det = det.Where(a => a.CategoryId == obj.CategoryId).ToList();
            }

            if (obj.SubCategoryId != 0)
            {
                det = det.Where(a => a.SubCategoryId == obj.SubCategoryId).ToList();
            }

            if (obj.SubSubCategoryId != 0)
            {
                det = det.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).ToList();
            }
            if (obj.BrandId != 0)
            {
                det = det.Where(a => a.BrandId == obj.BrandId).ToList();
            }

            if (obj.LotNo != "" && obj.LotNo != null)
            {
                det = det.Where(a => a.LotNo.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            if (obj.ItemName != "" && obj.ItemName != null)
            {
                det = det.Where(a => a.ItemName.ToLower().Contains(obj.ItemName.ToLower())).ToList();
            }

            if (obj.ExpiryDate != null && obj.ExpiryDate.Value != DateTime.MinValue)
            {
                if (obj.ExpiryDate.Value.Date == DateTime.Now.AddDays(-1).Date)
                {
                    det = det.Where(a => a.ExpiryDate.Value.Date <= obj.ExpiryDate).ToList();
                }
                else
                {
                    det = det.Where(a => a.ExpiryDate.Value.Date >= DateTime.Now && a.ExpiryDate.Value.Date <= obj.ExpiryDate).ToList();
                }
            }

            List<ClsPurchaseDetailsVm> _det1 = new List<ClsPurchaseDetailsVm>();
            List<ClsPurchaseDetailsVm> _det2 = new List<ClsPurchaseDetailsVm>();
            _det1 = det.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            foreach (var item in _det1)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
                }).FirstOrDefault();

                decimal TotalCurrentStock = item.QuantityRemaining;
                //decimal TotalUnitSold = item.QuantitySold;
                //decimal TotalUnitAdjusted = item.AdjustedQuantity;

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
                        //TotalUnitSold = TotalUnitSold / conversionRates.UToSValue;
                        //TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.UToSValue;
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
                        //TotalUnitSold = TotalUnitSold / conversionRates.UToSValue / conversionRates.SToTValue;
                        //TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.UToSValue / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue;
                        //TotalUnitSold = TotalUnitSold / conversionRates.SToTValue;
                        //TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.SToTValue;
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
                        //TotalUnitSold = TotalUnitSold / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        //TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue / conversionRates.TToQValue;
                        //TotalUnitSold = TotalUnitSold / conversionRates.SToTValue / conversionRates.TToQValue;
                        //TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.TToQValue;
                        //TotalUnitSold = TotalUnitSold / conversionRates.TToQValue;
                        //TotalUnitAdjusted = TotalUnitAdjusted / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 4)
                    {
                        //TotalCurrentStock= TotalCurrentStock;
                    }
                }

                _det2.Add(new ClsPurchaseDetailsVm
                {
                    BranchId = item.BranchId,
                    CategoryId = item.CategoryId,
                    SubCategoryId = item.SubCategoryId,
                    SubSubCategoryId = item.SubSubCategoryId,
                    BrandId = item.BranchId,
                    AddedOn = item.AddedOn,
                    LotNo = item.LotNo,
                    ExpiryDate = item.ExpiryDate,
                    ManufacturingDate = item.ManufacturingDate,
                    ItemId = item.ItemId,
                    //UnitShortName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == oConnectionContext.DbClsItem.Where(d => d.ItemId == b.ItemId).Select(d => d.UnitId).FirstOrDefault()).Select(c => c.UnitShortName).FirstOrDefault(),
                    ItemDetailsId = item.ItemDetailsId,
                    ItemName = item.ItemName,
                    SKU = item.SKU,
                    VariationName = item.VariationName,
                    Quantity = item.Quantity,
                    QuantityRemaining = TotalCurrentStock,
                    QuantitySold = item.QuantitySold,
                    UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit,
                });
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseDetails = _det2,
                    //Branchs = userDetails?.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize,
                    TotalCount = det.Count(),
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TrendingProductsReport(ClsItemVm obj)
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
            List<ClsItemVm> det = new List<ClsItemVm>();
            if (obj.Type == "amount")
            {
                if (obj.BranchId == 0)
                {
                    det = (from a in oConnectionContext.DbClsItemBranchMap
                           join b in oConnectionContext.DbClsItemDetails
                           on a.ItemDetailsId equals b.ItemDetailsId
                           join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                           where
                           oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                           && c.ProductType != "Combo"
                           select new ClsItemVm
                           {
                               ProductType = c.ProductType,
                               ItemId = b.ItemId,
                               ItemDetailsId = a.ItemDetailsId,
                               ItemName = c.ItemName,
                               SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                               VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                               TotalSales = (from e in oConnectionContext.DbClsSales
                                             join f in oConnectionContext.DbClsSalesDetails
    on e.SalesId equals f.SalesId
                                             where e.Status != "Draft" &&
                                             //e.BranchId == obj.BranchId 
                                             oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == e.BranchId)
                                             && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
               && f.IsActive == true && f.IsDeleted == false && f.ItemDetailsId == b.ItemDetailsId
               && DbFunctions.TruncateTime(e.SalesDate) >= obj.FromDate &&
           DbFunctions.TruncateTime(e.SalesDate) <= obj.ToDate
                                             select f.AmountIncTax).DefaultIfEmpty().Sum()
                           }).Union((from a in oConnectionContext.DbClsItem
                                     where a.ProductType == "Combo" &&
    //oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
    oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId
    //&& b.BranchId == obj.BranchId
    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   )
                                     select new ClsItemVm
                                     {
                                         ProductType = a.ProductType,
                                         ItemId = a.ItemId,
                                         ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                         ItemName = a.ItemName,
                                         SKU = a.SkuCode,
                                         VariationName = "",
                                         TotalSales = (from e in oConnectionContext.DbClsSales
                                                       join f in oConnectionContext.DbClsSalesDetails
    on e.SalesId equals f.SalesId
                                                       where e.Status != "Draft" &&
                                                       //e.BranchId == obj.BranchId
                                                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == e.BranchId)
                                                       && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                         && f.IsActive == true && f.IsDeleted == false && f.ItemId == a.ItemId
                         && DbFunctions.TruncateTime(e.SalesDate) >= obj.FromDate &&
           DbFunctions.TruncateTime(e.SalesDate) <= obj.ToDate
                                                       select f.AmountIncTax).DefaultIfEmpty().Sum()
                                     })).Where(a => a.TotalSales > 0).ToList();
                }
                else
                {
                    det = (from a in oConnectionContext.DbClsItemBranchMap
                           join b in oConnectionContext.DbClsItemDetails
                           on a.ItemDetailsId equals b.ItemDetailsId
                           join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                           where a.BranchId == obj.BranchId && c.ProductType != "Combo"
                           select new ClsItemVm
                           {
                               ProductType = c.ProductType,
                               ItemId = b.ItemId,
                               ItemDetailsId = a.ItemDetailsId,
                               ItemName = c.ItemName,
                               SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                               VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                               TotalSales = (from e in oConnectionContext.DbClsSales
                                             join f in oConnectionContext.DbClsSalesDetails
    on e.SalesId equals f.SalesId
                                             where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
               && f.IsActive == true && f.IsDeleted == false && f.ItemDetailsId == b.ItemDetailsId
               && DbFunctions.TruncateTime(e.SalesDate) >= obj.FromDate &&
           DbFunctions.TruncateTime(e.SalesDate) <= obj.ToDate
                                             select f.AmountIncTax).DefaultIfEmpty().Sum()
                           }).Union((from a in oConnectionContext.DbClsItem
                                     where a.ProductType == "Combo" &&
    oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
                                     select new ClsItemVm
                                     {
                                         ProductType = a.ProductType,
                                         ItemId = a.ItemId,
                                         ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                         ItemName = a.ItemName,
                                         SKU = a.SkuCode,
                                         VariationName = "",
                                         TotalSales = (from e in oConnectionContext.DbClsSales
                                                       join f in oConnectionContext.DbClsSalesDetails
    on e.SalesId equals f.SalesId
                                                       where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                         && f.IsActive == true && f.IsDeleted == false && f.ItemId == a.ItemId
                         && DbFunctions.TruncateTime(e.SalesDate) >= obj.FromDate &&
           DbFunctions.TruncateTime(e.SalesDate) <= obj.ToDate
                                                       select f.AmountIncTax).DefaultIfEmpty().Sum()
                                     })).Where(a => a.TotalSales > 0).ToList();
                }
            }
            else
            {
                if (obj.BranchId == 0)
                {
                    det = (from a in oConnectionContext.DbClsItemBranchMap
                           join b in oConnectionContext.DbClsItemDetails
                           on a.ItemDetailsId equals b.ItemDetailsId
                           join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                           where oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId) && c.ProductType != "Combo"
                           select new ClsItemVm
                           {
                               ProductType = c.ProductType,
                               ItemId = b.ItemId,
                               ItemDetailsId = a.ItemDetailsId,
                               ItemName = c.ItemName,
                               SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                               VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                               TotalSales = (from e in oConnectionContext.DbClsSales
                                             join f in oConnectionContext.DbClsSalesDetails
      on e.SalesId equals f.SalesId
                                             where e.Status != "Draft" &&
                                             //e.BranchId == obj.BranchId 
                                             oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == e.BranchId)
                                             && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
               && f.IsActive == true && f.IsDeleted == false && f.ItemDetailsId == b.ItemDetailsId
               && DbFunctions.TruncateTime(e.SalesDate) >= obj.FromDate &&
           DbFunctions.TruncateTime(e.SalesDate) <= obj.ToDate
                                             select f.AmountIncTax).Count()
                           }).Union((from a in oConnectionContext.DbClsItem
                                     where a.ProductType == "Combo" &&
    //oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
    oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId
    //&& b.BranchId == obj.BranchId
    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
   )
                                     select new ClsItemVm
                                     {
                                         ProductType = a.ProductType,
                                         ItemId = a.ItemId,
                                         ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                         ItemName = a.ItemName,
                                         SKU = a.SkuCode,
                                         VariationName = "",
                                         TotalSales = (from e in oConnectionContext.DbClsSales
                                                       join f in oConnectionContext.DbClsSalesDetails
    on e.SalesId equals f.SalesId
                                                       where e.Status != "Draft" &&
                                                       //e.BranchId == obj.BranchId 
                                                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == e.BranchId)
                                                       && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                         && f.IsActive == true && f.IsDeleted == false && f.ItemId == a.ItemId
                         && DbFunctions.TruncateTime(e.SalesDate) >= obj.FromDate &&
           DbFunctions.TruncateTime(e.SalesDate) <= obj.ToDate
                                                       select f.AmountIncTax).Count()
                                     })).Where(a => a.TotalSales > 0).ToList();
                }
                else
                {
                    det = (from a in oConnectionContext.DbClsItemBranchMap
                           join b in oConnectionContext.DbClsItemDetails
                           on a.ItemDetailsId equals b.ItemDetailsId
                           join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                           where a.BranchId == obj.BranchId && c.ProductType != "Combo"
                           select new ClsItemVm
                           {
                               ProductType = c.ProductType,
                               ItemId = b.ItemId,
                               ItemDetailsId = a.ItemDetailsId,
                               ItemName = c.ItemName,
                               SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                               VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                               TotalSales = (from e in oConnectionContext.DbClsSales
                                             join f in oConnectionContext.DbClsSalesDetails
      on e.SalesId equals f.SalesId
                                             where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
               && f.IsActive == true && f.IsDeleted == false && f.ItemDetailsId == b.ItemDetailsId
               && DbFunctions.TruncateTime(e.SalesDate) >= obj.FromDate &&
           DbFunctions.TruncateTime(e.SalesDate) <= obj.ToDate
                                             select f.AmountIncTax).Count()
                           }).Union((from a in oConnectionContext.DbClsItem
                                     where a.ProductType == "Combo" &&
    oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
                                     select new ClsItemVm
                                     {
                                         ProductType = a.ProductType,
                                         ItemId = a.ItemId,
                                         ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                                         ItemName = a.ItemName,
                                         SKU = a.SkuCode,
                                         VariationName = "",
                                         TotalSales = (from e in oConnectionContext.DbClsSales
                                                       join f in oConnectionContext.DbClsSalesDetails
    on e.SalesId equals f.SalesId
                                                       where e.Status != "Draft" && e.BranchId == obj.BranchId && e.IsActive == true && e.IsDeleted == false && e.IsCancelled == false
                         && f.IsActive == true && f.IsDeleted == false && f.ItemId == a.ItemId
                         && DbFunctions.TruncateTime(e.SalesDate) >= obj.FromDate &&
           DbFunctions.TruncateTime(e.SalesDate) <= obj.ToDate
                                                       select f.AmountIncTax).Count()
                                     })).Where(a => a.TotalSales > 0).ToList();
                }
            }

            if (obj.CategoryId != 0)
            {
                det = det.Where(a => a.CategoryId == obj.CategoryId).ToList();
            }

            if (obj.SubCategoryId != 0)
            {
                det = det.Where(a => a.SubCategoryId == obj.SubCategoryId).ToList();
            }

            if (obj.SubSubCategoryId != 0)
            {
                det = det.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).ToList();
            }
            if (obj.BrandId != 0)
            {
                det = det.Where(a => a.BrandId == obj.BrandId).ToList();
            }

            if (obj.ItemDetailsId != 0)
            {
                det = det.Where(a => a.ItemDetailsId == obj.ItemDetailsId).ToList();
            }

            if (obj.ProductType != "" && obj.ProductType != null)
            {
                det = det.Where(a => a.ProductType.ToLower() == obj.ProductType.ToLower()).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Dashboard = new
                    {
                        TopItems = det.OrderByDescending(a => a.TotalSales).Skip(skip).Take(obj.PageSize).ToList(),
                    },
                    TotalCount = det.Count(),
                    //Branchs = userDetails?.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockAlertReport(ClsItemVm obj)
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

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsItemVm> det;
            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsItemBranchMap
                       join b in oConnectionContext.DbClsItemDetails
                       on a.ItemDetailsId equals b.ItemDetailsId
                       join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                       where
                       //a.BranchId == obj.BranchId 
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && c.ProductType != "Combo" && c.IsManageStock == true && a.Quantity <= c.AlertQuantity
                       && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                       select new ClsItemVm
                       {
                           ProductType = c.ProductType,
                           ItemId = b.ItemId,
                           ItemDetailsId = a.ItemDetailsId,
                           ItemName = c.ItemName,
                           SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           Quantity = a.Quantity,
                           //UnitName = c.QuaternaryUnitId != 0 ? oConnectionContext.DbClsQuaternaryUnit.Where(bb => bb.QuaternaryUnitId == c.QuaternaryUnitId).Select(bb => bb.QuaternaryUnitName).FirstOrDefault()
                           //        : c.TertiaryUnitId != 0 ? oConnectionContext.DbClsTertiaryUnit.Where(bb => bb.TertiaryUnitId == c.TertiaryUnitId).Select(bb => bb.TertiaryUnitName).FirstOrDefault()
                           //        : c.SecondaryUnitId != 0 ? oConnectionContext.DbClsSecondaryUnit.Where(bb => bb.SecondaryUnitId == c.SecondaryUnitId).Select(bb => bb.SecondaryUnitName).FirstOrDefault()
                           //        : oConnectionContext.DbClsUnit.Where(bb => bb.UnitId == c.UnitId).Select(bb => bb.UnitName).FirstOrDefault(),
                       })
                       //                       .Union((from b in oConnectionContext.DbClsItemBranchMap
                       //                                 join a in oConnectionContext.DbClsItem
                       //on b.ItemId equals a.ItemId
                       //                                 where a.ProductType == "Combo" &&
                       //                                 //b.BranchId == obj.BranchId && 
                       //                                 oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       //    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId) &&
                       //                                 a.IsManageStock == true && b.Quantity <= a.AlertQuantity
                       //                                 && b.IsDeleted == false && b.IsCancelled == false && a.IsDeleted == false && a.IsCancelled == false
                       //                                 select new ClsItemVm
                       //                                 {
                       //                                     ProductType = a.ProductType,
                       //                                     ItemId = a.ItemId,
                       //                                     ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                       //                                     ItemName = a.ItemName,
                       //                                     SKU = a.SkuCode,
                       //                                     VariationName = "",
                       //                                     Quantity = b.Quantity,
                       //                                     //  UnitName = a.QuaternaryUnitId != 0 ? oConnectionContext.DbClsQuaternaryUnit.Where(bb => bb.QuaternaryUnitId == a.QuaternaryUnitId).Select(bb => bb.QuaternaryUnitName).FirstOrDefault()
                       //                                     //: a.TertiaryUnitId != 0 ? oConnectionContext.DbClsTertiaryUnit.Where(bb => bb.TertiaryUnitId == a.TertiaryUnitId).Select(bb => bb.TertiaryUnitName).FirstOrDefault()
                       //                                     //: a.SecondaryUnitId != 0 ? oConnectionContext.DbClsSecondaryUnit.Where(bb => bb.SecondaryUnitId == a.SecondaryUnitId).Select(bb => bb.SecondaryUnitName).FirstOrDefault()
                       //                                     //: oConnectionContext.DbClsUnit.Where(bb => bb.UnitId == a.UnitId).Select(bb => bb.UnitName).FirstOrDefault(),
                       //                                 }))
                       .ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsItemBranchMap
                       join b in oConnectionContext.DbClsItemDetails
                       on a.ItemDetailsId equals b.ItemDetailsId
                       join c in oConnectionContext.DbClsItem on b.ItemId equals c.ItemId
                       where a.BranchId == obj.BranchId && c.ProductType != "Combo" && c.IsManageStock == true && a.Quantity <= c.AlertQuantity
                       && a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                       select new ClsItemVm
                       {
                           ProductType = c.ProductType,
                           ItemId = b.ItemId,
                           ItemDetailsId = a.ItemDetailsId,
                           ItemName = c.ItemName,
                           SKU = c.ProductType == "Single" ? c.SkuCode : b.SKU,
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           Quantity = a.Quantity,
                           //UnitName = c.QuaternaryUnitId != 0 ? oConnectionContext.DbClsQuaternaryUnit.Where(bb => bb.QuaternaryUnitId == c.QuaternaryUnitId).Select(bb => bb.QuaternaryUnitName).FirstOrDefault()
                           //        : c.TertiaryUnitId != 0 ? oConnectionContext.DbClsTertiaryUnit.Where(bb => bb.TertiaryUnitId == c.TertiaryUnitId).Select(bb => bb.TertiaryUnitName).FirstOrDefault()
                           //        : c.SecondaryUnitId != 0 ? oConnectionContext.DbClsSecondaryUnit.Where(bb => bb.SecondaryUnitId == c.SecondaryUnitId).Select(bb => bb.SecondaryUnitName).FirstOrDefault()
                           //        : oConnectionContext.DbClsUnit.Where(bb => bb.UnitId == c.UnitId).Select(bb => bb.UnitName).FirstOrDefault(),
                       })
                       //                       .Union((from b in oConnectionContext.DbClsItemBranchMap
                       //                                 join a in oConnectionContext.DbClsItem
                       //on b.ItemId equals a.ItemId
                       //                                 where a.ProductType == "Combo" && b.BranchId == obj.BranchId && a.IsManageStock == true && b.Quantity <= a.AlertQuantity
                       //                                 && b.IsDeleted == false && b.IsCancelled == false && a.IsDeleted == false && a.IsCancelled == false
                       //                                 select new ClsItemVm
                       //                                 {
                       //                                     ProductType = a.ProductType,
                       //                                     ItemId = a.ItemId,
                       //                                     ItemDetailsId = oConnectionContext.DbClsItemDetails.Where(c => c.ItemId == a.ItemId).Select(c => c.ItemDetailsId).FirstOrDefault(),
                       //                                     ItemName = a.ItemName,
                       //                                     SKU = a.SkuCode,
                       //                                     VariationName = "",
                       //                                     Quantity = b.Quantity,
                       //                                     //  UnitName = a.QuaternaryUnitId != 0 ? oConnectionContext.DbClsQuaternaryUnit.Where(bb => bb.QuaternaryUnitId == a.QuaternaryUnitId).Select(bb => bb.QuaternaryUnitName).FirstOrDefault()
                       //                                     //: a.TertiaryUnitId != 0 ? oConnectionContext.DbClsTertiaryUnit.Where(bb => bb.TertiaryUnitId == a.TertiaryUnitId).Select(bb => bb.TertiaryUnitName).FirstOrDefault()
                       //                                     //: a.SecondaryUnitId != 0 ? oConnectionContext.DbClsSecondaryUnit.Where(bb => bb.SecondaryUnitId == a.SecondaryUnitId).Select(bb => bb.SecondaryUnitName).FirstOrDefault()
                       //                                     //: oConnectionContext.DbClsUnit.Where(bb => bb.UnitId == a.UnitId).Select(bb => bb.UnitName).FirstOrDefault(),
                       //                                 }))
                       .ToList();
            }

            //if (obj.ProductType != "" && obj.ProductType != null)
            //{
            //    det = det.Where(a => a.ProductType.ToLower() == obj.ProductType.ToLower()).ToList();
            //}

            //if (obj.ItemId != 0)
            //{
            //    det = det.Where(a => a.ItemId == obj.ItemId).ToList();
            //}

            List<ClsItemVm> _det1 = new List<ClsItemVm>();
            List<ClsItemVm> _det2 = new List<ClsItemVm>();

            _det1 = det.OrderBy(a => a.Quantity).Skip(skip).Take(obj.PageSize).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            foreach (var item in _det1)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
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

                _det2.Add(new ClsItemVm
                {
                    ProductType = item.ProductType,
                    ItemId = item.ItemId,
                    ItemDetailsId = item.ItemDetailsId,
                    ItemName = item.ItemName,
                    SKU = item.SKU,
                    VariationName = item.VariationName,
                    Quantity = item.Quantity,
                    UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit
                });
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Items = _det2,
                    TotalCount = det.Count(),
                    //Branchs = userDetails?.BranchIds,
                    PageSize = obj.PageSize
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemPurchaseSalesReport(ClsPurchaseSales obj)
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

            List<ClsPurchaseSales> det = (from a in oConnectionContext.DbClsItemBranchMap
                                          join x in oConnectionContext.DbClsItemDetails
                                          on a.ItemDetailsId equals x.ItemDetailsId
                                          join y in oConnectionContext.DbClsItem on x.ItemId equals y.ItemId
                                          where a.BranchId == obj.BranchId && y.ProductType != "Combo" && a.IsDeleted == false
                                          && x.IsDeleted == false && y.IsDeleted == false
                                          select new ClsPurchaseSales
                                          {
                                              Name = y.ItemName + " - " + oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == x.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault() + " - " + (y.ProductType == "Single" ? y.SkuCode : x.SKU),
                                              TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                                                               join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                               where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                               select b.GrandTotal).DefaultIfEmpty().Sum(),
                                              TotalPurchasePaid = (from b in oConnectionContext.DbClsPurchase
                                                                   join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                   join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                                                   where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && e.Type == "Purchase" &&
                                                                   b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                   select e.Amount).DefaultIfEmpty().Sum(),
                                              TotalPurchaseDue = (from b in oConnectionContext.DbClsPurchase
                                                                  join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                  where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                                  select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                                                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                                                 join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                                                                                                 where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && e.Type == "Purchase" &&
                                                                                                                 b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                                                                 select e.Amount).DefaultIfEmpty().Sum(),
                                              TotalSales = (from b in oConnectionContext.DbClsSales
                                                            join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                            where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                            select b.GrandTotal).DefaultIfEmpty().Sum(),
                                              TotalSalesPaid = (from b in oConnectionContext.DbClsSales
                                                                join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                                                where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && (e.Type.ToLower() == "sales payment") &&
                                                                b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                select e.Amount).DefaultIfEmpty().Sum(),
                                              TotalSalesDue = (from b in oConnectionContext.DbClsSales
                                                               join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                               where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                               select b.GrandTotal - b.WriteOffAmount).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsSales
                                                                                                                                 join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                                                                                 join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                                                                                                                 where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && (e.Type.ToLower() == "sales payment") &&
                                                                                                                                 b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                                                                                 select e.Amount).DefaultIfEmpty().Sum(),
                                          }).Concat((from a in oConnectionContext.DbClsItem
                                                     where a.ProductType == "Combo" &&
                  oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
                                                     select new ClsPurchaseSales
                                                     {
                                                         Name = a.ItemName + " - " + a.SkuCode,
                                                         TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                                                                          join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                          where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                                          select b.GrandTotal).DefaultIfEmpty().Sum(),
                                                         TotalPurchasePaid = (from b in oConnectionContext.DbClsPurchase
                                                                              join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                              join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                                                              where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && e.Type == "Purchase" &&
                                                                              b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                              select e.Amount).DefaultIfEmpty().Sum(),
                                                         TotalPurchaseDue = (from b in oConnectionContext.DbClsPurchase
                                                                             join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                             where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                                             select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                                                            join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                                                            join e in oConnectionContext.DbClsSupplierPayment on b.PurchaseId equals e.PurchaseId
                                                                                                                            where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && e.Type == "Purchase" &&
                                                                                                                            b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                                                                            select e.Amount).DefaultIfEmpty().Sum(),
                                                         TotalSales = (from b in oConnectionContext.DbClsSales
                                                                       join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                       where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                                       select b.GrandTotal).DefaultIfEmpty().Sum(),
                                                         TotalSalesPaid = (from b in oConnectionContext.DbClsSales
                                                                           join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                           join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                                                           where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && (e.Type.ToLower() == "sales payment") &&
                                                                           b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                           select e.Amount).DefaultIfEmpty().Sum(),
                                                         TotalSalesDue = (from b in oConnectionContext.DbClsSales
                                                                          join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                          where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                                          select b.GrandTotal - b.WriteOffAmount).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsSales
                                                                                                                                            join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                                                                                            join e in oConnectionContext.DbClsCustomerPayment on b.SalesId equals e.SalesId
                                                                                                                                            where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && (e.Type.ToLower() == "sales payment") &&
                                                                                                                                            b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && e.IsDeleted == false && e.IsCancelled == false
                                                                                                                                            select e.Amount).DefaultIfEmpty().Sum(),
                                                     })).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseSales = det.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemProfitLossReport(ClsPurchaseSales obj)
        {
            List<ClsPurchaseSales> det = (from a in oConnectionContext.DbClsItemBranchMap
                                          join x in oConnectionContext.DbClsItemDetails
                                          on a.ItemDetailsId equals x.ItemDetailsId
                                          join y in oConnectionContext.DbClsItem on x.ItemId equals y.ItemId
                                          where a.BranchId == obj.BranchId && y.ProductType != "Combo" && a.IsDeleted == false
                                          && x.IsDeleted == false && y.IsDeleted == false
                                          select new ClsPurchaseSales
                                          {
                                              Name = y.ItemName + " - " + oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == x.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault() + " - " + (y.ProductType == "Single" ? y.SkuCode : x.SKU),
                                              //TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                                              //                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                              //                 where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                              //                 select b.GrandTotal).DefaultIfEmpty().Sum(),
                                              //TotalSales = (from b in oConnectionContext.DbClsSales
                                              //              join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                              //              where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                              //              select b.GrandTotal).DefaultIfEmpty().Sum(),
                                              //TotalSalesQuantity = (from b in oConnectionContext.DbClsSales
                                              //                      join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                              //                      where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                              //                      select b.TotalQuantity).DefaultIfEmpty().Sum(),
                                              TotalGrossProfit = (from b in oConnectionContext.DbClsSales
                                                                  join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                  where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                                  select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                                                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                                                 where b.BranchId == obj.BranchId && c.ItemDetailsId == x.ItemDetailsId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                                                                                 select b.GrandTotal).DefaultIfEmpty().Sum(),
                                          }).Concat((from a in oConnectionContext.DbClsItem
                                                     where a.ProductType == "Combo" &&
                  oConnectionContext.DbClsItemBranchMap.Any(b => b.ItemId == a.ItemId && b.BranchId == obj.BranchId)
                                                     select new ClsPurchaseSales
                                                     {
                                                         Name = a.ItemName + " - " + a.SkuCode,
                                                         //TotalPurchase = (from b in oConnectionContext.DbClsPurchase
                                                         //                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                         //                 where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                                         //                 select b.GrandTotal).DefaultIfEmpty().Sum(),
                                                         //TotalSales = (from b in oConnectionContext.DbClsSales
                                                         //              join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                         //              where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                                         //              select b.GrandTotal).DefaultIfEmpty().Sum(),
                                                         //TotalSalesQuantity = (from b in oConnectionContext.DbClsSales
                                                         //                      join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                         //                      where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false && c.IsCancelled == false
                                                         //                      select b.TotalQuantity).DefaultIfEmpty().Sum(),
                                                         TotalGrossProfit = (from b in oConnectionContext.DbClsSales
                                                                             join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                                                             where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                                             select b.GrandTotal).DefaultIfEmpty().Sum() - (from b in oConnectionContext.DbClsPurchase
                                                                                                                            join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                                                                            where b.BranchId == obj.BranchId && c.ItemId == a.ItemId && b.IsDeleted == false && b.IsCancelled == false && c.IsDeleted == false
                                                                                                                            select b.GrandTotal).DefaultIfEmpty().Sum(),
                                                     })).OrderByDescending(a => a.TotalGrossProfit).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseSales = det
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemsReport(ClsSalesDetailsVm obj)
        {
            if (obj.PurchaseFromDate == DateTime.MinValue)
            {
                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

                obj.PurchaseFromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
                if (obj.PurchaseFromDate > DateTime.Now)
                {
                    obj.PurchaseFromDate = obj.PurchaseFromDate.AddYears(-1);
                }

                obj.PurchaseToDate = obj.PurchaseFromDate.AddMonths(11);

                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.PurchaseToDate.Year, obj.PurchaseToDate.Month);

                obj.PurchaseToDate = obj.PurchaseToDate.AddDays(days - 1);
            }

            obj.PurchaseFromDate = obj.PurchaseFromDate.AddHours(5).AddMinutes(30);
            obj.PurchaseToDate = obj.PurchaseToDate.AddHours(5).AddMinutes(30);

            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            List<ClsSalesDetailsVm> det;
            if (obj.BranchId == 0)
            {
                det = (from b in oConnectionContext.DbClsSalesDetails
                       join a in oConnectionContext.DbClsSales
                       on b.SalesId equals a.SalesId
                       join c in oConnectionContext.DbClsItemDetails
                       on b.ItemDetailsId equals c.ItemDetailsId
                       join d in oConnectionContext.DbClsItem
                       on c.ItemId equals d.ItemId
                       where b.IsDeleted == false && a.IsDeleted == false && a.IsCancelled == false && a.CompanyId == obj.CompanyId
                       && a.Status != "Draft" && a.IsCancelled == false
                       //&& a.BranchId == obj.BranchId
                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       select new ClsSalesDetailsVm
                       {
                           CustomerId = a.CustomerId,
                           BranchId = a.BranchId,
                           UnitName = b.PriceAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == oConnectionContext.DbClsItem.Where(dd => dd.ItemId == b.ItemId).Select(dd => dd.UnitId).FirstOrDefault()).Select(c => c.UnitName).FirstOrDefault() : b.PriceAddedFor == 2 ?
                           oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == oConnectionContext.DbClsItem.Where(dd => dd.ItemId == b.ItemId).Select(dd => dd.SecondaryUnitId).FirstOrDefault()).Select(c => c.SecondaryUnitName).FirstOrDefault() : b.PriceAddedFor == 3 ?
                           oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == oConnectionContext.DbClsItem.Where(dd => dd.ItemId == b.ItemId).Select(dd => dd.TertiaryUnitId).FirstOrDefault()).Select(c => c.TertiaryUnitName).FirstOrDefault() :
                           oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == oConnectionContext.DbClsItem.Where(dd => dd.ItemId == b.ItemId).Select(dd => dd.QuaternaryUnitId).FirstOrDefault()).Select(c => c.QuaternaryUnitName).FirstOrDefault(),
                           BranchName = oConnectionContext.DbClsBranch.Where(cc => cc.BranchId == a.BranchId).Select(cc => cc.Branch).FirstOrDefault(),
                           Discount = b.Discount,
                           Quantity = b.Quantity,
                           FreeQuantity = b.FreeQuantity,
                           CategoryId = d.CategoryId,
                           SubCategoryId = d.SubCategoryId,
                           SubSubCategoryId = d.SubSubCategoryId,
                           BrandId = d.BrandId,
                           VariationDetails = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           SKU = d.ProductType.ToLower() == "single" ? d.SkuCode : c.SKU,
                           PriceIncTax = b.PriceIncTax,
                           GrandTotal = b.AmountIncTax,
                           InvoiceNo = a.InvoiceNo,
                           StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(cc => cc.SalesDetailsId ==
                           b.SalesDetailsId).Select(cc => new ClsStockDeductionIds
                           {
                               Id = cc.Id,
                               Type = cc.Type,
                               Quantity = cc.Quantity
                           }).ToList(),
                           SalesDetailsId = b.SalesDetailsId,
                           SalesId = a.SalesId,
                           SalesDate = a.SalesDate,
                           ItemId = b.ItemId,
                           ItemDetailsId = b.ItemDetailsId,
                           ItemName = d.ItemName,
                           CustomerName = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.Name).FirstOrDefault(),
                           CustomerMobileNo = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.MobileNo).FirstOrDefault(),
                       }).ToList();
            }
            else
            {
                det = (from b in oConnectionContext.DbClsSalesDetails
                       join a in oConnectionContext.DbClsSales
                       on b.SalesId equals a.SalesId
                       join c in oConnectionContext.DbClsItemDetails
                       on b.ItemDetailsId equals c.ItemDetailsId
                       join d in oConnectionContext.DbClsItem
                       on c.ItemId equals d.ItemId
                       where b.IsDeleted == false && a.IsDeleted == false && a.IsCancelled == false && a.CompanyId == obj.CompanyId
                       && a.Status != "Draft" && a.IsCancelled == false
                       && a.BranchId == obj.BranchId
                       select new ClsSalesDetailsVm
                       {
                           CustomerId = a.CustomerId,
                           BranchId = a.BranchId,
                           UnitName = b.PriceAddedFor == 1 ? oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == oConnectionContext.DbClsItem.Where(dd => dd.ItemId == b.ItemId).Select(dd => dd.UnitId).FirstOrDefault()).Select(c => c.UnitName).FirstOrDefault() : b.PriceAddedFor == 2 ?
                           oConnectionContext.DbClsSecondaryUnit.Where(cc => cc.SecondaryUnitId == oConnectionContext.DbClsItem.Where(dd => dd.ItemId == b.ItemId).Select(dd => dd.SecondaryUnitId).FirstOrDefault()).Select(c => c.SecondaryUnitName).FirstOrDefault() : b.PriceAddedFor == 3 ?
                           oConnectionContext.DbClsTertiaryUnit.Where(cc => cc.TertiaryUnitId == oConnectionContext.DbClsItem.Where(dd => dd.ItemId == b.ItemId).Select(dd => dd.TertiaryUnitId).FirstOrDefault()).Select(c => c.TertiaryUnitName).FirstOrDefault() :
                           oConnectionContext.DbClsQuaternaryUnit.Where(cc => cc.QuaternaryUnitId == oConnectionContext.DbClsItem.Where(dd => dd.ItemId == b.ItemId).Select(dd => dd.QuaternaryUnitId).FirstOrDefault()).Select(c => c.QuaternaryUnitName).FirstOrDefault(),
                           BranchName = oConnectionContext.DbClsBranch.Where(cc => cc.BranchId == a.BranchId).Select(cc => cc.Branch).FirstOrDefault(),
                           Discount = b.Discount,
                           Quantity = b.Quantity,
                           FreeQuantity = b.FreeQuantity,
                           CategoryId = d.CategoryId,
                           SubCategoryId = d.SubCategoryId,
                           SubSubCategoryId = d.SubSubCategoryId,
                           BrandId = d.BrandId,
                           VariationDetails = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           SKU = d.ProductType.ToLower() == "single" ? d.SkuCode : c.SKU,
                           PriceIncTax = b.PriceIncTax,
                           GrandTotal = b.AmountIncTax,
                           InvoiceNo = a.InvoiceNo,
                           StockDeductionIds = oConnectionContext.DbClsSalesDeductionId.Where(cc => cc.SalesDetailsId ==
                           b.SalesDetailsId).Select(cc => new ClsStockDeductionIds
                           {
                               Id = cc.Id,
                               Type = cc.Type,
                               Quantity = cc.Quantity
                           }).ToList(),
                           SalesDetailsId = b.SalesDetailsId,
                           SalesId = a.SalesId,
                           SalesDate = a.SalesDate,
                           ItemId = b.ItemId,
                           ItemDetailsId = b.ItemDetailsId,
                           ItemName = d.ItemName,
                           CustomerName = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.Name).FirstOrDefault(),
                           CustomerMobileNo = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.MobileNo).FirstOrDefault(),
                       }).ToList();
            }

            if (obj.SalesFromDate != DateTime.MinValue)
            {
                det = det.Where(a => a.SalesDate >= obj.SalesFromDate &&
                    a.SalesDate <= obj.SalesToDate).ToList();
            }

            if (obj.CategoryId != 0)
            {
                det = det.Where(a => a.CategoryId == obj.CategoryId).ToList();
            }

            if (obj.SubCategoryId != 0)
            {
                det = det.Where(a => a.SubCategoryId == obj.SubCategoryId).ToList();
            }

            if (obj.SubSubCategoryId != 0)
            {
                det = det.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).ToList();
            }

            if (obj.BrandId != 0)
            {
                det = det.Where(a => a.BrandId == obj.BrandId).ToList();
            }

            if (obj.SKU != null && obj.SKU != "")
            {
                det = det.Where(a => a.SKU.ToLower().Contains(obj.SKU.ToLower())).ToList();
            }

            if (obj.ItemDetailsId != 0)
            {
                det = det.Where(a => a.ItemDetailsId == obj.ItemDetailsId).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).ToList();
            }

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            List<ClsSalesDetailsVm> SalesDetails = new List<ClsSalesDetailsVm>();
            foreach (var item in det)
            {
                if (item.StockDeductionIds != null && item.StockDeductionIds.Count > 0)
                {
                    //List<ClsStockDeductionIds> _StockDeductionIds = serializer.Deserialize<List<ClsStockDeductionIds>>(item.StockDeductionIds);
                    foreach (var inner in item.StockDeductionIds)
                    {
                        DateTime? purchaseDate = null;
                        string purchaseInvoiceNo = "";
                        long PurchaseId = 0;
                        string lotNo = "";
                        string supplierName = "";
                        string supplierMobileNo = "";
                        decimal purchasePrice = 0;
                        string Type = "";
                        long SupplierId = 0;

                        if (inner.Type == "purchase")
                        {
                            var purchase = (from a in oConnectionContext.DbClsPurchase
                                            join b in oConnectionContext.DbClsPurchaseDetails
    on a.PurchaseId equals b.PurchaseId
                                            where b.PurchaseDetailsId == inner.Id
                                            && a.Status != "Draft" && a.IsCancelled == false
                                            select new
                                            {
                                                a.PurchaseDate,
                                                a.ReferenceNo,
                                                b.LotNo,
                                                SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
                                                SupplierMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.MobileNo).FirstOrDefault(),
                                                b.PurchaseIncTax,
                                                a.PurchaseId,
                                                a.SupplierId
                                            }).FirstOrDefault();

                            purchaseDate = purchase.PurchaseDate;
                            purchaseInvoiceNo = purchase.ReferenceNo;
                            PurchaseId = purchase.PurchaseId;
                            lotNo = purchase.LotNo;
                            supplierName = purchase.SupplierName;
                            supplierMobileNo = purchase.SupplierMobileNo;
                            purchasePrice = purchase.PurchaseIncTax;
                            Type = "Purchase";
                            SupplierId = purchase.SupplierId;
                        }
                        else if (inner.Type == "openingstock")
                        {
                            var openingStock = (from a in oConnectionContext.DbClsOpeningStock
                                                where a.OpeningStockId == inner.Id
                                                select new
                                                {
                                                    a.Date,
                                                    a.LotNo,
                                                    a.UnitCost
                                                }).FirstOrDefault();

                            purchaseDate = openingStock.Date;
                            purchaseInvoiceNo = "(Opening Stock)";
                            lotNo = openingStock.LotNo;
                            purchasePrice = openingStock.UnitCost;
                            Type = "Opening Stock";
                        }
                        //             else if (inner.Type == "stockadjustment")
                        //             {
                        //                 var stockAdjustment = (from a in oConnectionContext.DbClsStockAdjustment
                        //                                        join b in oConnectionContext.DbClsStockAdjustmentDetails
                        //on a.StockAdjustmentId equals b.StockAdjustmentId
                        //                                        where b.StockAdjustmentDetailsId == inner.Id
                        //                                        select new
                        //                                        {
                        //                                            a.AdjustmentDate,
                        //                                            a.ReferenceNo,
                        //                                            //b.LotNo,
                        //                                            b.UnitCost
                        //                                        }).FirstOrDefault();

                        //                 purchaseDate = stockAdjustment.AdjustmentDate;
                        //                 purchaseInvoiceNo = stockAdjustment.ReferenceNo + " (Stock Adjustment)";
                        //                 lotNo = stockAdjustment.LotNo;
                        //                 purchasePrice = stockAdjustment.UnitCost;
                        //             }
                        else if (inner.Type == "stocktransfer")
                        {
                            var stockAdjustment = (from a in oConnectionContext.DbClsStockTransfer
                                                   join b in oConnectionContext.DbClsStockTransferDetails
           on a.StockTransferId equals b.StockTransferId
                                                   where b.StockTransferDetailsId == inner.Id
                                                   select new
                                                   {
                                                       a.Date,
                                                       a.ReferenceNo,
                                                       b.UnitCost,
                                                       a.StockTransferId
                                                   }).FirstOrDefault();

                            purchaseDate = stockAdjustment.Date;
                            purchaseInvoiceNo = stockAdjustment.ReferenceNo + " (Stock Transfer)";
                            PurchaseId = stockAdjustment.StockTransferId;
                            purchasePrice = stockAdjustment.UnitCost;
                            Type = "Stock Transfer";
                        }

                        var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                        {
                            a.UToSValue,
                            a.SToTValue,
                            a.TToQValue,
                            PrimaryUnit = a.PrimaryUnit,
                            SecondaryUnit = a.SecondaryUnit,
                            TertiaryUnit = a.TertiaryUnit,
                            QuaternaryUnit = a.QuaternaryUnit,
                        }).FirstOrDefault();

                        decimal TotalCurrentStock = inner.Quantity;

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

                        SalesDetails.Add(new ClsSalesDetailsVm
                        {
                            BranchId = item.BranchId,
                            ItemId = item.ItemId,
                            SalesId = item.SalesId,
                            UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit,
                            BranchName = item.BranchName,
                            Discount = item.Discount,
                            CategoryId = item.CategoryId,
                            SubCategoryId = item.SubCategoryId,
                            SubSubCategoryId = item.SubSubCategoryId,
                            BrandId = item.BrandId,
                            VariationDetails = item.VariationDetails,
                            ItemName = item.ItemName,
                            SKU = item.SKU,
                            PurchaseDate = purchaseDate,
                            PurchaseInvoiceNo = purchaseInvoiceNo,
                            LotNo = lotNo,
                            SupplierName = supplierName,
                            SupplierMobileNo = supplierMobileNo,
                            PurchaseIncTax = purchasePrice,
                            SalesDate = item.SalesDate,
                            InvoiceNo = item.InvoiceNo,
                            CustomerName = item.CustomerName,
                            CustomerMobileNo = item.CustomerMobileNo,
                            Quantity = TotalCurrentStock,
                            FreeQuantity = 0,
                            SalesIncTax = item.PriceIncTax,
                            AmountIncTax = item.GrandTotal,
                            PurchaseId = PurchaseId,
                            Type = Type,
                            CustomerId = item.CustomerId,
                            SupplierId = SupplierId
                        });
                    }
                }
                else
                {
                    SalesDetails.Add(new ClsSalesDetailsVm
                    {
                        BranchId = item.BranchId,
                        ItemId = item.ItemId,
                        SalesId = item.SalesId,
                        UnitName = item.UnitName,
                        BranchName = item.BranchName,
                        Discount = item.Discount,
                        CategoryId = item.CategoryId,
                        SubCategoryId = item.SubCategoryId,
                        SubSubCategoryId = item.SubSubCategoryId,
                        BrandId = item.BrandId,
                        VariationDetails = item.VariationDetails,
                        ItemName = item.ItemName,
                        SKU = item.SKU,
                        PurchaseDate = null,
                        PurchaseInvoiceNo = "",
                        LotNo = "",
                        Supplier = "",
                        PurchaseIncTax = 0,
                        SalesDate = item.SalesDate,
                        InvoiceNo = item.InvoiceNo,
                        CustomerName = item.CustomerName,
                        CustomerMobileNo = item.CustomerMobileNo,
                        Quantity = item.Quantity,
                        FreeQuantity = item.FreeQuantity,
                        SalesIncTax = item.PriceIncTax,
                        AmountIncTax = item.GrandTotal,
                        PurchaseId = 0,
                        Type = "",
                        CustomerId = item.CustomerId,
                        SupplierId = item.SupplierId
                    });
                }
            }

            if (obj.PurchaseFromDate != DateTime.MinValue)
            {
                det = SalesDetails.Where(a => a.PurchaseDate >= obj.PurchaseFromDate &&
                    a.PurchaseDate <= obj.PurchaseToDate).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesDetails = SalesDetails.OrderByDescending(a => a.SalesId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = SalesDetails.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.PurchaseFromDate,
                    ToDate = obj.PurchaseToDate
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AvailableSellingPriceGroups(ClsItemVm obj)
        {
            var Item = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.ItemId,
                a.ProductType
            }).FirstOrDefault();

            if (Item.ProductType == "Combo")
            {
                var ItemDetails = (from a in oConnectionContext.DbClsItemDetails
                                   join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                   where a.ItemId == Item.ItemId && b.IsActive == true && b.IsDeleted == false
                                   select new
                                   {
                                       b.ItemId,
                                       b.ProductType,
                                       a.ItemDetailsId,
                                       b.ItemName,
                                       SKU = a.SKU == null ? b.SkuCode : a.SKU,
                                       a.VariationDetailsId,
                                       VariationName = oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId).Select(c => c.VariationDetails).FirstOrDefault(),
                                       UnitName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == b.UnitId).Select(c => c.UnitName).FirstOrDefault(),
                                       a.SalesExcTax,
                                       a.SalesIncTax,
                                       a.TotalCost,
                                       b.ItemCode,
                                       SellingPriceGroups = oConnectionContext.DbClsSellingPriceGroup.Where(c => c.CompanyId == obj.CompanyId
                                       && c.IsDeleted == false && c.IsActive == true).Select(c => new
                                       {
                                           SellingPriceGroupId = c.SellingPriceGroupId,
                                           SellingPriceGroup = c.SellingPriceGroup,
                                           SellingPrice = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(d => d.ItemDetailsId == a.ItemDetailsId
                                           && d.SellingPriceGroupId == c.SellingPriceGroupId).Select(d => d.SellingPrice).DefaultIfEmpty().FirstOrDefault(),
                                           DiscountType = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(d => d.ItemDetailsId == a.ItemDetailsId
                                           && d.SellingPriceGroupId == c.SellingPriceGroupId).Select(d => d.DiscountType).DefaultIfEmpty().FirstOrDefault(),
                                       }).ToList()
                                   }).ToList().Take(1);

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        ItemDetails = ItemDetails,
                    }
                };
            }
            else
            {
                var ItemDetails = (from a in oConnectionContext.DbClsItemDetails
                                   join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                   where a.ItemId == Item.ItemId
                                   && b.IsActive == true && b.IsDeleted == false
                                   select new
                                   {
                                       b.ItemId,
                                       b.ProductType,
                                       a.ItemDetailsId,
                                       b.ItemName,
                                       SKU = a.SKU == null ? b.SkuCode : a.SKU,
                                       a.VariationDetailsId,
                                       VariationName = oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId).Select(c => c.VariationDetails).FirstOrDefault(),
                                       UnitName = oConnectionContext.DbClsUnit.Where(c => c.UnitId == b.UnitId).Select(c => c.UnitName).FirstOrDefault(),
                                       a.SalesExcTax,
                                       a.SalesIncTax,
                                       a.TotalCost,
                                       b.ItemCode,
                                       SellingPriceGroups = oConnectionContext.DbClsSellingPriceGroup.Where(c => c.CompanyId == obj.CompanyId
                                       && c.IsDeleted == false && c.IsActive == true).Select(c => new
                                       {
                                           SellingPriceGroupId = c.SellingPriceGroupId,
                                           SellingPriceGroup = c.SellingPriceGroup,
                                           SellingPrice = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(d => d.ItemDetailsId == a.ItemDetailsId
                                           && d.SellingPriceGroupId == c.SellingPriceGroupId).Select(d => d.SellingPrice).DefaultIfEmpty().FirstOrDefault(),
                                           DiscountType = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(d => d.ItemDetailsId == a.ItemDetailsId
                                           && d.SellingPriceGroupId == c.SellingPriceGroupId).Select(d => d.DiscountType).DefaultIfEmpty().FirstOrDefault(),
                                       }).ToList()
                                   }).ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        ItemDetails = ItemDetails,
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateAvailableSellingPriceGroups(ClsItemVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                foreach (var item in obj.ItemDetails)
                {
                    long ItemSellingPriceGroupMapId = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(a => a.CompanyId == obj.CompanyId && a.ItemDetailsId == item.ItemDetailsId
                    && a.ItemId == item.ItemId && a.SellingPriceGroupId == item.SellingPriceGroupId).Select(a => a.ItemSellingPriceGroupMapId).FirstOrDefault();

                    if (ItemSellingPriceGroupMapId == 0)
                    {
                        ClsItemSellingPriceGroupMap oClsItemSellingPriceGroupMap = new ClsItemSellingPriceGroupMap()
                        {
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            ItemDetailsId = item.ItemDetailsId,
                            ItemId = item.ItemId,
                            SellingPrice = item.SellingPrice,
                            SellingPriceGroupId = item.SellingPriceGroupId,
                            DiscountType = item.DiscountType
                        };
                        oConnectionContext.DbClsItemSellingPriceGroupMap.Add(oClsItemSellingPriceGroupMap);
                        oConnectionContext.SaveChanges();
                    }
                    else
                    {
                        ClsItemSellingPriceGroupMap oClsItemSellingPriceGroupMap = new ClsItemSellingPriceGroupMap()
                        {
                            ItemSellingPriceGroupMapId = ItemSellingPriceGroupMapId,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            SellingPrice = item.SellingPrice,
                            DiscountType = item.DiscountType
                        };

                        oConnectionContext.DbClsItemSellingPriceGroupMap.Attach(oClsItemSellingPriceGroupMap);
                        oConnectionContext.Entry(oClsItemSellingPriceGroupMap).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oClsItemSellingPriceGroupMap).Property(x => x.ModifiedOn).IsModified = true;
                        oConnectionContext.Entry(oClsItemSellingPriceGroupMap).Property(x => x.SellingPrice).IsModified = true;
                        oConnectionContext.Entry(oClsItemSellingPriceGroupMap).Property(x => x.DiscountType).IsModified = true;
                        oConnectionContext.SaveChanges();
                    }
                }

                long ItemId = obj.ItemDetails[0].ItemId;
                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Selling Price Group",
                    CompanyId = obj.CompanyId,
                    Description = "Selling Price Group for \"" + oConnectionContext.DbClsItem.Where(a => a.ItemId == ItemId).Select(a => a.SkuCode).FirstOrDefault() + "\" updated",
                    Id = 0,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ImportItem(ClsItemVm obj)
        {
            //using (TransactionScope dbContextTransaction = new TransactionScope())
            //{
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            if (obj.ItemImports == null || obj.ItemImports.Count == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "No data",
                    Data = new
                    {
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            //int TotalItemUsed = (from a in oConnectionContext.DbClsItemDetails
            //                     join b in oConnectionContext.DbClsItem
            //                        on a.ItemId equals b.ItemId
            //                     where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
            //                        && b.IsDeleted == false && b.IsCancelled == false
            //                     select a.ItemDetailsId).Count();
            //int TotalItem = oCommonController.fetchPlanQuantity(obj.CompanyId, "Item");
            //if ((TotalItemUsed + obj.ItemImports.Count) >= TotalItem)
            //{
            //    data = new
            //    {
            //        Status = 0,
            //        Message = "Item quota already used. Please upgrade addons from My Plan Menu",
            //        Data = new
            //        {
            //        }
            //    };
            //    return await Task.FromResult(Ok(data));
            //}

            ClsMenuVm oclsMenu = new ClsMenuVm { CompanyId = obj.CompanyId, AddedBy = obj.AddedBy };
            var PlanAddons = oCommonController.PlanAddons(oclsMenu);
            bool IsAccountsAddon = PlanAddons.Where(a => a.Type == "Accounts").Count() == 0 ? false : true;

            //bool EnableProductVariation = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.EnableProductVariation).FirstOrDefault();
            //bool isExcelHasVariation = false;

            List<ClsItemImport> oClsItemImports = new List<ClsItemImport>();
            int _rowNo = 1;
            if (obj.ItemImports != null)
            {
                foreach (var item in obj.ItemImports)
                {
                    if (item.SkuCode != "" && item.SkuCode != null)
                    {
                        oClsItemImports.Add(new ClsItemImport { SkuCode = item.SkuCode, RowNo = _rowNo });
                    }

                    if (item.ProductType != "" && item.ProductType != null)
                    {
                        if (item.ProductType.ToLower() == "variable")
                        {
                            //isExcelHasVariation = true;
                            if (item.VariationSKUs != "" && item.VariationSKUs != null)
                            {
                                var variationSKUs = item.VariationSKUs.Split('|');
                                for (int i = 0; i < variationSKUs.Length; i++)
                                {
                                    oClsItemImports.Add(new ClsItemImport { SkuCode = variationSKUs[i], RowNo = _rowNo });
                                }
                            }
                        }
                    }

                    _rowNo++;
                }
                ;
            }

            int count = 1;
            if (oClsItemImports != null)
            {
                foreach (var item in oClsItemImports)
                {
                    int innerCount = 1;

                    foreach (var inner in oClsItemImports)
                    {
                        if (item.SkuCode != "" && item.SkuCode != null)
                        {
                            if (item.SkuCode.ToLower() == inner.SkuCode.ToLower() && count != innerCount)
                            {
                                errors.Add(new ClsError { Message = "Duplicate Sku Code exists in row no " + item.RowNo, Id = "" });
                                isError = true;
                            }
                        }

                        //if (item.HsnCode != "" && item.HsnCode != null)
                        //{
                        //    if (item.HsnCode.ToLower() == inner.HsnCode.ToLower() && count != innerCount)
                        //    {
                        //        errors.Add(new ClsError { Message = "Duplicate Hsn Code exists in row no " + item.RowNo, Id = "" });
                        //        isError = true;
                        //    }
                        //}

                        innerCount++;
                    }
                    count++;
                }
            }

            count = 1;
            if (obj.ItemImports != null)
            {
                foreach (var item in obj.ItemImports)
                {
                    if (item.ItemType == "" || item.ItemType == null)
                    {
                        errors.Add(new ClsError { Message = "ItemType is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.SkuCode != "" && item.SkuCode != null)
                    {
                        if ((from a in oConnectionContext.DbClsItemDetails
                             join b in oConnectionContext.DbClsItem
                            on a.ItemId equals b.ItemId
                             where a.SKU.ToLower() == item.SkuCode.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false
                            && b.IsDeleted == false
                             select a.ItemId).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate SkuCode exists in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    //if (item.HsnCode != "" && item.HsnCode != null)
                    //{
                    //    if ((from a in oConnectionContext.DbClsItemDetails
                    //         join b in oConnectionContext.DbClsItem
                    //        on a.ItemId equals b.ItemId
                    //         where a.SKU.ToLower() == item.HsnCode.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                    //        && b.IsDeleted == false && b.IsCancelled == false
                    //         select a.ItemId).Count() > 0)
                    //    {
                    //        errors.Add(new ClsError { Message = "Duplicate HsnCode exists in row no " + count, Id = "" });
                    //        isError = true;
                    //    }
                    //}

                    if (item.ItemName == "" || item.ItemName == null)
                    {
                        errors.Add(new ClsError { Message = "ItemName is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.ItemType != null && item.ItemType != "")
                    {
                        if (item.ItemType == "Product")
                        {
                            if (item.UnitName == null || item.UnitName == "")
                            {
                                errors.Add(new ClsError { Message = "UnitName is required in row no " + count, Id = "" });
                                isError = true;
                            }

                            if (item.SecondaryUnitName != null && item.SecondaryUnitName != "")
                            {
                                if (item.UToSValue == 0)
                                {
                                    errors.Add(new ClsError { Message = "UToSValue is required in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                            if (item.UToSValue != 0)
                            {
                                if (item.SecondaryUnitName == null || item.SecondaryUnitName == "")
                                {
                                    errors.Add(new ClsError { Message = "SecondaryUnitName is required in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                            if (item.TertiaryUnitName != null && item.TertiaryUnitName != "")
                            {
                                if (item.SecondaryUnitName == null || item.SecondaryUnitName == "")
                                {
                                    errors.Add(new ClsError { Message = "SecondaryUnitName is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.UToSValue == 0)
                                {
                                    errors.Add(new ClsError { Message = "UToSValue is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.SToTValue == 0)
                                {
                                    errors.Add(new ClsError { Message = "SToTValue is required in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                            if (item.SToTValue != 0)
                            {
                                if (item.SecondaryUnitName == null || item.SecondaryUnitName == "")
                                {
                                    errors.Add(new ClsError { Message = "SecondaryUnitName is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.UToSValue == 0)
                                {
                                    errors.Add(new ClsError { Message = "UToSValue is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.TertiaryUnitName == null || item.TertiaryUnitName == "")
                                {
                                    errors.Add(new ClsError { Message = "TertiaryUnitName is required in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                            if (item.QuaternaryUnitName != null && item.QuaternaryUnitName != "")
                            {
                                if (item.SecondaryUnitName == null || item.SecondaryUnitName == "")
                                {
                                    errors.Add(new ClsError { Message = "SecondaryUnitName is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.UToSValue == 0)
                                {
                                    errors.Add(new ClsError { Message = "UToSValue is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.TertiaryUnitName == null || item.TertiaryUnitName == "")
                                {
                                    errors.Add(new ClsError { Message = "TertiaryUnitName is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.SToTValue == 0)
                                {
                                    errors.Add(new ClsError { Message = "SToTValue is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.TToQValue == 0)
                                {
                                    errors.Add(new ClsError { Message = "TToQValue is required in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                            if (item.TToQValue != 0)
                            {
                                if (item.SecondaryUnitName == null || item.SecondaryUnitName == "")
                                {
                                    errors.Add(new ClsError { Message = "SecondaryUnitName is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.UToSValue == 0)
                                {
                                    errors.Add(new ClsError { Message = "UToSValue is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.TertiaryUnitName == null || item.TertiaryUnitName == "")
                                {
                                    errors.Add(new ClsError { Message = "TertiaryUnitName is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.SToTValue == 0)
                                {
                                    errors.Add(new ClsError { Message = "SToTValue is required in row no " + count, Id = "" });
                                    isError = true;
                                }

                                if (item.QuaternaryUnitName == null || item.QuaternaryUnitName == "")
                                {
                                    errors.Add(new ClsError { Message = "QuaternaryUnitName is required in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                        }
                    }

                    //if (item.BarcodeType == null || item.BarcodeType == "")
                    //{
                    //    errors.Add(new ClsError { Message = "BarcodeType is required in row no " + count, Id = "" });
                    //    isError = true;
                    //}

                    if (item.CategoryName == "" || item.CategoryName == null)
                    {
                        errors.Add(new ClsError { Message = "CategoryName is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.SubCategoryName != "" && item.SubCategoryName != null)
                    {
                        if (item.CategoryName == "" || item.CategoryName == null)
                        {
                            errors.Add(new ClsError { Message = "CategoryName is required in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.SubSubCategoryName != "" && item.SubSubCategoryName != null)
                    {
                        //if (item.CategoryName == "" || item.CategoryName == null)
                        //{
                        //    errors.Add(new ClsError { Message = "CategoryName is required in row no " + count, Id = "" });
                        //    isError = true;
                        //}

                        if (item.SubCategoryName == "" || item.SubCategoryName == null)
                        {
                            errors.Add(new ClsError { Message = "SubCategoryName is required in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.BranchNames == null || item.BranchNames == "")
                    {
                        errors.Add(new ClsError { Message = "BranchNames is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.BranchNames != "" && item.BranchNames != null)
                    {
                        var branchs = item.BranchNames.Split('|');
                        foreach (var branch in branchs)
                        {
                            if (oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
                         a.Branch.ToLower().Trim() == branch.ToLower().Trim()).Count() == 0)
                            {
                                errors.Add(new ClsError { Message = "Invalid BranchNames in row no " + count, Id = "" });
                                isError = true;
                            }
                        }
                    }

                    if (item.Rack != "" && item.Rack != null)
                    {
                        var branchs = item.BranchNames.Split('|');
                        var racks = item.Rack.Split('|');
                        if (branchs.Count() != racks.Count())
                        {
                            errors.Add(new ClsError { Message = "Mismatch in no of Rack and BranchNames in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.Row != "" && item.Row != null)
                    {
                        var branchs = item.BranchNames.Split('|');
                        var rows = item.Row.Split('|');
                        if (branchs.Count() != rows.Count())
                        {
                            errors.Add(new ClsError { Message = "Mismatch in no of Row and BranchNames in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.Position != "" && item.Position != null)
                    {
                        var branchs = item.BranchNames.Split('|');
                        var positions = item.Position.Split('|');
                        if (branchs.Count() != positions.Count())
                        {
                            errors.Add(new ClsError { Message = "Mismatch in no of Position and BranchNames in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.TaxPreference == "" || item.TaxPreference == null)
                    {
                        errors.Add(new ClsError { Message = "TaxPreference is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.TaxPreference != "" && item.TaxPreference != null)
                    {
                        if (item.TaxPreference.Trim() == "Non-Taxable")
                        {
                            if (item.TaxExemptionReason == "" || item.TaxExemptionReason == null)
                            {
                                errors.Add(new ClsError { Message = "TaxExemptionReason is required in row no " + count, Id = "" });
                                isError = true;
                            }
                        }
                    }

                    if (item.TaxPreference != "" && item.TaxPreference != null)
                    {
                        if (item.TaxPreference.Trim() == "Taxable")
                        {
                            if (item.IntraStateTaxName == "" || item.IntraStateTaxName == null)
                            {
                                errors.Add(new ClsError { Message = "IntraStateTaxName is required in row no " + count, Id = "" });
                                isError = true;
                            }
                            else
                            {
                                if (oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == item.IntraStateTaxName.Trim().ToLower() &&
                                                       a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.TaxTypeId != 3
                                                       ).Select(a => a.TaxId).FirstOrDefault() == 0)
                                {
                                    errors.Add(new ClsError { Message = "Invalid IntraStateTaxName in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }
                        }
                    }

                    if (item.TaxPreference != "" && item.TaxPreference != null)
                    {
                        if (item.TaxPreference.Trim() == "Taxable")
                        {
                            if (item.InterStateTaxName == "" || item.InterStateTaxName == null)
                            {
                                errors.Add(new ClsError { Message = "InterStateTaxName is required in row no " + count, Id = "" });
                                isError = true;
                            }
                            else
                            {
                                if (oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == item.InterStateTaxName.Trim().ToLower() &&
                                                       a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
                                                       (a.TaxTypeId == 3 || a.TaxTypeId == 5)).Select(a => a.TaxId).FirstOrDefault() == 0)
                                {
                                    errors.Add(new ClsError { Message = "Invalid InterStateTaxName in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }
                        }
                    }

                    //if (item.TaxType == null || item.TaxType == "")
                    //{
                    //    errors.Add(new ClsError { Message = "TaxType is required in row no " + count, Id = "" });
                    //    isError = true;
                    //}

                    if (item.ProductType == "" || item.ProductType == null)
                    {
                        errors.Add(new ClsError { Message = "ProductType is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.PurchasePrice == "" || item.PurchasePrice == null)
                    {
                        errors.Add(new ClsError { Message = "PurchasePrice is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.SellingPrice == "" || item.SellingPrice == null)
                    {
                        errors.Add(new ClsError { Message = "SellingPrice is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.ProductType != "" && item.ProductType != null)
                    {
                        if (item.ProductType.ToLower() == "variable")
                        {
                            if (item.VariationGroupName == "" || item.VariationGroupName == null)
                            {
                                errors.Add(new ClsError { Message = "VariationGroupName is required in row no " + count, Id = "" });
                                isError = true;
                            }

                            if (item.VariationName == "" || item.VariationName == null)
                            {
                                errors.Add(new ClsError { Message = "VariationName is required in row no " + count, Id = "" });
                                isError = true;
                            }

                            if (item.VariationValues == "" || item.VariationValues == null)
                            {
                                errors.Add(new ClsError { Message = "VariationValues is required in row no " + count, Id = "" });
                                isError = true;
                            }

                            //if (item.VariationSKUs == "" || item.VariationSKUs == null)
                            //{
                            //    errors.Add(new ClsError { Message = "Variation SKUs is required in row no " + count, Id = "" });
                            //    isError = true;
                            //}

                            //if ((item.PurchasePriceIncTax == "" || item.PurchasePriceIncTax != null) && (item.PurchasePriceExcTax != "" && item.PurchasePriceExcTax != null))
                            //{
                            //    errors.Add(new ClsError { Message = "PurchasePriceIncTax/ PurchasePriceExcTax is required in row no " + count, Id = "" });
                            //    isError = true;
                            //}

                            //if (item.MRP == "" || item.MRP != null)
                            //{
                            //    errors.Add(new ClsError { Message = "PurchasePriceIncTax/PurchasePriceExcTax is required in row no " + count, Id = "" });
                            //    isError = true;
                            //}

                            if (item.VariationSKUs != "" && item.VariationSKUs != null)
                            {
                                var variationValues = item.VariationValues.Split('|');
                                var variationSKUs = item.VariationSKUs.Split('|');
                                if (variationValues.Count() != variationSKUs.Count())
                                {
                                    errors.Add(new ClsError { Message = "Mismatch in no of VariationSKUs and VariationValues in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                            if (item.PurchasePrice != "" && item.PurchasePrice != null)
                            {
                                var variationValues = item.VariationValues.Split('|');
                                var purchasePriceIncTax = item.PurchasePrice.Split('|');
                                if (variationValues.Count() != purchasePriceIncTax.Count())
                                {
                                    errors.Add(new ClsError { Message = "Mismatch in no of PurchasePrice and VariationValues in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                            //if (item.PurchasePriceExcTax != "" && item.PurchasePriceExcTax != null)
                            //{
                            //    var variationValues = item.VariationValues.Split('|');
                            //    var purchasePriceExcTax = item.PurchasePriceExcTax.Split('|');
                            //    if (variationValues.Count() != purchasePriceExcTax.Count())
                            //    {
                            //        errors.Add(new ClsError { Message = "Mismatch in no of PurchasePriceExcTax and VariationValues in row no " + count, Id = "" });
                            //        isError = true;
                            //    }
                            //}

                            if (item.ProfitMargin != "" && item.ProfitMargin != null)
                            {
                                var variationValues = item.VariationValues.Split('|');
                                var profitMargins = item.ProfitMargin.Split('|');
                                if (variationValues.Count() != profitMargins.Count())
                                {
                                    errors.Add(new ClsError { Message = "Mismatch in no of ProfitMargin and VariationValues in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                            if (item.SellingPrice != "" && item.SellingPrice != null)
                            {
                                var variationValues = item.VariationValues.Split('|');
                                var sellingPrice = item.SellingPrice.Split('|');
                                if (variationValues.Count() != sellingPrice.Count())
                                {
                                    errors.Add(new ClsError { Message = "Mismatch in no of SellingPrice and VariationValues in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                            if (item.MRP != "" && item.MRP != null)
                            {
                                var variationValues = item.VariationValues.Split('|');
                                var mrp = item.MRP.Split('|');
                                if (variationValues.Count() != mrp.Count())
                                {
                                    errors.Add(new ClsError { Message = "Mismatch in no of MRP and VariationValues in row no " + count, Id = "" });
                                    isError = true;
                                }
                            }

                            if (IsAccountsAddon == true)
                            {
                                if (item.InventoryAccount != "" && item.InventoryAccount != null)
                                {
                                    if (oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type.ToLower() == item.InventoryAccount.ToLower()).Count() == 0)
                                    {
                                        errors.Add(new ClsError { Message = "Invalid InventoryAccount in row no " + count, Id = "" });
                                        isError = true;
                                    }
                                }

                                if (item.PurchaseAccount != "" && item.PurchaseAccount != null)
                                {
                                    if (oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type.ToLower() == item.PurchaseAccount.ToLower()).Count() == 0)
                                    {
                                        errors.Add(new ClsError { Message = "PurchaseAccount is required in row no " + count, Id = "" });
                                        isError = true;
                                    }
                                }

                                if (item.SalesAccount != "" && item.SalesAccount != null)
                                {
                                    if (oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.Type.ToLower() == item.SalesAccount.ToLower()).Count() == 0)
                                    {
                                        errors.Add(new ClsError { Message = "SalesAccount is required in row no " + count, Id = "" });
                                        isError = true;
                                    }
                                }

                            }

                        }
                    }
                    count++;
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

            DateTime ExpiryDate = oConnectionContext.DbClsUser.Where(b => b.UserId == obj.CompanyId).Select(b => b.ExpiryDate).FirstOrDefault();

            long InventoryAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Type == "Inventory Asset").Select(a => a.AccountId).FirstOrDefault();

            long SalesAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Type == "Sales").Select(a => a.AccountId).FirstOrDefault();

            long PurchaseAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
            && a.Type == "Cost of Goods Sold").Select(a => a.AccountId).FirstOrDefault();

            decimal TaxPercent = 0;
            List<ClsItemImport> variationGroups = new List<ClsItemImport>();
            foreach (var item in obj.ItemImports)
            {
                using (TransactionScope dbContextTransaction = new TransactionScope())
                {
                    long ItemId = 0; string SingleSkuCode = "";
                    bool canVariationInsert = true;

                    if (item.ProductType.ToLower() == "variable")
                    {
                        if (variationGroups.Where(a => a.VariationGroupName.ToLower() == item.VariationGroupName.ToLower()).Count() == 0)
                        {
                            canVariationInsert = true;
                        }
                        else
                        {
                            ItemId = variationGroups.Where(a => a.VariationGroupName.ToLower() == item.VariationGroupName.ToLower()).Select(a => a.ItemId).FirstOrDefault();
                            canVariationInsert = false;
                        }
                    }

                    var BranchIds = "";
                    var branchNames = item.BranchNames.Split('|');

                    foreach (var branch in branchNames)
                    {
                        long BranchId = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true &&
                        a.IsDeleted == false && a.Branch.ToLower().Trim() == branch.ToLower().Trim()).Select(a => a.BranchId).FirstOrDefault();
                        BranchIds = BranchIds + '|' + BranchId;
                    }


                    if (canVariationInsert == true)
                    {
                        string SkuCode = "";
                        //if (item.SkuCode == "" || item.SkuCode == null)
                        //{
                        //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                        //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                        //                          join b in oConnectionContext.DbClsPrefixUserMap
                        //                           on a.PrefixMasterId equals b.PrefixMasterId
                        //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                        //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                        //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType.ToLower() == "item"
                        //                          && b.PrefixId == PrefixId
                        //                          select new
                        //                          {
                        //                              b.PrefixUserMapId,
                        //                              b.Prefix,
                        //                              b.NoOfDigits,
                        //                              b.Counter
                        //                          }).FirstOrDefault();

                        //    long PrefixUserMapId = prefixSettings.PrefixUserMapId;
                        //    SkuCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                        //    //increase counter
                        //    string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                        //    oConnectionContext.Database.ExecuteSqlCommand(q);
                        //    //increase counter
                        //}
                        //else
                        //{
                        SkuCode = item.SkuCode;
                        //}

                        long UnitId = 0;
                        if (item.UnitName != "" && item.UnitName != null)
                        {
                            UnitId = oConnectionContext.DbClsUnit.Where(a => a.UnitName.ToLower() == item.UnitName.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.UnitId).FirstOrDefault();

                            if (UnitId == 0)
                            {
                                ClsUnit oUnit = new ClsUnit()
                                {
                                    UnitName = item.UnitName,
                                    UnitShortName = item.UnitName,
                                    AllowDecimal = false,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                };
                                oConnectionContext.DbClsUnit.Add(oUnit);
                                oConnectionContext.SaveChanges();
                                UnitId = oUnit.UnitId;
                            }
                        }

                        long SecondaryUnitId = 0;
                        if (item.SecondaryUnitName != "" && item.SecondaryUnitName != null)
                        {
                            SecondaryUnitId = oConnectionContext.DbClsSecondaryUnit.Where(a => a.UnitId == UnitId && a.SecondaryUnitName.ToLower() == item.SecondaryUnitName.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.SecondaryUnitId).FirstOrDefault();

                            if (SecondaryUnitId == 0)
                            {
                                ClsSecondaryUnit oSecondaryUnit = new ClsSecondaryUnit()
                                {
                                    SecondaryUnitName = item.SecondaryUnitName,
                                    SecondaryUnitShortName = item.SecondaryUnitName,
                                    SecondaryUnitAllowDecimal = false,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    UnitId = UnitId
                                };
                                oConnectionContext.DbClsSecondaryUnit.Add(oSecondaryUnit);
                                oConnectionContext.SaveChanges();
                                SecondaryUnitId = oSecondaryUnit.SecondaryUnitId;
                            }
                        }

                        long TertiaryUnitId = 0;
                        if (item.TertiaryUnitName != "" && item.TertiaryUnitName != null)
                        {
                            TertiaryUnitId = oConnectionContext.DbClsTertiaryUnit.Where(a => a.UnitId == UnitId && a.SecondaryUnitId == SecondaryUnitId &&
                            a.TertiaryUnitName.ToLower() == item.TertiaryUnitName.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.TertiaryUnitId).FirstOrDefault();

                            if (TertiaryUnitId == 0)
                            {
                                ClsTertiaryUnit oTertiaryUnit = new ClsTertiaryUnit()
                                {
                                    TertiaryUnitName = item.TertiaryUnitName,
                                    TertiaryUnitShortName = item.TertiaryUnitName,
                                    TertiaryUnitAllowDecimal = false,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    UnitId = UnitId,
                                    SecondaryUnitId = SecondaryUnitId
                                };
                                oConnectionContext.DbClsTertiaryUnit.Add(oTertiaryUnit);
                                oConnectionContext.SaveChanges();
                                TertiaryUnitId = oTertiaryUnit.TertiaryUnitId;
                            }
                        }

                        long QuaternaryUnitId = 0;
                        if (item.QuaternaryUnitName != "" && item.QuaternaryUnitName != null)
                        {
                            QuaternaryUnitId = oConnectionContext.DbClsQuaternaryUnit.Where(a => a.UnitId == UnitId && a.SecondaryUnitId == SecondaryUnitId &&
                            a.TertiaryUnitId == TertiaryUnitId && a.QuaternaryUnitName.ToLower() == item.QuaternaryUnitName.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.QuaternaryUnitId).FirstOrDefault();

                            if (QuaternaryUnitId == 0)
                            {
                                ClsQuaternaryUnit oQuaternaryUnit = new ClsQuaternaryUnit()
                                {
                                    QuaternaryUnitName = item.QuaternaryUnitName,
                                    QuaternaryUnitShortName = item.QuaternaryUnitName,
                                    QuaternaryUnitAllowDecimal = false,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    UnitId = UnitId,
                                    SecondaryUnitId = SecondaryUnitId,
                                    TertiaryUnitId = TertiaryUnitId
                                };
                                oConnectionContext.DbClsQuaternaryUnit.Add(oQuaternaryUnit);
                                oConnectionContext.SaveChanges();
                                QuaternaryUnitId = oQuaternaryUnit.QuaternaryUnitId;
                            }
                        }

                        long BrandId = 0;
                        if (item.BrandName != "" && item.BrandName != null)
                        {
                            BrandId = oConnectionContext.DbClsBrand.Where(a => a.Brand.ToLower() == item.BrandName.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.BrandId).FirstOrDefault();

                            if (BrandId == 0)
                            {
                                //var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                                //                      join b in oConnectionContext.DbClsPrefixUserMap
                                //                       on a.PrefixId equals b.PrefixId
                                //                      where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                                //                      b.CompanyId == obj.CompanyId && b.IsActive == true
                                //                      && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Brand"
                                //                      select new
                                //                      {
                                //                          b.PrefixUserMapId,
                                //                          b.Prefix,
                                //                          b.NoOfDigits,
                                //                          b.Counter
                                //                      }).FirstOrDefault();
                                //long PrefixUserMapId = prefixSettings.PrefixUserMapId;
                                //string BrandCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                                ClsBrand oBrand = new ClsBrand()
                                {
                                    Brand = item.BrandName,
                                    //BrandCode = BrandCode,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                };
                                oConnectionContext.DbClsBrand.Add(oBrand);
                                oConnectionContext.SaveChanges();
                                BrandId = oBrand.BrandId;

                                ////increase counter
                                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                                //oConnectionContext.Database.ExecuteSqlCommand(q);
                                ////increase counter
                            }
                        }

                        long CategoryId = 0;
                        if (item.CategoryName != "" && item.CategoryName != null)
                        {
                            CategoryId = oConnectionContext.DbClsCategory.Where(a => a.Category.ToLower() == item.CategoryName.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.CategoryId).FirstOrDefault();

                            if (CategoryId == 0)
                            {
                                //var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                                //                      join b in oConnectionContext.DbClsPrefixUserMap
                                //                       on a.PrefixId equals b.PrefixId
                                //                      where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                                //                      b.CompanyId == obj.CompanyId && b.IsActive == true
                                //                      && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Category"
                                //                      select new
                                //                      {
                                //                          b.PrefixUserMapId,
                                //                          b.Prefix,
                                //                          b.NoOfDigits,
                                //                          b.Counter
                                //                      }).FirstOrDefault();
                                //long PrefixUserMapId = prefixSettings.PrefixUserMapId;
                                //string CategoryCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                                ClsCategory oCategory = new ClsCategory()
                                {
                                    Category = item.CategoryName,
                                    //CategoryCode = CategoryCode,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                };
                                oConnectionContext.DbClsCategory.Add(oCategory);
                                oConnectionContext.SaveChanges();
                                CategoryId = oCategory.CategoryId;

                                ////increase counter
                                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                                //oConnectionContext.Database.ExecuteSqlCommand(q);
                                ////increase counter
                            }
                        }

                        long SubCategoryId = 0;
                        if (item.SubCategoryName != "" && item.SubCategoryName != null)
                        {
                            SubCategoryId = oConnectionContext.DbClsSubCategory.Where(a => a.CategoryId == CategoryId && a.SubCategory.ToLower() == item.SubCategoryName.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.SubCategoryId).FirstOrDefault();

                            if (SubCategoryId == 0)
                            {
                                //var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                                //                      join b in oConnectionContext.DbClsPrefixUserMap
                                //                       on a.PrefixId equals b.PrefixId
                                //                      where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                                //                      b.CompanyId == obj.CompanyId && b.IsActive == true
                                //                      && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Sub Category"
                                //                      select new
                                //                      {
                                //                          b.PrefixUserMapId,
                                //                          b.Prefix,
                                //                          b.NoOfDigits,
                                //                          b.Counter
                                //                      }).FirstOrDefault();
                                //long PrefixUserMapId = prefixSettings.PrefixUserMapId;
                                //string SubCategoryCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                                ClsSubCategory oSubCategory = new ClsSubCategory()
                                {
                                    SubCategory = item.SubCategoryName,
                                    //SubCategoryCode = SubCategoryCode,
                                    CategoryId = CategoryId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                };
                                oConnectionContext.DbClsSubCategory.Add(oSubCategory);
                                oConnectionContext.SaveChanges();
                                SubCategoryId = oSubCategory.SubCategoryId;

                                ////increase counter
                                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                                //oConnectionContext.Database.ExecuteSqlCommand(q);
                                ////increase counter
                            }
                        }

                        long SubSubCategoryId = 0;
                        if (item.SubSubCategoryName != "" && item.SubSubCategoryName != null)
                        {
                            SubSubCategoryId = oConnectionContext.DbClsSubSubCategory.Where(a => a.CategoryId == CategoryId && a.SubCategoryId == SubCategoryId &&
                            a.SubSubCategory.ToLower() == item.SubSubCategoryName.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.SubCategoryId).FirstOrDefault();

                            if (SubSubCategoryId == 0)
                            {
                                //var prefixSettings = (from a in oConnectionContext.DbClsPrefix
                                //                      join b in oConnectionContext.DbClsPrefixUserMap
                                //                       on a.PrefixId equals b.PrefixId
                                //                      where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                                //                      b.CompanyId == obj.CompanyId && b.IsActive == true
                                //                      && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Sub Sub Category"
                                //                      select new
                                //                      {
                                //                          b.PrefixUserMapId,
                                //                          b.Prefix,
                                //                          b.NoOfDigits,
                                //                          b.Counter
                                //                      }).FirstOrDefault();
                                //long PrefixUserMapId = prefixSettings.PrefixUserMapId;
                                //string SubSubCategoryCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                                ClsSubSubCategory oSubSubCategory = new ClsSubSubCategory()
                                {
                                    SubSubCategory = item.SubSubCategoryName,
                                    //SubSubCategoryCode = SubSubCategoryCode,
                                    CategoryId = CategoryId,
                                    SubCategoryId = SubCategoryId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                };
                                oConnectionContext.DbClsSubSubCategory.Add(oSubSubCategory);
                                oConnectionContext.SaveChanges();
                                SubSubCategoryId = oSubSubCategory.SubSubCategoryId;

                                ////increase counter
                                //string q = "update tblPrefixUserMap set Counter = Counter,0)+1 where PrefixUserMapId=" + PrefixUserMapId;
                                //oConnectionContext.Database.ExecuteSqlCommand(q);
                                ////increase counter
                            }
                        }

                        long WarrantyId = 0;
                        if (item.WarrantyName != "" && item.WarrantyName != null)
                        {
                            WarrantyId = oConnectionContext.DbClsWarranty.Where(a => a.Warranty.ToLower() == item.WarrantyName.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.WarrantyId).FirstOrDefault();

                            if (WarrantyId == 0)
                            {
                                ClsWarranty oWarranty = new ClsWarranty()
                                {
                                    Warranty = item.WarrantyName,
                                    Description = obj.Description,
                                    Duration = item.WarrantyDurationType,
                                    DurationNo = item.WarrantyDuration,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                };
                                oConnectionContext.DbClsWarranty.Add(oWarranty);
                                oConnectionContext.SaveChanges();
                                WarrantyId = oWarranty.WarrantyId;
                            }
                        }

                        long SaltId = 0;
                        if (item.SaltName != "" && item.SaltName != null)
                        {
                            SaltId = oConnectionContext.DbClsSalt.Where(a => a.SaltName.ToLower() == item.SaltName.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.SaltId).FirstOrDefault();

                            if (SaltId == 0)
                            {
                                ClsSalt oSalt = new ClsSalt()
                                {
                                    SaltName = item.SaltName,
                                    //Indication = obj.Indication,
                                    //Dosage = obj.Dosage,
                                    //SideEffects = obj.SideEffects,
                                    //SpecialPrecautions = obj.SpecialPrecautions,
                                    //DrugInteractions = obj.DrugInteractions,
                                    //Notes = obj.Notes,
                                    //TBItem = obj.TBItem,
                                    //IsNarcotic = obj.IsNarcotic,
                                    //IsScheduleH = obj.IsScheduleH,
                                    //IsScheduleH1 = obj.IsScheduleH1,
                                    //IsDiscontinued = obj.IsDiscontinued,
                                    //IsProhibited = obj.IsProhibited,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                };
                                oConnectionContext.DbClsSalt.Add(oSalt);
                                oConnectionContext.SaveChanges();
                                SaltId = oSalt.SaltId;
                            }
                        }

                        long TaxExemptionId = 0;
                        if (item.TaxExemptionReason != "" && item.TaxExemptionReason != null)
                        {
                            TaxExemptionId = oConnectionContext.DbClsTaxExemption.Where(a => a.Reason.ToLower() == item.TaxExemptionReason.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.TaxExemptionId).FirstOrDefault();

                            if (TaxExemptionId == 0)
                            {
                                ClsTaxExemption oClsTaxExemption = new ClsTaxExemption()
                                {
                                    Reason = item.TaxExemptionReason,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                };
                                oConnectionContext.DbClsTaxExemption.Add(oClsTaxExemption);
                                oConnectionContext.SaveChanges();
                                TaxExemptionId = oClsTaxExemption.TaxExemptionId;
                            }
                        }

                        long IntraStateTaxId = 0, InterStateTaxId = 0;

                        if (item.TaxName != "" && item.TaxName != null)
                        {
                            IntraStateTaxId = oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == item.TaxName.Trim().ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.TaxId).FirstOrDefault();

                            TaxPercent = oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == item.TaxName.Trim().ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.TaxPercent).FirstOrDefault();
                        }

                        if (item.IntraStateTaxName != "" && item.IntraStateTaxName != null)
                        {
                            IntraStateTaxId = oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == item.IntraStateTaxName.Trim().ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.TaxId).FirstOrDefault();

                            TaxPercent = oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == item.IntraStateTaxName.Trim().ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.TaxPercent).FirstOrDefault();
                        }

                        if (item.InterStateTaxName != "" && item.InterStateTaxName != null)
                        {
                            InterStateTaxId = oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == item.InterStateTaxName.Trim().ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.TaxId).FirstOrDefault();

                            TaxPercent = oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == item.InterStateTaxName.Trim().ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.TaxPercent).FirstOrDefault();
                        }

                        long TaxPreferenceId = oConnectionContext.DbClsTax.Where(a => a.Tax == item.TaxPreference.Trim() && a.CompanyId == obj.CompanyId
                        && a.IsDeleted == false).Select(a => a.TaxId).FirstOrDefault();

                        long ItemCodeId = 0;
                        if (item.HsnSacCode != "" && item.HsnSacCode != null)
                        {
                            ItemCodeId = oConnectionContext.DbClsItemCode.Where(a => a.Code.ToLower() == item.HsnSacCode.ToLower() &&
                                               a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.ItemCodeId).FirstOrDefault();

                            if (ItemCodeId == 0)
                            {
                                ClsItemCode oItemCode = new ClsItemCode()
                                {
                                    Code = item.HsnSacCode,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                    IntraStateTaxId = IntraStateTaxId,
                                    InterStateTaxId = InterStateTaxId,
                                    ItemCodeType = item.ItemType.ToLower() == "product" ? "HSN" : "SAC",
                                    TaxPreferenceId = TaxPreferenceId,
                                    TaxExemptionId = TaxExemptionId,
                                };
                                oConnectionContext.DbClsItemCode.Add(oItemCode);
                                oConnectionContext.SaveChanges();
                                ItemCodeId = oItemCode.ItemCodeId;
                            }
                        }

                        int PriceAddedFor = 0;
                        if (QuaternaryUnitId != 0)
                        {
                            PriceAddedFor = 1;
                        }
                        else if (TertiaryUnitId != 0)
                        {
                            PriceAddedFor = 2;
                        }
                        else if (SecondaryUnitId != 0)
                        {
                            PriceAddedFor = 3;
                        }
                        else if (UnitId != 0)
                        {
                            PriceAddedFor = 4;
                        }

                        ClsItem oClsItem = new ClsItem()
                        {
                            ItemType = item.ItemType.Trim(),
                            ExpiryPeriod = item.ExpiryPeriod,
                            ExpiryPeriodType = item.ExpiryPeriodType,
                            ItemName = item.ItemName,
                            Description = item.Description,
                            SkuCode = SkuCode,
                            ItemCodeId = ItemCodeId,
                            BarcodeType = item.BarcodeType,
                            UnitId = UnitId,
                            SecondaryUnitId = SecondaryUnitId,
                            TertiaryUnitId = TertiaryUnitId,
                            QuaternaryUnitId = QuaternaryUnitId,
                            UToSValue = item.UToSValue,
                            SToTValue = item.SToTValue,
                            TToQValue = item.TToQValue,
                            BrandId = BrandId,
                            CategoryId = CategoryId,
                            SubCategoryId = SubCategoryId,
                            SubSubCategoryId = SubSubCategoryId,
                            IsManageStock = item.IsManageStock,
                            AlertQuantity = item.AlertQuantity,
                            TaxId = IntraStateTaxId,
                            InterStateTaxId = InterStateTaxId,
                            TaxType = "Inclusive",//item.TaxType,
                            ProductType = item.ProductType,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            //BranchIds = obj.BranchIds.TrimStart(','),
                            WarrantyId = WarrantyId,
                            PriceAddedFor = PriceAddedFor,
                            EnableImei = item.EnableImei,
                            BatchNo = obj.BatchNo,
                            TaxPreferenceId = TaxPreferenceId,
                            TaxExemptionId = TaxExemptionId,
                            SaltId = SaltId
                        };
                        oConnectionContext.DbClsItem.Add(oClsItem);
                        oConnectionContext.SaveChanges();

                        ItemId = oClsItem.ItemId;
                        SingleSkuCode = oClsItem.SkuCode;

                        if (item.VariationGroupName != "" && item.VariationGroupName != null)
                        {
                            variationGroups.Add(new ClsItemImport { VariationGroupName = item.VariationGroupName, ItemId = ItemId });
                        }
                    }

                    int counter = 10; int c = 0;

                    List<ClsItemDetails> ItemDetails = new List<ClsItemDetails>();

                    if (item.ProductType.ToLower() == "single")
                    {
                        decimal _purchaseExcTax = 0, _defaultProfitMargin = 0, _salesExcTax = 0, _mrp = 0;
                        //if (item.ProfitMargin == "" || item.ProfitMargin == null)
                        //{
                        //    _defaultProfitMargin = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DefaultProfitPercent).FirstOrDefault();
                        //}
                        //else
                        //{
                        //    _defaultProfitMargin = Convert.ToDecimal(item.ProfitMargin);
                        //}

                        //if (item.PurchasePriceExcTax != "" && item.PurchasePriceExcTax != null)
                        //{
                        //    _purchaseExcTax = Convert.ToDecimal(item.PurchasePriceExcTax);
                        //    if (TaxPercent == 0)
                        //    {
                        //        _purchaseIncTax = Convert.ToDecimal(item.PurchasePriceExcTax);
                        //    }
                        //    else
                        //    {
                        //        _purchaseIncTax = ((TaxPercent / 100) * Convert.ToDecimal(item.PurchasePriceExcTax)) + Convert.ToDecimal(item.PurchasePriceExcTax);
                        //    }
                        //}
                        //else
                        //{
                        //    _purchaseIncTax = Convert.ToDecimal(item.PurchasePriceIncTax);
                        //    _purchaseExcTax = (Convert.ToDecimal(item.PurchasePriceIncTax) * 100) / (100 + Convert.ToDecimal(TaxPercent));
                        //}

                        //_salesIncTax = (((_defaultProfitMargin / 100) * _purchaseIncTax) + _purchaseIncTax);
                        //_salesExcTax = (((_defaultProfitMargin / 100) * _purchaseExcTax) + _purchaseExcTax);

                        if (item.PurchasePrice != "" && item.PurchasePrice != null)
                        {
                            _purchaseExcTax = Convert.ToDecimal(item.PurchasePrice);
                        }

                        //if (item.ProfitMargin != "" && item.ProfitMargin != null)
                        //{
                        //    _defaultProfitMargin = Convert.ToDecimal(item.ProfitMargin);
                        //}

                        if (item.SellingPrice != "" && item.SellingPrice != null)
                        {
                            _salesExcTax = Convert.ToDecimal(item.SellingPrice);
                            _defaultProfitMargin = ((_salesExcTax - _purchaseExcTax) / _purchaseExcTax) * 100;
                        }

                        if (item.MRP != "" && item.MRP != null)
                        {
                            _mrp = Convert.ToDecimal(item.MRP);
                        }

                        long _InventoryAccountId = InventoryAccountId;
                        long _PurchaseAccountId = PurchaseAccountId;
                        long _SalesAccountId = SalesAccountId;

                        if (IsAccountsAddon == true)
                        {
                            if (item.InventoryAccount != "" && item.InventoryAccount != null)
                            {
                                _InventoryAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.DisplayAs.ToLower() == item.InventoryAccount.ToLower()).Select(a => a.AccountId).FirstOrDefault();
                            }

                            if (item.PurchaseAccount != "" && item.PurchaseAccount != null)
                            {
                                _PurchaseAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.DisplayAs.ToLower() == item.PurchaseAccount.ToLower()).Select(a => a.AccountId).FirstOrDefault();
                            }

                            if (item.SalesAccount != "" && item.SalesAccount != null)
                            {
                                _SalesAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
               && a.DisplayAs.ToLower() == item.SalesAccount.ToLower()).Select(a => a.AccountId).FirstOrDefault();
                            }
                        }

                        ItemDetails.Add(new ClsItemDetails
                        {
                            SKU = SingleSkuCode,
                            PurchaseExcTax = _purchaseExcTax,
                            //PurchaseIncTax = _purchaseIncTax,
                            DefaultProfitMargin = _defaultProfitMargin,
                            SalesExcTax = _salesExcTax,
                            DefaultMrp = _mrp,
                            InventoryAccountId = _InventoryAccountId,
                            PurchaseAccountId = _PurchaseAccountId,
                            SalesAccountId = _SalesAccountId
                        });
                    }
                    else
                    {
                        //decimal profitMargin = 0;
                        string[] _array = new string[item.VariationValues.Count()];

                        var variationValues = (item.VariationValues == "" || item.VariationValues == null) ? _array : item.VariationValues.Split('|');
                        var variationSKUs = (item.VariationSKUs == "" || item.VariationSKUs == null) ? _array : item.VariationSKUs.Split('|');
                        //var purchasePriceIncTax = (item.PurchasePriceIncTax == "" || item.PurchasePriceIncTax == null) ? _array : item.PurchasePriceIncTax.Split('|');
                        var purchasePrice = (item.PurchasePrice == "" || item.PurchasePrice == null) ? _array : item.PurchasePrice.Split('|');
                        var profitMargins = (item.ProfitMargin == "" || item.ProfitMargin == null) ? _array : item.ProfitMargin.Split('|');
                        var sellingPrice = (item.SellingPrice == "" || item.SellingPrice == null) ? _array : item.SellingPrice.Split('|');
                        var mrp = (item.MRP == "" || item.MRP == null) ? _array : item.MRP.Split('|');

                        for (int i = 0; i < variationValues.Count(); i++)
                        {
                            long PrefixUserMapId = 0, VariationId = 0, VariationDetailsId = 0;
                            VariationId = oConnectionContext.DbClsVariation.Where(a => a.Variation.ToLower() == item.VariationName.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.VariationId).FirstOrDefault();

                            if (VariationId == 0)
                            {
                                ClsVariation oClsVariation = new ClsVariation()
                                {
                                    //VariationCode = VariationCode,
                                    Variation = item.VariationName,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    CompanyId = obj.CompanyId,
                                };
                                oConnectionContext.DbClsVariation.Add(oClsVariation);
                                oConnectionContext.SaveChanges();

                                //increase counter
                                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                                oConnectionContext.Database.ExecuteSqlCommand(q);
                                //increase counter

                                VariationId = oClsVariation.VariationId;
                            }

                            string VariationDetails = variationValues[i];
                            VariationDetailsId = oConnectionContext.DbClsVariationDetails.Where(a => a.VariationId == VariationId && a.VariationDetails.ToLower() == VariationDetails.ToLower()
                            && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.VariationDetailsId).FirstOrDefault();

                            if (VariationDetailsId == 0)
                            {
                                ClsVariationDetails oClsVariationDetails = new ClsVariationDetails()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    VariationDetails = variationValues[i],
                                    CompanyId = obj.CompanyId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    VariationId = VariationId
                                };
                                ConnectionContext ocon1 = new ConnectionContext();
                                ocon1.DbClsVariationDetails.Add(oClsVariationDetails);
                                ocon1.SaveChanges();
                                VariationDetailsId = oClsVariationDetails.VariationDetailsId;
                            }

                            decimal _purchaseExcTax = 0, _defaultProfitMargin = 0, _salesExcTax = 0, _mrp = 0;

                            //if (profitMargins[i] == "" || profitMargins[i] == null)
                            //{
                            //    _defaultProfitMargin = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DefaultProfitPercent).FirstOrDefault();
                            //}
                            //else
                            //{
                            //    _defaultProfitMargin = Convert.ToDecimal(profitMargins[i]);
                            //}

                            //if (purchasePriceExcTax[i] != "" && purchasePriceExcTax[i] != null)
                            //{
                            //    _purchaseExcTax = Convert.ToDecimal(purchasePriceExcTax[i]);
                            //    if (TaxPercent == 0)
                            //    {
                            //        _purchaseIncTax = Convert.ToDecimal(purchasePriceExcTax[i]);
                            //    }
                            //    else
                            //    {
                            //        _purchaseIncTax = ((TaxPercent / 100) * Convert.ToDecimal(purchasePriceExcTax[i])) + Convert.ToDecimal(purchasePriceExcTax[i]);
                            //    }
                            //}
                            //else
                            //{
                            //    _purchaseIncTax = Convert.ToDecimal(purchasePriceIncTax[i]);
                            //    _purchaseExcTax = (Convert.ToDecimal(purchasePriceIncTax[i]) * 100) / (100 + Convert.ToDecimal(TaxPercent));
                            //}

                            //_salesIncTax = (((_defaultProfitMargin / 100) * _purchaseIncTax) + _purchaseIncTax);
                            //_salesExcTax = (((_defaultProfitMargin / 100) * _purchaseExcTax) + _purchaseExcTax);

                            if (purchasePrice[i] == "" || purchasePrice[i] == null)
                            {
                                _purchaseExcTax = Convert.ToDecimal(purchasePrice[i]);
                            }

                            if (profitMargins[i] == "" || profitMargins[i] == null)
                            {
                                _defaultProfitMargin = Convert.ToDecimal(profitMargins[i]);
                            }

                            if (sellingPrice[i] == "" || sellingPrice[i] == null)
                            {
                                _salesExcTax = Convert.ToDecimal(sellingPrice[i]);
                            }

                            if (mrp[i] == "" || mrp[i] == null)
                            {
                                _mrp = Convert.ToDecimal(mrp[i]);
                            }

                            long _InventoryAccountId = InventoryAccountId;
                            long _PurchaseAccountId = PurchaseAccountId;
                            long _SalesAccountId = SalesAccountId;

                            if (IsAccountsAddon == true)
                            {
                                if (item.InventoryAccount != "" && item.InventoryAccount != null)
                                {
                                    _InventoryAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                   && a.DisplayAs.ToLower() == item.InventoryAccount.ToLower()).Select(a => a.AccountId).FirstOrDefault();
                                }

                                if (item.PurchaseAccount != "" && item.PurchaseAccount != null)
                                {
                                    _PurchaseAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                   && a.DisplayAs.ToLower() == item.PurchaseAccount.ToLower()).Select(a => a.AccountId).FirstOrDefault();
                                }

                                if (item.SalesAccount != "" && item.SalesAccount != null)
                                {
                                    _SalesAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                   && a.DisplayAs.ToLower() == item.SalesAccount.ToLower()).Select(a => a.AccountId).FirstOrDefault();
                                }
                            }

                            ItemDetails.Add(new ClsItemDetails
                            {
                                VariationId = VariationId,
                                VariationDetailsId = VariationDetailsId,
                                SKU = variationSKUs[i],
                                PurchaseExcTax = _purchaseExcTax,
                                DefaultProfitMargin = _defaultProfitMargin,
                                SalesExcTax = _salesExcTax,
                                DefaultMrp = Convert.ToDecimal(mrp[i]),
                                InventoryAccountId = _InventoryAccountId,
                                PurchaseAccountId = _PurchaseAccountId,
                                SalesAccountId = _SalesAccountId
                            });
                        }
                    }

                    foreach (var itemDetail in ItemDetails)
                    {
                        long PrefixUserMapId = 0;
                        if (itemDetail.SKU == "" || itemDetail.SKU == null)
                        {
                            long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                            var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                                  join b in oConnectionContext.DbClsPrefixUserMap
                                                   on a.PrefixMasterId equals b.PrefixMasterId
                                                  where a.IsActive == true && a.IsDeleted == false &&
                                                  b.CompanyId == obj.CompanyId && b.IsActive == true
                                                  && b.IsDeleted == false && a.PrefixType.ToLower() == "item"
                                                  && b.PrefixId == PrefixId
                                                  select new
                                                  {
                                                      b.PrefixUserMapId,
                                                      b.Prefix,
                                                      b.NoOfDigits,
                                                      b.Counter
                                                  }).FirstOrDefault();
                            PrefixUserMapId = prefixSettings.PrefixUserMapId;
                            itemDetail.SKU = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');

                            //increase counter
                            string r = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                            oConnectionContext.Database.ExecuteSqlCommand(r);
                            //increase counter
                        }

                        ClsItemDetails oClsItemDetails = new ClsItemDetails()
                        {
                            ItemDetailsId = itemDetail.ItemDetailsId,
                            SKU = itemDetail.SKU,
                            PurchaseExcTax = itemDetail.PurchaseExcTax,
                            PurchaseIncTax = itemDetail.PurchaseIncTax,
                            DefaultProfitMargin = itemDetail.DefaultProfitMargin,
                            SalesExcTax = itemDetail.SalesExcTax,
                            SalesIncTax = itemDetail.SalesIncTax,
                            ItemId = ItemId,
                            Quantity = itemDetail.Quantity,
                            TotalCost = itemDetail.TotalCost,
                            VariationId = itemDetail.VariationId,
                            VariationDetailsId = itemDetail.VariationDetailsId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            ComboItemDetailsId = itemDetail.ComboItemDetailsId,
                            DefaultMrp = itemDetail.DefaultMrp,
                            InventoryAccountId = itemDetail.InventoryAccountId,
                            SalesAccountId = itemDetail.SalesAccountId,
                            PurchaseAccountId = itemDetail.PurchaseAccountId,
                        };

                        oConnectionContext.DbClsItemDetails.Add(oClsItemDetails);
                        oConnectionContext.SaveChanges();

                        counter++;

                        if (c == 0)
                        {
                            string[] _array = new string[BranchIds.Count()];
                            var Branchs = BranchIds.TrimStart('|').TrimEnd('|').Split('|');

                            var Rack = (item.Rack == "" || item.Rack == null) ? _array : item.Rack.Split('|');
                            var Row = (item.Row == "" || item.Row == null) ? _array : item.Row.Split('|');
                            var Position = (item.Position == "" || item.Position == null) ? _array : item.Position.Split('|');

                            if (Branchs != null)
                            {
                                for (int i = 0; i < Branchs.Count(); i++)
                                {
                                    long BranchId = Convert.ToInt64(Branchs[i]);
                                    ClsItemBranchMap oClsItemBranchMap = new ClsItemBranchMap()
                                    {
                                        AddedBy = obj.AddedBy,
                                        AddedOn = CurrentDate,
                                        BranchId = BranchId,
                                        CompanyId = obj.CompanyId,
                                        IsActive = true,
                                        IsDeleted = false,
                                        ItemDetailsId = oClsItemDetails.ItemDetailsId,
                                        ItemId = ItemId,
                                        Quantity = 0,
                                        Rack = Rack[i],
                                        Row = Row[i],
                                        Position = Position[i]
                                    };
                                    oConnectionContext.DbClsItemBranchMap.Add(oClsItemBranchMap);
                                    oConnectionContext.SaveChanges();
                                }
                                if (obj.ProductType == "Combo")
                                {
                                    c = c + 1;
                                }
                            }
                        }
                    }
                    dbContextTransaction.Complete();
                }
            }

            ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
            {
                AddedBy = obj.AddedBy,
                Browser = obj.Browser,
                Category = "Items",
                CompanyId = obj.CompanyId,
                Description = "Items imported",
                Id = 0,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = "Insert"
            };
            oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

            data = new
            {
                Status = 1,
                Message = "Items imported successfully",
                Data = new
                {
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SearchLot(ClsAvailableLots obj)
        {
            decimal customerGroupDiscountPercentage = 0;
            if (obj.CustomerId != 0)
            {
                long UserGroupId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.UserGroupId).DefaultIfEmpty().FirstOrDefault();

                if (UserGroupId != 0)
                {
                    var UserGroup = oConnectionContext.DbClsUserGroup.Where
                    (a => a.UserGroupId == UserGroupId && a.IsActive == true && a.IsDeleted == false).Select(a => new
                    {
                        a.CalculationPercentage,
                        a.PriceCalculationType,
                        a.SellingPriceGroupId
                    }).FirstOrDefault();

                    if (UserGroup.PriceCalculationType == 1)
                    {
                        customerGroupDiscountPercentage = (UserGroup.CalculationPercentage / 100);
                    }
                    else
                    {
                        obj.SellingPriceGroupId = UserGroup.SellingPriceGroupId;
                    }
                }
            }

            ClsItemDetailsVm ItemDetail = new ClsItemDetailsVm();
            if (obj.Type.ToLower() == "purchase")
            {
                ItemDetail = oConnectionContext.DbClsPurchaseDetails.Where(a => a.PurchaseDetailsId == obj.Id).Select(a => new ClsItemDetailsVm
                {
                    PurchaseExcTax = a.PurchaseExcTax,
                    PriceAddedFor = a.PriceAddedFor,
                    //UnitCost = a.UnitCost,
                    SalesExcTax = a.SalesExcTax + (customerGroupDiscountPercentage * a.SalesExcTax),
                    SalesIncTax = a.SalesIncTax,
                    Quantity = a.QuantityRemaining,
                    ItemId = a.ItemId,
                    ItemDetailsId = a.ItemDetailsId
                }).FirstOrDefault();
            }
            else if (obj.Type.ToLower() == "openingstock")
            {
                ItemDetail = oConnectionContext.DbClsOpeningStock.Where(a => a.OpeningStockId == obj.Id).Select(a => new ClsItemDetailsVm
                {
                    PurchaseExcTax = a.UnitCost,
                    PriceAddedFor = a.PriceAddedFor,
                    //UnitCost = a.UnitCost,
                    SalesExcTax = a.SalesExcTax + (customerGroupDiscountPercentage * a.SalesExcTax),
                    SalesIncTax = a.SalesIncTax,
                    Quantity = a.QuantityRemaining,
                    ItemId = a.ItemId,
                    ItemDetailsId = a.ItemDetailsId
                }).FirstOrDefault();
            }
            if (obj.Type.ToLower() == "stocktransfer")
            {
                ItemDetail = oConnectionContext.DbClsStockTransferDetails.Where(a => a.StockTransferDetailsId == obj.Id).Select(a => new ClsItemDetailsVm
                {
                    PurchaseExcTax = a.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == a.LotIdForLotNoChecking).Select(f => f.PurchaseExcTax).FirstOrDefault()
                                    : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == a.LotIdForLotNoChecking).Select(f => f.UnitCost).FirstOrDefault(),
                    PriceAddedFor = a.PriceAddedFor,
                    //UnitCost = a.UnitCost,
                    SalesExcTax = a.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == a.LotIdForLotNoChecking).Select(f => f.SalesExcTax + (customerGroupDiscountPercentage * f.SalesExcTax)).FirstOrDefault()
                                    : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == a.LotIdForLotNoChecking).Select(f => f.SalesExcTax + (customerGroupDiscountPercentage * f.SalesExcTax)).FirstOrDefault(),
                    SalesIncTax = a.LotTypeForLotNoChecking == "purchase" ? oConnectionContext.DbClsPurchaseDetails.Where(f => f.PurchaseDetailsId == a.LotIdForLotNoChecking).Select(f => f.SalesIncTax).FirstOrDefault()
                                    : oConnectionContext.DbClsOpeningStock.Where(f => f.OpeningStockId == a.LotIdForLotNoChecking).Select(f => f.SalesIncTax).FirstOrDefault(),
                    Quantity = a.QuantityRemaining,
                    ItemId = a.ItemId,
                    ItemDetailsId = a.ItemDetailsId
                }).FirstOrDefault();
            }
            //else if (obj.Type.ToLower() == "stockadjustment")
            //{
            //    ItemDetail = oConnectionContext.DbClsStockAdjustmentDetails.Where(a => a.StockAdjustmentDetailsId == obj.Id).Select(a => new ClsItemDetailsVm
            //    {
            //        PriceAddedFor = a.PriceAddedFor,
            //        //UnitCost = a.UnitCost,
            //        //SalesIncTax = a.SalesIncTax,
            //        Quantity = a.QuantityRemaining
            //    }).FirstOrDefault();
            //}
            else if (obj.Type.ToLower() == "all")
            {
                ItemDetail = (from c in oConnectionContext.DbClsItemBranchMap
                              join a in oConnectionContext.DbClsItemDetails
                               on c.ItemDetailsId equals a.ItemDetailsId
                              join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                              where c.ItemDetailsId == obj.ItemDetailsId && a.ItemDetailsId == obj.ItemDetailsId
                              && c.BranchId == obj.BranchId
                              && c.IsActive == true && b.IsActive == true && b.IsDeleted == false
                              select new ClsItemDetailsVm
                              {
                                  PurchaseExcTax = a.PurchaseExcTax,
                                  PriceAddedFor = b.PriceAddedFor,
                                  SalesExcTax = a.SalesExcTax + (customerGroupDiscountPercentage * a.SalesExcTax),
                                  SalesIncTax = a.SalesIncTax,
                                  Quantity = c.Quantity,
                                  ItemId = c.ItemId,
                                  ItemDetailsId = c.ItemDetailsId
                              }).FirstOrDefault();
            }

            ItemDetail.SalesIncTax = ItemDetail.SalesIncTax + (customerGroupDiscountPercentage * ItemDetail.SalesIncTax);

            if (obj.SellingPriceGroupId != 0)
            {
                ItemDetail.SalesExcTax = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == ItemDetail.ItemId &&
                    b.ItemDetailsId == ItemDetail.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId && b.SellingPrice > 0).Count() == 0 ? ItemDetail.SalesExcTax :
                    (oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == ItemDetail.ItemId && b.ItemDetailsId == ItemDetail.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.DiscountType).FirstOrDefault() == "Fixed" ?
                    oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == ItemDetail.ItemId && b.ItemDetailsId == ItemDetail.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.SellingPrice).FirstOrDefault()
                    : ((oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == ItemDetail.ItemId && b.ItemDetailsId == ItemDetail.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.SellingPrice).FirstOrDefault() / 100) * ItemDetail.SalesExcTax));

                ItemDetail.SalesIncTax = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == ItemDetail.ItemId &&
                    b.ItemDetailsId == ItemDetail.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId && b.SellingPrice > 0).Count() == 0 ? ItemDetail.SalesIncTax :
                    (oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == ItemDetail.ItemId && b.ItemDetailsId == ItemDetail.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.DiscountType).FirstOrDefault() == "Fixed" ?
                    oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == ItemDetail.ItemId && b.ItemDetailsId == ItemDetail.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.SellingPrice).FirstOrDefault()
                    : ((oConnectionContext.DbClsItemSellingPriceGroupMap.Where(b => b.ItemId == ItemDetail.ItemId && b.ItemDetailsId == ItemDetail.ItemDetailsId
                    && b.SellingPriceGroupId == obj.SellingPriceGroupId).Select(b => b.SellingPrice).FirstOrDefault() / 100) * ItemDetail.SalesIncTax));
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ItemDetail = ItemDetail,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CalculateExpiryDate(ClsOpeningStock obj)
        {
            var Item = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => new
            { a.ExpiryPeriod, a.ExpiryPeriodType }).FirstOrDefault();

            DateTime ExpiryDate = DateTime.Now;

            if (Item.ExpiryPeriodType.ToLower() == "year")
            {
                if (obj.ManufacturingDate != null)
                {
                    ExpiryDate = obj.ManufacturingDate.Value.AddYears(Item.ExpiryPeriod);
                }
            }
            else if (Item.ExpiryPeriodType.ToLower() == "month")
            {
                if (obj.ManufacturingDate != null)
                {
                    ExpiryDate = obj.ManufacturingDate.Value.AddMonths(Item.ExpiryPeriod);
                }
            }
            else if (Item.ExpiryPeriodType.ToLower() == "day")
            {
                if (obj.ManufacturingDate != null)
                {
                    ExpiryDate = obj.ManufacturingDate.Value.AddDays(Item.ExpiryPeriod);
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    OpeningStock = new ClsOpeningStock
                    {
                        ExpiryDate = ExpiryDate
                    }
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemSalesReport(ClsSalesDetailsVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //   oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //     && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
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

            List<ClsSalesDetailsVm> det;
            if (obj.BranchId == 0)
            {
                det = (from b in oConnectionContext.DbClsSalesDetails
                       join a in oConnectionContext.DbClsSales
                       on b.SalesId equals a.SalesId
                       join c in oConnectionContext.DbClsItemDetails
                       on b.ItemDetailsId equals c.ItemDetailsId
                       join d in oConnectionContext.DbClsItem
                       on c.ItemId equals d.ItemId
                       where b.IsDeleted == false && a.IsDeleted == false && a.IsCancelled == false && a.IsCancelled == false && a.Status != "Draft" &&
                       //a.BranchId == obj.BranchId
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                       && a.CompanyId == obj.CompanyId && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                       select new ClsSalesDetailsVm
                       {
                           QuantitySold = b.QuantitySold,
                           CategoryId = d.CategoryId,
                           SubCategoryId = d.SubCategoryId,
                           SubSubCategoryId = d.SubSubCategoryId,
                           BranchId = d.BrandId,
                           CustomerGroupId = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.UserGroupId).FirstOrDefault(),
                           ItemName = d.ItemName,
                           SKU = c.SKU == null ? d.SkuCode : c.SKU,
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           Name = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.Name).FirstOrDefault(),
                           MobileNo = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.MobileNo).FirstOrDefault(),
                           EmailId = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.EmailId).FirstOrDefault(),
                           InvoiceNo = a.InvoiceNo,
                           SalesDate = a.SalesDate,
                           Quantity = b.Quantity,
                           FreeQuantity = b.FreeQuantity,
                           UnitCost = b.UnitCost,
                           Discount = b.Discount,
                           PriceExcTax = b.PriceExcTax,
                           PriceIncTax = b.PriceIncTax,
                           AmountIncTax = b.AmountIncTax,
                           CustomerId = a.CustomerId,
                           ItemId = d.ItemId,
                           TaxPercent = oConnectionContext.DbClsTax.Where(x => x.TaxId == b.TaxId).Select(x => x.TaxPercent).FirstOrDefault(),
                           Tax = oConnectionContext.DbClsTax.Where(x => x.TaxId == b.TaxId).Select(x => x.Tax).FirstOrDefault(),
                           TaxAmount = b.TaxAmount,
                           SalesId = a.SalesId
                       }).ToList();
            }
            else
            {
                det = (from b in oConnectionContext.DbClsSalesDetails
                       join a in oConnectionContext.DbClsSales
                       on b.SalesId equals a.SalesId
                       join c in oConnectionContext.DbClsItemDetails
                       on b.ItemDetailsId equals c.ItemDetailsId
                       join d in oConnectionContext.DbClsItem
                       on c.ItemId equals d.ItemId
                       where b.IsDeleted == false && a.IsDeleted == false && a.IsCancelled == false && a.IsCancelled == false && a.Status != "Draft" &&
                       a.BranchId == obj.BranchId
                       && a.CompanyId == obj.CompanyId && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
                       select new ClsSalesDetailsVm
                       {
                           QuantitySold = b.QuantitySold,
                           CategoryId = d.CategoryId,
                           SubCategoryId = d.SubCategoryId,
                           SubSubCategoryId = d.SubSubCategoryId,
                           BranchId = d.BrandId,
                           CustomerGroupId = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.UserGroupId).FirstOrDefault(),
                           ItemName = d.ItemName,
                           SKU = c.SKU == null ? d.SkuCode : c.SKU,
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                           Name = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.Name).FirstOrDefault(),
                           MobileNo = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.MobileNo).FirstOrDefault(),
                           EmailId = oConnectionContext.DbClsUser.Where(x => x.UserId == a.CustomerId).Select(x => x.EmailId).FirstOrDefault(),
                           InvoiceNo = a.InvoiceNo,
                           SalesDate = a.SalesDate,
                           Quantity = b.Quantity,
                           FreeQuantity = b.FreeQuantity,
                           UnitCost = b.UnitCost,
                           Discount = b.Discount,
                           PriceExcTax = b.PriceExcTax,
                           PriceIncTax = b.PriceIncTax,
                           AmountIncTax = b.AmountIncTax,
                           CustomerId = a.CustomerId,
                           ItemId = d.ItemId,
                           TaxPercent = oConnectionContext.DbClsTax.Where(x => x.TaxId == b.TaxId).Select(x => x.TaxPercent).FirstOrDefault(),
                           Tax = oConnectionContext.DbClsTax.Where(x => x.TaxId == b.TaxId).Select(x => x.Tax).FirstOrDefault(),
                           TaxAmount = b.TaxAmount,
                           SalesId = obj.SalesId
                       }).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).Select(a => a).ToList();
            }

            if (obj.CustomerGroupId != 0)
            {
                det = det.Where(a => a.CustomerGroupId == obj.CustomerGroupId).Select(a => a).ToList();
            }

            if (obj.CategoryId != 0)
            {
                det = det.Where(a => a.CategoryId == obj.CategoryId).Select(a => a).ToList();
            }

            if (obj.SubCategoryId != 0)
            {
                det = det.Where(a => a.SubCategoryId == obj.SubCategoryId).Select(a => a).ToList();
            }

            if (obj.SubSubCategoryId != 0)
            {
                det = det.Where(a => a.SubSubCategoryId == obj.SubSubCategoryId).Select(a => a).ToList();
            }

            if (obj.BrandId != 0)
            {
                det = det.Where(a => a.BrandId == obj.BrandId).Select(a => a).ToList();
            }

            if (obj.BrandId != 0)
            {
                det = det.Where(a => a.BrandId == obj.BrandId).Select(a => a).ToList();
            }

            if (obj.ItemDetailsId != 0)
            {
                det = det.Where(a => a.ItemDetailsId == obj.ItemDetailsId).Select(a => a).ToList();
            }

            List<ClsSalesDetailsVm> _det1 = new List<ClsSalesDetailsVm>();
            List<ClsSalesDetailsVm> _det2 = new List<ClsSalesDetailsVm>();

            _det1 = det.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            foreach (var item in _det1)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
                }).FirstOrDefault();

                decimal TotalCurrentStock = item.QuantitySold;

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

                _det2.Add(new ClsSalesDetailsVm
                {
                    //QuantitySold = TotalCurrentStock,
                    CategoryId = item.CategoryId,
                    SubCategoryId = item.SubCategoryId,
                    SubSubCategoryId = item.SubSubCategoryId,
                    BranchId = item.BrandId,
                    CustomerGroupId = item.CustomerGroupId,
                    ItemName = item.ItemName,
                    SKU = item.SKU,
                    VariationName = item.VariationName,
                    Name = item.Name,
                    MobileNo = item.MobileNo,
                    EmailId = item.EmailId,
                    InvoiceNo = item.InvoiceNo,
                    SalesDate = item.SalesDate,
                    Quantity = TotalCurrentStock,
                    FreeQuantity = item.FreeQuantity,
                    UnitCost = item.UnitCost,
                    Discount = item.Discount,
                    PriceExcTax = item.PriceExcTax,
                    PriceIncTax = item.PriceIncTax,
                    AmountIncTax = item.AmountIncTax,
                    CustomerId = item.CustomerId,
                    ItemId = item.ItemId,
                    TaxPercent = item.TaxPercent,
                    Tax = item.Tax,
                    TaxAmount = item.TaxAmount,
                    UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit,
                    SalesId = item.SalesId
                });
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesDetails = _det2,
                    TotalCount = det.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesByItemReport(ClsSalesDetailsVm obj)
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

            List<ClsPurchaseSales> det = new List<ClsPurchaseSales>();

            if (obj.BranchId == 0)
            {
                det = (from bb in oConnectionContext.DbClsItemDetails
                       join cc in oConnectionContext.DbClsItem on bb.ItemId equals cc.ItemId
                       where cc.CompanyId == obj.CompanyId && bb.IsDeleted == false && cc.IsDeleted == false
                       select new ClsPurchaseSales
                       {
                           ItemId = bb.ItemId,
                           ItemDetailId = bb.ItemDetailsId,
                           Name = cc.ItemName,
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(ccc => ccc.VariationDetailsId == bb.VariationDetailsId).Select(ccc => ccc.VariationDetails).FirstOrDefault(),
                           SalesDetails = (from b in oConnectionContext.DbClsSales
                                           join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                           //join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                           where c.ItemDetailsId == bb.ItemDetailsId && b.Status != "Draft"
                                           && b.IsDeleted == false && b.IsCancelled == false
                                           //&& d.IsDeleted == false && d.IsCancelled == false 
                                           && c.IsDeleted == false
                                           && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                             DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                           select new ClsSalesDetailsVm
                                           {
                                               SKU = cc.ProductType.ToLower() == "single" ? cc.SkuCode : bb.SKU,
                                               BranchId = b.BranchId,
                                               ItemId = c.ItemId,
                                               CustomerId = b.CustomerId,
                                               SubCategoryId = cc.SubCategoryId,
                                               SubSubCategoryId = cc.SubSubCategoryId,
                                               BrandId = cc.BrandId,
                                               AmountIncTax = c.AmountIncTax,
                                               Quantity = c.QuantitySold
                                           }).ToList(),
                           SalesReturnDetails = (from b in oConnectionContext.DbClsSalesReturn
                                                 join c in oConnectionContext.DbClsSalesReturnDetails on b.SalesReturnId equals c.SalesReturnId
                                                 join d in oConnectionContext.DbClsSales on b.SalesId equals d.SalesId
                                                 //join e in oConnectionContext.DbClsItem on c.ItemId equals e.ItemId
                                                 where c.ItemDetailsId == bb.ItemDetailsId && b.Status != "Draft"
                                           && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false
                                           //&& e.IsDeleted == false && e.IsCancelled == false
                                           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                             DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                 select new ClsSalesReturnDetailsVm
                                                 {
                                                     SKU = cc.ProductType.ToLower() == "single" ? cc.SkuCode : bb.SKU,
                                                     BranchId = d.BranchId,
                                                     ItemId = c.ItemId,
                                                     CustomerId = d.CustomerId,
                                                     SubCategoryId = cc.SubCategoryId,
                                                     SubSubCategoryId = cc.SubSubCategoryId,
                                                     BrandId = cc.BrandId,
                                                     AmountIncTax = c.AmountIncTax,
                                                     Quantity = c.QuantityReturned
                                                 }).ToList(),
                       }).ToList();
            }
            else
            {
                det = (from aa in oConnectionContext.DbClsItemBranchMap
                       join bb in oConnectionContext.DbClsItemDetails
                       on aa.ItemDetailsId equals bb.ItemDetailsId
                       join cc in oConnectionContext.DbClsItem on bb.ItemId equals cc.ItemId
                       where aa.CompanyId == obj.CompanyId &&
                       aa.BranchId == obj.BranchId
                       && aa.IsDeleted == false && bb.IsDeleted == false && cc.IsDeleted == false
                       select new ClsPurchaseSales
                       {
                           BranchId = aa.BranchId,
                           ItemId = aa.ItemId,
                           ItemDetailId = bb.ItemDetailsId,
                           Name = cc.ItemName,
                           VariationName = oConnectionContext.DbClsVariationDetails.Where(ccc => ccc.VariationDetailsId == bb.VariationDetailsId).Select(ccc => ccc.VariationDetails).FirstOrDefault(),
                           SalesDetails = (from b in oConnectionContext.DbClsSales
                                           join c in oConnectionContext.DbClsSalesDetails on b.SalesId equals c.SalesId
                                           //join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                           where c.ItemDetailsId == aa.ItemDetailsId && b.Status != "Draft"
                                           && b.IsDeleted == false && b.IsCancelled == false
                                           //&& d.IsDeleted == false && d.IsCancelled == false 
                                           && c.IsDeleted == false
                                           && b.BranchId == obj.BranchId
                                           && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                             DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                                           select new ClsSalesDetailsVm
                                           {
                                               SKU = cc.ProductType.ToLower() == "single" ? cc.SkuCode : bb.SKU,
                                               BranchId = b.BranchId,
                                               ItemId = c.ItemId,
                                               CustomerId = b.CustomerId,
                                               SubCategoryId = cc.SubCategoryId,
                                               SubSubCategoryId = cc.SubSubCategoryId,
                                               BrandId = cc.BrandId,
                                               AmountIncTax = c.AmountIncTax,
                                               Quantity = c.QuantitySold
                                           }).ToList(),
                           SalesReturnDetails = (from b in oConnectionContext.DbClsSalesReturn
                                                 join c in oConnectionContext.DbClsSalesReturnDetails on b.SalesReturnId equals c.SalesReturnId
                                                 join d in oConnectionContext.DbClsSales on b.SalesId equals d.SalesId
                                                 //join e in oConnectionContext.DbClsItem on c.ItemId equals e.ItemId
                                                 where c.ItemDetailsId == aa.ItemDetailsId && b.Status != "Draft"
                                           && b.IsDeleted == false && b.IsCancelled == false && d.IsDeleted == false && d.IsCancelled == false && c.IsDeleted == false
                                            //&& e.IsDeleted == false && e.IsCancelled == false
                                            && d.BranchId == obj.BranchId
                                           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                             DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                 select new ClsSalesReturnDetailsVm
                                                 {
                                                     SKU = cc.ProductType.ToLower() == "single" ? cc.SkuCode : bb.SKU,
                                                     BranchId = d.BranchId,
                                                     ItemId = c.ItemId,
                                                     CustomerId = d.CustomerId,
                                                     SubCategoryId = cc.SubCategoryId,
                                                     SubSubCategoryId = cc.SubSubCategoryId,
                                                     BrandId = cc.BrandId,
                                                     AmountIncTax = c.AmountIncTax,
                                                     Quantity = c.QuantityReturned
                                                 }).ToList(),
                       }).ToList();
            }

            if (obj.SKU != null && obj.SKU != "")
            {
                det = det.Select(a => new ClsPurchaseSales
                {
                    ItemId = a.ItemId,
                    ItemDetailId = a.ItemDetailId,
                    Name = a.Name,
                    VariationName = a.VariationName,
                    SalesDetails = a.SalesDetails.Where(b => b.SKU.ToLower() == obj.SKU.ToLower()).Select(b => new ClsSalesDetailsVm
                    {
                        ItemId = b.ItemId,
                        CustomerId = b.CustomerId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity
                    }).ToList(),
                    SalesReturnDetails = a.SalesReturnDetails.Where(b => b.SKU.ToLower() == obj.SKU.ToLower()).Select(b => new ClsSalesReturnDetailsVm
                    {
                        ItemId = b.ItemId,
                        CustomerId = b.CustomerId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity
                    }).ToList(),
                }).ToList();
            }

            if (obj.ItemId != 0)
            {
                det = det.Select(a => new ClsPurchaseSales
                {
                    ItemId = a.ItemId,
                    ItemDetailId = a.ItemDetailId,
                    Name = a.Name,
                    VariationName = a.VariationName,
                    SalesDetails = a.SalesDetails.Where(b => b.ItemId == obj.ItemId).Select(b => new ClsSalesDetailsVm
                    {
                        ItemId = b.ItemId,
                        CustomerId = b.CustomerId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity
                    }).ToList(),
                    SalesReturnDetails = a.SalesReturnDetails.Where(b => b.ItemId == obj.ItemId).Select(b => new ClsSalesReturnDetailsVm
                    {
                        ItemId = b.ItemId,
                        CustomerId = b.CustomerId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity
                    }).ToList(),
                }).ToList();
            }

            var SalesDetails = det.Select(a => new ClsSalesDetailsVm
            {
                ItemId = a.ItemId,
                ItemDetailsId = a.ItemDetailId,
                Name = a.Name,
                VariationName = a.VariationName,
                AmountIncTax = a.SalesDetails.Select(b => b.AmountIncTax).DefaultIfEmpty().Sum(),
                Quantity = a.SalesDetails.Select(b => b.Quantity).DefaultIfEmpty().Sum(),
                QuantityRemaining = (from b in oConnectionContext.DbClsItem
                                     join c in oConnectionContext.DbClsItemBranchMap
on b.ItemId equals c.ItemId
                                     where b.ItemId == a.ItemId
         && c.BranchId == obj.BranchId
                                     select c.Quantity).DefaultIfEmpty().Sum(),
                ReturnedAmountIncTax = a.SalesReturnDetails.Select(b => b.AmountIncTax).DefaultIfEmpty().Sum(),
                ReturnedQuantity = a.SalesReturnDetails.Select(b => b.Quantity).DefaultIfEmpty().Sum(),
            }).ToList();

            List<ClsSalesDetailsVm> _det1 = new List<ClsSalesDetailsVm>();
            List<ClsSalesDetailsVm> _det2 = new List<ClsSalesDetailsVm>();

            SalesDetails = SalesDetails.Where(a => a.AmountIncTax > 0).ToList();
            _det1 = SalesDetails.OrderByDescending(a => a.ItemId).Skip(skip).Take(obj.PageSize).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            foreach (var item in _det1)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
                }).FirstOrDefault();

                decimal TotalCurrentStock = item.Quantity;
                decimal TotalReturnedStock = item.ReturnedQuantity;
                decimal TotalQuantityRemaining = item.QuantityRemaining;

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
                        TotalReturnedStock = TotalReturnedStock / conversionRates.UToSValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.UToSValue;
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
                        TotalReturnedStock = TotalReturnedStock / conversionRates.UToSValue / conversionRates.SToTValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.UToSValue / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue;
                        TotalReturnedStock = TotalReturnedStock / conversionRates.SToTValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.SToTValue;
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
                        TotalReturnedStock = TotalReturnedStock / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalReturnedStock = TotalReturnedStock / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.TToQValue;
                        TotalReturnedStock = TotalReturnedStock / conversionRates.TToQValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 4)
                    {
                        //TotalCurrentStock= TotalCurrentStock;
                    }
                }

                _det2.Add(new ClsSalesDetailsVm
                {
                    ItemId = item.ItemId,
                    ItemDetailsId = item.ItemDetailsId,
                    Name = item.Name,
                    VariationName = item.VariationName,
                    AmountIncTax = item.AmountIncTax,
                    Quantity = TotalCurrentStock,
                    QuantityRemaining = TotalQuantityRemaining,
                    UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit,
                    ReturnedAmountIncTax = item.ReturnedAmountIncTax,
                    ReturnedQuantity = TotalReturnedStock
                });
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesDetails = _det2,
                    TotalCount = SalesDetails.Count(),
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesDetailsByItem(ClsSalesDetailsVm obj)
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

            List<ClsBankPaymentVm> BankPayments = new List<ClsBankPaymentVm>();

            if (obj.BranchId == 0)
            {
                BankPayments = (from a in oConnectionContext.DbClsSalesDetails
                                join b in oConnectionContext.DbClsSales
                                    on a.SalesId equals b.SalesId
                                //    join c in oConnectionContext.DbClsItem
                                //on a.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                && a.AmountIncTax != 0 && b.Status != "Draft"
                && a.ItemDetailsId == obj.ItemDetailsId
                                select new ClsBankPaymentVm
                                {
                                    Id = b.SalesId,
                                    AddedOn = b.SalesDate,
                                    ReferenceNo = b.InvoiceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }
            else
            {
                BankPayments = (from a in oConnectionContext.DbClsSalesDetails
                                join b in oConnectionContext.DbClsSales
                                    on a.SalesId equals b.SalesId
                                //    join c in oConnectionContext.DbClsItem
                                //on a.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                       && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                       && b.BranchId == obj.BranchId
            && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                && a.AmountIncTax != 0 && b.Status != "Draft"
                && a.ItemDetailsId == obj.ItemDetailsId
                                select new ClsBankPaymentVm
                                {
                                    Id = b.SalesId,
                                    AddedOn = b.SalesDate,
                                    ReferenceNo = b.InvoiceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = BankPayments.OrderByDescending(a => a.Id).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = BankPayments.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesReturnDetailsByItem(ClsSalesDetailsVm obj)
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

            List<ClsBankPaymentVm> BankPayments = new List<ClsBankPaymentVm>();

            if (obj.BranchId == 0)
            {
                BankPayments = (from a in oConnectionContext.DbClsSalesReturnDetails
                                join b in oConnectionContext.DbClsSalesReturn
                             on a.SalesReturnId equals b.SalesReturnId
                                join p in oConnectionContext.DbClsSales
                                on b.SalesId equals p.SalesId
                                where a.CompanyId == obj.CompanyId
                     && a.IsDeleted == false && a.IsActive == true
                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                     && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == p.BranchId)
       && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
           DbFunctions.TruncateTime(b.Date) <= obj.ToDate
           && a.AmountIncTax != 0 && a.ItemDetailsId == obj.ItemDetailsId && b.Status != "Draft"
                                select new ClsBankPaymentVm
                                {
                                    Id = b.SalesReturnId,
                                    AddedOn = b.Date,
                                    ReferenceNo = b.InvoiceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }
            else
            {
                BankPayments = (from a in oConnectionContext.DbClsSalesReturnDetails
                                join b in oConnectionContext.DbClsSalesReturn
                             on a.SalesReturnId equals b.SalesReturnId
                                join p in oConnectionContext.DbClsSales
                                on b.SalesId equals p.SalesId
                                where a.CompanyId == obj.CompanyId
                     && a.IsDeleted == false && a.IsActive == true
                     && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                     && p.IsDeleted == false && p.IsCancelled == false && p.IsActive == true
                     && p.BranchId == obj.BranchId
       && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
           DbFunctions.TruncateTime(b.Date) <= obj.ToDate
           && a.AmountIncTax != 0 && a.ItemDetailsId == obj.ItemDetailsId && b.Status != "Draft"
                                select new ClsBankPaymentVm
                                {
                                    Id = b.SalesReturnId,
                                    AddedOn = b.Date,
                                    ReferenceNo = b.InvoiceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = BankPayments.OrderByDescending(a => a.Id).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = BankPayments.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseByItemReport(ClsSalesDetailsVm obj)
        {
            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
            //{
            //    a.IsCompany,
            //    a.UserRoleId,
            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
            //   oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
            //     && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
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

            List<ClsPurchaseSales> det = (from aa in oConnectionContext.DbClsItemBranchMap
                                          join bb in oConnectionContext.DbClsItemDetails
                                          on aa.ItemDetailsId equals bb.ItemDetailsId
                                          join cc in oConnectionContext.DbClsItem on bb.ItemId equals cc.ItemId
                                          where aa.CompanyId == obj.CompanyId &&
                                          oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == aa.BranchId)
                                          && aa.IsDeleted == false && bb.IsDeleted == false && cc.IsDeleted == false
                                          select new ClsPurchaseSales
                                          {
                                              ItemId = aa.ItemId,
                                              ItemDetailId = bb.ItemDetailsId,
                                              Name = cc.ItemName,
                                              VariationName = oConnectionContext.DbClsVariationDetails.Where(ccc => ccc.VariationDetailsId == bb.VariationDetailsId).Select(ccc => ccc.VariationDetails).FirstOrDefault(),
                                              PurchaseDetails = (from b in oConnectionContext.DbClsPurchase
                                                                 join c in oConnectionContext.DbClsPurchaseDetails on b.PurchaseId equals c.PurchaseId
                                                                 //join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                 where c.ItemDetailsId == aa.ItemDetailsId && b.Status != "Draft"
                                                                 && b.IsDeleted == false && b.IsCancelled == false
                                                                 //&& d.IsDeleted == false && d.IsCancelled == false 
                                                                 && c.IsDeleted == false
                                                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                              l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                                                 && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                                                   DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                                                                 select new ClsPurchaseDetailsVm
                                                                 {
                                                                     BranchId = b.BranchId,
                                                                     ItemId = c.ItemId,
                                                                     SupplierId = b.SupplierId,
                                                                     SubCategoryId = cc.SubCategoryId,
                                                                     SubSubCategoryId = cc.SubSubCategoryId,
                                                                     BrandId = cc.BrandId,
                                                                     AmountIncTax = c.AmountIncTax,
                                                                     Quantity = c.QuantityPurchased,
                                                                     QuantityRemaining = c.QuantityRemaining
                                                                 }).ToList(),
                                              PurchaseReturnDetails = (from b in oConnectionContext.DbClsPurchaseReturn
                                                                       join c in oConnectionContext.DbClsPurchaseReturnDetails on b.PurchaseReturnId equals c.PurchaseReturnId
                                                                       //join d in oConnectionContext.DbClsItem on c.ItemId equals d.ItemId
                                                                       where c.ItemDetailsId == aa.ItemDetailsId && b.Status != "Draft"
                                                                 && b.IsDeleted == false && b.IsCancelled == false
                                                                 //&& d.IsDeleted == false && d.IsCancelled == false 
                                                                 && c.IsDeleted == false
                                                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                              l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                                                                 && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                                                   DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                                                                       select new ClsPurchaseReturnDetailsVm
                                                                       {
                                                                           BranchId = b.BranchId,
                                                                           ItemId = c.ItemId,
                                                                           SupplierId = b.SupplierId,
                                                                           SubCategoryId = cc.SubCategoryId,
                                                                           SubSubCategoryId = cc.SubSubCategoryId,
                                                                           BrandId = cc.BrandId,
                                                                           AmountIncTax = c.AmountIncTax,
                                                                           Quantity = c.QuantityReturned,
                                                                           QuantityRemaining = c.QuantityRemaining
                                                                       }).ToList(),
                                          }
                                          ).GroupBy(x => x.ItemDetailId)
                              .Select(g => g.FirstOrDefault()).ToList();


            if (obj.BranchId != 0)
            {
                det = det.Select(a => new ClsPurchaseSales
                {
                    ItemId = a.ItemId,
                    ItemDetailId = a.ItemDetailId,
                    Name = a.Name,
                    VariationName = a.VariationName,
                    PurchaseDetails = a.PurchaseDetails.Where(b => b.BranchId == obj.BranchId).Select(b => new ClsPurchaseDetailsVm
                    {
                        ItemId = b.ItemId,
                        SupplierId = b.SupplierId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity,
                        QuantityRemaining = b.QuantityRemaining
                    }).ToList(),
                    PurchaseReturnDetails = a.PurchaseReturnDetails.Where(b => b.BranchId == obj.BranchId).Select(b => new ClsPurchaseReturnDetailsVm
                    {
                        ItemId = b.ItemId,
                        SupplierId = b.SupplierId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity,
                        QuantityRemaining = b.QuantityRemaining
                    }).ToList(),
                }).ToList();
            }

            //if (obj.CustomerId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        ItemId = a.ItemId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.SupplierId == obj.SupplierId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity,
            //            QuantityRemaining = b.QuantityRemaining
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.CustomerGroupId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        ItemId = a.ItemId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.CustomerGroupId == obj.CustomerGroupId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.CategoryId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        ItemId = a.ItemId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.CategoryId == obj.CategoryId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity,
            //            QuantityRemaining = b.QuantityRemaining
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.SubCategoryId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        ItemId = a.ItemId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.SubCategoryId == obj.SubCategoryId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity,
            //            QuantityRemaining = b.QuantityRemaining
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.SubSubCategoryId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        ItemId = a.ItemId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.SubSubCategoryId == obj.SubSubCategoryId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity,
            //            QuantityRemaining = b.QuantityRemaining
            //        }).ToList(),
            //    }).ToList();
            //}

            //if (obj.BrandId != 0)
            //{
            //    det = det.Select(a => new ClsPurchaseSales
            //    {
            //        ItemId = a.ItemId,
            //        Name = a.Name,
            //        PurchaseDetails = a.PurchaseDetails.Where(b => b.BrandId == obj.BrandId).Select(b => new ClsPurchaseDetailsVm
            //        {
            //            ItemId = b.ItemId,
            //            SupplierId = b.SupplierId,
            //            SubCategoryId = b.SubCategoryId,
            //            SubSubCategoryId = b.SubSubCategoryId,
            //            BrandId = b.BrandId,
            //            AmountIncTax = b.AmountIncTax,
            //            Quantity = b.Quantity,
            //            QuantityRemaining = b.QuantityRemaining
            //        }).ToList(),
            //    }).ToList();
            //}

            if (obj.ItemId != 0)
            {
                det = det.Select(a => new ClsPurchaseSales
                {
                    ItemId = a.ItemId,
                    ItemDetailId = a.ItemDetailId,
                    Name = a.Name,
                    VariationName = a.VariationName,
                    PurchaseDetails = a.PurchaseDetails.Where(b => b.ItemId == obj.ItemId).Select(b => new ClsPurchaseDetailsVm
                    {
                        ItemId = b.ItemId,
                        SupplierId = b.SupplierId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity,
                        QuantityRemaining = b.QuantityRemaining
                    }).ToList(),
                    PurchaseReturnDetails = a.PurchaseReturnDetails.Where(b => b.ItemId == obj.ItemId).Select(b => new ClsPurchaseReturnDetailsVm
                    {
                        ItemId = b.ItemId,
                        SupplierId = b.SupplierId,
                        SubCategoryId = b.SubCategoryId,
                        SubSubCategoryId = b.SubSubCategoryId,
                        BrandId = b.BrandId,
                        AmountIncTax = b.AmountIncTax,
                        Quantity = b.Quantity,
                        QuantityRemaining = b.QuantityRemaining
                    }).ToList(),
                }).ToList();
            }

            var PurchaseDetails = det.Select(a => new ClsPurchaseDetailsVm
            {
                ItemId = a.ItemId,
                ItemDetailsId = a.ItemDetailId,
                Name = a.Name,
                VariationName = a.VariationName,
                AmountIncTax = a.PurchaseDetails.Select(b => b.AmountIncTax).DefaultIfEmpty().Sum(),
                Quantity = a.PurchaseDetails.Select(b => b.Quantity).DefaultIfEmpty().Sum(),
                QuantityRemaining = a.PurchaseDetails.Select(b => b.QuantityRemaining).DefaultIfEmpty().Sum(),
                ReturnedAmountIncTax = a.PurchaseReturnDetails.Select(b => b.AmountIncTax).DefaultIfEmpty().Sum(),
                ReturnedQuantity = a.PurchaseReturnDetails.Select(b => b.Quantity).DefaultIfEmpty().Sum(),
            }).ToList();

            List<ClsPurchaseDetailsVm> _det1 = new List<ClsPurchaseDetailsVm>();
            List<ClsPurchaseDetailsVm> _det2 = new List<ClsPurchaseDetailsVm>();

            PurchaseDetails = PurchaseDetails.Where(a => a.AmountIncTax > 0).ToList();
            _det1 = PurchaseDetails.OrderByDescending(a => a.ItemId).Skip(skip).Take(obj.PageSize).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            foreach (var item in _det1)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
                }).FirstOrDefault();

                decimal TotalCurrentStock = item.Quantity;
                decimal TotalReturnedStock = item.ReturnedQuantity;
                decimal TotalQuantityRemaining = item.QuantityRemaining;

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
                        TotalReturnedStock = TotalReturnedStock / conversionRates.UToSValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.UToSValue;
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
                        TotalReturnedStock = TotalReturnedStock / conversionRates.UToSValue / conversionRates.SToTValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.UToSValue / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue;
                        TotalReturnedStock = TotalReturnedStock / conversionRates.SToTValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.SToTValue;
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
                        TotalReturnedStock = TotalReturnedStock / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalReturnedStock = TotalReturnedStock / conversionRates.SToTValue / conversionRates.TToQValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        TotalCurrentStock = TotalCurrentStock / conversionRates.TToQValue;
                        TotalReturnedStock = TotalReturnedStock / conversionRates.TToQValue;
                        TotalQuantityRemaining = TotalQuantityRemaining / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 4)
                    {
                        //TotalCurrentStock= TotalCurrentStock;
                    }
                }

                _det2.Add(new ClsPurchaseDetailsVm
                {
                    ItemId = item.ItemId,
                    ItemDetailsId = item.ItemDetailsId,
                    Name = item.Name,
                    VariationName = item.VariationName,
                    AmountIncTax = item.AmountIncTax,
                    Quantity = TotalCurrentStock,
                    QuantityRemaining = TotalQuantityRemaining,
                    UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit,
                    ReturnedAmountIncTax = item.ReturnedAmountIncTax,
                    ReturnedQuantity = TotalReturnedStock
                });
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PurchaseDetails = _det2,
                    TotalCount = PurchaseDetails.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseDetailsByItem(ClsSalesDetailsVm obj)
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

            List<ClsBankPaymentVm> BankPayments = new List<ClsBankPaymentVm>();
            if (obj.BranchId == 0)
            {
                BankPayments = (from a in oConnectionContext.DbClsPurchaseDetails
                                join b in oConnectionContext.DbClsPurchase
                             on a.PurchaseId equals b.PurchaseId
                                //join c in oConnectionContext.DbClsItem
                                //on a.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                && a.AmountIncTax != 0 && b.Status != "Draft"
                && a.ItemDetailsId == obj.ItemDetailsId
                                select new ClsBankPaymentVm
                                {
                                    Id = b.PurchaseId,
                                    AddedOn = b.PurchaseDate,
                                    ReferenceNo = b.ReferenceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }
            else
            {
                BankPayments = (from a in oConnectionContext.DbClsPurchaseDetails
                                join b in oConnectionContext.DbClsPurchase
                             on a.PurchaseId equals b.PurchaseId
                                //join c in oConnectionContext.DbClsItem
                                //on a.ItemId equals c.ItemId
                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                 && b.BranchId == obj.BranchId
            && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                && a.AmountIncTax != 0 && b.Status != "Draft"
                && a.ItemDetailsId == obj.ItemDetailsId
                                select new ClsBankPaymentVm
                                {
                                    Id = b.PurchaseId,
                                    AddedOn = b.PurchaseDate,
                                    ReferenceNo = b.ReferenceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = BankPayments.OrderByDescending(a => a.Id).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = BankPayments.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseReturnDetailsByItem(ClsSalesDetailsVm obj)
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

            List<ClsBankPaymentVm> BankPayments = new List<ClsBankPaymentVm>();

            if (obj.BranchId == 0)
            {
                BankPayments = (from a in oConnectionContext.DbClsPurchaseReturnDetails
                                join b in oConnectionContext.DbClsPurchaseReturn
                             on a.PurchaseReturnId equals b.PurchaseReturnId
                                where a.CompanyId == obj.CompanyId
                                && a.IsDeleted == false && a.IsActive == true
                                && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                     && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                         DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                         && a.AmountIncTax != 0 && a.ItemDetailsId == obj.ItemDetailsId && b.Status != "Draft"
                                select new ClsBankPaymentVm
                                {
                                    Id = b.PurchaseReturnId,
                                    AddedOn = b.Date,
                                    ReferenceNo = b.InvoiceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }
            else
            {
                BankPayments = (from a in oConnectionContext.DbClsPurchaseReturnDetails
                                join b in oConnectionContext.DbClsPurchaseReturn
                             on a.PurchaseReturnId equals b.PurchaseReturnId
                                where a.CompanyId == obj.CompanyId
                                && a.IsDeleted == false && a.IsActive == true
                                && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                                && b.BranchId == obj.BranchId
                     && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                         DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                         && a.AmountIncTax != 0 && a.ItemDetailsId == obj.ItemDetailsId && b.Status != "Draft"
                                select new ClsBankPaymentVm
                                {
                                    Id = b.PurchaseReturnId,
                                    AddedOn = b.Date,
                                    ReferenceNo = b.InvoiceNo,
                                    TransactionAmount = a.AmountIncTax
                                }).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = BankPayments.OrderByDescending(a => a.Id).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = BankPayments.Count(),
                    //Branchs = userDetails.BranchIds,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemCountByBatch(ClsItemVm obj)
        {
            long TotalCount = oConnectionContext.DbClsItem.Where(a => a.BatchNo == obj.BatchNo).Count();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TotalCount = TotalCount
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockSummary(ClsStockDetails obj)
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

            List<ClsStockDetails> Transactions = new List<ClsStockDetails>();
            List<ClsStockDetails> OpeningBalance = new List<ClsStockDetails>();
            List<ClsStockDetails> PreviousTransactions = new List<ClsStockDetails>();
            List<ClsStockDetails> PreviousOpeningBalance = new List<ClsStockDetails>();
            List<ClsStockDetails> PreviousLedger = new List<ClsStockDetails>();
            List<ClsStockDetails> Ledger = new List<ClsStockDetails>();

            //decimal TotalOpeningBalanceDebit = 0, TotalOpeningBalanceCredit = 0;

            OpeningBalance = oCommonController.StockOpeningBalance(obj);
            Transactions = oCommonController.StockTransactions(obj);

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            obj.ToDate = obj.FromDate;
            obj.FromDate = Convert.ToDateTime("01-01-2020");

            PreviousOpeningBalance = oCommonController.StockOpeningBalance(obj);
            PreviousTransactions = oCommonController.StockTransactions(obj);
            PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();

            List<ClsStockDetails> _stock = (from a in oConnectionContext.DbClsItemDetails.ToList()
                                            join b in oConnectionContext.DbClsItem
                                            on a.ItemId equals b.ItemId
                                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
                                            && b.IsDeleted == false && b.IsActive == true && b.IsManageStock == true
                                            select new ClsStockDetails
                                            {
                                                PriceAddedFor = a.PriceAddedFor,
                                                ItemDetailsId = a.ItemDetailsId,
                                                ItemId = a.ItemId,
                                                ItemName = b.ItemName,
                                                SKU = b.ProductType == "Single" ? b.SkuCode : a.SKU,
                                                VariationName = oConnectionContext.DbClsVariationDetails.Where(cc =>
                                                cc.VariationDetailsId == a.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                                OpeningBalanceQty = PreviousLedger.Where(c => c.ItemDetailsId == a.ItemDetailsId &&
                                                c.Type == "Opening Stock").Select(c => c.Quantity).DefaultIfEmpty().Sum() - PreviousLedger.Where(c => c.ItemDetailsId == a.ItemDetailsId &&
                                                c.Type == "Opening Stock Expired").Select(c => c.Quantity).DefaultIfEmpty().Sum(),
                                                OpeningBalanceAmount = PreviousLedger.Where(c => c.ItemDetailsId == a.ItemDetailsId &&
                                                c.Type == "Opening Stock").Select(c => c.AmountIncTax).DefaultIfEmpty().Sum() - PreviousLedger.Where(c => c.ItemDetailsId == a.ItemDetailsId &&
                                                c.Type == "Opening Stock Expired").Select(c => c.AmountIncTax).DefaultIfEmpty().Sum(),
                                                QuantityIn = Transactions.Where(b => b.ItemDetailsId == a.ItemDetailsId &&
                                                 (b.Type == "Sales Return" || b.Type == "Sales Return Combo" || b.Type == "Purchase"
                                                 || b.Type == "Stock Adjustment Credit" || b.Type == "Stock Received")).Select(b => b.Credit).DefaultIfEmpty().Sum(),
                                                AmountIn = Transactions.Where(b => b.ItemDetailsId == a.ItemDetailsId &&
                                                (b.Type == "Sales Return" || b.Type == "Sales Return Combo" || b.Type == "Purchase"
                                                || b.Type == "Stock Adjustment Credit" || b.Type == "Stock Received")).Select(b => b.AmountIncTax).DefaultIfEmpty().Sum(),
                                                QuantityOut = Transactions.Where(b => b.ItemDetailsId == a.ItemDetailsId &&
                                                 (b.Type == "Sales" || b.Type == "Sales Combo" || b.Type == "Purchase Expired"
                                                 || b.Type == "Purchase Return"
                                                 || b.Type == "Stock Adjustment Debit" || b.Type == "Stock Transfer")).Select(b => b.Debit).DefaultIfEmpty().Sum(),
                                                AmountOut = Transactions.Where(b => b.ItemDetailsId == a.ItemDetailsId &&
                                                (b.Type == "Sales" || b.Type == "Sales Combo" || b.Type == "Purchase Expired"
                                                || b.Type == "Purchase Return"
                                                || b.Type == "Stock Adjustment Debit" || b.Type == "Stock Transfer")).Select(b => b.AmountIncTax).DefaultIfEmpty().Sum(),
                                            }).OrderBy(a => a.ItemDetailsId).ToList();

            if (obj.ItemDetailsId != 0)
            {
                _stock = _stock.Where(a => a.ItemDetailsId == obj.ItemDetailsId).ToList();
            }

            var stockDetails = _stock.OrderByDescending(a => a.ItemDetailsId).Skip(skip).Take(obj.PageSize).ToList();

            var items = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                a.ItemId,
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
            }).ToList();

            foreach (var item in stockDetails)
            {
                var conversionRates = items.Where(a => a.ItemId == item.ItemId).Select(a => new
                {
                    a.UToSValue,
                    a.SToTValue,
                    a.TToQValue,
                    PrimaryUnit = a.PrimaryUnit,
                    SecondaryUnit = a.SecondaryUnit,
                    TertiaryUnit = a.TertiaryUnit,
                    QuaternaryUnit = a.QuaternaryUnit,
                }).FirstOrDefault();

                if (obj.PriceAddedFor == 0) { obj.PriceAddedFor = 1; }

                if (conversionRates.UToSValue == 0 && conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    //TotalCurrentStock = TotalCurrentStock;
                }
                else if (conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        item.OpeningBalanceQty = item.OpeningBalanceQty / conversionRates.UToSValue;
                        item.QuantityIn = item.QuantityIn / conversionRates.UToSValue;
                        item.QuantityOut = item.QuantityOut / conversionRates.UToSValue;
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
                        item.OpeningBalanceQty = item.OpeningBalanceQty / conversionRates.UToSValue / conversionRates.SToTValue;
                        item.QuantityIn = item.QuantityIn / conversionRates.UToSValue / conversionRates.SToTValue;
                        item.QuantityOut = item.QuantityOut / conversionRates.UToSValue / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        item.OpeningBalanceQty = item.OpeningBalanceQty / conversionRates.SToTValue;
                        item.QuantityIn = item.QuantityIn / conversionRates.SToTValue;
                        item.QuantityOut = item.QuantityOut / conversionRates.SToTValue;
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
                        item.OpeningBalanceQty = item.OpeningBalanceQty / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        item.QuantityIn = item.QuantityIn / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        item.QuantityOut = item.QuantityOut / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        item.OpeningBalanceQty = item.OpeningBalanceQty / conversionRates.SToTValue / conversionRates.TToQValue;
                        item.QuantityIn = item.QuantityIn / conversionRates.SToTValue / conversionRates.TToQValue;
                        item.QuantityOut = item.QuantityOut / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        item.OpeningBalanceQty = item.OpeningBalanceQty / conversionRates.TToQValue;
                        item.QuantityIn = item.QuantityIn / conversionRates.TToQValue;
                        item.QuantityOut = item.QuantityOut / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 4)
                    {
                        //TotalCurrentStock= TotalCurrentStock;
                    }
                }

                item.UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit;
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    FromDate = FromDate,
                    ToDate = ToDate,
                    StockDetails = stockDetails,
                    TotalCount = _stock.Count(),
                    PageSize = obj.PageSize,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> StockLedger(ClsStockDetails obj)
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

            List<ClsStockDetails> PreviousTransactions = new List<ClsStockDetails>();
            List<ClsStockDetails> PreviousOpeningBalance = new List<ClsStockDetails>();
            List<ClsStockDetails> Transactions = new List<ClsStockDetails>();
            List<ClsStockDetails> OpeningBalance = new List<ClsStockDetails>();
            List<ClsStockDetails> PreviousLedger = new List<ClsStockDetails>();
            List<ClsStockDetails> Ledger = new List<ClsStockDetails>();

            decimal TotalPreviousOpeningBalance = 0;

            OpeningBalance = oCommonController.StockOpeningBalance(obj);
            Transactions = oCommonController.StockTransactions(obj).ToList();

            DateTime FromDate = obj.FromDate;
            DateTime ToDate = obj.ToDate;

            obj.ItemId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == obj.ItemDetailsId).Select(a => a.ItemId).FirstOrDefault();

            if (obj.ItemDetailsId != 0)
            {
                Transactions = Transactions.Where(a => a.ItemDetailsId == obj.ItemDetailsId).ToList();

                obj.ToDate = obj.FromDate;
                obj.FromDate = Convert.ToDateTime("01-01-2020");

                PreviousOpeningBalance = oCommonController.StockOpeningBalance(obj);
                PreviousTransactions = oCommonController.StockTransactions(obj);
                PreviousLedger = OpeningBalance.Concat(PreviousOpeningBalance).Concat(PreviousTransactions).ToList();
                PreviousLedger = PreviousLedger.Where(a => a.ItemDetailsId == obj.ItemDetailsId).ToList();
                TotalPreviousOpeningBalance =
                    PreviousLedger.Where(a => a.Type == "Opening Stock").Select(a => a.Quantity).DefaultIfEmpty().Sum();
                //- PreviousLedger.Where(a => a.Type == "Opening Stock Expired").Select(a => a.Quantity).DefaultIfEmpty().Sum();

                Ledger.Add(new ClsStockDetails
                {
                    DetailsDate = obj.ToDate,
                    Balance = 0,
                    Date = obj.ToDate,
                    ReferenceNo = "",
                    Type = "Opening Balance",
                    Quantity = TotalPreviousOpeningBalance,
                    Credit = TotalPreviousOpeningBalance,
                    Debit = 0,
                    PriceAddedFor = 4,
                });

                Ledger = Ledger.Concat(Transactions).ToList();
            }

            int count = Ledger.Count();
            Ledger = Ledger.OrderBy(a => a.Date).ThenBy(a => a.DetailsDate).ToList();

            var conversionRates = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => new
            {
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
                PrimaryUnit = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                PrimaryUnitAllowDecimal = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.AllowDecimal).FirstOrDefault(),
                SecondaryUnit = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                SecondaryUnitAllowDecimal = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitAllowDecimal).FirstOrDefault(),
                TertiaryUnit = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                TertiaryUnitAllowDecimal = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitAllowDecimal).FirstOrDefault(),
                QuaternaryUnit = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
                QuaternaryUnitAllowDecimal = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitAllowDecimal).FirstOrDefault(),
            }).FirstOrDefault();

            foreach (var item in Ledger)
            {
                if (obj.PriceAddedFor == 0) { obj.PriceAddedFor = 1; }

                if (conversionRates.UToSValue == 0 && conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    //TotalCurrentStock = TotalCurrentStock;
                }
                else if (conversionRates.SToTValue == 0 && conversionRates.TToQValue == 0)
                {
                    if (obj.PriceAddedFor == 1)
                    {
                        item.Credit = item.Credit / conversionRates.UToSValue;
                        item.Debit = item.Debit / conversionRates.UToSValue;
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
                        item.Credit = item.Credit / conversionRates.UToSValue / conversionRates.SToTValue;
                        item.Debit = item.Debit / conversionRates.UToSValue / conversionRates.SToTValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        item.Credit = item.Credit / conversionRates.SToTValue;
                        item.Debit = item.Debit / conversionRates.SToTValue;
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
                        item.Credit = item.Credit / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                        item.Debit = item.Debit / conversionRates.UToSValue / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 2)
                    {
                        item.Credit = item.Credit / conversionRates.SToTValue / conversionRates.TToQValue;
                        item.Debit = item.Debit / conversionRates.SToTValue / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 3)
                    {
                        item.Credit = item.Credit / conversionRates.TToQValue;
                        item.Debit = item.Debit / conversionRates.TToQValue;
                    }
                    else if (obj.PriceAddedFor == 4)
                    {
                        //TotalCurrentStock= TotalCurrentStock;
                    }
                }
            }

            decimal Balance = 0;

            foreach (var item in Ledger)
            {
                Balance = Balance + (item.Credit - item.Debit);
                item.Balance = Balance;
            }

            if (obj.ItemDetailsId == 0)
            {
                var Stock = new
                {
                    StockDetails = Ledger
                };

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Stock = Stock,
                        FromDate = FromDate,
                        ToDate = ToDate,
                        BankPayments = Ledger.Skip(skip).Take(obj.PageSize).ToList(),
                        TotalCount = count,
                        PageSize = obj.PageSize
                    }
                };
            }
            else
            {
                var Stock = new
                {
                    ItemName = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                    VariationName = (from b in oConnectionContext.DbClsItemDetails where b.ItemDetailsId == obj.ItemDetailsId select oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == b.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault()).FirstOrDefault(),
                    SKU = (from a in oConnectionContext.DbClsItem
                           join b in oConnectionContext.DbClsItemDetails
    on a.ItemId equals b.ItemId
                           where a.ItemId == obj.ItemId && b.ItemDetailsId == obj.ItemDetailsId
                           select a.ProductType.ToLower() == "variable" ? b.SKU : a.SkuCode).FirstOrDefault(),
                    UnitName = obj.PriceAddedFor == 1 ? conversionRates.PrimaryUnit : obj.PriceAddedFor == 2 ? conversionRates.SecondaryUnit : obj.PriceAddedFor == 3 ? conversionRates.TertiaryUnit : conversionRates.QuaternaryUnit,
                    StockDetails = Ledger.OrderByDescending(a => a.Date).ThenByDescending(a => a.DetailsDate).Skip(skip).Take(obj.PageSize).ToList()
                };

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Stock = Stock,
                        FromDate = FromDate,
                        ToDate = ToDate,
                        BankPayments = Ledger.ToList(),
                        TotalCount = count,
                        PageSize = obj.PageSize
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ItemMultipleUnits(ClsItemVm obj)
        {
            if (obj.ItemDetailsId != 0)
            {
                obj.ItemId = oConnectionContext.DbClsItemDetails.Where(a => a.ItemDetailsId == obj.ItemDetailsId).Select(a => a.ItemId).FirstOrDefault();
            }
            var det = oConnectionContext.DbClsItem.Where(a => a.ItemId == obj.ItemId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                UnitId = a.UnitId,
                UnitName = oConnectionContext.DbClsUnit.Where(b => b.UnitId == a.UnitId).Select(b => b.UnitName).FirstOrDefault(),
                a.SecondaryUnitId,
                SecondaryUnitName = oConnectionContext.DbClsSecondaryUnit.Where(b => b.SecondaryUnitId == a.SecondaryUnitId).Select(b => b.SecondaryUnitName).FirstOrDefault(),
                a.TertiaryUnitId,
                TertiaryUnitName = oConnectionContext.DbClsTertiaryUnit.Where(b => b.TertiaryUnitId == a.TertiaryUnitId).Select(b => b.TertiaryUnitName).FirstOrDefault(),
                a.QuaternaryUnitId,
                QuaternaryUnitName = oConnectionContext.DbClsQuaternaryUnit.Where(b => b.QuaternaryUnitId == a.QuaternaryUnitId).Select(b => b.QuaternaryUnitName).FirstOrDefault(),
                a.UToSValue,
                a.SToTValue,
                a.TToQValue,
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Item = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }
        
        public async Task<IHttpActionResult> PublicItems(ClsItemBranchMapVm obj)
        {
            if (obj == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Invalid request"
                };

                return await Task.FromResult(Ok(data));
            }

            var pageIndex = obj.PageIndex > 0 ? obj.PageIndex : 1;
            var pageSize = obj.PageSize > 0 ? obj.PageSize : 0;

            var stockNotManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                   join a in oConnectionContext.DbClsItemDetails on c.ItemDetailsId equals a.ItemDetailsId
                                   join b in oConnectionContext.DbClsItem on c.ItemId equals b.ItemId
                                   where b.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive &&
                                         a.IsActive && !a.IsDeleted &&
                                         b.IsActive && !b.IsDeleted && !b.IsManageStock &&
                                         b.ProductType.ToLower() != "variable"
                                   select new ClsItemDetailsVm
                                   {
                                       ItemName = b.ItemName,
                                       Description =b.Description,
                                       SKU = b.SkuCode,
                                       Quantity = c.Quantity,
                                       ProductImage = b.ProductImage ?? "/Content/assets/img/item.jpg",
                                       ItemId = b.ItemId,
                                       ItemDetailsId = a.ItemDetailsId,
                                       VariationDetailsId = a.VariationDetailsId,
                                       VariationName = "",
                                       BrandId = b.BrandId,
                                       CategoryId = b.CategoryId,
                                       SubCategoryId = b.SubCategoryId,
                                       SubSubCategoryId = b.SubSubCategoryId,
                                       SalesExcTax= a.SalesExcTax,
                                       IsManageStock= b.IsManageStock,
                                       DefaultMrp=a.DefaultMrp,
                                       DiscountPercent = (a.DefaultMrp > 0 && a.DefaultMrp - a.SalesIncTax > 0) ? Math.Round(((a.DefaultMrp - a.SalesIncTax) / a.DefaultMrp) * 100, 2) : 0,
                                       TaxPreferenceId= b.TaxPreferenceId,
                                       TaxId=b.TaxId,
                                       InterStateTaxId= b.InterStateTaxId,
                                       TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                       InterStateTaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                       TaxType = b.TaxType
                                   }).ToList();

            var variableStockNotManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                           join a in oConnectionContext.DbClsItemDetails on c.ItemDetailsId equals a.ItemDetailsId
                                           join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                           where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive &&
                                                 a.IsActive && !a.IsDeleted &&
                                                 b.ProductType.ToLower() == "variable" && b.IsActive && !b.IsDeleted && !b.IsManageStock
                                           select new ClsItemDetailsVm
                                           {
                                               ItemName = b.ItemName,
                                               Description = b.Description,
                                               SKU = a.SKU,
                                               Quantity = c.Quantity,
                                               ProductImage = a.ProductImage ?? b.ProductImage ?? "/Content/assets/img/item.jpg",
                                               ItemId = b.ItemId,
                                               ItemDetailsId = a.ItemDetailsId,
                                               VariationDetailsId = a.VariationDetailsId,
                                               VariationName = oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId)
                                                   .Select(c => c.VariationDetails).FirstOrDefault(),
                                               BrandId = b.BrandId,
                                               CategoryId = b.CategoryId,
                                               SubCategoryId = b.SubCategoryId,
                                               SubSubCategoryId = b.SubSubCategoryId,
                                               SalesExcTax = a.SalesExcTax,
                                               IsManageStock =b.IsManageStock,
                                               DefaultMrp=a.DefaultMrp,
                                               DiscountPercent = (a.DefaultMrp > 0 && a.DefaultMrp - a.SalesIncTax > 0) ? Math.Round(((a.DefaultMrp - a.SalesIncTax) / a.DefaultMrp) * 100, 2) : 0,
                                               TaxPreferenceId=b.TaxPreferenceId,
                                               TaxId=b.TaxId,
                                               InterStateTaxId=b.InterStateTaxId,
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               InterStateTaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               TaxType = b.TaxType
                                           }).ToList();

            var stockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                join a in oConnectionContext.DbClsItemDetails on c.ItemDetailsId equals a.ItemDetailsId
                                join b in oConnectionContext.DbClsItem on c.ItemId equals b.ItemId
                                where b.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive &&
                                      a.IsActive && !a.IsDeleted &&
                                      b.IsActive && !b.IsDeleted && b.IsManageStock &&
                                      b.ProductType.ToLower() != "variable"
                                select new ClsItemDetailsVm
                                {
                                    ItemName = b.ItemName,
                                    Description =b.Description,
                                    SKU = b.SkuCode,
                                    Quantity = c.Quantity,
                                    ProductImage = b.ProductImage ?? "/Content/assets/img/item.jpg",
                                    ItemId = b.ItemId,
                                    ItemDetailsId = a.ItemDetailsId,
                                    VariationDetailsId = a.VariationDetailsId,
                                    VariationName = "",
                                    BrandId = b.BrandId,
                                    CategoryId = b.CategoryId,
                                    SubCategoryId = b.SubCategoryId,
                                    SubSubCategoryId = b.SubSubCategoryId,
                                    SalesExcTax = a.SalesExcTax,
                                    IsManageStock =b.IsManageStock,
                                    DefaultMrp=a.DefaultMrp,
                                    DiscountPercent = (a.DefaultMrp > 0 && a.DefaultMrp - a.SalesIncTax > 0) ? Math.Round(((a.DefaultMrp - a.SalesIncTax) / a.DefaultMrp) * 100, 2) : 0,
                                    TaxPreferenceId=b.TaxPreferenceId,
                                    TaxId =b.TaxId,
                                    InterStateTaxId =b.InterStateTaxId,
                                    TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                    InterStateTaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                    TaxType = b.TaxType
                                }).ToList();

            var variableStockManaged = (from c in oConnectionContext.DbClsItemBranchMap
                                        join a in oConnectionContext.DbClsItemDetails on c.ItemDetailsId equals a.ItemDetailsId
                                        join b in oConnectionContext.DbClsItem on a.ItemId equals b.ItemId
                                        where a.CompanyId == obj.CompanyId && c.BranchId == obj.BranchId && c.IsActive &&
                                              a.IsActive && !a.IsDeleted &&
                                              b.ProductType.ToLower() == "variable" && b.IsActive && !b.IsDeleted && b.IsManageStock
                                        select new ClsItemDetailsVm
                                        {
                                            ItemName = b.ItemName,
                                            Description =b.Description,
                                            SKU = a.SKU,
                                            Quantity = c.Quantity,
                                            ProductImage = a.ProductImage ?? b.ProductImage ?? "/Content/assets/img/item.jpg",
                                            ItemId = b.ItemId,
                                            ItemDetailsId = a.ItemDetailsId,
                                            VariationDetailsId = a.VariationDetailsId,
                                            VariationName = oConnectionContext.DbClsVariationDetails.Where(c => c.VariationDetailsId == a.VariationDetailsId)
                                                .Select(c => c.VariationDetails).FirstOrDefault(),
                                            BrandId = b.BrandId,
                                            CategoryId = b.CategoryId,
                                            SubCategoryId = b.SubCategoryId,
                                            SubSubCategoryId = b.SubSubCategoryId,
                                            SalesExcTax =a.SalesExcTax,
                                            IsManageStock =b.IsManageStock,
                                            DefaultMrp=a.DefaultMrp,
                                            DiscountPercent = (a.DefaultMrp > 0 && a.DefaultMrp - a.SalesIncTax > 0) ? Math.Round(((a.DefaultMrp - a.SalesIncTax) / a.DefaultMrp) * 100, 2) : 0,
                                            TaxPreferenceId=b.TaxPreferenceId,
                                            TaxId =b.TaxId,
                                            InterStateTaxId =b.InterStateTaxId,
                                            TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                            InterStateTaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.InterStateTaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                            TaxType = b.TaxType
                                        }).ToList();

            var itemDetails = stockNotManaged
                .Union(variableStockNotManaged)
                .Union(stockManaged)
                .Union(variableStockManaged)
                .Distinct()
                .ToList();

            var BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.StateId).FirstOrDefault();
            if(obj.PlaceOfSupplyId == 0)
            {
                obj.PlaceOfSupplyId = BranchStateId;
            }

            foreach (var item in itemDetails)
            {
                string ItemTaxPreference = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId
                && a.TaxId == item.TaxPreferenceId).Select(a => a.Tax).FirstOrDefault();

                if (ItemTaxPreference == "Taxable")
                {
                    decimal TaxPercent = 0;
                    long TaxId = 0;
                    if (BranchStateId == obj.PlaceOfSupplyId)
                    {
                        TaxPercent = item.TaxPercent;
                        TaxId = item.TaxId;
                    }
                    else
                    {
                        TaxPercent = item.InterStateTaxPercent;
                        TaxId = item.InterStateTaxId;
                    }

                    string TaxType = item.TaxType;
                    if (TaxType == "Exclusive")
                    {
                        item.SalesIncTax = ((TaxPercent / 100) * item.SalesExcTax) + item.SalesExcTax;
                    }
                    else
                    {
                        item.SalesIncTax = item.SalesExcTax;

                        item.SalesExcTax = (item.SalesIncTax) / (1 + (TaxPercent) / 100);
                    }

                    item.TaxPercent = TaxPercent;
                }
                else
                {
                    item.SalesIncTax = item.SalesExcTax;
                }
            }

            decimal minPrice = itemDetails.OrderBy(a => a.SalesIncTax).Select(a=>a.SalesIncTax).FirstOrDefault() -1;
            decimal maxPrice = itemDetails.OrderByDescending(a => a.SalesIncTax).Select(a => a.SalesIncTax).FirstOrDefault() + 1;

            var requestedBrandIds = (obj.BrandIds ?? new List<long>()).Where(id => id > 0).Distinct().ToList();
            if (requestedBrandIds.Any())
            {
                var brandSet = new HashSet<long>(requestedBrandIds);
                itemDetails = itemDetails
                    .Where(a => brandSet.Contains(Convert.ToInt64(a.BrandId)))
                    .ToList();
            }
            else if (obj.BrandId != 0)
            {
                itemDetails = itemDetails.Where(a => Convert.ToInt64(a.BrandId) == obj.BrandId).ToList();
            }

            if (obj.CategoryId != 0)
            {
                itemDetails = itemDetails.Where(a => Convert.ToInt64(a.CategoryId) == obj.CategoryId).ToList();
            }

            if (obj.SubCategoryId != 0)
            {
                itemDetails = itemDetails.Where(a => Convert.ToInt64(a.SubCategoryId) == obj.SubCategoryId).ToList();
            }

            if (obj.SubSubCategoryId != 0)
            {
                itemDetails = itemDetails.Where(a => Convert.ToInt64(a.SubSubCategoryId) == obj.SubSubCategoryId).ToList();
            }

            if (!string.IsNullOrWhiteSpace(obj.Search))
            {
                var searchTerm = obj.Search.Trim();
                itemDetails = itemDetails
                    .Where(a =>
                    {
                        var name = Convert.ToString(a.ItemName) ?? string.Empty;
                        var sku = Convert.ToString(a.SKU) ?? string.Empty;
                        var description = Convert.ToString(a.Description) ?? string.Empty;
                        return (!string.IsNullOrEmpty(name) && name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                               (!string.IsNullOrEmpty(sku) && sku.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                               (!string.IsNullOrEmpty(description) && description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
                    })
                    .ToList();
            }

            if (!obj.IncludeOutOfStock)
            {
                itemDetails = itemDetails
                    .Where(a => Convert.ToDecimal(a.Quantity) > 0)
                    .ToList();
            }

            if (obj.MinPrice.HasValue)
            {
                itemDetails = itemDetails.Where(a => a.SalesIncTax >= obj.MinPrice.Value).ToList();
            }

            if (obj.MaxPrice.HasValue)
            {
                itemDetails = itemDetails.Where(a => a.SalesIncTax <= obj.MaxPrice.Value).ToList();
            }

            switch (obj.SortOrder)
            {
                case 2: // Price low to high
                    itemDetails = itemDetails
                        .OrderBy(a => a.SalesIncTax)
                        .ThenBy(a => Convert.ToString(a.ItemName))
                        .ToList();
                    break;
                case 3: // Price high to low
                    itemDetails = itemDetails
                        .OrderByDescending(a => a.SalesIncTax)
                        .ThenBy(a => Convert.ToString(a.ItemName))
                        .ToList();
                    break;
                case 4: // Newest first - approximate by ItemDetailsId
                    itemDetails = itemDetails
                        .OrderByDescending(a => Convert.ToInt64(a.ItemDetailsId))
                        .ThenBy(a => Convert.ToString(a.ItemName))
                        .ToList();
                    break;
                default: // Alphabetical
                    itemDetails = itemDetails
                        .OrderBy(a => Convert.ToString(a.ItemName))
                        .ThenBy(a => Convert.ToString(a.SKU))
                        .ToList();
                    break;
            }

            // Group variable products by ItemId
            var variableItemIds = oConnectionContext.DbClsItem
                .Where(i => i.ProductType.ToLower() == "variable" && i.IsActive && !i.IsDeleted && i.CompanyId == obj.CompanyId)
                .Select(i => i.ItemId)
                .ToList();

            var variableProducts = itemDetails
                .Where(a => variableItemIds.Contains(a.ItemId))
                .GroupBy(a => a.ItemId)
                .ToList();

            var singleProducts = itemDetails
                .Where(a => !variableItemIds.Contains(a.ItemId))
                .ToList();

            // Create grouped variable products with variants - use ClsItemDetailsVm for response
            var groupedVariableProducts = new List<ClsItemDetailsVm>();
            foreach (var group in variableProducts)
            {
                var variants = group.ToList();
                if (variants.Count == 0) continue;

                var firstVariant = variants.First();

                // Create variants list as ClsItemDetailsVm objects
                var variantsList = variants.Select(v => new ClsItemDetailsVm
                {
                    ItemDetailsId = v.ItemDetailsId,
                    VariationName = v.VariationName ?? "",
                    ProductImage = v.ProductImage ?? (firstVariant.ProductImage ?? "/Content/assets/img/item.jpg"),
                    SalesIncTax = v.SalesIncTax,
                    SalesExcTax = v.SalesExcTax,
                    DefaultMrp = v.DefaultMrp,
                    Quantity = v.Quantity,
                    SKU = v.SKU ?? "",
                    DiscountPercent = (v.DefaultMrp > 0 && v.DefaultMrp - v.SalesIncTax > 0) 
                        ? Math.Round(((v.DefaultMrp - v.SalesIncTax) / v.DefaultMrp) * 100, 2) 
                        : 0,
                    ItemId = v.ItemId,
                    ItemName = v.ItemName,
                    Description = v.Description,
                    BrandId = v.BrandId,
                    CategoryId = v.CategoryId,
                    SubCategoryId = v.SubCategoryId,
                    SubSubCategoryId = v.SubSubCategoryId,
                    TaxPreferenceId = v.TaxPreferenceId,
                    TaxId = v.TaxId,
                    InterStateTaxId = v.InterStateTaxId,
                    TaxPercent = v.TaxPercent,
                    InterStateTaxPercent = v.InterStateTaxPercent,
                    TaxType = v.TaxType,
                    IsManageStock = v.IsManageStock,
                    ProductType = "Variable"
                }).ToList();

                // Create grouped product as ClsItemDetailsVm with variants
                var groupedProduct = new ClsItemDetailsVm
                {
                    ItemId = firstVariant.ItemId,
                    ItemDetailsId = firstVariant.ItemDetailsId,
                    ItemName = firstVariant.ItemName,
                    Description = firstVariant.Description,
                    SKU = firstVariant.SKU,
                    ProductImage = firstVariant.ProductImage ?? "/Content/assets/img/item.jpg",
                    SalesIncTax = variants.Min(v => v.SalesIncTax),
                    SalesExcTax = variants.Min(v => v.SalesExcTax),
                    DefaultMrp = variants.Max(v => v.DefaultMrp),
                    Quantity = variants.Sum(v => v.Quantity),
                    VariationName = "",
                    ProductType = "Variable",
                    BrandId = firstVariant.BrandId,
                    CategoryId = firstVariant.CategoryId,
                    SubCategoryId = firstVariant.SubCategoryId,
                    SubSubCategoryId = firstVariant.SubSubCategoryId,
                    TaxPreferenceId = firstVariant.TaxPreferenceId,
                    TaxId = firstVariant.TaxId,
                    InterStateTaxId = firstVariant.InterStateTaxId,
                    TaxPercent = firstVariant.TaxPercent,
                    InterStateTaxPercent = firstVariant.InterStateTaxPercent,
                    TaxType = firstVariant.TaxType,
                    IsManageStock = firstVariant.IsManageStock,
                    DiscountPercent = 0,
                    Variants = variantsList
                };

                groupedVariableProducts.Add(groupedProduct);
            }

            // Set ProductType for single products and initialize Variants
            foreach (var sp in singleProducts)
            {
                sp.ProductType = "Single";
                sp.Variants = new List<ClsItemDetailsVm>();
                
                // Populate AttributeMappings for single products
                sp.AttributeMappings = oConnectionContext.DbClsItemDetailsVariationMap
                    .Where(m => m.ItemDetailsId == sp.ItemDetailsId && m.IsDeleted == false)
                    .Select(m => new ClsItemDetailsVariationMapVm
                    {
                        ItemDetailsVariationMapId = m.ItemDetailsVariationMapId,
                        VariationId = m.VariationId,
                        VariationDetailsId = m.VariationDetailsId,
                        VariationName = oConnectionContext.DbClsVariation.Where(v => v.VariationId == m.VariationId).Select(v => v.Variation).FirstOrDefault(),
                        VariationDetailsName = oConnectionContext.DbClsVariationDetails.Where(vd => vd.VariationDetailsId == m.VariationDetailsId).Select(vd => vd.VariationDetails).FirstOrDefault()
                    }).ToList();
            }

            // Populate AttributeMappings for variable products and their variants
            foreach (var vp in groupedVariableProducts)
            {
                // Populate AttributeMappings for the main product
                vp.AttributeMappings = oConnectionContext.DbClsItemDetailsVariationMap
                    .Where(m => m.ItemDetailsId == vp.ItemDetailsId && m.IsDeleted == false)
                    .Select(m => new ClsItemDetailsVariationMapVm
                    {
                        ItemDetailsVariationMapId = m.ItemDetailsVariationMapId,
                        VariationId = m.VariationId,
                        VariationDetailsId = m.VariationDetailsId,
                        VariationName = oConnectionContext.DbClsVariation.Where(v => v.VariationId == m.VariationId).Select(v => v.Variation).FirstOrDefault(),
                        VariationDetailsName = oConnectionContext.DbClsVariationDetails.Where(vd => vd.VariationDetailsId == m.VariationDetailsId).Select(vd => vd.VariationDetails).FirstOrDefault()
                    }).ToList();
                
                // Populate AttributeMappings for each variant
                if (vp.Variants != null)
                {
                    foreach (var variant in vp.Variants)
                    {
                        variant.AttributeMappings = oConnectionContext.DbClsItemDetailsVariationMap
                            .Where(m => m.ItemDetailsId == variant.ItemDetailsId && m.IsDeleted == false)
                            .Select(m => new ClsItemDetailsVariationMapVm
                            {
                                ItemDetailsVariationMapId = m.ItemDetailsVariationMapId,
                                VariationId = m.VariationId,
                                VariationDetailsId = m.VariationDetailsId,
                                VariationName = oConnectionContext.DbClsVariation.Where(v => v.VariationId == m.VariationId).Select(v => v.Variation).FirstOrDefault(),
                                VariationDetailsName = oConnectionContext.DbClsVariationDetails.Where(vd => vd.VariationDetailsId == m.VariationDetailsId).Select(vd => vd.VariationDetails).FirstOrDefault()
                            }).ToList();
                    }
                }
            }

            // Combine single products with grouped variable products
            var allProducts = singleProducts.Union(groupedVariableProducts).ToList();

            // Re-sort after grouping
            switch (obj.SortOrder)
            {
                case 2: // Price low to high
                    allProducts = allProducts
                        .OrderBy(a => a.SalesIncTax)
                        .ThenBy(a => a.ItemName ?? "")
                        .ToList();
                    break;
                case 3: // Price high to low
                    allProducts = allProducts
                        .OrderByDescending(a => a.SalesIncTax)
                        .ThenBy(a => a.ItemName ?? "")
                        .ToList();
                    break;
                case 4: // Newest first
                    allProducts = allProducts
                        .OrderByDescending(a => a.ItemDetailsId)
                        .ThenBy(a => a.ItemName ?? "")
                        .ToList();
                    break;
                default: // Alphabetical
                    allProducts = allProducts
                        .OrderBy(a => a.ItemName ?? "")
                        .ThenBy(a => a.SKU ?? "")
                        .ToList();
                    break;
            }

            var totalCount = allProducts.Count;

            if (pageSize > 0)
            {
                var skip = (pageIndex - 1) * pageSize;
                if (skip < 0)
                {
                    skip = 0;
                }

                allProducts = allProducts
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    ItemDetails = allProducts,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    MinPrice = Convert.ToInt32(minPrice),
                    MaxPrice = Convert.ToInt32(maxPrice)
                }
            };

            return await Task.FromResult(Ok(data));
        }

        private static object GetDynamicValue(object source, string propertyName, object defaultValue = null)
        {
            if (source == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return defaultValue;
            }

            var dictionary = source as IDictionary<string, object>;
            if (dictionary != null)
            {
                object value;
                if (dictionary.TryGetValue(propertyName, out value))
                {
                    return value ?? defaultValue;
                }
                return defaultValue;
            }

            try
            {
                var type = source.GetType();
                var property = type.GetProperty(propertyName);
                if (property != null)
                {
                    var value = property.GetValue(source, null);
                    return value ?? defaultValue;
                }
            }
            catch
            {
                // Ignore reflection errors
            }

            return defaultValue;
        }

        private void NormalizeAttributeMappings(ClsItemDetailsVm item)
        {
            if (item == null || item.AttributeMappings == null)
            {
                return;
            }

            item.AttributeMappings = item.AttributeMappings
                .Where(a => a.VariationId > 0 && a.VariationDetailsId > 0)
                .ToList();

            if (item.AttributeMappings.Count == 0)
            {
                item.AttributeMappings = null;
                return;
            }

            if (item.VariationId == 0)
            {
                item.VariationId = item.AttributeMappings.First().VariationId;
            }

            if (item.VariationDetailsId == 0)
            {
                item.VariationDetailsId = item.AttributeMappings.First().VariationDetailsId;
            }

            if (string.IsNullOrWhiteSpace(item.VariationName))
            {
                item.VariationName = FormatAttributeSummary(item.AttributeMappings);
            }
        }

        private string FormatAttributeSummary(IEnumerable<ClsItemDetailsVariationMapVm> mappings)
        {
            if (mappings == null)
            {
                return string.Empty;
            }

            var summary = mappings.Select(a =>
            {
                if (!string.IsNullOrWhiteSpace(a.VariationName))
                {
                    return string.IsNullOrWhiteSpace(a.VariationDetailsName)
                        ? a.VariationName
                        : $"{a.VariationName}: {a.VariationDetailsName}";
                }

                return string.IsNullOrWhiteSpace(a.VariationDetailsName)
                    ? a.VariationDetailsId.ToString()
                    : a.VariationDetailsName;
            }).Where(a => !string.IsNullOrWhiteSpace(a));

            return string.Join(" / ", summary);
        }

        private void SaveItemDetailAttributes(List<ClsItemDetailsVariationMapVm> mappings, long itemDetailsId, long companyId, long addedBy, DateTime currentDate)
        {
            if (mappings == null || mappings.Count == 0)
            {
                return;
            }

            foreach (var map in mappings)
            {
                ClsItemDetailsVariationMap entity = new ClsItemDetailsVariationMap
                {
                    ItemDetailsId = itemDetailsId,
                    VariationId = map.VariationId,
                    VariationDetailsId = map.VariationDetailsId,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = addedBy,
                    AddedOn = currentDate,
                    CompanyId = companyId
                };
                oConnectionContext.DbClsItemDetailsVariationMap.Add(entity);
                oConnectionContext.SaveChanges();
            }
        }

        private void UpdateItemDetailAttributes(List<ClsItemDetailsVariationMapVm> mappings, long itemDetailsId, long companyId, long modifiedBy, DateTime currentDate)
        {
            // Delete existing mappings that are not in the new list
            var existingMappings = oConnectionContext.DbClsItemDetailsVariationMap
                .Where(m => m.ItemDetailsId == itemDetailsId && m.IsDeleted == false)
                .ToList();

            if (mappings == null || mappings.Count == 0)
            {
                // If no mappings provided, mark all existing as deleted
                foreach (var existing in existingMappings)
                {
                    existing.IsDeleted = true;
                    existing.ModifiedBy = modifiedBy;
                    existing.ModifiedOn = currentDate;
                    oConnectionContext.Entry(existing).Property(x => x.IsDeleted).IsModified = true;
                    oConnectionContext.Entry(existing).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(existing).Property(x => x.ModifiedOn).IsModified = true;
                }
                oConnectionContext.SaveChanges();
                return;
            }

            // Mark mappings as deleted if they're not in the new list
            var newMappingKeys = mappings
                .Where(m => m.VariationId > 0 && m.VariationDetailsId > 0)
                .Select(m => new { m.VariationId, m.VariationDetailsId })
                .ToList();

            foreach (var existing in existingMappings)
            {
                bool existsInNew = newMappingKeys.Any(n => n.VariationId == existing.VariationId && n.VariationDetailsId == existing.VariationDetailsId);
                if (!existsInNew)
                {
                    existing.IsDeleted = true;
                    existing.ModifiedBy = modifiedBy;
                    existing.ModifiedOn = currentDate;
                    oConnectionContext.Entry(existing).Property(x => x.IsDeleted).IsModified = true;
                    oConnectionContext.Entry(existing).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(existing).Property(x => x.ModifiedOn).IsModified = true;
                }
            }
            oConnectionContext.SaveChanges();

            // Add new mappings that don't exist
            foreach (var map in mappings)
            {
                if (map.VariationId <= 0 || map.VariationDetailsId <= 0)
                {
                    continue;
                }

                bool mappingExists = existingMappings.Any(e => 
                    e.VariationId == map.VariationId && 
                    e.VariationDetailsId == map.VariationDetailsId && 
                    e.IsDeleted == false);

                if (!mappingExists)
                {
                    ClsItemDetailsVariationMap entity = new ClsItemDetailsVariationMap
                    {
                        ItemDetailsId = itemDetailsId,
                        VariationId = map.VariationId,
                        VariationDetailsId = map.VariationDetailsId,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = modifiedBy,
                        AddedOn = currentDate,
                        CompanyId = companyId
                    };
                    oConnectionContext.DbClsItemDetailsVariationMap.Add(entity);
                    oConnectionContext.SaveChanges();
                }
            }
        }
    }
}
